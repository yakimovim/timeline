using EdlinSoftware.Timeline.Domain;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace EdlinSoftware.Timeline.Storage
{
    /// <summary>
    /// Represents specification of <see cref="Event"/> objects.
    /// </summary>
    public abstract class EventsSpecification
    {
        /// <summary>
        /// Returns filter expression.
        /// </summary>
        public abstract Expression<Func<Event, bool>> GetFilterExpression();

        /// <summary>
        /// Return time range specification.
        /// </summary>
        /// <param name="timeRange">Time range.</param>
        public static EventsSpecification InRange(TimeRange timeRange)
            => new TimeRangeEventsSpecification(timeRange);

        /// <summary>
        /// Return time range specification.
        /// </summary>
        /// <param name="rangeStart">Start of time range.</param>
        /// <param name="rangeEnd">End of time range.</param>
        public static EventsSpecification InRange(ExactDateInfo rangeStart, ExactDateInfo rangeEnd)
            => new TimeRangeEventsSpecification(new TimeRange(rangeStart, rangeEnd));

        /// <summary>
        /// Returns place specification without parents.
        /// </summary>
        /// <param name="place">Place.</param>
        public static EventsSpecification InPlace(HierarchyNode<string> place)
            => new PlaceWithoutParentsEventsSpecification(place);

        /// <summary>
        /// Returns place specification with parents.
        /// </summary>
        /// <param name="place">Place.</param>
        public static EventsSpecification InPlaceWithParents(HierarchyNode<string> place)
            => new PlaceWithParentsEventsSpecification(place);

        /// <summary>
        /// Returns exact place specification.
        /// </summary>
        /// <param name="place">Place.</param>
        public static EventsSpecification InExactPlace(HierarchyNode<string> place)
            => new ExactPlaceEventsSpecification(place);


        /// <summary>
        /// Returns ids specification.
        /// </summary>
        /// <param name="ids">Ids of events.</param>
        public static EventsSpecification Ids(params int[] ids)
            => new IdsEventsSpecification(ids);


        /// <summary>
        /// Combines this specification with another one
        /// using AND operator.
        /// </summary>
        /// <param name="anotherSpec">Another specification.</param>
        public EventsSpecification And(EventsSpecification anotherSpec)
            => new AndEventsSpecification(this, anotherSpec);

        /// <summary>
        /// Combines this specification with another one
        /// using OR operator.
        /// </summary>
        /// <param name="anotherSpec">Another specification.</param>
        public EventsSpecification Or(EventsSpecification anotherSpec)
            => new OrEventsSpecification(this, anotherSpec);

        /// <summary>
        /// Reverts specification.
        /// </summary>
        /// <param name="spec">Specification.</param>
        public static EventsSpecification Not(EventsSpecification spec)
            => new NotEventsSpecification(spec);
    }

    internal class ParameterReplacer : ExpressionVisitor
    {
        private ParameterExpression _parameter;

        public Expression ReplaceParameterWith(
            ParameterExpression parameter,
            Expression bodyExpr)
        {
            if (bodyExpr is null)
            {
                throw new ArgumentNullException(nameof(bodyExpr));
            }

            _parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));

            return Visit(bodyExpr);
        }

        protected override Expression VisitParameter(ParameterExpression node) => _parameter;
    }

    /// <summary>
    /// Applies several specifications combined with AND logical operator.
    /// </summary>
    public sealed class AndEventsSpecification : EventsSpecification
    {
        private readonly EventsSpecification[] _specifications;

        public AndEventsSpecification(params EventsSpecification[] specifications)
        {
            _specifications = specifications;
        }

        /// <inheritdoc />
        public override Expression<Func<Event, bool>> GetFilterExpression()
        {
            if (_specifications.Length == 0) return (e => true);

            Expression<Func<Event, bool>> result = _specifications[0].GetFilterExpression();

            var parameterReplacer = new ParameterReplacer();

            foreach (var specification in _specifications.Skip(1))
            {
                var filterExpression = specification.GetFilterExpression();

                var parameter = Expression.Parameter(typeof(Event));

                result = Expression.Lambda<Func<Event, bool>>(
                    Expression.AndAlso(
                        parameterReplacer.ReplaceParameterWith(parameter, result.Body),
                        parameterReplacer.ReplaceParameterWith(parameter, filterExpression.Body)
                    ),
                    parameter);
            }

            return result;
        }
    }

    /// <summary>
    /// Applies several specifications combined with OR logical operator.
    /// </summary>
    public sealed class OrEventsSpecification : EventsSpecification
    {
        private readonly EventsSpecification[] _specifications;

        public OrEventsSpecification(params EventsSpecification[] specifications)
        {
            _specifications = specifications;
        }

        /// <inheritdoc />
        public override Expression<Func<Event, bool>> GetFilterExpression()
        {
            if (_specifications.Length == 0) return (e => true);

            Expression<Func<Event, bool>> result = _specifications[0].GetFilterExpression();

            var parameterReplacer = new ParameterReplacer();

            foreach (var specification in _specifications.Skip(1))
            {
                var filterExpression = specification.GetFilterExpression();

                var parameter = Expression.Parameter(typeof(Event));

                result = Expression.Lambda<Func<Event, bool>>(
                    Expression.OrElse(
                        parameterReplacer.ReplaceParameterWith(parameter, result.Body),
                        parameterReplacer.ReplaceParameterWith(parameter, filterExpression.Body)
                    ),
                    parameter);
            }

            return result;
        }
    }

    /// <summary>
    /// Reverts result of a single specification.
    /// </summary>
    public sealed class NotEventsSpecification : EventsSpecification
    {
        private readonly EventsSpecification _specification;

        public NotEventsSpecification(EventsSpecification specification)
        {
            _specification = specification ?? throw new ArgumentNullException(nameof(specification));
        }

        /// <inheritdoc />
        public override Expression<Func<Event, bool>> GetFilterExpression()
        {
            var parameterReplacer = new ParameterReplacer();

            var filterExpression = _specification.GetFilterExpression();

            var parameter = Expression.Parameter(typeof(Event));

            var result = Expression.Lambda<Func<Event, bool>>(
                Expression.Not(
                    parameterReplacer.ReplaceParameterWith(parameter, filterExpression.Body)
                ),
                parameter);

            return result;
        }
    }

    /// <summary>
    /// Specification for events in given time range.
    /// </summary>
    public sealed class TimeRangeEventsSpecification : EventsSpecification
    {
        private readonly TimeRange _timeRange;

        public TimeRangeEventsSpecification(TimeRange timeRange)
        {
            _timeRange = timeRange ?? throw new ArgumentNullException(nameof(timeRange));
        }

        /// <inheritdoc />
        public override Expression<Func<Event, bool>> GetFilterExpression()
        {
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

            return (e => !(
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

    /// <summary>
    /// Specification for events in given place.
    /// It does not include "parent" places.
    /// </summary>
    public sealed class PlaceWithoutParentsEventsSpecification : EventsSpecification
    {
        private readonly HierarchyNode<string> _place;

        public PlaceWithoutParentsEventsSpecification(HierarchyNode<string> place)
        {
            _place = place ?? throw new ArgumentNullException(nameof(place));
        }

        public override Expression<Func<Event, bool>> GetFilterExpression()
        {
            return (e => e.Place.Left >= _place.Left && e.Place.Right <= _place.Right);
        }
    }

    /// <summary>
    /// Specification for events in given place.
    /// It includes "parent" places.
    /// </summary>
    public sealed class PlaceWithParentsEventsSpecification : EventsSpecification
    {
        private readonly HierarchyNode<string> _place;

        public PlaceWithParentsEventsSpecification(HierarchyNode<string> place)
        {
            _place = place ?? throw new ArgumentNullException(nameof(place));
        }

        public override Expression<Func<Event, bool>> GetFilterExpression()
        {
            return (e => 
                // self and children
                (e.Place.Left >= _place.Left && e.Place.Right <= _place.Right) || 
                // parents
                (e.Place.Left < _place.Left && e.Place.Right > _place.Right)
            );
        }
    }

    /// <summary>
    /// Specification for events in specific place.
    /// </summary>
    public sealed class ExactPlaceEventsSpecification : EventsSpecification
    {
        private readonly StringId _placeId;

        public ExactPlaceEventsSpecification(HierarchyNode<string> place)
        {
            _placeId = place?.Id ?? throw new ArgumentNullException(nameof(place));
        }

        public override Expression<Func<Event, bool>> GetFilterExpression()
        {
            return (e => e.Place.Id == _placeId.Id);
        }
    }

    /// <summary>
    /// Specification for events with given ids.
    /// </summary>
    public sealed class IdsEventsSpecification : EventsSpecification
    {
        private readonly HierarchyNode<string> _place;
        private readonly int[] _ids;

        public IdsEventsSpecification(params int[] ids)
        {
            _ids = ids;
        }

        public override Expression<Func<Event, bool>> GetFilterExpression()
        {
            return (e => _ids.Contains(e.Id));
        }
    }
}
