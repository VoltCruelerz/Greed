namespace Greed.Models.Json
{
    public enum SourceType
    {
        /// <summary>
        /// Initialize from Gold. Replace elements. Null removes.
        /// </summary>
        MergeReplace,

        /// <summary>
        /// Initialize from Gold. Merge and concatenate new elements. Null removes.
        /// </summary>
        MergeConcat,

        /// <summary>
        /// Initialize from Gold. Merge and union new elements (skip old ones). Null removes.
        /// </summary>
        MergeUnion,

        /// <summary>
        /// Fully overwrite what came before.
        /// </summary>
        Overwrite
    }
}
