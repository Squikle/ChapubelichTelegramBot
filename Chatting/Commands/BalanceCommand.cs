using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Database;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.Commands
{
    class BalanceCommand : Command
    {
        public override string Name => "\U0001F4B0 Баланс";

        public override async void Execute(Message message, ITelegramBotClient client)
        {
            using (var db = new ChapubelichdbContext())
                await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    $"Ваш баланс: {db.Users.First(x => x.UserId == message.From.Id).Balance} \U0001F4B0",
                    replyToMessageId: message.MessageId);
        }
    }
}
