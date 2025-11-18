using Microsoft.Extensions.Logging;
using ScheduleTrackingBot.TelegramBot.Core.Attributes;
using ScheduleTrackingBot.TelegramBot.Core.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleTrackingBot.TelegramBot.Infrastructure.Handlers.Callbacks
{
    [HandleText("hello")]
    [HandleText("hi")]
    public class HelloTextHandler : ITelegramHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<HelloTextHandler> _logger;

        public HelloTextHandler(
            ITelegramBotClient botClient,
            ILogger<HelloTextHandler> logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        public async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
        {
            var message = update.Message;
            if (message?.Chat == null) return;

            _logger.LogInformation("User {UserId} sent a greeting", message.From?.Id);

            await _botClient.SendMessage(
                chatId: message.Chat.Id,
                text: $"Hello, {message.From?.FirstName}! 😊\n\n" +
                      "I detected that you greeted me. This handler responds to any message " +
                      "containing 'hello' or 'hi'.",
                cancellationToken: cancellationToken);
        }
    }
}
