using System;
using System.ComponentModel.DataAnnotations;

namespace ChapubelichBot.Types.Entities
{
    public class CrocodileHostCandidate
    {
        [Key]
        public int CandidateId { get; set; }
        public User Candidate { get; set; }

        public long CrocodileGameSessionId { get; set; }
        public CrocodileGameSession CrocodileGameSession { get; set; }

        public DateTime RegistrationTime { get; set; }
    }
}
