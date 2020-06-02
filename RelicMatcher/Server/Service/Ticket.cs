using RelicMatcher.Shared;

namespace RelicMatcher.Server.Service
{
    public class Ticket
    {
        public Ticket(string connectionId, string displayName, RelicType relicType)
        {
            ConnectionId = connectionId;
            DisplayName = displayName;
            RelicType = relicType;
        }

        public string ConnectionId { get; set; }
        public string DisplayName { get; set; }
        public RelicType RelicType { get; set; }
        public Assignment? Assignment { get; set; }
    }
}