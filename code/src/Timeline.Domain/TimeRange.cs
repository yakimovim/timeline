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

        /// <summary>
        /// Middle of the range.
        /// </summary>
        public ExactDateInfo Middle
            => Start + (End - Start) / 2;

        /// <summary>
        /// Duration of the range.
        /// </summary>
        public Duration Duration => End - Start;

        /// <summary>
        /// Moves this time range by given duration.
        /// </summary>
        /// <param name="duration">Duration to move by.</param>
        public TimeRange Move(Duration duration)
        {
            return new TimeRange(
                Start + duration,
                End + duration
            );
        }

        /// <summary>
        /// Changes start of the time range.
        /// </summary>
        /// <param name="start">New start date.</param>
        public TimeRange SetStart(ExactDateInfo start)
        {
            return new TimeRange(start, End);
        }

        /// <summary>
        /// Changes end of the time range.
        /// </summary>
        /// <param name="end">New end date.</param>
        public TimeRange SetEnd(ExactDateInfo end)
        {
            return new TimeRange(Start, end);
        }

        /// <summary>
        /// Scales this range up.
        /// </summary>
        /// <param name="minimumDurationBetweenTicks">Minimum allowed duration between two ticks.</param>
        public TimeRange ScaleUp(Duration minimumDurationBetweenTicks)
        {
            var currentTickInterval = TickIntervals.GetFirstTickIntervalWithGreaterDuration(minimumDurationBetweenTicks);

            var newTickInterval = TickIntervals.GetFirstTickIntervalWithGreaterDuration(currentTickInterval.Duration.AddHours(1));

            var ratio = newTickInterval.Duration / currentTickInterval.Duration;

            var newRangeDuration = Duration * ratio;

            return new TimeRange(Middle - (newRangeDuration / 2), Middle + (newRangeDuration / 2));
        }

        /// <summary>
        /// Scales this range down.
        /// </summary>
        /// <param name="minimumDurationBetweenTicks">Minimum allowed duration between two ticks.</param>
        public TimeRange ScaleDown(Duration minimumDurationBetweenTicks)
        {
            var currentTickInterval = TickIntervals.GetFirstTickIntervalWithGreaterDuration(minimumDurationBetweenTicks);

            var newTickInterval = TickIntervals.GetLastTickIntervalWithLessDuration(currentTickInterval.Duration.AddHours(-1));

            var ratio = newTickInterval.Duration / currentTickInterval.Duration;

            var newRangeDuration = Duration * ratio;

            return new TimeRange(Middle - (newRangeDuration / 2), Middle + (newRangeDuration / 2));
        }
    }
}
