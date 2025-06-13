// EventEase.Controllers/EventController.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;
using EventEase.ViewModels;

namespace EventEase.Controllers
{
    public class EventController : Controller
    {
        private readonly EventEaseDBContext _context;
        private readonly IImageService _imageService;

        public EventController(EventEaseDBContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        // GET: Events
        public async Task<IActionResult> Index(
            string searchString,
            int? eventTypeId, // For Event Type filter
            int? venueId, // For Venue filter
            DateTime? startDate, // For Date Range filter
            DateTime? endDate, // For Date Range filter
            bool? availableVenuesOnly // For Venue Availability filter
            )
        {
            // Start with all events, including related Venue and EventType data
            var events = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .AsQueryable(); // Use AsQueryable() to build the query dynamically

            // Apply search string filter (Event Name or Description)
            if (!string.IsNullOrEmpty(searchString))
            {
                events = events.Where(e => e.EventName.Contains(searchString) || e.Description.Contains(searchString));
            }

            // Apply Event Type filter
            if (eventTypeId.HasValue && eventTypeId.Value > 0) // Check for a valid selection (not "-- All --")
            {
                events = events.Where(e => e.EventTypeId == eventTypeId.Value);
            }

            // Apply Venue filter
            if (venueId.HasValue && venueId.Value > 0) // Check for a valid selection
            {
                events = events.Where(e => e.VenueId == venueId.Value);
            }

            // Apply Date Range filter
            if (startDate.HasValue)
            {
                events = events.Where(e => e.EventDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                // To include events on the endDate itself, add one day to the end date.
                // Or you can make sure event.EventDate is only date part before comparison
                events = events.Where(e => e.EventDate <= endDate.Value.AddDays(1).Date);
            }

            if (availableVenuesOnly.HasValue && availableVenuesOnly.Value)
            {
                // Filter to show ONLY venues explicitly marked as "Available"
                events = events.Where(e => e.Venue.AvailabilityStatus == "Available");
            }


            // Order the results (e.g., by date)
            events = events.OrderBy(e => e.EventDate);

            // Create the EventIndexViewModel to hold filter options and the filtered events
            var viewModel = new EventIndexViewModel
            {
                Events = await events.ToListAsync(), // Execute the query
                SearchString = searchString,
                SelectedEventTypeId = eventTypeId,
                SelectedVenueId = venueId,
                StartDate = startDate,
                EndDate = endDate,
                AvailableVenuesOnly = availableVenuesOnly,

                // Populate filter dropdowns
                EventTypes = await _context.EventTypes
                                .OrderBy(et => et.Name)
                                .Select(et => new SelectListItem { Value = et.EventTypeId.ToString(), Text = et.Name })
                                .ToListAsync(),
                Venues = await _context.Venues
                                .OrderBy(v => v.VenueName)
                                .Select(v => new SelectListItem { Value = v.VenueId.ToString(), Text = v.VenueName })
                                .ToListAsync()
            };

            // Add "All" option to dropdowns at the beginning
            viewModel.EventTypes.Insert(0, new SelectListItem { Value = "", Text = "-- All Event Types --" });
            viewModel.Venues.Insert(0, new SelectListItem { Value = "", Text = "-- All Venues --" });


            return View(viewModel);
        }


        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType) // Include EventType
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            var eventViewModel = new EventViewModel
            {
                VenueList = _context.Venues.Select(v => new SelectListItem
                {
                    Value = v.VenueId.ToString(),
                    Text = v.VenueName
                }).ToList(),
                // Populate EventTypeList
                EventTypeList = _context.EventTypes.Select(et => new SelectListItem
                {
                    Value = et.EventTypeId.ToString(),
                    Text = et.Name
                }).ToList(),
                EventDate = DateTime.Today
            };

            return View(eventViewModel);
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel eventViewModel)
        {
            if (ModelState.IsValid)
            {
                var @event = new Event
                {
                    EventName = eventViewModel.EventName,
                    EventDate = eventViewModel.EventDate,
                    Description = eventViewModel.Description,
                    VenueId = eventViewModel.VenueId,
                    EventTypeId = eventViewModel.EventTypeId // Assign the selected EventTypeId
                };

                if (eventViewModel.ImageFile != null)
                {
                    @event.ImageUrl = await _imageService.UploadImageAsync(eventViewModel.ImageFile, "events");
                }

                _context.Events.Add(@event);
                await _context.SaveChangesAsync(); // First save to get EventId, then create booking

                var booking = new Booking
                {
                    EventId = @event.EventId,
                    VenueId = @event.VenueId,
                    BookingDate = @event.EventDate
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // ✅ Add success message
                TempData["SuccessMessage"] = $"Event \"{@event.EventName}\" was successfully created.";
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is not valid, re-populate lists before returning view
            eventViewModel.VenueList = _context.Venues.Select(v => new SelectListItem
            {
                Value = v.VenueId.ToString(),
                Text = v.VenueName
            }).ToList();
            eventViewModel.EventTypeList = _context.EventTypes.Select(et => new SelectListItem
            {
                Value = et.EventTypeId.ToString(),
                Text = et.Name
            }).ToList();

            return View(eventViewModel);
        }


        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();

            var eventViewModel = new EventViewModel
            {
                EventId = @event.EventId,
                EventName = @event.EventName,
                EventDate = @event.EventDate,
                Description = @event.Description,
                VenueId = @event.VenueId,
                EventTypeId = @event.EventTypeId, // Assign EventTypeId for the view model
                ExistingImageUrl = @event.ImageUrl,
                VenueList = _context.Venues.Select(v => new SelectListItem
                {
                    Value = v.VenueId.ToString(),
                    Text = v.VenueName,
                    Selected = v.VenueId == @event.VenueId
                }).ToList(),
                // Populate EventTypeList for edit
                EventTypeList = _context.EventTypes.Select(et => new SelectListItem
                {
                    Value = et.EventTypeId.ToString(),
                    Text = et.Name,
                    Selected = et.EventTypeId == @event.EventTypeId // Select current type
                }).ToList()
            };

            return View(eventViewModel);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventViewModel eventViewModel)
        {
            if (id != eventViewModel.EventId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var @event = await _context.Events.FindAsync(id);
                    if (@event == null)
                    {
                        return NotFound();
                    }

                    var oldVenueId = @event.VenueId;
                    var oldEventDate = @event.EventDate;

                    @event.EventName = eventViewModel.EventName;
                    @event.EventDate = eventViewModel.EventDate;
                    @event.Description = eventViewModel.Description;
                    @event.VenueId = eventViewModel.VenueId;
                    @event.EventTypeId = eventViewModel.EventTypeId; // Update EventTypeId

                    if (eventViewModel.ImageFile != null)
                    {
                        if (!string.IsNullOrEmpty(@event.ImageUrl) && !@event.ImageUrl.Contains("placeholder"))
                        {
                            await _imageService.DeleteImageAsync(@event.ImageUrl);
                        }
                        @event.ImageUrl = await _imageService.UploadImageAsync(eventViewModel.ImageFile, "events");
                    }
                    // If no new image and existing image was cleared, or if it was a placeholder
                    else if (string.IsNullOrEmpty(eventViewModel.ExistingImageUrl))
                    {
                        @event.ImageUrl = null; // Or set to your default placeholder if you prefer
                    }


                    _context.Update(@event);

                    var booking = await _context.Bookings
                        .FirstOrDefaultAsync(b => b.EventId == id && b.VenueId == oldVenueId && b.BookingDate == oldEventDate);

                    if (booking != null)
                    {
                        booking.VenueId = @event.VenueId;
                        booking.BookingDate = @event.EventDate;
                        _context.Update(booking);
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Event \"{@event.EventName}\" was successfully updated.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventViewModel.EventId))
                        return NotFound();
                    else
                        throw;
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException?.Message.Contains("duplicate") == true ||
                        ex.InnerException?.Message.Contains("UNIQUE") == true)
                    {
                        ModelState.AddModelError(string.Empty, "This venue is already booked for the specified date.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "An error occurred while saving the event.");
                        // Log the full exception for debugging in a real application
                        // _logger.LogError(ex, "Error saving event.");
                    }
                }
            }

            // If ModelState is not valid or an error occurred, re-populate lists before returning view
            eventViewModel.VenueList = _context.Venues.Select(v => new SelectListItem
            {
                Value = v.VenueId.ToString(),
                Text = v.VenueName
            }).ToList();
            eventViewModel.EventTypeList = _context.EventTypes.Select(et => new SelectListItem
            {
                Value = et.EventTypeId.ToString(),
                Text = et.Name
            }).ToList();
            return View(eventViewModel);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType) // Include EventType for display
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        // POST: Events/Delete/5
        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventToDelete = await _context.Events
                .Include(e => e.Bookings) // Ensure bookings are loaded
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventToDelete == null)
            {
                // If event not found, immediately redirect with an error message.
                TempData["ErrorMessage"] = "The event you tried to delete was not found.";
                return RedirectToAction(nameof(Index));
            }

