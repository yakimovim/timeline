﻿using EdlinSoftware.Timeline.Domain;
using EdlinSoftware.Timeline.Storage;
using Shouldly;
using System;
using System.Threading.Tasks;
using Timeline.Storage.Tests.TestFramework;
using Xunit;

namespace Timeline.Storage.Tests
{
    using static EventsSpecification;

    public class OrEventsSpecificationTests
        : IClassFixture<OrEventsSpecificationTestsFixture>
    {
        private readonly OrEventsSpecificationTestsFixture _fixture;

        public OrEventsSpecificationTests(
            OrEventsSpecificationTestsFixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task Get_events_when_there_are_events_that_meet_both_conditions()
        {
            // Act
            var eventsInTimeRange = await _fixture.EventsRepo.GetEventsAsync(
                InRange(
                    ExactDateInfo.BeforeChrist(20, 1, 1, 0),
                    ExactDateInfo.BeforeChrist(20, 10, 1, 0)
                ).Or(
                    InRange(
                        ExactDateInfo.BeforeChrist(30, 1, 1, 0),
                        ExactDateInfo.BeforeChrist(10, 1, 1, 0)
                    )
                )
            );

            // Assert

            eventsInTimeRange.ShouldNotBeNull();
            eventsInTimeRange.Count.ShouldBe(1);
            eventsInTimeRange[0].Description.ShouldBe("B");
        }

        [Fact]
        public async Task Get_events_when_there_are_no_events_that_meet_both_conditions()
        {
            // Act
            var eventsInTimeRange = await _fixture.EventsRepo.GetEventsAsync(
                InRange(
                    ExactDateInfo.BeforeChrist(20, 1, 1, 0),
                    ExactDateInfo.BeforeChrist(20, 10, 1, 0)
                ).Or(
                    InRange(
                        ExactDateInfo.BeforeChrist(100, 2, 3, 1),
                        ExactDateInfo.BeforeChrist(100, 2, 3, 21)
                    )
                )
            );

            // Assert

            eventsInTimeRange.ShouldNotBeNull();
            eventsInTimeRange.Count.ShouldBe(2);
            eventsInTimeRange.ShouldContain(e => e.Description == "B");
            eventsInTimeRange.ShouldContain(e => e.Description == "E");
        }
    }

    public class OrEventsSpecificationTestsFixture : IAsyncLifetime
    {
        public readonly TimelineContext Db;
        public readonly EventsRepository EventsRepo;

        public OrEventsSpecificationTestsFixture()
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
