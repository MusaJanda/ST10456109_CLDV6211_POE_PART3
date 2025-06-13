using EventEase.Models;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Data
{
    public class EventEaseDBContext : DbContext
    {
        public EventEaseDBContext(DbContextOptions<EventEaseDBContext> options)
            : base(options)
        {
        }

        public DbSet<EventEase.Models.Venue> Venues { get; set; }
        public DbSet<EventEase.Models.Event> Events { get; set; }
        public DbSet<EventEase.Models.Booking> Bookings { get; set; }
        public DbSet<EventType> EventTypes { get; set; } // New DbSet

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuring many-to-many relationship via Booking
            modelBuilder.Entity<Booking>()
                .HasKey(b => b.BookingId);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VenueId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // For validation: add a unique constraint for Event + Venue + Date combination
            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.EventId, b.VenueId, b.BookingDate })
                .IsUnique();


            // Configure relationships if necessary, e.g., for unique constraints or default values
            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.VenueId, b.BookingDate })
                .IsUnique();
            // Configure EventType
            modelBuilder.Entity<EventType>()
                .HasKey(et => et.EventTypeId);

            modelBuilder.Entity<EventType>()
                .Property(et => et.Name)
                .IsRequired()
                .HasMaxLength(50); // Or whatever length you want

            // Seed EventType data
            modelBuilder.Entity<EventType>().HasData(
                new EventType { EventTypeId = 1, Name = "Concert" },
                new EventType { EventTypeId = 2, Name = "Wedding" },
                new EventType { EventTypeId = 3, Name = "Sport" },
                new EventType { EventTypeId = 4, Name = "Conference" },
                new EventType { EventTypeId = 5, Name = "Festival" }
            );

            // Configure the relationship between Event and EventType
            modelBuilder.Entity<Event>()
                .HasOne(e => e.EventType)
                .WithMany(et => et.Events)
                .HasForeignKey(e => e.EventTypeId)
                .OnDelete(DeleteBehavior.Restrict); // Or Cascade, depending on your desired behavior

            // Add AvailabilityStatus to Venues if it's not already there
            modelBuilder.Entity<Venue>()
                .Property(v => v.AvailabilityStatus) // Assuming this is an enum or string
                .HasConversion<string>(); // If it's an enum, store as string
                                          // .IsRequired(); // Adjust nullability as needed

            // Add Status to Bookings if it's not already there
            modelBuilder.Entity<Booking>()
                .Property(b => b.Status) // Assuming this is an enum or string
                .HasConversion<string>(); // If it's an enum, store as string
                                          // .IsRequired(); // Adjust nullability as needed

            // IMPORTANT: Make sure your existing FKs use onDelete: ReferentialAction.NoAction or .Restrict where appropriate
            // Current migrations show Cascade for Events_Venues and NoAction for Bookings FKs.
            // Ensure consistency.
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany()
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.NoAction); // Matches your existing

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany()
                .HasForeignKey(b => b.VenueId)
                .OnDelete(DeleteBehavior.NoAction); // Matches your existing
            base.OnModelCreating(modelBuilder);
        }
    }
}
