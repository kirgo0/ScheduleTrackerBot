using DisconnectionSchedule.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ScheduleTrackingBot.TelegramBot.Core.Attributes;
using ScheduleTrackingBot.TelegramBot.Core.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramTemplateBot.TelegramBot.Infrastructure.Handlers.Callbacks.Queue.Navigation
{
    [HandleCallback("back_to_all")]
    public class BackToAllCallbackHandler : ITelegramHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IDisconnectionDataParser _dataParser;
        private readonly ILogger<BackToAllCallbackHandler> _logger;

        public BackToAllCallbackHandler(
            ITelegramBotClient botClient,
            IDisconnectionDataParser dataParser,
            ILogger<BackToAllCallbackHandler> logger)
        {
            _botClient = botClient;
            _dataParser = dataParser;
            _logger = logger;
        }

        public async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
        {
            var callback = update.CallbackQuery;
            if (callback?.Message?.Chat == null) return;

            await _botClient.AnswerCallbackQuery(callback.Id, cancellationToken: cancellationToken);

            var chatId = callback.Message.Chat.Id;
            var messageId = callback.Message.MessageId;

            try
            {
                var queues = _dataParser.GetAllQueues();

                if (!queues.Any())
                {
                    await _botClient.EditMessageText(
                        chatId: chatId,
                        messageId: messageId,
                        text: "No queues available at the moment.",
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
                var message = "📊 *Available Power Queues*\n\nSelect a queue to view details:";

                await _botClient.EditMessageText(
                    chatId: chatId,
                    messageId: messageId,
                    text: message,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating back to all queues");
            }
        }
    }
}
