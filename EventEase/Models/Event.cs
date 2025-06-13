using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// using EventEase.Models; // This using is redundant as it's already in the same namespace

namespace EventEase.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Event Name")]
        public string? EventName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [Required]
        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Venue")]
        public int VenueId { get; set; }

        [Display(Name = "Event Image")]
        public string? ImageUrl { get; set; }

        // Navigation properties
        [ForeignKey("VenueId")]
        public virtual Venue? Venue { get; set; }

        // New: Foreign Key for EventType
        public int EventTypeId { get; set; }
        public EventType? EventType { get; set; } // Navigation property

        public virtual ICollection<Booking>? Bookings { get; set; }

        // Initialize collections in the constructor
        public Event()
        {
            Bookings = new List<Booking>();
        }
    }
}