using System;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Entities.Users;
using ChapubelichBot.Types.Managers.MessagesSender;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = ChapubelichBot.Types.Entities.Users.User;

namespace ChapubelichBot.CommandEntities.Commands
{
    class EarnDailyReward : Command
    {
        public override string Name => "💵 Ежедневная награда";

        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            User user;
            int dailyRewardSum;

            await using (var dbContext = new ChapubelichdbContext())
            {
                user = await dbContext.Users.Include(u => u.DailyReward).FirstOrDefaultAsync(x => x.UserId == message.From.Id);
                if (user == null)
                    return;
                if (user.DailyReward == null)
                {
                    user.DailyReward = new UserDailyReward()
                    {
                        Stage = 0,
                        User = user,
                        Rewarded = false
                    };
                    try
                    {
                        await dbContext.SaveChangesAsync();
                    }
                    catch (DbUpdateException)
                    {

                        Console.WriteLine("Повторное добавление ежедневной награды пользователя");
                        return;
                    }
                }
                else if (user.DailyReward.Rewarded)
                {
                    await client.TrySendTextMessageAsync(
                            message.Chat.Id,
                            $"<i><a href=\"tg://user?id={user.UserId}\">{message.From.FirstName}</a></i>, ты уже забрал сегодняшнюю награду.\nПриходи завтра! 😉\n" +
                            $"{GetRewardsProgress(user.DailyReward.Stage)}",
                            replyToMessageId: message.MessageId,
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    return;
                }

                dailyRewardSum = GetDailyReward(user.DailyReward.Stage);

                user.Balance += dailyRewardSum;
                bool saved = false;
                while (!saved)
                {
                    user.DailyReward.Rewarded = true;
                    try
                    {
                        await dbContext.SaveChangesAsync();
                        saved = true;
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        foreach (var entry in ex.Entries)
                        {
                            if (entry.Entity is User entryUser)
                            {
                                Console.WriteLine("Конфликт параллелизма для баланса пользователя (EarnDailyReward)");
                                await entry.ReloadAsync();

                                entryUser.Balance += dailyRewardSum;

                                if (!entryUser.DailyReward.Rewarded)
                                    continue;
                                Console.WriteLine("Повторная попытка получения ежедневной награды пользователя");
                                return;
                            }
                            if (entry.Entity is UserDailyReward)
                            {
                                Console.WriteLine("Повторная попытка получения ежедневной награды пользователя");
                                return;
                            }
                        }
                    }
                }
            }

            await client.TrySendTextMessageAsync(
            message.Chat.Id,
            $"<i><a href=\"tg://user?id={user.UserId}\">{message.From.FirstName}</a></i>, {(user.DailyReward.Stage == 6 ? "Ура!🥳 Ты получил максимальную награду" : "ты получил")} <b>{dailyRewardSum}</b> 💵\n" +
            $"{GetRewardsProgress(user.DailyReward.Stage)}",
            replyToMessageId: message.MessageId,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }

        private int GetDailyReward(int stage)
        {
            if (stage == 6)
                return 500;

            int min = 100;
            int dailyAdition = 50;

            return min + stage * dailyAdition;
        }

        private string GetRewardsProgress(int stage)
        {
            return stage switch
            {
                0 => "🟢->⭕️->⭕️->⭕️->⭕️->⭕️->💎",
                1 => "🟢->🟢->⭕️->⭕️->⭕️->⭕️->💎",
                2 => "🟢->🟢->🟢->⭕️->⭕️->⭕️->💎",
                3 => "🟢->🟢->🟢->🟢->⭕️->⭕️->💎",
                4 => "🟢->🟢->🟢->🟢->🟢->⭕️->💎",
                5 => "🟢->🟢->🟢->🟢->🟢->🟢->💎",
                6 => "🟢->🟢->🟢->🟢->🟢->🟢->💎 ✅",
                _ => throw new Exception("Only 6 DailyReward stages allowed")
            };
        }
    }
}
