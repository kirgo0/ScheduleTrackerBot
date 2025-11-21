using DisconnectionSchedule.Helper;
using DisconnectionSchedule.Services;
using Microsoft.Extensions.Logging;
using ScheduleTrackingBot.TelegramBot.Core.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramTemplateBot.TelegramBot.Infrastructure.Handlers.Callbacks.Queue
{
    public class QueueCallbackHandler : ITelegramHandler
    {
        protected readonly ITelegramBotClient _botClient;
        protected readonly IDisconnectionDataParser _dataParser;
        protected readonly IUserRepository _userRepository;
        protected readonly ILogger<QueueCallbackHandler> _logger;
        private IQueueImageService _imageService;

        public QueueCallbackHandler(
            ITelegramBotClient botClient,
            IDisconnectionDataParser dataParser,
            IUserRepository userRepository,
            ILogger<QueueCallbackHandler> logger,
            IQueueImageService imageService)
        {
            _botClient = botClient;
            _dataParser = dataParser;
            _userRepository = userRepository;
            _logger = logger;
            _imageService = imageService;
        }

        public virtual Task HandleAsync(Update update, CancellationToken cancellationToken = default)
        {
            // Base implementation - will be overridden by specific handlers
            return Task.CompletedTask;
        }

        protected async Task ShowQueueDetailsAsync(
            CallbackQuery callback,
            string queueIndex,
            CancellationToken cancellationToken)
        {
            var chatId = callback.Message.Chat.Id;
            var messageId = callback.Message.MessageId;

            var queue = _dataParser.GetQueue(queueIndex);

            if (queue == null)
            {
                await _botClient.EditMessageText(
                    chatId: chatId,
                    messageId: messageId,
                    text: "Queue not found.",
                    cancellationToken: cancellationToken);
                return;
            }

            var subscriptions = await _userRepository.GetUserSubscriptionsAsync(chatId);
            var isSubscribed = subscriptions.Contains(queueIndex);

            var messageText = QueueFormatter.FormatQueue(queue);

            var buttons = new List<List<InlineKeyboardButton>>();

            if (isSubscribed)
            {
                buttons.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(
                        "✅ Subscribed", "noop"),
                    InlineKeyboardButton.WithCallbackData(
                        "❌ Unsubscribe", $"unsubscribe_{queueIndex}")
                });
            }
            else
            {
                buttons.Add(new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(
                        "🔔 Subscribe", $"subscribe_{queueIndex}")
                });
            }

            buttons.Add(new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData("⬅️ Back to All", "back_to_all"),
                InlineKeyboardButton.WithCallbackData("📋 My Subscriptions", "back_to_subscriptions")
            });

            var keyboard = new InlineKeyboardMarkup(buttons);

            await _botClient.EditMessageText(
                chatId: chatId,
                messageId: messageId,
                text: messageText,
                parseMode: ParseMode.Markdown,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }
    }
}
