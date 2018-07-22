using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesDataStates
    {
        public byte[] StateId { get; set; }
        public byte[] GameId { get; set; }
        public string State { get; set; }
        public string RenderRep { get; set; }
        public DateTime Timestamp { get; set; }

        public virtual GamesData Game { get; set; }
    }
}
