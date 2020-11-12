using Microsoft.EntityFrameworkCore;

namespace Timeline.Storage
{
    /// <summary>
    /// Database context for timeline storage.
    /// </summary>
    public class TimelineContext : DbContext
    {
        /// <summary>
        /// Stores places hierarchy.
        /// </summary>
        public DbSet<PlaceHierarchy> Places { get; set; }

        public TimelineContext(DbContextOptions<TimelineContext> options)
            : base(options)
        { }
    }
}
