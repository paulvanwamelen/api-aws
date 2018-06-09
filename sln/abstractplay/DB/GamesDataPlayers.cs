using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesDataPlayers
    {
        public byte[] GameId { get; set; }
        public byte[] PlayerId { get; set; }
        public int Seat { get; set; }

        public GamesData Game { get; set; }
        public Owners Player { get; set; }
    }
}
