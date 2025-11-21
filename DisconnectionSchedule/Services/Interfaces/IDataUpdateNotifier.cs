using DisconnectionSchedule.Models;

namespace DisconnectionSchedule.Services.Interfaces
{
    public interface IDataUpdateNotifier
    {
        Action Subscribe(Func<string, Task> callback);
        void Unsubscribe(Action subscription);
        Task NotifyUpdate(string updatedContent);
    }
}
