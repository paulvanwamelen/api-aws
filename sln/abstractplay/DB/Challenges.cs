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
        public DateTimeOffset DateIssued { get; set; }
        public string Notes { get; set; }
        public ushort ClockStart { get; set; }
        public ushort ClockInc { get; set; }
        public ushort ClockMax { get; set; }
        public string Variants { get; set; }

        public GamesMeta Game { get; set; }
        public Owners Owner { get; set; }
        public ICollection<ChallengesPlayers> ChallengesPlayers { get; set; }
    }
}
