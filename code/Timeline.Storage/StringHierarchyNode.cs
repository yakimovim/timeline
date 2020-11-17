using System.ComponentModel.DataAnnotations;

namespace EdlinSoftware.Timeline.Storage
{
    /// <summary>
    /// Model of hierarchy node with string content for storage.
    /// </summary>
    public abstract class StringHierarchyNode
    {
        [Key]
        [Required]
        [RegularExpression("^[a-zA-Z_0-9]+$")]
        [MaxLength(100)]
        public string Id { get; set; }

        [Required]
        public string Content { get; set; }

        public int Left { get; set; }

        public int Right { get; set; }
    }
}
