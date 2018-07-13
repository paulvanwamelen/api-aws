using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesDataChats
    {
        public byte[] ChatId { get; set; }
        public byte[] GameId { get; set; }
        public byte[] PlayerId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Message { get; set; }

        public GamesData Game { get; set; }
        public Owners Player { get; set; }
    }
}
