using DisconnectionSchedule.Events;
using DisconnectionSchedule.Helper;
using DisconnectionSchedule.Models;
using DisconnectionSchedule.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramTemplateBot.Extensions;

namespace TelegramTemplateBot.Services
{
    public class TelegramNotificationService : BackgroundService, ITelegramNotificationService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IDisconnectionDataParser _dataParser;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<TelegramNotificationService> _logger;
        private readonly IServiceScopeFactory _scope;

        public TelegramNotificationService(
            ITelegramBotClient botClient,
            IDisconnectionDataParser dataParser,
            IUserRepository userRepository,
            ILogger<TelegramNotificationService> logger,
            IServiceScopeFactory scope)
        {
            _botClient = botClient;
            _dataParser = dataParser;
            _userRepository = userRepository;
            _logger = logger;
            _scope = scope;
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

                using (var scope = _scope.CreateScope())
                {
                    var imageService = scope.ServiceProvider.GetRequiredService<IQueueImageService>();
                    var newImagePath = await imageService.GenerateImageAsync(args.UpdatedData, $"output/{args.QueueIndex}.png");
                }

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

        public async Task SendMessageToUserAsync(long telegramId, string message, string queueIndex, ParseMode parseMode = ParseMode.Markdown)
        {
            try
            {
                await _botClient.SendQueuePhotoFromFile(queueIndex, telegramId, message);
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
            await SendMessageToUserAsync(telegramId, message, queue.Index, ParseMode.Markdown);
        }
    }
}
