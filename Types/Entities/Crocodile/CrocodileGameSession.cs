using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ChapubelichBot.Types.Entities.Groups;
using ChapubelichBot.Types.Entities.Users;

namespace ChapubelichBot.Types.Entities.Crocodile
{
    public class CrocodileGameSession
    {
        [Key]
        public long GroupId { get; set; }
        [StringLength(50)]
        public string[] WordVariants { get; set; }
        public string GameWord { get; set; }
        public int GameMessageId { get; set; }
        public string GameMessageText { get; set; }
        public DateTime? StartTime { get; set; }
        [ConcurrencyCheck]
        public int Attempts { get; set; }

        public List<CrocodileHostCandidate> HostCandidates { get; set; }

        public User Host { get; set; }
        public DateTime LastActivity { get; set; }
        public Group Group { get; set; }

        public CrocodileGameSession()
        {
            HostCandidates = new List<CrocodileHostCandidate>();
            LastActivity = DateTime.UtcNow;
        }
    }
}
