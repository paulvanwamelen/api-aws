using System;
using System.Collections.Generic;

namespace abstractplay.DB
{
    public partial class OwnersNames
    {
        public byte[] EntryId { get; set; }
        public byte[] OwnerId { get; set; }
        public string Name { get; set; }
        public DateTime EffectiveFrom { get; set; }

        public virtual Owners Owner { get; set; }
    }
}
