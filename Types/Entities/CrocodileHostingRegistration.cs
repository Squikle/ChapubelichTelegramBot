using System;
using System.Collections.Generic;
using System.Text;

namespace ChapubelichBot.Types.Entities
{
    public class CrocodileHostingRegistration
    {
        public int CandidateId { get; set; }
        public User Candidate { get; set; }

        public long CrocodileGameSessionId { get; set; }
        public CrocodileGameSession CrocodileGameSession { get; set; }

        public DateTime RegistrationTime { get; set; }
    }
}
