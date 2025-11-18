namespace ScheduleTrackingBot.TelegramBot.Core.Attributes
{
    /// <summary>
    /// Marks a service as an inline query handler
    /// </summary>
    public class HandleInlineQueryAttribute : TelegramHandlerAttribute
    {
        public HandleInlineQueryAttribute(string pattern = "") : base(pattern)
        {
        }
    }
}
