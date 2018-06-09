using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesInfoStatus
    {
        public byte[] StatusId { get; set; }
        public byte[] GameId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public sbyte IsUp { get; set; }
        public string Message { get; set; }

        public GamesInfo Game { get; set; }
    }
}
