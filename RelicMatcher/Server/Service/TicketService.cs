using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RelicMatcher.Shared;

namespace RelicMatcher.Server.Service
{
    public class TicketService
    {
        private readonly ILogger<TicketService> _logger;
        public TicketService(ILogger<TicketService> logger)
        {
            _logger = logger;
        }
        private Dictionary<Guid, Ticket> Tickets { get; set; } = new Dictionary<Guid, Ticket>();
        //private HashSet<Ticket> Index { get; set; } = new HashSet<Ticket>();

        public Ticket CreateTicket(Guid userGuid, string displayName, RelicType relicType)
        {
            var ticket = new Ticket(userGuid, displayName, relicType);
            Tickets[userGuid] = ticket;
            return ticket;
        }

        public Ticket? GetTicket(Guid userGuid)
        {
            _logger.LogDebug(Tickets.Keys.ToString());
            return Tickets.GetValueOrDefault(userGuid);
        }

        public void DeleteTicket(Guid userGuid)
        {
            if (Tickets.TryGetValue(userGuid, out var ticket))
            {
                ticket.Assignment?.Members.Remove(ticket);
                Tickets.Remove(userGuid);
            }
        }

        public IEnumerable<Ticket> GetTickets()
        {
            return Tickets.Values;
        }
        //public void IndexTicket(Ticket ticket)
        //{

        //}

        public IReadOnlyCollection<Ticket> GetIndexedTickets()
        {
            return Tickets.Values.Where(x => x.Assignment == null && x.Active).ToImmutableList();
        }

        public void SetTicketActiveState(Ticket ticket, bool active)
        {
            ticket.Active = active;
        }
        public Assignment CreateAssignment(ICollection<Ticket> tickets, RelicType relicType)
        {
            var assignment = new Assignment(relicType, tickets.ToList());
            foreach (var ticket in tickets)
            {
                ticket.Assignment = assignment;
                ticket.Accepted = false;
            }

            return assignment;
        }


    }
}
