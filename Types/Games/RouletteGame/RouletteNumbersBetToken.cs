using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Database.Models;

namespace ChapubelichBot.Types.Games.RouletteGame
{
    class RouletteNumbersBetToken : RouletteBetToken
    {
        public int[] ChoosenNumbers { get; set; }

        public RouletteNumbersBetToken(User user, long betSum, int[] choosenNumbers) : base(user, betSum)
        {
            ChoosenNumbers = choosenNumbers;
        }
    }
}
