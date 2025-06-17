using Microsoft.EntityFrameworkCore;
using TicketBookingService.Abstract;
using TicketBookingService.Common;
using TicketBookingService.Entity;
using TicketBookingService.Interfaces;
using TicketBookingService.Models;
using TicketBookingService.TicketBookingDbContext;

namespace TicketBookingService.Services
{
    public class TicketService : ITicketService
    {
        private readonly TicketDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly IRabbitMqPublisher _publisher;

        public TicketService(TicketDbContext context, ICurrentUserService currentUser, IHttpClientFactory httpClientFactory, IConfiguration config, IRabbitMqPublisher publisher)
        {
            _context = context;
            _currentUser = currentUser;
            _httpClientFactory = httpClientFactory;
            _config = config;
            _publisher = publisher;
        }

        public async Task<(bool Success, string? ErrorMessage, List<AvailableTicketDto>? Tickets)> GetAvailableTicketsAsync(int eventId)
        {
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _config["EventServiceBaseUrl"];
            var response = await client.GetAsync($"{baseUrl}/api/Event/GetEventTicketType/{eventId}");

            if (!response.IsSuccessStatusCode)
                return (false, "Unable to fetch ticket types from EventService.", null);

            var ticketTypes = await response.Content.ReadFromJsonAsync<List<TicketTypeFromEventServiceModel>>();
            var availability = new List<AvailableTicketDto>();

            foreach (var ticketType in ticketTypes)
            {
                var used = await _context.Ticket
                    .Where(t => t.TicketTypeId == ticketType.Id &&
                                (t.Status == TicketStatusEnum.Purchased ||
                                 (t.Status == TicketStatusEnum.Reserved && t.ExpiresAt > DateTime.UtcNow)))
                    .SumAsync(t => t.Quantity);

                availability.Add(new AvailableTicketDto
                {
                    TicketTypeId = ticketType.Id,
                    Name = ticketType.Name,
                    Price = ticketType.Price,
                    OriginalQuantity = ticketType.Quantity,
                    Remaining = Math.Max(0, ticketType.Quantity - used)
                });
            }

            return (true, null, availability);
        }

        public async Task<(bool Success, string? ErrorMessage, ReserveTicketResponse? Response)> ReserveTicketAsync(ReserveTicketRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _config["EventServiceBaseUrl"];
            var ticketResp = await client.GetAsync($"{baseUrl}/api/Event/GetEventTicketType/{request.EventId}/{request.TicketTypeId}");

            if (!ticketResp.IsSuccessStatusCode)
                return (false, "Invalid TicketTypeId.", null);

            var ticketType = await ticketResp.Content.ReadFromJsonAsync<TicketTypeFromEventServiceModel>();
            if (ticketType == null)
                return (false, "Ticket type not found.", null);

            var used = await _context.Ticket
                .Where(t => t.TicketTypeId == request.TicketTypeId &&
                            (t.Status == TicketStatusEnum.Purchased ||
                             (t.Status == TicketStatusEnum.Reserved && t.ExpiresAt > DateTime.Now)))
                .SumAsync(t => t.Quantity);

            var remaining = ticketType.Quantity - used;
            if (request.Quantity > remaining)
                return (false, $"Only {remaining} tickets left for this type.", null);

            var code = Guid.NewGuid().ToString("N");
            var ticket = new Ticket
            {
                UserId = _currentUser.UserId,
                TicketTypeId = ticketType.Id,
                TicketTypeName = ticketType.Name,
                Quantity = request.Quantity,
                PricePerTicket = ticketType.Price,
                Status = TicketStatusEnum.Reserved,
                ReservedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(10),
                ReservationCode = code
            };

            _context.Ticket.Add(ticket);
            await _context.SaveChangesAsync();

            return (true, null, new ReserveTicketResponse
            {
                TicketId = ticket.Id,
                TicketTypeName = ticket.TicketTypeName,
                PricePerTicket = ticket.PricePerTicket,
                Quantity = ticket.Quantity,
                ExpiresAt = ticket.ExpiresAt,
                ReservationCode = code
            });
        }

        public async Task<(bool Success, string? ErrorMessage, object? Response)> ConfirmTicketAsync(ConfirmTicketRequest request)
        {
            var ticket = await _context.Ticket
                .FirstOrDefaultAsync(t =>
                    t.ReservationCode == request.ReservationCode &&
                    t.UserId == _currentUser.UserId);

            if (ticket == null)
                return (false, "Ticket not found for this user.", null);

            if (ticket.Status != TicketStatusEnum.Reserved)
                return (false, "Only reserved tickets can be confirmed.", null);

            if (ticket.ExpiresAt <= DateTime.Now)
            {
                ticket.Status = TicketStatusEnum.Cancelled;
                await _context.SaveChangesAsync();
                return (false, "Reservation has expired and is now cancelled.", null);
            }

            ticket.Status = TicketStatusEnum.Purchased;
            ticket.PurchasedAt = DateTime.Now;

            _publisher.Publish(new EmailMessageModel
            {
                To = _currentUser.Email,
                Subject = "Ticket Confirmation",
                Body = $"Hi {_currentUser.FullName},\n\nThanks for booking {ticket.Quantity}x {ticket.TicketTypeName}.\nTicket ID: {ticket.Id}"
            }, "emailQueue");

            await _context.SaveChangesAsync();

            return (true, null, new
            {
                message = "Ticket successfully purchased.",
                ticketType = ticket.TicketTypeName,
                quantity = ticket.Quantity,
                totalPrice = ticket.Quantity * ticket.PricePerTicket,
                purchasedAt = ticket.PurchasedAt
            });
        }
    }
}
