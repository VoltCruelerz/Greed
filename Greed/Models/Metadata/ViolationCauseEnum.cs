using System.ComponentModel;

namespace Greed.Models.Metadata
{
    public enum ViolationCauseEnum
    {
        [Description("The mod needs a newer version of Greed than you have installed.")]
        LiveGreedTooOld = 0,

        [Description("The mod relies on an old version of Greed, and there have been breaking changes since then in your installed Greed.")]
        ModGreedTooOld = 1,

        [Description("The mod needs a newer version of Sins II than you have installed.")]
        LiveSinsTooOld = 2,

        [Description("The mod relies on an old version of Sins II, and there have been breaking changes since then in your installed Sins II.")]
        ModSinsTooOld = 3
    }
}
