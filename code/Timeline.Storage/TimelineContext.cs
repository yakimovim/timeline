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
        public DbSet<PlaceHierarchy> Places { get; set; }

        /// <summary>
        /// Events.
        /// </summary>
        public DbSet<Event> Events { get; set; }

        public TimelineContext(DbContextOptions<TimelineContext> options)
            : base(options)
        { }
    }
}
