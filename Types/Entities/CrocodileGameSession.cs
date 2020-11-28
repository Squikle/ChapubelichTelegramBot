using System.ComponentModel.DataAnnotations;

namespace ChapubelichBot.Types.Entities
{
    class CrocodileGameSession
    {
        [Key]
        public long ChatId { get; set; }
    }
}
