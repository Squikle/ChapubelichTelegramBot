using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChapubelichBot.Types.Entities
{
    [Table("GirlCompliments")]
    public class GirlCompliment
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
