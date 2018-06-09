using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesInfoPublishers
    {
        public GamesInfoPublishers()
        {
            GamesInfo = new HashSet<GamesInfo>();
        }

        public byte[] PublisherId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string EmailAdmin { get; set; }
        public string EmailTechnical { get; set; }

        public ICollection<GamesInfo> GamesInfo { get; set; }
    }
}
