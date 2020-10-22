using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EdlinSoftware.Timeline.Domain
{
    /// <summary>
    /// Contains several lines of non-overlapping events.
    /// </summary>
    /// <typeparam name="TDescription">Type of event description.</typeparam>
    public sealed class EventsDistribution<TDescription>
    {
        /// <summary>
        /// List of events lines.
        /// </summary>
        public List<EventsLine<TDescription>> Lines { get; } = new List<EventsLine<TDescription>>();
    }

    /// <summary>
    /// Contains several non-overlapping events.
    /// </summary>
    /// <typeparam name="TDescription">Type of event description.</typeparam>
    public sealed class EventsLine<TDescription>
    {
        /// <summary>
        /// List of non-overlapping events in the line.
        /// </summary>
        public List<Event<TDescription>> Events { get; } = new List<Event<TDescription>>();
    }

    /// <summary>
    /// Collection of non-overlapping events.
    /// </summary>
    /// <typeparam name="TDescription">Type of event description.</typeparam>
    public sealed class NonOverlappintEvents<TDescription> : Collection<Event<TDescription>>
    {
        /// <inheritdoc />
        protected override void SetItem(int index, Event<TDescription> item)
        {
            if (index < 0 || index >= Count)
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
        protected override void InsertItem(int index, Event<TDescription> item)
        {
            if (index < 0 || index >= Count)
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
