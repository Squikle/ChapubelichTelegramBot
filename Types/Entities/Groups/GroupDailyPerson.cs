using System.ComponentModel.DataAnnotations;
using ChapubelichBot.Types.Entities.Users;

namespace ChapubelichBot.Types.Entities.Groups
{
    public class GroupDailyPerson
    {
        public string RolledName { get; set; }
        public int UserId { get; set; }
        [Key]
        public long GroupId { get; set; }
        public int? RollMessageId { get; set; }

        public User User;
        public Group Group;
    }
}
