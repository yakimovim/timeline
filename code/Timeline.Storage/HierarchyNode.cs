using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Timeline.Storage
{
    public abstract class HierarchyNode
    {
        [Key]
        [Required]
        [RegularExpression("^[a-zA-Z_0-9]+$")]
        public string Id { get; set; }

        [Required]
        public string Content { get; set; }

        public int Left { get; set; }

        public int Right { get; set; }
    }
}
