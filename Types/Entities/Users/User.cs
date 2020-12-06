using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ChapubelichBot.Types.Entities.Crocodile;
using ChapubelichBot.Types.Entities.Groups;

namespace ChapubelichBot.Types.Entities.Users
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }
        public bool Gender { get; set; }
        [StringLength(32)]
        public string Username { get; set; }
        public long Balance { get; set; }
        public short DefaultBet { get; set; }
        public List<int> LastGameSessions { get; set; }

        public UserDailyReward DailyReward { get; set; }
        public UserTheft UserTheft { get; set; }
        public UserCompliment UserCompliment { get; set; }
        public CrocodileHostCandidate CrocodileHostingRegistration { get; set; }

        public List<Group> Groups { get; set; }

        public User()
        {
            Groups = new List<Group>();
            LastGameSessions = new List<int>();
            Balance = 300;
            Gender = true;
            DefaultBet = 50;
        }
    }
}
