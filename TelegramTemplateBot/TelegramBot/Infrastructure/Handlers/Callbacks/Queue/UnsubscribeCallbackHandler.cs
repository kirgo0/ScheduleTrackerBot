using DisconnectionSchedule.Services;
using Microsoft.Extensions.Logging;
using ScheduleTrackingBot.TelegramBot.Core.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramTemplateBot.TelegramBot.Infrastructure.Handlers.Callbacks.Queue
{
    [HandleCallback("unsubscribe_")]
    public class UnsubscribeCallbackHandler : QueueCallbackHandler
    {
        public UnsubscribeCallbackHandler(
            ITelegramBotClient botClient,
            IDisconnectionDataParser dataParser,
            IUserRepository userRepository,
            ILogger<UnsubscribeCallbackHandler> logger)
            : base(botClient, dataParser, userRepository, logger)
        {
        }

        public override async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
        {
            var callback = update.CallbackQuery;
            if (callback?.Message?.Chat == null || callback.Data == null) return;

            // Check if this is an "unsubscribe_" callback
            if (!callback.Data.StartsWith("unsubscribe_")) return;

            var queueIndex = callback.Data.Substring(12);
            var chatId = callback.Message.Chat.Id;

            _logger.LogInformation("User {UserId} unsubscribing from queue {QueueIndex} via callback",
                chatId, queueIndex);

            try
            {
                var success = await _userRepository.UnsubscribeFromQueueAsync(chatId, queueIndex);

                if (success)
                {
                    await _botClient.AnswerCallbackQuery(
                        callback.Id,
                        text: $"✅ Unsubscribed from queue {queueIndex}",
                        showAlert: false,
                        cancellationToken: cancellationToken);

                    // Refresh the view
                    await ShowQueueDetailsAsync(callback, queueIndex, cancellationToken);
                }
                else
                {
                    await _botClient.AnswerCallbackQuery(
                        callback.Id,
                        text: "Failed to unsubscribe",
                        showAlert: true,
                        cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing via callback");
                await _botClient.AnswerCallbackQuery(
                    callback.Id,
                    text: "Error occurred",
                    showAlert: true,
                    cancellationToken: cancellationToken);
            }
        }
    }
}
