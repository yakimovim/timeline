using EdlinSoftware.Timeline.Domain;
using EdlinSoftware.Timeline.Storage;
using Shouldly;
using System;
using System.Threading.Tasks;
using Timeline.Storage.Tests.TestFramework;
using Xunit;

namespace Timeline.Storage.Tests
{
    using static EventsSpecification;

    public class NotEventsSpecificationTests
        : IClassFixture<NotEventsSpecificationTestsFixture>
    {
        private readonly NotEventsSpecificationTestsFixture _fixture;

        public NotEventsSpecificationTests(
            NotEventsSpecificationTestsFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task Revert_specification()
        {
            // Act

            var eventsInTimeRange = await _fixture.EventsRepo.GetEventsAsync(
                Not(
                    InRange(
                        ExactDateInfo.BeforeChrist(20, 1, 1, 0),
                        ExactDateInfo.AnnoDomini(1, 1, 1, 0)
                    )
                )
            );

            // Assert

            eventsInTimeRange.ShouldNotBeNull();
            eventsInTimeRange.Count.ShouldBe(3);

            eventsInTimeRange.ShouldContain(e => e.Description == "C");
            eventsInTimeRange.ShouldContain(e => e.Description == "D");
            eventsInTimeRange.ShouldContain(e => e.Description == "E");
        }
    }

    public class NotEventsSpecificationTestsFixture : IAsyncLifetime
    {
        public readonly TimelineContext Db;
        public readonly EventsRepository EventsRepo;

        public NotEventsSpecificationTestsFixture()
        {
            Db = TimelineContextProvider.GetDbContext();

            EventsRepo = new EventsRepository(Db);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            Db.Events.RemoveRange(Db.Events);

            await Db.SaveChangesAsync();

            var events = new[]
                {
                    new Event<string, string>("A", SpecificDate.BeforeChrist(10), SpecificDate.AnnoDomini(12)),
                    new Event<string, string>("B", SpecificDate.BeforeChrist(20, 5)),
                    new Event<string, string>("C", SpecificDate.AnnoDomini(2020, 1, 1), NowDate.Instance),
                    new Event<string, string>("D", NowDate.Instance),
                    new Event<string, string>("E", SpecificDate.BeforeChrist(100, 2, 3, 10), SpecificDate.BeforeChrist(100, 2, 3, 20)),
                };

            await EventsRepo.SaveEventsAsync(events);
        }
    }
}
