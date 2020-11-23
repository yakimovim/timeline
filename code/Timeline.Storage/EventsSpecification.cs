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
                    Expression.And(
                        parameterReplacer.ReplaceParameterWith(parameter, result.Body),
                        parameterReplacer.ReplaceParameterWith(parameter, filterExpression.Body)
                    ),
                    parameter);
            }

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
}
