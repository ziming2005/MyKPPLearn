using System;

namespace MyKPPLearn.Common
{
    /// <summary>Small display helpers shared by Learning pages.</summary>
    public static class LearningDisplay
    {
        /// <summary>Whole-number percentage; 0 when total is 0 (no divide-by-zero).</summary>
        public static int Percent(int completed, int total)
        {
            if (total <= 0)
            {
                return 0;
            }

            return (int)Math.Round(completed * 100.0 / total, MidpointRounding.AwayFromZero);
        }
    }
}
