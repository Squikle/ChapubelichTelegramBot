using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Entities;
using ChapubelichBot.Types.Managers;
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
            await using (var dbContext = new ChapubelichdbContext())
            {
                gameSession = await RouletteGameManager.GetGameSessionOrNullAsync(message.Chat.Id, dbContext);
            }
            if (gameSession != null)
                await RouletteGameManager.BetNumbersRequestAsync(message, Pattern);
        }
    }
}
