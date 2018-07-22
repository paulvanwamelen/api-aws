using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesData
    {
        public GamesData()
        {
            GamesDataChats = new HashSet<GamesDataChats>();
            GamesDataClocks = new HashSet<GamesDataClocks>();
            GamesDataPlayers = new HashSet<GamesDataPlayers>();
            GamesDataStates = new HashSet<GamesDataStates>();
            GamesDataWhoseturn = new HashSet<GamesDataWhoseturn>();
        }

        public byte[] EntryId { get; set; }
        public byte[] GameMetaId { get; set; }
        public bool Closed { get; set; }
        public bool Alert { get; set; }
        public string Variants { get; set; }
        public ushort ClockStart { get; set; }
        public ushort ClockInc { get; set; }
        public ushort ClockMax { get; set; }
        public bool ClockFrozen { get; set; }

        public virtual GamesMeta GameMeta { get; set; }
        public virtual ICollection<GamesDataChats> GamesDataChats { get; set; }
        public virtual ICollection<GamesDataClocks> GamesDataClocks { get; set; }
        public virtual ICollection<GamesDataPlayers> GamesDataPlayers { get; set; }
        public virtual ICollection<GamesDataStates> GamesDataStates { get; set; }
        public virtual ICollection<GamesDataWhoseturn> GamesDataWhoseturn { get; set; }
    }
}
