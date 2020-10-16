﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChapubelichBot.Database.Models
{
    public class Group
    {
        [Key]
        public long GroupId { get; set; }
        public string Name { get; set; }
        public bool IsAvailable { get; set; }

        public ICollection<User> Users { get; set; }

        public Group()
        {
            Users = new List<User>();
            IsAvailable = true;
        }
    }
}
