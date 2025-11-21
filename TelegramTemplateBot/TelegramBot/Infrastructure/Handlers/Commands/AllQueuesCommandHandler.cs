using DisconnectionSchedule.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ScheduleTrackingBot.TelegramBot.Core.Attributes;
using ScheduleTrackingBot.TelegramBot.Core.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramTemplateBot.TelegramBot.Infrastructure.Handlers.Commands
{
    [HandleCommand("/all")]
    public class AllQueuesCommandHandler : ITelegramHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IDisconnectionDataParser _dataParser;
        private readonly ILogger<AllQueuesCommandHandler> _logger;

        public AllQueuesCommandHandler(
            ITelegramBotClient botClient,
            IDisconnectionDataParser dataParser,
            ILogger<AllQueuesCommandHandler> logger)
        {
            _botClient = botClient;
            _dataParser = dataParser;
            _logger = logger;
        }

        public async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
        {
            var message = update.Message;
            if (message?.Chat == null) return;

            var chatId = message.Chat.Id;
            _logger.LogInformation("User {UserId} requested all queues", chatId);

            try
            {
                var queues = _dataParser.GetAllQueues();

                if (!queues.Any())
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "No queues available at the moment. Please try again later.",
                        cancellationToken: cancellationToken);
                    return;
                }

                var buttons = new List<List<InlineKeyboardButton>>();

                // Create buttons in rows of 3
                var queueList = queues.OrderBy(q => q.Key).ToList();
                for (int i = 0; i < queueList.Count; i += 3)
                {
                    var row = new List<InlineKeyboardButton>();
                    for (int j = i; j < Math.Min(i + 3, queueList.Count); j++)
                    {
                        var queue = queueList[j];
                        row.Add(InlineKeyboardButton.WithCallbackData(
                            $"GPV{queue.Key}",
                            $"show_{queue.Key}"));
                    }
                    buttons.Add(row);
                }

                var keyboard = new InlineKeyboardMarkup(buttons);
                var msg = "📊 *Available Power Queues*\n\nSelect a queue to view details:";

                await _botClient.SendMessage(
                    chatId: chatId,
                    text: msg,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error displaying all queues for user {UserId}", chatId);
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "Sorry, an error occurred while fetching queues. Please try again later.",
                    cancellationToken: cancellationToken);
            }
        }
    }
}
