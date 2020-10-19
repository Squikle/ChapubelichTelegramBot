using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChapubelichBot.Types.Abstractions
{
    class Compliment
    {
        [Key]
        public int ComplimentId { get; set; }

        [Column(TypeName = "VARCHAR")]
        [Required]
        [Index(IsUnique = true)]
        public string ComplimentText { get; set; }

        public Compliment(string compliment)
        {
            ComplimentText = compliment;
        }
        public Compliment()
        { }
    }
}
