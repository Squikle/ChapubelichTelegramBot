using ChapubelichBot.Types.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChapubelichBot.Types.Games.RouletteGame;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.CallbackMessages
{
    class RouletteBetColorCallbackMessage : CallBackMessage
    {
        public override List<string> IncludingData => new List<string> { "rouletteBetRed", "rouletteBetBlack", "rouletteBetGreen" };
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            var gameSession = RouletteGameManager.GetGameSessionOrNull(query.Message.Chat.Id);
            if (gameSession != null)
                await RouletteGameManager.BetColorRequest(query);
        }
    }
}
