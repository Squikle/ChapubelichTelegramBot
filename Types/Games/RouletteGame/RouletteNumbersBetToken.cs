using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Database.Models;

namespace ChapubelichBot.Types.Games.RouletteGame
{
    class RouletteNumbersBetToken : RouletteBetToken
    {
        public int[] ChoosenNumbers { get; set; }

        public RouletteNumbersBetToken(User user, int bet, int[] choosenNumbers) : base(user, bet)
        {
            ChoosenNumbers = choosenNumbers;
        }
    }
}
