using System;
using System.Numerics;

namespace FrameForge.FixedPoint;

// ============================================================================
// Advanced Math: Sqrt, Exp, Log, Pow2
// ============================================================================

public readonly partial struct FP
{
    // ========================================================================
    // Sqrt (Newton's Method)
    // ========================================================================

    /// <summary>
    /// Returns the square root of <paramref name="value"/> using Newton's method
    /// with a bit-length-based initial guess for rapid convergence.
    /// 8 iterations guarantee convergence within <see cref="Epsilon"/> for Q32.32.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if value is negative.</exception>
    public static FP Sqrt(FP value)
    {
        if (value.RawValue < 0)
            throw new ArgumentOutOfRangeException(nameof(value),
                "Cannot compute square root of a negative fixed-point number.");
        if (value.RawValue == 0)
            return Zero;

        // --- Initial guess via MSB position --------------------------------
        // sqrt(x) raw value has roughly (bit_length(x_raw) + 32) / 2 bits.
        // Starting from the right bit position makes Newton converge in ≤ 5
        // iterations even for extreme values like 1e6.
        int msb = 63 - BitOperations.LeadingZeroCount((ulong)value.RawValue);
        int shift = (msb + 32) / 2;
        if (shift >= 63)
            shift = 62; // prevent overflow
        long guessRaw = 1L << shift;

        FP guess = new FP(guessRaw);

        // 8 Newton iterations: x_{n+1} = (x_n + value / x_n) / 2
        // With the improved initial guess this is more than sufficient.
        for (int i = 0; i < 8; i++)
        {
            if (guess.RawValue == 0) // Safety: avoid division by zero
                guess = Epsilon;
            guess = (guess + value / guess) / Two;
        }

        return guess;
    }

    /// <summary>
    /// Returns 1 / Sqrt(<paramref name="value"/>). Useful for normalizing vectors.
    /// </summary>
    public static FP InvSqrt(FP value)
    {
        return One / Sqrt(value);
    }

    // ========================================================================
    // Exp (e^x via Taylor Series)
    // ========================================================================

    /// <summary>
    /// Returns e raised to the power <paramref name="exponent"/>.
    /// Uses Taylor series with range reduction for large |x|.
    /// Precision: &lt; 1 × 10⁻⁵.
    /// </summary>
    public static FP Exp(FP exponent)
    {
        // Handle special cases
        if (exponent.RawValue == 0)
            return One;
        if (exponent == One)
            return E;

        bool negative = exponent.RawValue < 0;
        FP x = negative ? -exponent : exponent;

        // Range reduction: if x > 1, compute exp(x/2) and square it
        // Keep reducing until x <= 1
        int reductions = 0;
        while (x > One)
        {
            x /= Two;
            reductions++;
        }

        // Taylor series: exp(x) = 1 + x + x²/2! + x³/3! + ...
        // Use 15 terms for good precision in [0, 1]
        FP result = One;
        FP term = One;
        for (int i = 1; i <= 15; i++)
        {
            // term = term * x / i
            term = term * x / FromInt(i);
            result += term;

            // Early exit if term is negligible
            if (Abs(term) < Epsilon)
                break;
        }

        // Reverse the range reduction: square the result
        for (int i = 0; i < reductions; i++)
        {
            result = result * result;
        }

        // Handle negative exponent: exp(-x) = 1 / exp(x)
        if (negative)
            result = One / result;

        return result;
    }

    // ========================================================================
    // Log (Natural Logarithm via Atanh Series)
    // ========================================================================

    /// <summary>
    /// Returns the natural logarithm (base e) of <paramref name="value"/>.
    /// Uses the identity: log(x) = 2 * atanh((x-1)/(x+1)), with range reduction
    /// for values far from 1.
    /// Precision: &lt; 1 × 10⁻⁵.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if value ≤ 0.</exception>
    public static FP Log(FP value)
    {
        if (value.RawValue <= 0)
            throw new ArgumentOutOfRangeException(nameof(value),
                "Logarithm is defined only for positive values.");
        if (value == One)
            return Zero;

        // Range reduction: log(x * 2^n) = log(x) + n * log(2)
        // Normalize value to [0.5, 2] for fast convergence of atanh series
        int shifts = 0;
        FP normalized = value;

        // Scale up if too small
        while (normalized < Half)
        {
            normalized *= Two;
            shifts--;
        }

        // Scale down if too large
        while (normalized > Two)
        {
            normalized /= Two;
            shifts++;
        }

        // Compute log(normalized) using atanh series:
        // log(x) = 2 * atanh(y) where y = (x-1)/(x+1)
        FP y = (normalized - One) / (normalized + One);
        FP y2 = y * y;
        FP atanhResult = y;
        FP term = y;

        // atanh(y) = y + y³/3 + y⁵/5 + y⁷/7 + ...
        // Converges fast for |y| < 1/3 (when x in [0.5, 2])
        for (int i = 3; i <= 31; i += 2)
        {
            term = term * y2;
            FP nextTerm = term / FromInt(i);
            atanhResult += nextTerm;

            if (Abs(nextTerm) < Epsilon)
                break;
        }

        FP result = Two * atanhResult;

        // Add back the range reduction
        if (shifts != 0)
        {
            result += FromInt(shifts) * Ln2;
        }

        return result;
    }

    /// <summary>
    /// Returns the base-2 logarithm of <paramref name="value"/>.
    /// </summary>
    public static FP Log2(FP value)
    {
        return Log(value) / Ln2;
    }

    /// <summary>
    /// Returns the base-10 logarithm of <paramref name="value"/>.
    /// </summary>
    public static FP Log10(FP value)
    {
        return Log(value) / Ln10;
    }

    // ========================================================================
    // Pow2 (2^x)
    // ========================================================================

    /// <summary>
    /// Returns 2 raised to the power <paramref name="exponent"/>.
    /// For integer exponents the result is exact; for fractional exponents
    /// it uses <c>Exp(exponent * Ln2)</c>.
    /// </summary>
    public static FP Pow2(FP exponent)
    {
        // Check if exponent is a whole number
        if ((exponent.RawValue & 0xFFFFFFFFL) == 0)
        {
            // Integer exponent: use bit shifting for exact results
            int exp = exponent.ToInt();
            if (exp >= 0)
            {
                if (exp == 0) return One;
                if (exp == 1) return Two;

                // Check for overflow
                if (exp >= 31)
                    throw new OverflowException(
                        $"2^{exp} overflows Q32.32 fixed-point range.");

                return new FP(1L << (32 + exp));
            }
            else
            {
                // 2^(-n) = 1 / 2^n
                if (exp <= -32)
                    return Zero;
                return new FP(1L << (32 + exp)); // exp is negative, so 32+exp < 32
            }
        }

        // Fractional exponent: 2^x = exp(x * ln(2))
        return Exp(exponent * Ln2);
    }
}
