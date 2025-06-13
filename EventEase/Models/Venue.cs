using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// using EventEase.Models; // This using is redundant as it's already in the same namespace

namespace EventEase.Models
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Venue Name")]
        public string? VenueName { get; set; }

        [Required]
        [StringLength(200)]
        public string? Location { get; set; }

        [Range(1, 100000)]
        public int Capacity { get; set; }

        [Display(Name = "Venue Image")]
        public string? ImageUrl { get; set; }

        public string AvailabilityStatus { get; set; } = "Available"; // Or "Unknown" if you chose that default

        // Navigation properties
        public virtual ICollection<Event>? Events { get; set; } // Inverse of Event.Venue
        public virtual ICollection<Booking>? Bookings { get; set; }

        // Initialize collections in the constructor
        public Venue()
        {
            Events = new List<Event>();
            Bookings = new List<Booking>();
        }
    }
}