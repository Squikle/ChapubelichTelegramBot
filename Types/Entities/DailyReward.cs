using System.ComponentModel.DataAnnotations;

namespace ChapubelichBot.Types.Entities
{
    public class DailyReward
    {
        [Key]
        public int UserId { get; set; }
        public int Stage { get; set; }
        public bool Rewarded { get; set; }
        public User User { get; set; }
    }
}
