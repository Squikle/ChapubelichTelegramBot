using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;

namespace ChapubelichBot.Chatting.Commands
{
    class HelloCommand : Command
    {
        public override string Name => "/hello";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await client.TrySendTextMessageAsync(message.Chat.Id, "Привет!\U0001F604", replyToMessageId: message.MessageId);
        }
    }
}
