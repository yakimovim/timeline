using EdlinSoftware.Timeline.Domain;
using EdlinSoftware.Timeline.Storage;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Storage.Tests.TestFramework;
using Xunit;

namespace Timeline.Storage.Tests
{
    using static EventsSpecification;

    public class IdsEventsSpecificationTests
        : IClassFixture<IdsEventsSpecificationTestsFixture>
    {
        private readonly IdsEventsSpecificationTestsFixture _fixture;

        public IdsEventsSpecificationTests(
            IdsEventsSpecificationTestsFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task Get_events_by_ids()
        {
            // Arrange

            var event1 = _fixture.Events.Single(e => e.Description == "B");
            var event2 = _fixture.Events.Single(e => e.Description == "D");

            // Act

            var events = await _fixture.EventsRepo.GetEventsAsync(
                Ids(event1.Id.Value, event2.Id.Value)
            );

            // Assert

            events.Count.ShouldBe(2);
            events.ShouldContain(e => e.Description == "B");
            events.ShouldContain(e => e.Description == "D");
        }
    }

    public class IdsEventsSpecificationTestsFixture : IAsyncLifetime
    {
        public readonly TimelineContext Db;
        public readonly EventsRepository EventsRepo;

        public readonly IReadOnlyList<Event<string, string>> Events;

        public IdsEventsSpecificationTestsFixture()
        {
            Db = TimelineContextProvider.GetDbContext();

            EventsRepo = new EventsRepository(Db);

            Events = new[]
            {
                new Event<string, string>("A", SpecificDate.BeforeChrist(10), SpecificDate.AnnoDomini(12)),
                new Event<string, string>("B", SpecificDate.BeforeChrist(20, 5)),
                new Event<string, string>("C", SpecificDate.AnnoDomini(2020, 1, 1), NowDate.Instance),
                new Event<string, string>("D", NowDate.Instance),
                new Event<string, string>("E", SpecificDate.BeforeChrist(100, 2, 3, 10), SpecificDate.BeforeChrist(100, 2, 3, 20)),
            };

        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            Db.Events.RemoveRange(Db.Events);

            await Db.SaveChangesAsync();

            await EventsRepo.SaveEventsAsync(Events);
        }
    }
}
