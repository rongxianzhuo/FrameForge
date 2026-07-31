using FrameForge.Foundation.FixedPoint;

namespace FrameForge.Foundation.Math
{
/// <summary>
/// Provides common mathematical functions operating on <see cref="FP"/> values.
/// This is the Unity <c>Mathf</c>-equivalent for the FrameForge framework.
/// Most methods delegate to <see cref="FP"/> static methods; additional utility
/// functions not present on <see cref="FP"/> itself are implemented here.
/// </summary>
public static class FPMath
{
    // ========================================================================
    // Constants
    // ========================================================================

    /// <summary>π ≈ 3.14159265358979...</summary>
    public static readonly FP PI = FP.PI;

    /// <summary>Degrees-to-radians conversion factor (π / 180).</summary>
    public static readonly FP Deg2Rad = FP.Deg2Rad;

    /// <summary>Radians-to-degrees conversion factor (180 / π).</summary>
    public static readonly FP Rad2Deg = FP.Rad2Deg;

    /// <summary>
    /// The smallest representable positive value for <see cref="FP"/> (1 raw unit).
    /// Represents approximately 2.33 × 10⁻¹⁰.
    /// </summary>
    public static readonly FP Epsilon = FP.Epsilon;

    // ========================================================================
    // Basic Math
    // ========================================================================

    /// <inheritdoc cref="FP.Abs"/>
    public static FP Abs(FP value) => FP.Abs(value);

    /// <summary>Returns the absolute value of an integer.</summary>
    public static int Abs(int value) => value >= 0 ? value : -value;

    /// <inheritdoc cref="FP.Sign"/>
    public static FP Sign(FP value) => FP.Sign(value);

    /// <summary>Returns the sign of an integer: 1, -1, or 0.</summary>
    public static int Sign(int value) => value > 0 ? 1 : (value < 0 ? -1 : 0);

    /// <inheritdoc cref="FP.Min(FP, FP)"/>
    public static FP Min(FP a, FP b) => FP.Min(a, b);

    /// <summary>Returns the smaller of two integers.</summary>
    public static int Min(int a, int b) => a <= b ? a : b;

    /// <inheritdoc cref="FP.Max(FP, FP)"/>
    public static FP Max(FP a, FP b) => FP.Max(a, b);

    /// <summary>Returns the larger of two integers.</summary>
    public static int Max(int a, int b) => a >= b ? a : b;

    // ========================================================================
    // Powers & Roots
    // ========================================================================

    /// <inheritdoc cref="FP.Pow2"/>
    public static FP Pow(FP value, FP exponent)
    {
        // For base-2, use Pow2. For general case, use Exp/Log.
        if (value == FP.Two)
            return FP.Pow2(exponent);
        if (value == FP.Zero)
        {
            if (exponent == FP.Zero)
                return FP.One;
            if (exponent > FP.Zero)
                return FP.Zero;
            // Negative exponent → undefined (division by zero)
            throw new ArgumentOutOfRangeException(nameof(exponent),
                "Cannot raise zero to a negative power.");
        }
        // a^b = exp(b * log(a))
        return FP.Exp(exponent * FP.Log(value));
    }

    /// <inheritdoc cref="FP.Sqrt"/>
    public static FP Sqrt(FP value) => FP.Sqrt(value);

    // ========================================================================
    // Interpolation
    // ========================================================================

    /// <inheritdoc cref="FP.Lerp"/>
    public static FP Lerp(FP a, FP b, FP t) => FP.Lerp(a, b, t);

    /// <inheritdoc cref="FP.LerpUnclamped"/>
    public static FP LerpUnclamped(FP a, FP b, FP t) => FP.LerpUnclamped(a, b, t);

    /// <summary>
    /// Calculates the interpolation parameter t such that
    /// Lerp(a, b, t) = value. Inverse of <see cref="Lerp"/>.
    /// Result is clamped to [0, 1].
    /// </summary>
    public static FP InverseLerp(FP a, FP b, FP value)
    {
        if (a == b)
            return FP.Zero;
        return FP.Clamp01((value - a) / (b - a));
    }

