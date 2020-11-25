using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Managers;
using ChapubelichBot.Types.Managers.MessagesSender;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Group = ChapubelichBot.Types.Entities.Group;
using User = ChapubelichBot.Types.Entities.User;

namespace ChapubelichBot.CommandEntities.RegexCommands
{
    class TopChatBalanceRegex : RegexCommand
    {
        public override string Pattern => @"^\/? *(топ|лучшие|best|top) *?(\d*)(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await using ChapubelichdbContext dbContext = new ChapubelichdbContext();

            Group group = await dbContext.Groups.Include(g => g.Users).FirstOrDefaultAsync(g => g.GroupId == message.Chat.Id);
            if (group == null)
                return;

            if (!int.TryParse(Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase).Groups[2].Value, out int usersToOutput) || usersToOutput == 0)
                usersToOutput = 10;

            int maxTopChatOutput = ChapubelichClient.GetConfig().GetValue<int>("AppSettings:MaxTopChatOutput");
            if (usersToOutput > maxTopChatOutput)
                usersToOutput = maxTopChatOutput;
            if (usersToOutput > group.Users.Count)
                usersToOutput = group.Users.Count;

            List<KeyValuePair<User, string>> topUsersNamed = group.Users
                .OrderByDescending(u => u.Balance)
                .Take(usersToOutput)
                .AsParallel()
                .Select(tu =>
            {
                ChatMember member = client.GetChatMemberAsync(message.Chat.Id, tu.UserId).Result;
                return new KeyValuePair<User, string>(tu, member?.User?.FirstName);
            })
                .OrderByDescending(p => p.Key.Balance)
                .ToList();

            List<long> topThreeBalances = topUsersNamed.Select(tu => tu.Key.Balance).TakeTopValues(3).ToList();
            StringBuilder answer = new StringBuilder($"💰 Топ <b>{topUsersNamed.Count}</b> богатеев чата 💰\n");
            for (int i = 0; i < topUsersNamed.Count; i++)
            {
                if (topUsersNamed.ElementAt(i).Value == null)
                    continue;

                var currUser = topUsersNamed.ElementAt(i);

                answer.Append($"<b>{i + 1}.</b> <i>{currUser.Value}</i> - <b>{currUser.Key.Balance.ToMoneyFormat()}</b>");

                if (currUser.Key.Balance > topThreeBalances.ElementAtOrDefault(3) || currUser.Key.Balance > 0)
                {
                    if (currUser.Key.Balance == topThreeBalances.ElementAt(0))
                        answer.Append("🥇");
                    else if (currUser.Key.Balance == topThreeBalances.ElementAt(1))
                        answer.Append("🥈");
                    else if (currUser.Key.Balance == topThreeBalances.ElementAt(2))
                        answer.Append("🥉");
                }
                answer.Append("\n");
            }

            await client.TrySendTextMessageAsync(message.Chat.Id, answer.ToString(),
                replyToMessageId: message.MessageId, 
                parseMode: ParseMode.Html);
        }
    }
}
