using EdlinSoftware.Timeline.Domain;
using EdlinSoftware.Timeline.Storage;
using Microsoft.EntityFrameworkCore;
using Shouldly;
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

            await _repo.SaveEventsAsync(
                new []
                {
                    new Event<string>("A", SpecificDate.BeforeChrist(10), SpecificDate.AnnoDomini(12)),
                    new Event<string>("B", SpecificDate.BeforeChrist(20, 5)),
                    new Event<string>("C", SpecificDate.AnnoDomini(2020, 1, 1), NowDate.Instance),
                    new Event<string>("D", NowDate.Instance),
                    new Event<string>("E", SpecificDate.BeforeChrist(100, 2, 3, 10), SpecificDate.BeforeChrist(100, 2, 3, 20)),
                });

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
        }
    }
}
