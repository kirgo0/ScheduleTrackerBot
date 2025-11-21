using DisconnectionSchedule.Events;
using DisconnectionSchedule.Helper;
using DisconnectionSchedule.Models;
using DisconnectionSchedule.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace TelegramTemplateBot.Services
{
    public class TelegramNotificationService : BackgroundService, ITelegramNotificationService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IDisconnectionDataParser _dataParser;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<TelegramNotificationService> _logger;

        public TelegramNotificationService(
            ITelegramBotClient botClient,
            IDisconnectionDataParser dataParser,
            IUserRepository userRepository,
            ILogger<TelegramNotificationService> logger)
        {
            _botClient = botClient;
            _dataParser = dataParser;
            _userRepository = userRepository;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _dataParser.QueueUpdated += async (sender, args) =>
            {
                await HandleQueueUpdate(args);
            };

            return Task.CompletedTask;
        }

        public async Task HandleQueueUpdate(QueueUpdateEventArgs args)
        {
            try
            {
                _logger.LogInformation($"Processing update for queue {args.QueueIndex}");

                // Get all users subscribed to this queue
                var subscribedUsers = await _userRepository.GetUsersSubscribedToQueueAsync(args.QueueIndex);

                foreach (var user in subscribedUsers)
                {
                    try
                    {
                        await SendQueueUpdateToUserAsync(user.TelegramId, args.UpdatedData);
                        _logger.LogInformation($"Sent update to user {user.TelegramId} for queue {args.QueueIndex}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to send update to user {user.TelegramId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling queue update for {args.QueueIndex}");
            }
        }

        public async Task SendMessageToUserAsync(long telegramId, string message, ParseMode parseMode = ParseMode.Markdown)
        {
            try
            {
                await _botClient.SendTextMessageAsync(
                    chatId: telegramId,
                    text: message,
                    parseMode: parseMode,
                    disableNotification: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send message to user {telegramId}");
                throw;
            }
        }

        public async Task SendQueueUpdateToUserAsync(long telegramId, QueueData queue)
        {
            var message = QueueFormatter.FormatQueueUpdate(queue);
            await SendMessageToUserAsync(telegramId, message, ParseMode.Markdown);
        }
    }
}
