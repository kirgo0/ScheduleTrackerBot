using DisconnectionSchedule.Services;
using Microsoft.Extensions.Logging;

namespace TelegramTemplateBot.Services
{
    public class ScheduleUpdateSubscriber
    {
        private readonly ILogger<ScheduleUpdateSubscriber> _logger;
        private readonly IDataUpdateNotifier _notifier;
        private Action? _unsubscribe;

        public ScheduleUpdateSubscriber(ILogger<ScheduleUpdateSubscriber> logger, IDataUpdateNotifier notifier)
        {
            _logger = logger;
            _notifier = notifier;
        }

        public void Start()
        {
            _unsubscribe = _notifier.Subscribe(OnDataUpdate);
            _logger.LogInformation("ExampleSubscriber started and subscribed to updates");
        }

        public void Stop()
        {
            _unsubscribe?.Invoke();
            _logger.LogInformation("ExampleSubscriber stopped and unsubscribed");
        }

        private async Task OnDataUpdate(string newContent)
        {
            _logger.LogInformation("ExampleSubscriber received update. Content length: {Length}",
                newContent?.Length ?? 0);

            await Task.CompletedTask;
        }
    }
}
