using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Entities;
using ChapubelichBot.Types.GameManagers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.RegexCommands.Roulette
{
    class BetNumberRegex : RegexCommand
    {
        public override string Pattern => @"^\/? *(\d+)(?: +(?:([0-9]|[1-3][0-9])(?: *- *([0-9]|[1-3][0-9]))?)| *(чет|четное|нечет|нечетное)) *(го|ролл|погнали|крути|roll|go)?(@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            RouletteGameSession gameSession;
            await using (var db = new ChapubelichdbContext())
            {
                gameSession = RouletteGameManager.GetGameSessionOrNull(message.Chat.Id, db);
            }
            if (gameSession != null)
                await RouletteGameManager.BetNumbersRequest(message, Pattern);
        }
    }
}
