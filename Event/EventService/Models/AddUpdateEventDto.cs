﻿using System.ComponentModel.DataAnnotations;

namespace EventService.Models
{
    public class AddUpdateEventDto
    {
        public int EventId { get; set; } // Optional. If null or 0 → Add

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public int VenueId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int TotalCapacity { get; set; }

        [Range(0, int.MaxValue)]
        public int AvailableTickets { get; set; } 
    }
    public class EventResponseDto
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public int VenueId { get; set; }
        public int TotalCapacity { get; set; }
        public int AvailableTickets { get; set; }
    }
}
