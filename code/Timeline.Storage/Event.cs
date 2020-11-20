using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EdlinSoftware.Timeline.Domain;

namespace EdlinSoftware.Timeline.Storage
{
    /// <summary>
    /// Represents first null date part for <see cref="PartialDateInfo"/>
    /// </summary>
    public enum NullableDateParts
    {
        Month,
        Day,
        Hour,
        Nothing
    }

    /// <summary>
    /// Event representation for storage.
    /// </summary>
    [Table("Events")]
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public bool StartIsCurrent { get; set; }

        public long? StartDuration { get; set; }

        public NullableDateParts? StartNullPart { get; set; }

        public bool? EndIsCurrent { get; set; }

        public long? EndDuration { get; set; }

        public NullableDateParts? EndNullPart { get; set; }

        public string PlaceId { get; set; }

        public Place Place { get; set; }
    }
}
