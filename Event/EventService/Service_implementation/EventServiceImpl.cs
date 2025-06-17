using EventService.Entity;
using EventService.EventDb;
using EventService.Interface;
using EventService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventService.Service_implementation
{
    public class EventServiceImpl : IEventService
    {
        private readonly EventDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public EventServiceImpl(EventDbContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<(bool Success, string? ErrorMessage, object? Result)> AddOrUpdateEventAsync(AddUpdateEventDto dto)
        {
            if (dto == null)
                return (false, "Invalid event data.", null);

            var client = _httpClientFactory.CreateClient();
            var venueBaseUrl = _config["VenueServiceBaseUrl"];
            var venueResp = await client.GetAsync($"{venueBaseUrl}/api/Venue/GetVenueById/{dto.VenueId}");

            if (!venueResp.IsSuccessStatusCode)
                return (false, "Unable to verify venue.", null);

            var venue = await venueResp.Content.ReadFromJsonAsync<VenueResponseModel>();
            if (venue == null)
                return (false, "Venue not found.", null);

            if (dto.TotalCapacity > venue.MaxCapacity)
                return (false, $"Requested capacity ({dto.TotalCapacity}) exceeds venue limit ({venue.MaxCapacity}).", null);

            if (dto.EventId > 0)
            {
                var ev = await _context.Event.FindAsync(dto.EventId);
                if (ev == null)
                    return (false, $"Event with ID {dto.EventId} not found.", null);

                ev.Title = dto.Title;
                ev.Description = dto.Description;
                ev.EventDate = dto.EventDate;
                ev.VenueId = dto.VenueId;
                ev.TotalCapacity = dto.TotalCapacity;
                ev.AvailableTickets = dto.AvailableTickets;

                await _context.SaveChangesAsync();
                return (true, null, "Event updated successfully.");
            }
            else
            {
                var ev = new Event
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    EventDate = dto.EventDate,
                    VenueId = dto.VenueId,
                    TotalCapacity = dto.TotalCapacity,
                    AvailableTickets = dto.TotalCapacity
                };

                _context.Event.Add(ev);
                await _context.SaveChangesAsync();

                var response = new EventResponseDto
                {
                    EventId = ev.EventId,
                    Title = ev.Title,
                    Description = ev.Description,
                    EventDate = ev.EventDate,
                    VenueId = ev.VenueId,
                    TotalCapacity = ev.TotalCapacity,
                    AvailableTickets = ev.AvailableTickets
                };

                return (true, null, response);
            }
        }

        public async Task<List<EventResponseDto>> GetAllEventsAsync()
        {
            return await _context.Event.Select(ev => new EventResponseDto
            {
                EventId = ev.EventId,
                Title = ev.Title,
                Description = ev.Description,
                EventDate = ev.EventDate,
                VenueId = ev.VenueId,
                TotalCapacity = ev.TotalCapacity,
                AvailableTickets = ev.AvailableTickets
            }).ToListAsync();
        }

        public async Task<EventResponseDto> GetEventByIdAsync(int id)
        {
            var ev = await _context.Event.FindAsync(id);
            if (ev == null) return null;

            return new EventResponseDto
            {
                EventId = ev.EventId,
                Title = ev.Title,
                Description = ev.Description,
                EventDate = ev.EventDate,
                VenueId = ev.VenueId,
                TotalCapacity = ev.TotalCapacity,
                AvailableTickets = ev.AvailableTickets
            };
        }

        public async Task<List<TicketTypeModel>> GetEventTicketTypesAsync(int eventId)
        {
            return await _context.TicketType
                .Where(x => x.EventId == eventId)
                .Select(y => new TicketTypeModel
                {
                    Id = y.Id,
                    Name = y.Name,
                    Price = y.Price,
                    Quantity = y.Quantity
                }).ToListAsync();
        }

        public async Task<TicketTypeModel> GetSpecificTicketTypeAsync(int eventId, int ticketTypeId)
        {
            var ticketdata = await _context.TicketType
                .Where(x => x.EventId == eventId && x.Id == ticketTypeId)
                .Select(y => new TicketTypeModel
                {
                    Id = y.Id,
                    Name = y.Name,
                    Price = y.Price,
                    Quantity = y.Quantity
                }).FirstOrDefaultAsync();

            return ticketdata;
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteEventAsync(int id)
        {
            var ev = await _context.Event.FindAsync(id);
            if (ev == null)
                return (false, "Event not found.");

            _context.Event.Remove(ev);
            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}
