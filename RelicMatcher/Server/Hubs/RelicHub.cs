using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using RelicMatcher.Client;
using RelicMatcher.Client.Pages;
using RelicMatcher.Server.Service;
using RelicMatcher.Shared;

namespace RelicMatcher.Server.Hubs
{
    public class RelicHub : Hub
    {
        private readonly TicketService _ticketService;
        private IEnumerable<RelicQueueItem> QueueList => _ticketService.GetTickets().Where(x => x.Assignment == null).Select(x => new RelicQueueItem(){RelicType = x.RelicType, User = x.DisplayName});
        private IEnumerable<Ticket> CurrentQueue => _ticketService.GetTickets().Where(x => x.Assignment == null);
        public RelicHub(TicketService ticketService)
        {
            _ticketService = ticketService;
        }

        public static int PartySize = 2;
        public async Task QueueRelic(RelicQueueItem item)
        {
            _ticketService.CreateTicket(Context.ConnectionId, item.User, item.RelicType);
            await CheckForGroups();
            await RefreshClient(Context.ConnectionId);
            await UpdateClients();
        }

        public async Task DeQueueRelic()
        {
            _ticketService.DeleteTicket(Context.ConnectionId);
            await RefreshClient(Context.ConnectionId);
            await UpdateClients();
        }

        public async Task AcceptAssignment()
        {
            var ticket = _ticketService.GetTicket(Context.ConnectionId);
            ticket.Accepted = true;
            CheckAssignment(ticket.Assignment);
            await RefreshClient(Context.ConnectionId);
            await UpdateParty(ticket.Assignment);

        }

        public async Task Reset()
        {
            _ticketService.DeleteTicket(Context.ConnectionId);
            await RefreshClient(Context.ConnectionId);
            await Clients.Caller.SendAsync("ReceiveRelicQueue", QueueList);
        }
        public override async Task OnConnectedAsync()
        {
            await RefreshClient(Context.ConnectionId);
            await Clients.Caller.SendAsync("ReceiveRelicQueue", QueueList);
            await base.OnConnectedAsync();
        }

        private async Task RefreshClient(string connectionId)
        {
            var ticket = _ticketService.GetTicket(connectionId);

            if (ticket == null)
            {
                var queueStatus = RelicQueueStatus.None;
                UserRelicQueueState userState = new UserRelicQueueState(queueStatus, null, null);
                await Clients.Client(connectionId).SendAsync("ReceiveUserState", userState);
            }
            else if (ticket.Assignment != null)
            {
                var queueStatus = ticket.Assignment.Done ? RelicQueueStatus.Done : RelicQueueStatus.PartyFound;
                var party = new Party()
                {
                    Done = ticket.Assignment.Done,
                    RelicType = ticket.Assignment.RelicType,
                    Members = ticket.Assignment.Members.Select(x => new UserWrapper()
                    {
                        Accepted = x.Accepted,
                        ConnectionID = x.ConnectionId,
                        DisplayName = x.DisplayName
                    })
                };
                UserRelicQueueState userState = new UserRelicQueueState(queueStatus, ticket.RelicType, party);
                await Clients.Client(connectionId).SendAsync("ReceiveUserState", userState);
            }
            else
            {
                var queueStatus = RelicQueueStatus.Queued;
                UserRelicQueueState userState = new UserRelicQueueState(queueStatus, ticket.RelicType, null);
                await Clients.Client(connectionId).SendAsync("ReceiveUserState", userState);
            }
            //var userState = new UserRelicQueueState();
        }

        private async Task UpdateClients()
        {
            await Clients.All.SendAsync("ReceiveRelicQueue", QueueList);
        }

        private async Task CheckForGroups()
        {
            var groups =
                CurrentQueue
                    .GroupBy(x => x.RelicType)
                    .Where(x => x.Count() >= PartySize);
            foreach (var group in groups)
            {
                var members = group.Take(PartySize).ToList();
                _ticketService.CreateAssignment(members, group.Key);
                foreach(var member in members)
                {
                    await RefreshClient(member.ConnectionId);
                }
                //var userWrappers = members.Select(x => new UserWrapper()
                //    {ConnectionID = x.ConnectionId, DisplayName = x.DisplayName});
                //await Clients.Clients(members.Select(x => x.ConnectionId).ToList())
                //    .SendAsync("ReceivePartyFound", new Party(){Members = userWrappers, RelicType = group.Key});
            }
        }

        private async Task UpdateParty(Assignment assignment)
        {
            foreach (var member in assignment.Members)
            {
                await RefreshClient(member.ConnectionId);
            }
        }

        private void CheckAssignment(Assignment assignment)
        {
            if (assignment.Members.All(x => x.Accepted))
            {
                assignment.Done = true;
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _ticketService.DeleteTicket(Context.ConnectionId);
            await UpdateClients();
            await base.OnDisconnectedAsync(exception);
        }
    }
}