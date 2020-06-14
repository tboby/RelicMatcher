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
    public class RelicHub : Hub<IRelicHub>
    {
        private readonly TicketService _ticketService;
        private readonly ConnectionSessionService _connectionSessionService;
        private readonly ILogger<RelicHub> _logger;
        private readonly RelicListService _relicListService;
        private readonly RelicHubResponseService _relicHubResponseService;
        private IEnumerable<RelicQueueDisplay> QueueList => _ticketService.GetIndexedTickets().Select(x => new RelicQueueDisplay(){RelicDisplayName = x.RelicType.DisplayName, User = x.DisplayName});
        private IEnumerable<Ticket> CurrentQueue => _ticketService.GetIndexedTickets();

        //private int 
        public RelicHub(TicketService ticketService, ILogger<RelicHub> logger, ConnectionSessionService connectionSessionService, RelicListService relicListService, RelicHubResponseService relicHubResponseService)
        {
            _ticketService = ticketService;
            _connectionSessionService = connectionSessionService;
            _relicListService = relicListService;
            _relicHubResponseService = relicHubResponseService;
            _logger = logger;
        }


        private Guid UserGuid => _connectionSessionService.GetUserForConnection(Context.ConnectionId);

        public async Task ConnectUser(Guid sessionUserGuid)
        {
            _connectionSessionService.SetUserForConnection(Context.ConnectionId, sessionUserGuid);
            var ticket = _ticketService.GetTicket(sessionUserGuid);
            if (ticket != null)
            {
                _ticketService.SetTicketActiveState(ticket, true);
            }

            await _relicHubResponseService.RefreshClients(UserGuid);
            await UpdateClients();

        }
        public async Task QueueRelic(RelicQueueInput item)
        {
            _ticketService.CreateTicket(UserGuid, item.User, _relicListService.GetRelicFromUniqueName(item.RelicUniqueName));
            await _relicHubResponseService.RefreshClients(UserGuid);
            await UpdateClients();
        }

        public async Task DeQueueRelic()
        {
            _ticketService.DeleteTicket(UserGuid);
            await _relicHubResponseService.RefreshClients(UserGuid);
            await UpdateClients();
        }

        public async Task AcceptAssignment()
        {
            var ticket = _ticketService.GetTicket(UserGuid);
            ticket.Accepted = true;
            CheckAssignment(ticket.Assignment);
            await _relicHubResponseService.RefreshClients(UserGuid);
            await UpdateParty(ticket.Assignment);

        }

        public async Task Reset()
        {
            _ticketService.DeleteTicket(UserGuid);
            await _relicHubResponseService.RefreshClients(UserGuid);
            await Clients.Caller.ReceiveRelicQueue(QueueList);
        }
        public override async Task OnConnectedAsync()
        {
            //await RefreshClient(Context.ConnectionId);
            //await Clients.Caller.SendAsync("ReceiveRelicQueue", QueueList);
            await base.OnConnectedAsync();
        }


        private async Task UpdateClients()
        {
            await Clients.All.ReceiveRelicQueue(QueueList);
        }


        private async Task UpdateParty(Assignment assignment)
        {
            foreach (var member in assignment.Members)
            {
                await _relicHubResponseService.RefreshClients(member.UserGuid);
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