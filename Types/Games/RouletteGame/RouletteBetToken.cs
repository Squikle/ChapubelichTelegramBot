using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Enums;
using System.Collections.Generic;

namespace ChapubelichBot.Types.Games.RouletteGame
{
    public class RouletteBetToken
    {
        public int UserId { get; set; }
        public int BetSum { get; set; }
        public RouletteColorEnum? ChoosenColor { get; set; }
        public int[] ChoosenNumbers { get; set; }
        public RouletteBetToken(User user, int bet, RouletteColorEnum? choosenColor = null, int[] choosenNumbers = null)
        {
            UserId = user.UserId;
            BetSum = bet;
            ChoosenColor = choosenColor;
            ChoosenNumbers = choosenNumbers;
        }
    }
}
