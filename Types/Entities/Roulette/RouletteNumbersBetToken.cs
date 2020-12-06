using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Entities.Users;

namespace ChapubelichBot.Types.Entities.Roulette
{
    public class RouletteNumbersBetToken : RouletteBetToken
    {
        public int[] ChoosenNumbers { get; set; }

        public RouletteNumbersBetToken(User user, long betSum, int[] choosenNumbers) : base(user, betSum)
        {
            ChoosenNumbers = choosenNumbers;
        }

        public RouletteNumbersBetToken()
        { }
    }
}
