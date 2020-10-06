using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chapubelich.Database.Models
{
    public class User
    {
        public User()
        {
            Balance = 300;
            IsAvailable = true;
        }
        [Key]
        public int Id { get; set; }
        public bool? Gender { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public int Balance { get; set; }
        public bool IsAvailable { get; set; }

        [Index(IsUnique = true)]
        public int UserId { get; set; }

        public ICollection<UserGroup> UserGroup { get; set; }
    }
}
