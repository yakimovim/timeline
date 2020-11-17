using System.ComponentModel.DataAnnotations.Schema;

namespace EdlinSoftware.Timeline.Storage
{
    [Table("places")]
    public class PlaceHierarchy : StringHierarchyNode
    { }
}
