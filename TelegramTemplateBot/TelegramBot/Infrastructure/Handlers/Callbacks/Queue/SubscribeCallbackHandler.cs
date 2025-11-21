using DisconnectionSchedule.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ScheduleTrackingBot.TelegramBot.Core.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramTemplateBot.TelegramBot.Infrastructure.Handlers.Callbacks.Queue
{
    [HandleCallback("subscribe_")]
    public class SubscribeCallbackHandler : QueueCallbackHandler
    {
        public SubscribeCallbackHandler(ITelegramBotClient botClient, IDisconnectionDataParser dataParser, IUserRepository userRepository, ILogger<QueueCallbackHandler> logger) : base(botClient, dataParser, userRepository, logger)
        {
        }

        public override async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
        {
            var callback = update.CallbackQuery;
            if (callback?.Message?.Chat == null || callback.Data == null) return;

            // Check if this is a "subscribe_" callback
            if (!callback.Data.StartsWith("subscribe_")) return;

            var queueIndex = callback.Data.Substring(10);
            var chatId = callback.Message.Chat.Id;

            _logger.LogInformation("User {UserId} subscribing to queue {QueueIndex} via callback",
                chatId, queueIndex);

            try
            {
                var success = await _userRepository.SubscribeToQueueAsync(chatId, queueIndex);

                if (success)
                {
                    await _botClient.AnswerCallbackQuery(
                        callback.Id,
                        text: $"✅ Subscribed to queue {queueIndex}",
                        showAlert: false,
                        cancellationToken: cancellationToken);

                    // Refresh the view
                    await ShowQueueDetailsAsync(callback, queueIndex, cancellationToken);
                }
                else
                {
                    await _botClient.AnswerCallbackQuery(
                        callback.Id,
                        text: "Failed to subscribe",
                        showAlert: true,
                        cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing via callback");
                await _botClient.AnswerCallbackQuery(
                    callback.Id,
                    text: "Error occurred",
                    showAlert: true,
                    cancellationToken: cancellationToken);
            }
        }
    }
}
