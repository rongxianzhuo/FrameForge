using FrameForge.Foundation.FixedPoint;
using FrameForge.Foundation.Math;
using Xunit;

namespace FrameForge.Foundation.Math.Tests;

/// <summary>
/// Test helper extensions for fixed-point math assertions.
/// </summary>
public static class TestExtensions
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
    /// Tolerance: Epsilon * 500 (~1.2e-7).
    /// </summary>
    public static void ShouldBeApproximately(this FP actual, FP expected)
    {
        ShouldBeApproximately(actual, expected, DefaultFPTolerance);
    }

    /// <summary>
    /// Approximate comparison for trig results. Per spec: &lt; 1 × 10⁻⁶.
    /// </summary>
    public static void ShouldBeApproximatelyTrig(this FP actual, FP expected)
    {
        ShouldBeApproximately(actual, expected, FP.Epsilon * FP.FromInt(5000));
    }

    /// <summary>
    /// Asserts that two Vector2 values are approximately equal.
    /// </summary>
    public static void ShouldBeApproximately(this Vector2 actual, Vector2 expected, FP tolerance)
    {
        Assert.True(
            FP.Approximately(actual.X, expected.X, tolerance) &&
            FP.Approximately(actual.Y, expected.Y, tolerance),
            $"Expected ~{expected}, got {actual}");
    }

    /// <summary>
    /// Asserts that two Vector3 values are approximately equal.
    /// </summary>
    public static void ShouldBeApproximately(this Vector3 actual, Vector3 expected, FP tolerance)
    {
        Assert.True(
            FP.Approximately(actual.X, expected.X, tolerance) &&
            FP.Approximately(actual.Y, expected.Y, tolerance) &&
            FP.Approximately(actual.Z, expected.Z, tolerance),
            $"Expected ~{expected}, got {actual}");
    }

    /// <summary>
    /// Asserts that two Vector4 values are approximately equal.
    /// </summary>
    public static void ShouldBeApproximately(this Vector4 actual, Vector4 expected, FP tolerance)
    {
        Assert.True(
            FP.Approximately(actual.X, expected.X, tolerance) &&
            FP.Approximately(actual.Y, expected.Y, tolerance) &&
            FP.Approximately(actual.Z, expected.Z, tolerance) &&
            FP.Approximately(actual.W, expected.W, tolerance),
            $"Expected ~{expected}, got {actual}");
    }

    /// <summary>
    /// Asserts that two Quaternion values are approximately equal.
    /// </summary>
    public static void ShouldBeApproximately(this Quaternion actual, Quaternion expected, FP tolerance)
    {
        // Quaternion equality should account for sign ambiguity (q and -q represent same rotation)
        bool direct =
            FP.Approximately(actual.X, expected.X, tolerance) &&
            FP.Approximately(actual.Y, expected.Y, tolerance) &&
            FP.Approximately(actual.Z, expected.Z, tolerance) &&
            FP.Approximately(actual.W, expected.W, tolerance);
        bool negated =
            FP.Approximately(actual.X, -expected.X, tolerance) &&
            FP.Approximately(actual.Y, -expected.Y, tolerance) &&
            FP.Approximately(actual.Z, -expected.Z, tolerance) &&
            FP.Approximately(actual.W, -expected.W, tolerance);

        Assert.True(direct || negated,
            $"Expected ~{expected}, got {actual}");
    }

    /// <summary>
    /// Default tolerance for vector comparisons (looser for trig precision).
    /// </summary>
    private static readonly FP VecTolerance = FP.Epsilon * FP.FromInt(10000);

    /// <summary>
    /// Default tolerance for FP comparisons from vector/quaternion operations.
    /// </summary>
    private static readonly FP DefaultFPTolerance = FP.Epsilon * FP.FromInt(500);

    /// <summary>
    /// Asserts that two Vector2 values are approximately equal (default tolerance).
    /// </summary>
    public static void ShouldBeApproximately(this Vector2 actual, Vector2 expected)
    {
        ShouldBeApproximately(actual, expected, VecTolerance);
    }

    /// <summary>
    /// Asserts that two Vector3 values are approximately equal (default tolerance).
    /// </summary>
    public static void ShouldBeApproximately(this Vector3 actual, Vector3 expected)
    {
        ShouldBeApproximately(actual, expected, VecTolerance);
    }

    /// <summary>
    /// Asserts that two Vector4 values are approximately equal (default tolerance).
    /// </summary>
    public static void ShouldBeApproximately(this Vector4 actual, Vector4 expected)
    {
        ShouldBeApproximately(actual, expected, VecTolerance);
    }

    /// <summary>
    /// Asserts that two Quaternion values are approximately equal (default tolerance).
    /// </summary>
    public static void ShouldBeApproximately(this Quaternion actual, Quaternion expected)
    {
        ShouldBeApproximately(actual, expected, VecTolerance);
    }
}
