using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using RelicMatcher.Client.Pages;
using RelicMatcher.Shared;

namespace RelicMatcher.Server.Hubs
{
    public class RelicHub : Hub
    {
        public class RelicQueueItem
        {
            public string User { get; set; }
            public RelicType RelicType { get; set; }
        }
        private static List<RelicType> CurrentRelics = new List<RelicType>();
        // TODO: Rewrite this to make connectionid secure
        private static Dictionary<string, RelicQueueItem> CurrentQueue = new Dictionary<string, RelicQueueItem>();
        private static IEnumerable<RelicType> QueueRelics => CurrentQueue.Values.Select(x => x.RelicType);
        public async Task QueueRelic(string user, RelicType relicType)
        {
            CurrentQueue[Context.ConnectionId] = new RelicQueueItem {RelicType = relicType, User = user};
            await UpdateClients();
        }
        public async Task AddRelic(string user, RelicType relicType)
        {
            CurrentRelics.Add(relicType);
            await UpdateClients();
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("ReceiveRelicList", QueueRelics);
            await base.OnConnectedAsync();
        }

        private async Task UpdateClients()
        {
            await Clients.All.SendAsync("ReceiveRelicList", QueueRelics);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            CurrentQueue.Remove(Context.ConnectionId);
            await UpdateClients();
            await base.OnDisconnectedAsync(exception);
        }
    }
}