using EdlinSoftware.Timeline.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;

namespace Timeline.Storage.Tests.TestFramework
{
    public class TimelineContextProvider
    {
        public static TimelineContext GetDbContext(Action<TimelineContext> initialize = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TimelineContext>();

            optionsBuilder.UseInMemoryDatabase("TimelineInMemory", new InMemoryDatabaseRoot());

            var options = optionsBuilder.Options;

            var dbContext = new TimelineContext(options);

            InitializeDbContext(dbContext, initialize);

            return dbContext;
        }

        private static void InitializeDbContext(TimelineContext dbContext, Action<TimelineContext> initialize)
        {
            initialize?.Invoke(dbContext);
        }

    }
}
