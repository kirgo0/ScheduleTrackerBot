using DisconnectionSchedule.Events;
using DisconnectionSchedule.Models;
using DisconnectionSchedule.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DisconnectionSchedule.Services.Implementations
{
    public class DisconnectionDataParser : BackgroundService, IDisconnectionDataParser
    {
        private readonly IDataUpdateNotifier _dataUpdateNotifier;
        private readonly ILogger<DisconnectionDataParser> _logger;
        private readonly Dictionary<string, QueueData> _queues = new();
        private readonly object _lock = new();
        private Action _unsubscribe;

        public event EventHandler<QueueUpdateEventArgs> QueueUpdated;

        public DisconnectionDataParser(
            IDataUpdateNotifier dataUpdateNotifier,
            ILogger<DisconnectionDataParser> logger)
        {
            _dataUpdateNotifier = dataUpdateNotifier;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _unsubscribe = _dataUpdateNotifier.Subscribe(ProcessDataUpdate);
            return Task.CompletedTask;
        }

        private async Task ProcessDataUpdate(string data)
        {
            try
            {
                // Extract DisconSchedule.fact JSON from the string
                var factMatch = Regex.Match(data, @"DisconSchedule\.fact\s*=\s*({.*?})(?:;|$)",
                    RegexOptions.Singleline);

                if (!factMatch.Success)
                {
                    _logger.LogWarning("Could not find DisconSchedule.fact in the data");
                    return;
                }

                var jsonString = factMatch.Groups[1].Value;
                var factData = JsonSerializer.Deserialize<DisconnectionFact>(jsonString,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (factData?.Data == null)
                {
                    _logger.LogWarning("Invalid fact data structure");
                    return;
                }

                var updatedQueues = new List<(string index, QueueData data)>();

                var todayKey = factData.Today.ToString();

                if (!factData.Data.TryGetValue(todayKey, out var todayData))
                {
                    _logger.LogWarning($"No data for today ({todayKey}). Returning null.");
                    return;
                }

                factData.Data = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>
                {
                    { todayKey, todayData }
                };

                foreach (var dateEntry in factData.Data)
                {
                    foreach (var queueEntry in dateEntry.Value)
                    {
                        var queueFullName = queueEntry.Key; // e.g., "GPV1.1"
                        var queueIndex = ExtractQueueIndex(queueFullName); // e.g., "1.1"

                        var newQueueData = new QueueData
                        {
                            FullName = queueFullName,
                            Index = queueIndex,
                            HourlyStatus = new Dictionary<int, PowerStatus>(),
                            LastUpdated = DateTime.UtcNow
                        };

                        foreach (var hourEntry in queueEntry.Value)
                        {
                            if (int.TryParse(hourEntry.Key, out int hour) && hour >= 1 && hour <= 24)
                            {
                                newQueueData.HourlyStatus[hour] = ParsePowerStatus(hourEntry.Value);
                            }
                        }

                        bool isUpdated = false;
                        lock (_lock)
                        {
                            if (!_queues.ContainsKey(queueIndex) ||
                                !AreQueuesEqual(_queues[queueIndex], newQueueData))
                            {
                                _queues[queueIndex] = newQueueData;
                                isUpdated = true;
                            }
                        }

                        if (isUpdated)
                        {
                            updatedQueues.Add((queueIndex, newQueueData));
                        }
                    }
                }

                // Notify about updates
                foreach (var (index, queueData) in updatedQueues)
                {
                    _logger.LogInformation($"Queue {index} updated");
                    QueueUpdated?.Invoke(this, new QueueUpdateEventArgs
                    {
                        QueueIndex = index,
                        UpdatedData = queueData,
                        UpdateTime = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing data update");
            }
        }

        private string ExtractQueueIndex(string fullName)
        {
            // Extract index from full name (e.g., "GPV1.1" -> "1.1")
            var match = Regex.Match(fullName, @"GPV(\d+\.\d+)");
            return match.Success ? match.Groups[1].Value : fullName;
        }

        private PowerStatus ParsePowerStatus(string status)
        {
            return status?.ToLower() switch
            {
                "yes" => PowerStatus.Yes,
                "no" => PowerStatus.No,
                "first" => PowerStatus.First,
                "second" => PowerStatus.Second,
                _ => PowerStatus.No
            };
        }

        private bool AreQueuesEqual(QueueData q1, QueueData q2)
        {
            if (q1.HourlyStatus.Count != q2.HourlyStatus.Count)
                return false;

            foreach (var kvp in q1.HourlyStatus)
            {
                if (!q2.HourlyStatus.ContainsKey(kvp.Key) ||
                    q2.HourlyStatus[kvp.Key] != kvp.Value)
                    return false;
            }

            return true;
        }

        public Dictionary<string, QueueData> GetAllQueues()
        {
            lock (_lock)
            {
                return new Dictionary<string, QueueData>(_queues);
            }
        }

        public QueueData GetQueue(string index)
        {
            lock (_lock)
            {
                return _queues.TryGetValue(index, out var queue) ? queue : null;
            }
        }

        public bool HasQueue(string index)
        {
            lock (_lock)
            {
                return _queues.ContainsKey(index);
            }
        }

        public void InitializeQueue(string queueIndex, QueueData queueData)
        {
            if (string.IsNullOrEmpty(queueIndex) || queueData == null)
            {
                _logger.LogWarning("Attempted to initialize queue with null or empty data");
                return;
            }

            lock (_lock)
            {
                _queues[queueIndex] = queueData;
                _logger.LogInformation($"Initialized queue {queueIndex} with persisted data from database");
            }
        }

        public override void Dispose()
        {
            _unsubscribe?.Invoke();
            base.Dispose();
        }
    }
}