using Shouldly;
using Xunit;

namespace EdlinSoftware.Timeline.Domain.Tests
{
    public class TimeRangeTests
    {
        [Fact]
        public void Scaling_up_hours()
        {
            var timeRange = new TimeRange(
                ExactDateInfo.AnnoDomini(100, 1, 1, 0),
                ExactDateInfo.AnnoDomini(100, 1, 1, 10)
            );

            var scaledTimeRange = timeRange.ScaleUp(Duration.Zero.AddHours(1));

            scaledTimeRange.ShouldNotBeNull();

            scaledTimeRange.Start.ShouldBe(timeRange.Start - Duration.Zero.AddHours(55));
            scaledTimeRange.End.ShouldBe(timeRange.End + Duration.Zero.AddHours(55));
            scaledTimeRange.Middle.ShouldBe(timeRange.Middle);
        }

        [Fact]
        public void Scaling_up_twelve_hours()
        {
            var timeRange = new TimeRange(
                ExactDateInfo.AnnoDomini(100, 1, 1, 0),
                ExactDateInfo.AnnoDomini(100, 1, 2, 0)
            );

            var scaledTimeRange = timeRange.ScaleUp(Duration.Zero.AddHours(11));

            scaledTimeRange.ShouldNotBeNull();

            scaledTimeRange.Start.ShouldBe(timeRange.Start - Duration.Zero.AddHours(12));
            scaledTimeRange.End.ShouldBe(timeRange.End + Duration.Zero.AddHours(12));
            scaledTimeRange.Middle.ShouldBe(timeRange.Middle);
        }

        [Fact]
        public void Scaling_up_one_day()
        {
            var timeRange = new TimeRange(
                ExactDateInfo.AnnoDomini(100, 1, 1, 0),
                ExactDateInfo.AnnoDomini(100, 1, 7, 0)
            );

            var scaledTimeRange = timeRange.ScaleUp(Duration.Zero.AddHours(20));

            scaledTimeRange.ShouldNotBeNull();

            scaledTimeRange.Start.ShouldBe(timeRange.Start - Duration.Zero.AddDays(30 * 3));
            scaledTimeRange.End.ShouldBe(timeRange.End + Duration.Zero.AddDays(30 * 3));
            scaledTimeRange.Middle.ShouldBe(timeRange.Middle);
        }

        [Fact]
        public void Scaling_up_one_month()
        {
            var timeRange = new TimeRange(
                ExactDateInfo.AnnoDomini(100, 1, 1, 0),
                ExactDateInfo.AnnoDomini(100, 7, 1, 0)
            );

            var scaledTimeRange = timeRange.ScaleUp(Duration.Zero.AddDays(15));

            scaledTimeRange.ShouldNotBeNull();

            scaledTimeRange.Start.ShouldBe(timeRange.Start - Duration.Zero.AddMonths(5 * 3));
            scaledTimeRange.End.ShouldBe(timeRange.End + Duration.Zero.AddMonths(5 * 3));
            scaledTimeRange.Middle.ShouldBe(timeRange.Middle);
        }

        [Fact]
        public void Scaling_up_six_months()
        {
            var timeRange = new TimeRange(
                ExactDateInfo.AnnoDomini(100, 1, 1, 0),
                ExactDateInfo.AnnoDomini(101, 1, 1, 0)
            );

            var scaledTimeRange = timeRange.ScaleUp(Duration.Zero.AddMonths(2));

            scaledTimeRange.ShouldNotBeNull();

            scaledTimeRange.Start.ShouldBe(timeRange.Start - Duration.Zero.AddMonths(6));
            scaledTimeRange.End.ShouldBe(timeRange.End + Duration.Zero.AddMonths(6));
            scaledTimeRange.Middle.ShouldBe(timeRange.Middle);
        }

        [Fact]
        public void Scaling_up_one_year()
        {
            var timeRange = new TimeRange(
                ExactDateInfo.AnnoDomini(100, 1, 1, 0),
                ExactDateInfo.AnnoDomini(106, 1, 1, 0)
            );

            var scaledTimeRange = timeRange.ScaleUp(Duration.Zero.AddMonths(10));

            scaledTimeRange.ShouldNotBeNull();

            scaledTimeRange.Start.ShouldBe(timeRange.Start - Duration.Zero.AddYears(27));
            scaledTimeRange.End.ShouldBe(timeRange.End + Duration.Zero.AddYears(27));
            scaledTimeRange.Middle.ShouldBe(timeRange.Middle);
        }

