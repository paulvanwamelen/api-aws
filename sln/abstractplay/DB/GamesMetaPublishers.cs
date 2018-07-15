using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesMetaPublishers
    {
        public GamesMetaPublishers()
        {
            GamesMeta = new HashSet<GamesMeta>();
        }

        public byte[] PublisherId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string EmailAdmin { get; set; }
        public string EmailTechnical { get; set; }

        public virtual ICollection<GamesMeta> GamesMeta { get; set; }
    }
}
