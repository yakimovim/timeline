using EdlinSoftware.Timeline.Domain;
using EdlinSoftware.Timeline.Storage;
using Shouldly;
using System;
using System.Threading.Tasks;
using Timeline.Storage.Tests.TestFramework;
using Xunit;

namespace Timeline.Storage.Tests
{
    public class TimeRangeEventsSpecificationTests
        : IClassFixture<TimeRangeEventsSpecificationTestsFixture>
    {
        private readonly TimeRangeEventsSpecificationTestsFixture _fixture;

        public TimeRangeEventsSpecificationTests(
            TimeRangeEventsSpecificationTestsFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task Get_events_from_time_range_with_no_events()
        {
            // Act

            var eventsInTimeRange = await _fixture.EventsRepo.GetEventsAsync(
                new TimeRangeEventsSpecification(
                    new TimeRange(
                        ExactDateInfo.AnnoDomini(3000, 1, 1, 0),
                        ExactDateInfo.AnnoDomini(4000, 1, 1, 0)
                    )
                )
            );

            // Assert

            eventsInTimeRange.ShouldNotBeNull();
            eventsInTimeRange.ShouldBeEmpty();
        }

        [Fact]
        public async Task Get_events_from_time_range_with_full_events()
        {
            // Act

            var eventsInTimeRange = await _fixture.EventsRepo.GetEventsAsync(
                new TimeRangeEventsSpecification(
                    new TimeRange(
                        ExactDateInfo.BeforeChrist(20, 1, 1, 0),
                        ExactDateInfo.BeforeChrist(20, 12, 1, 0)
                    )
                )
            );

            // Assert

            eventsInTimeRange.ShouldNotBeNull();
            eventsInTimeRange.Count.ShouldBe(1);

            var @event = eventsInTimeRange[0];
            @event.ShouldNotBeNull();
            @event.Description.ShouldBe("B");
        }

        [Fact]
        public async Task Get_events_from_time_range_with_only_start_in_range()
        {
            // Act

            var eventsInTimeRange = await _fixture.EventsRepo.GetEventsAsync(
                new TimeRangeEventsSpecification(
                    new TimeRange(
                        ExactDateInfo.BeforeChrist(20, 1, 1, 0),
                        ExactDateInfo.AnnoDomini(1, 1, 1, 0)
                    )
                )
            );

            // Assert

            eventsInTimeRange.ShouldNotBeNull();
            eventsInTimeRange.Count.ShouldBe(2);

            eventsInTimeRange.ShouldContain(e => e.Description == "A");
            eventsInTimeRange.ShouldContain(e => e.Description == "B");
        }

        [Fact]
        public async Task Get_events_from_time_range_with_only_end_in_range()
        {
            // Act

            var now = DateTime.Now;

            var rangeEnd = new ExactDateInfo(
                Era.AnnoDomini,
                now.Year + 1,
                1,
                1,
                1
            );

            var eventsInTimeRange = await _fixture.EventsRepo.GetEventsAsync(
                new TimeRangeEventsSpecification(
                    new TimeRange(
                        ExactDateInfo.AnnoDomini(2020, 2, 1, 0),
                        rangeEnd
                    )
                )
            );

            // Assert

            eventsInTimeRange.ShouldNotBeNull();
            eventsInTimeRange.Count.ShouldBe(2);

            eventsInTimeRange.ShouldContain(e => e.Description == "C");
            eventsInTimeRange.ShouldContain(e => e.Description == "D");
        }
    }

    public class TimeRangeEventsSpecificationTestsFixture : IAsyncLifetime
    {
        public readonly TimelineContext Db;
        public readonly EventsRepository EventsRepo;

        public TimeRangeEventsSpecificationTestsFixture()
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
