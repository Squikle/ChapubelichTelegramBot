﻿using ChapubelichBot.Database;
using ChapubelichBot.Init;
using ChapubelichBot.Types.Abstractions;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Statics;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.Commands
{
    class DailyRewardCommand : Command
    {
        public override string Name => "💵 Ежедневная награда";

        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            ChapubelichBot.Database.Models.User user;
            int configDailyReward = Bot.GetConfig().GetValue<int>("AppSettings:DailyReward");
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

                await db.SaveChangesAsync();
            }

            await client.TrySendTextMessageAsync(
            message.Chat.Id,
            $"<a href=\"tg://user?id={user.UserId}\">{message.From.FirstName}</a>, ты получил {totalDailyReward} 💵",
            replyToMessageId: message.MessageId,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}
