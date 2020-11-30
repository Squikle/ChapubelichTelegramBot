using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChapubelichBot.Types.Entities
{
    public class CrocodileGameSession
    {
        [Key]
        public long ChatId { get; set; }
        public string Word { get; set; }
        public int GameMessageId { get; set; }
        public List<User> HostCandidates { get; set; }
        public User Host { get; set; }
    }
}
