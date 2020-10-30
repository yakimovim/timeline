using Shouldly;
using System.Collections.Generic;
using Xunit;

namespace EdlinSoftware.Timeline.Domain.Tests
{
    public class DurationTests
    {
        [Fact]
        public void Duration_from_Christ_birth_of_our_era_start()
        {
            Duration.GetDurationFromChristBirth(PartialDateInfo.AnnoDomini(1)).ShouldBe(Duration.Zero);
            Duration.GetDurationFromChristBirth(PartialDateInfo.AnnoDomini(1, 1)).ShouldBe(Duration.Zero);
            Duration.GetDurationFromChristBirth(PartialDateInfo.AnnoDomini(1, 1, 1)).ShouldBe(Duration.Zero);
            Duration.GetDurationFromChristBirth(PartialDateInfo.AnnoDomini(1, 1, 1, 0)).ShouldBe(Duration.Zero);
        }

        [Fact]
        public void Duration_from_Christ_birth_of_dates_in_our_era()
        {
            Duration.GetDurationFromChristBirth(PartialDateInfo.AnnoDomini(100)).ShouldBe(Duration.Zero.AddYears(99));
            Duration.GetDurationFromChristBirth(PartialDateInfo.AnnoDomini(15, 3)).ShouldBe(Duration.Zero.AddYears(14).AddMonths(2));
            Duration.GetDurationFromChristBirth(PartialDateInfo.AnnoDomini(1941, 6, 21)).ShouldBe(Duration.Zero.AddYears(1940).AddMonths(5).AddDays(20));
            Duration.GetDurationFromChristBirth(PartialDateInfo.AnnoDomini(1, 7, 5, 13)).ShouldBe(Duration.Zero.AddMonths(6).AddDays(4).AddHours(13));
        }

        [Fact]
        public void Duration_from_Christ_birth_of_dates_in_previous_era()
        {
            Duration.GetDurationFromChristBirth(PartialDateInfo.BeforeChrist(11)).ShouldBe(Duration.Zero.AddYears(-11));
            Duration.GetDurationFromChristBirth(PartialDateInfo.BeforeChrist(300, 4)).ShouldBe(Duration.Zero.AddYears(-300).AddMonths(3));
            Duration.GetDurationFromChristBirth(PartialDateInfo.BeforeChrist(4000, 9, 10)).ShouldBe(Duration.Zero.AddYears(-4000).AddMonths(8).AddDays(9));
            Duration.GetDurationFromChristBirth(PartialDateInfo.BeforeChrist(57, 3, 22, 5)).ShouldBe(Duration.Zero.AddYears(-57).AddMonths(2).AddDays(21).AddHours(5));
        }

        [Theory]
        [MemberData(nameof(DurationEqualityData))]
        public void Duration_equality(
            PartialDateInfo date1, 
            PartialDateInfo date2,
            PartialDateInfo date3,
            PartialDateInfo date4,
            bool equals)
        {
            var d1 = Duration.GetDurationFromChristBirth(date1)
                - Duration.GetDurationFromChristBirth(date2);
            var d2 = Duration.GetDurationFromChristBirth(date3)
                - Duration.GetDurationFromChristBirth(date4);

            (d1 == d2).ShouldBe(equals);
        }

        [Fact]
        public void Duration_comparison()
        {
            Duration.Zero.AddYears(10).ShouldBeGreaterThan(Duration.Zero.AddYears(9));
            Duration.Zero.ShouldBeGreaterThan(Duration.Zero.AddDays(-1));
        }

