using System;

namespace FrameForge.Foundation.FixedPoint
{

// ============================================================================
// Trigonometry: Sin, Cos, Tan, Asin, Acos, Atan, Atan2
// Uses a 1024-entry lookup table covering [0, PI/2] with linear interpolation
// and symmetry for full-range coverage.
// ============================================================================

public readonly partial struct FP
{
    // ========================================================================
    // Lookup Table
    // ========================================================================

    private const int TRIG_TABLE_SIZE = 1024;

    /// <summary>
    /// Sin lookup table: sin(i * PI/2 / (TABLE_SIZE - 1)) for i in [0, TABLE_SIZE-1].
    /// Stored as raw Q32.32 values for deterministic lookup.
    /// </summary>
    private static readonly long[] SinTable = new long[TRIG_TABLE_SIZE];

    /// <summary>
    /// Step between consecutive table entries = PI/2 / (TABLE_SIZE - 1), as raw value.
    /// </summary>
    private static long _trigStepRaw;

    /// <summary>
    /// (TABLE_SIZE - 1) / (PI/2) as raw value for fast index computation.
    /// </summary>
    private static long _trigInvStepRaw;

    /// <summary>
    /// Generates the 1024-entry sin lookup table in the static constructor.
    /// Table covers [0, PI/2] uniformly.
    /// Pure fixed-point generation — zero dependency on System.Math / System.MathF.
    ///
    /// Algorithm:
    ///   1. Compute sin(step) for the tiny angular step (~0.0015 rad) via
    ///      truncated Taylor series (3 terms suffice for Q32.32 precision).
    ///   2. Compute cos(step) = sqrt(1 - sin²(step)) via FP.Sqrt.
    ///   3. Fill the table iteratively using the angle-addition formulas:
    ///        sin(θ+δ) = sin(θ)·cos(δ) + cos(θ)·sin(δ)
    ///        cos(θ+δ) = cos(θ)·cos(δ) - sin(θ)·sin(δ)
    ///      This runs 1024 iterations; the accumulated error per step is
    ///      O(ε) so total drift is &lt; 1 raw unit, well below spec (&lt; 1e-6).
    /// </summary>
    private static void GenerateTrigTable()
    {
        FP piOver2 = PI / Two;
        FP step = piOver2 / FromInt(TRIG_TABLE_SIZE - 1);
        _trigStepRaw = step.RawValue;

        // Inverse step for fast index computation: (1 << 32) / _trigStepRaw
        _trigInvStepRaw = (long)(((Int128)1 << 32) / (ulong)_trigStepRaw);

        // --- Compute sin(step) via Taylor series ----------------------------
        // step ≈ 0.001534 rad → step³/6 ≈ 6e-10 (barely above Epsilon)
        // Three terms give error < 1e-15, far below Q32.32 resolution.
        FP stepSq = step * step;
        FP stepCu = stepSq * step;              // step³
        FP stepQuintic = stepCu * stepSq;       // step⁵

        // sin(x) ≈ x - x³/6 + x⁵/120
        FP sinStep = step - stepCu / FromInt(6) + stepQuintic / FromInt(120);

        // cos(step) = sqrt(1 - sin²(step))
        FP cosStep = Sqrt(One - sinStep * sinStep);

        // --- Iterative angle-addition fill ----------------------------------
        FP sinCur = Zero;
        FP cosCur = One;

        for (int i = 0; i < TRIG_TABLE_SIZE; i++)
        {
            SinTable[i] = sinCur.RawValue;

            // sin(θ+δ) = sin(θ)·cos(δ) + cos(θ)·sin(δ)
            // cos(θ+δ) = cos(θ)·cos(δ) - sin(θ)·sin(δ)
            FP sinNext = sinCur * cosStep + cosCur * sinStep;
            FP cosNext = cosCur * cosStep - sinCur * sinStep;
            sinCur = sinNext;
            cosCur = cosNext;
        }

        // Clamp the final entry to exactly 1.0 to avoid accumulated drift
        SinTable[TRIG_TABLE_SIZE - 1] = One.RawValue;
    }

    // ========================================================================
    // Private: Normalize angle to [0, 2*PI)
    // ========================================================================

    /// <summary>
    /// Normalizes a raw angle value to [0, 2*PI) using modulo arithmetic.
    /// </summary>
    private static long NormalizeAngleRaw(long rawAngle, long twoPiRaw)
    {
        if (rawAngle >= 0)
            return rawAngle % twoPiRaw;

        // For negative angles: add 2*PI until positive
        long mod = rawAngle % twoPiRaw;
        if (mod < 0)
            mod += twoPiRaw;
        return mod;
    }

    // ========================================================================
    // Private: Sin in [0, PI/2] using lookup table with linear interpolation
    // ========================================================================

    /// <summary>
    /// Computes sin(angle) for an angle in [0, PI/2] using the lookup table.
    /// <paramref name="angleRaw"/> must be in [0, pi/2_raw].
    /// </summary>
    private static long SinQuadrant0(long angleRaw, long piOver2Raw)
    {
        if (angleRaw <= 0)
            return 0;
        if (angleRaw >= piOver2Raw)
            return SinTable[TRIG_TABLE_SIZE - 1];

        // Compute index: angle / step
        // index_frac = angleRaw * _trigInvStepRaw >> 32
        long indexFrac = (long)(((Int128)angleRaw * _trigInvStepRaw) >> 32);
        int index = (int)indexFrac;
        long t = angleRaw - index * _trigStepRaw; // Fractional part (in raw units)

        if (index >= TRIG_TABLE_SIZE - 1)
            return SinTable[TRIG_TABLE_SIZE - 1];

        long lo = SinTable[index];
        long hi = SinTable[index + 1];

        // Linear interpolation: lo + (hi - lo) * t / step
        long diff = hi - lo;
        long interpolated = (long)(((Int128)diff * t) / _trigStepRaw);
        return lo + interpolated;
    }

    // ========================================================================
    // Sin
    // ========================================================================

    /// <summary>
    /// Returns the sine of <paramref name="radians"/>.
    /// Uses a 1024-entry lookup table with linear interpolation.
    /// Precision: &lt; 1 × 10⁻⁶.
    /// </summary>
    public static FP Sin(FP radians)
    {
        long piRaw = PI.RawValue;
        long twoPiRaw = piRaw * 2;
        long piOver2Raw = piRaw / 2;
        long threePiOver2Raw = piRaw + piOver2Raw;

        // Normalize to [0, 2*PI)
        long angle = NormalizeAngleRaw(radians.RawValue, twoPiRaw);

        // Determine quadrant and compute
        if (angle < piOver2Raw)
        {
            // Quadrant I: [0, PI/2)
            return new FP(SinQuadrant0(angle, piOver2Raw));
        }
        else if (angle < piRaw)
        {
            // Quadrant II: [PI/2, PI): sin(x) = sin(PI - x)
            long mirrored = piRaw - angle;
            return new FP(SinQuadrant0(mirrored, piOver2Raw));
        }
        else if (angle < threePiOver2Raw)
        {
            // Quadrant III: [PI, 3*PI/2): sin(x) = -sin(x - PI)
            long adjusted = angle - piRaw;
            return new FP(-SinQuadrant0(adjusted, piOver2Raw));
        }
        else
        {
            // Quadrant IV: [3*PI/2, 2*PI): sin(x) = -sin(2*PI - x)
            long mirrored = twoPiRaw - angle;
            return new FP(-SinQuadrant0(mirrored, piOver2Raw));
        }
    }

    // ========================================================================
    // Cos
    // ========================================================================

    /// <summary>
    /// Returns the cosine of <paramref name="radians"/>.
    /// Computed as Sin(radians + PI/2).
    /// </summary>
    public static FP Cos(FP radians)
    {
        return Sin(radians + PI / Two);
    }

    // ========================================================================
    // Tan
    // ========================================================================

    /// <summary>
    /// Returns the tangent of <paramref name="radians"/>.
    /// Computed as Sin(radians) / Cos(radians).
    /// Throws for angles where cos ≈ 0 (odd multiples of PI/2).
    /// </summary>
    public static FP Tan(FP radians)
    {
        FP cos = Cos(radians);
        // Check if cos is near zero (within 10 raw units)
        if (Abs(cos).RawValue < 10)
            throw new ArgumentOutOfRangeException(nameof(radians),
                "Tangent is undefined (cosine near zero).");
        return Sin(radians) / cos;
    }

    // ========================================================================
    // Asin (arcsine via Atan)
    // ========================================================================

    /// <summary>
    /// Returns the arcsine of <paramref name="value"/> in radians.
    /// Uses the identity: asin(x) = atan(x / sqrt(1 - x²)).
    /// Domain: [-1, 1]. Range: [-PI/2, PI/2].
    /// </summary>
    public static FP Asin(FP value)
    {
        if (value.RawValue > One.RawValue || value.RawValue < MinusOne.RawValue)
            throw new ArgumentOutOfRangeException(nameof(value),
                "Asin argument must be in [-1, 1].");

        if (value == One)
            return PI / Two;
        if (value == MinusOne)
            return -(PI / Two);
        if (Abs(value).RawValue < 10) // Near zero
            return value; // Small angle approximation

        // asin(x) = atan(x / sqrt(1 - x²))
        FP denominator = Sqrt(One - value * value);
        return Atan(value / denominator);
    }

    // ========================================================================
    // Acos (arccosine via Asin)
    // ========================================================================

    /// <summary>
    /// Returns the arccosine of <paramref name="value"/> in radians.
    /// Uses the identity: acos(x) = PI/2 - asin(x).
    /// Domain: [-1, 1]. Range: [0, PI].
    /// </summary>
    public static FP Acos(FP value)
    {
        if (value.RawValue > One.RawValue || value.RawValue < MinusOne.RawValue)
            throw new ArgumentOutOfRangeException(nameof(value),
                "Acos argument must be in [-1, 1].");

        return PI / Two - Asin(value);
    }

    // ========================================================================
    // Atan (arctangent via Taylor Series)
    // ========================================================================

    /// <summary>
    /// Returns the arctangent of <paramref name="value"/> in radians.
    /// Uses Taylor series with range reduction for fast convergence:
    /// <list type="bullet">
    /// <item>|x| &gt; 1: atan(x) = PI/2 - atan(1/x)</item>
    /// <item>|x| &gt; 0.5: atan(x) = PI/4 - atan((1-x)/(1+x))</item>
    /// </list>
    /// Range: [-PI/2, PI/2].
    /// </summary>
    public static FP Atan(FP value)
    {
        // Handle special cases
        if (value.RawValue == 0)
            return Zero;

        bool negative = value.RawValue < 0;
        FP x = negative ? -value : value;

        // Range reduction to ensure |x| <= 0.5 for fast convergence
        FP adjustment = Zero;

        if (x > One)
        {
            // atan(x) = PI/2 - atan(1/x)
            x = One / x;
            adjustment = PI / Two;
        }

        if (x > Half)
        {
            // atan(x) = PI/4 - atan((1-x)/(1+x))
            // (1-x)/(1+x) is in [0, 1/3] when x in [0.5, 1]
            x = (One - x) / (One + x);
            adjustment = PI / FromInt(4);
        }

        // Taylor series: atan(x) = x - x³/3 + x⁵/5 - x⁷/7 + ...
        FP x2 = x * x;
        FP result = x;
        FP term = x;

        // For x <= 0.5, 15 odd terms give < 1e-7 precision
        for (int i = 3; i <= 29; i += 2)
        {
            term = term * x2; // term = x^i
            FP nextTerm = term / FromInt(i); // x^i / i

            if ((i / 2) % 2 == 1) // odd-indexed terms: subtract
                result -= nextTerm;
            else
                result += nextTerm;

            if (Abs(nextTerm) < Epsilon)
                break;
        }

        // Apply range reduction correction
        if (adjustment.RawValue != 0)
            result = adjustment - result;

        return negative ? -result : result;
    }

    // ========================================================================
    // Atan2
    // ========================================================================

    /// <summary>
    /// Returns the arctangent of <paramref name="y"/>/<paramref name="x"/> in radians
    /// using the signs of both arguments to determine the quadrant.
    /// Range: [-PI, PI].
    /// </summary>
    public static FP Atan2(FP y, FP x)
    {
        if (x.RawValue == 0)
        {
            if (y.RawValue > 0) return PI / Two;
            if (y.RawValue < 0) return -(PI / Two);
            return Zero; // atan2(0, 0) = 0 by convention
        }

        FP atan = Atan(y / x);

        if (x.RawValue > 0)
        {
            return atan;
        }
        else // x < 0
        {
            if (y.RawValue >= 0)
                return atan + PI;
            else
                return atan - PI;
        }
    }

    // ========================================================================
    // Degree Versions
    // ========================================================================

    /// <summary>
    /// Returns the sine of <paramref name="degrees"/> (input in degrees).
    /// </summary>
    public static FP SinDeg(FP degrees)
    {
        return Sin(degrees * Deg2Rad);
    }

    /// <summary>
    /// Returns the cosine of <paramref name="degrees"/> (input in degrees).
    /// </summary>
    public static FP CosDeg(FP degrees)
    {
        return Cos(degrees * Deg2Rad);
    }

    /// <summary>
    /// Returns the tangent of <paramref name="degrees"/> (input in degrees).
    /// </summary>
    public static FP TanDeg(FP degrees)
    {
        return Tan(degrees * Deg2Rad);
    }
}
}
