﻿using EdlinSoftware.Timeline.Domain;
using EdlinSoftware.Timeline.Storage;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timeline.Storage.Tests.TestFramework;
using Xunit;

namespace Timeline.Storage.Tests
{
    public class EventsRepositoryTests
    {
        private readonly TimelineContext _db;
        private readonly EventsRepository _repo;

        public EventsRepositoryTests()
        {
            _db = TimelineContextProvider.GetDbContext();

            _repo = new EventsRepository(_db);
        }

        [Fact]
        public async Task Save_new_events()
        {
            // Arrange

            _db.Events.RemoveRange(_db.Events);

            await _db.SaveChangesAsync();

            // Act

            var events = new[]
                {
                    new Event<string, string>("A", SpecificDate.BeforeChrist(10), SpecificDate.AnnoDomini(12)),
                    new Event<string, string>("B", SpecificDate.BeforeChrist(20, 5)),
                    new Event<string, string>("C", SpecificDate.AnnoDomini(2020, 1, 1), NowDate.Instance),
                    new Event<string, string>("D", NowDate.Instance),
                    new Event<string, string>("E", SpecificDate.BeforeChrist(100, 2, 3, 10), SpecificDate.BeforeChrist(100, 2, 3, 20)),
                };

            await _repo.SaveEventsAsync(events);

            // Assert

            var storedEvents = await _db.Events.ToArrayAsync();

            storedEvents.ShouldNotBeNull();
            storedEvents.Length.ShouldBe(5);

            var @event = storedEvents.FirstOrDefault(e => e.Content == "A");
            @event.ShouldNotBeNull();
            @event.StartIsCurrent.ShouldBeFalse();
            @event.StartDuration.ShouldNotBeNull();
            @event.StartDuration.Value.ShouldBe(Duration.GetDurationFromChristBirth(SpecificDate.BeforeChrist(10)));
            @event.StartNullPart.ShouldNotBeNull();
            @event.StartNullPart.ShouldBe(NullableDateParts.Month);
            @event.EndIsCurrent.ShouldNotBeNull();
            @event.EndIsCurrent.Value.ShouldBeFalse();
            @event.EndDuration.ShouldNotBeNull();
            @event.EndDuration.Value.ShouldBe(Duration.GetDurationFromChristBirth(SpecificDate.AnnoDomini(12)));
            @event.EndNullPart.ShouldNotBeNull();
            @event.EndNullPart.Value.ShouldBe(NullableDateParts.Month);

            @event = storedEvents.FirstOrDefault(e => e.Content == "B");
            @event.ShouldNotBeNull();
            @event.StartIsCurrent.ShouldBeFalse();
            @event.StartDuration.ShouldNotBeNull();
            @event.StartDuration.Value.ShouldBe(Duration.GetDurationFromChristBirth(SpecificDate.BeforeChrist(20, 5)));
            @event.StartNullPart.ShouldNotBeNull();
            @event.StartNullPart.ShouldBe(NullableDateParts.Day);
            @event.EndIsCurrent.ShouldBeNull();
            @event.EndDuration.ShouldBeNull();
            @event.EndNullPart.ShouldBeNull();

            @event = storedEvents.FirstOrDefault(e => e.Content == "C");
            @event.ShouldNotBeNull();
            @event.StartIsCurrent.ShouldBeFalse();
            @event.StartDuration.ShouldNotBeNull();
            @event.StartDuration.Value.ShouldBe(Duration.GetDurationFromChristBirth(SpecificDate.AnnoDomini(2020, 1, 1)));
            @event.StartNullPart.ShouldNotBeNull();
            @event.StartNullPart.ShouldBe(NullableDateParts.Hour);
            @event.EndIsCurrent.ShouldNotBeNull();
            @event.EndIsCurrent.Value.ShouldBeTrue();
            @event.EndDuration.ShouldBeNull();
            @event.EndNullPart.ShouldBeNull();

            @event = storedEvents.FirstOrDefault(e => e.Content == "D");
            @event.ShouldNotBeNull();
            @event.StartIsCurrent.ShouldBeTrue();
            @event.StartDuration.ShouldBeNull();
            @event.StartNullPart.ShouldBeNull();
            @event.EndIsCurrent.ShouldBeNull();
            @event.EndDuration.ShouldBeNull();
            @event.EndNullPart.ShouldBeNull();

            @event = storedEvents.FirstOrDefault(e => e.Content == "E");
            @event.ShouldNotBeNull();
            @event.StartIsCurrent.ShouldBeFalse();
            @event.StartDuration.ShouldNotBeNull();
            @event.StartDuration.Value.ShouldBe(Duration.GetDurationFromChristBirth(SpecificDate.BeforeChrist(100, 2, 3, 10)));
            @event.StartNullPart.ShouldNotBeNull();
            @event.StartNullPart.ShouldBe(NullableDateParts.Nothing);
            @event.EndIsCurrent.ShouldNotBeNull();
            @event.EndIsCurrent.Value.ShouldBeFalse();
            @event.EndDuration.ShouldNotBeNull();
            @event.EndDuration.Value.ShouldBe(Duration.GetDurationFromChristBirth(SpecificDate.BeforeChrist(100, 2, 3, 20)));
            @event.EndNullPart.ShouldNotBeNull();
            @event.EndNullPart.Value.ShouldBe(NullableDateParts.Nothing);

            events.ShouldAllBe(e => e.Id.HasValue);
        }

