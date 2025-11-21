using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramTemplateBot.Extensions
{
    public static class TelegramBotExtensions
    {
        public static async Task<Message> SendQueuePhotoFromFile(this ITelegramBotClient bot, string queueIndex, long chatId, string messageText = null, CancellationToken cancellationToken = default)
        {
            try
            {
                using var stream = System.IO.File.OpenRead($"output/{queueIndex}.png");

                return await bot.SendPhoto(
                    chatId: chatId,
                    photo: InputFile.FromStream(stream, Path.GetFileName($"{queueIndex}.png")),
                    caption: messageText,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
            catch (Exception ex)
            {
                await bot.SendMessage(
                    chatId: chatId,
                    text: "Sorry, an error occurred while fetching the queue. Please try again later.",
                    cancellationToken: cancellationToken);
                throw;
            }
        } 
    }
}
