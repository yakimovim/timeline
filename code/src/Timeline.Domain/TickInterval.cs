using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EdlinSoftware.Timeline.Domain
{
    /// <summary>
    /// Represents interval between two ticks
    /// on time axis.
    /// </summary>
    public class TickInterval
    {
        private readonly Duration _duration;
        private readonly Func<ExactDateInfo, ExactDateInfo> _firstTickDateProvider;
        private readonly Func<ExactDateInfo, string> _nameProvider;

        public TickInterval(
            Duration duration,
            Func<ExactDateInfo, ExactDateInfo> firstTickDateProvider,
            Func<ExactDateInfo, string> nameProvider)
        {
            if (duration <= Duration.Zero)
                throw new ArgumentOutOfRangeException(nameof(duration), "Duration of tick interval must be positive.");
            _duration = duration;
            _firstTickDateProvider = firstTickDateProvider ?? throw new ArgumentNullException(nameof(firstTickDateProvider));
            _nameProvider = nameProvider ?? throw new ArgumentNullException(nameof(nameProvider));
        }

        public IReadOnlyList<Tick> GetTicksBetween(ExactDateInfo start, ExactDateInfo end)
        {
            var ticks = new List<Tick>();

            for (var tickDate = _firstTickDateProvider(start);
                tickDate < end;
                tickDate += _duration)
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
        public static IEnumerable<TickInterval> GetValidTickIntervals()
        {
            yield return new TickInterval(Duration.Zero.AddHours(1), OneHourFirstTickDate, HoursTickName);
            yield return new TickInterval(Duration.Zero.AddHours(12), TwelveHoursFirstTickDate, HoursTickName);
            yield return new TickInterval(Duration.Zero.AddDays(1), DaysFirstTickDate, DaysTickName);
            yield return new TickInterval(Duration.Zero.AddMonths(1), OneMonthFirstTickDate, MonthsTickName);
            yield return new TickInterval(Duration.Zero.AddMonths(6), SixMonthsFirstTickDate, MonthsTickName);

            long years = 1;

            while (true)
            {
                yield return new TickInterval(Duration.Zero.AddYears(1), YearsFirstTickDate(years), YearsTickName);
                years *= 10;
            }
        }

        private static ExactDateInfo OneHourFirstTickDate(ExactDateInfo start)
        {
            return start;
        }

        private static string HoursTickName(ExactDateInfo date)
        {
            return date.ToString();
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
            builder.Append(" " + CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month));
            builder.Append(" " + date.Day);
            builder.Append(" " + date.Era.ToEraString());
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
            builder.Append(" " + CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month));
            builder.Append(" " + date.Era.ToEraString());
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

        private static Func<ExactDateInfo, ExactDateInfo> YearsFirstTickDate(long years)
        {
            return start =>
            {
                var year = start.Year / years;

                var date = new ExactDateInfo(
                    start.Era,
                    year,
                    1,
                    1,
                    0
                );

                if (date < start) date += Duration.Zero.AddYears(years);

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
    }
}
