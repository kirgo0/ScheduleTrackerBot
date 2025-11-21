using DisconnectionSchedule.Events;
using DisconnectionSchedule.Models;

namespace DisconnectionSchedule.Services.Interfaces
{
    public interface IDisconnectionDataParser
    {
        event EventHandler<QueueUpdateEventArgs> QueueUpdated;
        Dictionary<string, QueueData> GetAllQueues();
        QueueData GetQueue(string index);
        bool HasQueue(string index);
        void InitializeQueue(string queueIndex, QueueData queueData);
    }
}
