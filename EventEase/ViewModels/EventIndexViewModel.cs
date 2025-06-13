using EventEase.Models;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectListItem
using System.Collections.Generic;
using System;

namespace EventEase.ViewModels
{
    public class EventIndexViewModel
    {
        // This list will hold the events to display on the index page, AFTER filtering
        public List<Event> Events { get; set; } = new List<Event>();

        // These properties will hold the user's selected filter criteria
        public string? SearchString { get; set; }
        public int? SelectedEventTypeId { get; set; } // To pre-select the chosen event type in the dropdown
        public int? SelectedVenueId { get; set; }     // To pre-select the chosen venue in the dropdown
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? AvailableVenuesOnly { get; set; }

        // These properties will hold the data to populate the filter dropdowns in the UI
        public List<SelectListItem> EventTypes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Venues { get; set; } = new List<SelectListItem>();
    }
}