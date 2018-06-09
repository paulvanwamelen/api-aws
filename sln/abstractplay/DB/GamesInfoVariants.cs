using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class GamesInfoVariants
    {
        public byte[] GameId { get; set; }
        public byte[] VariantId { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string Group { get; set; }

        public GamesInfo Game { get; set; }
    }
}
