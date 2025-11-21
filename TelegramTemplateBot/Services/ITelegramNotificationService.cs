using DisconnectionSchedule.Models;
using Telegram.Bot.Types.Enums;

namespace TelegramTemplateBot.Services
{
    public interface ITelegramNotificationService
    {
        Task SendMessageToUserAsync(long telegramId, string message, string queueIndex, ParseMode parseMode = ParseMode.Markdown);
        Task SendQueueUpdateToUserAsync(long telegramId, QueueData queue);
    }
}
