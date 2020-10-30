using Shouldly;
using System.Linq;
using Xunit;

namespace EdlinSoftware.Timeline.Domain.Tests
{
    public class TickIntervalTests
    {
        [Fact]
        public void One_hour_tick_interval()
        {
            var tickInterval = GetFirstTickIntervalWithGreaterDuration(Duration.Zero);

            tickInterval.ShouldNotBeNull();

            var d1 = ExactDateInfo.AnnoDomini(100, 2, 3, 12);
            var d2 = ExactDateInfo.AnnoDomini(100, 2, 3, 14);

            var ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(3);

            ticks[0].Date.ShouldBe(d1);
            ticks[1].Date.ShouldBe(ExactDateInfo.AnnoDomini(100, 2, 3, 13));
            ticks[2].Date.ShouldBe(d2);

            d1 = ExactDateInfo.BeforeChrist(100, 2, 3, 12);
            d2 = ExactDateInfo.BeforeChrist(100, 2, 3, 14);

            ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(3);

            ticks[0].Date.ShouldBe(d1);
            ticks[1].Date.ShouldBe(ExactDateInfo.BeforeChrist(100, 2, 3, 13));
            ticks[2].Date.ShouldBe(d2);
        }

        [Fact]
        public void Twelve_hours_tick_interval()
        {
            var tickInterval = GetFirstTickIntervalWithGreaterDuration(Duration.Zero.AddHours(6));

            tickInterval.ShouldNotBeNull();

            var d1 = ExactDateInfo.AnnoDomini(100, 2, 3, 0);
            var d2 = ExactDateInfo.AnnoDomini(100, 2, 4, 0);

            var ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(3);

            ticks[0].Date.ShouldBe(d1);
            ticks[1].Date.ShouldBe(ExactDateInfo.AnnoDomini(100, 2, 3, 12));
            ticks[2].Date.ShouldBe(d2);

            d1 = ExactDateInfo.AnnoDomini(100, 2, 3, 5);
            d2 = ExactDateInfo.AnnoDomini(100, 2, 4, 17);

            ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(3);

            ticks[0].Date.ShouldBe(ExactDateInfo.AnnoDomini(100, 2, 3, 12));
            ticks[1].Date.ShouldBe(ExactDateInfo.AnnoDomini(100, 2, 4, 0));
            ticks[2].Date.ShouldBe(ExactDateInfo.AnnoDomini(100, 2, 4, 12));

            d1 = ExactDateInfo.BeforeChrist(100, 2, 3, 5);
            d2 = ExactDateInfo.BeforeChrist(100, 2, 4, 17);

            ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(3);

            ticks[0].Date.ShouldBe(ExactDateInfo.BeforeChrist(100, 2, 3, 12));
            ticks[1].Date.ShouldBe(ExactDateInfo.BeforeChrist(100, 2, 4, 0));
            ticks[2].Date.ShouldBe(ExactDateInfo.BeforeChrist(100, 2, 4, 12));
        }

        [Fact]
        public void One_day_tick_interval()
        {
            var tickInterval = GetFirstTickIntervalWithGreaterDuration(Duration.Zero.AddHours(20));

            tickInterval.ShouldNotBeNull();

            var d1 = ExactDateInfo.AnnoDomini(100, 2, 3, 0);
            var d2 = ExactDateInfo.AnnoDomini(100, 2, 5, 0);

            var ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(3);

            ticks[0].Date.ShouldBe(d1);
            ticks[1].Date.ShouldBe(ExactDateInfo.AnnoDomini(100, 2, 4, 0));
            ticks[2].Date.ShouldBe(d2);

            d1 = ExactDateInfo.AnnoDomini(100, 2, 3, 5);
            d2 = ExactDateInfo.AnnoDomini(100, 2, 5, 17);

            ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(2);

            ticks[0].Date.ShouldBe(ExactDateInfo.AnnoDomini(100, 2, 4, 0));
            ticks[1].Date.ShouldBe(ExactDateInfo.AnnoDomini(100, 2, 5, 0));

            d1 = ExactDateInfo.BeforeChrist(100, 2, 3, 5);
            d2 = ExactDateInfo.BeforeChrist(100, 2, 5, 17);

            ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(2);

            ticks[0].Date.ShouldBe(ExactDateInfo.BeforeChrist(100, 2, 4, 0));
            ticks[1].Date.ShouldBe(ExactDateInfo.BeforeChrist(100, 2, 5, 0));
        }

