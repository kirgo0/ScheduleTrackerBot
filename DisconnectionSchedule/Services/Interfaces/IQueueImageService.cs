using DisconnectionSchedule.Models;

namespace DisconnectionSchedule.Services.Interfaces
{
    public interface IQueueImageService
    {
        Task<string> GenerateImageAsync(QueueData data, string outputPath);
    }
}
