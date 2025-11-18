using Microsoft.Extensions.DependencyInjection;
using ScheduleTrackingBot.TelegramBot.Core.Attributes;
using ScheduleTrackingBot.TelegramBot.Core.Handlers;
using System.Reflection;

namespace ScheduleTrackingBot.TelegramBot.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Automatically registers all Telegram handler services marked with handler attributes
        /// </summary>
        public static IServiceCollection RegisterTelegramHandlers(this IServiceCollection services)
        {
            var registry = new HandlerRegistry();

            var assembly = Assembly.GetExecutingAssembly();
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(ITelegramHandler).IsAssignableFrom(t))
                .ToList();

            foreach (var handlerType in handlerTypes)
            {
                // Register the service as scoped
                services.AddScoped(handlerType);

                // Process HandleCommand attributes
                var commandAttributes = handlerType.GetCustomAttributes<HandleCommandAttribute>();
                foreach (var attr in commandAttributes)
                {
                    registry.CommandHandlers[attr.Pattern] = handlerType;
                }

                // Process HandleCallback attributes
                var callbackAttributes = handlerType.GetCustomAttributes<HandleCallbackAttribute>();
                foreach (var attr in callbackAttributes)
                {
                    registry.CallbackHandlers[attr.Pattern] = handlerType;
                }

                // Process HandleText attributes
                var textAttributes = handlerType.GetCustomAttributes<HandleTextAttribute>();
                foreach (var attr in textAttributes)
                {
                    registry.TextHandlers[attr.Pattern] = handlerType;
                }

                // Process HandleSticker attribute
                if (handlerType.GetCustomAttribute<HandleStickerAttribute>() != null)
                {
                    registry.StickerHandler = handlerType;
                }

                // Process HandlePhoto attribute
                if (handlerType.GetCustomAttribute<HandlePhotoAttribute>() != null)
                {
                    registry.PhotoHandler = handlerType;
                }

                // Process HandleVideo attribute
                if (handlerType.GetCustomAttribute<HandleVideoAttribute>() != null)
                {
                    registry.VideoHandler = handlerType;
                }

                // Process HandleDocument attribute
                if (handlerType.GetCustomAttribute<HandleDocumentAttribute>() != null)
                {
                    registry.DocumentHandler = handlerType;
                }

                // Process HandleVoice attribute
                if (handlerType.GetCustomAttribute<HandleVoiceAttribute>() != null)
                {
                    registry.VoiceHandler = handlerType;
                }

                // Process HandleAudio attribute
                if (handlerType.GetCustomAttribute<HandleAudioAttribute>() != null)
                {
                    registry.AudioHandler = handlerType;
                }

                // Process HandleLocation attribute
                if (handlerType.GetCustomAttribute<HandleLocationAttribute>() != null)
                {
                    registry.LocationHandler = handlerType;
                }

                // Process HandleContact attribute
                if (handlerType.GetCustomAttribute<HandleContactAttribute>() != null)
                {
                    registry.ContactHandler = handlerType;
                }

                // Process HandleInlineQuery attributes
                var inlineQueryAttributes = handlerType.GetCustomAttributes<HandleInlineQueryAttribute>();
                foreach (var attr in inlineQueryAttributes)
                {
                    registry.InlineQueryHandlers[attr.Pattern] = handlerType;
                }

                // Process HandleChosenInlineResult attribute
                if (handlerType.GetCustomAttribute<HandleChosenInlineResultAttribute>() != null)
                {
                    registry.ChosenInlineResultHandler = handlerType;
                }

                // Process HandlePoll attribute
                if (handlerType.GetCustomAttribute<HandlePollAttribute>() != null)
                {
                    registry.PollHandler = handlerType;
                }

                // Process HandlePollAnswer attribute
                if (handlerType.GetCustomAttribute<HandlePollAnswerAttribute>() != null)
                {
                    registry.PollAnswerHandler = handlerType;
                }
            }

            // Register the registry as singleton
            services.AddSingleton(registry);

            return services;
        }
    }

}
