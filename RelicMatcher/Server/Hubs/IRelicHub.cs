using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RelicMatcher.Client;
using RelicMatcher.Shared;

namespace RelicMatcher.Server.Hubs
{
    public interface IRelicHub
    {
        public Task ReceiveUserState(UserRelicQueueState userState);
        public Task ReceiveRelicQueue(IEnumerable<RelicQueueDisplay> relicQueue);

    }
}
