using System;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;

namespace EdlinSoftware.Timeline.Domain
{

    /// <summary>
    /// Represents some (possible not exact) date.
    /// </summary>
    public abstract class Date
    {
        
    }

    /// <summary>
    /// Represents current date.
    /// </summary>
    public sealed class NowDate : Date
    {
        [DebuggerStepThrough]
        private NowDate() { }

        public static readonly NowDate Instance = new NowDate();

        public override string ToString() => DateTime.Now.ToString("G");
    }

    public sealed class YearDate : Date
    {
        public YearDate(BigInteger year, Era era)
        {
            if (year < 0)
                throw new ArgumentOutOfRangeException(nameof(year), "Only non-negative years are allowed");
            Year = year;
            Era = era;
        }

        public BigInteger Year { get; }
        public Era Era { get; }

        public override string ToString() => $"{Year} {Era.ToEraString()}";
    }

    public sealed class YearMonthDate : Date
    {
        public YearMonthDate(BigInteger year, int month, Era era)
        {
            if (year < 0)
                throw new ArgumentOutOfRangeException(nameof(year), "Only non-negative years are allowed");
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), "Month should be between 1 and 12");

            Year = year;
            Month = month;
            Era = era;
        }

        public BigInteger Year { get; }
        public int Month { get; }
        public Era Era { get; }

        public override string ToString() => $"{Year} {CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(Month)} {Era.ToEraString()}";
    }

    public sealed class YearMonthDayDate : Date
    {
        public YearMonthDayDate(BigInteger year, int month, int day, Era era)
        {
            if (year < 0)
                throw new ArgumentOutOfRangeException(nameof(year), "Only non-negative years are allowed");
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), "Month should be between 1 and 12");
            if (day < 1 || day > 31)
                throw new ArgumentOutOfRangeException(nameof(day), "Day should be between 1 and 31");
            if(year < int.MaxValue)
            {
                var intYear = (int)year;
                var date = new DateTime(intYear, month, 1);
                date = date.AddMonths(1).AddDays(-1);
                if(day > date.Day)
                    throw new ArgumentOutOfRangeException(nameof(day), $"Day the {month} month of the {year} year should not be greater than {date.Day}");
            }

            Year = year;
            Month = month;
            Day = day;
            Era = era;
        }

        public BigInteger Year { get; }
        public int Month { get; }
        public int Day { get; }
        public Era Era { get; }

        public override string ToString() => $"{Year} {CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(Month)} {Day} {Era.ToEraString()}";
    }

    public sealed class YearMonthDayHourDate : Date
    {
        public YearMonthDayHourDate(BigInteger year, int month, int day, int hour, Era era)
        {
            if (year < 0)
                throw new ArgumentOutOfRangeException(nameof(year), "Only non-negative years are allowed");
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), "Month should be between 1 and 12");
            if (day < 1 || day > 31)
                throw new ArgumentOutOfRangeException(nameof(day), "Day should be between 1 and 31");
            if (year < int.MaxValue)
            {
                var intYear = (int)year;
                var date = new DateTime(intYear, month, 1);
                date = date.AddMonths(1).AddDays(-1);
                if (day > date.Day)
                    throw new ArgumentOutOfRangeException(nameof(day), $"Day the {month} month of the {year} year should not be greater than {date.Day}");
            }
            if(hour < 0 || hour > 23)
                throw new ArgumentOutOfRangeException(nameof(hour), "Hour should be between 0 and 23");

            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Era = era;
        }

        public BigInteger Year { get; }
        public int Month { get; }
        public int Day { get; }
        public int Hour { get; }
        public Era Era { get; }

        public override string ToString() => $"{Year} {CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(Month)} {Day} {Era.ToEraString()}";
    }
}
