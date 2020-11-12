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
    [DebuggerDisplay("{" + nameof(_hours) + "}")]
    public struct Duration : IEquatable<Duration>, IComparable<Duration>
    {
        private readonly static long HoursInDay = 24L;
        private readonly static long HoursInMonth = HoursInDay * 31;
        private readonly static long HoursInYear = HoursInMonth * 12;

        private readonly long _hours;

        [DebuggerStepThrough]
        private Duration(long hours)
        {
            _hours = hours;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Duration)) return false;

            return Equals((Duration)obj);
        }

        public bool Equals(Duration other)
        {
            return _hours == other._hours;
        }

        public override int GetHashCode()
        {
            return -894432144 + _hours.GetHashCode();
        }

        public int CompareTo(Duration other)
        {
            return _hours.CompareTo(other._hours);
        }

        public readonly static Duration Zero = new Duration(0);

        public Duration AddYears(long years)
        {
            return new Duration(_hours + HoursInYear * years);
        }

        public Duration AddMonths(int months)
        {
            return new Duration(_hours + HoursInMonth * months);
        }

        public Duration AddDays(int days)
        {
            return new Duration(_hours + HoursInDay * days);
        }

        public Duration AddHours(int hours)
        {
            return new Duration(_hours + hours);
        }

        /// <summary>
        /// Gets date corresponding to this duration
        /// after Christ birth.
        /// </summary>
        public ExactDateInfo GetDateAfterChristBirth()
        {
            var duration = _hours;

            var era = duration >= 0
                ? Era.AnnoDomini
                : Era.BeforeChrist;

            if (era == Era.AnnoDomini)
            {
                var years = duration / HoursInYear;

                duration -= HoursInYear * years;

                years += 1;

                var months = Convert.ToInt32(duration / HoursInMonth);

                duration -= HoursInMonth * months;

                months += 1;

                var days = Convert.ToInt32(duration / HoursInDay);

                duration -= HoursInDay * days;

                days += 1;

                FixDate(ref years, ref months, ref days);

                var hours = Convert.ToInt32(duration);

                return ExactDateInfo.AnnoDomini(years, months, days, hours);
            }
            else
            {
                var years = duration / HoursInYear;

                if(duration != years * HoursInYear)
                {
                    years -= 1;
                }

                duration -= HoursInYear * years;

                years = -years;

                var months = Convert.ToInt32(duration / HoursInMonth);

                duration -= HoursInMonth * months;

                months += 1;

                var days = Convert.ToInt32(duration / HoursInDay);

                duration -= HoursInDay * days;

                days += 1;

                var hours = Convert.ToInt32(duration);

                return ExactDateInfo.BeforeChrist(years, months, days, hours);
            }
        }

        private void FixDate(ref long years, ref int months, ref int days)
        {
            if (years >= 9998) return; // see DateTime constructor documentation: https://docs.microsoft.com/en-us/dotnet/api/system.datetime.-ctor?view=netcore-3.1#System_DateTime__ctor_System_Int32_System_Int32_System_Int32_

            var date = new DateTime((int)years, months, 1);
            date = date.AddMonths(1).AddDays(-1);

            if(date.Day < days)
            {
                date = date.AddDays(days - date.Day);
                years = date.Year;
                months = date.Month;
                days = date.Day;
            }
        }

        public static Duration GetDurationFromChristBirth(PartialDateInfo dateInfo)
        {
            long hours = 0;

            if (dateInfo.Era == Era.AnnoDomini)
            {
                hours += HoursInYear * (dateInfo.Year - 1); // There is no zero year.
            }
            else
            {
                hours = HoursInYear * (-dateInfo.Year);
            }

            if (dateInfo.Month.HasValue)
            {
                hours += HoursInMonth * (dateInfo.Month.Value - 1);

                if (dateInfo.Day.HasValue)
                {
                    hours += HoursInDay * (dateInfo.Day.Value - 1);

                    if (dateInfo.Hour.HasValue)
                    {
                        hours += dateInfo.Hour.Value;
                    }
                }
            }

            return new Duration(hours);
        }

        public static Duration operator -(Duration a, Duration b)
        {
            return new Duration(a._hours - b._hours);
        }

        public static Duration operator +(Duration a, Duration b)
        {
            return new Duration(a._hours + b._hours);
        }

        public static Duration operator -(Duration a, long hours)
        {
            return new Duration(a._hours - hours);
        }

        public static Duration operator +(Duration a, long hours)
        {
            return new Duration(a._hours + hours);
        }

        public static Duration operator /(Duration a, double b)
        {
            return new Duration(Convert.ToInt64(a._hours / b));
        }

        public static double operator /(Duration a, Duration b)
        {
            return ((double)a._hours) / (b._hours);
        }

        public static Duration operator *(Duration a, double b)
        {
            return new Duration(Convert.ToInt64(a._hours * b));
        }

        public static bool operator ==(Duration a, Duration b)
        {
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
