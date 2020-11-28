using System;
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
        public bool Complimented { get; set; }
        public bool ComplimentSubscription { get; set; }
        public List<int> LastGameSessions { get; set; }

        public DailyReward DailyReward { get; set; }
        public UserTheft UserTheft { get; set; }

        public List<Group> Groups { get; set; }

        public User()
        {
            Groups = new List<Group>();
            LastGameSessions = new List<int>();
            Balance = 300;
            Gender = true;
            DefaultBet = 50;
            Complimented = false;
        }
    }
}
