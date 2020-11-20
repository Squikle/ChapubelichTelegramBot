using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ChapubelichBot.Types.Games.RouletteGame;

namespace ChapubelichBot.Database.Models
{
    public class RouletteGameSession
    {
        [Key]
        public long ChatId { get; set; }
        public int GameMessageId { get; set; }
        public int AnimationMessageId { get; set; }
        public List<RouletteColorBetToken> ColorBetTokens { get; set; }
        public List<RouletteNumbersBetToken> NumberBetTokens { get; set; }
        public bool Resulting { get; set; }
        public int ResultNumber { get; set; }
        public DateTime LastActivity { get; set; }
    }
}
