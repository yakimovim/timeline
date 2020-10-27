using System;

namespace EdlinSoftware.Timeline.Domain
{
    /// <summary>
    /// Represents single tick on the time axis.
    /// </summary>
    public class Tick
    {
        private readonly string _name;

        public Tick(ExactDateInfo date, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Date = date;
            _name = name;
        }

        /// <summary>
        /// Date of the tick.
        /// </summary>
        public ExactDateInfo Date { get; }

        public override string ToString() => _name;
    }
}
