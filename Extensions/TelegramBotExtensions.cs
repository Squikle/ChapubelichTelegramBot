using Chapubelich.Database;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chapubelich.Extensions
{
    static class TelegramBotExtensions
    {
        public static async Task<Message> TrySendTextMessageAsync(this ITelegramBotClient client, ChatId chatId, string text, ParseMode parseMode = ParseMode.Default, bool disableWebPagePreview = false, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            using (var db = new ChapubelichdbContext())
            {
                var receiverUser = db.Users.FirstOrDefault(x => x.UserId == chatId.Identifier);
                Message message = null;
                try
                {
                    message = await client.SendTextMessageAsync(
                        chatId, text, parseMode,
                        disableWebPagePreview,
                        disableNotification, replyToMessageId,
                        replyMarkup, cancellationToken);
                }
                catch (Exception e)
                {
                    if (e is ApiRequestException)
                    {
                        if (null != receiverUser && receiverUser.IsAvailable)
                        {
                            receiverUser.IsAvailable = false;
                            db.SaveChanges();
                        }
                    }
                    Console.WriteLine($"Не удалось отправить сообщение. ChatId: {chatId}\nОшибка: {e.GetType()}");
                    return null;
                }

                if (null != receiverUser && !receiverUser.IsAvailable)
                {
                    receiverUser.IsAvailable = true;
                    db.SaveChanges();
                }
                return message;
            }
        }
        public static async Task TryDeleteMessageAsync(this ITelegramBotClient client, ChatId chatId, int messageId, CancellationToken cancellationToken = default)
        {
            try
            {
                await client.DeleteMessageAsync(chatId, messageId, cancellationToken);
            }
            catch (Exception e)
            {
                if (e is ApiRequestException)
                    Console.WriteLine($"Не удалось удалить сообщение. ChatId: {chatId}\nОшибка: {e.GetType()}");
            }
        }
        public static async Task<Message> TryEditMessageAsync(this ITelegramBotClient client, ChatId chatId, int messageId, string text, ParseMode parseMode = ParseMode.Default, bool disableWebPagePreview = false, InlineKeyboardMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            Message message = null;
            try
            {
                message = await client.EditMessageTextAsync(chatId, messageId, text, parseMode, disableWebPagePreview, replyMarkup, cancellationToken);
            }
            catch (Exception e)
            {
                if (e is ApiRequestException)
                    Console.WriteLine($"Не удалось редактировать сообщение. ChatId: {chatId}, MessageId: {messageId} \nОшибка: {e.GetType()}");
                return null;
            }

            return message;
        }
        public static async Task<Message> TryEditMessageReplyMarkupAsync(this ITelegramBotClient client, ChatId chatId, int messageId, InlineKeyboardMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            Message message = null;
            try
            {
                message = await client.EditMessageReplyMarkupAsync(chatId, messageId, replyMarkup, cancellationToken);
            }
            catch (Exception e)
            {
                if (e is ApiRequestException)
                    Console.WriteLine($"Не удалось редактировать разметку сообщения. ChatId: {chatId}, MessageId: {messageId} \nОшибка: {e.GetType()}");
                return null;
            }

            return message;
        }
    }
}
