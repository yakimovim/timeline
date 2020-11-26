using Microsoft.EntityFrameworkCore;

namespace EdlinSoftware.Timeline.Storage
{
    /// <summary>
    /// Database context for timeline storage.
    /// </summary>
    public class TimelineContext : DbContext
    {
        /// <summary>
        /// Places hierarchy nodes.
        /// </summary>
        public DbSet<Place> Places { get; set; }

        /// <summary>
        /// Events.
        /// </summary>
        public DbSet<Event> Events { get; set; }

        /// <summary>
        /// Events sets.
        /// </summary>
        public DbSet<EventsSet> EventSets { get; set; }

        /// <summary>
        /// Connection between events and sets.
        /// </summary>
        public DbSet<EventInSet> EventsInSets { get; set; }

        public TimelineContext(DbContextOptions<TimelineContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Place)
                .WithMany()
                .HasForeignKey(e => e.PlaceId);

            modelBuilder.Entity<EventInSet>()
                .HasKey(e => new { e.EventId, e.SetId });
        }
    }
}
