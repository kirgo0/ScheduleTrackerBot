using Microsoft.Extensions.Logging;
using ScheduleTrackingBot.TelegramBot.Core.Attributes;
using ScheduleTrackingBot.TelegramBot.Core.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleTrackingBot.TelegramBot.Infrastructure.Handlers.Callbacks
{
    [HandleCallback("hello")]
    public class HelloCallbackHandler : ITelegramHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<HelloCallbackHandler> _logger;

        public HelloCallbackHandler(
            ITelegramBotClient botClient,
            ILogger<HelloCallbackHandler> logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        public async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
        {
            var callbackQuery = update.CallbackQuery;
            if (callbackQuery?.Message == null) return;

            _logger.LogInformation("User {UserId} clicked hello button", callbackQuery.From.Id);

            // Answer the callback query to remove the loading state
            await _botClient.AnswerCallbackQuery(
                callbackQueryId: callbackQuery.Id,
                text: "Hello! 👋",
                cancellationToken: cancellationToken);

            // Send a message
            await _botClient.SendMessage(
                chatId: callbackQuery.Message.Chat.Id,
                text: $"Hello, {callbackQuery.From.FirstName}! 🎉\n\n" +
                      "This is a callback handler response. You can handle any callback data " +
                      "by creating a service with the [HandleCallback] attribute!",
                cancellationToken: cancellationToken);
        }
    }
}
