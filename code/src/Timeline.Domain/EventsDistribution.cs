using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EdlinSoftware.Timeline.Domain
{
    /// <summary>
    /// Contains several lines of non-overlapping events.
    /// </summary>
    /// <typeparam name="TDescription">Type of event description.</typeparam>
    /// <typeparam name="TPlace">Type of event place.</typeparam>
    public sealed class EventsDistribution<TDescription, TPlace>
    {
        /// <summary>
        /// List of events lines.
        /// </summary>
        public List<EventsLine<TDescription, TPlace>> Lines { get; } = new List<EventsLine<TDescription, TPlace>>();
    }

    /// <summary>
    /// Contains several non-overlapping events.
    /// </summary>
    /// <typeparam name="TDescription">Type of event description.</typeparam>
    /// <typeparam name="TPlace">Type of event place.</typeparam>
    public sealed class EventsLine<TDescription, TPlace>
    {
        /// <summary>
        /// Is this line contain point (non-inverval) events.
        /// </summary>
        public bool IsPointEvents { get; }

        public EventsLine(bool isPointEvents)
        {
            IsPointEvents = isPointEvents;
        }

        /// <summary>
        /// List of non-overlapping events in the line.
        /// </summary>
        public NonOverlappintEvents<TDescription, TPlace> Events { get; } = new NonOverlappintEvents<TDescription, TPlace>();
    }

    /// <summary>
    /// Collection of non-overlapping events.
    /// </summary>
    /// <typeparam name="TDescription">Type of event description.</typeparam>
    /// <typeparam name="TPlace">Type of event place.</typeparam>
    public sealed class NonOverlappintEvents<TDescription, TPlace> : Collection<Event<TDescription, TPlace>>
    {
        /// <inheritdoc />
        protected override void SetItem(int index, Event<TDescription, TPlace> item)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            for (int i = 0; i < Count; i++)
            {
                if (i == index) continue;

                if (this[i].OverlapsWith(item))
                    throw new ArgumentException("This collection can't contain overlapping events", nameof(item));
            }

            base.SetItem(index, item);
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, Event<TDescription, TPlace> item)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            for (int i = 0; i < Count; i++)
            {
                if (i == index) continue;

                if (this[i].OverlapsWith(item))
                    throw new ArgumentException("This collection can't contain overlapping events", nameof(item));
            }

            base.InsertItem(index, item);
        }
    }
}
