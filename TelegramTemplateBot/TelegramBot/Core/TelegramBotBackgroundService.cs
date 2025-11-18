using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace ScheduleTrackingBot.TelegramBot.Core
{
    /// <summary>
    /// Background service that runs the Telegram bot
    /// </summary>
    public class TelegramBotBackgroundService : BackgroundService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TelegramBotBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public TelegramBotBackgroundService(
            ITelegramBotClient botClient,
            ILogger<TelegramBotBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _botClient = botClient;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Telegram bot service is starting");

            try
            {
                var me = await _botClient.GetMe(stoppingToken);
                _logger.LogInformation("Bot started: @{BotUsername} ({BotName})", me.Username, me.FirstName);

                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = Array.Empty<Telegram.Bot.Types.Enums.UpdateType>(), // Receive all update types
                    DropPendingUpdates = true // Drop pending updates on start
                };

                // Create handler delegates
                async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken ct)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var updateHandler = scope.ServiceProvider.GetRequiredService<IBotUpdateHandler>();
                    await updateHandler.HandleUpdateAsync(update, ct);
                }

                async Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken ct)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var updateHandler = scope.ServiceProvider.GetRequiredService<IBotUpdateHandler>();
                    await updateHandler.HandleErrorAsync(exception, ct);
                }

                // Start receiving updates
                await _botClient.ReceiveAsync(
                    HandleUpdateAsync,
                    HandleErrorAsync,
                    receiverOptions,
                    stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running the bot");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Telegram bot service is stopping");
            await base.StopAsync(cancellationToken);
        }
    }

}
