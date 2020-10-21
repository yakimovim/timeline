using Shouldly;
using System;
using Xunit;

namespace EdlinSoftware.Timeline.Domain.Tests
{
    public class EventTests
    {
        [Fact]
        public void Cant_create_event_without_start()
        {
            Should.Throw<ArgumentNullException>(() =>
            {
                new Event<string>("Event", null);
            });
        }

        [Fact]
        public void Cant_create_event_with_start_greater_then_end()
        {
            Should.Throw<ArgumentException>(() =>
            {
                new Event<string>("Event", SpecificDate.AnnoDomini(2020), SpecificDate.AnnoDomini(2000));
            });
        }

        [Fact]
        public void Cant_set_null_start()
        {
            var @event = new Event<string>(
                "Event", 
                SpecificDate.AnnoDomini(2000), 
                SpecificDate.AnnoDomini(2020)
            );

            Should.Throw<ArgumentException>(() =>
            {
                @event.SetInterval(null);
            });
        }

        [Fact]
        public void Cant_set_start_greater_then_end()
        {
            var @event = new Event<string>(
                "Event",
                SpecificDate.AnnoDomini(2000),
                SpecificDate.AnnoDomini(2020)
            );

            Should.Throw<ArgumentException>(() =>
            {
                @event.SetInterval(
                    SpecificDate.AnnoDomini(2020),
                    SpecificDate.AnnoDomini(2000)
                );
            });
        }

        [Fact]
        public void Event_with_start_and_end_has_correct_date_related_properties()
        {
            var @event = new Event<string>(
                "Event",
                SpecificDate.AnnoDomini(2000),
                SpecificDate.AnnoDomini(2020)
            );

            @event.Start.ShouldBe(SpecificDate.AnnoDomini(2000));
            @event.End.ShouldBe(SpecificDate.AnnoDomini(2020));
            @event.Duration.ShouldBe(Duration.Zero.AddYears(20));
        }

        [Fact]
        public void Event_with_only_start_has_correct_date_related_properties()
        {
            var @event = new Event<string>(
                "Event",
                SpecificDate.AnnoDomini(2000)
            );

            @event.Start.ShouldBe(SpecificDate.AnnoDomini(2000));
            @event.End.ShouldBeNull();
            @event.Duration.ShouldBe(Duration.Zero);
        }
    }
}
