using System.Collections.Generic;
using System.Threading.Tasks;
using ChapubelichBot.Main.Chapubelich;
using ChapubelichBot.Types.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using ChapubelichBot.Types.Abstractions.Commands;
using ChapubelichBot.Types.Managers;

namespace ChapubelichBot.CommandEntities.CallbackCommands.Roulette
{
    class BetNumbersCallback : CallbackCommand
    {
        public override List<string> IncludingData => new List<string> { 
            "rouletteBetEven", "rouletteBetOdd", 
            "rouletteBetFirstHalf", "rouletteBetSecondHalf",
             "rouletteBetFirstTwelve", "rouletteBetSecondTwelve", "rouletteBetThirdTwelve",
             "rouletteBetFirstRow", "rouletteBetSecondRow", "rouletteBetThirdRow"
        };
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            RouletteGameSession gameSession = null;
            await using (var dbContext = new ChapubelichdbContext())
            {
                gameSession = await RouletteGameManager.GetGameSessionOrNullAsync(query.Message.Chat.Id, dbContext);
            }
            if (gameSession != null)
                await RouletteGameManager.BetNumbersRequestAsync(query);
        }
    }
}
