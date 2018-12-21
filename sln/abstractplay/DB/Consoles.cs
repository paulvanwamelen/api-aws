using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class Consoles
    {
        public Consoles()
        {
            ConsolesVotes = new HashSet<ConsolesVotes>();
        }

        public byte[] EntryId { get; set; }
        public byte[] GameId { get; set; }
        public byte[] OwnerId { get; set; }
        public DateTime Timestamp { get; set; }
        public ushort Command { get; set; }
        public string Data { get; set; }

        public virtual GamesData Game { get; set; }
        public virtual Owners Owner { get; set; }
        public virtual ICollection<ConsolesVotes> ConsolesVotes { get; set; }
    }
}
