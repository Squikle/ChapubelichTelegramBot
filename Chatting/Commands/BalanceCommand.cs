using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Database;
using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.Commands
{
    class BalanceCommand : Command
    {
        public override string Name => "💰 Баланс";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            ChapubelichBot.Database.Models.User user;
            await using (var db = new ChapubelichdbContext())
            {
                user = db.Users.FirstOrDefault(x => x.UserId == message.From.Id);
            }

            if (user != null)
                await client.TrySendTextMessageAsync(
                message.Chat.Id,
                $"<a href=\"tg://user?id={user.UserId}\">{user.FirstName}</a>, твой баланс: {user.Balance.ToMoneyFormat()} \U0001F4B0",
                replyToMessageId: message.MessageId,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}
