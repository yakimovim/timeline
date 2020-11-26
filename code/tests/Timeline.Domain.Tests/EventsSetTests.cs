using Shouldly;
using Xunit;

namespace EdlinSoftware.Timeline.Domain.Tests
{
    public class EventsSetTests
    {
        [Fact]
        public void Empty_set()
        {
            var set = new EventsSet<string, string>("S");

            set.Events.ShouldBeEmpty();
        }

        [Fact]
        public void Add_events()
        {
            var set = new EventsSet<string, string>("S");

            var e1 = new Event<string, string>("A", SpecificDate.AnnoDomini(100));
            var e2 = new Event<string, string>("B", SpecificDate.AnnoDomini(100));

            set.Add(e1, e2);

            set.Events.Count.ShouldBe(2);

            set.Events.ShouldContain(e => e == e1);
            set.Events.ShouldContain(e => e == e2);
        }

        [Fact]
        public void Should_not_add_duplicates()
        {
            var set = new EventsSet<string, string>("S");

            var e1 = new Event<string, string>("A", SpecificDate.AnnoDomini(100));
            var e2 = new Event<string, string>("B", SpecificDate.AnnoDomini(100));

            set.Add(e1, e2);

            set.Add(e1);

            set.Events.Count.ShouldBe(2);

            set.Events.ShouldContain(e => e == e1);
            set.Events.ShouldContain(e => e == e2);
        }

        [Fact]
        public void Remove_events()
        {
            var set = new EventsSet<string, string>("S");

            var e1 = new Event<string, string>("A", SpecificDate.AnnoDomini(100));
            var e2 = new Event<string, string>("B", SpecificDate.AnnoDomini(100));

            set.Add(e1, e2);

            set.Remove(e1);

            set.Events.Count.ShouldBe(1);

            set.Events.ShouldContain(e => e == e2);
        }
    }
}
