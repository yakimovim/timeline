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
        private readonly TimelineContext _db;
        private readonly PlacesRepository _repo;

        public PlacesRepositoryTests()
        {
            _db = TimelineContextProvider.GetDbContext();

            _repo = new PlacesRepository(_db);
        }

        [Fact]
        public async Task Save_places_hierarchy()
        {
            // Arrange

            var hierarchy = new Hierarchy<string>();
            hierarchy.AddTopNode("universe", "Universe");
            hierarchy.GetNodeById("universe").AddSubNode("solar_system", "Solar system");
            hierarchy.GetNodeById("solar_system").AddSubNode("earth", "Earth");
            hierarchy.GetNodeById("solar_system").AddSubNode("mars", "Mars");

            // Act

            await _repo.SavePlacesAsync(hierarchy);

            // Assert

            var places = await _db.Places.ToArrayAsync();

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

            var hierarchy = new Hierarchy<string>();
            hierarchy.AddTopNode("universe", "Universe");
            hierarchy.GetNodeById("universe").AddSubNode("solar_system", "Solar system");
            hierarchy.GetNodeById("solar_system").AddSubNode("earth", "Earth");
            hierarchy.GetNodeById("solar_system").AddSubNode("mars", "Mars");

            // Act

            await _repo.SavePlacesAsync(hierarchy);

            var restoredHierarchy = await _repo.GetPlacesAsync();

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
    }
}
