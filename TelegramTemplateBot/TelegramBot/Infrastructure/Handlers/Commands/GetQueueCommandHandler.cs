using DisconnectionSchedule.Helper;
using DisconnectionSchedule.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ScheduleTrackingBot.TelegramBot.Core.Attributes;
using ScheduleTrackingBot.TelegramBot.Core.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramTemplateBot.Extensions;

namespace TelegramTemplateBot.TelegramBot.Infrastructure.Handlers.Commands
{
    [HandleCommand("/get")]
    public class GetQueueCommandHandler : ITelegramHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IDisconnectionDataParser _dataParser;
        private readonly ILogger<GetQueueCommandHandler> _logger;

        public GetQueueCommandHandler(
            ITelegramBotClient botClient,
            IDisconnectionDataParser dataParser,
            ILogger<GetQueueCommandHandler> logger)
        {
            _botClient = botClient;
            _dataParser = dataParser;
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
                    text: "Please specify queue index. Example: /get 1.1",
                    cancellationToken: cancellationToken);
                return;
            }

            var queueIndex = parts[1].Trim();
            _logger.LogInformation("User {UserId} requested queue {QueueIndex}", chatId, queueIndex);

            try
            {
                var queue = _dataParser.GetQueue(queueIndex);

                var messageText = QueueFormatter.FormatQueue(queue);
                
                await _botClient.SendQueuePhotoFromFile(queueIndex, chatId, messageText, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting queue {QueueIndex} for user {UserId}",
                    queueIndex, chatId);
            }
        }
    }
}
