using System;
using System.Diagnostics;

namespace EdlinSoftware.Timeline.Domain
{
    /// <summary>
    /// Represents some event.
    /// </summary>
    /// <typeparam name="TDescription">Type of event description.</typeparam>
    /// <typeparam name="TPlace">Type of event place.</typeparam>
    public class Event<TDescription, TPlace>
    {
        private TDescription _description;
        private Date _start;

        /// <summary>
        /// Event id.
        /// </summary>
        public int? Id { get; set; }

        public Event(TDescription description, Date start, Date end = null)
        {
            if(description == null)
                throw new ArgumentNullException(nameof(description), "Event description can't be null.");
            _description = description;
            SetInterval(start, end);
        }

        /// <summary>
        /// Event description.
        /// </summary>
        public TDescription Description
        {
            [DebuggerStepThrough]
            get => _description;
            [DebuggerStepThrough]
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "Event description can't be null.");

                _description = value;
            }

        }

        /// <summary>
        /// Start of event.
        /// </summary>
        public Date Start 
        { 
            [DebuggerStepThrough]
            get => _start;
            [DebuggerStepThrough]
            private set
            {
                _start = value ?? throw new ArgumentNullException(nameof(value), "Event start can't be null.");
            }
        }

        /// <summary>
        /// End of event.
        /// </summary>
        public Date End { get; private set; }

        /// <summary>
        /// Duration of the event.
        /// </summary>
        public Duration Duration
        {
            get
            {
                if (End == null) return Duration.Zero;

                return End - Start;
            }
        }

        /// <summary>
        /// Sets time interval of this event.
        /// </summary>
        /// <param name="start">Start of the event.</param>
        /// <param name="end">End of the event.</param>
        public void SetInterval(Date start, Date end = null)
        {
            if (start is null) throw new ArgumentNullException(nameof(start), "Event start can't be null.");

            if (end != null && end < start)
                throw new ArgumentException("Event end should be no less then start.", nameof(end));

            _start = start;
            End = end;
        }

        /// <summary>
        /// Checks if this event overlaps with another one
        /// by time.
        /// </summary>
        /// <param name="other">Another event.</param>
        public bool OverlapsWith(Event<TDescription, TPlace> other)
        {
            if (other is null)
                throw new ArgumentNullException(nameof(other));

            if (End == null && other.End == null)
                return (Start - other.Start) == Duration.Zero;

            return (Start <= other.Start &&
                other.Start - Start < Duration)
                || (other.Start <= Start &&
                Start - other.Start < other.Duration);
        }

        /// <summary>
        /// Place of the event.
        /// </summary>
        public HierarchyNode<TPlace> Place { get; set; }
    }
}
