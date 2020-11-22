using System.ComponentModel.DataAnnotations;

namespace ChapubelichBot.Types.Entities
{
    public class GroupDailyPerson
    {
        public string RolledName { get; set; }
        public int UserId { get; set; }
        [Key]
        public long GroupId { get; set; }

        public User User;
        public Group Group;
    }
}
