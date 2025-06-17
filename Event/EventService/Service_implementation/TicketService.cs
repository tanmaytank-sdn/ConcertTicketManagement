using EventService.Entity;
using EventService.EventDb;
using EventService.Interface;
using EventService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventService.Service_implementation
{
    public class TicketService : ITicketTypeService
    {
        private readonly EventDbContext _context;

        public TicketService(EventDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string? ErrorMessage, TicketType CreatedTicket)> AddTicketTypeAsync(AddTicketTypeDto dto)
        {
            var ev = await _context.Event.Include(e => e.TicketTypes).FirstOrDefaultAsync(e => e.EventId == dto.EventId);

            if (ev == null)
                return (false, "Event not found.", null);

            var existingTickets = ev.TicketTypes ?? new List<TicketType>();
            int totalExisting = existingTickets.Sum(t => t.Quantity);
            int totalAfterAdd = totalExisting + dto.Quantity;

            if (totalAfterAdd > ev.TotalCapacity)
                return (false, $"Total ticket quantity ({totalAfterAdd}) exceeds event capacity ({ev.TotalCapacity}).", null);

            var newTicket = new TicketType
            {
                EventId = dto.EventId,
                Name = dto.Name,
                Quantity = dto.Quantity,
                Price = dto.Price
            };

            _context.TicketType.Add(newTicket);
            await _context.SaveChangesAsync();

            return (true, null, newTicket);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateTicketTypeAsync(int ticketId, UpdateTicketTypeDto dto)
        {
            var ticket = await _context.TicketType
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
                return (false, "Ticket type not found.");

            var ev = ticket.Event;
            if (ev == null)
                return (false, "Associated event not found.");

            var allEventTickets = await _context.TicketType
                .Where(x => x.EventId == ticket.EventId)
                .ToListAsync();

            int totalOtherTickets = allEventTickets
                .Where(t => t.Id != ticketId)
                .Sum(t => t.Quantity);

            int newTotal = totalOtherTickets + dto.Quantity;
            if (newTotal > ev.TotalCapacity)
                return (false, $"Updated quantity would exceed event capacity ({ev.TotalCapacity}).");

            ticket.Name = dto.Name;
            ticket.Quantity = dto.Quantity;
            ticket.Price = dto.Price;

            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}
