using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EdlinSoftware.Timeline.Domain;

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

        public async Task<IReadOnlyList<Event<string>>> GetEventsAsync()
        {
            var eventsInfo = await _db.Events.ToArrayAsync();

            var events = new List<Event<string>>(eventsInfo.Length);

            foreach (var eventInfo in eventsInfo)
            {
                var startDate = GetStartDate(eventInfo);
                var endDate = GetEndDate(eventInfo);

                var @event = new Event<string>(eventInfo.Content, startDate, endDate);

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
        public async Task SaveEventsAsync(IReadOnlyList<Event<string>> events)
        {
            foreach (var @event in events)
            {
                var eventInfo = GetEventInfo(@event);

                if(@event.Id.HasValue)
                {
                    eventInfo.Id = @event.Id.Value;
                    _db.Entry(eventInfo).State = EntityState.Modified;
                }
                else
                {
                    _db.Entry(eventInfo).State = EntityState.Added;
                }
            }

            await _db.SaveChangesAsync();
        }

        private Event GetEventInfo(Event<string> @event)
        {
            var eventInfo = new Event
            {
                Content = @event.Description
            };

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
