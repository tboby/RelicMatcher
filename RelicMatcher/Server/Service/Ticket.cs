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
            TimeStamp = DateTime.Now;
        }

        public DateTime TimeStamp { get; private set; }
        public Guid UserGuid { get; set; }
        public string DisplayName { get; set; }
        public RelicType RelicType { get; set; }
        public Characteristics Characteristics { get; set; }
        public bool Active { get; private set; }
        public Assignment? Assignment { get; set; }
        public bool Accepted { get; set; }

        public void ResetActive()
        {
            Active = true;
            TimeStamp = DateTime.Now;
        }

        public void SetInactive()
        {
            Active = false;
        }
    }
}