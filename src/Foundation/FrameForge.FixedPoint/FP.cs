using System;
using System.Globalization;

namespace FrameForge.Foundation.FixedPoint;

/// <summary>
/// Represents a Q32.32 fixed-point number with 32 bits for the integer part
/// and 32 bits for the fractional part. All framework math is based on this
/// type to guarantee cross-platform determinism for lockstep networking and
/// reproducible AI.
/// </summary>
/// <remarks>
/// Internal storage is a single <see cref="long"/> (64-bit signed integer).
/// <para>Precision: ~2.3 × 10⁻¹⁰ per raw unit.</para>
/// <para>Range: ±2,147,483,647.9999999998</para>
/// </remarks>
public readonly partial struct FP : IEquatable<FP>, IComparable<FP>
{
    // ========================================================================
    // Internal Storage
    // ========================================================================

    /// <summary>
    /// Raw underlying value in Q32.32 format.
    /// High 32 bits = integer part, low 32 bits = fractional part.
    /// 1 raw unit = 1 / 2^32 ≈ 2.32830644 × 10⁻¹⁰
    /// </summary>
    internal readonly long RawValue;

    // ========================================================================
    // Private Raw Constants
    // ========================================================================

    private const long RAW_ONE = 1L << 32;        // 4294967296
    private const long RAW_HALF = 1L << 31;       // 2147483648
    private const long RAW_NEG_ONE = -(1L << 32); // -4294967296
    private const long FRAC_MASK = 0xFFFFFFFFL;
    private const long RAW_TEN = 10L << 32;       // 42949672960
    private const long RAW_HUNDRED_EIGHTY = 180L << 32;
    private const long RAW_ONE_MILLION = 1_000_000L;

    // ========================================================================
    // Static Constructor
    // ========================================================================

    // ========================================================================
    // Hardcoded Mathematical Constants (Q32.32 raw values)
    // Computed offline with high precision; stored as raw longs to achieve
    // zero runtime dependency on System.Math / System.MathF.
    // ========================================================================

    // π × 2^32 = 3.14159265358979323846... × 4294967296 ≈ 13493037704.522
    private const long RAW_PI = 13493037705L;
    // e × 2^32 = 2.71828182845904523536... × 4294967296 ≈ 11674931554.478
    private const long RAW_E = 11674931555L;
    // ln(2) × 2^32 = 0.69314718055994530942... × 4294967296 ≈ 2977044472.260
    private const long RAW_LN2 = 2977044472L;
    // ln(10) × 2^32 = 2.30258509299404568402... × 4294967296 ≈ 9889527670.667
    private const long RAW_LN10 = 9889527671L;

    static FP()
    {
        Zero = new FP(0);
        One = new FP(RAW_ONE);
        Half = new FP(RAW_HALF);
        MinusOne = new FP(RAW_NEG_ONE);
        Two = new FP(RAW_ONE * 2);
        Ten = new FP(RAW_TEN);
        PointOne = new FP(RAW_ONE / 10);

        Epsilon = new FP(1);
        MaxValue = new FP(long.MaxValue);
        MinValue = new FP(1); // Smallest representable positive value

        // Mathematical constants — hardcoded raw values (zero double dependency)
        PI = new FP(RAW_PI);
        E = new FP(RAW_E);
        Ln2 = new FP(RAW_LN2);
        Ln10 = new FP(RAW_LN10);

        Deg2Rad = PI / new FP(RAW_HUNDRED_EIGHTY);
        Rad2Deg = new FP(RAW_HUNDRED_EIGHTY) / PI;

        GenerateTrigTable();
    }

    // ========================================================================
    // Public Constants
    // ========================================================================

    /// <summary>Fixed-point representation of 0.</summary>
    public static readonly FP Zero;

    /// <summary>Fixed-point representation of 1.</summary>
    public static readonly FP One;

    /// <summary>Fixed-point representation of 0.5.</summary>
    public static readonly FP Half;

    /// <summary>Fixed-point representation of -1.</summary>
    public static readonly FP MinusOne;

    /// <summary>Fixed-point representation of 2.</summary>
    public static readonly FP Two;

    /// <summary>Fixed-point representation of 10.</summary>
    public static readonly FP Ten;

    /// <summary>Fixed-point representation of 0.1.</summary>
    public static readonly FP PointOne;

    /// <summary>Maximum representable positive value.</summary>
    public static readonly FP MaxValue;

    /// <summary>Smallest representable positive value (> 0).</summary>
    public static readonly FP MinValue;

    /// <summary>Smallest representable positive magnitude (1 raw unit).</summary>
    public static readonly FP Epsilon;

    /// <summary>π ≈ 3.14159265358979...</summary>
    public static readonly FP PI;

    /// <summary>e ≈ 2.71828182845905...</summary>
    public static readonly FP E;

    /// <summary>π / 180, for converting degrees to radians.</summary>
    public static readonly FP Deg2Rad;

    /// <summary>180 / π, for converting radians to degrees.</summary>
    public static readonly FP Rad2Deg;

    /// <summary>ln(2), used internally by Exp/Log/Pow2.</summary>
    internal static readonly FP Ln2;

    /// <summary>ln(10), used internally by Log10.</summary>
    internal static readonly FP Ln10;

    // ========================================================================
    // Constructors & Factory Methods
    // ========================================================================

    /// <summary>
    /// Constructs an <see cref="FP"/> directly from a raw Q32.32 value.
    /// Typically for internal use; prefer <see cref="FromRaw"/> for explicitness.
    /// </summary>
    public FP(long rawValue)
    {
        RawValue = rawValue;
    }

    /// <summary>
    /// Creates an <see cref="FP"/> from a raw Q32.32 value.
    /// </summary>
    public static FP FromRaw(long rawValue) => new(rawValue);

    /// <summary>
    /// Creates an <see cref="FP"/> from an <see cref="int"/>.
    /// </summary>
    public static FP FromInt(int value) => new((long)value << 32);

    /// <summary>
    /// Creates an <see cref="FP"/> from a <see cref="long"/>.
    /// The value must fit within the Q32.32 integer range (±2,147,483,647).
    /// </summary>
    public static FP FromLong(long value)
    {
        if (value > int.MaxValue || value < int.MinValue)
            throw new ArgumentOutOfRangeException(nameof(value),
                "Value exceeds Q32.32 integer range. Use a smaller value.");
        return new FP(value << 32);
    }

    /// <summary>
    /// ⚠️ Creates an <see cref="FP"/> from a <see cref="float"/>.
    /// Editor / tool use only! Game logic MUST NOT use this — it breaks determinism.
    /// </summary>
    public static FP FromFloat(float value)
    {
        double rounded = Math.Round((double)value * (double)RAW_ONE);
        return new FP((long)rounded);
    }

    /// <summary>
    /// ⚠️ Creates an <see cref="FP"/> from a <see cref="double"/>.
    /// Editor / tool / config import only! Game logic MUST NOT use this — it breaks determinism.
    /// </summary>
    public static FP FromDouble(double value)
    {
        double rounded = Math.Round(value * (double)RAW_ONE);
        return new FP((long)rounded);
    }

    // ========================================================================
    // String Parsing
    // ========================================================================

    /// <summary>
    /// Parses a string representation of a fixed-point number (e.g. "123.456789").
    /// Culture-invariant; uses '.' as decimal separator.
    /// </summary>
    public static FP Parse(string s)
    {
        if (!TryParse(s, out FP result))
            throw new FormatException($"Cannot parse '{s}' as a fixed-point number.");
        return result;
    }

    /// <summary>
    /// Attempts to parse a string representation of a fixed-point number.
    /// Culture-invariant; uses '.' as decimal separator.
    /// </summary>
    public static bool TryParse(string s, out FP result)
    {
        result = Zero;
        if (string.IsNullOrWhiteSpace(s))
            return false;

        s = s.Trim();

        bool negative = false;
        int start = 0;

        if (s[0] == '-')
        {
            negative = true;
            start = 1;
        }
        else if (s[0] == '+')
        {
            start = 1;
        }

        // Find decimal point
        int dotIndex = s.IndexOf('.', start);

        // Parse integer part
        long intPart = 0;
        int intEnd = dotIndex >= 0 ? dotIndex : s.Length;

        if (intEnd == start)
            return false; // No digits at all

        for (int i = start; i < intEnd; i++)
        {
            char c = s[i];
            if (c < '0' || c > '9')
                return false;
            intPart = intPart * 10 + (c - '0');
            if (intPart > int.MaxValue + 1L) // Allow up to 2147483648 for -MinValue case
                return false;
        }

        // Parse fractional part (up to 10 digits for full Q32.32 precision)
        long fracRaw = 0;
        if (dotIndex >= 0)
        {
            int fracStart = dotIndex + 1;
            int fracDigits = s.Length - fracStart;
            if (fracDigits == 0)
                return false; // Trailing dot with no digits

            // We compute: fracRaw = frac * 2^32 / 10^fracDigits
            // To avoid overflow: frac first, then shift
            for (int i = fracStart; i < s.Length; i++)
            {
                char c = s[i];
                if (c < '0' || c > '9')
                    return false;
                // Clamp to 10 significant decimal digits (more than enough for Q32.32)
                if (i - fracStart >= 10)
                    continue;
                fracRaw = fracRaw * 10 + (c - '0');
            }

            // Pad to the actual number of decimal digits (up to 10)
            int actualDigits = Math.Min(fracDigits, 10);
            // Scale: fracRaw * 2^32 / 10^actualDigits
            long denominator = 1;
            for (int i = 0; i < actualDigits; i++)
                denominator *= 10;

            // fracRaw / denominator * 2^32, but we need to preserve precision
            // Compute: (fracRaw << 32) / denominator
            fracRaw = (long)(((Int128)fracRaw << 32) / denominator);
        }

        long raw = (intPart << 32) | (fracRaw & FRAC_MASK);
        if (negative)
            raw = -raw;

        result = new FP(raw);
        return true;
    }

    // ========================================================================
    // Conversion Methods
    // ========================================================================

    /// <summary>
    /// Converts to <see cref="int"/> by truncating toward zero.
    /// </summary>
    public int ToInt()
    {
        if (RawValue >= 0)
            return (int)(RawValue >> 32);
        // For negative values with fractional part, round toward zero
        if ((RawValue & FRAC_MASK) == 0)
            return (int)(RawValue >> 32);
        return (int)(RawValue >> 32) + 1;
    }

    /// <summary>
    /// Returns the largest integer ≤ this value (floor).
    /// </summary>
    public int FloorToInt()
    {
        return (int)(RawValue >> 32); // Arithmetic shift floors for negative too
    }

    /// <summary>
    /// Returns the smallest integer ≥ this value (ceil).
    /// </summary>
    public int CeilToInt()
    {
        if ((RawValue & FRAC_MASK) == 0)
            return (int)(RawValue >> 32);
        return (int)(RawValue >> 32) + 1;
    }

    /// <summary>
    /// Rounds to the nearest integer (midpoint rounds away from zero).
    /// </summary>
    public int RoundToInt()
    {
        if (RawValue >= 0)
        {
            long frac = RawValue & FRAC_MASK;
            long intPart = RawValue >> 32;
            return frac >= RAW_HALF ? (int)intPart + 1 : (int)intPart;
        }
        else
        {
            // Handle negative via absolute value
            long absRaw = -RawValue;
            long frac = absRaw & FRAC_MASK;
            long intPart = absRaw >> 32;
            int absResult = frac >= RAW_HALF ? (int)intPart + 1 : (int)intPart;
            return -absResult;
        }
    }

    /// <summary>
    /// ⚠️ Converts to <see cref="float"/>. Render layer only! Game logic MUST NOT use this.
    /// </summary>
    public float ToFloat() => (float)(RawValue / (double)RAW_ONE);

    /// <summary>
    /// Converts to <see cref="double"/>. For diagnostics / serialization only.
    /// </summary>
    public double ToDouble() => RawValue / (double)RAW_ONE;

    // ========================================================================
    // Arithmetic Operators
    // ========================================================================

    /// <inheritdoc />
    public static FP operator +(FP a, FP b) => new(a.RawValue + b.RawValue);

    /// <inheritdoc />
    public static FP operator -(FP a, FP b) => new(a.RawValue - b.RawValue);

    /// <summary>Unary negation.</summary>
    public static FP operator -(FP v) => new(-v.RawValue);

    /// <summary>
    /// Multiplies two fixed-point numbers using <see cref="Int128"/> to prevent overflow.
    /// </summary>
    public static FP operator *(FP a, FP b)
    {
        var product = (Int128)a.RawValue * b.RawValue;
        return new FP((long)(product >> 32));
    }

    /// <summary>
    /// Divides two fixed-point numbers using <see cref="Int128"/> for precision.
    /// </summary>
    public static FP operator /(FP a, FP b)
    {
        if (b.RawValue == 0)
            throw new DivideByZeroException("Division by zero in fixed-point arithmetic.");
        var dividend = (Int128)a.RawValue << 32;
        return new FP((long)(dividend / b.RawValue));
    }

    /// <summary>
    /// Remainder (modulo) operation. Result has the sign of the dividend.
    /// </summary>
    public static FP operator %(FP a, FP b)
    {
        if (b.RawValue == 0)
            throw new DivideByZeroException("Modulo by zero in fixed-point arithmetic.");
        return new FP(a.RawValue % b.RawValue);
    }

    // ========================================================================
    // Comparison Operators
    // ========================================================================

    /// <inheritdoc />
    public static bool operator ==(FP a, FP b) => a.RawValue == b.RawValue;

    /// <inheritdoc />
    public static bool operator !=(FP a, FP b) => a.RawValue != b.RawValue;

    /// <inheritdoc />
    public static bool operator >(FP a, FP b) => a.RawValue > b.RawValue;

    /// <inheritdoc />
    public static bool operator <(FP a, FP b) => a.RawValue < b.RawValue;

    /// <inheritdoc />
    public static bool operator >=(FP a, FP b) => a.RawValue >= b.RawValue;

    /// <inheritdoc />
    public static bool operator <=(FP a, FP b) => a.RawValue <= b.RawValue;

    // ========================================================================
    // Basic Math Methods
    // ========================================================================

    /// <summary>Returns the absolute value.</summary>
    public static FP Abs(FP value) => value.RawValue >= 0 ? value : new FP(-value.RawValue);

    /// <summary>Returns the sign: 1 for positive, -1 for negative, 0 for zero.</summary>
    public static FP Sign(FP value)
    {
        if (value.RawValue > 0) return One;
        if (value.RawValue < 0) return MinusOne;
        return Zero;
    }

    /// <summary>Returns the smaller of two values.</summary>
    public static FP Min(FP a, FP b) => a.RawValue <= b.RawValue ? a : b;

    /// <summary>Returns the larger of two values.</summary>
    public static FP Max(FP a, FP b) => a.RawValue >= b.RawValue ? a : b;

    /// <summary>Clamps a value between <paramref name="min"/> and <paramref name="max"/>.</summary>
    public static FP Clamp(FP value, FP min, FP max)
    {
        if (value.RawValue < min.RawValue) return min;
        if (value.RawValue > max.RawValue) return max;
        return value;
    }

    /// <summary>Clamps a value between 0 and 1.</summary>
    public static FP Clamp01(FP value) => Clamp(value, Zero, One);

    // ========================================================================
    // Rounding & Fractional Methods
    // ========================================================================

    /// <summary>Returns the largest integer ≤ value (floor).</summary>
    public static FP Floor(FP value)
    {
        long frac = value.RawValue & FRAC_MASK;
        if (frac == 0)
            return value;
        long intPart = value.RawValue & ~FRAC_MASK;
        // For both positive and negative: clearing the fractional bits
        // already gives the floor (arithmetic shift floors negatives correctly)
        return new FP(intPart);
    }

    /// <summary>Returns the smallest integer ≥ value (ceil).</summary>
    public static FP Ceil(FP value)
    {
        long frac = value.RawValue & FRAC_MASK;
        if (frac == 0)
            return value;
        long intPart = value.RawValue & ~FRAC_MASK;
        return new FP(intPart + RAW_ONE);
    }

    /// <summary>Rounds to the nearest integer (midpoint rounds away from zero).</summary>
    public static FP Round(FP value)
    {
        long frac = value.RawValue & FRAC_MASK;
        long intPart = value.RawValue & ~FRAC_MASK;

        if (value.RawValue >= 0)
        {
            if (frac >= RAW_HALF)
                return new FP(intPart + RAW_ONE);
            return new FP(intPart);
        }
        else
        {
            // For negatives: frac is the lower 32 bits of the negative raw value.
            // The fractional magnitude is |frac| for positive raw → but for negative raw,
            // the actual fractional part in [0,1) sense is (frac - 0) when raw is negative
            // with two's complement. Simpler: use Abs then round then re-negate.
            FP abs = Abs(value);
            FP roundedAbs = Round(abs);
            return -roundedAbs;
        }
    }

    /// <summary>Returns the fractional part (value - floor(value)) in [0, 1).</summary>
    public static FP Fract(FP value)
    {
        long frac = value.RawValue & FRAC_MASK;
        return new FP(frac);
    }

    // ========================================================================
    // Interpolation
    // ========================================================================

    /// <summary>
    /// Linearly interpolates between <paramref name="a"/> and <paramref name="b"/>
    /// by <paramref name="t"/>. <paramref name="t"/> is clamped to [0, 1].
    /// </summary>
    public static FP Lerp(FP a, FP b, FP t)
    {
        t = Clamp01(t);
        return a + (b - a) * t;
    }

    /// <summary>
    /// Linearly interpolates between <paramref name="a"/> and <paramref name="b"/>
    /// by <paramref name="t"/> without clamping.
    /// </summary>
    public static FP LerpUnclamped(FP a, FP b, FP t)
    {
        return a + (b - a) * t;
    }

    // ========================================================================
    // Comparison Helpers
    // ========================================================================

    /// <summary>
    /// Returns true if the difference between <paramref name="a"/> and
    /// <paramref name="b"/> is less than <see cref="Epsilon"/>.
    /// </summary>
    public static bool Approximately(FP a, FP b)
    {
        return Approximately(a, b, Epsilon);
    }

    /// <summary>
    /// Returns true if the difference between <paramref name="a"/> and
    /// <paramref name="b"/> is less than <paramref name="tolerance"/>.
    /// </summary>
    public static bool Approximately(FP a, FP b, FP tolerance)
    {
        FP diff = Abs(a - b);
        return diff < tolerance;
    }

    // ========================================================================
    // Interface Implementations
    // ========================================================================

    /// <inheritdoc />
    public bool Equals(FP other) => RawValue == other.RawValue;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is FP other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => RawValue.GetHashCode();

    /// <inheritdoc />
    public int CompareTo(FP other) => RawValue.CompareTo(other.RawValue);

    /// <summary>
    /// Returns a string representation with exactly 6 decimal places (e.g. "123.456789").
    /// Pure fixed-point implementation — no floating-point used.
    /// </summary>
    public override string ToString()
    {
        long raw = RawValue;
        if (raw == 0)
            return "0.000000";

        bool negative = raw < 0;
        // Work with absolute value to avoid overflow at MinValue
        long absRaw = negative ? -raw : raw;

        long intPart = absRaw >> 32;
        long fracRaw = absRaw & FRAC_MASK;

        // Compute 6 fractional digits: fracRaw * 1_000_000 / 2^32, rounded
        // Use Int128 to avoid overflow
        long fracScaled = (long)(((Int128)fracRaw * RAW_ONE_MILLION + (RAW_ONE >> 1)) >> 32);

        // Handle carry from rounding
        if (fracScaled >= RAW_ONE_MILLION)
        {
            intPart++;
            fracScaled -= RAW_ONE_MILLION;
        }

        string sign = negative ? "-" : "";
        return $"{sign}{intPart}.{fracScaled:D6}";
    }
}