        [Fact]
        public async Task Save_updates_events()
        {
            // Arrange

            _db.Events.RemoveRange(_db.Events);

            await _db.SaveChangesAsync();

            var events = new[]
                {
                    new Event<string, string>("A", SpecificDate.BeforeChrist(10), SpecificDate.AnnoDomini(12)),
                    new Event<string, string>("B", SpecificDate.BeforeChrist(20, 5)),
                    new Event<string, string>("C", SpecificDate.AnnoDomini(2020, 1, 1), NowDate.Instance),
                    new Event<string, string>("D", NowDate.Instance),
                    new Event<string, string>("E", SpecificDate.BeforeChrist(100, 2, 3, 10), SpecificDate.BeforeChrist(100, 2, 3, 20)),
                };

            await _repo.SaveEventsAsync(events);

            // Act

            var updatedEvents = new[]
            {
                events[1],
                events[3]
            };

            var newYear = DateTime.Now.Year + 10;

            events[1].Description = "B1";
            events[3].SetInterval(events[3].Start, SpecificDate.AnnoDomini(newYear, 2, 1));

            var id1 = events[1].Id.ShouldNotBeNull();
            var id3 = events[3].Id.ShouldNotBeNull();

            await _repo.SaveEventsAsync(updatedEvents);

            // Assert

            var storedEvents = await _db.Events.ToArrayAsync();

            storedEvents.ShouldNotBeNull();
            storedEvents.Length.ShouldBe(5);

            var @event = storedEvents.FirstOrDefault(e => e.Content == "A");
            @event.ShouldNotBeNull();
            @event.StartIsCurrent.ShouldBeFalse();
            @event.StartDuration.ShouldNotBeNull();
            @event.StartDuration.Value.ShouldBe(Duration.GetDurationFromChristBirth(SpecificDate.BeforeChrist(10)));
            @event.StartNullPart.ShouldNotBeNull();
            @event.StartNullPart.ShouldBe(NullableDateParts.Month);
            @event.EndIsCurrent.ShouldNotBeNull();
            @event.EndIsCurrent.Value.ShouldBeFalse();
            @event.EndDuration.ShouldNotBeNull();
            @event.EndDuration.Value.ShouldBe(Duration.GetDurationFromChristBirth(SpecificDate.AnnoDomini(12)));
            @event.EndNullPart.ShouldNotBeNull();
            @event.EndNullPart.Value.ShouldBe(NullableDateParts.Month);

            @event = storedEvents.FirstOrDefault(e => e.Content == "B1");
            @event.ShouldNotBeNull();
            @event.StartIsCurrent.ShouldBeFalse();
            @event.StartDuration.ShouldNotBeNull();
            @event.StartDuration.Value.ShouldBe(Duration.GetDurationFromChristBirth(SpecificDate.BeforeChrist(20, 5)));
            @event.StartNullPart.ShouldNotBeNull();
            @event.StartNullPart.ShouldBe(NullableDateParts.Day);
            @event.EndIsCurrent.ShouldBeNull();
            @event.EndDuration.ShouldBeNull();
            @event.EndNullPart.ShouldBeNull();
            @event.Id.ShouldBe(id1);

            @event = storedEvents.FirstOrDefault(e => e.Content == "C");
            @event.ShouldNotBeNull();
            @event.StartIsCurrent.ShouldBeFalse();
            @event.StartDuration.ShouldNotBeNull();
            @event.StartDuration.Value.ShouldBe(Duration.GetDurationFromChristBirth(SpecificDate.AnnoDomini(2020, 1, 1)));
            @event.StartNullPart.ShouldNotBeNull();
            @event.StartNullPart.ShouldBe(NullableDateParts.Hour);
            @event.EndIsCurrent.ShouldNotBeNull();
            @event.EndIsCurrent.Value.ShouldBeTrue();
            @event.EndDuration.ShouldBeNull();
            @event.EndNullPart.ShouldBeNull();

            @event = storedEvents.FirstOrDefault(e => e.Content == "D");
            @event.ShouldNotBeNull();
            @event.StartIsCurrent.ShouldBeTrue();
            @event.StartDuration.ShouldBeNull();
            @event.StartNullPart.ShouldBeNull();
            @event.EndIsCurrent.ShouldNotBeNull();
            @event.EndIsCurrent.Value.ShouldBeFalse();
            @event.EndDuration.ShouldNotBeNull();
            @event.EndDuration.Value.ShouldBe(Duration.GetDurationFromChristBirth(SpecificDate.AnnoDomini(newYear, 2, 1)));
            @event.EndNullPart.ShouldNotBeNull();
            @event.EndNullPart.Value.ShouldBe(NullableDateParts.Hour);
            @event.Id.ShouldBe(id3);

            @event = storedEvents.FirstOrDefault(e => e.Content == "E");
            @event.ShouldNotBeNull();
            @event.StartIsCurrent.ShouldBeFalse();
            @event.StartDuration.ShouldNotBeNull();
            @event.StartDuration.Value.ShouldBe(Duration.GetDurationFromChristBirth(SpecificDate.BeforeChrist(100, 2, 3, 10)));
            @event.StartNullPart.ShouldNotBeNull();
            @event.StartNullPart.ShouldBe(NullableDateParts.Nothing);
            @event.EndIsCurrent.ShouldNotBeNull();
            @event.EndIsCurrent.Value.ShouldBeFalse();
            @event.EndDuration.ShouldNotBeNull();
            @event.EndDuration.Value.ShouldBe(Duration.GetDurationFromChristBirth(SpecificDate.BeforeChrist(100, 2, 3, 20)));
            @event.EndNullPart.ShouldNotBeNull();
            @event.EndNullPart.Value.ShouldBe(NullableDateParts.Nothing);

            events.ShouldAllBe(e => e.Id.HasValue);
        }

