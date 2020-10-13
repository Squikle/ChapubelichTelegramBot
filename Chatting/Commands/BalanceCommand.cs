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
            {
                var user = db.Users.FirstOrDefault(x => x.UserId == message.From.Id);

                if (user != null)
                    await client.TrySendTextMessageAsync(
                    message.Chat.Id,
                    $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, Ваш баланс: {user.Balance.ToMoneyFormat()} \U0001F4B0",
                    replyToMessageId: message.MessageId,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }
    }
}
