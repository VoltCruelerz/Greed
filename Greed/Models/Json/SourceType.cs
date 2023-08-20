namespace Greed.Models.Json
{
    public enum SourceType
    {
        /// <summary>
        /// Replace elements. Null removes.
        /// </summary>
        MergeReplace,

        /// <summary>
        /// Merge and concatenate new elements. Null removes.
        /// </summary>
        MergeConcat,

        /// <summary>
        /// Merge and union new elements (skip old ones). Null removes.
        /// </summary>
        MergeUnion,

        /// <summary>
        /// Fully overwrite what came before.
        /// </summary>
        Overwrite
    }
}
