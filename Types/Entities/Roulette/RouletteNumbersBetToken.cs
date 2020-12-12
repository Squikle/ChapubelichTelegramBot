using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Entities.Users;

namespace ChapubelichBot.Types.Entities.Roulette
{
    public class RouletteNumbersBetToken : RouletteBetToken
    {
        public int[] ChoosenNumbers { get; set; }

        public override long GetGainSum()
        {
            return ChoosenNumbers.Length switch
            {
                1 => BetSum * 35,
                2 => BetSum * 17,
                3 => BetSum * 11,
                4 => BetSum * 8,
                6 => BetSum * 5,
                12 => BetSum * 2,
                18 => BetSum * 1,
                _ => BetSum
            };
        }

        public RouletteNumbersBetToken(User user, long betSum, int[] choosenNumbers) : base(user, betSum)
        {
            ChoosenNumbers = choosenNumbers;
        }

        public RouletteNumbersBetToken()
        { }
    }
}
