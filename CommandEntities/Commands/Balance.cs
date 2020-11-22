using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = ChapubelichBot.Types.Entities.User;
using ChapubelichBot.Types.Abstractions.Commands;

namespace ChapubelichBot.CommandEntities.Commands
{
    class Balance : Command
    {
        public override string Name => "💰 Баланс";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            User user;
            await using (var db = new ChapubelichdbContext())
            {
                user = db.Users.FirstOrDefault(x => x.UserId == message.From.Id);
            }

            if (user != null)
                await client.TrySendTextMessageAsync(
                message.Chat.Id,
                $"<a href=\"tg://user?id={user.UserId}\">{message.From.FirstName}</a>, твой баланс: {user.Balance.ToMoneyFormat()} \U0001F4B0",
                replyToMessageId: message.MessageId,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}
