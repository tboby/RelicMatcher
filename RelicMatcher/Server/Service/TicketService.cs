using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using RelicMatcher.Shared;

namespace RelicMatcher.Server.Service
{
    public class TicketService
    {
        private Dictionary<string, Ticket> Tickets { get; set; } = new Dictionary<string, Ticket>();
        //private HashSet<Ticket> Index { get; set; } = new HashSet<Ticket>();

        public Ticket CreateTicket(string connectionId, string displayName, RelicType relicType)
        {
            var ticket = new Ticket(connectionId, displayName, relicType);
            Tickets[connectionId] = ticket;
            return ticket;
        }

        public Ticket? GetTicket(string connectionID)
        {
            return Tickets.GetValueOrDefault(connectionID);
        }

        public void DeleteTicket(string connectionID)
        {
            if (Tickets.TryGetValue(connectionID, out var ticket))
            {
                ticket.Assignment?.Members.Remove(ticket);
                Tickets.Remove(connectionID);
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
            return Tickets.Values.Where(x => x.Assignment == null).ToImmutableList();
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
