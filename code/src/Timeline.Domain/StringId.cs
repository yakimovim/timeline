using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EdlinSoftware.Timeline.Domain
{
    public sealed class StringId : ValueObject
    {
        private readonly static Regex _validationRegex = new Regex("^[a-zA-Z_0-9\\-]+$");

        public StringId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace", nameof(id));
            }

            if(!_validationRegex.IsMatch(id))
            {
                throw new ArgumentException($"'{nameof(id)}' must contain only latin letters, digits and underscore", nameof(id));
            }

            if(id.Length > 100)
            {
                throw new ArgumentException($"'{nameof(id)}' must contain less than 100 symbols", nameof(id));
            }

            Id = id;
        }

        public string Id { get; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }

        public override string ToString() => Id;

        public static implicit operator string(StringId id)
        {
            return id.Id;
        }

        public static implicit operator StringId(string id)
        {
            return new StringId(id);
        }

    }
}
