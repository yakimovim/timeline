using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace EdlinSoftware.Timeline.Domain
{
    /// <summary>
    /// Represents exact date.
    /// </summary>
    public struct ExactDateInfo
        : IEquatable<ExactDateInfo>,
          IComparable<ExactDateInfo>
    {
        public ExactDateInfo(
            Era era,
            long year, 
            int month, 
            int day, 
            int hour)
        {
            if (year <= 0)
                throw new ArgumentOutOfRangeException(nameof(year), "Only positive years are allowed. See https://en.wikipedia.org/wiki/Year_zero");
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), "Month should be between 1 and 12");
            if (day < 1 || day > 31)
                throw new ArgumentOutOfRangeException(nameof(day), "Day should be between 1 and 31");
            if (era == Era.AnnoDomini && year < 9998) // see DateTime constructor documentation: https://docs.microsoft.com/en-us/dotnet/api/system.datetime.-ctor?view=netcore-3.1#System_DateTime__ctor_System_Int32_System_Int32_System_Int32_
            {
                var intYear = (int)year;
                var date = new DateTime(intYear, month, 1);
                date = date.AddMonths(1).AddDays(-1);
                if (day > date.Day)
                    throw new ArgumentOutOfRangeException(nameof(day), $"Day the {month} month of the {year} year should not be greater than {date.Day}");
            }
            if (hour < 0 || hour > 23)
                throw new ArgumentOutOfRangeException(nameof(hour), "Hour should be between 0 and 23");

            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Era = era;
        }

        public long Year { get; }
        public int Month { get; }
        public int Day { get; }
        public int Hour { get; }
        public Era Era { get; }

        public int CompareTo(ExactDateInfo other)
        {
            if (Era == Era.BeforeChrist && other.Era == Era.AnnoDomini) return -1;
            if (Era == Era.AnnoDomini && other.Era == Era.BeforeChrist) return 1;

            Debug.Assert(Era == other.Era);

            var yearsComparison = Era == Era.AnnoDomini
                ? CompareNullable(Year, other.Year)
                : -CompareNullable(Year, other.Year);
            if (yearsComparison.HasValue) return yearsComparison.Value;

            return CompareDateParts(other);
        }

        private int CompareDateParts(ExactDateInfo other)
        {
            return CompareNullable(Month, other.Month)
                ?? CompareNullable(Day, other.Day)
                ?? CompareNullable(Hour, other.Hour)
                ?? 0;
        }

        private int? CompareNullable(long x, long y)
        {
            if (x < y) return -1;
            if (x > y) return 1;

            return null;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ExactDateInfo)) return false;

            return Equals((ExactDateInfo) obj);
        }

        public bool Equals(ExactDateInfo other)
        {
            if (Era != other.Era) return false;
            if (Year != other.Year) return false;
            if (Month != other.Month) return false;
            if (Day != other.Day) return false;
            if (Hour != other.Hour) return false;
            return true;
        }

        public override int GetHashCode()
        {
            var hash = Era.GetHashCode();
            hash = (hash * 23) + Year.GetHashCode();
            hash = (hash * 23) + Month.GetHashCode();
            hash = (hash * 23) + Day.GetHashCode();
            hash = (hash * 23) + Hour.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Year);
            builder.Append(" " + CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(Month));
            builder.Append(" " + Day);
            builder.Append(" " + Hour + ":00");
            builder.Append(" " + Era.ToEraString());
            return builder.ToString();
        }

        public static bool operator ==(ExactDateInfo a, ExactDateInfo b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(ExactDateInfo a, ExactDateInfo b)
        {
            return !(a == b);
        }

        public static bool operator >(ExactDateInfo a, ExactDateInfo b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator <(ExactDateInfo a, ExactDateInfo b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >=(ExactDateInfo a, ExactDateInfo b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool operator <=(ExactDateInfo a, ExactDateInfo b)
        {
            return a.CompareTo(b) <= 0;
        }

        public static implicit operator PartialDateInfo(ExactDateInfo d)
        {
            return new PartialDateInfo(d.Era, d.Year, d.Month, d.Day, d.Hour);
        }

        public static Duration operator -(ExactDateInfo a, ExactDateInfo b)
        {
            return Duration.GetDurationFromChristBirth(a)
                - Duration.GetDurationFromChristBirth(b);
        }

        public static ExactDateInfo operator -(ExactDateInfo a, Duration d)
        {
            return (Duration.GetDurationFromChristBirth(a) - d).GetDateAfterChristBirth();
        }

        public static ExactDateInfo operator +(ExactDateInfo a, Duration d)
        {
            return (Duration.GetDurationFromChristBirth(a) + d).GetDateAfterChristBirth();
        }

        public static ExactDateInfo BeforeChrist(
            long year,
            int month,
            int day,
            int hour)
            => new ExactDateInfo(Era.BeforeChrist, year, month, day, hour);

        public static ExactDateInfo AnnoDomini(
            long year,
            int month,
            int day,
            int hour)
            => new ExactDateInfo(Era.AnnoDomini, year, month, day, hour);
    }
}
