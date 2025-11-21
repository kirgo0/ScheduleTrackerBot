using DisconnectionSchedule.Services;
using Microsoft.Extensions.Logging;
using ScheduleTrackingBot.TelegramBot.Core.Attributes;
using ScheduleTrackingBot.TelegramBot.Core.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramTemplateBot.TelegramBot.Infrastructure.Handlers.Callbacks.Queue.Navigation
{
    [HandleCallback("back_to_subscriptions")]
    public class BackToSubscriptionsCallbackHandler : ITelegramHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserRepository _userRepository;
        private readonly IDisconnectionDataParser _dataParser;
        private readonly ILogger<BackToSubscriptionsCallbackHandler> _logger;

        public BackToSubscriptionsCallbackHandler(
            ITelegramBotClient botClient,
            IUserRepository userRepository,
            IDisconnectionDataParser dataParser,
            ILogger<BackToSubscriptionsCallbackHandler> logger)
        {
            _botClient = botClient;
            _userRepository = userRepository;
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
                var subscriptions = await _userRepository.GetUserSubscriptionsAsync(chatId);

                if (!subscriptions.Any())
                {
                    await _botClient.EditMessageText(
                        chatId: chatId,
                        messageId: messageId,
                        text: "You have no active subscriptions.\n\nUse /all to see available queues.",
                        cancellationToken: cancellationToken);
                    return;
                }

                var buttons = new List<List<InlineKeyboardButton>>();

                foreach (var queueIndex in subscriptions.OrderBy(s => s))
                {
                    var queue = _dataParser.GetQueue(queueIndex);
                    if (queue != null)
                    {
                        buttons.Add(new List<InlineKeyboardButton>
                        {
                            InlineKeyboardButton.WithCallbackData(
                                $"📊 {queue.FullName}",
                                $"show_{queueIndex}"),
                            InlineKeyboardButton.WithCallbackData(
                                "❌ Unsubscribe",
                                $"unsubscribe_{queueIndex}")
                        });
                    }
                }

                var keyboard = new InlineKeyboardMarkup(buttons);
                var msg = "📋 *Your Subscriptions*\n\nSelect a queue to view or unsubscribe:";

                await _botClient.EditMessageText(
                    chatId: chatId,
                    messageId: messageId,
                    text: msg,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating back to subscriptions");
            }
        }
    }
}
