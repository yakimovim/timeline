using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EdlinSoftware.Timeline.Domain;
using System.Linq;

namespace EdlinSoftware.Timeline.Storage
{
    /// <summary>
    /// Represents repository of <see cref="Event{String}"/>
    /// </summary>
    public class EventsRepository
    {
        private readonly TimelineContext _db;

        public EventsRepository(TimelineContext dbContext)
        {
            _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IReadOnlyList<Event<string, string>>> GetEventsAsync(EventsSpecification specification = null)
        {
            IQueryable<Event> query = _db.Events;

            if(specification != null)
            {
                query = query.Where(specification.GetFilterExpression());
            }

            var eventsInfo = await query.ToArrayAsync();

            var events = new List<Event<string, string>>(eventsInfo.Length);

            if (eventsInfo.Length == 0) return events;

            var placesRepository = new PlacesRepository(_db);

            var places = await placesRepository.GetPlacesAsync();

            foreach (var eventInfo in eventsInfo)
            {
                var startDate = GetStartDate(eventInfo);
                var endDate = GetEndDate(eventInfo);

                var @event = new Event<string, string>(eventInfo.Content, startDate, endDate)
                {
                    Id = eventInfo.Id
                };

                if(eventInfo.PlaceId != null)
                {
                    @event.Place = places.GetNodeById(eventInfo.PlaceId);
                }

                events.Add(@event);
            }

            return events;
        }

        private Date GetStartDate(Event eventInfo)
        {
            if (eventInfo.StartIsCurrent) return NowDate.Instance;
            if (!eventInfo.StartDuration.HasValue)
                throw new InvalidOperationException($"Non-current start date must have duration for event {eventInfo.Id}");
            if (!eventInfo.StartNullPart.HasValue)
                throw new InvalidOperationException($"Non-current start date must have null part for event {eventInfo.Id}");

            var duration = Duration.Zero + eventInfo.StartDuration.Value;

            var exactDate = duration.GetDateAfterChristBirth();

            switch(eventInfo.StartNullPart.Value)
            {
                case NullableDateParts.Nothing:
                    return new SpecificDate(exactDate.Era, exactDate.Year, exactDate.Month, exactDate.Day, exactDate.Hour);
                case NullableDateParts.Hour:
                    return new SpecificDate(exactDate.Era, exactDate.Year, exactDate.Month, exactDate.Day);
                case NullableDateParts.Day:
                    return new SpecificDate(exactDate.Era, exactDate.Year, exactDate.Month);
                case NullableDateParts.Month:
                    return new SpecificDate(exactDate.Era, exactDate.Year);
                default:
                    throw new InvalidOperationException($"Unknown null date part for start date for event {eventInfo.Id}");
            }
        }

        private Date GetEndDate(Event eventInfo)
        {
            if (!eventInfo.EndIsCurrent.HasValue) return null;
            if (eventInfo.EndIsCurrent.Value) return NowDate.Instance;
            if (!eventInfo.EndDuration.HasValue)
                throw new InvalidOperationException($"Non-current end date must have duration for event {eventInfo.Id}");
            if (!eventInfo.EndNullPart.HasValue)
                throw new InvalidOperationException($"Non-current end date must have null part for event {eventInfo.Id}");

            var duration = Duration.Zero + eventInfo.EndDuration.Value;

            var exactDate = duration.GetDateAfterChristBirth();

            switch (eventInfo.EndNullPart.Value)
            {
                case NullableDateParts.Nothing:
                    return new SpecificDate(exactDate.Era, exactDate.Year, exactDate.Month, exactDate.Day, exactDate.Hour);
                case NullableDateParts.Hour:
                    return new SpecificDate(exactDate.Era, exactDate.Year, exactDate.Month, exactDate.Day);
                case NullableDateParts.Day:
                    return new SpecificDate(exactDate.Era, exactDate.Year, exactDate.Month);
                case NullableDateParts.Month:
                    return new SpecificDate(exactDate.Era, exactDate.Year);
                default:
                    throw new InvalidOperationException($"Unknown null date part for end date for event {eventInfo.Id}");
            }
        }

        public async Task RemoveEventsAsync(IReadOnlyList<Event<string, string>> events)
        {
            var storedEvents = events.Where(e => e.Id.HasValue).ToArray();

            foreach (var @event in storedEvents)
            {
                var storedEvent = new Event { Id = @event.Id.Value };

                var trackedEvent = _db.ChangeTracker.Entries<Event>().FirstOrDefault(e => e.Entity.Id == @event.Id.Value);
                if (trackedEvent != null)
                {
                    trackedEvent.State = EntityState.Detached;
                }

                _db.Events.Remove(storedEvent);

                @event.Id = null;
            }

            await _db.SaveChangesAsync();
        }

        public async Task SaveEventsAsync(IReadOnlyList<Event<string, string>> events)
        {
            var newEvents = new LinkedList<(Event StorageEvent, Event<string, string> DomainEvent)>();

            foreach (var @event in events)
            {
                var eventInfo = GetEventInfo(@event);

                if(@event.Id.HasValue)
                {
                    var trackedEvent = _db.ChangeTracker.Entries<Event>().FirstOrDefault(e => e.Entity.Id == @event.Id.Value);
                    if (trackedEvent != null)
                    { 
                        trackedEvent.State = EntityState.Detached; 
                    }

                    eventInfo.Id = @event.Id.Value;
                    _db.Entry(eventInfo).State = EntityState.Modified;
                }
                else
                {
                    newEvents.AddLast((eventInfo, @event));
                    _db.Entry(eventInfo).State = EntityState.Added;
                }
            }

            await _db.SaveChangesAsync();

            foreach (var corresondence in newEvents)
            {
                corresondence.DomainEvent.Id = corresondence.StorageEvent.Id;
            }
        }

        private Event GetEventInfo(Event<string, string> @event)
        {
            var eventInfo = new Event
            {
                Content = @event.Description
            };

            if(@event.Place != null)
            {
                eventInfo.PlaceId = @event.Place.Id;
            }

            switch(@event.Start)
            {
                case NowDate _:
                    {
                        eventInfo.StartIsCurrent = true;
                        eventInfo.StartDuration = null;
                        eventInfo.StartNullPart = null;
                        break;
                    }
                case SpecificDate d:
                    {
                        PartialDateInfo pD = d;
                        eventInfo.StartIsCurrent = false;
                        eventInfo.StartDuration = Duration.GetDurationFromChristBirth(pD);
                        eventInfo.StartNullPart = pD switch { 
                            _ when pD.Month is null => NullableDateParts.Month,
                            _ when pD.Day is null => NullableDateParts.Day,
                            _ when pD.Hour is null => NullableDateParts.Hour,
                            _ => NullableDateParts.Nothing
                        };
                        break;
                    }
            }

            if(@event.End is null)
            {
                eventInfo.EndIsCurrent = null;
                eventInfo.EndDuration = null;
                eventInfo.EndNullPart = null;
            }
            else
            {
                switch (@event.End)
                {
                    case NowDate _:
                        {
                            eventInfo.EndIsCurrent = true;
                            eventInfo.EndDuration = null;
                            eventInfo.EndNullPart = null;
                            break;
                        }
                    case SpecificDate d:
                        {
                            PartialDateInfo pD = d;
                            eventInfo.EndIsCurrent = false;
                            eventInfo.EndDuration = Duration.GetDurationFromChristBirth(pD);
                            eventInfo.EndNullPart = pD switch
                            {
                                _ when pD.Month is null => NullableDateParts.Month,
                                _ when pD.Day is null => NullableDateParts.Day,
                                _ when pD.Hour is null => NullableDateParts.Hour,
                                _ => NullableDateParts.Nothing
                            };
                            break;
                        }
                }
            }

            return eventInfo;
        }
    }
}
