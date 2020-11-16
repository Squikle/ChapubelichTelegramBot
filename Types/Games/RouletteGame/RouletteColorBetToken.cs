using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Enums;

namespace ChapubelichBot.Types.Games.RouletteGame
{
    public class RouletteColorBetToken : RouletteBetToken
    {
        public RouletteColorEnum ChoosenColor { get; set; }
        public RouletteColorBetToken(User user, long betSum, RouletteColorEnum choosenColor) : base(user, betSum)
        {
            ChoosenColor = choosenColor;
        }
    }
}
