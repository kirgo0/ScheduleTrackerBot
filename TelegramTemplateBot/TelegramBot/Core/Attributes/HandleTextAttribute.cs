namespace ScheduleTrackingBot.TelegramBot.Core.Attributes
{
    /// <summary>
    /// Marks a service as a text message handler
    /// </summary>
    public class HandleTextAttribute : TelegramHandlerAttribute
    {
        public HandleTextAttribute(string pattern) : base(pattern)
        {
        }
    }
}
