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
        public RelicQueueStatus RelicQueueStatus { get; private set; }
        public RelicType? RelicType { get; private set; }
        public Party? Party { get; private set;  }

        public static UserRelicQueueState DefaultState =>
            new UserRelicQueueState()
            {
                RelicQueueStatus = RelicQueueStatus.None,
                RelicType = null,
                Party = null
            };

        private UserRelicQueueState()
        {
                
        }
        public UserRelicQueueState(RelicQueueStatus relicQueueStatus, RelicType? relicType, Party? party)
        {
            RelicQueueStatus = relicQueueStatus;
            RelicType = relicType;
            Party = party;
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
