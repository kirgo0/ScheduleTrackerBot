namespace DisconnectionSchedule.Models
{
    public class DisconnectionFact
    {
        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> Data { get; set; }
        public string Update { get; set; }
        public long Today { get; set; }
    }

}
