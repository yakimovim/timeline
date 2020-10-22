using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            var internalEvents = events
                .Where(e => e.Duration > _pointEventMaximumDuration)
                .OrderBy(e => e.Start)
                .ToArray();

            var distribution = new EventsDistribution<T>();

            AddPointEvents(distribution, pointEvents);

            AddIntervalEvents(distribution, internalEvents);

            return distribution;
        }

        private void AddPointEvents<T>(
            EventsDistribution<T> distribution, 
            Event<T>[] pointEvents)
        {
            throw new NotImplementedException();
        }

        private void AddIntervalEvents<T>(
            EventsDistribution<T> distribution, 
            Event<T>[] internalEvents)
        {
            throw new NotImplementedException();
        }
    }
}