    /// <summary>
    /// Performs smooth Hermite interpolation between 0 and 1
    /// using the formula t²(3 - 2t). Result is clamped to [0, 1].
    /// </summary>
    public static FP SmoothStep(FP a, FP b, FP t)
    {
        t = FP.Clamp01(t);
        // Hermite blend: t² * (3 - 2t)
        FP smoothT = t * t * (FP.FromInt(3) - FP.Two * t);
        return LerpUnclamped(a, b, smoothT);
    }

    /// <summary>
    /// Gradually changes a value towards a target over time with smoothing.
    /// Behaviour matches Unity's <c>Mathf.SmoothDamp</c> — uses a critically
    /// damped spring model (based on Game Programming Gems 4, Chapter 1.10).
    /// </summary>
    /// <param name="current">The current value.</param>
    /// <param name="target">The target value.</param>
    /// <param name="currentVelocity">Reference to the current velocity; updated each call.</param>
    /// <param name="smoothTime">Approximately the time to reach the target.</param>
    /// <param name="maxSpeed">Optional maximum speed cap.</param>
    /// <param name="deltaTime">Time since the last call.</param>
    /// <returns>The new smoothed value.</returns>
    public static FP SmoothDamp(FP current, FP target, ref FP currentVelocity, FP smoothTime, FP maxSpeed, FP deltaTime)
    {
        // Clamp minimum smooth time to avoid division by zero
        FP minSmoothTime = FP.Epsilon * FP.FromInt(10000);
        if (smoothTime < minSmoothTime)
            smoothTime = minSmoothTime;

        // Omega = 2 / smoothTime for critically damped spring
        FP omega = FP.Two / smoothTime;
        FP x = omega * deltaTime;

        // exp = 1 / (1 + x + 0.48*x² + 0.235*x³)
        // This is a Padé approximant of exp(-x), more stable for FP
        FP x2 = x * x;
        FP x3 = x2 * x;
        FP exp = FP.One / (FP.One + x + FP.FromDouble(0.48) * x2 + FP.FromDouble(0.235) * x3);

        FP change = current - target;
        FP originalTo = target;

        // Clamp max change
        FP maxChange = maxSpeed * smoothTime;
        if (maxChange > FP.Zero)
        {
            FP absChange = FP.Abs(change);
            if (absChange > maxChange)
            {
                change = FP.Sign(change) * maxChange;
            }
        }

        target = current - change;
        FP temp = (currentVelocity + omega * change) * deltaTime;
        currentVelocity = (currentVelocity - omega * temp) * exp;
        FP output = target + (change + temp) * exp;

        // Prevent overshooting
        if ((originalTo > current) == (output > originalTo))
        {
            output = originalTo;
            if (deltaTime != FP.Zero)
                currentVelocity = (output - originalTo) / deltaTime;
        }

        return output;
    }

    /// <summary>
    /// Gradually changes a value towards a target over time. Uses default maxSpeed (infinite).
    /// </summary>
    public static FP SmoothDamp(FP current, FP target, ref FP currentVelocity, FP smoothTime, FP deltaTime)
    {
        return SmoothDamp(current, target, ref currentVelocity, smoothTime, FP.MaxValue, deltaTime);
    }

    // ========================================================================
    // Movement
    // ========================================================================

    /// <summary>
    /// Moves <paramref name="current"/> towards <paramref name="target"/> by at most
    /// <paramref name="maxDelta"/>. Won't overshoot.
    /// Equivalent to <see cref="FP.Clamp"/> with step but gives correct sign behavior.
    /// </summary>
    public static FP MoveTowards(FP current, FP target, FP maxDelta)
    {
        FP diff = target - current;
        FP absDiff = FP.Abs(diff);

        if (absDiff <= maxDelta || absDiff == FP.Zero)
            return target;

        return current + FP.Sign(diff) * maxDelta;
    }

    /// <summary>
    /// Moves an angle (in degrees) towards a target angle, taking the shortest
    /// path around the circle.
    /// </summary>
    public static FP MoveTowardsAngle(FP current, FP target, FP maxDelta)
    {
        FP deltaAngle = DeltaAngle(current, target);
        if (FP.Abs(deltaAngle) <= maxDelta)
            return target;
        target = current + deltaAngle;
        return MoveTowards(current, target, maxDelta);
    }

