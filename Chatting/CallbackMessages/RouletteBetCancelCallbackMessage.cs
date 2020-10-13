using ChapubelichBot.Database;
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
using User = ChapubelichBot.Database.Models.User;

namespace ChapubelichBot.Chatting.CallbackMessages
{
    class RouletteBetCancelCallbackMessage : CallBackMessage
    {
        public override List<string> IncludingData => new List<string> { "rouletteBetsCancel" };
        public override async Task ExecuteAsync(CallbackQuery query, ITelegramBotClient client)
        {
            var gameSession = RouletteTableStatic.GetGameSessionOrNull(query.Message.Chat.Id);
            if (gameSession != null)
                await gameSession.BetCancel(query, client);
        }
    }
}
