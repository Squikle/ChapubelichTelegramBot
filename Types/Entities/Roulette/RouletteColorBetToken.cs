using ChapubelichBot.Types.Abstractions;
using ChapubelichBot.Types.Entities.Users;
using ChapubelichBot.Types.Enums;

namespace ChapubelichBot.Types.Entities.Roulette
{
    public class RouletteColorBetToken : RouletteBetToken
    {
        public RouletteColorEnum ChoosenColor { get; set; }
        public override long GetGainSum()
        {
            return ChoosenColor switch
            {
                RouletteColorEnum.Red => BetSum * 1,
                RouletteColorEnum.Black => BetSum * 1,
                RouletteColorEnum.Green => BetSum * 35,
                _ => BetSum
            };
        }

        public RouletteColorBetToken(User user, long betSum, RouletteColorEnum choosenColor) : base(user, betSum)
        {
            ChoosenColor = choosenColor;
        }
        public RouletteColorBetToken()
        {}
    }
}
