using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ChapubelichBot.Types.Entities.Alias;
using ChapubelichBot.Types.Entities.Users;

namespace ChapubelichBot.Types.Entities.Groups
{
    public class Group
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GroupId { get; set; }
        public string Name { get; set; }
        public bool IsAvailable { get; set; }
        public List<int> LastGameSessions { get; set; }

        public GroupDailyPerson GroupDailyPerson { get; set; }
        public AliasGameSession AliasGameSession { get; set; }

        public List<User> Users { get; set; }

        public Group()
        {
            Users = new List<User>();
            LastGameSessions = new List<int>();
            IsAvailable = true;
        }
    }
}
