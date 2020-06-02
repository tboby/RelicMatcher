using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RelicMatcher.Shared;

namespace RelicMatcher.Server.Service
{
    public class Assignment
    {
        public Assignment(RelicType relicType, List<Ticket> members)
        {
            RelicType = relicType;
            Members = members;
        }

        public RelicType RelicType { get; set; }
        public List<Ticket> Members { get; set; }
    }
}
