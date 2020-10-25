using Shouldly;
using System;
using System.Collections.Generic;
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

        [Theory]
        [MemberData(nameof(EventsOverlappingData))]
        public void Events_overlapping(
            Date start1,
            Date end1,
            Date start2,
            Date end2,
            bool overlaps
            )
        {
            var e1 = new Event<string>(
                "Event1",
                start1,
                end1
            );

            var e2 = new Event<string>(
                "Event2",
                start2,
                end2
            );

            e1.OverlapsWith(e2).ShouldBe(overlaps);
        }

        public static IEnumerable<object[]> EventsOverlappingData()
        {
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                null,
                SpecificDate.AnnoDomini(200),
                null,
                false
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(200),
                null,
                SpecificDate.AnnoDomini(100),
                null,
                false
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                null,
                SpecificDate.AnnoDomini(100),
                null,
                true
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                null,
                SpecificDate.AnnoDomini(100, 1),
                null,
                true
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                null,
                SpecificDate.AnnoDomini(200),
                SpecificDate.AnnoDomini(201),
                false
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                null,
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                true
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100, 2),
                null,
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                true
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(101),
                null,
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                false
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(102),
                null,
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                false
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                SpecificDate.AnnoDomini(80),
                null,
                false
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                SpecificDate.AnnoDomini(100),
                null,
                true
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                SpecificDate.AnnoDomini(100, 4),
                null,
                true
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                SpecificDate.AnnoDomini(101),
                null,
                false
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                SpecificDate.AnnoDomini(105),
                null,
                false
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                SpecificDate.AnnoDomini(80),
                SpecificDate.AnnoDomini(81),
                false
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                SpecificDate.AnnoDomini(80),
                SpecificDate.AnnoDomini(100),
                false
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                SpecificDate.AnnoDomini(80),
                SpecificDate.AnnoDomini(100, 4),
                true
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                SpecificDate.AnnoDomini(80),
                SpecificDate.AnnoDomini(105),
                true
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(100, 5),
                true
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                true
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(105),
                true
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                SpecificDate.AnnoDomini(100, 4),
                SpecificDate.AnnoDomini(100, 6),
                true
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                SpecificDate.AnnoDomini(100, 4),
                SpecificDate.AnnoDomini(101),
                true
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                SpecificDate.AnnoDomini(100, 4),
                SpecificDate.AnnoDomini(105),
                true
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                SpecificDate.AnnoDomini(101),
                SpecificDate.AnnoDomini(105),
                false
            };
            yield return new object[]
            {
                SpecificDate.AnnoDomini(100),
                SpecificDate.AnnoDomini(101),
                SpecificDate.AnnoDomini(102),
                SpecificDate.AnnoDomini(105),
                false
            };
        }
    }
}
