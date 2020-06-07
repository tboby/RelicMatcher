﻿using System;
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
        private IEnumerable<RelicQueueItem> QueueList => _ticketService.GetTickets().Select(x => new RelicQueueItem(){RelicType = x.RelicType, User = x.DisplayName});
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
            await UpdateClients();
        }

        public async Task DeQueueRelic()
        {
            _ticketService.DeleteTicket(Context.ConnectionId);
            await UpdateClients();
        }

        public async Task AcceptAssignment()
        {
            var ticket = _ticketService.GetTicket(Context.ConnectionId);
            ticket.Accepted = true;
            CheckAssignment(ticket.Assignment);
            await UpdateParty(ticket.Assignment);

        }

        public async Task Reset()
        {
            _ticketService.DeleteTicket(Context.ConnectionId);
            await Clients.Caller.SendAsync("ReceiveRelicQueue", QueueList);
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
                var userWrappers = members.Select(x => new UserWrapper()
                    {ConnectionID = x.ConnectionId, DisplayName = x.DisplayName});
                await Clients.Clients(members.Select(x => x.ConnectionId).ToList())
                    .SendAsync("ReceivePartyFound", new Party(){Members = userWrappers, RelicType = group.Key});
            }
        }

        private async Task UpdateParty(Assignment assignment)
        {
            var userWrappers = assignment.Members.Select(x => new UserWrapper()
            {
                ConnectionID = x.ConnectionId,
                DisplayName = x.DisplayName,
                Accepted = x.Accepted
            });
            var party = new Party()
            {
                Done = assignment.Done,
                Members = userWrappers,
                RelicType = assignment.RelicType
            };
            await Clients.Clients(assignment.Members.Select(x => x.ConnectionId).ToList())
                .SendAsync("ReceivePartyStatus", party);
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