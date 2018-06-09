using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesInfo
    {
        public GamesInfo()
        {
            GamesInfoStatus = new HashSet<GamesInfoStatus>();
            GamesInfoTags = new HashSet<GamesInfoTags>();
            GamesInfoVariants = new HashSet<GamesInfoVariants>();
        }

        public byte[] GameId { get; set; }
        public string Shortcode { get; set; }
        public string Name { get; set; }
        public DateTime LiveDate { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public sbyte IsLive { get; set; }
        public byte[] PublisherId { get; set; }
        public string PlayerCounts { get; set; }
        public byte Version { get; set; }
        public string State { get; set; }
        public string Changelog { get; set; }
        public uint? Rating { get; set; }

        public GamesInfoPublishers Publisher { get; set; }
        public ICollection<GamesInfoStatus> GamesInfoStatus { get; set; }
        public ICollection<GamesInfoTags> GamesInfoTags { get; set; }
        public ICollection<GamesInfoVariants> GamesInfoVariants { get; set; }
    }
}
