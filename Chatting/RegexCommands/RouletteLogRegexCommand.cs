using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChapubelichBot.Database;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = ChapubelichBot.Database.Models.User;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteLogRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(log|лог|история|игры|последние)(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            await using var db = new ChapubelichdbContext();
            int[] lastGameSessions = null;
            if (message.Chat.Type == ChatType.Private)
            {
                User user = db.Users.FirstOrDefault(x => x.UserId == message.From.Id);
                if (user == null)
                    return;
                lastGameSessions = user.LastGameSessions.ToArray();
            }
            else
            {
                Group group = db.Groups.FirstOrDefault(x => x.GroupId == message.Chat.Id);
                if (group == null)
                    return;
                lastGameSessions = group.LastGameSessions.ToArray();
            }

            StringBuilder answer = new StringBuilder(25);
            foreach (var gameSessionResult in lastGameSessions)
            {
                answer.Append($"{gameSessionResult} {gameSessionResult.ToRouletteColor()}\n");
            }

            await client.TrySendTextMessageAsync(message.Chat.Id, answer.ToString(),
                replyToMessageId: message.MessageId);
        }
    }
}
