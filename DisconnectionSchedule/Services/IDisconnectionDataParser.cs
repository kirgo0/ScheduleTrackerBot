using DisconnectionSchedule.Events;
using DisconnectionSchedule.Models;

namespace DisconnectionSchedule.Services
{
    public interface IDisconnectionDataParser
    {
        event EventHandler<QueueUpdateEventArgs> QueueUpdated;
        Dictionary<string, QueueData> GetAllQueues();
        QueueData GetQueue(string index);
        bool HasQueue(string index);
    }
}
