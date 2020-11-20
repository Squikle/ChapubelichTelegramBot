using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ChapubelichBot.Database.Models
{
    public class Configuration
    {
        [DefaultValue("1")]
        [Key]
        public bool Id { get; set; } 
        public DateTime LastResetTime { get; set; }
    }
}
