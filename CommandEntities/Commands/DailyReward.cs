using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.Extensions.Configuration;
using User = ChapubelichBot.Types.Entities.User;

namespace ChapubelichBot.CommandEntities.Commands
{
    class DailyReward : Command
    {
        public override string Name => "💵 Ежедневная награда";

        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            User user;
            int configDailyReward = ChapubelichClient.GetConfig().GetValue<int>("AppSettings:DailyReward");
            int totalDailyReward = configDailyReward >= 1000 ? 1000 : configDailyReward;

            await using (var db = new ChapubelichdbContext())
            {
                user = db.Users.FirstOrDefault(x => x.UserId == message.From.Id);
                if (user == null)
                    return;
                if (user.DailyRewarded)
                {
                    await client.TrySendTextMessageAsync(
                            message.Chat.Id,
                            $"<a href=\"tg://user?id={user.UserId}\">{message.From.FirstName}</a>, ты уже получил ежедневную награду. Забери новую награду завтра😉",
                            replyToMessageId: message.MessageId,
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    return;
                }

                user.Balance += totalDailyReward;
                user.DailyRewarded = true;

                db.SaveChanges();
            }

            await client.TrySendTextMessageAsync(
            message.Chat.Id,
            $"<a href=\"tg://user?id={user.UserId}\">{message.From.FirstName}</a>, ты получил {totalDailyReward} 💵",
            replyToMessageId: message.MessageId,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}
