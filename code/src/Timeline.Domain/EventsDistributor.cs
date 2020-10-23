﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace EdlinSoftware.Timeline.Domain
{
    /// <summary>
    /// Distribute all events by several lines of
    /// non-overlapping events.
    /// </summary>
    /// <remarks>
    /// Point events - events which duration is less then given threshold.
    /// Interval events - events which duration is more then given threshold.
    /// First line of events - line of point events (if they exist). Some events
    /// may be filtered our if they are close to each other that given threshold.
    /// Other lines are lines of non-overlapping interval events.
    /// </remarks>
    public class EventsDistributor
    {
        private readonly Duration _pointEventMaximumDuration;

        public EventsDistributor(Duration pointEventMaximumDuration)
        {
            _pointEventMaximumDuration = pointEventMaximumDuration;
        }

        public EventsDistribution<T> Distribute<T>(IReadOnlyList<Event<T>> events)
        {
            if (events is null)
                throw new ArgumentNullException(nameof(events));

            var pointEvents = events
                .Where(e => e.Duration <= _pointEventMaximumDuration)
                .OrderBy(e => e.Start)
                .ToArray();

            var intervalEvents = events
                .Where(e => e.Duration > _pointEventMaximumDuration)
                .OrderBy(e => e.Start)
                .ToArray();

            var distribution = new EventsDistribution<T>();

            AddPointEvents(distribution, pointEvents);

            AddIntervalEvents(distribution, intervalEvents);

            return distribution;
        }

        private void AddPointEvents<T>(
            EventsDistribution<T> distribution, 
            Event<T>[] pointEvents)
        {
            if (pointEvents.Length == 0) return;

            var eventsLine = new EventsLine<T>();

            while(true)
            {
                var @event = pointEvents[0];

                eventsLine.Events.Add(@event);

                pointEvents = pointEvents
                    .SkipWhile(e => e.Start - @event.Start <= _pointEventMaximumDuration)
                    .ToArray();

                if (pointEvents.Length == 0) break;
            }

            distribution.Lines.Add(eventsLine);
        }

        private void AddIntervalEvents<T>(
            EventsDistribution<T> distribution, 
            Event<T>[] intervalEvents)
        {
            if (intervalEvents.Length == 0) return;

            while(true)
            {
                var (restIntervalEvents, eventsLine) =
                    FillEventsLine(intervalEvents);

                distribution.Lines.Add(eventsLine);

                if (restIntervalEvents.Length == 0) break;

                intervalEvents = restIntervalEvents;
            }
        }

        private (Event<T>[] restIntervalEvents, EventsLine<T> eventsLine) FillEventsLine<T>(Event<T>[] intervalEvents)
        {
            if (intervalEvents.Length == 0)
                throw new InvalidOperationException();

            var eventsLine = new EventsLine<T>();
            var restIntervalEvents = new LinkedList<Event<T>>();

            var @event = intervalEvents[0];
            eventsLine.Events.Add(@event);

            for (int i = 1; i < intervalEvents.Length; i++)
            {
                var currentEvent = intervalEvents[i];

                if(@event.OverlapsWith(currentEvent))
                {
                    restIntervalEvents.AddLast(currentEvent);
                }
                else
                {
                    @event = currentEvent;
                    eventsLine.Events.Add(@event);
                }
            }

            return (restIntervalEvents.ToArray(), eventsLine);
        }
    }
}
