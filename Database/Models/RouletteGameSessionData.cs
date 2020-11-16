using System.Collections.Generic;
using ChapubelichBot.Types.Abstractions;
using Telegram.Bot.Types;

namespace ChapubelichBot.Database.Models
{
    class RouletteGameSessionData
    {
        public long ChatId { get; set; }
        public int GameMessageId { get; set; }
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
