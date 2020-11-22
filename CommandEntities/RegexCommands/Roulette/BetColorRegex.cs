using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Entities;
using ChapubelichBot.Types.GameManagers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.CommandEntities.RegexCommands.Roulette
{
    class BetColorRegex : RegexCommand
    {
        public override string Pattern => @"^\/?(\d+) *(к(?:расный)?|ч(?:ерный)?|з(?:еленый)?|r(?:ed)?|b(?:lack)?|g(?:reen)?) *(го|г|ролл|погнали|крути|roll|go)?(?:@ChapubelichBot)?$";
        public override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            RouletteGameSession gameSession;
            await using (var db = new ChapubelichdbContext())
            {
                gameSession = RouletteGameManager.GetGameSessionOrNull(message.Chat.Id, db);
            }
            if (gameSession != null)
                await RouletteGameManager.BetColorRequest(message, Pattern);
        }
    }
}
