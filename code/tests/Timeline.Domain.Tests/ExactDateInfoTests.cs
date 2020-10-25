using Shouldly;
using System;
using Xunit;

namespace EdlinSoftware.Timeline.Domain.Tests
{
    public class ExactDateInfoTests
    {
        [Fact]
        public void Create_universe_start_date()
        {
            var dateInfo = new ExactDateInfo(Era.BeforeChrist, 13750000000L, 1, 1, 0);

            dateInfo.Era.ShouldBe(Era.BeforeChrist);
            dateInfo.Year.ShouldBe(13750000000L);
            dateInfo.Month.ShouldBe(1);
            dateInfo.Day.ShouldBe(1);
            dateInfo.Hour.ShouldBe(0);
        }

        [Fact]
        public void Year_cant_be_negative()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => { 
                new ExactDateInfo(Era.BeforeChrist, -3, 1, 1, 0); 
            });
        }

        [Fact]
        public void Year_cant_be_zero()
        {
            // see https://en.wikipedia.org/wiki/Year_zero

            Should.Throw<ArgumentOutOfRangeException>(() => {
                new ExactDateInfo(Era.BeforeChrist, 0, 1, 1, 0);
            });
        }

        [Fact]
        public void Month_should_be_between_1_and_12()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => {
                new ExactDateInfo(Era.BeforeChrist, 1, 0, 1, 0);
            });
            Should.Throw<ArgumentOutOfRangeException>(() => {
                new ExactDateInfo(Era.BeforeChrist, 1, 13, 1, 0);
            });
        }

        [Fact]
        public void Day_should_be_between_1_and_31_in_any_case()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => {
                new ExactDateInfo(Era.BeforeChrist, 1, 1, 0, 0);
            });
            Should.Throw<ArgumentOutOfRangeException>(() => {
                new ExactDateInfo(Era.BeforeChrist, 1, 1, 32, 0);
            });
        }

        [Fact]
        public void Day_should_respect_month()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => {
                new ExactDateInfo(Era.AnnoDomini, 1979, 2, 30, 0);
            });
            Should.Throw<ArgumentOutOfRangeException>(() => {
                new ExactDateInfo(Era.AnnoDomini, 1979, 4, 31, 0);
            });
        }

        [Fact]
        public void Hour_should_be_between_0_and_23()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => {
                new ExactDateInfo(Era.BeforeChrist, 1, 1, 1, -1);
            });
            Should.Throw<ArgumentOutOfRangeException>(() => {
                new ExactDateInfo(Era.BeforeChrist, 1, 1, 1, 24);
            });
        }

        [Fact]
        public void Compare_dates_in_different_eras()
        {
            ExactDateInfo.BeforeChrist(100, 1, 1, 0).ShouldBeLessThan(ExactDateInfo.AnnoDomini(100, 1, 1, 0));
            ExactDateInfo.BeforeChrist(200, 1, 1, 0).ShouldBeLessThan(ExactDateInfo.AnnoDomini(100, 1, 1, 0));
        }

        [Fact]
        public void Compare_dates_with_different_years()
        {
            ExactDateInfo.AnnoDomini(100, 1, 1, 0).ShouldBeLessThan(ExactDateInfo.AnnoDomini(200, 1, 1, 0));
            ExactDateInfo.BeforeChrist(200, 1, 1, 0).ShouldBeLessThan(ExactDateInfo.BeforeChrist(100, 1, 1, 0));
        }

        [Fact]
        public void Compare_dates_with_different_years_and_months()
        {
            ExactDateInfo.AnnoDomini(100, 2, 1, 0).ShouldBeLessThan(ExactDateInfo.AnnoDomini(100, 3, 1, 0));
            ExactDateInfo.BeforeChrist(200, 2, 1, 0).ShouldBeLessThan(ExactDateInfo.BeforeChrist(200, 3, 1, 0));
        }

        [Fact]
        public void Compare_dates_with_different_years_months_and_days()
        {
            ExactDateInfo.AnnoDomini(100, 2, 10, 0).ShouldBeLessThan(ExactDateInfo.AnnoDomini(100, 2, 15, 0));
            ExactDateInfo.BeforeChrist(200, 3, 10, 0).ShouldBeLessThan(ExactDateInfo.BeforeChrist(200, 3, 15, 0));
        }

        [Fact]
        public void Compare_dates_with_different_years_months_days_and_hours()
        {
            ExactDateInfo.AnnoDomini(100, 2, 10, 3).ShouldBeLessThan(ExactDateInfo.AnnoDomini(100, 2, 10, 5));
            ExactDateInfo.BeforeChrist(200, 3, 10, 3).ShouldBeLessThan(ExactDateInfo.BeforeChrist(200, 3, 10, 5));
        }
    }
}
