using Shouldly;
using Xunit;

namespace EdlinSoftware.Timeline.Domain.Tests
{
    public class EventsDistributorTests
    {
        private readonly Duration _interval;
        private readonly EventsDistributor _eventsDistributor;

        public EventsDistributorTests()
        {
            _interval = Duration.Zero.AddYears(1);
            _eventsDistributor = new EventsDistributor(_interval);
        }

        [Fact]
        public void Distribute_no_events()
        {
            var distribution = Distribute();

            distribution.ShouldNotBeNull();

            distribution.Lines.Count.ShouldBe(0);
        }

        [Fact]
        public void Distribute_single_point_event()
        {
            var e = new Event<string, string>(
                "1",
                SpecificDate.AnnoDomini(2020),
                SpecificDate.AnnoDomini(2020, 3)
            );

            var distribution = Distribute(e);

            distribution.ShouldNotBeNull();

            distribution.Lines.Count.ShouldBe(1);

            distribution.Lines[0].Events.Count.ShouldBe(1);

            distribution.Lines[0].Events[0].ShouldBeSameAs(e);
        }

        [Fact]
        public void Distribute_single_interval_event()
        {
            var e = new Event<string, string>(
                "1",
                SpecificDate.AnnoDomini(2020),
                SpecificDate.AnnoDomini(2022)
            );

            var distribution = Distribute(e);

            distribution.ShouldNotBeNull();

            distribution.Lines.Count.ShouldBe(1);

            distribution.Lines[0].Events.Count.ShouldBe(1);

            distribution.Lines[0].Events[0].ShouldBeSameAs(e);
        }

        [Fact]
        public void Point_and_interval_events_should_be_in_separate_lines()
        {
            var e1 = new Event<string, string>(
                "1",
                SpecificDate.AnnoDomini(2020),
                SpecificDate.AnnoDomini(2020, 3)
            );
            var e2 = new Event<string, string>(
                "2",
                SpecificDate.AnnoDomini(2020),
                SpecificDate.AnnoDomini(2022)
            );

            var distribution = Distribute(e1, e2);

            distribution.ShouldNotBeNull();

            distribution.Lines.Count.ShouldBe(2);

            distribution.Lines[0].Events.Count.ShouldBe(1);

            distribution.Lines[0].Events[0].ShouldBeSameAs(e1);

            distribution.Lines[1].Events.Count.ShouldBe(1);

            distribution.Lines[1].Events[0].ShouldBeSameAs(e2);
        }

        [Fact]
        public void Some_point_events_can_be_dropped_if_distance_between_them_is_less_than_threshold()
        {
            var e1 = new Event<string, string>(
                "1",
                SpecificDate.AnnoDomini(2020, 2),
                SpecificDate.AnnoDomini(2020, 3)
            );
            var e2 = new Event<string, string>(
                "2",
                SpecificDate.AnnoDomini(2020)
            );
            var e3 = new Event<string, string>(
                "3",
                SpecificDate.AnnoDomini(2022, 1),
                SpecificDate.AnnoDomini(2022, 2)
            );

            var distribution = Distribute(e1, e2, e3);

            distribution.ShouldNotBeNull();

            distribution.Lines.Count.ShouldBe(1);

            distribution.Lines[0].Events.Count.ShouldBe(2);

            distribution.Lines[0].Events[0].ShouldBeSameAs(e2);

            distribution.Lines[0].Events[1].ShouldBeSameAs(e3);
        }

        [Fact]
        public void Overlapping_interval_events_must_be_separated_to_several_lines()
        {
            var e1 = new Event<string, string>(
                "1",
                SpecificDate.AnnoDomini(2020),
                SpecificDate.AnnoDomini(2022)
            );
            var e2 = new Event<string, string>(
                "2",
                SpecificDate.AnnoDomini(2021),
                SpecificDate.AnnoDomini(2023)
            );
            var e3 = new Event<string, string>(
                "3",
                SpecificDate.AnnoDomini(2021),
                SpecificDate.AnnoDomini(2024)
            );

            var distribution = Distribute(e1, e2, e3);

            distribution.ShouldNotBeNull();

            distribution.Lines.Count.ShouldBe(3);

            distribution.Lines[0].Events.Count.ShouldBe(1);

            distribution.Lines[0].Events[0].ShouldBeSameAs(e1);

            distribution.Lines[1].Events.Count.ShouldBe(1);

            distribution.Lines[1].Events[0].ShouldBeSameAs(e2);

            distribution.Lines[2].Events.Count.ShouldBe(1);

            distribution.Lines[2].Events[0].ShouldBeSameAs(e3);
        }

        [Fact]
        public void Non_overlapping_interval_events_must_be_in_single_line()
        {
            var e1 = new Event<string, string>(
                "1",
                SpecificDate.AnnoDomini(2020),
                SpecificDate.AnnoDomini(2022)
            );
            var e2 = new Event<string, string>(
                "2",
                SpecificDate.AnnoDomini(2021),
                SpecificDate.AnnoDomini(2023)
            );
            var e3 = new Event<string, string>(
                "3",
                SpecificDate.AnnoDomini(2023),
                SpecificDate.AnnoDomini(2025)
            );

            var distribution = Distribute(e1, e2, e3);

            distribution.ShouldNotBeNull();

            distribution.Lines.Count.ShouldBe(2);

            distribution.Lines[0].Events.Count.ShouldBe(2);

            distribution.Lines[0].Events[0].ShouldBeSameAs(e1);

            distribution.Lines[0].Events[1].ShouldBeSameAs(e3);

            distribution.Lines[1].Events.Count.ShouldBe(1);

            distribution.Lines[1].Events[0].ShouldBeSameAs(e2);
        }

        private EventsDistribution<string, string> Distribute(params Event<string, string>[] events)
        {
            return _eventsDistributor.Distribute(events);
        }
    }
}
