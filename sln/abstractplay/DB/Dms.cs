using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class Dms
    {
        public byte[] EntryId { get; set; }
        public byte[] SenderId { get; set; }
        public byte[] ReceiverId { get; set; }
        public DateTime DateSent { get; set; }
        public string Message { get; set; }
        public bool Read { get; set; }

        public virtual Owners Receiver { get; set; }
        public virtual Owners Sender { get; set; }
    }
}
