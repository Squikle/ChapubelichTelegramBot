using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChapubelichBot.Database.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }
        public bool Gender { get; set; }
        [StringLength(32)]
        public string Username { get; set; }
        [StringLength(64)]
        public string FirstName { get; set; }
        public long Balance { get; set; }
        public bool IsAvailable { get; set; }
        public short DefaultBet { get; set; }
        public bool Complimented { get; set; }
        public bool ComplimentSubscription { get; set; }
        public bool DailyRewarded { get; set; }
        public List<UserGroup> UserGroups { get; set; }
        public DateTime LastMoneyTheft { get; set; }
        public User()
        {
            UserGroups = new List<UserGroup>();
            Balance = 300;
            IsAvailable = true;
            Gender = true;
            DefaultBet = 50;
            Complimented = false;
            DailyRewarded = false;
        }
    }
}
