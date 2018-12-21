using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class ConsolesVotes
    {
        public byte[] EntryId { get; set; }
        public byte[] ConsoleId { get; set; }
        public byte[] Voter { get; set; }
        public bool Vote { get; set; }

        public virtual Consoles Console { get; set; }
        public virtual Owners VoterNavigation { get; set; }
    }
}
