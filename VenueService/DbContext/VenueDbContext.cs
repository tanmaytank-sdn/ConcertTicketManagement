using Microsoft.EntityFrameworkCore;
using TicketBookingService.Entity;

namespace VenueService.Venue_DbContext
{
    public class VenueDbContext : DbContext
    {
        public VenueDbContext(DbContextOptions<VenueDbContext> options) : base(options)
        {
        }

        public DbSet<Venue> Venue { get; set; }
    }
}
