using ChapubelichBot.Types.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChapubelichBot.Database.Models
{
    [Table("GirlCompliments")]
    class GirlCompliment
    {
        [Key]
        public int ComplimentId { get; set; }

        [Column(TypeName = "VARCHAR")]
        [Required]
        public string ComplimentText { get; set; }

        public GirlCompliment(string compliment)
        {
            ComplimentText = compliment;
        }
        public GirlCompliment()
        { }
    }
}
