using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChapubelichBot.Types.Entities
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
        public bool Started { get; set; }

        public List<User> HostingCandidates { get; set; }
        public List<CrocodileHostingRegistration> CrocodileHostingRegistrations { get; set; }

        public User Host { get; set; }
        public DateTime LastActivity { get; set; }
        public Group Group { get; set; }

        public CrocodileGameSession()
        {
            HostingCandidates = new List<User>();
            CrocodileHostingRegistrations = new List<CrocodileHostingRegistration>();
            Started = false;
            LastActivity = DateTime.UtcNow;
        }
    }
}
