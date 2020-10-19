using Shouldly;
using Xunit;

namespace EdlinSoftware.Timeline.Domain.Tests
{
    public class DurationTests
    {
        [Fact]
        public void Duration_from_Christ_birth()
        {
            Duration.GetDurationFromChristBirth(DateInfo.AnnoDomini(1)).ShouldBe(Duration.Zero);
        }
    }
}
