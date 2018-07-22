using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesDataChats
    {
        public byte[] ChatId { get; set; }
        public byte[] GameId { get; set; }
        public byte[] OwnerId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }

        public virtual GamesData Game { get; set; }
        public virtual Owners Owner { get; set; }
    }
}
