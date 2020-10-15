using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChapubelichBot.Database.Models
{
    public class Group
    {
        public string Name { get; set; }
        [Key]
        public long GroupId { get; set; }
        public bool IsAvailable { get; set; }
        

        public ICollection<UserGroup> UserGroup { get; set; }
    }
}
