using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Managers;
using ChapubelichBot.Types.Managers.MessagesSender;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Group = ChapubelichBot.Types.Entities.Group;
using User = ChapubelichBot.Types.Entities.User;

namespace ChapubelichBot.CommandEntities.RegexCommands.Roulette
{
    class RouletteLogRegex : RegexCommand
    {
        public override string Pattern => @"^\/? *(log|лог|история|игры|последние) *?(\d*)(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
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
                    await client.TrySendTextMessageAsync(message.Chat.Id, "История игр пуста 😞",
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
                    await client.TrySendTextMessageAsync(message.Chat.Id, "История игр пуста 😞",
                        replyToMessageId: message.MessageId); 
                    return;
                }
            }

            if (!int.TryParse(Regex.Match(message.Text, Pattern, RegexOptions.IgnoreCase).Groups[2].Value, out int gameSessionsToOutput) 
                || gameSessionsToOutput == 0 
                || gameSessionsToOutput > 10)
                gameSessionsToOutput = 10;

            if (gameSessionsToOutput > lastGameSessions.Length)
                gameSessionsToOutput = lastGameSessions.Length;

            StringBuilder answer = new StringBuilder($"Результат последних <b>{gameSessionsToOutput}</b> игр:\n", 60);
            for (int i = lastGameSessions.Length - gameSessionsToOutput; i < lastGameSessions.Length; i++)
            {
                answer.Append($"{lastGameSessions[i].ToRouletteColor().ToEmoji()} <i>{lastGameSessions[i]}</i>\n");
            }

            await client.TrySendTextMessageAsync(message.Chat.Id, answer.ToString(),
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.Html);
        }
    }
}
