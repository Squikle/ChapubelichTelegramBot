using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChapubelichBot.Database.Models
{
    class Configuration
    {
        [DefaultValue("1")]
        [Key]
        public bool Id { get; set; } 
        public DateTime LastResetTime { get; set; }
    }
}
