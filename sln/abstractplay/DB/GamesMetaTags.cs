using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesMetaTags
    {
        public byte[] EntryId { get; set; }
        public byte[] GameId { get; set; }
        public byte[] OwnerId { get; set; }
        public string Tag { get; set; }

        public GamesMeta Game { get; set; }
        public Owners Owner { get; set; }
    }
}
