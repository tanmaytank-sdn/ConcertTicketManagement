using EventService.Entity;
using EventService.Models;

namespace EventService.Interface
{
    public interface ITicketTypeService
    {
        Task<(bool Success, string? ErrorMessage, TicketType? CreatedTicket)> AddTicketTypeAsync(AddTicketTypeDto dto);
        Task<(bool Success, string? ErrorMessage)> UpdateTicketTypeAsync(int ticketId, UpdateTicketTypeDto dto);
    }
}
