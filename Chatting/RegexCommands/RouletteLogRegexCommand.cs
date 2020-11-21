using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChapubelichBot.Database;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Group = ChapubelichBot.Database.Models.Group;
using User = ChapubelichBot.Database.Models.User;

namespace ChapubelichBot.Chatting.RegexCommands
{
    class RouletteLogRegexCommand : RegexCommand
    {
        public override string Pattern => @"^\/? *(log|лог|история|игры|последние) *?(\d*)(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            StringBuilder answer = new StringBuilder(35);
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

            if (!int.TryParse(Regex.Match(message.Text, Pattern).Groups[2].Value, out int gameSessionsToOutput) 
                || gameSessionsToOutput == 0 
                || gameSessionsToOutput > 10)
                gameSessionsToOutput = 10;

            if (gameSessionsToOutput > lastGameSessions.Length)
                gameSessionsToOutput = lastGameSessions.Length;

            for (int i = lastGameSessions.Length - gameSessionsToOutput; i < lastGameSessions.Length; i++)
            {
                /*string gameSessionResultString = gameSessionResult.ToString();
                if (gameSessionResultString.Length < 2)
                    gameSessionResultString += "  ";
                answer.Append($"{gameSessionResultString} {gameSessionResult.ToRouletteColor().ToEmoji()}\n");*/
                answer.Append($"{lastGameSessions[i].ToRouletteColor().ToEmoji()} {lastGameSessions[i]}\n");
            }

            await client.TrySendTextMessageAsync(message.Chat.Id, answer.ToString(),
                replyToMessageId: message.MessageId);
        }
    }
}
