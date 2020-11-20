using ChapubelichBot.Types.Abstractions;

namespace ChapubelichBot.Database.Models
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
