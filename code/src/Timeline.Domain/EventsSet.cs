using System.Collections.Generic;

namespace EdlinSoftware.Timeline.Domain
{
    /// <summary>
    /// Represents a set of events.
    /// </summary>
    /// <typeparam name="TDescription">Type of event description.</typeparam>
    /// <typeparam name="TPlace">Type of event place.</typeparam>
    public class EventsSet<TDescription, TPlace>
    {
        private readonly HashSet<Event<TDescription, TPlace>> _events;

        /// <summary>
        /// Set id.
        /// </summary>
        public int? Id { get; set; }

        public EventsSet(string name, params Event<TDescription, TPlace>[] events)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new System.ArgumentException($"'{nameof(name)}' cannot be null or whitespace", nameof(name));
            }

            _events = new HashSet<Event<TDescription, TPlace>>(events);
            Name = name;
        }

        /// <summary>
        /// Events in the set.
        /// </summary>
        public IReadOnlyCollection<Event<TDescription, TPlace>> Events => _events;

        /// <summary>
        /// Name of this set.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Adds events into the set.
        /// </summary>
        /// <param name="events">Events</param>
        public void Add(params Event<TDescription, TPlace>[] events)
        {
            foreach (var @event in events)
            {
                _events.Add(@event);
            }
        }

        /// <summary>
        /// Removes events from the set.
        /// </summary>
        /// <param name="events">Events.</param>
        public void Remove(params Event<TDescription, TPlace>[] events)
        {
            foreach (var @event in events)
            {
                _events.Remove(@event);
            }
        }
    }
}
