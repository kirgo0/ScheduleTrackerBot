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
    [HandleCommand("/subscribe")]
    public class SubscribeCommandHandler : ITelegramHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserRepository _userRepository;
        private readonly IDisconnectionDataParser _dataParser;
        private readonly ILogger<SubscribeCommandHandler> _logger;

        public SubscribeCommandHandler(
            ITelegramBotClient botClient,
            IUserRepository userRepository,
            IDisconnectionDataParser dataParser,
            ILogger<SubscribeCommandHandler> logger)
        {
            _botClient = botClient;
            _userRepository = userRepository;
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
                    text: "Please specify queue index. Example: /subscribe 1.1",
                    cancellationToken: cancellationToken);
                return;
            }

            var queueIndex = parts[1].Trim();
            _logger.LogInformation("User {UserId} subscribing to queue {QueueIndex}", chatId, queueIndex);

            try
            {
                // Check if queue exists
                if (!_dataParser.HasQueue(queueIndex))
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: $"❌ Queue {queueIndex} not found. Use /all to see available queues.",
                        cancellationToken: cancellationToken);
                    return;
                }

                // Ensure user exists
                var user = await _userRepository.GetUserByTelegramIdAsync(chatId);
                if (user == null)
                {
                    user = await _userRepository.CreateUserAsync(chatId);
                }

                // Subscribe to queue
                var success = await _userRepository.SubscribeToQueueAsync(chatId, queueIndex);

                if (success)
                {
                    var queue = _dataParser.GetQueue(queueIndex);
                    var message_text = $"✅ Successfully subscribed to queue *{queue.Index}*\n\n";

                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: message_text,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken);

                    try
                    {
                        await _botClient.SendQueuePhotoFromFile(queueIndex, chatId, cancellationToken: cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error getting queue {QueueIndex} for user {UserId}",
                            queueIndex, chatId);
                    }

                    _logger.LogInformation("User {UserId} successfully subscribed to queue {QueueIndex}",
                        chatId, queueIndex);
                }
                else
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "Failed to subscribe. You might already be subscribed or please try again later.",
                        cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing user {UserId} to queue {QueueIndex}",
                    chatId, queueIndex);
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "Sorry, an error occurred while subscribing. Please try again later.",
                    cancellationToken: cancellationToken);
            }
        }
    }
}
