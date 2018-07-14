using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class ChallengesPlayers
    {
        public byte[] EntryId { get; set; }
        public byte[] ChallengeId { get; set; }
        public byte[] OwnerId { get; set; }
        public bool Confirmed { get; set; }
        public byte? Seat { get; set; }

        public Challenges Challenge { get; set; }
        public Owners Owner { get; set; }
    }
}
