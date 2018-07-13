using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesMetaStatus
    {
        public byte[] StatusId { get; set; }
        public byte[] GameId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public bool IsUp { get; set; }
        public string Message { get; set; }

        public GamesMeta Game { get; set; }
    }
}
