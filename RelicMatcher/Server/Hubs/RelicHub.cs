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
        private static List<RelicType> CurrentRelics = new List<RelicType>();
        // TODO: Rewrite this to make connectionid secure
        private static Dictionary<string, RelicQueueItem> CurrentQueue = new Dictionary<string, RelicQueueItem>();
        private static IEnumerable<RelicQueueItem> QueueList => CurrentQueue.Values;
        public async Task QueueRelic(RelicQueueItem item)
        {
            CurrentQueue[Context.ConnectionId] = item;
            await UpdateClients();
        }
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("ReceiveRelicQueue", QueueList);
            await base.OnConnectedAsync();
        }

        private async Task UpdateClients()
        {
            await Clients.All.SendAsync("ReceiveRelicQueue", QueueList);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            CurrentQueue.Remove(Context.ConnectionId);
            await UpdateClients();
            await base.OnDisconnectedAsync(exception);
        }
    }
}