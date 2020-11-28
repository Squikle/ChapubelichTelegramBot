using System.ComponentModel.DataAnnotations;

namespace ChapubelichBot.Types.Entities
{
    public class UserCompliment
    {
        [Key]
        public int UserId { get; set; }
        public bool Praised { get; set; }

        public User User { get; set; }
    }
}
