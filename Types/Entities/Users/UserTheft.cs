using System;
using System.ComponentModel.DataAnnotations;

namespace ChapubelichBot.Types.Entities.Users
{
    public class UserTheft
    {
        [Key]
        public int UserId { get; set; }
        public User User { get; set; }
        [ConcurrencyCheck]
        public DateTime LastMoneyTheft { get; set; }
    }
}
