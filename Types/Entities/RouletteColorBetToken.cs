﻿using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Enums;

namespace ChapubelichBot.Types.Entities
{
    public class RouletteColorBetToken : RouletteBetToken
    {
        public RouletteColorEnum ChoosenColor { get; set; }
        public RouletteColorBetToken(User user, long betSum, RouletteColorEnum choosenColor) : base(user, betSum)
        {
            ChoosenColor = choosenColor;
        }
        public RouletteColorBetToken()
        {}
    }
}