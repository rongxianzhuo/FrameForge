using System.Numerics;
using FrameForge.Foundation.FixedPoint;

namespace FrameForge.Foundation.Math;

/// <summary>
/// Extended math utility functions that complement <see cref="FPMath"/>.
/// </summary>
public static class MathEx
{
    // ========================================================================
    // Value Mapping
    // ========================================================================

    /// <summary>
    /// Maps <paramref name="value"/> from the range [<paramref name="fromMin"/>,
    /// <paramref name="fromMax"/>] to [<paramref name="toMin"/>,
    /// <paramref name="toMax"/>].
    /// </summary>
    public static FP Map(FP value, FP fromMin, FP fromMax, FP toMin, FP toMax)
    {
        if (fromMin == fromMax)
            return toMin;
        FP t = (value - fromMin) / (fromMax - fromMin);
        return FP.LerpUnclamped(toMin, toMax, t);
    }

    // ========================================================================
    // Power of Two Utilities
    // ========================================================================

    /// <summary>
    /// Returns true if <paramref name="value"/> is a power of two.
    /// </summary>
    public static bool IsPowerOfTwo(int value)
    {
        return value > 0 && (value & (value - 1)) == 0;
    }

    /// <summary>
    /// Returns the smallest power of two ≥ <paramref name="value"/>.
    /// </summary>
    public static int NextPowerOfTwo(int value)
    {
        if (value <= 0)
            return 1;
        if (IsPowerOfTwo(value))
            return value;

        // Use BitOperations for efficient computation
        return (int)(0x80000000UL >> (BitOperations.LeadingZeroCount((uint)(value - 1)) - 1));
    }
}
