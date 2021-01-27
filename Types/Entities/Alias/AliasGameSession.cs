using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ChapubelichBot.Types.Entities.Groups;
using ChapubelichBot.Types.Entities.Users;

namespace ChapubelichBot.Types.Entities.Alias
{
    public class AliasGameSession
    {
        [Key]
        public long GroupId { get; set; }
        [StringLength(50)]
        public string[] WordVariants { get; set; }
        public string GameWord { get; set; }
        public int GameMessageId { get; set; }
        [ConcurrencyCheck]
        public string GameMessageText { get; set; }
        [ConcurrencyCheck]
        public DateTime? StartTime { get; set; }
        [ConcurrencyCheck]
        public int Attempts { get; set; }

        public List<AliasHostCandidate> HostCandidates { get; set; }

        public User Host { get; set; }
        public DateTime LastActivity { get; set; }
        public Group Group { get; set; }

        public AliasGameSession()
        {
            HostCandidates = new List<AliasHostCandidate>();
            LastActivity = DateTime.UtcNow;
        }
    }
}
