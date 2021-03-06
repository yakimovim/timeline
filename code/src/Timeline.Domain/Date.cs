﻿using System;
using System.Diagnostics;

namespace EdlinSoftware.Timeline.Domain
{

    /// <summary>
    /// Represents some (possible not exact) date.
    /// </summary>
    public abstract class Date
        : IEquatable<Date>, IComparable<Date>
    {
        public int CompareTo(Date other)
        {
            return GetDateInfo().CompareTo(other.GetDateInfo());
        }

        public bool Equals(Date other)
        {
            return GetDateInfo().Equals(other.GetDateInfo());
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            return Equals((Date)obj);
        }

        protected abstract PartialDateInfo GetDateInfo();

        public override int GetHashCode() => GetDateInfo().GetHashCode();

        public static bool operator ==(Date a, Date b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Date a, Date b)
        {
            return !(a == b);
        }

        public static bool operator >(Date a, Date b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator <(Date a, Date b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >=(Date a, Date b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool operator <=(Date a, Date b)
        {
            return a.CompareTo(b) <= 0;
        }

        public static Duration operator -(Date a, Date b)
        {
            return Duration.GetDurationFromChristBirth(a.GetDateInfo())
                - Duration.GetDurationFromChristBirth(b.GetDateInfo());
        }

        public static implicit operator PartialDateInfo(Date d)
        {
            return d.GetDateInfo();
        }
    }

    /// <summary>
    /// Represents current date.
    /// </summary>
    public sealed class NowDate : Date
    {
        [DebuggerStepThrough]
        private NowDate() { }

        public static readonly NowDate Instance = new NowDate();

        public override int GetHashCode() => -1;

        public override string ToString() => DateTime.Now.ToLongDateString();

        protected override PartialDateInfo GetDateInfo()
        {
            var now = DateTime.Now;

            return PartialDateInfo.AnnoDomini(now.Year, now.Month, now.Day, now.Hour);
        }
    }

    /// <summary>
    /// Represents some specific date.
    /// </summary>
    public sealed class SpecificDate : Date
    {
        private readonly PartialDateInfo _dateInfo;

        public SpecificDate(Era era, long year, int? month = null, int? day = null, int? hour = null)
        {
            _dateInfo = new PartialDateInfo(era, year, month, day, hour);
        }

        public override string ToString() => _dateInfo.ToString();

        protected override PartialDateInfo GetDateInfo() => _dateInfo;

        public static SpecificDate BeforeChrist(long year, int? month = null, int? day = null, int? hour = null)
            => new SpecificDate(Era.BeforeChrist, year, month, day, hour);

        public static SpecificDate AnnoDomini(long year, int? month = null, int? day = null, int? hour = null)
            => new SpecificDate(Era.AnnoDomini, year, month, day, hour);
    }

}
