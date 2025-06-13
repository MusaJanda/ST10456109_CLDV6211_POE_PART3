using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int VenueId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; }

        public string? Status { get; set; } // Consider making this an enum for strict control

        // Navigation properties
        // REMOVED: [ForeignKey("EventId")] - EF Core will infer this correctly
        public virtual Event? Event { get; set; }

        // REMOVED: [ForeignKey("VenueId")] - EF Core will infer this correctly
        public virtual Venue? Venue { get; set; }
    }
}