        public static IEnumerable<object[]> DurationEqualityData()
        {
            yield return new object[]
            {
                PartialDateInfo.AnnoDomini(99),
                PartialDateInfo.AnnoDomini(90),
                PartialDateInfo.AnnoDomini(1945),
                PartialDateInfo.AnnoDomini(1936),
                true
            };
            yield return new object[]
            {
                PartialDateInfo.AnnoDomini(70, 6),
                PartialDateInfo.AnnoDomini(55, 3),
                PartialDateInfo.AnnoDomini(4, 9),
                PartialDateInfo.BeforeChrist(12, 6),
                true
            };
            yield return new object[]
            {
                PartialDateInfo.AnnoDomini(400, 3, 15),
                PartialDateInfo.AnnoDomini(411, 1, 5),
                PartialDateInfo.BeforeChrist(333, 9, 20),
                PartialDateInfo.BeforeChrist(322, 7, 10),
                true
            };
            yield return new object[]
            {
                PartialDateInfo.AnnoDomini(2000, 10, 7, 12),
                PartialDateInfo.AnnoDomini(2000, 10, 7, 5),
                PartialDateInfo.BeforeChrist(2000, 10, 6, 2),
                PartialDateInfo.BeforeChrist(2000, 10, 5, 19),
                true
            };
            yield return new object[]
            {
                PartialDateInfo.AnnoDomini(2000, 10, 7, 12),
                PartialDateInfo.AnnoDomini(2000, 10, 7, 5),
                PartialDateInfo.AnnoDomini(2000, 10, 7, 13),
                PartialDateInfo.AnnoDomini(2000, 10, 7, 5),
                false
            };
        }

        [Theory]
        [MemberData(nameof(DurationToDateData))]
        public void Convert_duration_to_date_after_Christ_birth(
            Duration duration, ExactDateInfo date
            )
        {
            duration.GetDateAfterChristBirth().ShouldBe(date);
        }

        public static IEnumerable<object[]> DurationToDateData()
        {
            yield return new object[]
            {
                Duration.Zero,
                ExactDateInfo.AnnoDomini(1, 1, 1, 0)
            };
            yield return new object[]
            {
                Duration.Zero.AddYears(5),
                ExactDateInfo.AnnoDomini(6, 1, 1, 0)
            };
            yield return new object[]
            {
                Duration.Zero.AddMonths(3),
                ExactDateInfo.AnnoDomini(1, 4, 1, 0)
            };
            yield return new object[]
            {
                Duration.Zero.AddDays(13),
                ExactDateInfo.AnnoDomini(1, 1, 14, 0)
            };
            yield return new object[]
            {
                Duration.Zero.AddHours(20),
                ExactDateInfo.AnnoDomini(1, 1, 1, 20)
            };
            yield return new object[]
            {
                Duration.Zero
                    .AddYears(1000000 - 1)
                    .AddMonths(12 - 1)
                    .AddDays(31 - 1)
                    .AddHours(23),
                ExactDateInfo.AnnoDomini(1000000, 12, 31, 23)
            };
            yield return new object[]
            {
                Duration.Zero.AddYears(-1),
                ExactDateInfo.BeforeChrist(1, 1, 1, 0)
            };
            yield return new object[]
            {
                Duration.Zero.AddYears(-5).AddMonths(4),
                ExactDateInfo.BeforeChrist(5, 5, 1, 0)
            };
            yield return new object[]
            {
                Duration.Zero.AddYears(-5).AddDays(12),
                ExactDateInfo.BeforeChrist(5, 1, 13, 0)
            };
            yield return new object[]
            {
                Duration.Zero.AddYears(-5).AddHours(21),
                ExactDateInfo.BeforeChrist(5, 1, 1, 21)
            };
            yield return new object[]
            {
                Duration.Zero
                    .AddYears(-5000000)
                    .AddMonths(12 - 1)
                    .AddDays(31 - 1)
                    .AddHours(23),
                ExactDateInfo.BeforeChrist(5000000, 12, 31, 23)
            };
            yield return new object[]
            {
                Duration.Zero
                    .AddYears(1979)
                    .AddMonths(1)
                    .AddDays(29)
                    .AddHours(1),
                ExactDateInfo.AnnoDomini(1980, 3, 1, 1)
            };
        }
    }
}
