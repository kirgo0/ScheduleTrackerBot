
namespace DisconnectionSchedule.Models
{
    public class User
    {
        public string Id { get; set; }
        public long TelegramId { get; set; }
        public List<string> SubscribedQueues { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
