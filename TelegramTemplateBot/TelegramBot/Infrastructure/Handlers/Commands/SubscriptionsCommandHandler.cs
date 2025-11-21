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
    [HandleCommand("/subscriptions")]
    public class SubscriptionsCommandHandler : ITelegramHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserRepository _userRepository;
        private readonly IDisconnectionDataParser _dataParser;
        private readonly ILogger<SubscriptionsCommandHandler> _logger;

        public SubscriptionsCommandHandler(
            ITelegramBotClient botClient,
            IUserRepository userRepository,
            IDisconnectionDataParser dataParser,
            ILogger<SubscriptionsCommandHandler> logger)
        {
            _botClient = botClient;
            _userRepository = userRepository;
            _dataParser = dataParser;
            _logger = logger;
        }

        public async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
        {
            var message = update.Message;
            if (message?.Chat == null) return;

            var chatId = message.Chat.Id;
            _logger.LogInformation("User {UserId} requested subscriptions", chatId);

            try
            {
                var subscriptions = await _userRepository.GetUserSubscriptionsAsync(chatId);

                if (!subscriptions.Any())
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
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

                await _botClient.SendMessage(
                    chatId: chatId,
                    text: msg,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error displaying subscriptions for user {UserId}", chatId);
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "Sorry, an error occurred while fetching your subscriptions. Please try again later.",
                    cancellationToken: cancellationToken);
            }
        }
    }
}