        [Fact]
        public async Task Get_all_events()
        {
            // Arrange

            _db.Events.RemoveRange(_db.Events);

            await _db.SaveChangesAsync();

            // Act

            var events = new[]
                {
                    new Event<string, string>("A", SpecificDate.BeforeChrist(10), SpecificDate.AnnoDomini(12)),
                    new Event<string, string>("B", SpecificDate.BeforeChrist(20, 5)),
                    new Event<string, string>("C", SpecificDate.AnnoDomini(2020, 1, 1), NowDate.Instance),
                    new Event<string, string>("D", NowDate.Instance),
                    new Event<string, string>("E", SpecificDate.BeforeChrist(100, 2, 3, 10), SpecificDate.BeforeChrist(100, 2, 3, 20)),
                };

            await _repo.SaveEventsAsync(events);

            // Assert

            var restoredEvents = await _repo.GetEventsAsync();

            restoredEvents.ShouldNotBeNull();
            restoredEvents.Count.ShouldBe(5);

            var @event = restoredEvents.FirstOrDefault(e => e.Description == "A");
            @event.ShouldNotBeNull();
            @event.Start.ShouldBe(SpecificDate.BeforeChrist(10));
            @event.End.ShouldBe(SpecificDate.AnnoDomini(12));

            @event = restoredEvents.FirstOrDefault(e => e.Description == "B");
            @event.ShouldNotBeNull();
            @event.Start.ShouldBe(SpecificDate.BeforeChrist(20, 5));
            @event.End.ShouldBeNull();

            @event = restoredEvents.FirstOrDefault(e => e.Description == "C");
            @event.ShouldNotBeNull();
            @event.Start.ShouldBe(SpecificDate.AnnoDomini(2020, 1, 1));
            @event.End.ShouldBe(NowDate.Instance);

            @event = restoredEvents.FirstOrDefault(e => e.Description == "D");
            @event.ShouldNotBeNull();
            @event.Start.ShouldBe(NowDate.Instance);
            @event.End.ShouldBeNull();

            @event = restoredEvents.FirstOrDefault(e => e.Description == "E");
            @event.ShouldNotBeNull();
            @event.Start.ShouldBe(SpecificDate.BeforeChrist(100, 2, 3, 10));
            @event.End.ShouldBe(SpecificDate.BeforeChrist(100, 2, 3, 20));

            restoredEvents.ShouldAllBe(e => e.Id.HasValue);
        }
    }
}
