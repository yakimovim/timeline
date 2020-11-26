using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdlinSoftware.Timeline.Storage
{
    [Table("EventsSets")]
    public class EventsSet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MinLength(1)]
        public string Name { get; set; }
    }

    [Table("EventsInSets")]
    public class EventInSet
    {
        public int EventId { get; set; }
        public int SetId { get; set; }
    }
}