        [Fact]
        public void Scaling_down_ten_years()
        {
            var timeRange = new TimeRange(
                ExactDateInfo.AnnoDomini(100, 1, 1, 0),
                ExactDateInfo.AnnoDomini(120, 1, 1, 0)
            );

            var scaledTimeRange = timeRange.ScaleDown(Duration.Zero.AddYears(3));

            scaledTimeRange.ShouldNotBeNull();

            scaledTimeRange.Start.ShouldBe(timeRange.Middle - Duration.Zero.AddYears(1));
            scaledTimeRange.End.ShouldBe(timeRange.Middle + Duration.Zero.AddYears(1));
            scaledTimeRange.Middle.ShouldBe(timeRange.Middle);
        }

        [Fact]
        public void Scaling_down_one_year()
        {
            var timeRange = new TimeRange(
                ExactDateInfo.AnnoDomini(100, 1, 1, 0),
                ExactDateInfo.AnnoDomini(102, 1, 1, 0)
            );

            var scaledTimeRange = timeRange.ScaleDown(Duration.Zero.AddMonths(8));

            scaledTimeRange.ShouldNotBeNull();

            scaledTimeRange.Start.ShouldBe(timeRange.Middle - Duration.Zero.AddMonths(6));
            scaledTimeRange.End.ShouldBe(timeRange.Middle + Duration.Zero.AddMonths(6));
            scaledTimeRange.Middle.ShouldBe(timeRange.Middle);
        }

        [Fact]
        public void Scaling_down_six_months()
        {
            var timeRange = new TimeRange(
                ExactDateInfo.AnnoDomini(100, 1, 1, 0),
                ExactDateInfo.AnnoDomini(101, 1, 1, 0)
            );

            var scaledTimeRange = timeRange.ScaleDown(Duration.Zero.AddMonths(4));

            scaledTimeRange.ShouldNotBeNull();

            scaledTimeRange.Start.ShouldBe(timeRange.Middle - Duration.Zero.AddMonths(1));
            scaledTimeRange.End.ShouldBe(timeRange.Middle + Duration.Zero.AddMonths(1));
            scaledTimeRange.Middle.ShouldBe(timeRange.Middle);
        }

        [Fact]
        public void Scaling_down_one_month()
        {
            var timeRange = new TimeRange(
                ExactDateInfo.AnnoDomini(100, 1, 1, 0),
                ExactDateInfo.AnnoDomini(100, 7, 1, 0)
            );

            var scaledTimeRange = timeRange.ScaleDown(Duration.Zero.AddDays(20));

            scaledTimeRange.ShouldNotBeNull();

            scaledTimeRange.Start.ShouldBe(timeRange.Middle - Duration.Zero.AddDays(3));
            scaledTimeRange.End.ShouldBe(timeRange.Middle + Duration.Zero.AddDays(3));
            scaledTimeRange.Middle.ShouldBe(timeRange.Middle);
        }

        [Fact]
        public void Scaling_down_one_day()
        {
            var timeRange = new TimeRange(
                ExactDateInfo.AnnoDomini(100, 1, 1, 0),
                ExactDateInfo.AnnoDomini(100, 1, 13, 0)
            );

            var scaledTimeRange = timeRange.ScaleDown(Duration.Zero.AddHours(20));

            scaledTimeRange.ShouldNotBeNull();

            scaledTimeRange.Start.ShouldBe(timeRange.Middle - Duration.Zero.AddDays(3));
            scaledTimeRange.End.ShouldBe(timeRange.Middle + Duration.Zero.AddDays(3));
            scaledTimeRange.Middle.ShouldBe(timeRange.Middle);
        }

        [Fact]
        public void Scaling_down_twelve_hours()
        {
            var timeRange = new TimeRange(
                ExactDateInfo.AnnoDomini(100, 1, 1, 0),
                ExactDateInfo.AnnoDomini(100, 1, 2, 0)
            );

            var scaledTimeRange = timeRange.ScaleDown(Duration.Zero.AddHours(4));

            scaledTimeRange.ShouldNotBeNull();

            scaledTimeRange.Start.ShouldBe(timeRange.Middle - Duration.Zero.AddHours(1));
            scaledTimeRange.End.ShouldBe(timeRange.Middle + Duration.Zero.AddHours(1));
            scaledTimeRange.Middle.ShouldBe(timeRange.Middle);
        }

        [Fact]
        public void Scaling_down_one_hour_should_do_nothing()
        {
            var timeRange = new TimeRange(
                ExactDateInfo.AnnoDomini(100, 1, 1, 0),
                ExactDateInfo.AnnoDomini(100, 1, 1, 10)
            );

            var scaledTimeRange = timeRange.ScaleDown(Duration.Zero.AddHours(1));

            scaledTimeRange.ShouldNotBeNull();

            scaledTimeRange.Start.ShouldBe(timeRange.Start);
            scaledTimeRange.End.ShouldBe(timeRange.End);
            scaledTimeRange.Middle.ShouldBe(timeRange.Middle);
        }
    }
}
