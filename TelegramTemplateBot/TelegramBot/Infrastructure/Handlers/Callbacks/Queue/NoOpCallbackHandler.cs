using ScheduleTrackingBot.TelegramBot.Core.Attributes;
using ScheduleTrackingBot.TelegramBot.Core.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramTemplateBot.TelegramBot.Infrastructure.Handlers.Callbacks.Queue
{
    [HandleCallback("noop")]
    public class NoOpCallbackHandler : ITelegramHandler
    {
        private readonly ITelegramBotClient _botClient;

        public NoOpCallbackHandler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
        {
            var callback = update.CallbackQuery;
            if (callback == null) return;

            await _botClient.AnswerCallbackQuery(callback.Id, cancellationToken: cancellationToken);
        }
    }
}
