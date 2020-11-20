using System.ComponentModel.DataAnnotations.Schema;

namespace EdlinSoftware.Timeline.Storage
{
    [Table("Places")]
    public class Place : StringHierarchyNode
    { }
}
