﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Entities.Messages;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace ChapubelichBot.Types.Managers
{
    static class MessageSenderManager
    {
        private static RelevantChats _relevantChats;
        private static Timer _timer;
        /*private static int _millisecondsInterval;
        public static int Interval
        {
            get => _millisecondsInterval;
            set
            {
                _millisecondsInterval = value;
                _timer.Change(value, value);
            }
        }*/

        public static int MaxMessagesPerSecond { get; set; }

        public static void Init(int millisecondsInterval, int maxMessagesPerSecond)
        {
            /*_millisecondsInterval = millisecondsInterval;
            _timer = new Timer(t =>
            {
                _available = true;
                _sendedMessages = 0;
            }, null, 0, millisecondsInterval);
            _available = true;*/
            MaxMessagesPerSecond = maxMessagesPerSecond;
            _relevantChats = new RelevantChats();
        }

        private static void MessageSended(ChatId chatId)
        {
            if (!_relevantChats.Contains(chatId))
                _relevantChats.Add(chatId);
            else 
                _relevantChats.Get(chatId).MessagesSended += 1;

            Console.WriteLine(_relevantChats.Get(chatId)?.MessagesSended);
        }

        private static bool? Muted => ChapubelichClient.GetConfig().GetValue<bool?>("AppSettings:Mute");
        public static async Task<Message> TrySendTextMessageAsync(this ITelegramBotClient client, ChatId chatId, string text, ParseMode parseMode = ParseMode.Default, bool disableWebPagePreview = false, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            Message message;
            try
            {
                while (!_relevantChats.CanSendMessageToChat(chatId))
                    Thread.Sleep(100);
                MessageSended(chatId);
                bool muted = Muted != null ? Muted == true : disableNotification;
                message = await client.SendTextMessageAsync(
                    chatId, text + " " + _relevantChats.Get(chatId).MessagesSended, parseMode,
                    disableWebPagePreview,
                    muted, replyToMessageId,
                    replyMarkup, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    $"Не удалось отправить сообщение. ChatId: {chatId}\nОшибка: {e.GetType()}\nСообщение ошибки: {e.Message}\nСтек вызовов:\n{e.StackTrace}");
                return null;
            }

            return message;
        }
        public static async Task<Message> TrySendStickerAsync(this ITelegramBotClient client, ChatId chatId, InputOnlineFile sticker, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            Message message;
            try
            {
                bool muted = Muted != null ? Muted == true : disableNotification;
                message = await client.SendStickerAsync(
                    chatId, sticker, muted,
                    replyToMessageId, replyMarkup, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Не удалось отправить стикер. ChatId: {chatId}\nОшибка: {e.GetType()}\nСообщение ошибки: {e.Message}\nСтек вызовов: {e.StackTrace}");
                return null;
            }

            return message;
        }
        public static async Task<Message> TrySendPhotoAsync(this ITelegramBotClient client, ChatId chatId, InputOnlineFile photo, string caption = null, ParseMode parseMode = ParseMode.Default, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            Message message;
            try
            {
                bool muted = Muted != null ? Muted == true : disableNotification;
                message = await client.SendPhotoAsync(chatId, photo, caption, parseMode, muted, replyToMessageId, replyMarkup, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Не удалось отправить фото ChatId: {chatId}\nОшибка: {e.GetType()}\nСообщение ошибки: {e.Message}\nСтек вызовов: {e.StackTrace}");
                return null;
            }

            return message;
        }
        public static async Task<Message> TrySendVideoAsync(this ITelegramBotClient client, ChatId chatId, InputOnlineFile video, int duration = 0, int width = 0, int height = 0, string caption = null, ParseMode parseMode = ParseMode.Default, bool supportsStreaming = false, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default, InputMedia thumb = null)
        {
            Message message;
            try
            {
                bool muted = Muted != null ? Muted == true : disableNotification;
                message = await client.SendVideoAsync(chatId, video, duration, width, height, caption, parseMode, supportsStreaming, muted, replyToMessageId, replyMarkup, cancellationToken, thumb);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Не удалось отправить видео ChatId: {chatId}\nОшибка: {e.GetType()}\nСообщение ошибки: {e.Message}\nСтек вызовов: {e.StackTrace}");
                return null;
            }

            return message;
        }
        public static async Task<Message> TrySendAudioAsync(this ITelegramBotClient client, ChatId chatId, InputOnlineFile audio, string caption = null, ParseMode parseMode = ParseMode.Default, int duration = 0, string performer = null, string title = null, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default, InputMedia thumb = null)
        {
            Message message;
            try
            {
                bool muted = Muted != null ? Muted == true : disableNotification;
                message = await client.SendAudioAsync(chatId, audio, caption, parseMode, duration, performer, title, muted, replyToMessageId, replyMarkup, cancellationToken, thumb);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Не удалось отправить аудио ChatId: {chatId}\nОшибка: {e.GetType()}\nСообщение ошибки: {e.Message}\nСтек вызовов: {e.StackTrace}");
                return null;
            }

            return message;
        }
        public static async Task<Message> TrySendAnimationAsync(this ITelegramBotClient client, ChatId chatId, InputOnlineFile animation, int duration = 0, int width = 0, int height = 0, InputMedia thumb = null, string caption = null, ParseMode parseMode = ParseMode.Default, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            Message message;
            try
            {
                bool muted = Muted != null ? Muted == true : disableNotification;
                message = await client.SendAnimationAsync(chatId, animation, duration, width, height, thumb, caption, parseMode, muted, replyToMessageId, replyMarkup, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Не удалось отправить анимацию ChatId: {chatId}\nОшибка: {e.GetType()}\nСообщение ошибки: {e.Message}\nСтек вызовов: {e.StackTrace}");
                return null;
            }

            return message;
        }
        public static async Task<Message> TrySendPollAsync(this ITelegramBotClient client, ChatId chatId, string question, IEnumerable<string> options, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default, bool? isAnonymous = null, PollType? type = null, bool? allowsMultipleAnswers = null, int? correctOptionId = null, bool? isClosed = null, string explanation = null, ParseMode explanationParseMode = ParseMode.Default, int? openPeriod = null, DateTime? closeDate = null)
        {
            Message message;
            try
            {
                bool muted = Muted != null ? Muted == true : disableNotification;
                message = await client.SendPollAsync(chatId, question, options, muted, replyToMessageId, replyMarkup, cancellationToken, isAnonymous, type, allowsMultipleAnswers, correctOptionId, isClosed, explanation, explanationParseMode, openPeriod, closeDate);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Не удалось отправить голосование ChatId: {chatId}\nОшибка: {e.GetType()}\nСообщение ошибки: {e.Message}\nСтек вызовов: {e.StackTrace}");
                return null;
            }

            return message;
        }
        public static async Task<Message> TryEditMessageAsync(this ITelegramBotClient client, ChatId chatId, int messageId, string text, ParseMode parseMode = ParseMode.Default, bool disableWebPagePreview = false, InlineKeyboardMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            Message message;
            try
            {
                message = await client.EditMessageTextAsync(chatId, messageId, text, parseMode, disableWebPagePreview,
                    replyMarkup, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    $"Не удалось редактировать сообщение. ChatId: {chatId}, MessageId: {messageId}\nОшибка: {e.GetType()}\nСообщение ошибки: {e.Message}\nСтек вызовов: {e.StackTrace}");
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
                Console.WriteLine($"Не удалось редактировать разметку сообщения. ChatId: {chatId}, MessageId: {messageId}\nОшибка: {e.GetType()}\nСообщение ошибки: {e.Message}\nСтек вызовов: {e.StackTrace}");
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
                Console.WriteLine($"Не удалось удалить сообщение. ChatId: {chatId}\nОшибка: {e.GetType()}\nСообщение ошибки: {e.Message}\nСтек вызовов: {e.StackTrace}");
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
                Console.WriteLine($"Не удалось удалить сообщение. callbackQueryId: {callbackQueryId}\nОшибка: {e.GetType()}\nСообщение ошибки: {e.Message}\nСтек вызовов: {e.StackTrace}");
            }
        }
    }
}
