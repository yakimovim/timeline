using EdlinSoftware.Timeline.Domain;
using EdlinSoftware.Timeline.Storage;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using System.Threading.Tasks;
using Timeline.Storage.Tests.TestFramework;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Timeline.Storage.Tests
{
    public class EventsSetsRepositoryTests : IAsyncLifetime
    {
        private readonly EventsSetsRepositoryTestsFixture _fixture;

        public EventsSetsRepositoryTests()
        {
            _fixture = new EventsSetsRepositoryTestsFixture();
        }

        public Task DisposeAsync()
        {
            return _fixture.DisposeAsync();
        }

        public Task InitializeAsync()
        {
            return _fixture.InitializeAsync();
        }

        [Fact]
        public async Task Create_new_set()
        {
            var set = new EventsSet<string, string>("Set", 
                _fixture.Events[1],
                _fixture.Events[3]
            );

            await _fixture.EventsSetsRepo.SaveEventsSetsAsync(set);

            set.Id.ShouldNotBeNull();

            var storedSet = await _fixture.Db.EventSets.FindAsync(set.Id.Value);

            storedSet.ShouldNotBeNull();
            storedSet.Name.ShouldBe("Set");

            var storedSetEvents = await _fixture.Db.EventsInSets
                .Where(i => i.SetId == set.Id.Value)
                .ToArrayAsync();

            storedSetEvents.Length.ShouldBe(2);
            storedSetEvents.ShouldContain(i => i.EventId == _fixture.Events[1].Id.Value);
            storedSetEvents.ShouldContain(i => i.EventId == _fixture.Events[3].Id.Value);
        }

        [Fact]
        public async Task Remove_set()
        {
            var set = new EventsSet<string, string>("Set",
                _fixture.Events[1],
                _fixture.Events[3]
            );

            await _fixture.EventsSetsRepo.SaveEventsSetsAsync(set);

            set.Id.ShouldNotBeNull();

            await _fixture.EventsSetsRepo.RemoveEventsSetAsync(set);

            set.Id.ShouldBeNull();

            var storedSets = await _fixture.Db.EventSets.ToArrayAsync();

            storedSets.ShouldBeEmpty();

            var storedEventsInSets = await _fixture.Db.EventsInSets.ToArrayAsync();

            storedEventsInSets.ShouldBeEmpty();
        }

        [Fact]
        public async Task Get_all_sets()
        {
            var set1 = new EventsSet<string, string>("Set1",
                _fixture.Events[1],
                _fixture.Events[3]
            );

            var set2 = new EventsSet<string, string>("Set2",
                _fixture.Events[0],
                _fixture.Events[3]
            );

            await _fixture.EventsSetsRepo.SaveEventsSetsAsync(set1, set2);

            var allSets = await _fixture.EventsSetsRepo.GetEventsSetsAsync();

            allSets.ShouldNotBeNull();
            allSets.Count.ShouldBe(2);

            allSets.ShouldContain(s => s.Name == "Set1");
            allSets.ShouldContain(s => s.Name == "Set2");
        }

        [Fact]
        public async Task Get_all_sets_with_name_filter()
        {
            var set1 = new EventsSet<string, string>("Set1",
                _fixture.Events[1],
                _fixture.Events[3]
            );

            var set2 = new EventsSet<string, string>("Set2",
                _fixture.Events[0],
                _fixture.Events[3]
            );

            await _fixture.EventsSetsRepo.SaveEventsSetsAsync(set1, set2);

            var allSets = await _fixture.EventsSetsRepo.GetEventsSetsAsync("2");

            allSets.ShouldNotBeNull();
            allSets.Count.ShouldBe(1);

            allSets.ShouldContain(s => s.Name == "Set2");
        }

        [Fact]
        public async Task Get_single_set()
        {
            var set1 = new EventsSet<string, string>("Set1",
                _fixture.Events[1],
                _fixture.Events[3]
            );

            var set2 = new EventsSet<string, string>("Set2",
                _fixture.Events[0],
                _fixture.Events[3]
            );

            await _fixture.EventsSetsRepo.SaveEventsSetsAsync(set1, set2);

            var set = await _fixture.EventsSetsRepo.GetEventsSetAsync(set1.Id.Value);

            set.ShouldNotBeNull();
            set.Name.ShouldBe("Set1");
            set.Events.Count.ShouldBe(2);
            set.Events.ShouldContain(e => e.Description == _fixture.Events[1].Description);
            set.Events.ShouldContain(e => e.Description == _fixture.Events[3].Description);
        }
    }

    public class EventsSetsRepositoryTestsFixture : IAsyncLifetime
    {
        public readonly TimelineContext Db;
        public readonly EventsRepository EventsRepo;
        public readonly EventsSetsRepository EventsSetsRepo;

        public readonly IReadOnlyList<Event<string, string>> Events;

        public EventsSetsRepositoryTestsFixture()
        {
            Db = TimelineContextProvider.GetDbContext();

            EventsRepo = new EventsRepository(Db);

            EventsSetsRepo = new EventsSetsRepository(Db);

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
