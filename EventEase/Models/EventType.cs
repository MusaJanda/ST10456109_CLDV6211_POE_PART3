using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class EventType
    {
        public int EventTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string? Name { get; set; } // CHANGED FROM TypeName to Name to match DbContext seeding

        public ICollection<Event>? Events { get; set; }

        // Initialize collections in the constructor to prevent NullReferenceExceptions
        public EventType()
        {
            Events = new List<Event>();
        }
    }
}