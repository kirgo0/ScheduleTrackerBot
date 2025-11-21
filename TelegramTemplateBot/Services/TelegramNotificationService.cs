using DisconnectionSchedule.Events;
using DisconnectionSchedule.Helper;
using DisconnectionSchedule.Models;
using DisconnectionSchedule.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramTemplateBot.Extensions;

namespace TelegramTemplateBot.Services
{
    public class TelegramNotificationService : BackgroundService, ITelegramNotificationService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IDisconnectionDataParser _dataParser;
        private readonly IUserRepository _userRepository;
        private readonly IQueueRepository _queueRepository;
        private readonly ILogger<TelegramNotificationService> _logger;
        private readonly IServiceScopeFactory _scope;
        private bool _isInitialized = false;

        public TelegramNotificationService(
            ITelegramBotClient botClient,
            IDisconnectionDataParser dataParser,
            IUserRepository userRepository,
            IQueueRepository queueRepository,
            ILogger<TelegramNotificationService> logger,
            IServiceScopeFactory scope)
        {
            _botClient = botClient;
            _dataParser = dataParser;
            _userRepository = userRepository;
            _queueRepository = queueRepository;
            _logger = logger;
            _scope = scope;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Load existing queue data from database before starting to process updates
                await InitializeQueueDataAsync();

                _dataParser.QueueUpdated += async (sender, args) =>
                {
                    await HandleQueueUpdate(args);
                };

                _logger.LogInformation("TelegramNotificationService initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize TelegramNotificationService");
            }
        }

        private async Task InitializeQueueDataAsync()
        {
            try
            {
                _logger.LogInformation("Initializing queue data from database...");

                var existingQueues = await _queueRepository.GetAllQueuesAsync();

                _logger.LogInformation($"Loaded {existingQueues.Count} existing queues from database");

                foreach (var kvp in existingQueues)
                {
                    _dataParser.InitializeQueue(kvp.Key, kvp.Value);
                    _logger.LogInformation($"Initialized queue {kvp.Key} with existing data");
                }

                _isInitialized = true;
                _logger.LogInformation("Queue data initialization completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing queue data");
                _isInitialized = true; // Continue anyway to avoid blocking the service
            }
        }

        public async Task HandleQueueUpdate(QueueUpdateEventArgs args)
        {
            try
            {
                _logger.LogInformation($"Processing update for queue {args.QueueIndex}");

                await _queueRepository.UpsertQueueAsync(args.UpdatedData);

                if (!_isInitialized)
                {
                    _logger.LogInformation($"Skipping notifications for queue {args.QueueIndex} - no changes detected or still initializing");
                    return;
                }

                // Generate image for the updated queue
                using (var scope = _scope.CreateScope())
                {
                    var imageService = scope.ServiceProvider.GetRequiredService<IQueueImageService>();
                    var newImagePath = await imageService.GenerateImageAsync(args.UpdatedData, $"output/{args.QueueIndex}.png");
                }

                // Send notifications to subscribed users
                var subscribedUsers = await _userRepository.GetUsersSubscribedToQueueAsync(args.QueueIndex);

                _logger.LogInformation($"Sending notifications to {subscribedUsers.Count()} users for queue {args.QueueIndex}");

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