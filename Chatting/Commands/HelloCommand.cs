using Chapubelich.Abstractions;
using Chapubelich.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Chapubelich.Chatting.Commands
{
    class HelloCommand : Command
    {
        public override string Name => "/hello";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(message.Chat.Id, "Yo!", replyToMessageId: message.MessageId);
        }
    }
}
