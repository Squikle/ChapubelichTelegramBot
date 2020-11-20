using ChapubelichBot.Types.Abstractions;
using System.Threading.Tasks;
using System.Collections.Generic;
using ChapubelichBot.Database;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Games.RouletteGame;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.CallbackMessages
{
    class RouletteBetNumbersCallbackMessage : CallBackMessage
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
            await using (var db = new ChapubelichdbContext())
            {
                gameSession = RouletteGameManager.GetGameSessionOrNull(query.Message.Chat.Id, db);
            }
            if (gameSession != null)
                await RouletteGameManager.BetNumbersRequest(query);
        }
    }
}
