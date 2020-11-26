using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Managers.MessagesSender.Limiters;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace ChapubelichBot.Types.Managers.MessagesSender
{
    static class MessageSenderManager
    {
        private static int TryDelay => 250;
        private static ChatLimiter _chatLimiter;
        private static GlobalLimiter _globalLimiter;

        public static void Init(GlobalLimiter globalLimiter, ChatLimiter chatLimiter)
        {
            _chatLimiter = chatLimiter;
            _globalLimiter = globalLimiter;
        }
        public static void Terminate()
        {_globalLimiter.Dispose();
        }

        private static void RequestCreated(ChatId chatId)
        {
            _chatLimiter.RequestCreated(chatId);
            _globalLimiter.RequestCreated();
        }
        private static bool AvailableToSend(ChatId chatId)
        {
            return _chatLimiter.IsAvailableToSend(chatId) && _globalLimiter.IsAvailableToSend();
        }

        private static bool? Muted => ChapubelichClient.GetConfig().GetValue<bool?>("AppSettings:Mute");
        public static async Task<Message> TrySendTextMessageAsync(this ITelegramBotClient client, ChatId chatId, string text, ParseMode parseMode = ParseMode.Default, bool disableWebPagePreview = false, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            bool muted = Muted != null ? Muted == true : disableNotification;
            Message message = null;
            while (!AvailableToSend(chatId))
                await Task.Delay(TryDelay);
            RequestCreated(chatId);
            try
            {
                message = await client.SendTextMessageAsync(
                    chatId, text, parseMode,
                    disableWebPagePreview,
                    muted, replyToMessageId,
                    replyMarkup, cancellationToken);
            }
            catch (ApiRequestException e)
            {
                if (e.Message.Equals("Bad Request: reply message not found") && replyToMessageId != 0)
                {
                    Console.WriteLine(
                        $"Не удалось отправить текстовое сообщение. ChatId: {chatId}\nОшибка: сообщение для ответа не найдено. Попробую отправить еще раз без ответа...");
                    while (!AvailableToSend(chatId))
                        await Task.Delay(TryDelay);
                    RequestCreated(chatId);
                    message = await client.SendTextMessageAsync(
                        chatId, text, parseMode,
                        disableWebPagePreview,
                        muted, 0,
                        replyMarkup, cancellationToken);
                }
                else Console.WriteLine($"Не удалось отправить текстовое сообщение. ChatId: {chatId}\nОшибка: {e.Message} - Тип: {e.GetType()}");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(
                    e.Message.Equals("Response status code does not indicate success: 429 (Too Many Requests)")
                        ? $"Не удалось отправить текстовое сообщение: {chatId}\nОшибка: Слишком много запросов"
                        : $"Не удалось отправить текстовое сообщение: {chatId}\nОшибка: {e.GetType()} - Тип: {e.Message}");
            }

            return message;
        }
        public static async Task<Message> TryForwardMessageAsync(this ITelegramBotClient client, ChatId chatId, ChatId fromChatId, int messageId, bool disableNotification = false, CancellationToken cancellationToken = default)
        {
            bool muted = Muted != null ? Muted == true : disableNotification;
            Message message = default;
            while (!AvailableToSend(chatId))
                await Task.Delay(TryDelay);
            try
            {
                message = await client.ForwardMessageAsync(
                    chatId, fromChatId, messageId,
                    muted, cancellationToken);
            }
            catch (ApiRequestException e)
            {
                Console.WriteLine($"Не удалось переслать сообщение. ChatId: {chatId} - FromChatId: {fromChatId}\nОшибка: {e.Message} - Тип: {e.GetType()}");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(
                    e.Message.Equals("Response status code does not indicate success: 429 (Too Many Requests)")
                        ? $"Не удалось переслать сообщение: {chatId}\nОшибка: Слишком много запросов"
                        : $"Не удалось переслать сообщение: {chatId}\nОшибка: {e.GetType()} - Тип: {e.Message}");
            }
            return message;
        }
        public static async Task<Message> TrySendStickerAsync(this ITelegramBotClient client, ChatId chatId, InputOnlineFile sticker, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            bool muted = Muted != null ? Muted == true : disableNotification;
            Message message = default;
            while (!AvailableToSend(chatId))
                await Task.Delay(TryDelay);
            RequestCreated(chatId);
            try
            {
                message = await client.SendStickerAsync(
                    chatId, sticker, muted,
                    replyToMessageId, replyMarkup, cancellationToken);
            }
            catch (ApiRequestException e)
            {
                if (e.Message.Equals("Bad Request: reply message not found") && replyToMessageId != 0)
                {
                    Console.WriteLine(
                        $"Не удалось отправить стикер. ChatId: {chatId}\nОшибка: сообщение для ответа не найдено. Попробую отправить еще раз без ответа...");
                    while (!AvailableToSend(chatId))
                        await Task.Delay(TryDelay);
                    RequestCreated(chatId);
                    message = await client.SendStickerAsync(
                        chatId, sticker, muted,
                        0, replyMarkup, cancellationToken);
                }
                else Console.WriteLine($"Не удалось отправить стикер. ChatId: {chatId}\nОшибка: {e.Message} - Тип: {e.GetType()}");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(
                    e.Message.Equals("Response status code does not indicate success: 429 (Too Many Requests)")
                        ? $"Не удалось отправить стикер: {chatId}\nОшибка: Слишком много запросов"
                        : $"Не удалось отправить стикер: {chatId}\nОшибка: {e.GetType()} - Тип: {e.Message}");
            }

            return message;
        }
        public static async Task<Message> TrySendPhotoAsync(this ITelegramBotClient client, ChatId chatId, InputOnlineFile photo, string caption = null, ParseMode parseMode = ParseMode.Default, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            bool muted = Muted != null ? Muted == true : disableNotification;
            Message message = default;
            while (!AvailableToSend(chatId))
                await Task.Delay(TryDelay);
            RequestCreated(chatId);
            try
            {
                message = await client.SendPhotoAsync(chatId, photo, caption, parseMode, muted, replyToMessageId, replyMarkup, cancellationToken);
            }
            catch (ApiRequestException e)
            {
                if (e.Message.Equals("Bad Request: reply message not found") && replyToMessageId != 0)
                {
                    Console.WriteLine(
                        $"Не удалось отправить картинку. ChatId: {chatId}\nОшибка: сообщение для ответа не найдено. Попробую отправить еще раз без ответа...");
                    while (!AvailableToSend(chatId))
                        await Task.Delay(TryDelay);
                    RequestCreated(chatId);
                    message = await client.SendPhotoAsync(chatId, photo, caption, parseMode, muted, 0, replyMarkup, cancellationToken);
                }
                else Console.WriteLine($"Не удалось отправить картинку. ChatId: {chatId}\nОшибка: {e.Message} - Тип: {e.GetType()}");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(
                    e.Message.Equals("Response status code does not indicate success: 429 (Too Many Requests)")
                        ? $"Не удалось отправить картинку: {chatId}\nОшибка: Слишком много запросов"
                        : $"Не удалось отправить картинку: {chatId}\nОшибка: {e.GetType()} - Тип: {e.Message}");
            }

            return message;
        }
        public static async Task<Message> TrySendVideoAsync(this ITelegramBotClient client, ChatId chatId, InputOnlineFile video, int duration = 0, int width = 0, int height = 0, string caption = null, ParseMode parseMode = ParseMode.Default, bool supportsStreaming = false, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default, InputMedia thumb = null)
        {
            bool muted = Muted != null ? Muted == true : disableNotification;
            Message message = default;
            while (!AvailableToSend(chatId))
                await Task.Delay(TryDelay);
            RequestCreated(chatId);
            try
            {
                message = await client.SendVideoAsync(chatId, video, duration, width, height, caption, parseMode, 
                    supportsStreaming, muted, replyToMessageId, replyMarkup, cancellationToken, thumb);
            }
            catch (ApiRequestException e)
            {
                if (e.Message.Equals("Bad Request: reply message not found") && replyToMessageId != 0)
                {
                    Console.WriteLine(
                        $"Не удалось отправить видео. ChatId: {chatId}\nОшибка: сообщение для ответа не найдено. Попробую отправить еще раз без ответа...");
                    while (!AvailableToSend(chatId))
                        await Task.Delay(TryDelay);
                    RequestCreated(chatId);
                    message = await client.SendVideoAsync(chatId, video, duration, width, height, caption, parseMode, 
                        supportsStreaming, muted, 0, replyMarkup, cancellationToken, thumb);
                }
                else Console.WriteLine($"Не удалось отправить видео. ChatId: {chatId}\nОшибка: {e.Message} - Тип: {e.GetType()}");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(
                    e.Message.Equals("Response status code does not indicate success: 429 (Too Many Requests)")
                        ? $"Не удалось отправить видео: {chatId}\nОшибка: Слишком много запросов"
                        : $"Не удалось отправить видео: {chatId}\nОшибка: {e.GetType()} - Тип: {e.Message}");
            }

            return message;
        }
        public static async Task<Message> TrySendAudioAsync(this ITelegramBotClient client, ChatId chatId, InputOnlineFile audio, string caption = null, ParseMode parseMode = ParseMode.Default, int duration = 0, string performer = null, string title = null, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default, InputMedia thumb = null)
        {
            bool muted = Muted != null ? Muted == true : disableNotification;
            Message message = default;
            while (!AvailableToSend(chatId))
                await Task.Delay(TryDelay);
            RequestCreated(chatId);
            try
            {
                message = await client.SendAudioAsync(chatId, audio, caption, parseMode, 
                    duration, performer, title, muted, replyToMessageId, replyMarkup, cancellationToken, thumb);
            }
            catch (ApiRequestException e)
            {
                if (e.Message.Equals("Bad Request: reply message not found") && replyToMessageId != 0)
                {
                    Console.WriteLine(
                        $"Не удалось отправить аудио. ChatId: {chatId}\nОшибка: сообщение для ответа не найдено. Попробую отправить еще раз без ответа...");
                    while (!AvailableToSend(chatId))
                        await Task.Delay(TryDelay);
                    RequestCreated(chatId);
                    message = await client.SendAudioAsync(chatId, audio, caption, parseMode, 
                        duration, performer, title, muted, 0, replyMarkup, cancellationToken, thumb);
                }
                else Console.WriteLine($"Не удалось отправить аудио. ChatId: {chatId}\nОшибка: {e.Message} - Тип: {e.GetType()}");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(
                    e.Message.Equals("Response status code does not indicate success: 429 (Too Many Requests)")
                        ? $"Не удалось отправить аудио: {chatId}\nОшибка: Слишком много запросов"
                        : $"Не удалось отправить аудио: {chatId}\nОшибка: {e.GetType()} - Тип: {e.Message}");
            }
            return message;
        }
        public static async Task<Message> TrySendAnimationAsync(this ITelegramBotClient client, ChatId chatId, InputOnlineFile animation, int duration = 0, int width = 0, int height = 0, InputMedia thumb = null, string caption = null, ParseMode parseMode = ParseMode.Default, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            bool muted = Muted != null ? Muted == true : disableNotification;
            Message message = default;
            while (!AvailableToSend(chatId))
                await Task.Delay(TryDelay);
            RequestCreated(chatId);
            try
            {
                message = await client.SendAnimationAsync(chatId, animation, duration, width, height, thumb, caption, parseMode, 
                    muted, replyToMessageId, replyMarkup, cancellationToken);
            }
            catch (ApiRequestException e)
            {
                if (e.Message.Equals("Bad Request: reply message not found") && replyToMessageId != 0)
                {
                    Console.WriteLine(
                        $"Не удалось отправить анимацию. ChatId: {chatId}\nОшибка: сообщение для ответа не найдено. Попробую отправить еще раз без ответа...");
                    while (!AvailableToSend(chatId))
                        await Task.Delay(TryDelay);
                    RequestCreated(chatId);
                    message = await client.SendAnimationAsync(chatId, animation, duration, width, height, thumb, caption, parseMode,
                        muted, 0, replyMarkup, cancellationToken);
                }
                else Console.WriteLine($"Не удалось отправить анимацию. ChatId: {chatId}\nОшибка: {e.Message} - Тип: {e.GetType()}");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(
                    e.Message.Equals("Response status code does not indicate success: 429 (Too Many Requests)")
                        ? $"Не удалось отправить анимацию: {chatId}\nОшибка: Слишком много запросов"
                        : $"Не удалось отправить анимацию: {chatId}\nОшибка: {e.GetType()} - Тип: {e.Message}");
            }

            return message;
        }
        public static async Task<Message> TrySendPollAsync(this ITelegramBotClient client, ChatId chatId, string question, IEnumerable<string> options, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default, bool? isAnonymous = null, PollType? type = null, bool? allowsMultipleAnswers = null, int? correctOptionId = null, bool? isClosed = null, string explanation = null, ParseMode explanationParseMode = ParseMode.Default, int? openPeriod = null, DateTime? closeDate = null)
        {
            bool muted = Muted != null ? Muted == true : disableNotification;
            Message message = default;
            while (!AvailableToSend(chatId))
                await Task.Delay(TryDelay);
            RequestCreated(chatId);
            try
            {
                message = await client.SendPollAsync(chatId, question, options, muted, replyToMessageId, replyMarkup, 
                    cancellationToken, isAnonymous, type, allowsMultipleAnswers, correctOptionId, isClosed, 
                    explanation, explanationParseMode, openPeriod, closeDate);
            }
            catch (ApiRequestException e)
            {
                if (e.Message.Equals("Bad Request: reply message not found") && replyToMessageId != 0)
                {
                    Console.WriteLine(
                        $"Не удалось отправить голосование. ChatId: {chatId}\nОшибка: сообщение для ответа не найдено. Попробую отправить еще раз без ответа...");
                    while (!AvailableToSend(chatId))
                        await Task.Delay(TryDelay);
                    RequestCreated(chatId);
                    message = await client.SendPollAsync(chatId, question, options, muted, 0, replyMarkup, 
                        cancellationToken, isAnonymous, type, allowsMultipleAnswers, correctOptionId, isClosed, 
                        explanation, explanationParseMode, openPeriod, closeDate);
                }
                else Console.WriteLine($"Не удалось отправить голосование. ChatId: {chatId}\nОшибка: {e.Message} - Тип: {e.GetType()}");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(
                    e.Message.Equals("Response status code does not indicate success: 429 (Too Many Requests)")
                        ? $"Не удалось отправить голосование: {chatId}\nОшибка: Слишком много запросов"
                        : $"Не удалось отправить голосование: {chatId}\nОшибка: {e.GetType()} - Тип: {e.Message}");
            }

            return message;
        }
        public static async Task<Message> TryEditMessageAsync(this ITelegramBotClient client, ChatId chatId, int messageId, string text, ParseMode parseMode = ParseMode.Default, bool disableWebPagePreview = false, InlineKeyboardMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            while (!AvailableToSend(chatId))
                await Task.Delay(TryDelay);
            RequestCreated(chatId);
            Message message = default;
            try
            {
                message = await client.EditMessageTextAsync(chatId, messageId, text, parseMode, disableWebPagePreview,
                    replyMarkup, cancellationToken);
            }
            catch (ApiRequestException e)
            {
                Console.WriteLine(
                    $"Не удалось редактировать сообщение. ChatId: {chatId}, MessageId: {messageId}\nОшибка: {e.GetType()}\nСообщение ошибки: {e.Message}");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(
                    e.Message.Equals("Response status code does not indicate success: 429 (Too Many Requests).")
                        ? $"Не удалось редактировать разметку сообщения: {chatId}\nОшибка: Слишком много запросов"
                        : $"Не удалось редактировать разметку сообщения: {chatId}\nОшибка: {e.Message} - Тип: {e.GetType()}");
            }

            return message;
        }
        public static async Task<Message> TryEditMessageReplyMarkupAsync(this ITelegramBotClient client, ChatId chatId, int messageId, InlineKeyboardMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            Message message = default;
            while (!AvailableToSend(chatId))
                await Task.Delay(TryDelay);
            RequestCreated(chatId);
            try
            {
                message = await client.EditMessageReplyMarkupAsync(chatId, messageId, replyMarkup, cancellationToken);
            }
            catch (ApiRequestException e)
            {
                //if (e.Message.Equals("Bad Request: message can't be edited"))
                //Console.WriteLine($"Не удалось редактировать разметку сообщения. ChatId: {chatId}\nОшибка: сообщение для редактирования не найдено");
                Console.WriteLine($"Не удалось редактировать разметку сообщения: ChatId: {chatId}\nОшибка: {e.GetType()}\nСообщение ошибки: {e.Message}");
            }
            catch (HttpRequestException e)
            {
                    Console.WriteLine(
                        e.Message.Equals("Response status code does not indicate success: 429 (Too Many Requests).")
                        ? $"Не удалось редактировать разметку сообщения: {chatId}\nОшибка: Слишком много запросов"
                        : $"Не удалось редактировать разметку сообщения: {chatId}\nОшибка: {e.Message} - Тип: {e.GetType()}");
            }

            return message;
        }
        public static async Task TryDeleteMessageAsync(this ITelegramBotClient client, ChatId chatId, int messageId, CancellationToken cancellationToken = default)
        {
            while (!AvailableToSend(chatId))
                await Task.Delay(TryDelay);
            RequestCreated(chatId);
            try
            {
                await client.DeleteMessageAsync(chatId, messageId, cancellationToken);
            }
            catch (ApiRequestException e)
            {
                Console.WriteLine($"Не удалось удалить сообщение. ChatId: {chatId}\nОшибка: {e.GetType()}\nСообщение ошибки: {e.Message}");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(
                    e.Message.Equals("Response status code does not indicate success: 429 (Too Many Requests).")
                        ? $"Не удалось удалить сообщение: {chatId}\nОшибка: Слишком много запросов"
                        : $"Не удалось удалить сообщение: {chatId}\nОшибка: {e.Message} - Тип: {e.GetType()}");
            }
        }
        public static async Task TryAnswerCallbackQueryAsync(this ITelegramBotClient client, string callbackQueryId, string text = null, bool showAlert = false, string url = null, int cacheTime = 0, CancellationToken cancellationToken = default)
        {
            try
            {
                await client.AnswerCallbackQueryAsync(callbackQueryId, text, showAlert, url, cacheTime, cancellationToken);
            }
            catch (ApiRequestException e)
            {
                Console.WriteLine($"Не удалось ответить на callback. callbackQueryId: {callbackQueryId}\nОшибка: {e.GetType()}\nСообщение ошибки: {e.Message}");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(
                    e.Message.Equals("Response status code does not indicate success: 429 (Too Many Requests).")
                        ? $"Не удалось ответить на callback: {callbackQueryId}\nОшибка: Слишком много запросов"
                        : $"Не удалось ответить на callback: {callbackQueryId}\nОшибка: {e.Message} - Тип: {e.GetType()}");
            }
        }
    }
}
