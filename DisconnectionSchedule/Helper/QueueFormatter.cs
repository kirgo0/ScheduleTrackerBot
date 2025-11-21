using DisconnectionSchedule.Models;
using System.Text;

namespace DisconnectionSchedule.Helper
{
    public static class QueueFormatter
    {
        public static string FormatQueue(QueueData queue)
        {
            if (queue == null)
            {
                return "Queue data not available";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"📊 *Queue: {queue.FullName}*");
            sb.AppendLine($"⏰ _Updated: {queue.LastUpdated:yyyy-MM-dd HH:mm} UTC_");
            sb.AppendLine();

            return sb.ToString();
        }

        public static string FormatQueueUpdate(QueueData queue)
        {
            if (queue == null)
            {
                return "Queue data not available";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"⚡ *Power Schedule Update*");
            sb.AppendLine($"📊 Queue: *{queue.FullName}*");
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
