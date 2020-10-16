using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace EdlinSoftware.Timeline.Domain
{
    /// <summary>
    /// Represents some (possible not exact) date.
    /// </summary>
    public struct DateInfo
        : IEquatable<DateInfo>,
          IComparable<DateInfo>
    {
        public DateInfo(
            Era era,
            long year, 
            int? month = null, 
            int? day = null, 
            int? hour = null)
        {
            if (year <= 0)
                throw new ArgumentOutOfRangeException(nameof(year), "Only positive years are allowed. See https://en.wikipedia.org/wiki/Year_zero");
            if(month.HasValue)
            {
                if (month.Value < 1 || month.Value > 12)
                    throw new ArgumentOutOfRangeException(nameof(month), "Month should be between 1 and 12");
            }
            else
            {
                if (day.HasValue)
                    throw new ArgumentException("Day should be null if month is not specified", nameof(day));
                if (hour.HasValue)
                    throw new ArgumentException("Hour should be null if month is not specified", nameof(hour));
            }
            if(day.HasValue)
            {
                if (day.Value < 1 || day.Value > 31)
                    throw new ArgumentOutOfRangeException(nameof(day), "Day should be between 1 and 31");
                if (year < int.MaxValue - 1)
                {
                    var intYear = (int)year;
                    var date = new DateTime(intYear, month.Value, 1);
                    date = date.AddMonths(1).AddDays(-1);
                    if (day > date.Day)
                        throw new ArgumentOutOfRangeException(nameof(day), $"Day the {month} month of the {year} year should not be greater than {date.Day}");
                }
            }
            else
            {
                if (hour.HasValue)
                    throw new ArgumentException("Hour should be null if day is not specified", nameof(hour));
            }
            if(hour.HasValue)
            {
                if (hour < 0 || hour > 23)
                    throw new ArgumentOutOfRangeException(nameof(hour), "Hour should be between 0 and 23");
            }
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Era = era;
        }

        public long Year { get; }
        public int? Month { get; }
        public int? Day { get; }
        public int? Hour { get; }
        public Era Era { get; }

        public int CompareTo(DateInfo other)
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

        private int CompareDateParts(DateInfo other)
        {
            return CompareNullable(Month, other.Month)
                ?? CompareNullable(Day, other.Day)
                ?? CompareNullable(Hour, other.Hour)
                ?? 0;
        }

        private int? CompareNullable(long? x, long? y)
        {
            if (!x.HasValue && y.HasValue) return -1;
            if (x.HasValue && !y.HasValue) return 1;
            if (!x.HasValue && !y.HasValue) return null;

            Debug.Assert(x.HasValue && y.HasValue);

            var xVal = x.Value;
            var yVal = y.Value;

            if (xVal < yVal) return -1;
            if (xVal > yVal) return 1;

            return null;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DateInfo)) return false;

            return Equals((DateInfo) obj);
        }

        public bool Equals(DateInfo other)
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
            if(Month.HasValue)
            {
                hash = (hash * 23) + Month.Value.GetHashCode();
            }
            if (Day.HasValue)
            {
                hash = (hash * 23) + Day.Value.GetHashCode();
            }
            if (Hour.HasValue)
            {
                hash = (hash * 23) + Hour.Value.GetHashCode();
            }

            return hash;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Year);
            if(Month.HasValue)
            {
                builder.Append(" " + CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(Month.Value));
                if(Day.HasValue)
                {
                    builder.Append(" " + Day);
                    if(Hour.HasValue)
                    {
                        builder.Append(" " + Hour + ":00");
                    }
                }
            }
            builder.Append(" " + Era.ToEraString());
            return builder.ToString();
        }

        public static bool operator ==(DateInfo a, DateInfo b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(DateInfo a, DateInfo b)
        {
            return !(a == b);
        }

        public static bool operator >(DateInfo a, DateInfo b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator <(DateInfo a, DateInfo b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >=(DateInfo a, DateInfo b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool operator <=(DateInfo a, DateInfo b)
        {
            return a.CompareTo(b) <= 0;
        }

        public static DateInfo BeforeChrist(
            long year,
            int? month = null,
            int? day = null,
            int? hour = null)
            => new DateInfo(Era.BeforeChrist, year, month, day, hour);

        public static DateInfo AnnoDomini(
            long year,
            int? month = null,
            int? day = null,
            int? hour = null)
            => new DateInfo(Era.AnnoDomini, year, month, day, hour);
    }
}
