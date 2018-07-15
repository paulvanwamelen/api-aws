using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesData
    {
        public GamesData()
        {
            GamesDataChats = new HashSet<GamesDataChats>();
            GamesDataPlayers = new HashSet<GamesDataPlayers>();
            GamesDataStates = new HashSet<GamesDataStates>();
            GamesDataWhoseturn = new HashSet<GamesDataWhoseturn>();
        }

        public byte[] EntryId { get; set; }
        public byte[] GameId { get; set; }
        public bool Closed { get; set; }
        public bool Alert { get; set; }
        public string Variants { get; set; }

        public virtual ICollection<GamesDataChats> GamesDataChats { get; set; }
        public virtual ICollection<GamesDataPlayers> GamesDataPlayers { get; set; }
        public virtual ICollection<GamesDataStates> GamesDataStates { get; set; }
        public virtual ICollection<GamesDataWhoseturn> GamesDataWhoseturn { get; set; }
    }
}
