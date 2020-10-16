using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChapubelichBot.Database.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public bool Gender { get; set; }
        [StringLength(32)]
        public string Username { get; set; }
        [StringLength(64)]
        public string FirstName { get; set; }
        public long Balance { get; set; }
        public bool IsAvailable { get; set; }
        public short DefaultBet { get; set; }

        public ICollection<Group> Groups { get; set; }

        public User()
        {
            Groups = new List<Group>();
            Balance = 300;
            IsAvailable = true;
            Gender = true;
            DefaultBet = 50;
        }
    }
}
