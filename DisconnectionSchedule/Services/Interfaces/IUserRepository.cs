using DisconnectionSchedule.Models;

namespace DisconnectionSchedule.Services.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserByTelegramIdAsync(long telegramId);
        Task<User> CreateUserAsync(long telegramId);
        Task<bool> SubscribeToQueueAsync(long telegramId, string queueIndex);
        Task<bool> UnsubscribeFromQueueAsync(long telegramId, string queueIndex);
        Task<IEnumerable<User>> GetUsersSubscribedToQueueAsync(string queueIndex);
        Task<IEnumerable<string>> GetUserSubscriptionsAsync(long telegramId);
    }
}
