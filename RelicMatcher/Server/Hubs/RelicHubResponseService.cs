using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using RelicMatcher.Client;
using RelicMatcher.Server.Service;
using RelicMatcher.Shared;

namespace RelicMatcher.Server.Hubs
{
    public class RelicHubResponseService
    {

        public static int PartySize = 2;

        private readonly ConnectionSessionService _connectionSessionService;
        private readonly TicketService _ticketService;
        private readonly IHubContext<RelicHub, IRelicHub> _hubContext;
        public RelicHubResponseService(IHubContext<RelicHub, IRelicHub> hubContext, TicketService ticketService, ConnectionSessionService connectionSessionService)
        {
            _hubContext = hubContext;
            _ticketService = ticketService;
            _connectionSessionService = connectionSessionService;
        }
        public async Task RefreshClients(Guid userGuid)
        {

            //var userGuid = _connectionSessionService.GetUserForConnection(connectionId);
            var connectionIDs = _connectionSessionService.GetAllConnectionsForUser(userGuid);
            var ticket = _ticketService.GetTicket(userGuid);

            if (ticket == null)
            {
                var queueStatus = RelicQueueStatus.None;
                UserRelicQueueState userState = new UserRelicQueueState(queueStatus, null, null);
                await _hubContext.Clients.Clients(connectionIDs).ReceiveUserState(userState);
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
                    }),
                    DeadLine = ticket.Assignment.DeadLine
                };
                UserRelicQueueState userState = new UserRelicQueueState(queueStatus, ticket.RelicType, party);
                await _hubContext.Clients.Clients(connectionIDs).ReceiveUserState(userState);
            }
            else
            {
                var queueStatus = RelicQueueStatus.Queued;
                UserRelicQueueState userState = new UserRelicQueueState(queueStatus, ticket.RelicType, null);
                await _hubContext.Clients.Clients(connectionIDs).ReceiveUserState(userState);
            }
            //var userState = new UserRelicQueueState();
        }

        public async Task CheckForGroups()
        {
            var groups =
                _ticketService.GetIndexedTickets()
                    .GroupBy(x => x.RelicType)
                    .Where(x => x.Count() >= PartySize);
            foreach (var group in groups)
            {
                var members = group.Take(PartySize).ToList();
                _ticketService.CreateAssignment(members, group.Key);
                foreach (var member in members)
                {
                    await RefreshClients(member.UserGuid);
                }
            }
        }

        public async Task ExpireGroups()
        {
            var assignments = _ticketService.GetAssignments();
            foreach (var assignment in assignments.Where(x => x.DeadLine.AddSeconds(2) < DateTime.Now))
            {
                foreach (var member in assignment.Members)
                {
                    member.Active = member.Accepted;
                    member.Assignment = null;
                }
            }
        }
    }
}
