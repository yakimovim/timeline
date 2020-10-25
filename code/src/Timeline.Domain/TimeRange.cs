using System;

namespace EdlinSoftware.Timeline.Domain
{
    /// <summary>
    /// Represents range between two exact dates.
    /// </summary>
    public class TimeRange
    {
        public TimeRange(ExactDateInfo start, ExactDateInfo end)
        {
            if (start > end) throw new ArgumentException("Start should not be greater then end.", nameof(end));
            Start = start;
            End = end;
        }

        /// <summary>
        /// Start of the range.
        /// </summary>
        public ExactDateInfo Start { get; }

        /// <summary>
        /// End of the range.
        /// </summary>
        public ExactDateInfo End { get; }
    }
}
