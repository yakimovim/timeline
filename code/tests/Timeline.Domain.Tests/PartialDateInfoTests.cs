﻿using Shouldly;
using System;
using Xunit;

namespace EdlinSoftware.Timeline.Domain.Tests
{
    public class PartialDateInfoTests
    {
        [Fact]
        public void Create_universe_start_date()
        {
            var dateInfo = new PartialDateInfo(Era.BeforeChrist, 13750000000L);

            dateInfo.Era.ShouldBe(Era.BeforeChrist);
            dateInfo.Year.ShouldBe(13750000000L);
            dateInfo.Month.ShouldBeNull();
            dateInfo.Day.ShouldBeNull();
            dateInfo.Hour.ShouldBeNull();
        }

        [Fact]
        public void Year_cant_be_negative()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => { 
                new PartialDateInfo(Era.BeforeChrist, -3); 
            });
        }

        [Fact]
        public void Year_cant_be_zero()
        {
            // see https://en.wikipedia.org/wiki/Year_zero

            Should.Throw<ArgumentOutOfRangeException>(() => {
                new PartialDateInfo(Era.BeforeChrist, 0);
            });
        }

        [Fact]
        public void Month_should_be_between_1_and_12()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => {
                new PartialDateInfo(Era.BeforeChrist, 1, 0);
            });
            Should.Throw<ArgumentOutOfRangeException>(() => {
                new PartialDateInfo(Era.BeforeChrist, 1, 13);
            });
        }

        [Fact]
        public void Create_date_with_month()
        {
            var dateInfo = new PartialDateInfo(Era.AnnoDomini, 124, 3);

            dateInfo.Era.ShouldBe(Era.AnnoDomini);
            dateInfo.Year.ShouldBe(124L);
            dateInfo.Month.ShouldBe(3);
            dateInfo.Day.ShouldBeNull();
            dateInfo.Hour.ShouldBeNull();
        }

        [Fact]
        public void Day_should_be_between_1_and_31_in_any_case()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => {
                new PartialDateInfo(Era.BeforeChrist, 1, 1, 0);
            });
            Should.Throw<ArgumentOutOfRangeException>(() => {
                new PartialDateInfo(Era.BeforeChrist, 1, 1, 32);
            });
        }

        [Fact]
        public void Day_should_respect_month()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => {
                new PartialDateInfo(Era.AnnoDomini, 1979, 2, 30);
            });
            Should.Throw<ArgumentOutOfRangeException>(() => {
                new PartialDateInfo(Era.AnnoDomini, 1979, 4, 31);
            });
        }

        [Fact]
        public void Unable_to_set_day_without_month()
        {
            Should.Throw<ArgumentException>(() => {
                new PartialDateInfo(Era.AnnoDomini, 1979, null, 1);
            });
        }

        [Fact]
        public void Create_date_with_month_and_day()
        {
            var dateInfo = new PartialDateInfo(Era.AnnoDomini, 124, 3, 14);

            dateInfo.Era.ShouldBe(Era.AnnoDomini);
            dateInfo.Year.ShouldBe(124L);
            dateInfo.Month.ShouldBe(3);
            dateInfo.Day.ShouldBe(14);
            dateInfo.Hour.ShouldBeNull();
        }

        [Fact]
        public void Hour_should_be_between_0_and_23()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => {
                new PartialDateInfo(Era.BeforeChrist, 1, 1, 1, -1);
            });
            Should.Throw<ArgumentOutOfRangeException>(() => {
                new PartialDateInfo(Era.BeforeChrist, 1, 1, 1, 24);
            });
        }

        [Fact]
        public void Unable_to_set_hour_without_month_or_day()
        {
            Should.Throw<ArgumentException>(() => {
                new PartialDateInfo(Era.AnnoDomini, 1979, null, 1, 13);
            });
            Should.Throw<ArgumentException>(() => {
                new PartialDateInfo(Era.AnnoDomini, 1979, 1, null, 13);
            });
        }

        [Fact]
        public void Create_date_with_month_and_day_and_hour()
        {
            var dateInfo = new PartialDateInfo(Era.AnnoDomini, 124, 3, 14, 2);

            dateInfo.Era.ShouldBe(Era.AnnoDomini);
            dateInfo.Year.ShouldBe(124L);
            dateInfo.Month.ShouldBe(3);
            dateInfo.Day.ShouldBe(14);
            dateInfo.Hour.ShouldBe(2);
        }

        [Fact]
        public void Compare_dates_in_different_eras()
        {
            PartialDateInfo.BeforeChrist(100).ShouldBeLessThan(PartialDateInfo.AnnoDomini(100));
            PartialDateInfo.BeforeChrist(200).ShouldBeLessThan(PartialDateInfo.AnnoDomini(100));
        }

        [Fact]
        public void Compare_dates_with_different_years()
        {
            PartialDateInfo.AnnoDomini(100).ShouldBeLessThan(PartialDateInfo.AnnoDomini(200));
            PartialDateInfo.BeforeChrist(200).ShouldBeLessThan(PartialDateInfo.BeforeChrist(100));
        }

        [Fact]
        public void Compare_dates_with_different_years_and_months()
        {
            PartialDateInfo.AnnoDomini(100, 2).ShouldBeLessThan(PartialDateInfo.AnnoDomini(100, 3));
            PartialDateInfo.BeforeChrist(200, 2).ShouldBeLessThan(PartialDateInfo.BeforeChrist(200, 3));
        }

        [Fact]
        public void Compare_dates_with_only_years_and_with_years_and_months()
        {
            PartialDateInfo.AnnoDomini(100).ShouldBeLessThan(PartialDateInfo.AnnoDomini(100, 1));
            PartialDateInfo.BeforeChrist(200).ShouldBeLessThan(PartialDateInfo.BeforeChrist(200, 1));
        }

        [Fact]
        public void Compare_dates_with_different_years_months_and_days()
        {
            PartialDateInfo.AnnoDomini(100, 2, 10).ShouldBeLessThan(PartialDateInfo.AnnoDomini(100, 2, 15));
            PartialDateInfo.BeforeChrist(200, 3, 10).ShouldBeLessThan(PartialDateInfo.BeforeChrist(200, 3, 15));
        }

        [Fact]
        public void Compare_dates_with_only_part_of_years_months_and_days()
        {
            PartialDateInfo.AnnoDomini(100).ShouldBeLessThan(PartialDateInfo.AnnoDomini(100, 2, 15));
            PartialDateInfo.AnnoDomini(100, 2).ShouldBeLessThan(PartialDateInfo.AnnoDomini(100, 2, 15));
            PartialDateInfo.BeforeChrist(200).ShouldBeLessThan(PartialDateInfo.BeforeChrist(200, 3, 15));
            PartialDateInfo.BeforeChrist(200, 3).ShouldBeLessThan(PartialDateInfo.BeforeChrist(200, 3, 15));
        }

        [Fact]
        public void Compare_dates_with_different_years_months_days_and_hours()
        {
            PartialDateInfo.AnnoDomini(100, 2, 10, 3).ShouldBeLessThan(PartialDateInfo.AnnoDomini(100, 2, 10, 5));
            PartialDateInfo.BeforeChrist(200, 3, 10, 3).ShouldBeLessThan(PartialDateInfo.BeforeChrist(200, 3, 10, 5));
        }

        [Fact]
        public void Compare_dates_with_only_part_of_years_months_days_and_hours()
        {
            PartialDateInfo.AnnoDomini(100).ShouldBeLessThan(PartialDateInfo.AnnoDomini(100, 2, 10, 5));
            PartialDateInfo.AnnoDomini(100, 2).ShouldBeLessThan(PartialDateInfo.AnnoDomini(100, 2, 10, 5));
            PartialDateInfo.AnnoDomini(100, 2, 10).ShouldBeLessThan(PartialDateInfo.AnnoDomini(100, 2, 10, 5));
            PartialDateInfo.BeforeChrist(200).ShouldBeLessThan(PartialDateInfo.BeforeChrist(200, 3, 10, 5));
            PartialDateInfo.BeforeChrist(200, 3).ShouldBeLessThan(PartialDateInfo.BeforeChrist(200, 3, 10, 5));
            PartialDateInfo.BeforeChrist(200, 3, 10).ShouldBeLessThan(PartialDateInfo.BeforeChrist(200, 3, 10, 5));
        }

    }
}
