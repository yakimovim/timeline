using EdlinSoftware.Timeline.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EdlinSoftware.Timeline.Storage
{
    using static EventsSpecification;

    public class EventsSetsRepository
    {
        private readonly TimelineContext _db;

        public EventsSetsRepository(TimelineContext context)
        {
            _db = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IReadOnlyList<EventsSet>> GetEventsSetsAsync(string namePart = null)
        {
            IQueryable<EventsSet> query = _db.EventSets;

            if(!string.IsNullOrWhiteSpace(namePart))
            {
                query = query.Where(s => s.Name.Contains(namePart));
            }

            return await query.ToArrayAsync();
        }

        public async Task<EventsSet<string, string>> GetEventsSetAsync(int id)
        {
            var eventsSet = await _db.EventSets.FindAsync(id);

            if (eventsSet == null) return null;

            var result = new EventsSet<string, string>(eventsSet.Name)
            {
                Id = eventsSet.Id
            };

            var eventIdsInSet = await _db.EventsInSets
                .Where(i => i.SetId == id)
                .Select(i => i.EventId)
                .ToArrayAsync();

            var eventsRepo = new EventsRepository(_db);

            var eventsInSet = await eventsRepo.GetEventsAsync(Ids(eventIdsInSet));

            result.Add(eventsInSet.ToArray());

            return result;
        }

        public async Task SaveEventsSetsAsync(params EventsSet<string, string>[] eventsSets)
        {
            if (eventsSets is null)
            {
                throw new ArgumentNullException(nameof(eventsSets));
            }

            foreach (var eventsSet in eventsSets)
            {
                if(eventsSet is null)
                {
                    throw new ArgumentException("Events sets should not be null", nameof(eventsSets));
                }
                // Save all events in the set

                var eventsRepo = new EventsRepository(_db);

                await eventsRepo.SaveEventsAsync(eventsSet.Events);

                // Save events set

                EventsSet eventsSetForStorage;

                if (eventsSet.Id.HasValue)
                {
                    var trackedEventsSet = _db.ChangeTracker.Entries<EventsSet>().FirstOrDefault(e => e.Entity.Id == eventsSet.Id.Value);
                    if (trackedEventsSet != null)
                    {
                        trackedEventsSet.State = EntityState.Detached;
                    }

                    eventsSetForStorage = new EventsSet
                    {
                        Id = eventsSet.Id.Value,
                        Name = eventsSet.Name
                    };
                    _db.Entry(eventsSetForStorage).State = EntityState.Modified;
                }
                else
                {
                    eventsSetForStorage = new EventsSet
                    {
                        Name = eventsSet.Name
                    };
                    _db.Entry(eventsSetForStorage).State = EntityState.Added;
                }

                await _db.SaveChangesAsync();

                eventsSet.Id = eventsSetForStorage.Id;

                // Save events correspondence

                _db.RemoveRange(_db.EventsInSets.Where(i => i.SetId == eventsSetForStorage.Id));

                foreach (var @event in eventsSet.Events)
                {
                    _db.EventsInSets.Add(new EventInSet
                    {
                        SetId = eventsSetForStorage.Id,
                        EventId = @event.Id.Value
                    });
                }

                await _db.SaveChangesAsync();
            }
        }

        public async Task RemoveEventsSetAsync(EventsSet<string, string> eventsSet)
        {
            if (eventsSet is null)
            {
                throw new ArgumentNullException(nameof(eventsSet));
            }

            if (!eventsSet.Id.HasValue) return;

            await RemoveEventsSetAsync(eventsSet.Id.Value);

            eventsSet.Id = null;
        }

        public async Task RemoveEventsSetAsync(int eventsSetId)
        {
            var trackedEventsSet = _db.ChangeTracker.Entries<EventsSet>()
                .FirstOrDefault(s => s.Entity.Id == eventsSetId);

            if(trackedEventsSet != null)
            {
                trackedEventsSet.State = EntityState.Detached;
            }

            var storedEventsSet = new EventsSet
            {
                Id = eventsSetId
            };

            _db.EventsInSets.RemoveRange(_db.EventsInSets.Where(i => i.SetId == eventsSetId));

            _db.EventSets.Remove(storedEventsSet);

            await _db.SaveChangesAsync();
        }
    }
}
