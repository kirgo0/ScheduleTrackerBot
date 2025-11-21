using DisconnectionSchedule.Models;

namespace DisconnectionSchedule.Services
{
    public interface IQueueImageService
    {
        Task<string> GenerateImageAsync(QueueData data, string outputPath);
    }
}
