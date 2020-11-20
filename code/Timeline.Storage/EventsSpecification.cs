using EdlinSoftware.Timeline.Domain;
using System;
using System.Linq;

namespace EdlinSoftware.Timeline.Storage
{
    /// <summary>
    /// Represents specification of <see cref="Event"/> objects.
    /// </summary>
    public abstract class EventsSpecification
    {
        /// <summary>
        /// Augments events query.
        /// </summary>
        /// <param name="query">Events query.</param>
        public abstract IQueryable<Event> AugmentQuery(IQueryable<Event> query);
    }

    /// <summary>
    /// Applies several specifications one after another.
    /// </summary>
    public sealed class CompositeEventsSpecification : EventsSpecification
    {
        private readonly EventsSpecification[] _specifications;

        public CompositeEventsSpecification(params EventsSpecification[] specifications)
        {
            _specifications = specifications;
        }

        /// <inheritdoc />
        public override IQueryable<Event> AugmentQuery(IQueryable<Event> query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            foreach (var specification in _specifications)
            {
                query = specification.AugmentQuery(query);
            }

            return query;
        }
    }

    public sealed class TimeRangeEventsSpecification : EventsSpecification
    {
        private readonly TimeRange _timeRange;

        public TimeRangeEventsSpecification(TimeRange timeRange)
        {
            _timeRange = timeRange ?? throw new ArgumentNullException(nameof(timeRange));
        }

        /// <inheritdoc />
        public override IQueryable<Event> AugmentQuery(IQueryable<Event> query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            long timeRangeStartDuration = Duration.GetDurationFromChristBirth(_timeRange.Start);
            long timeRangeEndDuration = Duration.GetDurationFromChristBirth(_timeRange.End);

            var now = DateTime.Now;
            long nowDuration = Duration.GetDurationFromChristBirth(
                new ExactDateInfo(
                    Era.AnnoDomini,
                    now.Year,
                    now.Month,
                    now.Day,
                    now.Hour
                )
            );

            return query.Where(e => !(
                ( // start date of event is greater than time range end
                    ( // start date of event is current date
                        e.StartIsCurrent &&
                        (nowDuration >= timeRangeEndDuration)
                    ) ||
                    ( // start date of event is not current date
                        !e.StartIsCurrent &&
                        e.StartDuration >= timeRangeEndDuration
                    )
                ) ||
                ( // end date of event is less than time range start
                    ( // end is null
                        (e.EndIsCurrent == null) &&
                        ( // use start date of event as itsend date
                            ( // start date of event is current date
                                e.StartIsCurrent &&
                                (nowDuration <= timeRangeStartDuration)
                            ) ||
                            ( // start date of event is not current date
                                !e.StartIsCurrent &&
                                (e.StartDuration <= timeRangeStartDuration)
                            )
                        )
                    ) ||
                    ( // end is not null
                        (e.EndIsCurrent != null) && 
                        (
                            ( // end date of event is current date
                                e.EndIsCurrent.Value &&
                                (nowDuration <= timeRangeStartDuration)
                            ) ||
                            ( // end date of event is not current date
                                !e.EndIsCurrent.Value &&
                                (e.EndDuration.Value <= timeRangeStartDuration)
                            )
                        )
                    )
                )
            )); 
        }
    }

    public sealed class PlaceWithoutParentsEventsSpecification : EventsSpecification
    {
        private readonly HierarchyNode<string> _place;

        public PlaceWithoutParentsEventsSpecification(HierarchyNode<string> place)
        {
            _place = place ?? throw new ArgumentNullException(nameof(place));
        }

        public override IQueryable<Event> AugmentQuery(IQueryable<Event> query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return query.Where(e => e.Place.Left >= _place.Left && e.Place.Right <= _place.Right);
        }
    }
}
