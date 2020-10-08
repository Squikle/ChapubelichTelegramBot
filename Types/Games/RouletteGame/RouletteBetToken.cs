using ChapubelichBot.Database.Models;
using ChapubelichBot.Types.Enums;

namespace ChapubelichBot.Types.Games.RouletteGame
{
    public class RouletteBetToken
    {
        public int UserId { get; set; }
        public int BetSum { get; set; }
        public RouletteColorEnum ColorChoose { get; set; }
        public RouletteBetToken(User user, int bet, RouletteColorEnum colorChoose)
        {
            UserId = user.UserId;
            BetSum = bet;
            ColorChoose = colorChoose;
        }
    }
}
