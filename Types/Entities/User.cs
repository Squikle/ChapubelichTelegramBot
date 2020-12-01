using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChapubelichBot.Types.Entities
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

        public DailyReward DailyReward { get; set; }
        public UserTheft UserTheft { get; set; }
        public UserCompliment UserCompliment { get; set; }

        public List<Group> Groups { get; set; }

        public List<CrocodileGameSession> HostingSessionRequests { get; set; }
        public List<CrocodileHostingRegistration> CrocodileHostingRegistrations { get; set; }

        public User()
        {
            HostingSessionRequests = new List<CrocodileGameSession>();
            CrocodileHostingRegistrations = new List<CrocodileHostingRegistration>();
            Groups = new List<Group>();
            LastGameSessions = new List<int>();
            Balance = 300;
            Gender = true;
            DefaultBet = 50;
        }
    }
}
