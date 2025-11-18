using Microsoft.Extensions.Logging;
using ScheduleTrackingBot.TelegramBot.Core.Attributes;
using ScheduleTrackingBot.TelegramBot.Core.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleTrackingBot.TelegramBot.Infrastructure.Handlers.Commands
{
    [HandleCommand("/start")]
    public class StartCommandHandler : ITelegramHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<StartCommandHandler> _logger;

        public StartCommandHandler(
            ITelegramBotClient botClient,
            ILogger<StartCommandHandler> logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        public async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
        {
            var message = update.Message;
            if (message?.Chat == null) return;

            _logger.LogInformation("User {UserId} started the bot", message.From?.Id);

            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Say Hello 👋", "hello"),
                InlineKeyboardButton.WithCallbackData("Get Info ℹ️", "info")
            }
        });

            await _botClient.SendMessage(
                chatId: message.Chat.Id,
                text: $"Hello, {message.From?.FirstName}! 👋\n\n" +
                      "Welcome to the Telegram Bot Template. This bot demonstrates a flexible, " +
                      "attribute-based architecture for handling different types of updates.\n\n" +
                      "Try these buttons below or send me different types of content!",
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }
    }
}
