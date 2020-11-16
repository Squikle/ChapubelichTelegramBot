using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Games.RouletteGame;

namespace ChapubelichBot.Database.Models
{
    class RouletteGameSessionData
    {
        [Key]
        public long ChatId { get; set; }
        public int GameMessageId { get; set; }
        public List<RouletteColorBetToken> ColorBetTokens { get; set; }
        public List<RouletteNumbersBetToken> NumberBetTokens { get; set; }
        public bool Resulting { get; set; }
        public int ResultNumber { get; set; }

        public RouletteGameSessionData(long chatId)
        {
            ChatId = chatId;
            ColorBetTokens = new List<RouletteColorBetToken>();
            NumberBetTokens = new List<RouletteNumbersBetToken>();
        }
    }
}
