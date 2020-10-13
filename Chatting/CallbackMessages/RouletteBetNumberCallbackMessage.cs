﻿using ChapubelichBot.Database;
using ChapubelichBot.Types.Abstractions;
using System.Threading.Tasks;
using ChapubelichBot.Types.Extensions;
using ChapubelichBot.Types.Games.RouletteGame;
using ChapubelichBot.Types.Statics;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = ChapubelichBot.Database.Models.User;

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
            var gameSession = RouletteTableStatic.GetGameSessionOrNull(query.Message.Chat.Id);
            if (gameSession != null)
                await gameSession.BetNumbers(query, client);
        }
    }
}
