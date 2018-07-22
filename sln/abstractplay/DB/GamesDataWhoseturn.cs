using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesDataWhoseturn
    {
        public byte[] GameId { get; set; }
        public byte[] OwnerId { get; set; }

        public virtual GamesData Game { get; set; }
        public virtual Owners Owner { get; set; }
    }
}
