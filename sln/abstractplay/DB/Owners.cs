using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class Owners
    {
        public Owners()
        {
            GamesDataChats = new HashSet<GamesDataChats>();
            GamesDataPlayers = new HashSet<GamesDataPlayers>();
            GamesDataWhoseturn = new HashSet<GamesDataWhoseturn>();
            GamesInfoTags = new HashSet<GamesInfoTags>();
            OwnersNames = new HashSet<OwnersNames>();
        }

        public byte[] OwnerId { get; set; }
        public byte[] CognitoId { get; set; }
        public byte[] PlayerId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime ConsentDate { get; set; }
        public string Country { get; set; }
        public bool Anonymous { get; set; }
        public string Tagline { get; set; }

        public ICollection<GamesDataChats> GamesDataChats { get; set; }
        public ICollection<GamesDataPlayers> GamesDataPlayers { get; set; }
        public ICollection<GamesDataWhoseturn> GamesDataWhoseturn { get; set; }
        public ICollection<GamesInfoTags> GamesInfoTags { get; set; }
        public ICollection<OwnersNames> OwnersNames { get; set; }
    }
}
