namespace ScheduleTrackingBot.TelegramBot.Core.Attributes
{
    /// <summary>
    /// Base attribute for Telegram update handlers
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class TelegramHandlerAttribute : Attribute
    {
        public string Pattern { get; }

        protected TelegramHandlerAttribute(string pattern)
        {
            Pattern = pattern;
        }
    }
}
