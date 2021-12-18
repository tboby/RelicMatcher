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
        private IEnumerable<RelicQueueDisplay> QueueList =>
            _ticketService.GetIndexedTickets()
                .Select(x => new RelicQueueDisplay()
                {
                    RelicDisplayName = x.RelicType.DisplayName,
                    User = x.DisplayName,
                    Platform = x.Characteristics.Platform.ToString()
                });

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
                UserRelicQueueState userState = new UserRelicQueueState(queueStatus, null, null, null, true);
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
                        DisplayName = x.DisplayName,
                        Host = ticket.Assignment.Host == x
                    }),
                    DeadLine = ticket.Assignment.DeadLine
                };
                UserRelicQueueState userState = new UserRelicQueueState(queueStatus, ticket.RelicType, party, ticket.DisplayName, ticket.Active);
                await _hubContext.Clients.Clients(connectionIDs).ReceiveUserState(userState);
            }
            else
            {
                var queueStatus = RelicQueueStatus.Queued;
                UserRelicQueueState userState = new UserRelicQueueState(queueStatus, ticket.RelicType, null, ticket.DisplayName, ticket.Active);
                await _hubContext.Clients.Clients(connectionIDs).ReceiveUserState(userState);
            }
            //var userState = new UserRelicQueueState();
        }

        public async Task CheckForGroups()
        {
            var groups =
                _ticketService.GetIndexedTickets()
                    .GroupBy(x => new {x.RelicType, x.Characteristics.Platform})
                    .Where(x => x.Count() >= PartySize);
            foreach (var group in groups)
            {
                var members = group.OrderBy(x => x.TimeStamp);
                var host = members.First(x => x.Characteristics.HostPreference != HostPreference.CannotHost);
                var restOfGroup = members.Where(x => x != host).Take(PartySize - 1).ToList();
                restOfGroup.Add(host);
                _ticketService.CreateAssignment(restOfGroup, group.Key.RelicType, host); 
                foreach (var member in members)
                {
                    await RefreshClients(member.UserGuid);
                }
            }
        }

        public async Task UpdateClients()
        {
            await _hubContext.Clients.All.ReceiveRelicQueue(QueueList);
        }


        public async Task ExpireGroups()
        {
            var assignments = _ticketService.GetAssignments();
            foreach (var assignment in assignments.Where(x => x.DeadLine.AddSeconds(2) < DateTime.Now))
            {
                foreach (var member in assignment.Members)
                {
                    if (member.Accepted == false)
                    {
                        member.SetInactive();
                    }
                    member.Assignment = null;
                    await RefreshClients(member.UserGuid);
                }
            }
        }
    }
}
