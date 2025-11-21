using Microsoft.Extensions.Logging;
using ScheduleTrackingBot.TelegramBot.Core.Attributes;
using ScheduleTrackingBot.TelegramBot.Core.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramTemplateBot.TelegramBot.Infrastructure.Handlers.Commands
{
    [HandleCommand("/help")]
    public class HelpCommandHandler : ITelegramHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<HelpCommandHandler> _logger;

        public HelpCommandHandler(
            ITelegramBotClient botClient,
            ILogger<HelpCommandHandler> logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        public async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
        {
            var message = update.Message;
            if (message?.Chat == null) return;

            var chatId = message.Chat.Id;
            _logger.LogInformation("User {UserId} requested help", chatId);

            var helpMessage = "📚 *Bot Commands:*\n\n" +
                "/start - Initialize bot and create user\n" +
                "/all - View all available queues\n" +
                "/subscriptions - View and manage your subscriptions\n" +
                "/get <index> - Get specific queue schedule\n" +
                "/subscribe <index> - Subscribe to queue updates\n" +
                "/unsubscribe <index> - Unsubscribe from queue\n" +
                "/help - Show this help message\n\n" +
                "Example: `/subscribe 1.1` to subscribe to GPV1.1 queue\n\n" +
                "*Legend:*\n" +
                "✅ - Power ON\n" +
                "⛔ - Power OFF\n" +
                "🟡 - First 30 minutes\n" +
                "🟠 - Last 30 minutes";

            await _botClient.SendMessage(
                chatId: chatId,
                text: helpMessage,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);
        }
    }
}
