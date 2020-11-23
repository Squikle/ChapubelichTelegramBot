using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ChapubelichBot.Types.Entities.Messages
{
    class DelayedTextMessage : IDelayedMessage
    {
        private readonly SendMessageRequest _messageRequest;
        private readonly CancellationToken _cancellationToken;

        public DelayedTextMessage(ChatId chatId, string text, ParseMode parseMode = ParseMode.Default,
            bool disableWebPagePreview = false, bool disableNotification = false, int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default)
        {
            _messageRequest = new SendMessageRequest(chatId, text)
            {
                ParseMode = parseMode,
                DisableWebPagePreview = disableWebPagePreview,
                DisableNotification = disableNotification,
                ReplyToMessageId = replyToMessageId,
                ReplyMarkup = replyMarkup
            };
            _cancellationToken = cancellationToken;
        }

        public Task<Message> Send(ITelegramBotClient client)
        {
            return client.MakeRequestAsync(_messageRequest, _cancellationToken);
        }
    }
}
