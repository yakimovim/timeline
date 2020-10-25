using Shouldly;
using Xunit;

namespace EdlinSoftware.Timeline.Domain.Tests
{
    public class DateTests
    {
        [Fact]
        public void Create_specific_date()
        {
            var date = new SpecificDate(Era.AnnoDomini, 200);

            date.ShouldNotBeNull();
        }

        [Fact]
        public void Create_now_date()
        {
            var date = NowDate.Instance;

            date.ShouldNotBeNull();
        }

        [Fact]
        public void Two_now_dates_must_be_equal()
        {
            NowDate.Instance.ShouldBe(NowDate.Instance);
        }

        [Fact]
        public void Compare_specific_and_now_dates()
        {
            SpecificDate.BeforeChrist(1).ShouldBeLessThan<Date>(NowDate.Instance);
            SpecificDate.AnnoDomini(2000).ShouldBeLessThan<Date>(NowDate.Instance);
            NowDate.Instance.ShouldBeLessThan<Date>(SpecificDate.AnnoDomini(14000000000L));
        }

        [Fact]
        public void Duration_between_two_dates()
        {
            var d1 = SpecificDate.AnnoDomini(2020);
            var d2 = SpecificDate.AnnoDomini(2000);

            (d1 - d2).ShouldBe(Duration.Zero.AddYears(20));
        }
    }
}
