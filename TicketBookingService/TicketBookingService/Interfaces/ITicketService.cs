using TicketBookingService.Models;

namespace TicketBookingService.Interfaces
{
    public interface ITicketService
    {
        Task<(bool Success, string? ErrorMessage, List<AvailableTicketDto>? Tickets)> GetAvailableTicketsAsync(int eventId);
        Task<(bool Success, string? ErrorMessage, ReserveTicketResponse? Response)> ReserveTicketAsync(ReserveTicketRequest request);
        Task<(bool Success, string? ErrorMessage, object? Response)> ConfirmTicketAsync(ConfirmTicketRequest request);
    }
}
