using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChapubelichBot.Database.Models
{
    public class User
    {
        public User()
        {
            Balance = 300;
            IsAvailable = true;
            Gender = true;
            DefaultBet = 50;
        }
        [Key]
        public int Id { get; set; }
        public bool Gender { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public long Balance { get; set; }
        public bool IsAvailable { get; set; }
        public int DefaultBet { get; set; }

        [Index(IsUnique = true)]
        public int UserId { get; set; }

        public ICollection<UserGroup> UserGroup { get; set; }
    }
}
