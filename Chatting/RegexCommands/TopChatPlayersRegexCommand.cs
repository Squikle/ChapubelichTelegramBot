using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChapubelichBot.Database;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Group = ChapubelichBot.Database.Models.Group;
using User = ChapubelichBot.Database.Models.User;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class TopChatPlayersRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(топ|лучшие|best|top) *?(\d*)(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await using var db = new ChapubelichdbContext();

            Group group = db.Groups.Include(g => g.Users).FirstOrDefault(g => g.GroupId == message.Chat.Id);
            if (group == null)
                return;

            if (!int.TryParse(Regex.Match(message.Text, Pattern).Groups[2].Value, out int usersToOutput) || usersToOutput == 0)
                usersToOutput = 10;

            if (usersToOutput > group.Users.Count)
                usersToOutput = group.Users.Count;

            List<User> topUsers = group.Users.Take(usersToOutput).ToList();
            var topUsersNamed = topUsers.AsParallel().Select(async tu =>
            {
                ChatMember member = await client.GetChatMemberAsync(message.Chat.Id, tu.UserId);
                return new KeyValuePair<User, string>(tu, member?.User?.FirstName);
            }).ToDictionary(k => k.Result.Key, v => v.Result.Value);

            var orderedTopUsers = topUsersNamed.OrderByDescending(k => k.Key.Balance).ToList();

            StringBuilder answer = new StringBuilder();
            for (int i = 0; i < orderedTopUsers.Count; i++)
            {
                if (topUsersNamed.ElementAt(i).Value != null)
                    answer.Append($"{i + 1}. {topUsersNamed.ElementAt(i).Value} - {topUsersNamed.ElementAt(i).Key.Balance.ToMoneyFormat()} 💰\n");
            }

            await client.TrySendTextMessageAsync(message.Chat.Id, answer.ToString(),
                replyToMessageId: message.MessageId);
        }
    }
}
