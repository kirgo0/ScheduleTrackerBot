using DisconnectionSchedule.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ScheduleTrackingBot.TelegramBot.Core.Attributes;
using ScheduleTrackingBot.TelegramBot.Core.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramTemplateBot.TelegramBot.Infrastructure.Handlers.Commands
{
    [HandleCommand("/unsubscribe")]
    public class UnsubscribeCommandHandler : ITelegramHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UnsubscribeCommandHandler> _logger;

        public UnsubscribeCommandHandler(
            ITelegramBotClient botClient,
            IUserRepository userRepository,
            ILogger<UnsubscribeCommandHandler> logger)
        {
            _botClient = botClient;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
        {
            var message = update.Message;
            if (message?.Chat == null || message.Text == null) return;

            var chatId = message.Chat.Id;
            var parts = message.Text.Split(' ', 2);

            if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1]))
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "Please specify queue index. Example: /unsubscribe 1.1",
                    cancellationToken: cancellationToken);
                return;
            }

            var queueIndex = parts[1].Trim();
            _logger.LogInformation("User {UserId} unsubscribing from queue {QueueIndex}", chatId, queueIndex);

            try
            {
                var success = await _userRepository.UnsubscribeFromQueueAsync(chatId, queueIndex);

                if (success)
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: $"✅ Successfully unsubscribed from queue {queueIndex}",
                        cancellationToken: cancellationToken);

                    _logger.LogInformation("User {UserId} successfully unsubscribed from queue {QueueIndex}",
                        chatId, queueIndex);
                }
                else
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "Failed to unsubscribe. You might not be subscribed to this queue.",
                        cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing user {UserId} from queue {QueueIndex}",
                    chatId, queueIndex);
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "Sorry, an error occurred while unsubscribing. Please try again later.",
                    cancellationToken: cancellationToken);
            }
        }
    }
}
