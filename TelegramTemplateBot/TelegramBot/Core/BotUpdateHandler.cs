using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScheduleTrackingBot.TelegramBot.Core.Handlers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ScheduleTrackingBot.TelegramBot.Core
{
    /// <summary>
    /// Main bot update handler that routes updates to appropriate service handlers
    /// </summary>
    public class BotUpdateHandler : IBotUpdateHandler
    {
        private readonly ILogger<BotUpdateHandler> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly HandlerRegistry _registry;

        public BotUpdateHandler(
            ILogger<BotUpdateHandler> logger,
            IServiceProvider serviceProvider,
            HandlerRegistry registry)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _registry = registry;
        }

        public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received update {UpdateId} of type {UpdateType}",
                update.Id, update.Type);

            try
            {
                var handler = update.Type switch
                {
                    UpdateType.Message => HandleMessageAsync(update, cancellationToken),
                    UpdateType.CallbackQuery => HandleCallbackQueryAsync(update, cancellationToken),
                    UpdateType.InlineQuery => HandleInlineQueryAsync(update, cancellationToken),
                    UpdateType.ChosenInlineResult => HandleChosenInlineResultAsync(update, cancellationToken),
                    UpdateType.Poll => HandlePollAsync(update, cancellationToken),
                    UpdateType.PollAnswer => HandlePollAnswerAsync(update, cancellationToken),
                    _ => Task.CompletedTask
                };

                await handler;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, cancellationToken);
            }
        }

        private async Task HandleMessageAsync(Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            if (message == null) return;

            Type? handlerType = null;

            // Check for command
            if (message.Text?.StartsWith("/") == true)
            {
                var command = message.Text.Split(' ')[0].Split('@')[0]; // Remove bot username if present
                if (_registry.CommandHandlers.TryGetValue(command, out handlerType))
                {
                    _logger.LogDebug("Routing command {Command} to handler {HandlerType}",
                        command, handlerType.Name);
                }
            }
            // Check for text message
            else if (!string.IsNullOrEmpty(message.Text))
            {
                foreach (var (pattern, type) in _registry.TextHandlers)
                {
                    if (message.Text.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        handlerType = type;
                        _logger.LogDebug("Routing text message to handler {HandlerType}",
                            handlerType.Name);
                        break;
                    }
                }
            }
            // Check for sticker
            else if (message.Sticker != null)
            {
                handlerType = _registry.StickerHandler;
                _logger.LogDebug("Routing sticker to handler");
            }
            // Check for photo
            else if (message.Photo?.Length > 0)
            {
                handlerType = _registry.PhotoHandler;
                _logger.LogDebug("Routing photo to handler");
            }
            // Check for video
            else if (message.Video != null)
            {
                handlerType = _registry.VideoHandler;
                _logger.LogDebug("Routing video to handler");
            }
            // Check for document
            else if (message.Document != null)
            {
                handlerType = _registry.DocumentHandler;
                _logger.LogDebug("Routing document to handler");
            }
            // Check for voice
            else if (message.Voice != null)
            {
                handlerType = _registry.VoiceHandler;
                _logger.LogDebug("Routing voice to handler");
            }
            // Check for audio
            else if (message.Audio != null)
            {
                handlerType = _registry.AudioHandler;
                _logger.LogDebug("Routing audio to handler");
            }
            // Check for location
            else if (message.Location != null)
            {
                handlerType = _registry.LocationHandler;
                _logger.LogDebug("Routing location to handler");
            }
            // Check for contact
            else if (message.Contact != null)
            {
                handlerType = _registry.ContactHandler;
                _logger.LogDebug("Routing contact to handler");
            }

            if (handlerType != null)
            {
                await ExecuteHandlerAsync(handlerType, update, cancellationToken);
            }
            else
            {
                _logger.LogWarning("No handler found for message type");
            }
        }

        private async Task HandleCallbackQueryAsync(Update update, CancellationToken cancellationToken)
        {
            var callbackQuery = update.CallbackQuery;
            if (callbackQuery?.Data == null) return;

            if (_registry.CallbackHandlers.TryGetValue(callbackQuery.Data, out var handlerType))
            {
                _logger.LogDebug("Routing callback {CallbackData} to handler {HandlerType}",
                    callbackQuery.Data, handlerType.Name);
                await ExecuteHandlerAsync(handlerType, update, cancellationToken);
            }
            else
            {
                _logger.LogWarning("No handler found for callback data: {CallbackData}",
                    callbackQuery.Data);
            }
        }

        private async Task HandleInlineQueryAsync(Update update, CancellationToken cancellationToken)
        {
            var inlineQuery = update.InlineQuery;
            if (inlineQuery == null) return;

            Type? handlerType = null;

            // Try to find a matching pattern handler
            foreach (var (pattern, type) in _registry.InlineQueryHandlers)
            {
                if (string.IsNullOrEmpty(pattern) ||
                    inlineQuery.Query.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    handlerType = type;
                    break;
                }
            }

            if (handlerType != null)
            {
                _logger.LogDebug("Routing inline query to handler {HandlerType}", handlerType.Name);
                await ExecuteHandlerAsync(handlerType, update, cancellationToken);
            }
            else
            {
                _logger.LogWarning("No handler found for inline query");
            }
        }

        private async Task HandleChosenInlineResultAsync(Update update, CancellationToken cancellationToken)
        {
            if (_registry.ChosenInlineResultHandler != null)
            {
                _logger.LogDebug("Routing chosen inline result to handler");
                await ExecuteHandlerAsync(_registry.ChosenInlineResultHandler, update, cancellationToken);
            }
        }

        private async Task HandlePollAsync(Update update, CancellationToken cancellationToken)
        {
            if (_registry.PollHandler != null)
            {
                _logger.LogDebug("Routing poll to handler");
                await ExecuteHandlerAsync(_registry.PollHandler, update, cancellationToken);
            }
        }

        private async Task HandlePollAnswerAsync(Update update, CancellationToken cancellationToken)
        {
            if (_registry.PollAnswerHandler != null)
            {
                _logger.LogDebug("Routing poll answer to handler");
                await ExecuteHandlerAsync(_registry.PollAnswerHandler, update, cancellationToken);
            }
        }

        private async Task ExecuteHandlerAsync(Type handlerType, Update update, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService(handlerType) as ITelegramHandler;

            if (handler != null)
            {
                await handler.HandleAsync(update, cancellationToken);
            }
            else
            {
                _logger.LogError("Failed to create handler instance for type {HandlerType}",
                    handlerType.Name);
            }
        }

        public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Error occurred while handling update");
            return Task.CompletedTask;
        }
    }

}
