using DisconnectionSchedule.Models;
using DisconnectionSchedule.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace DisconnectionSchedule.Services.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMemoryCache _cache;
        private readonly ILogger<UserRepository> _logger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        public UserRepository(
            IMongoDatabase database,
            IMemoryCache cache,
            ILogger<UserRepository> logger)
        {
            _usersCollection = database.GetCollection<User>("users");
            _cache = cache;
            _logger = logger;

            // Create indexes
            var indexKeysDefinition = Builders<User>.IndexKeys.Ascending(u => u.TelegramId);
            _usersCollection.Indexes.CreateOneAsync(new CreateIndexModel<User>(indexKeysDefinition));
        }

        public async Task<User> GetUserByTelegramIdAsync(long telegramId)
        {
            var cacheKey = $"user_{telegramId}";

            if (_cache.TryGetValue<User>(cacheKey, out var cachedUser))
            {
                return cachedUser;
            }

            var user = await _usersCollection
                .Find(u => u.TelegramId == telegramId)
                .FirstOrDefaultAsync();

            if (user != null)
            {
                _cache.Set(cacheKey, user, _cacheExpiration);
            }

            return user;
        }

        public async Task<User> CreateUserAsync(long telegramId)
        {
            var existingUser = await GetUserByTelegramIdAsync(telegramId);
            if (existingUser != null)
            {
                return existingUser;
            }

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                TelegramId = telegramId,
                SubscribedQueues = new List<string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _usersCollection.InsertOneAsync(user);

            var cacheKey = $"user_{telegramId}";
            _cache.Set(cacheKey, user, _cacheExpiration);

            _logger.LogInformation($"Created new user with Telegram ID: {telegramId}");

            return user;
        }

        public async Task<bool> SubscribeToQueueAsync(long telegramId, string queueIndex)
        {
            var user = await GetUserByTelegramIdAsync(telegramId);
            if (user == null)
            {
                return false;
            }

            if (user.SubscribedQueues.Contains(queueIndex))
            {
                return true; // Already subscribed
            }

            var update = Builders<User>.Update
                .AddToSet(u => u.SubscribedQueues, queueIndex)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _usersCollection.UpdateOneAsync(
                u => u.TelegramId == telegramId,
                update);

            if (result.ModifiedCount > 0)
            {
                // Update cache
                user.SubscribedQueues.Add(queueIndex);
                user.UpdatedAt = DateTime.UtcNow;

                var cacheKey = $"user_{telegramId}";
                _cache.Set(cacheKey, user, _cacheExpiration);

                // Invalidate queue subscribers cache
                _cache.Remove($"queue_subscribers_{queueIndex}");

                _logger.LogInformation($"User {telegramId} subscribed to queue {queueIndex}");
                return true;
            }

            return false;
        }

        public async Task<bool> UnsubscribeFromQueueAsync(long telegramId, string queueIndex)
        {
            var user = await GetUserByTelegramIdAsync(telegramId);
            if (user == null)
            {
                return false;
            }

            if (!user.SubscribedQueues.Contains(queueIndex))
            {
                return true; // Already unsubscribed
            }

            var update = Builders<User>.Update
                .Pull(u => u.SubscribedQueues, queueIndex)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _usersCollection.UpdateOneAsync(
                u => u.TelegramId == telegramId,
                update);

            if (result.ModifiedCount > 0)
            {
                // Update cache
                user.SubscribedQueues.Remove(queueIndex);
                user.UpdatedAt = DateTime.UtcNow;

                var cacheKey = $"user_{telegramId}";
                _cache.Set(cacheKey, user, _cacheExpiration);

                // Invalidate queue subscribers cache
                _cache.Remove($"queue_subscribers_{queueIndex}");

                _logger.LogInformation($"User {telegramId} unsubscribed from queue {queueIndex}");
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<User>> GetUsersSubscribedToQueueAsync(string queueIndex)
        {
            var cacheKey = $"queue_subscribers_{queueIndex}";

            if (_cache.TryGetValue<List<User>>(cacheKey, out var cachedUsers))
            {
                return cachedUsers;
            }

            var users = await _usersCollection
                .Find(u => u.SubscribedQueues.Contains(queueIndex))
                .ToListAsync();

            _cache.Set(cacheKey, users, _cacheExpiration);

            return users;
        }

        public async Task<IEnumerable<string>> GetUserSubscriptionsAsync(long telegramId)
        {
            var user = await GetUserByTelegramIdAsync(telegramId);
            return user?.SubscribedQueues ?? new List<string>();
        }
    }
}
