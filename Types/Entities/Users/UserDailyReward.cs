using System.ComponentModel.DataAnnotations;

namespace ChapubelichBot.Types.Entities.Users
{
    public class UserDailyReward
    {
        [Key]
        public int UserId { get; set; }
        public int Stage { get; set; }
        [ConcurrencyCheck]
        public bool Rewarded { get; set; }
        public User User { get; set; }
    }
}
