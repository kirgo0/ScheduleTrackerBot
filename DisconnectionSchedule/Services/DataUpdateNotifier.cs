using Microsoft.Extensions.Logging;

namespace DisconnectionSchedule.Services
{
    public class DataUpdateNotifier : IDataUpdateNotifier
    {
        private readonly ILogger<DataUpdateNotifier> _logger;
        private readonly List<Func<string, Task>> _subscribers;
        private readonly object _lock = new object();

        public DataUpdateNotifier(ILogger<DataUpdateNotifier> logger)
        {
            _logger = logger;
            _subscribers = new List<Func<string, Task>>();
        }

        /// <summary>
        /// Subscribe to data update events
        /// </summary>
        /// <param name="callback">Callback function that receives the updated content</param>
        /// <returns>Unsubscribe action</returns>
        public Action Subscribe(Func<string, Task> callback)
        {
            lock (_lock)
            {
                _subscribers.Add(callback);
                _logger.LogInformation("New subscriber added. Total subscribers: {Count}", _subscribers.Count);
            }

            // Return unsubscribe action
            return () =>
            {
                lock (_lock)
                {
                    _subscribers.Remove(callback);
                    _logger.LogInformation("Subscriber removed. Total subscribers: {Count}", _subscribers.Count);
                }
            };
        }

        public void Unsubscribe(Action subscription)
        {
            subscription?.Invoke();
        }

        /// <summary>
        /// Notify all subscribers about data update
        /// </summary>
        /// <param name="updatedContent">The new content</param>
        public async Task NotifyUpdate(string updatedContent)
        {
            List<Func<string, Task>> subscribersCopy;

            lock (_lock)
            {
                subscribersCopy = new List<Func<string, Task>>(_subscribers);
            }

            _logger.LogInformation("Notifying {Count} subscribers about data update", subscribersCopy.Count);

            var tasks = new List<Task>();
            foreach (var subscriber in subscribersCopy)
            {
                tasks.Add(NotifySubscriber(subscriber, updatedContent));
            }

            await Task.WhenAll(tasks);
        }

        private async Task NotifySubscriber(Func<string, Task> subscriber, string content)
        {
            try
            {
                await subscriber(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying subscriber");
            }
        }
    }
}
