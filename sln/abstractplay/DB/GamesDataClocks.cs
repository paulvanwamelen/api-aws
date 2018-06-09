using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesDataClocks
    {
        public byte[] GameId { get; set; }
        public byte[] PlayerId { get; set; }
        public int Current { get; set; }
        public int Increment { get; set; }
        public int Maximum { get; set; }
    }
}