        [Fact]
        public void One_month_tick_interval()
        {
            var tickInterval = GetFirstTickIntervalWithGreaterDuration(Duration.Zero.AddDays(20));

            tickInterval.ShouldNotBeNull();

            var d1 = ExactDateInfo.AnnoDomini(100, 2, 1, 0);
            var d2 = ExactDateInfo.AnnoDomini(100, 5, 1, 0);

            var ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(4);

            ticks[0].Date.ShouldBe(d1);
            ticks[1].Date.ShouldBe(ExactDateInfo.AnnoDomini(100, 3, 1, 0));
            ticks[2].Date.ShouldBe(ExactDateInfo.AnnoDomini(100, 4, 1, 0));
            ticks[3].Date.ShouldBe(d2);

            d1 = ExactDateInfo.AnnoDomini(100, 3, 3, 5);
            d2 = ExactDateInfo.AnnoDomini(100, 5, 5, 17);

            ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(2);

            ticks[0].Date.ShouldBe(ExactDateInfo.AnnoDomini(100, 4, 1, 0));
            ticks[1].Date.ShouldBe(ExactDateInfo.AnnoDomini(100, 5, 1, 0));

            d1 = ExactDateInfo.BeforeChrist(100, 11, 3, 5);
            d2 = ExactDateInfo.BeforeChrist(99, 2, 5, 17);

            ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(3);

            ticks[0].Date.ShouldBe(ExactDateInfo.BeforeChrist(100, 12, 1, 0));
            ticks[1].Date.ShouldBe(ExactDateInfo.BeforeChrist(99, 1, 1, 0));
            ticks[2].Date.ShouldBe(ExactDateInfo.BeforeChrist(99, 2, 1, 0));
        }

        [Fact]
        public void Six_months_tick_interval()
        {
            var tickInterval = GetFirstTickIntervalWithGreaterDuration(Duration.Zero.AddMonths(3));

            tickInterval.ShouldNotBeNull();

            var d1 = ExactDateInfo.AnnoDomini(100, 1, 1, 0);
            var d2 = ExactDateInfo.AnnoDomini(101, 1, 1, 0);

            var ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(3);

            ticks[0].Date.ShouldBe(d1);
            ticks[1].Date.ShouldBe(ExactDateInfo.AnnoDomini(100, 7, 1, 0));
            ticks[2].Date.ShouldBe(d2);

            d1 = ExactDateInfo.AnnoDomini(100, 3, 3, 5);
            d2 = ExactDateInfo.AnnoDomini(101, 8, 5, 17);

            ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(3);

            ticks[0].Date.ShouldBe(ExactDateInfo.AnnoDomini(100, 7, 1, 0));
            ticks[1].Date.ShouldBe(ExactDateInfo.AnnoDomini(101, 1, 1, 0));
            ticks[2].Date.ShouldBe(ExactDateInfo.AnnoDomini(101, 7, 1, 0));

            d1 = ExactDateInfo.BeforeChrist(100, 3, 3, 5);
            d2 = ExactDateInfo.BeforeChrist(99, 8, 5, 17);

            ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(3);

            ticks[0].Date.ShouldBe(ExactDateInfo.BeforeChrist(100, 7, 1, 0));
            ticks[1].Date.ShouldBe(ExactDateInfo.BeforeChrist(99, 1, 1, 0));
            ticks[2].Date.ShouldBe(ExactDateInfo.BeforeChrist(99, 7, 1, 0));
        }

