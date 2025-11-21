using DisconnectionSchedule.Models;

namespace DisconnectionSchedule.Services.Interfaces
{
    public interface IQueueRepository
    {
        Task<QueueData> GetQueueByIndexAsync(string queueIndex);
        Task<QueueData> UpsertQueueAsync(QueueData queueData);
        Task<Dictionary<string, QueueData>> GetAllQueuesAsync();
        Task<bool> DeleteQueueAsync(string queueIndex);
    }
}
