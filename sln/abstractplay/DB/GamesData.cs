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
        public sbyte Closed { get; set; }
        public sbyte Alert { get; set; }
        public string Variants { get; set; }

        public ICollection<GamesDataChats> GamesDataChats { get; set; }
        public ICollection<GamesDataPlayers> GamesDataPlayers { get; set; }
        public ICollection<GamesDataStates> GamesDataStates { get; set; }
        public ICollection<GamesDataWhoseturn> GamesDataWhoseturn { get; set; }
    }
}
