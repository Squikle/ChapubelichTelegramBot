﻿using ChapubelichBot.Database;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace ChapubelichBot.Types.Extensions
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
                    Console.WriteLine($"Не удалось отправить сообщение. ChatId: {chatId}\nОшибка: {e.Message}");
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
        public static async Task<Message> TryEditMessageAsync(this ITelegramBotClient client, ChatId chatId, int messageId, string text, ParseMode parseMode = ParseMode.Default, bool disableWebPagePreview = false, InlineKeyboardMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            Message message;
            try
            {
                message = await client.EditMessageTextAsync(chatId, messageId, text, parseMode, disableWebPagePreview, replyMarkup, cancellationToken);
            }
            catch (Exception e)
            {
                if (e is ApiRequestException)
                    Console.WriteLine($"Не удалось редактировать сообщение. ChatId: {chatId}, MessageId: {messageId} \nОшибка: {e.Message}");
                return null;
            }

            return message;
        }
        public static async Task<Message> TryEditMessageReplyMarkupAsync(this ITelegramBotClient client, ChatId chatId, int messageId, InlineKeyboardMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            Message message;
            try
            {
                message = await client.EditMessageReplyMarkupAsync(chatId, messageId, replyMarkup, cancellationToken);
            }
            catch (Exception e)
            {
                if (e is ApiRequestException)
                    Console.WriteLine($"Не удалось редактировать разметку сообщения. ChatId: {chatId}, MessageId: {messageId} \nОшибка: {e.Message}");
                return null;
            }

            return message;
        }
        public static async Task<Message> TrySendAnimationAsync(this ITelegramBotClient client, ChatId chatId, InputOnlineFile animation, int duration = 0, int width = 0, int height = 0, InputMedia thumb = null, string caption = null, ParseMode parseMode = ParseMode.Default, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            Message message;
            try
            {
                message = await client.SendAnimationAsync(chatId, animation, duration, width, height, thumb, caption, parseMode, disableNotification, replyToMessageId, replyMarkup, cancellationToken);
            }
            catch (Exception e)
            {
                if (e is ApiRequestException)
                    Console.WriteLine($"Не удалось отправить анимацию ChatId: {chatId}\nОшибка: {e.Message}");
                return null;
            }

            return message;
        }
        public static async Task<Message> TrySendPhotoAsync(this ITelegramBotClient client, ChatId chatId, InputOnlineFile photo, string caption = null, ParseMode parseMode = ParseMode.Default, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            Message message;
            try
            {
                message = await client.SendPhotoAsync(chatId, photo, caption, parseMode, disableNotification, replyToMessageId, replyMarkup, cancellationToken);
            }
            catch (Exception e)
            {
                if (e is ApiRequestException)
                    Console.WriteLine($"Не удалось отправить фото ChatId: {chatId}\nОшибка: {e.Message}");
                return null;
            }

            return message;
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
                    Console.WriteLine($"Не удалось удалить сообщение. ChatId: {chatId}\nОшибка: {e.Message}");
            }
        }
        public static async Task TryAnswerCallbackQueryAsync(this ITelegramBotClient client, string callbackQueryId, string text = null, bool showAlert = false, string url = null, int cacheTime = 0, CancellationToken cancellationToken = default)
        {
            try
            {
                await client.AnswerCallbackQueryAsync(callbackQueryId, text, showAlert, url, cacheTime, cancellationToken);
            }
            catch (Exception e)
            {
                if (e is ApiRequestException)
                    Console.WriteLine($"Не удалось удалить сообщение. callbackQueryId: {callbackQueryId}\nОшибка: {e.Message}");
            }
        }
        public static string ToMoneyFormat(this int moneySum)
        {
            return String.Format("{0:n0}", moneySum);
        }
        public static string ToMoneyFormat(this long moneySum)
        {
            return String.Format("{0:n0}", moneySum);
        }
    }
}