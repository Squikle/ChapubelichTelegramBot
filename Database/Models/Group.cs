using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChapubelichBot.Database.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        [Index(IsUnique = true)]
        public long GroupId { get; set; }
        public bool IsAvailable { get; set; }
        

        public ICollection<UserGroup> UserGroup { get; set; }
    }
}
