using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RelicMatcher.Client;
using RelicMatcher.Client.Pages;
using RelicMatcher.Server.Service;
using RelicMatcher.Shared;

namespace RelicMatcher.Server.Hubs
{
    public class RelicHub : Hub
    {
        private readonly TicketService _ticketService;
        private readonly ConnectionSessionService _connectionSessionService;
        private readonly ILogger<RelicHub> _logger;
        private readonly RelicListService _relicListService;
        private IEnumerable<RelicQueueDisplay> QueueList => _ticketService.GetIndexedTickets().Select(x => new RelicQueueDisplay(){RelicDisplayName = x.RelicType.DisplayName, User = x.DisplayName});
        private IEnumerable<Ticket> CurrentQueue => _ticketService.GetIndexedTickets();

        //private int 
        public RelicHub(TicketService ticketService, ILogger<RelicHub> logger, ConnectionSessionService connectionSessionService, RelicListService relicListService)
        {
            _ticketService = ticketService;
            _connectionSessionService = connectionSessionService;
            _relicListService = relicListService;
            _logger = logger;
        }

        public static int PartySize = 2;

        private Guid UserGuid => _connectionSessionService.GetUserForConnection(Context.ConnectionId);

        public async Task ConnectUser(Guid sessionUserGuid)
        {
            _connectionSessionService.SetUserForConnection(Context.ConnectionId, sessionUserGuid);
            var ticket = _ticketService.GetTicket(sessionUserGuid);
            if (ticket != null)
            {
                _ticketService.SetTicketActiveState(ticket, true);
            }
            await RefreshClient(UserGuid);
            await UpdateClients();

        }
        public async Task QueueRelic(RelicQueueInput item)
        {
            _ticketService.CreateTicket(UserGuid, item.User, _relicListService.GetRelicFromUniqueName(item.RelicUniqueName));
            await CheckForGroups();
            await RefreshClient(UserGuid);
            await UpdateClients();
        }

        public async Task DeQueueRelic()
        {
            _ticketService.DeleteTicket(UserGuid);
            await RefreshClient(UserGuid);
            await UpdateClients();
        }

        public async Task AcceptAssignment()
        {
            var ticket = _ticketService.GetTicket(UserGuid);
            ticket.Accepted = true;
            CheckAssignment(ticket.Assignment);
            await RefreshClient(UserGuid);
            await UpdateParty(ticket.Assignment);

        }

        public async Task Reset()
        {
            _ticketService.DeleteTicket(UserGuid);
            await RefreshClient(UserGuid);
            await Clients.Caller.SendAsync("ReceiveRelicQueue", QueueList);
        }
        public override async Task OnConnectedAsync()
        {
            //await RefreshClient(Context.ConnectionId);
            //await Clients.Caller.SendAsync("ReceiveRelicQueue", QueueList);
            await base.OnConnectedAsync();
        }

        private async Task RefreshClient(Guid userGuid)
        {
            //var userGuid = _connectionSessionService.GetUserForConnection(connectionId);
            var connectionIDs = _connectionSessionService.GetAllConnectionsForUser(userGuid);
            var ticket = _ticketService.GetTicket(userGuid);

            if (ticket == null)
            {
                var queueStatus = RelicQueueStatus.None;
                UserRelicQueueState userState = new UserRelicQueueState(queueStatus, null, null);
                await Clients.Clients(connectionIDs).SendAsync("ReceiveUserState", userState);
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
                        UserGuid = x.UserGuid,
                        DisplayName = x.DisplayName
                    })
                };
                UserRelicQueueState userState = new UserRelicQueueState(queueStatus, ticket.RelicType, party);
                await Clients.Clients(connectionIDs).SendAsync("ReceiveUserState", userState);
            }
            else
            {
                var queueStatus = RelicQueueStatus.Queued;
                UserRelicQueueState userState = new UserRelicQueueState(queueStatus, ticket.RelicType, null);
                await Clients.Clients(connectionIDs).SendAsync("ReceiveUserState", userState);
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
                    await RefreshClient(member.UserGuid);
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
                await RefreshClient(member.UserGuid);
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
            var ticket = _ticketService.GetTicket(UserGuid);
            if (ticket != null)
            {
                _ticketService.SetTicketActiveState(ticket, false);
            }
            await UpdateClients();
            await base.OnDisconnectedAsync(exception);
        }
    }
}