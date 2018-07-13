using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesMeta
    {
        public GamesMeta()
        {
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

        public GamesMetaPublishers Publisher { get; set; }
        public ICollection<GamesMetaStatus> GamesMetaStatus { get; set; }
        public ICollection<GamesMetaTags> GamesMetaTags { get; set; }
        public ICollection<GamesMetaVariants> GamesMetaVariants { get; set; }
    }
}
