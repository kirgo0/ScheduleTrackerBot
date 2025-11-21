using DisconnectionSchedule.Models;
using DisconnectionSchedule.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace DisconnectionSchedule.Services.Implementations
{
    public class QueueRepository : IQueueRepository
    {
        private readonly IMongoCollection<QueueData> _queuesCollection;
        private readonly IMemoryCache _cache;
        private readonly ILogger<QueueRepository> _logger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        public QueueRepository(
            IMongoDatabase database,
            IMemoryCache cache,
            ILogger<QueueRepository> logger)
        {
            _queuesCollection = database.GetCollection<QueueData>("queues");
            _cache = cache;
            _logger = logger;

            // Create indexes
            var indexKeysDefinition = Builders<QueueData>.IndexKeys.Ascending(q => q.Index);
            _queuesCollection.Indexes.CreateOneAsync(new CreateIndexModel<QueueData>(indexKeysDefinition,
                new CreateIndexOptions { Unique = true }));
        }

        public async Task<QueueData> GetQueueByIndexAsync(string queueIndex)
        {
            var cacheKey = $"queue_{queueIndex}";

            if (_cache.TryGetValue<QueueData>(cacheKey, out var cachedQueue))
            {
                return cachedQueue;
            }

            var queue = await _queuesCollection
                .Find(q => q.Index == queueIndex)
                .FirstOrDefaultAsync();

            if (queue != null)
            {
                _cache.Set(cacheKey, queue, _cacheExpiration);
            }

            return queue;
        }

        public async Task<QueueData> UpsertQueueAsync(QueueData queueData)
        {
            if (queueData == null)
            {
                throw new ArgumentNullException(nameof(queueData));
            }

            var filter = Builders<QueueData>.Filter.Eq(q => q.Index, queueData.Index);

            var options = new FindOneAndReplaceOptions<QueueData>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            var updatedQueue = await _queuesCollection.FindOneAndReplaceAsync(filter, queueData, options);

            // Update cache
            var cacheKey = $"queue_{queueData.Index}";
            _cache.Set(cacheKey, updatedQueue, _cacheExpiration);

            _logger.LogInformation($"Upserted queue data for index: {queueData.Index}");

            return updatedQueue;
        }

        public async Task<Dictionary<string, QueueData>> GetAllQueuesAsync()
        {
            var cacheKey = "all_queues";

            if (_cache.TryGetValue<Dictionary<string, QueueData>>(cacheKey, out var cachedQueues))
            {
                return cachedQueues;
            }

            var queues = await _queuesCollection
                .Find(_ => true)
                .ToListAsync();

            var queueDict = queues.ToDictionary(q => q.Index, q => q);

            _cache.Set(cacheKey, queueDict, _cacheExpiration);

            return queueDict;
        }

        public async Task<bool> DeleteQueueAsync(string queueIndex)
        {
            var result = await _queuesCollection.DeleteOneAsync(q => q.Index == queueIndex);

            if (result.DeletedCount > 0)
            {
                // Invalidate cache
                var cacheKey = $"queue_{queueIndex}";
                _cache.Remove(cacheKey);
                _cache.Remove("all_queues");

                _logger.LogInformation($"Deleted queue: {queueIndex}");
                return true;
            }

            return false;
        }

        private bool AreQueuesEqual(QueueData queue1, QueueData queue2)
        {
            if (queue1 == null || queue2 == null)
                return false;

            // Compare basic properties
            if (queue1.Index != queue2.Index)
            {
                return false;
            }

            return true;
        }
    }
}