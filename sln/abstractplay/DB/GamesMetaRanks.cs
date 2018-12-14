using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesMetaRanks
    {
        public byte[] EntryId { get; set; }
        public byte[] OwnerId { get; set; }
        public byte[] GameId { get; set; }
        public ushort Rank { get; set; }

        public virtual GamesMeta Game { get; set; }
        public virtual Owners Owner { get; set; }
    }
}
