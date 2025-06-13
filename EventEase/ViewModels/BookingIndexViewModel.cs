using EventEase.Models; // Make sure to include your models namespace
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectListItem

namespace EventEase.ViewModels
{
    public class BookingIndexViewModel
    {
        public IEnumerable<Booking> Bookings { get; set; } // The list of bookings to display

        // Filter properties
        public int? SelectedEventTypeId { get; set; }
        public DateTime? SelectedDateFrom { get; set; }
        public DateTime? SelectedDateTo { get; set; }
        public string? SelectedVenueAvailabilityStatus { get; set; } // Matches your string property in Venue

        // Filter options for dropdowns
        public SelectList? EventTypes { get; set; } // For EventType dropdown
        public SelectList? VenueAvailabilityStatuses { get; set; } // For Venue Availability dropdown

        // Constructor to initialize collections if needed, though not strictly required for this ViewModel
        public BookingIndexViewModel()
        {
            Bookings = new List<Booking>();
        }
    }
}