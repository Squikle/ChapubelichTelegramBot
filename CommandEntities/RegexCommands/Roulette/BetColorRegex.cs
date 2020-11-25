using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Entities;
using ChapubelichBot.Types.Managers;
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
            await using (var dbContext = new ChapubelichdbContext())
            {
                gameSession = await RouletteGameManager.GetGameSessionOrNullAsync(message.Chat.Id, dbContext);
            }
            if (gameSession != null)
                await RouletteGameManager.BetColorRequestAsync(message, Pattern);
        }
    }
}
