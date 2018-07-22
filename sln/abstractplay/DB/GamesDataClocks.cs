using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesDataClocks
    {
        public byte[] GameId { get; set; }
        public byte[] OwnerId { get; set; }
        public short Bank { get; set; }

        public virtual GamesData Game { get; set; }
        public virtual Owners Owner { get; set; }
    }
}
