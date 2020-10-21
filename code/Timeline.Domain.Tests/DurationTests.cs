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
            Duration.GetDurationFromChristBirth(DateInfo.AnnoDomini(1)).ShouldBe(Duration.Zero);
            Duration.GetDurationFromChristBirth(DateInfo.AnnoDomini(1, 1)).ShouldBe(Duration.Zero);
            Duration.GetDurationFromChristBirth(DateInfo.AnnoDomini(1, 1, 1)).ShouldBe(Duration.Zero);
            Duration.GetDurationFromChristBirth(DateInfo.AnnoDomini(1, 1, 1, 0)).ShouldBe(Duration.Zero);
        }

        [Fact]
        public void Duration_from_Christ_birth_of_dates_in_our_era()
        {
            Duration.GetDurationFromChristBirth(DateInfo.AnnoDomini(100)).ShouldBe(Duration.Zero.AddYears(99));
            Duration.GetDurationFromChristBirth(DateInfo.AnnoDomini(15, 3)).ShouldBe(Duration.Zero.AddYears(14).AddMonths(2));
            Duration.GetDurationFromChristBirth(DateInfo.AnnoDomini(1941, 6, 21)).ShouldBe(Duration.Zero.AddYears(1940).AddMonths(5).AddDays(20));
            Duration.GetDurationFromChristBirth(DateInfo.AnnoDomini(1, 7, 5, 13)).ShouldBe(Duration.Zero.AddMonths(6).AddDays(4).AddHours(13));
        }

        [Fact]
        public void Duration_from_Christ_birth_of_dates_in_previous_era()
        {
            Duration.GetDurationFromChristBirth(DateInfo.BeforeChrist(11)).ShouldBe(Duration.Zero.AddYears(-11));
            Duration.GetDurationFromChristBirth(DateInfo.BeforeChrist(300, 4)).ShouldBe(Duration.Zero.AddYears(-300).AddMonths(3));
            Duration.GetDurationFromChristBirth(DateInfo.BeforeChrist(4000, 9, 10)).ShouldBe(Duration.Zero.AddYears(-4000).AddMonths(8).AddDays(9));
            Duration.GetDurationFromChristBirth(DateInfo.BeforeChrist(57, 3, 22, 5)).ShouldBe(Duration.Zero.AddYears(-57).AddMonths(2).AddDays(21).AddHours(5));
        }

        [Theory]
        [MemberData(nameof(DurationEqualityData))]
        public void Duration_equality(
            DateInfo date1, 
            DateInfo date2,
            DateInfo date3,
            DateInfo date4,
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
                DateInfo.AnnoDomini(99),
                DateInfo.AnnoDomini(90),
                DateInfo.AnnoDomini(1945),
                DateInfo.AnnoDomini(1936),
                true
            };
            yield return new object[]
            {
                DateInfo.AnnoDomini(70, 6),
                DateInfo.AnnoDomini(55, 3),
                DateInfo.AnnoDomini(4, 9),
                DateInfo.BeforeChrist(12, 6),
                true
            };
            yield return new object[]
            {
                DateInfo.AnnoDomini(400, 3, 15),
                DateInfo.AnnoDomini(411, 1, 5),
                DateInfo.BeforeChrist(333, 9, 20),
                DateInfo.BeforeChrist(322, 7, 10),
                true
            };
            yield return new object[]
            {
                DateInfo.AnnoDomini(2000, 10, 7, 12),
                DateInfo.AnnoDomini(2000, 10, 7, 5),
                DateInfo.BeforeChrist(2000, 10, 6, 2),
                DateInfo.BeforeChrist(2000, 10, 5, 19),
                true
            };
            yield return new object[]
            {
                DateInfo.AnnoDomini(2000, 10, 7, 12),
                DateInfo.AnnoDomini(2000, 10, 7, 5),
                DateInfo.AnnoDomini(2000, 10, 7, 13),
                DateInfo.AnnoDomini(2000, 10, 7, 5),
                false
            };
        }
    }
}
