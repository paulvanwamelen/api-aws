using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesMeta
    {
        public GamesMeta()
        {
            Challenges = new HashSet<Challenges>();
            GamesData = new HashSet<GamesData>();
            GamesMetaRanks = new HashSet<GamesMetaRanks>();
            GamesMetaStatus = new HashSet<GamesMetaStatus>();
            GamesMetaTags = new HashSet<GamesMetaTags>();
            GamesMetaVariants = new HashSet<GamesMetaVariants>();
        }

        public byte[] GameId { get; set; }
        public string Shortcode { get; set; }
        public string Name { get; set; }
        public DateTime? LiveDate { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public bool IsLive { get; set; }
        public byte[] PublisherId { get; set; }
        public string PlayerCounts { get; set; }
        public string Version { get; set; }
        public string State { get; set; }
        public string Changelog { get; set; }
        public string SampleRep { get; set; }

        public virtual GamesMetaPublishers Publisher { get; set; }
        public virtual ICollection<Challenges> Challenges { get; set; }
        public virtual ICollection<GamesData> GamesData { get; set; }
        public virtual ICollection<GamesMetaRanks> GamesMetaRanks { get; set; }
        public virtual ICollection<GamesMetaStatus> GamesMetaStatus { get; set; }
        public virtual ICollection<GamesMetaTags> GamesMetaTags { get; set; }
        public virtual ICollection<GamesMetaVariants> GamesMetaVariants { get; set; }
    }
}
