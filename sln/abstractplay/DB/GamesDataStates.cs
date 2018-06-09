using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesDataStates
    {
        public byte[] StateId { get; set; }
        public byte[] GameId { get; set; }
        public string State { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public GamesData Game { get; set; }
    }
}
