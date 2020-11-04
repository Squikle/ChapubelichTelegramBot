using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChapubelichBot.Database.Models
{
    class BoyCompliment
    {

        [Key]
        public int ComplimentId { get; set; }

        [Column(TypeName = "VARCHAR")]
        [Required]
        public string ComplimentText { get; set; }

        public BoyCompliment(string compliment)
        {
            ComplimentText = compliment;
        }
        public BoyCompliment()
        { }
    }
}
