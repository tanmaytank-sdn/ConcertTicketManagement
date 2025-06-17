using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventService.Entity
{
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime EventDate { get; set; }

        public int VenueId { get; set; }

        public int TotalCapacity { get; set; }

        public int AvailableTickets { get; set; }

        // property: One Event → Many TicketTypes
        public ICollection<TicketType>? TicketTypes { get; set; }
    }
}
