using FrameForge.Foundation.FixedPoint;
using Xunit;

namespace FrameForge.Foundation.FixedPoint.Tests
{

/// <summary>
/// Test helper extensions for FP assertions.
/// </summary>
public static class FPTestExtensions
{
    /// <summary>
    /// Asserts that two FP values are approximately equal within the given tolerance.
    /// </summary>
    public static void ShouldBeApproximately(this FP actual, FP expected, FP tolerance)
    {
        Assert.True(FP.Approximately(actual, expected, tolerance),
            $"Expected ~{expected}, got {actual} (diff: {FP.Abs(actual - expected)})");
    }

    /// <summary>
    /// Default approximate comparison suitable for arithmetic results.
    /// Tolerance: Epsilon * 100 (~2.3e-8).
    /// </summary>
    public static void ShouldBeApproximately(this FP actual, FP expected)
    {
        ShouldBeApproximately(actual, expected, FP.Epsilon * FP.FromInt(100));
    }

    /// <summary>
    /// Approximate comparison for trig results. Per spec: &lt; 1 × 10⁻⁶.
    /// </summary>
    public static void ShouldBeApproximatelyTrig(this FP actual, FP expected)
    {
        // 1e-6 * 2^32 ≈ 4295 raw units
        ShouldBeApproximately(actual, expected, FP.Epsilon * FP.FromInt(5000));
    }

    /// <summary>
    /// Approximate comparison for Exp/Log results. Per spec: &lt; 1 × 10⁻⁵.
    /// </summary>
    public static void ShouldBeApproximatelyTrans(this FP actual, FP expected)
    {
        // 1e-5 * 2^32 ≈ 42950 raw units
        ShouldBeApproximately(actual, expected, FP.Epsilon * FP.FromInt(50000));
    }
}
}
