using EventService.Models;

namespace EventService.Interface
{
    public interface IEventService
    {
        Task<(bool Success, string? ErrorMessage, object? Result)> AddOrUpdateEventAsync(AddUpdateEventDto dto);
        Task<List<EventResponseDto>> GetAllEventsAsync();
        Task<EventResponseDto> GetEventByIdAsync(int id);
        Task<List<TicketTypeModel>> GetEventTicketTypesAsync(int eventId);
        Task<TicketTypeModel> GetSpecificTicketTypeAsync(int eventId, int ticketTypeId);
        Task<(bool Success, string? ErrorMessage)> DeleteEventAsync(int id);
    }
}
