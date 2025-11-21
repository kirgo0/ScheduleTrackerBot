using DisconnectionSchedule.Services;
using Microsoft.Extensions.Logging;
using ScheduleTrackingBot.TelegramBot.Core.Attributes;
using ScheduleTrackingBot.TelegramBot.Core.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ScheduleTrackingBot.TelegramBot.Infrastructure.Handlers.Commands
{
    [HandleCommand("/start")]
    public class StartCommandHandler : ITelegramHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<StartCommandHandler> _logger;

        public StartCommandHandler(
            ITelegramBotClient botClient,
            IUserRepository userRepository,
            ILogger<StartCommandHandler> logger)
        {
            _botClient = botClient;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
        {
            var message = update.Message;
            if (message?.Chat == null) return;

            var chatId = message.Chat.Id;
            _logger.LogInformation("User {UserId} started bot", chatId);

            try
            {
                // Create or get user
                var user = await _userRepository.CreateUserAsync(chatId);

                var welcomeMessage = "🔌 *Welcome to Power Schedule Bot!*\n\n" +
                    "I will help you track power disconnection schedules and notify you about updates.\n\n" +
                    "*Available commands:*\n" +
                    "/all - View all queues\n" +
                    "/subscriptions - Manage your subscriptions\n" +
                    "/get <index> - Get queue schedule\n" +
                    "/subscribe <index> - Subscribe to queue\n" +
                    "/unsubscribe <index> - Unsubscribe from queue\n" +
                    "/help - Show this help message";

                await _botClient.SendMessage(
                    chatId: chatId,
                    text: welcomeMessage,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in StartCommandHandler for user {UserId}", chatId);
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "Sorry, an error occurred. Please try again later.",
                    cancellationToken: cancellationToken);
            }
        }
    }
}
