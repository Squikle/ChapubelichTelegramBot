using ChapubelichBot.Types.Abstractions;
using System.Threading.Tasks;
using System.Collections.Generic;
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
            var gameSession = RouletteGameManager.GetGameSessionOrNull(query.Message.Chat.Id);
            if (gameSession != null)
                await RouletteGameManager.BetNumbersRequest(query);
        }
    }
}
