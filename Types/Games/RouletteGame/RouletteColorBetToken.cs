using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Enums;

namespace ChapubelichBot.Types.Games.RouletteGame
{
    public class RouletteColorBetToken : RouletteBetToken
    {
        public RouletteColorEnum ChoosenColor { get; set; }
        public RouletteColorBetToken(User user, int bet, RouletteColorEnum choosenColor) : base(user, bet)
        {
            ChoosenColor = choosenColor;
        }
    }
}
