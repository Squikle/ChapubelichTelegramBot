using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChapubelichBot.Chatting.CallbackMessages
{
    class RouletteRollCallbackMessage : CallBackMessage
    {
        public override List<string> IncludingData => new List<string> {"rouletteRoll"};
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            var gameSession = RouletteTableStatic.GetGameSessionOrNull(query.Message.Chat.Id);
            if (gameSession != null)
                await gameSession.Roll(query, client);
        }
    }
}
