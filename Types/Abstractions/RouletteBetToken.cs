using System.ComponentModel.DataAnnotations;
using ChapubelichBot.Types.Entities.Roulette;
using ChapubelichBot.Types.Entities.Users;
using ChapubelichBot.Types.Enums;

namespace ChapubelichBot.Types.Abstractions
{
    public abstract class RouletteBetToken
    {
        public int UserId { get; set; }
        public User User { get; set; }

        [ConcurrencyCheck]
        public long BetSum { get; set; }
        public abstract long GetGainSum();

        protected RouletteBetToken(User user, long bet)
        {
            UserId = user.UserId;
            BetSum = bet;
        }

        protected RouletteBetToken()
        { }

    }
}