        [Fact]
        public void One_year_tick_interval()
        {
            var tickInterval = GetFirstTickIntervalWithGreaterDuration(Duration.Zero.AddMonths(8));

            tickInterval.ShouldNotBeNull();

            var d1 = ExactDateInfo.AnnoDomini(100, 1, 1, 0);
            var d2 = ExactDateInfo.AnnoDomini(102, 1, 1, 0);

            var ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(3);

            ticks[0].Date.ShouldBe(d1);
            ticks[1].Date.ShouldBe(ExactDateInfo.AnnoDomini(101, 1, 1, 0));
            ticks[2].Date.ShouldBe(d2);

            d1 = ExactDateInfo.AnnoDomini(100, 3, 3, 5);
            d2 = ExactDateInfo.AnnoDomini(104, 8, 5, 17);

            ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(4);

            ticks[0].Date.ShouldBe(ExactDateInfo.AnnoDomini(101, 1, 1, 0));
            ticks[1].Date.ShouldBe(ExactDateInfo.AnnoDomini(102, 1, 1, 0));
            ticks[2].Date.ShouldBe(ExactDateInfo.AnnoDomini(103, 1, 1, 0));
            ticks[3].Date.ShouldBe(ExactDateInfo.AnnoDomini(104, 1, 1, 0));

            d1 = ExactDateInfo.BeforeChrist(100, 3, 3, 5);
            d2 = ExactDateInfo.BeforeChrist(97, 8, 5, 17);

            ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(3);

            ticks[0].Date.ShouldBe(ExactDateInfo.BeforeChrist(99, 1, 1, 0));
            ticks[1].Date.ShouldBe(ExactDateInfo.BeforeChrist(98, 1, 1, 0));
            ticks[2].Date.ShouldBe(ExactDateInfo.BeforeChrist(97, 1, 1, 0));

            d1 = ExactDateInfo.BeforeChrist(2, 3, 3, 5);
            d2 = ExactDateInfo.AnnoDomini(2, 8, 5, 17);

            ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(3);

            ticks[0].Date.ShouldBe(ExactDateInfo.BeforeChrist(1, 1, 1, 0));
            ticks[1].Date.ShouldBe(ExactDateInfo.AnnoDomini(1, 1, 1, 0));
            ticks[2].Date.ShouldBe(ExactDateInfo.AnnoDomini(2, 1, 1, 0));

            d1 = ExactDateInfo.AnnoDomini(1, 3, 3, 5);
            d2 = ExactDateInfo.AnnoDomini(2, 8, 5, 17);

            ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(1);

            ticks[0].Date.ShouldBe(ExactDateInfo.AnnoDomini(2, 1, 1, 0));
        }

        [Theory]
        [InlineData(10L)]
        [InlineData(100L)]
        [InlineData(1000L)]
        [InlineData(10000L)]
        [InlineData(100000L)]
        [InlineData(1000000L)]
        [InlineData(10000000000000L)]
        public void Several_years_tick_interval(long years)
        {
            var tickInterval = GetFirstTickIntervalWithGreaterDuration(Duration.Zero.AddYears(years).AddMonths(-1));

            tickInterval.ShouldNotBeNull();

            var d1 = ExactDateInfo.AnnoDomini(years, 1, 1, 0);
            var d2 = ExactDateInfo.AnnoDomini(3 * years, 1, 1, 0);

            var ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(3);

            ticks[0].Date.ShouldBe(d1);
            ticks[1].Date.ShouldBe(ExactDateInfo.AnnoDomini(years * 2, 1, 1, 0));
            ticks[2].Date.ShouldBe(d2);

            d1 = ExactDateInfo.AnnoDomini(years, 3, 3, 5);
            d2 = ExactDateInfo.AnnoDomini(years * 4, 8, 5, 17);

            ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(3);

            ticks[0].Date.ShouldBe(ExactDateInfo.AnnoDomini(years * 2, 1, 1, 0));
            ticks[1].Date.ShouldBe(ExactDateInfo.AnnoDomini(years * 3, 1, 1, 0));
            ticks[2].Date.ShouldBe(ExactDateInfo.AnnoDomini(years * 4, 1, 1, 0));

            d1 = ExactDateInfo.BeforeChrist(years * 4, 3, 3, 5);
            d2 = ExactDateInfo.BeforeChrist(years * 1, 8, 5, 17);

            ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(3);

            ticks[0].Date.ShouldBe(ExactDateInfo.BeforeChrist(years * 3, 1, 1, 0));
            ticks[1].Date.ShouldBe(ExactDateInfo.BeforeChrist(years * 2, 1, 1, 0));
            ticks[2].Date.ShouldBe(ExactDateInfo.BeforeChrist(years * 1, 1, 1, 0));

            d1 = ExactDateInfo.BeforeChrist(years * 2, 3, 3, 5);
            d2 = ExactDateInfo.AnnoDomini(years * 1, 8, 5, 17);

            ticks = tickInterval.GetTicksBetween(d1, d2).ToArray();

            ticks.Length.ShouldBe(3);

            ticks[0].Date.ShouldBe(ExactDateInfo.BeforeChrist(years * 1, 1, 1, 0));
            ticks[1].Date.ShouldBe(ExactDateInfo.AnnoDomini(1, 1, 1, 0));
            ticks[2].Date.ShouldBe(ExactDateInfo.AnnoDomini(years * 1, 1, 1, 0));
        }

        private TickInterval GetFirstTickIntervalWithGreaterDuration(Duration duration)
        {
            return TickIntervals.GetFirstTickIntervalWithGreaterDuration(duration);
        }
    }
}
