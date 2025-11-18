namespace ScheduleTrackingBot.TelegramBot.Core
{
    /// <summary>
    /// Registry for storing handler type mappings
    /// </summary>
    public class HandlerRegistry
    {
        public Dictionary<string, Type> CommandHandlers { get; } = new();
        public Dictionary<string, Type> CallbackHandlers { get; } = new();
        public Dictionary<string, Type> TextHandlers { get; } = new();
        public Type? StickerHandler { get; set; }
        public Type? PhotoHandler { get; set; }
        public Type? VideoHandler { get; set; }
        public Type? DocumentHandler { get; set; }
        public Type? VoiceHandler { get; set; }
        public Type? AudioHandler { get; set; }
        public Type? LocationHandler { get; set; }
        public Type? ContactHandler { get; set; }
        public Dictionary<string, Type> InlineQueryHandlers { get; } = new();
        public Type? ChosenInlineResultHandler { get; set; }
        public Type? PollHandler { get; set; }
        public Type? PollAnswerHandler { get; set; }
    }
}
