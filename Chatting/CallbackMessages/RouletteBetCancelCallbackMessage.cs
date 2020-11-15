using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Statics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.CallbackMessages
{
    class RouletteBetCancelCallbackMessage : CallBackMessage
    {
        public override List<string> IncludingData => new List<string> { "rouletteBetsCancel" };
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            var gameSession = RouletteGame.GetGameSessionOrNull(query.Message.Chat.Id);
            if (gameSession != null)
                await gameSession.BetCancelRequest(query, client);
        }
    }
}
