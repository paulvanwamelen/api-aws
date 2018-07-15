using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class Challenges
    {
        public Challenges()
        {
            ChallengesPlayers = new HashSet<ChallengesPlayers>();
        }

        public byte[] ChallengeId { get; set; }
        public byte[] GameId { get; set; }
        public byte[] OwnerId { get; set; }
        public byte NumPlayers { get; set; }
        public DateTime DateIssued { get; set; }
        public string Notes { get; set; }
        public ushort ClockStart { get; set; }
        public ushort ClockInc { get; set; }
        public ushort ClockMax { get; set; }
        public string Variants { get; set; }

        public virtual GamesMeta Game { get; set; }
        public virtual Owners Owner { get; set; }
        public virtual ICollection<ChallengesPlayers> ChallengesPlayers { get; set; }
    }
}
