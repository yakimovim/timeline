using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace EdlinSoftware.Timeline.Domain
{
    /// <summary>
    /// Represents interval between two ticks
    /// on time axis.
    /// </summary>
    public class TickInterval
    {
        private readonly Func<ExactDateInfo, ExactDateInfo> _firstTickDateProvider;
        private readonly Func<ExactDateInfo, string> _nameProvider;
        private readonly Func<ExactDateInfo, ExactDateInfo> _nextTickDateProvider;

        public TickInterval(
            Duration duration,
            Func<ExactDateInfo, ExactDateInfo> firstTickDateProvider,
            Func<ExactDateInfo, string> nameProvider,
            Func<ExactDateInfo, ExactDateInfo> nextTickDateProvider = null)
        {
            if (duration <= Duration.Zero)
                throw new ArgumentOutOfRangeException(nameof(duration), "Duration of tick interval must be positive.");
            Duration = duration;
            _firstTickDateProvider = firstTickDateProvider ?? throw new ArgumentNullException(nameof(firstTickDateProvider));
            _nameProvider = nameProvider ?? throw new ArgumentNullException(nameof(nameProvider));
            _nextTickDateProvider = nextTickDateProvider ?? (date => date + Duration);
        }

        public Duration Duration { get; }

        public IReadOnlyList<Tick> GetTicksBetween(ExactDateInfo start, ExactDateInfo end)
        {
            var ticks = new List<Tick>();

            for (var tickDate = _firstTickDateProvider(start);
                tickDate <= end;
                tickDate = _nextTickDateProvider(tickDate))
            {
                ticks.Add(
                    new Tick(
                        tickDate,
                        _nameProvider(tickDate)
                    )
                );
            }

            return ticks;
        }
    }

    /// <summary>
    /// Provides infinite range of valid tick intervals.
    /// </summary>
    public static class TickIntervals
    {
        public static TickInterval GetFirstTickIntervalWithGreaterDuration(Duration duration)
        {
            return GetValidTickIntervals()
                .First(i => i.Duration >= duration);
        }

        public static TickInterval GetLastTickIntervalWithLessDuration(Duration duration)
        {
            TickInterval last = null;

            foreach (var tickInterval in GetValidTickIntervals())
            {
                if(last == null)
                {
                    last = tickInterval;
                }

                if (tickInterval.Duration >= duration)
                    return last;

                last = tickInterval;
            }

            throw new InvalidOperationException();
        }

        private static IEnumerable<TickInterval> GetValidTickIntervals()
        {
            yield return new TickInterval(Duration.Zero.AddHours(1), OneHourFirstTickDate, HoursTickName);
            yield return new TickInterval(Duration.Zero.AddHours(12), TwelveHoursFirstTickDate, HoursTickName);
            yield return new TickInterval(Duration.Zero.AddDays(1), DaysFirstTickDate, DaysTickName);
            yield return new TickInterval(Duration.Zero.AddMonths(1), OneMonthFirstTickDate, MonthsTickName);
            yield return new TickInterval(Duration.Zero.AddMonths(6), SixMonthsFirstTickDate, MonthsTickName);

            long years = 1;

            while (true)
            {
                var tickDuration = Duration.Zero.AddYears(years);

                yield return new TickInterval(
                    tickDuration,
                    YearsFirstTickDate(years),
                    YearsTickName,
                    NextTickDateForYears(tickDuration)
                );
                years *= 10;
            }
        }

        private static ExactDateInfo OneHourFirstTickDate(ExactDateInfo start)
        {
            return start;
        }

        private static string HoursTickName(ExactDateInfo date)
        {
            var builder = new StringBuilder();
            builder.Append(date.Year);
            builder.AppendLine(" " + date.Era.ToEraString());
            builder.Append(CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month));
            builder.AppendLine(" " + date.Day);
            builder.Append(date.Hour + ":00");
            return builder.ToString();
        }

        private static ExactDateInfo TwelveHoursFirstTickDate(ExactDateInfo start)
        {
            var date = new ExactDateInfo(
                start.Era,
                start.Year,
                start.Month,
                start.Day,
                0
            );

            if (date < start) date += Duration.Zero.AddHours(12);

            return date;
        }

        private static ExactDateInfo DaysFirstTickDate(ExactDateInfo start)
        {
            var date = new ExactDateInfo(
                start.Era,
                start.Year,
                start.Month,
                start.Day,
                0
            );

            if (date < start) date += Duration.Zero.AddDays(1);

            return date;
        }

        private static string DaysTickName(ExactDateInfo date)
        {
            var builder = new StringBuilder();
            builder.Append(date.Year);
            builder.AppendLine(" " + date.Era.ToEraString());
            builder.Append(CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month));
            builder.Append(" " + date.Day);
            return builder.ToString();
        }

        private static ExactDateInfo OneMonthFirstTickDate(ExactDateInfo start)
        {
            var date = new ExactDateInfo(
                start.Era,
                start.Year,
                start.Month,
                1,
                0
            );

            if (date < start) date += Duration.Zero.AddMonths(1);

            return date;
        }

        private static string MonthsTickName(ExactDateInfo date)
        {
            var builder = new StringBuilder();
            builder.Append(date.Year);
            builder.AppendLine(" " + date.Era.ToEraString());
            builder.Append(CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month));
            return builder.ToString();
        }

        private static ExactDateInfo SixMonthsFirstTickDate(ExactDateInfo start)
        {
            var date = new ExactDateInfo(
                start.Era,
                start.Year,
                1,
                1,
                0
            );

            if (date < start) date += Duration.Zero.AddMonths(6);

            return date;
        }

        private static Func<ExactDateInfo, ExactDateInfo> YearsFirstTickDate(long yearsPeriod)
        {
            return start =>
            {
                var periodsInYear = start.Year / yearsPeriod;

                var date = new ExactDateInfo(
                    start.Era,
                    Math.Max(1, periodsInYear) * yearsPeriod,
                    1,
                    1,
                    0
                );

                if (date < start) date += Duration.Zero.AddYears(yearsPeriod);

                return date;
            };
        }

        private static string YearsTickName(ExactDateInfo date)
        {
            var builder = new StringBuilder();
            builder.Append(date.Year);
            builder.Append(" " + date.Era.ToEraString());
            return builder.ToString();
        }

        private static Func<ExactDateInfo, ExactDateInfo> NextTickDateForYears(
            Duration tickDuration)
        {
            var oneYear = Duration.Zero.AddYears(1);
            var eraStart = ExactDateInfo.AnnoDomini(1, 1, 1, 0);

            return prevTickDate =>
            {
                if (tickDuration > oneYear
                    && prevTickDate == eraStart)
                {
                    return prevTickDate + tickDuration - oneYear;
                }

                return prevTickDate + tickDuration;
            };
        }
    }
}
