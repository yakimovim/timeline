using System;
using System.Diagnostics;

namespace EdlinSoftware.Timeline.Domain
{
    public class Event<TDescription>
    {
        private TDescription _description;
        private Date _start;

        public Event(TDescription description, Date start)
        {
            if(description == null)
                throw new ArgumentNullException(nameof(description), "Event description can't be null.");
            _description = description;
            _start = start ?? throw new ArgumentNullException(nameof(start), "Event start can't be null.");
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
            set
            {
                if(value == null)
                    throw new ArgumentNullException(nameof(value), "Event start can't be null.");

                _start = value;
            }
        }

        /// <summary>
        /// End of event.
        /// </summary>
        public Date End { get; set; }
    }
}
