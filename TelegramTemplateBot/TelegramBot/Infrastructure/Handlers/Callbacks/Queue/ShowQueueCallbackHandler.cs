using DisconnectionSchedule.Services;
using Microsoft.Extensions.Logging;
using ScheduleTrackingBot.TelegramBot.Core.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramTemplateBot.TelegramBot.Infrastructure.Handlers.Callbacks.Queue
{
    [HandleCallback("show_")]
    public class ShowQueueCallbackHandler : QueueCallbackHandler
    {

        public ShowQueueCallbackHandler(
            ITelegramBotClient botClient,
            IDisconnectionDataParser dataParser,
            IUserRepository userRepository,
            ILogger<ShowQueueCallbackHandler> logger)
            : base(botClient, dataParser, userRepository, logger)
        {
        }

        public override async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
        {
            var callback = update.CallbackQuery;
            if (callback?.Message?.Chat == null || callback.Data == null) return;

            await _botClient.AnswerCallbackQuery(callback.Id, cancellationToken: cancellationToken);

            // Check if this is a "show_" callback
            if (!callback.Data.StartsWith("show_")) return;

            var queueIndex = callback.Data.Substring(5);
            _logger.LogInformation("User {UserId} viewing queue {QueueIndex}",
                callback.Message.Chat.Id, queueIndex);

            try
            {
                await ShowQueueDetailsAsync(callback, queueIndex, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing queue details");
            }
        }
    }
}
