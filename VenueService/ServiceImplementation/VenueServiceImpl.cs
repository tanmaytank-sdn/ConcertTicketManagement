using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TicketBookingService.Entity;
using VenueService.Interface;
using VenueService.RequestDTO;
using VenueService.ResponseModels;
using VenueService.Venue_DbContext;

namespace VenueService.Services
{
    public class VenueServiceImpl : IVenueService
    {
        private readonly VenueDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;

        public VenueServiceImpl(VenueDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }

        private bool IsAdmin()
        {
            var role = _contextAccessor.HttpContext?.User?
                .Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            return role == "Admin";
        }

        public async Task<(bool Success, string? ErrorMessage)> AddOrUpdateVenueAsync(AddOrUpdateVenueRequest request)
        {
            if (!IsAdmin())
                return (false, "You are not authorized to perform this action.");

            try
            {
                if (string.IsNullOrWhiteSpace(request.Name) || request.MaxCapacity <= 0)
                    return (false, "Venue name and valid capacity are required.");

                var existingVenue = await _context.Venue
                    .FirstOrDefaultAsync(v => v.VenueId == request.VenueId);

                if (existingVenue != null && request.VenueId > 0)
                {
                    existingVenue.Name = request.Name;
                    existingVenue.Address = request.Address;
                    existingVenue.MaxCapacity = request.MaxCapacity;

                    _context.Venue.Update(existingVenue);
                }
                else if (existingVenue == null)
                {
                    var newVenue = new Venue
                    {
                        Name = request.Name,
                        Address = request.Address,
                        MaxCapacity = request.MaxCapacity
                    };

                    _context.Venue.Add(newVenue);
                }
                else
                {
                    return (false, "Venue already exists.");
                }

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, "Internal Server Error");
            }
        }

        public async Task<(bool Success, List<VenueResponseModel>? Data, string? ErrorMessage)> GetAllVenuesAsync()
        {
            if (!IsAdmin())
                return (false, null, "You are not authorized to view this data.");

            var venues = await _context.Venue.Select(v => new VenueResponseModel
            {
                VenueId = v.VenueId,
                Name = v.Name,
                Address = v.Address,
                MaxCapacity = v.MaxCapacity
            }).ToListAsync();

            return (true, venues, null);
        }
        //await _context.Venue.ToListAsync();

        public async Task<(bool Success, VenueResponseModel? Data, string? ErrorMessage)> GetVenueByIdAsync(int id)
        {
            if (!IsAdmin())
                return (false, null, "You are not authorized to view this data.");

            var venuedata = await _context.Venue.Where(v=>v.VenueId == id).Select(v => new VenueResponseModel
            {
                VenueId = v.VenueId,
                Name = v.Name,
                Address = v.Address,
                MaxCapacity = v.MaxCapacity
            }).FirstOrDefaultAsync();

            return (true, venuedata, null);
        }

    }
}
