using System;
using MathNet.Numerics.Distributions;

namespace Traumar.Core
{
    /// <summary>
    /// 提供 SEQIC 指標所需的統計計算方法。
    /// </summary>
    public static class StatHelper
    {
        /// <summary>
        /// 計算 Wilson Score 信賴區間。
        /// </summary>
        /// <param name="x">成功次數 (Numerator)</param>
        /// <param name="n">總次數 (Denominator)</param>
        /// <param name="confLevel">信賴水準 (預設 0.95)</param>
        /// <returns>包含 Lower 與 Upper 的元組</returns>
        public static (double Lower, double Upper) CalculateWilsonInterval(int x, int n, double confLevel = 0.95)
        {
            if (n <= 0) return (double.NaN, double.NaN);
            
            double p = (double)x / n;
            double alpha = 1 - confLevel;
            double z = Normal.InvCDF(0, 1, 1 - alpha / 2);
            double zSq = z * z;
            
            // 使用連續性修正 (Continuity Correction) 版本的 Wilson Score Interval
            // 這是 R 語言 prop.test 與 nemsqar 套件的預設行為
            double lower = (2 * n * p + zSq - 1 - z * Math.Sqrt(zSq - (2 + 1.0 / n) + 4 * p * (n * (1 - p) + 1))) / (2 * (n + zSq));
            double upper = (2 * n * p + zSq + 1 + z * Math.Sqrt(zSq + (2 - 1.0 / n) + 4 * p * (n * (1 - p) - 1))) / (2 * (n + zSq));

            // 處理邊界
            if (x == 0) lower = 0;
            if (x == n) upper = 1;

            return (Math.Max(0, lower), Math.Min(1, upper));
        }

        /// <summary>
        /// 計算 Clopper-Pearson 信賴區間。
        /// </summary>
        /// <param name="x">成功次數 (Numerator)</param>
        /// <param name="n">總次數 (Denominator)</param>
        /// <param name="confLevel">信賴水準 (預設 0.95)</param>
        /// <returns>包含 Lower 與 Upper 的元組</returns>
        public static (double Lower, double Upper) CalculateClopperPearsonInterval(int x, int n, double confLevel = 0.95)
        {
            if (n <= 0) return (double.NaN, double.NaN);
            if (n == 0) return (0, 1);
            
            double alpha = 1 - confLevel;
            double lower = 0;
            double upper = 1;

            if (x > 0)
            {
                lower = Beta.InvCDF(x, n - x + 1, alpha / 2);
            }

            if (x < n)
            {
                upper = Beta.InvCDF(x + 1, n - x, 1 - alpha / 2);
            }

            return (lower, upper);
        }
    }
}
