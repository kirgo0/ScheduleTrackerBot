namespace ScheduleTrackingBot.TelegramBot.Core.Attributes
{
    /// <summary>
    /// Marks a service as a command handler
    /// </summary>
    public class HandleCommandAttribute : TelegramHandlerAttribute
    {
        public HandleCommandAttribute(string command) : base(command)
        {
        }
    }
}
