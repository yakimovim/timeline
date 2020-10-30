using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EdlinSoftware.Timeline.Domain.Tests
{
    public class TimeRangeTests
    {
        [Fact]
        public void Scaling_down_with_minimum_tick_interval_should_do_nothing()
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
    }
}
