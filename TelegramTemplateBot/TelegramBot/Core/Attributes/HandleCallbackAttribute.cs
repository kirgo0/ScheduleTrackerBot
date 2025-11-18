namespace ScheduleTrackingBot.TelegramBot.Core.Attributes
{
    /// <summary>
    /// Marks a service as a callback query handler
    /// </summary>
    public class HandleCallbackAttribute : TelegramHandlerAttribute
    {
        public HandleCallbackAttribute(string callbackData) : base(callbackData)
        {
        }
    }

}
