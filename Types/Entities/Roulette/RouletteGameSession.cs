﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChapubelichBot.Types.Entities.Roulette
{
    public class RouletteGameSession
    {
        [Key]
        public long ChatId { get; set; }
        public int GameMessageId { get; set; }
        public int AnimationMessageId { get; set; }
        public List<RouletteColorBetToken> ColorBetTokens { get; set; }
        public List<RouletteNumbersBetToken> NumberBetTokens { get; set; }
        [ConcurrencyCheck]
        public bool Resulting { get; set; }
        public int ResultNumber { get; set; }
        public DateTime LastActivity { get; set; }
    }
}
