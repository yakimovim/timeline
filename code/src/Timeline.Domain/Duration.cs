using System;
using System.Diagnostics;

namespace EdlinSoftware.Timeline.Domain
{
    /// <summary>
    /// Represents approximate duration between two <see cref="Date"/>.
    /// </summary>
    /// <remarks>
    /// Year = 1
    /// Month = 1 / 12 of year
    /// Day = 1 / 31 of month
    /// Hour = 1 / 24 of day
    /// </remarks>
    [DebuggerDisplay("{" + nameof(_years) + "}")]
    public struct Duration : IEquatable<Duration>, IComparable<Duration>
    {
        private readonly static decimal YearsInMonth = 1M / 12;
        private readonly static decimal YearsInDay = YearsInMonth / 31;
        private readonly static decimal YearsInHour = YearsInDay / 24;

        private readonly decimal _years;

        [DebuggerStepThrough]
        private Duration(decimal years)
        {
            _years = years;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Duration)) return false;

            return Equals((Duration)obj);
        }

        public bool Equals(Duration other)
        {
            return other != null &&
                Math.Abs(_years - other._years) < YearsInHour / 10;
        }

        public override int GetHashCode()
        {
            return -894432144 + _years.GetHashCode();
        }

        public int CompareTo(Duration other)
        {
            return _years.CompareTo(other._years);
        }

        public readonly static Duration Zero = new Duration(0);

        public Duration AddYears(long years)
        {
            return new Duration(_years + years);
        }

        public Duration AddMonths(int months)
        {
            return new Duration(_years + YearsInMonth * months);
        }

        public Duration AddDays(int days)
        {
            return new Duration(_years + YearsInDay * days);
        }

        public Duration AddHours(int hours)
        {
            return new Duration(_years + YearsInHour * hours);
        }

        /// <summary>
        /// Gets date corresponding to this duration
        /// after Christ birth.
        /// </summary>
        public ExactDateInfo GetDateAfterChristBirth()
        {
            var duration = _years;

            var era = duration >= 0
                ? Era.AnnoDomini
                : Era.BeforeChrist;

            if (era == Era.AnnoDomini)
            {
                var years = Convert.ToInt64(Math.Floor(duration));

                duration -= years;

                var months = Convert.ToInt32(Math.Floor(duration / YearsInMonth));

                duration -= YearsInMonth * months;

                var days = Convert.ToInt32(Math.Floor(duration / YearsInDay));

                duration -= YearsInDay * days;

                var hours = Convert.ToInt32(Math.Floor(duration / YearsInHour));

                return ExactDateInfo.AnnoDomini(years + 1, months + 1, days + 1, hours);
            }
            else
            {
                var years = Convert.ToInt64(Math.Floor(duration));

                duration -= years;

                var months = Convert.ToInt32(Math.Floor(duration / YearsInMonth));

                duration -= YearsInMonth * months;

                var days = Convert.ToInt32(Math.Floor(duration / YearsInDay));

                duration -= YearsInDay * days;

                var hours = Convert.ToInt32(Math.Floor(duration / YearsInHour));

                return ExactDateInfo.BeforeChrist(-years, months + 1, days + 1, hours);
            }
        }


        public static Duration GetDurationFromChristBirth(PartialDateInfo dateInfo)
        {
            decimal years = 0;

            if (dateInfo.Era == Era.AnnoDomini)
            {
                years += dateInfo.Year - 1; // There is no zero year.
            }
            else
            {
                years = -dateInfo.Year;
            }

            if (dateInfo.Month.HasValue)
            {
                years += YearsInMonth * (dateInfo.Month.Value - 1);

                if (dateInfo.Day.HasValue)
                {
                    years += YearsInDay * (dateInfo.Day.Value - 1);

                    if (dateInfo.Hour.HasValue)
                    {
                        years += YearsInHour * dateInfo.Hour.Value;
                    }
                }
            }

            return new Duration(years);
        }

        public static Duration operator -(Duration a, Duration b)
        {
            return new Duration(a._years - b._years);
        }

        public static Duration operator +(Duration a, Duration b)
        {
            return new Duration(a._years + b._years);
        }

        public static Duration operator -(Duration a, decimal b)
        {
            return new Duration(a._years - b);
        }

        public static Duration operator +(Duration a, decimal b)
        {
            return new Duration(a._years + b);
        }

        public static Duration operator /(Duration a, decimal b)
        {
            return new Duration(a._years / b);
        }

        public static Duration operator *(Duration a, decimal b)
        {
            return new Duration(a._years * b);
        }

        public static bool operator ==(Duration a, Duration b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Duration a, Duration b)
        {
            return !(a == b);
        }

        public static bool operator >(Duration a, Duration b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator <(Duration a, Duration b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >=(Duration a, Duration b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool operator <=(Duration a, Duration b)
        {
            return a.CompareTo(b) <= 0;
        }

    }
}
