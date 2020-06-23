using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RelicMatcher.Shared;

namespace RelicMatcher.Server.Service
{
    public class Characteristics
    {
        public Region PreferredRegion { get; set; }
        public Platform Platform { get; set; }
        public HostPreference HostPreference { get; set; }
    }
}
