using TicketBookingService.Entity;
using VenueService.RequestDTO;
using VenueService.ResponseModels;

namespace VenueService.Interface
{
    public interface IVenueService
    {
        Task<(bool Success, VenueResponseModel? Data, string? ErrorMessage)> GetVenueByIdAsync(int id);
        Task<(bool Success, List<VenueResponseModel>? Data, string? ErrorMessage)> GetAllVenuesAsync();
        Task<(bool Success, string? ErrorMessage)> AddOrUpdateVenueAsync(AddOrUpdateVenueRequest request);
    }
}