            // --- FIRST CHECK: Does it have bookings? ---
            if (eventToDelete.Bookings != null && eventToDelete.Bookings.Any())
            {
                // If it has bookings, set the specific error message and REDIRECT IMMEDIATELY.
                TempData["ErrorMessage"] = $"Event \"{eventToDelete.EventName}\" cannot be deleted because it has existing bookings.";
                return RedirectToAction(nameof(Index)); // <-- This is the crucial redirect.
                                                        // No further database operations for this scenario.
            }

            // --- If no bookings, proceed with actual deletion attempt ---
            try
            {
                // Delete associated image if it's not a placeholder
                if (!string.IsNullOrEmpty(eventToDelete.ImageUrl) && !eventToDelete.ImageUrl.Contains("placeholder"))
                {
                    await _imageService.DeleteImageAsync(eventToDelete.ImageUrl);
                }

                _context.Events.Remove(eventToDelete);
                await _context.SaveChangesAsync(); // This should now only run if there are NO bookings.

                TempData["SuccessMessage"] = $"Event \"{eventToDelete.EventName}\" was successfully deleted.";
                return RedirectToAction(nameof(Index)); // Redirect to Index after successful deletion
            }
            catch (DbUpdateException ex)
            {
                // This catch block should ideally only be hit for other DB-related issues (e.g., integrity errors
                // not related to FKs on bookings, or if something was missed).
                // But just in case, ensure it redirects to Index.
                TempData["ErrorMessage"] = $"An error occurred while deleting the event \"{eventToDelete.EventName}\". It might still have associated booking.";
                // For debugging, you might want to log the full ex here:
                // _logger.LogError(ex, "Error deleting event {EventId}", id);
                return RedirectToAction(nameof(Index)); // Always redirect to Index on error
            }
            catch (Exception ex)
            {
                // General catch for any other unexpected errors
                TempData["ErrorMessage"] = $"An unexpected error occurred: {ex.Message}";
                // Log for debugging: _logger.LogError(ex, "Unexpected error deleting event {EventId}", id);
                return RedirectToAction(nameof(Index)); // Always redirect to Index on error
            }
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}