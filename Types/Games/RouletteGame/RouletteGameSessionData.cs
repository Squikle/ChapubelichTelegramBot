using System.Collections.Generic;
using System.Threading;
using ChapubelichBot.Init;
using ChapubelichBot.Types.Abstractions;
using Microsoft.Extensions.Configuration;
using Telegram.Bot.Types;

namespace ChapubelichBot.Types.Games.RouletteGame
{
    class RouletteGameSessionData
    {
        public long ChatId { get; set; }
        public Message GameMessage { get; set; }
        public List<RouletteBetToken> BetTokens { get; set; }
        public bool Resulting { get; set; }
        public int ResultNumber { get; set; }

        public RouletteGameSessionData(long chatId)
        {
            ChatId = chatId;
            BetTokens = new List<RouletteBetToken>();
        }
    }
}
