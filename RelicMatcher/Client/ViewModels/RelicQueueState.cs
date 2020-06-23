using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RelicMatcher.Shared;

#nullable enable
namespace RelicMatcher.Client
{
    public class UserRelicQueueState
    {
        public RelicQueueStatus RelicQueueStatus { get; set; }
        public string? UserDisplayName { get; set; }
        public RelicType? RelicType { get; set; }
        public Party? Party { get; set;  }
        public bool Active { get; set; }

        public static UserRelicQueueState DefaultState =>
#pragma warning disable CS0618 // Type or member is obsolete
            new UserRelicQueueState()
            {
                RelicQueueStatus = RelicQueueStatus.None,
                RelicType = null,
                Party = null,
                Active = true
            };
#pragma warning restore CS0618 // Type or member is obsolete
        [Obsolete("Do not use, only for signalr")]
        public UserRelicQueueState()
        {
                
        }
        public UserRelicQueueState(RelicQueueStatus relicQueueStatus, RelicType? relicType, Party? party, string? userDisplayName, bool active)
        {
            RelicQueueStatus = relicQueueStatus;
            RelicType = relicType;
            Party = party;
            UserDisplayName = userDisplayName;
            Active = active;
        }
    }

    public enum RelicQueueStatus
    {
        None,
        Queued,
        PartyFound,
        Done
    }
}