    // ========================================================================
    // Range Operations
    // ========================================================================

    /// <inheritdoc cref="FP.Clamp"/>
    public static FP Clamp(FP value, FP min, FP max) => FP.Clamp(value, min, max);

    /// <summary>Clamps an integer between min and max.</summary>
    public static int Clamp(int value, int min, int max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    /// <inheritdoc cref="FP.Clamp01"/>
    public static FP Clamp01(FP value) => FP.Clamp01(value);

    /// <summary>
    /// Loops <paramref name="value"/> so that it stays within [0, <paramref name="length"/>).
    /// </summary>
    public static FP Repeat(FP value, FP length)
    {
        if (length == FP.Zero)
            return FP.Zero;
        return FP.Clamp(value - FP.Floor(value / length) * length, FP.Zero, length);
    }

    /// <summary>
    /// Ping-pongs <paramref name="value"/> so that it bounces between
    /// 0 and <paramref name="length"/>.
    /// </summary>
    public static FP PingPong(FP value, FP length)
    {
        if (length == FP.Zero)
            return FP.Zero;
        FP t = Repeat(value, length * FP.Two);
        return length - FP.Abs(t - length);
    }

    // ========================================================================
    // Trigonometry (delegates to FP)
    // ========================================================================

    /// <inheritdoc cref="FP.Sin"/>
    public static FP Sin(FP radians) => FP.Sin(radians);

    /// <inheritdoc cref="FP.Cos"/>
    public static FP Cos(FP radians) => FP.Cos(radians);

    /// <inheritdoc cref="FP.Tan"/>
    public static FP Tan(FP radians) => FP.Tan(radians);

    /// <inheritdoc cref="FP.Asin"/>
    public static FP Asin(FP value) => FP.Asin(value);

    /// <inheritdoc cref="FP.Acos"/>
    public static FP Acos(FP value) => FP.Acos(value);

    /// <inheritdoc cref="FP.Atan"/>
    public static FP Atan(FP value) => FP.Atan(value);

    /// <inheritdoc cref="FP.Atan2"/>
    public static FP Atan2(FP y, FP x) => FP.Atan2(y, x);

    // ========================================================================
    // Angle Utilities
    // ========================================================================

    /// <summary>
    /// Calculates the shortest signed difference between two angles in degrees.
    /// Result is in [-180, 180].
    /// </summary>
    public static FP DeltaAngle(FP current, FP target)
    {
        FP delta = Repeat(target - current, FP.FromInt(360));
        if (delta > FP.FromInt(180))
            delta -= FP.FromInt(360);
        return delta;
    }

    // ========================================================================
    // Rounding
    // ========================================================================

    /// <inheritdoc cref="FP.Floor"/>
    public static FP Floor(FP value) => FP.Floor(value);

    /// <summary>Returns the largest integer ≤ value, as an int.</summary>
    public static int FloorToInt(FP value) => value.FloorToInt();

    /// <inheritdoc cref="FP.Ceil"/>
    public static FP Ceil(FP value) => FP.Ceil(value);

    /// <summary>Returns the smallest integer ≥ value, as an int.</summary>
    public static int CeilToInt(FP value) => value.CeilToInt();

    /// <inheritdoc cref="FP.Round"/>
    public static FP Round(FP value) => FP.Round(value);

    /// <summary>Rounds to the nearest integer (midpoint away from zero), as an int.</summary>
    public static int RoundToInt(FP value) => value.RoundToInt();

    // ========================================================================
    // Comparison
    // ========================================================================

    /// <inheritdoc cref="FP.Approximately(FP, FP)"/>
    public static bool Approximately(FP a, FP b) => FP.Approximately(a, b);

    /// <summary>
    /// Returns true if the difference between <paramref name="a"/> and
    /// <paramref name="b"/> is less than <paramref name="tolerance"/>.
    /// </summary>
    public static bool Approximately(FP a, FP b, FP tolerance) => FP.Approximately(a, b, tolerance);
}
}
