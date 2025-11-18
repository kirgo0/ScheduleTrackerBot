using Microsoft.Extensions.Logging;
using ScheduleTrackingBot.TelegramBot.Core.Attributes;
using ScheduleTrackingBot.TelegramBot.Core.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleTrackingBot.TelegramBot.Infrastructure.Handlers.Callbacks
{
    [HandleCallback("info")]
    public class InfoCallbackHandler : ITelegramHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<InfoCallbackHandler> _logger;

        public InfoCallbackHandler(
            ITelegramBotClient botClient,
            ILogger<InfoCallbackHandler> logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        public async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
        {
            var callbackQuery = update.CallbackQuery;
            if (callbackQuery?.Message == null) return;

            _logger.LogInformation("User {UserId} requested info", callbackQuery.From.Id);

            await _botClient.AnswerCallbackQuery(
                callbackQueryId: callbackQuery.Id,
                cancellationToken: cancellationToken);

            await _botClient.SendMessage(
                chatId: callbackQuery.Message.Chat.Id,
                text: "ℹ️ <b>Bot Information</b>\n\n" +
                      "This bot is built with:\n" +
                      "• .NET 10\n" +
                      "• Telegram.Bot library\n" +
                      "• Attribute-based routing\n" +
                      "• Dependency Injection\n" +
                      "• Background hosted service\n\n" +
                      "All handlers are automatically registered and scoped!",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken);
        }
    }
}
