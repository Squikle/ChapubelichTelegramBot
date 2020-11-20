using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChapubelichBot.Database;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
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
            StringBuilder answer = new StringBuilder(25);
            await using var db = new ChapubelichdbContext();
            int[] lastGameSessions;
            if (message.Chat.Type == ChatType.Private)
            {
                User user = db.Users.FirstOrDefault(x => x.UserId == message.From.Id);
                if (user == null)
                    return;
                if (user.LastGameSessions != null)
                    lastGameSessions = user.LastGameSessions.ToArray();
                else 
                {
                    await client.TrySendTextMessageAsync(message.Chat.Id, "История игр пуста😞",
                    replyToMessageId: message.MessageId);
                    return;
                }
            }
            else
            {
                Group group = db.Groups.FirstOrDefault(x => x.GroupId == message.Chat.Id);
                if (group == null)
                    return;
                if (group.LastGameSessions != null)
                    lastGameSessions = group.LastGameSessions.ToArray();
                else
                {
                    await client.TrySendTextMessageAsync(message.Chat.Id, "История игр пуста😞",
                        replyToMessageId: message.MessageId); 
                    return;
                }
            }

            foreach (var gameSessionResult in lastGameSessions)
            {
                /*string gameSessionResultString = gameSessionResult.ToString();
                if (gameSessionResultString.Length < 2)
                    gameSessionResultString += "  ";
                answer.Append($"{gameSessionResultString} {gameSessionResult.ToRouletteColor().ToEmoji()}\n");*/
                answer.Append($"{gameSessionResult.ToRouletteColor().ToEmoji()} {gameSessionResult}\n");
            }

            await client.TrySendTextMessageAsync(message.Chat.Id, answer.ToString(),
                replyToMessageId: message.MessageId);
        }
    }
}
