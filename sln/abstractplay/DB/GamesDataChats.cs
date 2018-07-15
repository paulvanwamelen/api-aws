using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesDataChats
    {
        public byte[] ChatId { get; set; }
        public byte[] GameId { get; set; }
        public byte[] PlayerId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }

        public virtual GamesData Game { get; set; }
        public virtual Owners Player { get; set; }
    }
}
