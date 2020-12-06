using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ChapubelichBot.Types.Entities.Users;

namespace ChapubelichBot.Types.Entities.Alias
{
    [Table("AliasHostCandidates")]
    public class AliasHostCandidate
    {
        [Key]
        public int CandidateId { get; set; }
        public User Candidate { get; set; }

        public long AliasGameSessionId { get; set; }
        public AliasGameSession AliasGameSession { get; set; }

        public DateTime RegistrationTime { get; set; }
    }
}
