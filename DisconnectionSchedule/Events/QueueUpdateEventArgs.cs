using DisconnectionSchedule.Models;

namespace DisconnectionSchedule.Events
{
    public class QueueUpdateEventArgs : EventArgs
    {
        public string QueueIndex { get; set; }
        public QueueData UpdatedData { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
