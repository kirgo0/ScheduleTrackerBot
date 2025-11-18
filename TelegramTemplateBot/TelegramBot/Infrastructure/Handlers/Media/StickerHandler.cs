using Microsoft.Extensions.Logging;
using ScheduleTrackingBot.TelegramBot.Core.Attributes;
using ScheduleTrackingBot.TelegramBot.Core.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleTrackingBot.TelegramBot.Infrastructure.Handlers.Media
{
    [HandleSticker]
    public class StickerHandler : ITelegramHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<StickerHandler> _logger;

        public StickerHandler(
            ITelegramBotClient botClient,
            ILogger<StickerHandler> logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        public async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
        {
            var message = update.Message;
            if (message?.Sticker == null || message.Chat == null) return;

            _logger.LogInformation("User {UserId} sent a sticker", message.From?.Id);

            await _botClient.SendMessage(
                chatId: message.Chat.Id,
                text: $"Cool sticker! 😎\n" +
                      $"Sticker emoji: {message.Sticker.Emoji ?? "N/A"}\n" +
                      $"Set name: {message.Sticker.SetName ?? "N/A"}",
                cancellationToken: cancellationToken);
        }
    }

}
