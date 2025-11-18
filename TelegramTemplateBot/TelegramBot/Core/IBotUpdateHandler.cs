using Telegram.Bot.Types;

namespace ScheduleTrackingBot.TelegramBot.Core
{
    /// <summary>
    /// Interface for the main bot update handler
    /// </summary>
    public interface IBotUpdateHandler
    {
        Task HandleUpdateAsync(Update update, CancellationToken cancellationToken);
        Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken);
    }
}
