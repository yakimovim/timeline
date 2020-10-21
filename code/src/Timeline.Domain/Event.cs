using System;
using System.Diagnostics;

namespace EdlinSoftware.Timeline.Domain
{
    public class Event<TDescription>
    {
        private TDescription _description;
        private Date _start;

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
                if(value == null)
                    throw new ArgumentNullException(nameof(value), "Event start can't be null.");

                _start = value;
            }
        }

        /// <summary>
        /// End of event.
        /// </summary>
        public Date End { get; private set; }

        /// <summary>
        /// Sets time interval of this event.
        /// </summary>
        /// <param name="start">Start of the event.</param>
        /// <param name="end">End of the event.</param>
        public void SetInterval(Date start, Date end = null)
        {
            if (start is null) throw new ArgumentNullException(nameof(start), "Event start can't be null.");

            if(end != null && end < start)
                throw new ArgumentException("Event end should be no less then start.", nameof(end));

            _start = start;
            End = end;
        }

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
    }
}
