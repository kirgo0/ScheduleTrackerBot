namespace DisconnectionSchedule.Models
{
    public class QueueData
    {
        public string FullName { get; set; }
        public string Index { get; set; }
        public Dictionary<int, PowerStatus> HourlyStatus { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }
}
