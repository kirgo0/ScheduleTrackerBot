
namespace DisconnectionSchedule.Services
{
    public interface IDataUpdateNotifier
    {
        Action Subscribe(Func<string, Task> callback);
        void Unsubscribe(Action subscription);

        Task NotifyUpdate(string updatedContent);
    }
}
