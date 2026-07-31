using FrameForge.Foundation.FixedPoint;
using FrameForge.Foundation.Math;
using Xunit;

namespace FrameForge.Core.Tests {

/// <summary>
/// Test helper extensions for Core module assertions.
/// </summary>
public static class TestExtensions
{
    private static readonly FP DefaultFPTolerance = FP.Epsilon * FP.FromInt(500);
    private static readonly FP VecTolerance = FP.Epsilon * FP.FromInt(10000);

    public static void ShouldBeApproximately(this FP actual, FP expected, FP? tolerance = null)
    {
        FP tol = tolerance ?? DefaultFPTolerance;
        Assert.True(FP.Approximately(actual, expected, tol),
            $"Expected ~{expected}, got {actual} (diff: {FP.Abs(actual - expected)})");
    }

    public static void ShouldBeApproximately(this Vector3 actual, Vector3 expected, FP? tolerance = null)
    {
        FP tol = tolerance ?? VecTolerance;
        Assert.True(
            FP.Approximately(actual.X, expected.X, tol) &&
            FP.Approximately(actual.Y, expected.Y, tol) &&
            FP.Approximately(actual.Z, expected.Z, tol),
            $"Expected ~{expected}, got {actual}");
    }

    public static void ShouldBeApproximately(this Quaternion actual, Quaternion expected, FP? tolerance = null)
    {
        FP tol = tolerance ?? VecTolerance;
        bool direct =
            FP.Approximately(actual.X, expected.X, tol) &&
            FP.Approximately(actual.Y, expected.Y, tol) &&
            FP.Approximately(actual.Z, expected.Z, tol) &&
            FP.Approximately(actual.W, expected.W, tol);
        bool negated =
            FP.Approximately(actual.X, -expected.X, tol) &&
            FP.Approximately(actual.Y, -expected.Y, tol) &&
            FP.Approximately(actual.Z, -expected.Z, tol) &&
            FP.Approximately(actual.W, -expected.W, tol);

        Assert.True(direct || negated,
            $"Expected ~{expected}, got {actual}");
    }
}
}
