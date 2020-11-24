using System;
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

namespace ChapubelichBot.Types.Managers.MessagesSender
{
    static class MessageSenderManager
    {

        private static readonly int _globalMessagesPerInterval = 30;
        private static readonly int _globalMessagesInterval = 1000;

#pragma warning disable IDE0052, IDE0044
        private static Timer _timer;
        private static object _locker = new object();
#pragma warning restore IDE0052, IDE0044

        private static RelevantChats _relevantChats;
        private static int _globalMessagesCounter;
        private static bool _globalAvailable;

        public static void Init(int chatLimitMessagesPerMinute, int chatLimitMessagesPerSecond)
        {
            _relevantChats = new RelevantChats(chatLimitMessagesPerMinute, chatLimitMessagesPerSecond);
            _globalAvailable = true;
            _timer = new Timer(t =>
            {
                lock (_locker)   
                {
                    _globalAvailable = true;
                    _globalMessagesCounter = 0;
                }
            }, null, 0, _globalMessagesInterval);
        }
        public static void Terminate()
        {
            _timer.Dispose();
        }

        private static void MessageSended(ChatId chatId)
        {
            lock (_locker)
            {
                _relevantChats.MessageSended(chatId);
                if (_globalMessagesCounter >= _globalMessagesPerInterval)
                    _globalAvailable = false;
                else
                    _globalMessagesCounter++;
            }
        }
        private static bool AvailableToSend(ChatId chatId)
        {
            lock (_locker)
            {
                return _relevantChats.AvailableToSend(chatId) && _globalAvailable;
            }
        }

        private static bool? Muted => ChapubelichClient.GetConfig().GetValue<bool?>("AppSettings:Mute");
        public static async Task<Message> TrySendTextMessageAsync(this ITelegramBotClient client, ChatId chatId, string text, ParseMode parseMode = ParseMode.Default, bool disableWebPagePreview = false, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            Message message;
            while (!AvailableToSend(chatId))
                await Task.Delay(250);
            MessageSended(chatId);
            bool muted = Muted != null ? Muted == true : disableNotification;
            try
            {
                message = await client.SendTextMessageAsync(
                    chatId, text, parseMode,
                    disableWebPagePreview,
                    muted, replyToMessageId,
                    replyMarkup, cancellationToken);
            }
            catch (Exception e)
            {
                // TODO ограничить повторные отправления
                /*if (replyToMessageId != 0)
                {
                    Console.WriteLine(
                        "Не удалось отправить сообщение с ответом. Попробую еще раз без ответа");
                    message = await client.SendTextMessageAsync(
                        chatId, text, parseMode,
                        disableWebPagePreview,
                        muted, 0,
                        replyMarkup, cancellationToken);
                    return message;
                }*/

                Console.WriteLine(
                    $"Не удалось отправить сообщение. ChatId: {chatId}\nОшибка: {e.GetType()}\nСообщение ошибки: {e.Message}Стек вызовов: {e.StackTrace}");

                return null;
            }

            return message;
        }
        public static async Task<Message> TryForwardMessageAsync(this ITelegramBotClient client, ChatId chatId, ChatId fromChatId, int messageId, bool disableNotification = false, CancellationToken cancellationToken = default)
        {
            Message message;
            try
            {
                bool muted = Muted != null ? Muted == true : disableNotification;
                message = await client.ForwardMessageAsync(
                    chatId, fromChatId, messageId,
                    muted, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Не удалось переслать сообщение. ChatId: {chatId}\nОшибка: {e.Message}\nСтек вызовов: {e.StackTrace}");
                return null;
            }

            return message;
        }
        public static async Task<Message> TrySendStickerAsync(this ITelegramBotClient client, ChatId chatId, InputOnlineFile sticker, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            Message message;
            while (!AvailableToSend(chatId))
                await Task.Delay(250);
            MessageSended(chatId);
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
            while (!AvailableToSend(chatId))
                await Task.Delay(250);
            MessageSended(chatId);
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
            while (!AvailableToSend(chatId))
                await Task.Delay(250);
            MessageSended(chatId);
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
            while (!AvailableToSend(chatId))
                await Task.Delay(250);
            MessageSended(chatId);
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
            while (!AvailableToSend(chatId))
                await Task.Delay(250);
            MessageSended(chatId);
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
            while (!AvailableToSend(chatId))
                await Task.Delay(250);
            MessageSended(chatId);
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
            while (!AvailableToSend(chatId))
                await Task.Delay(250);
            MessageSended(chatId);
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
            while (!AvailableToSend(chatId))
                await Task.Delay(250);
            MessageSended(chatId);
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
