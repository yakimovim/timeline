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
    public class PlacesRepositoryTests
    {
        private readonly PlacesRepositoryTestsFixture _fixture;

        public PlacesRepositoryTests()
        {
            _fixture = new PlacesRepositoryTestsFixture();
        }

        [Fact]
        public async Task Save_places_hierarchy()
        {
            // Act

            var result = await _fixture.Repo.SavePlacesAsync(_fixture.Hierarchy);

            // Assert

            result.IsSuccess.ShouldBeTrue();

            var places = await _fixture.Db.Places.ToArrayAsync();

            places.ShouldNotBeNull();
            places.Length.ShouldBe(4);

            var universe = places.FirstOrDefault(n => n.Id == "universe");
            universe.ShouldNotBeNull();
            universe.Content.ShouldBe("Universe");

            var solarSystem = places.FirstOrDefault(n => n.Id == "solar_system");
            solarSystem.ShouldNotBeNull();
            solarSystem.Content.ShouldBe("Solar system");

            var earth = places.FirstOrDefault(n => n.Id == "earth");
            earth.ShouldNotBeNull();
            earth.Content.ShouldBe("Earth");

            var mars = places.FirstOrDefault(n => n.Id == "mars");
            mars.ShouldNotBeNull();
            mars.Content.ShouldBe("Mars");
        }

        [Fact]
        public async Task Get_places_hierarchy()
        {
            // Arrange

            await _fixture.Repo.SavePlacesAsync(_fixture.Hierarchy);

            // Act

            var restoredHierarchy = await _fixture.Repo.GetPlacesAsync();

            // Assert

            restoredHierarchy.ShouldNotBeNull();

            restoredHierarchy.TopNodes.Count.ShouldBe(1);
            restoredHierarchy.Count().ShouldBe(4);

            restoredHierarchy.TopNodes[0].Id.ShouldBe<StringId>("universe");
            restoredHierarchy.TopNodes[0].Content.ShouldBe("Universe");

            restoredHierarchy.TopNodes[0]
                .SubNodes[0].Id.ShouldBe<StringId>("solar_system");
            restoredHierarchy.TopNodes[0]
                .SubNodes[0].Content.ShouldBe("Solar system");

            restoredHierarchy.TopNodes[0]
                .SubNodes[0]
                .SubNodes[0].Id.ShouldBe<StringId>("earth");
            restoredHierarchy.TopNodes[0]
                .SubNodes[0]
                .SubNodes[0].Content.ShouldBe("Earth");

            restoredHierarchy.TopNodes[0]
                .SubNodes[0]
                .SubNodes[1].Id.ShouldBe<StringId>("mars");
            restoredHierarchy.TopNodes[0]
                .SubNodes[0]
                .SubNodes[1].Content.ShouldBe("Mars");
        }

        [Fact]
        public async Task Cant_silently_remove_some_places()
        {
            // Arrange

            await _fixture.Repo.SavePlacesAsync(_fixture.Hierarchy);

            var newPlaces = new Hierarchy<string>();
            newPlaces.AddTopNode("universe", "Universe");

            // Act

            var result = await _fixture.Repo.SavePlacesAsync(newPlaces);

            // Assert

            result.IsFailure.ShouldBeTrue();
        }

        [Fact]
        public async Task Change_existing_place_id()
        {
            // Arrange

            await _fixture.Repo.SavePlacesAsync(_fixture.Hierarchy);

            var places = await _fixture.Repo.GetPlacesAsync();

            var @event = new Event<string, string>("A", NowDate.Instance)
            {
                Place = places.GetNodeById("universe")
            };

            var eventsRepo = new EventsRepository(_fixture.Db);

            await eventsRepo.SaveEventsAsync(new[] { @event });

            // Act

            await _fixture.Repo.ChangePlaceId("universe", "big_universe");

            // Assert

            places = await _fixture.Repo.GetPlacesAsync();

            places.ContainsNodeWithId("universe").ShouldBeFalse();
            places.ContainsNodeWithId("big_universe").ShouldBeTrue();

            @event = (await eventsRepo.GetEventsAsync()).Single();

            @event.Place.Id.ShouldBe<StringId>("big_universe");
        }

        [Fact]
        public async Task Change_unknown_place_id()
        {
            // Arrange

            await _fixture.Repo.SavePlacesAsync(_fixture.Hierarchy);

            var places = await _fixture.Repo.GetPlacesAsync();

            var @event = new Event<string, string>("A", NowDate.Instance)
            {
                Place = places.GetNodeById("universe")
            };

            var eventsRepo = new EventsRepository(_fixture.Db);

            await eventsRepo.SaveEventsAsync(new[] { @event });

            // Act

            await _fixture.Repo.ChangePlaceId("unknown", "big_universe");

            // Assert

            places = await _fixture.Repo.GetPlacesAsync();

            places.ContainsNodeWithId("unknown").ShouldBeFalse();
            places.ContainsNodeWithId("big_universe").ShouldBeFalse();

            @event = (await eventsRepo.GetEventsAsync()).Single();

            @event.Place.Id.ShouldBe<StringId>("universe");
        }

        [Fact]
        public async Task Remove_existing_place()
        {
            // Arrange

            await _fixture.Repo.SavePlacesAsync(_fixture.Hierarchy);

            var events = new[]
            {
                new Event<string, string>("A", NowDate.Instance),
                new Event<string, string>("B", NowDate.Instance)
                {
                    Place = _fixture.Hierarchy.GetNodeById("universe")
                },
                new Event<string, string>("C", NowDate.Instance)
                {
                    Place = _fixture.Hierarchy.GetNodeById("solar_system")
                },
                new Event<string, string>("D", NowDate.Instance)
                {
                    Place = _fixture.Hierarchy.GetNodeById("earth")
                },
            };

            await _fixture.EventsRepo.SaveEventsAsync(events);

            // Act

            await _fixture.Repo.RemovePlaceAsync("solar_system");

            // Assert

            var places = await _fixture.Repo.GetPlacesAsync();

            places.Count().ShouldBe(1);
            places.ContainsNodeWithId("universe").ShouldBeTrue();

            var storedEvents = await _fixture.EventsRepo.GetEventsAsync();

            storedEvents.Count.ShouldBe(4);
            storedEvents.Single(e => e.Description == "A").Place.ShouldBeNull();
            storedEvents.Single(e => e.Description == "B").Place.Id.ShouldBe<StringId>("universe");
            storedEvents.Single(e => e.Description == "C").Place.ShouldBeNull();
            storedEvents.Single(e => e.Description == "D").Place.ShouldBeNull();
        }

        [Fact]
        public async Task Remove_unknown_place()
        {
            // Arrange

            await _fixture.Repo.SavePlacesAsync(_fixture.Hierarchy);

            var events = new[]
            {
                new Event<string, string>("A", NowDate.Instance),
                new Event<string, string>("B", NowDate.Instance)
                {
                    Place = _fixture.Hierarchy.GetNodeById("universe")
                },
                new Event<string, string>("C", NowDate.Instance)
                {
                    Place = _fixture.Hierarchy.GetNodeById("solar_system")
                },
                new Event<string, string>("D", NowDate.Instance)
                {
                    Place = _fixture.Hierarchy.GetNodeById("earth")
                },
            };

            await _fixture.EventsRepo.SaveEventsAsync(events);

            // Act

            await _fixture.Repo.RemovePlaceAsync("unknown");

            // Assert

            var places = await _fixture.Repo.GetPlacesAsync();

            places.Count().ShouldBe(4);
            places.ContainsNodeWithId("universe").ShouldBeTrue();
            places.ContainsNodeWithId("solar_system").ShouldBeTrue();
            places.ContainsNodeWithId("earth").ShouldBeTrue();
            places.ContainsNodeWithId("mars").ShouldBeTrue();

            var storedEvents = await _fixture.EventsRepo.GetEventsAsync();

            storedEvents.Count.ShouldBe(4);
            storedEvents.Single(e => e.Description == "A").Place.ShouldBeNull();
            storedEvents.Single(e => e.Description == "B").Place.Id.ShouldBe<StringId>("universe");
            storedEvents.Single(e => e.Description == "C").Place.Id.ShouldBe<StringId>("solar_system");
            storedEvents.Single(e => e.Description == "D").Place.Id.ShouldBe<StringId>("earth"); ;
        }
    }

    public sealed class PlacesRepositoryTestsFixture
    {
        public readonly Hierarchy<string> Hierarchy;
        public readonly TimelineContext Db;
        public readonly PlacesRepository Repo;
        public readonly EventsRepository EventsRepo;

        public PlacesRepositoryTestsFixture()
        {
            Hierarchy = new Hierarchy<string>();
            Hierarchy.AddTopNode("universe", "Universe");
            Hierarchy.GetNodeById("universe").AddSubNode("solar_system", "Solar system");
            Hierarchy.GetNodeById("solar_system").AddSubNode("earth", "Earth");
            Hierarchy.GetNodeById("solar_system").AddSubNode("mars", "Mars");

            Db = TimelineContextProvider.GetDbContext();

            Repo = new PlacesRepository(Db);

            EventsRepo = new EventsRepository(Db);
        }
    }
}
