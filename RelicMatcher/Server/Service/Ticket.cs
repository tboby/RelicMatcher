using System;
using RelicMatcher.Shared;

namespace RelicMatcher.Server.Service
{
    public class Ticket
    {
        public Ticket(Guid userGuid, string displayName, Characteristics characteristics, RelicType relicType)
        {
            UserGuid = userGuid;
            DisplayName = displayName;
            RelicType = relicType;
            Active = true;
            Characteristics = characteristics;
        }

        public Guid UserGuid { get; set; }
        public string DisplayName { get; set; }
        public RelicType RelicType { get; set; }
        public Characteristics Characteristics { get; set; }
        public bool Active { get; set; }
        public Assignment? Assignment { get; set; }
        public bool Accepted { get; set; }
    }
}