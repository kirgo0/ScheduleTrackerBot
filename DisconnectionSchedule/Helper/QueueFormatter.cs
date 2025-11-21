using DisconnectionSchedule.Models;
using System.Text;

namespace DisconnectionSchedule.Helper
{
    public static class QueueFormatter
    {
        private const string PowerOnEmoji = "✅";
        private const string PowerOffEmoji = "⛔";
        private const string PowerFirstHalfEmoji = "🟡";
        private const string PowerSecondHalfEmoji = "🟠";
        private const string ClockEmoji = "🕐";

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
            sb.AppendLine("*Schedule for 24 hours:*");

            // Format hours in groups of 6 for better readability
            for (int startHour = 1; startHour <= 24; startHour += 6)
            {
                sb.AppendLine();
                sb.Append($"{ClockEmoji} *{startHour:D2}:00 - {Math.Min(startHour + 5, 24):D2}:00*: ");

                for (int hour = startHour; hour <= Math.Min(startHour + 5, 24); hour++)
                {
                    if (queue.HourlyStatus.TryGetValue(hour, out var status))
                    {
                        sb.Append(GetStatusEmoji(status));
                    }
                    else
                    {
                        sb.Append("❓");
                    }

                    if (hour < Math.Min(startHour + 5, 24))
                    {
                        sb.Append(" ");
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("*Legend:*");
            sb.AppendLine($"{PowerOnEmoji} - Power ON");
            sb.AppendLine($"{PowerOffEmoji} - Power OFF");
            sb.AppendLine($"{PowerFirstHalfEmoji} - First half of hour");
            sb.AppendLine($"{PowerSecondHalfEmoji} - Second half of hour");

            return sb.ToString();
        }

        public static string FormatQueueCompact(QueueData queue)
        {
            if (queue == null)
            {
                return "N/A";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"*{queue.FullName}*");

            // Single line format for all 24 hours
            for (int hour = 1; hour <= 24; hour++)
            {
                if (queue.HourlyStatus.TryGetValue(hour, out var status))
                {
                    sb.Append(GetStatusEmoji(status));
                }
                else
                {
                    sb.Append("❓");
                }

                // Add space every 3 hours for readability
                if (hour % 3 == 0 && hour < 24)
                {
                    sb.Append(" ");
                }
            }

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

            var currentHour = DateTime.UtcNow.Hour + 1; // Assuming hours are 1-24
            if (currentHour > 24) currentHour = 1;

            // Show current and next few hours prominently
            sb.AppendLine($"*Current Hour ({currentHour:D2}:00):*");
            if (queue.HourlyStatus.TryGetValue(currentHour, out var currentStatus))
            {
                sb.AppendLine($"{GetStatusEmoji(currentStatus)} {GetStatusText(currentStatus)}");
            }

            sb.AppendLine();
            sb.AppendLine($"*Next 6 hours:*");
            for (int i = 1; i <= 6; i++)
            {
                int hour = currentHour + i;
                if (hour > 24) hour = hour - 24;

                if (queue.HourlyStatus.TryGetValue(hour, out var status))
                {
                    sb.AppendLine($"{hour:D2}:00 - {GetStatusEmoji(status)} {GetStatusText(status)}");
                }
            }

            sb.AppendLine();
            sb.AppendLine($"_View full schedule with /get {queue.Index}_");

            return sb.ToString();
        }

        private static string GetStatusEmoji(PowerStatus status)
        {
            return status switch
            {
                PowerStatus.Yes => PowerOnEmoji,
                PowerStatus.No => PowerOffEmoji,
                PowerStatus.First => PowerFirstHalfEmoji,
                PowerStatus.Second => PowerSecondHalfEmoji,
                _ => "❓"
            };
        }

        private static string GetStatusText(PowerStatus status)
        {
            return status switch
            {
                PowerStatus.Yes => "Power ON",
                PowerStatus.No => "Power OFF",
                PowerStatus.First => "First 30 min",
                PowerStatus.Second => "Last 30 min",
                _ => "Unknown"
            };
        }
    }
}
