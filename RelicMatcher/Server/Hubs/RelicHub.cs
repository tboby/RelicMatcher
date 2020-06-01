using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using RelicMatcher.Shared;

namespace RelicMatcher.Server.Hubs
{
    public class RelicHub : Hub
    {
        private static List<RelicType> CurrentRelics = new List<RelicType>();

        public async Task AddRelic(string user, RelicType relicType)
        {
            CurrentRelics.Add(relicType);
            await Clients.All.SendAsync("ReceiveRelicList", CurrentRelics);
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("ReceiveRelicList", CurrentRelics);
            await base.OnConnectedAsync();
        }
    }
}