using EdlinSoftware.Timeline.Domain;
using EdlinSoftware.Timeline.Storage;
using Shouldly;
using System.Threading.Tasks;
using Timeline.Storage.Tests.TestFramework;
using Xunit;

namespace Timeline.Storage.Tests
{
    using static EventsSpecification;

    public class PlaceWithParentsEventsSpecificationTests
        : IClassFixture<PlaceWithParentsEventsSpecificationTestsFixture>
    {
        private readonly PlaceWithParentsEventsSpecificationTestsFixture _fixture;

        public PlaceWithParentsEventsSpecificationTests(
            PlaceWithParentsEventsSpecificationTestsFixture fixture)
        {
            _fixture = fixture ?? throw new System.ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task Get_events_for_place_with_no_events()
        {
            // Act

            var eventsInPlace = await _fixture.EventsRepo.GetEventsAsync(
                InPlaceWithParents(
                    _fixture.Hierarchy.GetNodeById("mars")
                )
            );

            // Assert

            eventsInPlace.ShouldNotBeNull();
            eventsInPlace.Count.ShouldBe(2);
            eventsInPlace.ShouldContain(e => e.Description == "B");
            eventsInPlace.ShouldContain(e => e.Description == "C");
        }

        [Fact]
        public async Task Get_events_for_place_with_events()
        {
            // Act

            var eventsInPlace = await _fixture.EventsRepo.GetEventsAsync(
                InPlaceWithParents(
                    _fixture.Hierarchy.GetNodeById("solar_system")
                )
            );

            // Assert

            eventsInPlace.ShouldNotBeNull();
            eventsInPlace.Count.ShouldBe(3);
            eventsInPlace.ShouldContain(e => e.Description == "B");
            eventsInPlace.ShouldContain(e => e.Description == "C");
            eventsInPlace.ShouldContain(e => e.Description == "D");
        }

    }

    public class PlaceWithParentsEventsSpecificationTestsFixture : IAsyncLifetime
    {
        public readonly TimelineContext Db;
        public readonly EventsRepository EventsRepo;
        public readonly PlacesRepository PlacesRepo;
        public readonly Hierarchy<string> Hierarchy;

        public PlaceWithParentsEventsSpecificationTestsFixture()
        {
            Db = TimelineContextProvider.GetDbContext();

            EventsRepo = new EventsRepository(Db);

            PlacesRepo = new PlacesRepository(Db);

            Hierarchy = new Hierarchy<string>();
            Hierarchy.AddTopNode("universe", "Universe");
            Hierarchy.GetNodeById("universe").AddSubNode("solar_system", "Solar system");
            Hierarchy.GetNodeById("solar_system").AddSubNode("earth", "Earth");
            Hierarchy.GetNodeById("solar_system").AddSubNode("mars", "Mars");
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            await PlacesRepo.SavePlacesAsync(Hierarchy);

            var events = new[]
            {
                new Event<string, string>("A", NowDate.Instance),
                new Event<string, string>("B", NowDate.Instance)
                {
                    Place = Hierarchy.GetNodeById("universe")
                },
                new Event<string, string>("C", NowDate.Instance)
                {
                    Place = Hierarchy.GetNodeById("solar_system")
                },
                new Event<string, string>("D", NowDate.Instance)
                {
                    Place = Hierarchy.GetNodeById("earth")
                },
            };

            await EventsRepo.SaveEventsAsync(events);
        }
    }
}
