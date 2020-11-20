using System.ComponentModel.DataAnnotations.Schema;
using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Enums;

namespace ChapubelichBot.Types.Abstractions
{
    public abstract class RouletteBetToken
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public long BetSum { get; set; }

        protected RouletteBetToken(User user, long bet)
        {
            UserId = user.UserId;
            BetSum = bet;
        }

        protected RouletteBetToken()
        { }
        public long GetGainSum()
        {
            if (this is RouletteColorBetToken colorBetToken)
            {
                switch (colorBetToken.ChoosenColor)
                {
                    case RouletteColorEnum.Red:
                        return BetSum * 1;
                    case RouletteColorEnum.Black:
                        return BetSum * 1;
                    case RouletteColorEnum.Green:
                        return BetSum * 35;
                }
            }
            else if (this is RouletteNumbersBetToken numbersBetToken)
            {
                //return (int)(winBetToken.BetSum * 35 / numbersBetToken.ChoosenNumbers.Length);
                switch (numbersBetToken.ChoosenNumbers.Length)
                {
                    case 1:
                        return BetSum * 35;
                    case 2:
                        return BetSum * 17;
                    case 3:
                        return BetSum * 11;
                    case 4:
                        return BetSum * 8;
                    case 6:
                        return BetSum * 5;
                    case 12:
                        return BetSum * 2;
                    case 18:
                        return BetSum * 1;
                }
            }

            return BetSum;
        }
    }
}
