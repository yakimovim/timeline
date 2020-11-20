using EdlinSoftware.Timeline.Domain;
using EdlinSoftware.Timeline.Storage;
using Shouldly;
using System.Threading.Tasks;
using Timeline.Storage.Tests.TestFramework;
using Xunit;

namespace Timeline.Storage.Tests
{
    public class PlaceWithoutParentsEventsSpecificationTests
    {
        private readonly TimelineContext _db;
        private readonly EventsRepository _eventsRepo;
        private readonly PlacesRepository _placesRepo;

        public PlaceWithoutParentsEventsSpecificationTests()
        {
            _db = TimelineContextProvider.GetDbContext();

            _eventsRepo = new EventsRepository(_db);

            _placesRepo = new PlacesRepository(_db);
        }

        [Fact]
        public async Task Get_events_for_place_with_no_events()
        {
            // Arrange

            var hierarchy = new Hierarchy<string>();
            hierarchy.AddTopNode("universe", "Universe");
            hierarchy.GetNodeById("universe").AddSubNode("solar_system", "Solar system");
            hierarchy.GetNodeById("solar_system").AddSubNode("earth", "Earth");
            hierarchy.GetNodeById("solar_system").AddSubNode("mars", "Mars");

            await _placesRepo.SavePlacesAsync(hierarchy);

            var events = new[]
            {
                new Event<string, string>("A", NowDate.Instance),
                new Event<string, string>("B", NowDate.Instance) 
                { 
                    Place = hierarchy.GetNodeById("universe")
                },
                new Event<string, string>("C", NowDate.Instance)
                {
                    Place = hierarchy.GetNodeById("solar_system")
                },
                new Event<string, string>("D", NowDate.Instance)
                {
                    Place = hierarchy.GetNodeById("earth")
                },
            };

            await _eventsRepo.SaveEventsAsync(events);

            // Act

            var eventsInPlace = await _eventsRepo.GetEventsAsync(
                new PlaceWithoutParentsEventsSpecification(
                    hierarchy.GetNodeById("mars")
                )
            );

            // Assert

            eventsInPlace.ShouldNotBeNull();
            eventsInPlace.ShouldBeEmpty();
        }

        [Fact]
        public async Task Get_events_for_place_with_events()
        {
            // Arrange

            var hierarchy = new Hierarchy<string>();
            hierarchy.AddTopNode("universe", "Universe");
            hierarchy.GetNodeById("universe").AddSubNode("solar_system", "Solar system");
            hierarchy.GetNodeById("solar_system").AddSubNode("earth", "Earth");
            hierarchy.GetNodeById("solar_system").AddSubNode("mars", "Mars");

            await _placesRepo.SavePlacesAsync(hierarchy);

            var events = new[]
            {
                new Event<string, string>("A", NowDate.Instance),
                new Event<string, string>("B", NowDate.Instance)
                {
                    Place = hierarchy.GetNodeById("universe")
                },
                new Event<string, string>("C", NowDate.Instance)
                {
                    Place = hierarchy.GetNodeById("solar_system")
                },
                new Event<string, string>("D", NowDate.Instance)
                {
                    Place = hierarchy.GetNodeById("earth")
                },
            };

            await _eventsRepo.SaveEventsAsync(events);

            // Act

            var eventsInPlace = await _eventsRepo.GetEventsAsync(
                new PlaceWithoutParentsEventsSpecification(
                    hierarchy.GetNodeById("solar_system")
                )
            );

            // Assert

            eventsInPlace.ShouldNotBeNull();
            eventsInPlace.Count.ShouldBe(2);
            events.ShouldContain(e => e.Description == "C");
            events.ShouldContain(e => e.Description == "D");
        }

    }
}
