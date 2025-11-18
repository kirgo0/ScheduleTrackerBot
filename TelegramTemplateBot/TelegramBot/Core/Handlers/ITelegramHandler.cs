using Telegram.Bot.Types;

namespace ScheduleTrackingBot.TelegramBot.Core.Handlers
{
    /// <summary>
    /// Base interface for all Telegram update handlers
    /// </summary>
    public interface ITelegramHandler
    {
        Task HandleAsync(Update update, CancellationToken cancellationToken = default);
    }
}
