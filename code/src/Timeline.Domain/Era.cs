using System;

namespace EdlinSoftware.Timeline.Domain
{
    /// <summary>
    /// Represents time era.
    /// </summary>
    public enum Era
    {
        /// <summary>
        /// Before our era.
        /// </summary>
        BeforeChrist = 0,
        /// <summary>
        /// In our era.
        /// </summary>
        AnnoDomini = 1
    }

    public static class EraExtentions
    {
        public static string ToEraString(this Era era)
        {
            switch(era)
            {
                case Era.AnnoDomini:
                    return "AD";
                case Era.BeforeChrist:
                    return "BC";
                default:
                    throw new ArgumentException("Unknown era", nameof(era));
            }
        }
    }
}
