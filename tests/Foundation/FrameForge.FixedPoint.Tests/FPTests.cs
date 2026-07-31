using FrameForge.Foundation.FixedPoint;
using Xunit;

namespace FrameForge.Foundation.FixedPoint.Tests
{

// ============================================================================
// FPTests — Comprehensive unit tests for the FP (Q32.32) fixed-point type.
// Organized by functional area. Target: >= 95% code coverage.
// ============================================================================

public class FPTests
{
    // ========================================================================
    // Constants
    // ========================================================================

    [Fact]
    public void Constants_Zero_IsZero()
    {
        Assert.Equal(0L, FP.Zero.RawValue);
        Assert.Equal(FP.Zero, FP.Zero);
    }

    [Fact]
    public void Constants_One_EqualsOne()
    {
        Assert.Equal(1L << 32, FP.One.RawValue);
    }

    [Fact]
    public void Constants_Half_EqualsPointFive()
    {
        Assert.Equal(FP.One / FP.Two, FP.Half);
    }

    [Fact]
    public void Constants_Two_EqualsTwo()
    {
        Assert.Equal(FP.One + FP.One, FP.Two);
    }

    [Fact]
    public void Constants_MinusOne_NegatesCorrectly()
    {
        Assert.Equal(-FP.One, FP.MinusOne);
        Assert.True(FP.MinusOne.RawValue < 0);
    }

    [Fact]
    public void Constants_PI_IsPositive()
    {
        Assert.True(FP.PI > FP.Zero);
        Assert.True(FP.PI > FP.FromInt(3));
        Assert.True(FP.PI < FP.FromInt(4));
    }

    [Fact]
    public void Constants_E_IsPositive()
    {
        Assert.True(FP.E > FP.Zero);
        Assert.True(FP.E > FP.FromInt(2));
        Assert.True(FP.E < FP.FromInt(3));
    }

    [Fact]
    public void Constants_Deg2Rad_Rad2Deg_AreInverse()
    {
        FP oneDegreeInRad = FP.Deg2Rad;
        FP backToDeg = oneDegreeInRad * FP.Rad2Deg;
        backToDeg.ShouldBeApproximately(FP.One);
    }

    [Fact]
    public void Constants_PointOne_IsApproximatelyOneTenth()
    {
        FP tenTimes = FP.PointOne * FP.Ten;
        tenTimes.ShouldBeApproximately(FP.One, FP.Epsilon * FP.FromInt(100));
    }

    [Fact]
    public void Constants_Epsilon_IsSmallestPositive()
    {
        Assert.True(FP.Epsilon > FP.Zero);
        Assert.Equal(1L, FP.Epsilon.RawValue);
    }

    // ========================================================================
    // Creation & Conversion
    // ========================================================================

    [Fact]
    public void FromInt_CreatesCorrectValue()
    {
        FP five = FP.FromInt(5);
        Assert.Equal(5L << 32, five.RawValue);
    }

    [Fact]
    public void FromInt_Negative_CreatesCorrectValue()
    {
        FP negThree = FP.FromInt(-3);
        Assert.Equal(-3L << 32, negThree.RawValue);
    }

    [Fact]
    public void FromLong_ValidRange_CreatesCorrectValue()
    {
        FP value = FP.FromLong(100);
        Assert.Equal(100L << 32, value.RawValue);
    }

    [Fact]
    public void FromLong_ExceedsIntMax_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FP.FromLong((long)int.MaxValue + 1));
    }

    [Fact]
    public void FromFloat_RoundTripsApproximately()
    {
        FP value = FP.FromFloat(3.14f);
        float back = value.ToFloat();
        double diff = System.Math.Abs(3.14f - back);
        Assert.True(diff < 1e-6, $"Expected ~3.14, got {back}");
    }

    [Fact]
    public void FromDouble_RoundTripsApproximately()
    {
        FP value = FP.FromDouble(2.718281828);
        double back = value.ToDouble();
        double diff = System.Math.Abs(2.718281828 - back);
        Assert.True(diff < 1e-9, $"Expected ~2.718281828, got {back}");
    }

    [Fact]
    public void ToInt_Positive_Truncates()
    {
        FP value = FP.FromInt(3) + FP.Half; // 3.5
        Assert.Equal(3, value.ToInt());
    }

    [Fact]
    public void ToInt_Negative_TruncatesTowardZero()
    {
        FP value = -(FP.FromInt(3) + FP.Half); // -3.5
        Assert.Equal(-3, value.ToInt());
    }

    [Fact]
    public void FloorToInt_Positive_Works()
    {
        FP value = FP.FromInt(3) + FP.Half; // 3.5
        Assert.Equal(3, value.FloorToInt());
    }

    [Fact]
    public void FloorToInt_Negative_Works()
    {
        FP value = -(FP.FromInt(3) + FP.Half); // -3.5
        Assert.Equal(-4, value.FloorToInt());
    }

    [Fact]
    public void CeilToInt_Positive_Works()
    {
        FP value = FP.FromInt(3) + FP.Half; // 3.5
        Assert.Equal(4, value.CeilToInt());
    }

    [Fact]
    public void CeilToInt_Negative_Works()
    {
        FP value = -(FP.FromInt(3) + FP.Half); // -3.5
        Assert.Equal(-3, value.CeilToInt());
    }

    [Fact]
    public void RoundToInt_Midpoint_RoundsAwayFromZero()
    {
        Assert.Equal(4, (FP.FromInt(3) + FP.Half).RoundToInt());  // 3.5 → 4
        Assert.Equal(-4, (-(FP.FromInt(3) + FP.Half)).RoundToInt()); // -3.5 → -4
    }

    // ========================================================================
    // Arithmetic Operators
    // ========================================================================

    [Fact]
    public void Add_TwoPositive_ReturnsSum()
    {
        FP a = FP.FromInt(2);
        FP b = FP.FromInt(3);
        Assert.Equal(FP.FromInt(5), a + b);
    }

    [Fact]
    public void Add_PositiveNegative_ReturnsDifference()
    {
        FP a = FP.FromInt(5);
        FP b = FP.FromInt(-2);
        Assert.Equal(FP.FromInt(3), a + b);
    }

    [Fact]
    public void Subtract_TwoPositive_ReturnsDifference()
    {
        FP a = FP.FromInt(10);
        FP b = FP.FromInt(3);
        Assert.Equal(FP.FromInt(7), a - b);
    }

    [Fact]
    public void UnaryNegation_Works()
    {
        FP a = FP.FromInt(7);
        Assert.Equal(FP.FromInt(-7), -a);
    }

    [Fact]
    public void Multiply_TwoIntegers_ReturnsProduct()
    {
        FP a = FP.FromInt(6);
        FP b = FP.FromInt(7);
        Assert.Equal(FP.FromInt(42), a * b);
    }

    [Fact]
    public void Multiply_Fractional_ReturnsCorrectProduct()
    {
        FP a = FP.Half;
        FP b = FP.Half;
        // 0.5 * 0.5 = 0.25
        FP expected = FP.One / FP.FromInt(4);
        (a * b).ShouldBeApproximately(expected);
    }

    [Fact]
    public void Multiply_Large_DoesNotOverflow()
    {
        // Q32.32 max integer is ~2.1e9, so test near the safe range
        FP a = FP.FromInt(46340); // √(2^31) ≈ 46340
        FP b = FP.FromInt(46340);
        FP result = a * b;
        Assert.True(result > FP.Zero);
    }

    [Fact]
    public void Divide_TwoIntegers_ReturnsQuotient()
    {
        FP a = FP.FromInt(42);
        FP b = FP.FromInt(7);
        Assert.Equal(FP.FromInt(6), a / b);
    }

    [Fact]
    public void Divide_NonDivisible_ReturnsFraction()
    {
        FP a = FP.FromInt(1);
        FP b = FP.FromInt(3);
        FP result = a / b;
        // result * 3 should be approximately 1
        (result * FP.FromInt(3)).ShouldBeApproximately(FP.One);
    }

    [Fact]
    public void Divide_ByZero_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => FP.One / FP.Zero);
    }

    [Fact]
    public void Modulo_Positive_Works()
    {
        FP a = FP.FromInt(10);
        FP b = FP.FromInt(3);
        FP result = a % b;
        result.ShouldBeApproximately(FP.One);
    }

    [Fact]
    public void Modulo_ByZero_Throws()
    {
        Assert.Throws<DivideByZeroException>(() => FP.One % FP.Zero);
    }

    // ========================================================================
    // Arithmetic Properties
    // ========================================================================

    [Fact]
    public void Arithmetic_CommutativeAdd()
    {
        FP a = FP.FromInt(3);
        FP b = FP.FromInt(7);
        Assert.Equal(a + b, b + a);
    }

    [Fact]
    public void Arithmetic_CommutativeMultiply()
    {
        FP a = FP.FromInt(3);
        FP b = FP.FromInt(7);
        Assert.Equal(a * b, b * a);
    }

    [Fact]
    public void Arithmetic_AssociativeAdd()
    {
        FP a = FP.FromInt(1);
        FP b = FP.FromInt(2);
        FP c = FP.FromInt(3);
        Assert.Equal((a + b) + c, a + (b + c));
    }

    [Fact]
    public void Arithmetic_Distributive()
    {
        FP a = FP.FromInt(2);
        FP b = FP.FromInt(3);
        FP c = FP.FromInt(4);
        (a * (b + c)).ShouldBeApproximately(a * b + a * c);
    }

    // ========================================================================
    // Comparison Operators
    // ========================================================================

    [Fact]
    public void Comparison_Equals_Works()
    {
        FP a = FP.FromInt(5);
        FP b = FP.FromInt(5);
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void Comparison_NotEquals_Works()
    {
        FP a = FP.FromInt(5);
        FP b = FP.FromInt(6);
        Assert.True(a != b);
        Assert.False(a == b);
    }

    [Fact]
    public void Comparison_GreaterThan_Works()
    {
        Assert.True(FP.FromInt(5) > FP.FromInt(3));
        Assert.False(FP.FromInt(3) > FP.FromInt(5));
        Assert.False(FP.FromInt(3) > FP.FromInt(3));
    }

    [Fact]
    public void Comparison_LessThan_Works()
    {
        Assert.True(FP.FromInt(3) < FP.FromInt(5));
        Assert.False(FP.FromInt(5) < FP.FromInt(3));
        Assert.False(FP.FromInt(3) < FP.FromInt(3));
    }

    [Fact]
    public void Comparison_GreaterOrEqual_Works()
    {
        Assert.True(FP.FromInt(5) >= FP.FromInt(3));
        Assert.True(FP.FromInt(3) >= FP.FromInt(3));
        Assert.False(FP.FromInt(3) >= FP.FromInt(5));
    }

    [Fact]
    public void Comparison_LessOrEqual_Works()
    {
        Assert.True(FP.FromInt(3) <= FP.FromInt(5));
        Assert.True(FP.FromInt(3) <= FP.FromInt(3));
        Assert.False(FP.FromInt(5) <= FP.FromInt(3));
    }

    [Fact]
    public void Comparison_NegativeValues_Work()
    {
        Assert.True(FP.MinusOne < FP.Zero);
        Assert.True(FP.FromInt(-5) < FP.FromInt(-3));
        Assert.True(FP.FromInt(-3) > FP.FromInt(-5));
    }

    // ========================================================================
    // Basic Math
    // ========================================================================

    [Fact]
    public void Abs_Positive_ReturnsSame()
    {
        Assert.Equal(FP.FromInt(5), FP.Abs(FP.FromInt(5)));
    }

    [Fact]
    public void Abs_Negative_ReturnsPositive()
    {
        Assert.Equal(FP.FromInt(5), FP.Abs(FP.FromInt(-5)));
    }

    [Fact]
    public void Abs_Zero_ReturnsZero()
    {
        Assert.Equal(FP.Zero, FP.Abs(FP.Zero));
    }

    [Fact]
    public void Sign_Positive_ReturnsOne()
    {
        Assert.Equal(FP.One, FP.Sign(FP.FromInt(42)));
    }

    [Fact]
    public void Sign_Negative_ReturnsMinusOne()
    {
        Assert.Equal(FP.MinusOne, FP.Sign(FP.FromInt(-42)));
    }

    [Fact]
    public void Sign_Zero_ReturnsZero()
    {
        Assert.Equal(FP.Zero, FP.Sign(FP.Zero));
    }

    [Fact]
    public void Min_ReturnsSmaller()
    {
        Assert.Equal(FP.FromInt(3), FP.Min(FP.FromInt(3), FP.FromInt(5)));
        Assert.Equal(FP.FromInt(-5), FP.Min(FP.FromInt(3), FP.FromInt(-5)));
    }

    [Fact]
    public void Max_ReturnsLarger()
    {
        Assert.Equal(FP.FromInt(5), FP.Max(FP.FromInt(3), FP.FromInt(5)));
        Assert.Equal(FP.FromInt(3), FP.Max(FP.FromInt(3), FP.FromInt(-5)));
    }

    [Fact]
    public void Clamp_WithinRange_ReturnsValue()
    {
        Assert.Equal(FP.FromInt(5), FP.Clamp(FP.FromInt(5), FP.FromInt(0), FP.FromInt(10)));
    }

    [Fact]
    public void Clamp_Below_ReturnsMin()
    {
        Assert.Equal(FP.FromInt(0), FP.Clamp(FP.FromInt(-5), FP.FromInt(0), FP.FromInt(10)));
    }

    [Fact]
    public void Clamp_Above_ReturnsMax()
    {
        Assert.Equal(FP.FromInt(10), FP.Clamp(FP.FromInt(15), FP.FromInt(0), FP.FromInt(10)));
    }

    [Fact]
    public void Clamp01_Works()
    {
        Assert.Equal(FP.One, FP.Clamp01(FP.FromInt(2)));
        Assert.Equal(FP.Zero, FP.Clamp01(FP.FromInt(-1)));
        Assert.Equal(FP.Half, FP.Clamp01(FP.Half));
    }

    // ========================================================================
    // Rounding & Fractional
    // ========================================================================

    [Fact]
    public void Floor_ExactInteger_ReturnsSame()
    {
        Assert.Equal(FP.FromInt(5), FP.Floor(FP.FromInt(5)));
    }

    [Fact]
    public void Floor_PositiveFraction_ReturnsIntegerPart()
    {
        Assert.Equal(FP.FromInt(3), FP.Floor(FP.FromInt(3) + FP.Half));
    }

    [Fact]
    public void Floor_NegativeFraction_ReturnsLowerInteger()
    {
        FP value = -(FP.FromInt(3) + FP.Half); // -3.5
        Assert.Equal(FP.FromInt(-4), FP.Floor(value));
    }

    [Fact]
    public void Ceil_ExactInteger_ReturnsSame()
    {
        Assert.Equal(FP.FromInt(5), FP.Ceil(FP.FromInt(5)));
    }

    [Fact]
    public void Ceil_PositiveFraction_ReturnsNextInteger()
    {
        Assert.Equal(FP.FromInt(4), FP.Ceil(FP.FromInt(3) + FP.Half));
    }

    [Fact]
    public void Ceil_NegativeFraction_ReturnsHigherInteger()
    {
        FP value = -(FP.FromInt(3) + FP.Half); // -3.5
        Assert.Equal(FP.FromInt(-3), FP.Ceil(value));
    }

    [Fact]
    public void Round_Midpoint_AwayFromZero()
    {
        Assert.Equal(FP.FromInt(4), FP.Round(FP.FromInt(3) + FP.Half));
        Assert.Equal(FP.FromInt(-4), FP.Round(-(FP.FromInt(3) + FP.Half)));
    }

    [Fact]
    public void Round_BelowMidpoint_ReturnsFloor()
    {
        FP value = FP.FromInt(3) + FP.PointOne; // 3.1
        Assert.Equal(FP.FromInt(3), FP.Round(value));
    }

    [Fact]
    public void Fract_Positive_ReturnsFraction()
    {
        FP value = FP.FromInt(3) + FP.Half; // 3.5
        FP fract = FP.Fract(value);
        fract.ShouldBeApproximately(FP.Half);
    }

    [Fact]
    public void Fract_Negative_ReturnsPositiveFraction()
    {
        FP value = -(FP.FromInt(3) + FP.Half); // -3.5
        FP fract = FP.Fract(value);
        fract.ShouldBeApproximately(FP.Half);
        Assert.True(fract >= FP.Zero);
        Assert.True(fract < FP.One);
    }

    [Fact]
    public void Fract_ExactInteger_ReturnsZero()
    {
        Assert.Equal(FP.Zero, FP.Fract(FP.FromInt(5)));
        Assert.Equal(FP.Zero, FP.Fract(FP.FromInt(-5)));
    }

    // ========================================================================
    // Interpolation
    // ========================================================================

    [Fact]
    public void Lerp_TZero_ReturnsA()
    {
        FP a = FP.FromInt(0);
        FP b = FP.FromInt(10);
        Assert.Equal(a, FP.Lerp(a, b, FP.Zero));
    }

    [Fact]
    public void Lerp_TOne_ReturnsB()
    {
        FP a = FP.FromInt(0);
        FP b = FP.FromInt(10);
        Assert.Equal(b, FP.Lerp(a, b, FP.One));
    }

    [Fact]
    public void Lerp_THalf_ReturnsMidpoint()
    {
        FP a = FP.FromInt(0);
        FP b = FP.FromInt(10);
        FP result = FP.Lerp(a, b, FP.Half);
        result.ShouldBeApproximately(FP.FromInt(5));
    }

    [Fact]
    public void Lerp_TClamped_StaysInRange()
    {
        FP a = FP.FromInt(0);
        FP b = FP.FromInt(10);
        FP result = FP.Lerp(a, b, FP.FromInt(2)); // t = 2, should clamp to 1
        Assert.Equal(b, result);
    }

    [Fact]
    public void LerpUnclamped_Two_Extrapolates()
    {
        FP a = FP.FromInt(0);
        FP b = FP.FromInt(10);
        FP result = FP.LerpUnclamped(a, b, FP.FromInt(2));
        result.ShouldBeApproximately(FP.FromInt(20));
    }

    // ========================================================================
    // String Parsing & Formatting
    // ========================================================================

    [Fact]
    public void Parse_Integer_Works()
    {
        FP value = FP.Parse("42");
        Assert.Equal(FP.FromInt(42), value);
    }

    [Fact]
    public void Parse_Decimal_Works()
    {
        FP value = FP.Parse("3.5");
        FP expected = FP.FromInt(3) + FP.Half;
        value.ShouldBeApproximately(expected);
    }

    [Fact]
    public void Parse_Negative_Works()
    {
        FP value = FP.Parse("-1.25");
        Assert.True(value < FP.Zero);
        value.ShouldBeApproximately(-(FP.One + FP.One / FP.FromInt(4)));
    }

    [Fact]
    public void Parse_PlusSign_Works()
    {
        FP value = FP.Parse("+7.5");
        value.ShouldBeApproximately(FP.FromInt(7) + FP.Half);
    }

    [Fact]
    public void Parse_Invalid_Throws()
    {
        Assert.Throws<FormatException>(() => FP.Parse("abc"));
        Assert.Throws<FormatException>(() => FP.Parse(""));
        Assert.Throws<FormatException>(() => FP.Parse("12.34.56"));
    }

    [Fact]
    public void TryParse_Valid_ReturnsTrue()
    {
        Assert.True(FP.TryParse("123.456", out FP result));
        Assert.True(result > FP.Zero);
    }

    [Fact]
    public void TryParse_Invalid_ReturnsFalse()
    {
        Assert.False(FP.TryParse("hello", out FP _));
    }

    [Fact]
    public void ToString_Zero_ReturnsFormatted()
    {
        Assert.Equal("0.000000", FP.Zero.ToString());
    }

    [Fact]
    public void ToString_PositiveInteger()
    {
        Assert.Equal("5.000000", FP.FromInt(5).ToString());
    }

    [Fact]
    public void ToString_NegativeInteger()
    {
        Assert.Equal("-3.000000", FP.FromInt(-3).ToString());
    }

    [Fact]
    public void ToString_Fraction()
    {
        string s = FP.Half.ToString();
        Assert.Equal("0.500000", s);
    }

    [Fact]
    public void Parse_ToString_RoundTrip()
    {
        FP[] testValues = {
            FP.Zero, FP.One, FP.Half, FP.MinusOne,
            FP.FromInt(42), FP.FromInt(-7),
            FP.FromInt(3) + FP.Half,
            FP.FromInt(100) + FP.FromInt(1) / FP.FromInt(3)
        };

        foreach (FP val in testValues)
        {
            string s = val.ToString();
            FP parsed = FP.Parse(s);
            // 6 decimal places in ToString means round-trip is approximate,
            // not exact. Tolerance: 1e-6 * 2 units.
            parsed.ShouldBeApproximately(val, FP.Epsilon * FP.FromInt(10000));
        }
    }

    // ========================================================================
    // Sqrt
    // ========================================================================

    [Fact]
    public void Sqrt_PerfectSquare_ExactResult()
    {
        Assert.Equal(FP.FromInt(4), FP.Sqrt(FP.FromInt(16)));
        Assert.Equal(FP.FromInt(3), FP.Sqrt(FP.FromInt(9)));
        Assert.Equal(FP.FromInt(10), FP.Sqrt(FP.FromInt(100)));
    }

    [Fact]
    public void Sqrt_NonPerfectSquare_CloseApproximation()
    {
        FP sqrt2 = FP.Sqrt(FP.FromInt(2));
        // sqrt(2) ≈ 1.41421356...
        (sqrt2 * sqrt2).ShouldBeApproximately(FP.FromInt(2), FP.Epsilon * FP.FromInt(1000));
    }

    [Fact]
    public void Sqrt_One_ReturnsOne()
    {
        Assert.Equal(FP.One, FP.Sqrt(FP.One));
    }

    [Fact]
    public void Sqrt_Zero_ReturnsZero()
    {
        Assert.Equal(FP.Zero, FP.Sqrt(FP.Zero));
    }

    [Fact]
    public void Sqrt_Fraction_Works()
    {
        // sqrt(0.25) = 0.5
        FP quarter = FP.Half * FP.Half;
        FP result = FP.Sqrt(quarter);
        result.ShouldBeApproximately(FP.Half);
    }

    [Fact]
    public void Sqrt_LargeValue_Works()
    {
        FP large = FP.FromInt(1_000_000);
        FP sqrt = FP.Sqrt(large);
        (sqrt * sqrt).ShouldBeApproximately(large, FP.Epsilon * FP.FromInt(10000));
    }

    [Fact]
    public void Sqrt_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FP.Sqrt(FP.MinusOne));
    }

    [Fact]
    public void InvSqrt_Works()
    {
        FP invSqrt4 = FP.InvSqrt(FP.FromInt(4));
        invSqrt4.ShouldBeApproximately(FP.Half);
    }

    // ========================================================================
    // Trigonometry: Sin / Cos / Tan
    // ========================================================================

    [Fact]
    public void Sin_Zero_ReturnsZero()
    {
        Assert.Equal(FP.Zero, FP.Sin(FP.Zero));
    }

    [Fact]
    public void Sin_PIOver2_ReturnsOne()
    {
        FP result = FP.Sin(FP.PI / FP.Two);
        result.ShouldBeApproximately(FP.One);
    }

    [Fact]
    public void Sin_PI_ReturnsZero()
    {
        FP result = FP.Sin(FP.PI);
        result.ShouldBeApproximately(FP.Zero);
    }

    [Fact]
    public void Sin_Oddness_SinNegX_Equals_NegSinX()
    {
        FP x = FP.PI / FP.FromInt(6); // PI/6 = 30 degrees
        FP sinNegX = FP.Sin(-x);
        FP negSinX = -FP.Sin(x);
        sinNegX.ShouldBeApproximately(negSinX);
    }

    [Fact]
    public void Cos_Zero_ReturnsOne()
    {
        FP result = FP.Cos(FP.Zero);
        result.ShouldBeApproximately(FP.One);
    }

    [Fact]
    public void Cos_PIOver2_ReturnsZero()
    {
        FP result = FP.Cos(FP.PI / FP.Two);
        result.ShouldBeApproximately(FP.Zero);
    }

    [Fact]
    public void Cos_PI_ReturnsMinusOne()
    {
        FP result = FP.Cos(FP.PI);
        result.ShouldBeApproximately(FP.MinusOne);
    }

    [Fact]
    public void SinCos_Identity_Sin2PlusCos2_EqualsOne()
    {
        FP[] angles = { FP.Zero, FP.PI / FP.FromInt(6), FP.PI / FP.FromInt(4),
                        FP.PI / FP.FromInt(3), FP.PI / FP.Two, FP.PI };

        foreach (FP angle in angles)
        {
            FP sinVal = FP.Sin(angle);
            FP cosVal = FP.Cos(angle);
            FP sum = sinVal * sinVal + cosVal * cosVal;
            sum.ShouldBeApproximatelyTrig(FP.One);
        }
    }

    [Fact]
    public void Sin_ShiftedByPIOver2_EqualsCos()
    {
        FP x = FP.PI / FP.FromInt(3); // 60 degrees
        FP sinShifted = FP.Sin(x + FP.PI / FP.Two);
        FP cosX = FP.Cos(x);
        sinShifted.ShouldBeApproximately(cosX);
    }

    [Fact]
    public void Tan_Zero_ReturnsZero()
    {
        Assert.Equal(FP.Zero, FP.Tan(FP.Zero));
    }

    [Fact]
    public void Tan_PIOver4_ReturnsOne()
    {
        FP result = FP.Tan(FP.PI / FP.FromInt(4));
        result.ShouldBeApproximately(FP.One);
    }

    [Fact]
    public void Tan_NearPIOver2_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FP.Tan(FP.PI / FP.Two));
    }

    [Fact]
    public void Sin_NegativeAngle_Works()
    {
        FP x = -FP.PI / FP.FromInt(6);
        FP result = FP.Sin(x);
        Assert.True(result < FP.Zero);
        result.ShouldBeApproximatelyTrig(-FP.Half);
    }

    [Fact]
    public void Sin_LargeAngle_NormalizedCorrectly()
    {
        // sin(5*PI/2) = sin(PI/2) = 1
        FP large = FP.PI * FP.FromInt(5) / FP.Two;
        FP result = FP.Sin(large);
        result.ShouldBeApproximately(FP.One);
    }

    // ========================================================================
    // Trigonometry: Asin / Acos / Atan / Atan2
    // ========================================================================

    [Fact]
    public void Asin_Zero_ReturnsZero()
    {
        Assert.Equal(FP.Zero, FP.Asin(FP.Zero));
    }

    [Fact]
    public void Asin_One_ReturnsPIOver2()
    {
        FP result = FP.Asin(FP.One);
        result.ShouldBeApproximately(FP.PI / FP.Two);
    }

    [Fact]
    public void Asin_MinusOne_ReturnsMinusPIOver2()
    {
        FP result = FP.Asin(FP.MinusOne);
        result.ShouldBeApproximately(-(FP.PI / FP.Two));
    }

    [Fact]
    public void Asin_RoundTrip_WithSin()
    {
        FP[] values = { FP.Zero, FP.Half, -FP.Half,
                        FP.FromInt(1) / FP.FromInt(3),
                        -FP.FromInt(2) / FP.FromInt(3) };

        foreach (FP val in values)
        {
            if (val > FP.One || val < FP.MinusOne)
                continue;
            FP asinVal = FP.Asin(val);
            FP sinBack = FP.Sin(asinVal);
            sinBack.ShouldBeApproximatelyTrig(val);
        }
    }

    [Fact]
    public void Asin_OutOfRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FP.Asin(FP.FromInt(2)));
        Assert.Throws<ArgumentOutOfRangeException>(() => FP.Asin(FP.FromInt(-2)));
    }

    [Fact]
    public void Acos_One_ReturnsZero()
    {
        FP result = FP.Acos(FP.One);
        result.ShouldBeApproximately(FP.Zero);
    }

    [Fact]
    public void Acos_Zero_ReturnsPIOver2()
    {
        FP result = FP.Acos(FP.Zero);
        result.ShouldBeApproximately(FP.PI / FP.Two);
    }

    [Fact]
    public void Acos_MinusOne_ReturnsPI()
    {
        FP result = FP.Acos(FP.MinusOne);
        result.ShouldBeApproximately(FP.PI);
    }

    [Fact]
    public void AsinAcos_Complementary()
    {
        FP x = FP.Half;
        FP asinX = FP.Asin(x);
        FP acosX = FP.Acos(x);
        (asinX + acosX).ShouldBeApproximately(FP.PI / FP.Two);
    }

    [Fact]
    public void Atan_Zero_ReturnsZero()
    {
        Assert.Equal(FP.Zero, FP.Atan(FP.Zero));
    }

    [Fact]
    public void Atan_One_ReturnsPIOver4()
    {
        FP result = FP.Atan(FP.One);
        result.ShouldBeApproximately(FP.PI / FP.FromInt(4));
    }

    [Fact]
    public void Atan_RoundTrip_WithTan()
    {
        FP[] angles = { FP.Zero, FP.PI / FP.FromInt(8), FP.PI / FP.FromInt(6),
                        FP.PI / FP.FromInt(5), FP.PI / FP.FromInt(4) };

        foreach (FP angle in angles)
        {
            FP tanVal = FP.Tan(angle);
            FP atanBack = FP.Atan(tanVal);
            atanBack.ShouldBeApproximatelyTrig(angle);
        }
    }

    [Fact]
    public void Atan_Oddness_AtanNegX_Equals_NegAtanX()
    {
        FP x = FP.FromInt(2);
        FP atanNegX = FP.Atan(-x);
        FP negAtanX = -FP.Atan(x);
        atanNegX.ShouldBeApproximately(negAtanX);
    }

    [Fact]
    public void Atan_LargeValue_ApproachesPIOver2()
    {
        FP result = FP.Atan(FP.FromInt(1000));
        Assert.True(result > FP.Zero);
        Assert.True(result < FP.PI / FP.Two);
        // atan(1000) ≈ PI/2 - 1/1000 + O(1/1000^3) ≈ PI/2 - 0.001
        // Should be within ~0.001 of PI/2
        FP diff = FP.PI / FP.Two - result;
        Assert.True(diff > FP.Zero);
        Assert.True(diff < FP.FromInt(1) / FP.FromInt(500)); // < 0.002
    }

    [Fact]
    public void Atan2_QuadrantI_ReturnsPositive()
    {
        FP result = FP.Atan2(FP.One, FP.One);
        result.ShouldBeApproximately(FP.PI / FP.FromInt(4));
    }

    [Fact]
    public void Atan2_QuadrantII_ReturnsObtuse()
    {
        FP result = FP.Atan2(FP.One, FP.MinusOne);
        Assert.True(result > FP.PI / FP.Two);
        Assert.True(result < FP.PI);
    }

    [Fact]
    public void Atan2_QuadrantIII_ReturnsNegative()
    {
        FP result = FP.Atan2(FP.MinusOne, FP.MinusOne);
        Assert.True(result < -FP.PI / FP.Two);
        Assert.True(result > -FP.PI);
    }

    [Fact]
    public void Atan2_QuadrantIV_ReturnsNegative()
    {
        FP result = FP.Atan2(FP.MinusOne, FP.One);
        Assert.True(result < FP.Zero);
        Assert.True(result > -FP.PI / FP.Two);
    }

    [Fact]
    public void Atan2_ZeroX_PositiveY_ReturnsPIOver2()
    {
        FP result = FP.Atan2(FP.One, FP.Zero);
        result.ShouldBeApproximately(FP.PI / FP.Two);
    }

    [Fact]
    public void Atan2_ZeroX_NegativeY_ReturnsMinusPIOver2()
    {
        FP result = FP.Atan2(FP.MinusOne, FP.Zero);
        result.ShouldBeApproximately(-(FP.PI / FP.Two));
    }

    [Fact]
    public void Atan2_ZeroBoth_ReturnsZero()
    {
        Assert.Equal(FP.Zero, FP.Atan2(FP.Zero, FP.Zero));
    }

    // ========================================================================
    // Degree Trig Functions
    // ========================================================================

    [Fact]
    public void SinDeg_90_ReturnsOne()
    {
        FP result = FP.SinDeg(FP.FromInt(90));
        result.ShouldBeApproximately(FP.One);
    }

    [Fact]
    public void CosDeg_180_ReturnsMinusOne()
    {
        FP result = FP.CosDeg(FP.FromInt(180));
        result.ShouldBeApproximately(FP.MinusOne);
    }

    [Fact]
    public void TanDeg_45_ReturnsOne()
    {
        FP result = FP.TanDeg(FP.FromInt(45));
        result.ShouldBeApproximately(FP.One);
    }

    [Fact]
    public void SinDeg_And_SinRad_Consistent()
    {
        FP deg30 = FP.FromInt(30);
        FP rad30 = FP.Deg2Rad * deg30;
        FP sinDeg = FP.SinDeg(deg30);
        FP sinRad = FP.Sin(rad30);
        sinDeg.ShouldBeApproximately(sinRad);
    }

    // ========================================================================
    // Exp
    // ========================================================================

    [Fact]
    public void Exp_Zero_ReturnsOne()
    {
        Assert.Equal(FP.One, FP.Exp(FP.Zero));
    }

    [Fact]
    public void Exp_One_ReturnsE()
    {
        FP result = FP.Exp(FP.One);
        result.ShouldBeApproximately(FP.E);
    }

    [Fact]
    public void Exp_NegativeOne_ReturnsOneOverE()
    {
        FP result = FP.Exp(FP.MinusOne);
        FP expected = FP.One / FP.E;
        result.ShouldBeApproximately(expected, FP.Epsilon * FP.FromInt(500));
    }

    [Fact]
    public void Exp_PositiveIncreases()
    {
        Assert.True(FP.Exp(FP.FromInt(2)) > FP.One);
    }

    [Fact]
    public void Exp_NegativeDecreases()
    {
        Assert.True(FP.Exp(FP.FromInt(-2)) < FP.One);
        Assert.True(FP.Exp(FP.FromInt(-2)) > FP.Zero);
    }

    [Fact]
    public void Exp_Additivity_Property()
    {
        FP a = FP.FromInt(1) / FP.FromInt(2);
        FP b = FP.FromInt(1) / FP.FromInt(3);
        FP expA_B = FP.Exp(a + b);
        FP expA_times_expB = FP.Exp(a) * FP.Exp(b);
        expA_B.ShouldBeApproximately(expA_times_expB, FP.Epsilon * FP.FromInt(1000));
    }

    // ========================================================================
    // Log
    // ========================================================================

    [Fact]
    public void Log_One_ReturnsZero()
    {
        Assert.Equal(FP.Zero, FP.Log(FP.One));
    }

    [Fact]
    public void Log_E_ReturnsOne()
    {
        FP result = FP.Log(FP.E);
        result.ShouldBeApproximately(FP.One);
    }

    [Fact]
    public void Log_NonPositive_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FP.Log(FP.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => FP.Log(FP.MinusOne));
    }

    [Fact]
    public void Log_LessThanOne_ReturnsNegative()
    {
        Assert.True(FP.Log(FP.Half) < FP.Zero);
    }

    [Fact]
    public void Log_Exp_Inverse()
    {
        FP[] values = { FP.Half, FP.One, FP.FromInt(2), FP.FromInt(3),
                        FP.FromInt(1) / FP.FromInt(3) };

        foreach (FP val in values)
        {
            FP logVal = FP.Log(val);
            FP expBack = FP.Exp(logVal);
            expBack.ShouldBeApproximately(val, FP.Epsilon * FP.FromInt(2000));
        }
    }

    [Fact]
    public void Log_Product_Property()
    {
        FP a = FP.FromInt(2);
        FP b = FP.FromInt(3);
        FP logProduct = FP.Log(a * b);
        FP sumLogs = FP.Log(a) + FP.Log(b);
        logProduct.ShouldBeApproximately(sumLogs, FP.Epsilon * FP.FromInt(500));
    }

    [Fact]
    public void Log2_PowersOfTwo_AreIntegers()
    {
        Assert.Equal(FP.Zero, FP.Log2(FP.One));
        FP log2of2 = FP.Log2(FP.Two);
        log2of2.ShouldBeApproximately(FP.One);
        FP log2of4 = FP.Log2(FP.FromInt(4));
        log2of4.ShouldBeApproximately(FP.FromInt(2));
    }

    [Fact]
    public void Log10_PowersOfTen_AreIntegers()
    {
        FP log10of10 = FP.Log10(FP.Ten);
        log10of10.ShouldBeApproximatelyTrans(FP.One);
        FP log10of100 = FP.Log10(FP.FromInt(100));
        log10of100.ShouldBeApproximatelyTrans(FP.FromInt(2));
    }

    // ========================================================================
    // Pow2
    // ========================================================================

    [Fact]
    public void Pow2_Zero_ReturnsOne()
    {
        Assert.Equal(FP.One, FP.Pow2(FP.Zero));
    }

    [Fact]
    public void Pow2_One_ReturnsTwo()
    {
        Assert.Equal(FP.Two, FP.Pow2(FP.One));
    }

    [Fact]
    public void Pow2_Two_ReturnsFour()
    {
        Assert.Equal(FP.FromInt(4), FP.Pow2(FP.FromInt(2)));
    }

    [Fact]
    public void Pow2_NegativeOne_ReturnsHalf()
    {
        FP result = FP.Pow2(FP.MinusOne);
        result.ShouldBeApproximately(FP.Half);
    }

    [Fact]
    public void Pow2_NegativeTwo_ReturnsQuarter()
    {
        FP result = FP.Pow2(FP.FromInt(-2));
        FP expected = FP.One / FP.FromInt(4);
        result.ShouldBeApproximately(expected);
    }

    [Fact]
    public void Pow2_Fractional_ReturnsSqrt2()
    {
        // 2^(0.5) = sqrt(2)
        FP result = FP.Pow2(FP.Half);
        FP sqrt2 = FP.Sqrt(FP.FromInt(2));
        result.ShouldBeApproximately(sqrt2, FP.Epsilon * FP.FromInt(500));
    }

    [Fact]
    public void Pow2_IntegerExponent_ExactResult()
    {
        for (int i = 0; i <= 10; i++)
        {
            FP pow = FP.Pow2(FP.FromInt(i));
            FP expected = FP.FromInt(1 << i);
            Assert.Equal(expected.RawValue, pow.RawValue);
        }
    }

    [Fact]
    public void Pow2_LargePositive_Throws()
    {
        Assert.Throws<OverflowException>(() => FP.Pow2(FP.FromInt(32)));
    }

    // ========================================================================
    // Approximately
    // ========================================================================

    [Fact]
    public void Approximately_EqualValues_ReturnsTrue()
    {
        Assert.True(FP.Approximately(FP.One, FP.One));
    }

    [Fact]
    public void Approximately_VeryClose_ReturnsTrue()
    {
        FP a = FP.One;
        FP b = FP.One + FP.Epsilon;
        Assert.True(FP.Approximately(a, b, FP.Epsilon * FP.FromInt(2)));
    }

    [Fact]
    public void Approximately_Different_ReturnsFalse()
    {
        Assert.False(FP.Approximately(FP.Zero, FP.One));
    }

    // ========================================================================
    // IEquatable / IComparable / Overrides
    // ========================================================================

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        FP a = FP.FromInt(42);
        FP b = FP.FromInt(42);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_Object_Works()
    {
        FP a = FP.FromInt(7);
        object b = FP.FromInt(7);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        FP a = FP.FromInt(1);
        Assert.False(a.Equals("hello"));
    }

    [Fact]
    public void GetHashCode_SameValues_HaveSameHash()
    {
        FP a = FP.FromInt(99);
        FP b = FP.FromInt(99);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void CompareTo_LessThan_ReturnsNegative()
    {
        Assert.True(FP.Zero.CompareTo(FP.One) < 0);
    }

    [Fact]
    public void CompareTo_GreaterThan_ReturnsPositive()
    {
        Assert.True(FP.One.CompareTo(FP.Zero) > 0);
    }

    [Fact]
    public void CompareTo_Equal_ReturnsZero()
    {
        Assert.Equal(0, FP.Half.CompareTo(FP.Half));
    }

    // ========================================================================
    // Edge Cases & Overflow Protection
    // ========================================================================

    [Fact]
    public void MaxValue_Operation_DoesNotThrow()
    {
        // Basic operations on MaxValue should work
        FP halfMax = FP.MaxValue / FP.Two;
        Assert.True(halfMax > FP.Zero);
    }

    [Fact]
    public void Multiplication_Boundary_DoesNotOverflow()
    {
        // sqrt(MaxValue) * sqrt(MaxValue) should ≈ MaxValue (with some error)
        FP sqrtMax = FP.Sqrt(FP.MaxValue);
        FP product = sqrtMax * sqrtMax;
        Assert.True(product.RawValue > 0);
    }

    [Fact]
    public void Division_Fraction_Works()
    {
        // 1 / 7 should be ~0.142857...
        FP seventh = FP.One / FP.FromInt(7);
        FP back = seventh * FP.FromInt(7);
        back.ShouldBeApproximately(FP.One, FP.Epsilon * FP.FromInt(100));
    }

    [Fact]
    public void FromRaw_RoundTrip()
    {
        long raw = 12345678901234L;
        FP value = FP.FromRaw(raw);
        Assert.Equal(raw, value.RawValue);
    }

    [Fact]
    public void Sign_Fraction_Works()
    {
        Assert.Equal(FP.One, FP.Sign(FP.Epsilon));
        Assert.Equal(FP.MinusOne, FP.Sign(-FP.Epsilon));
    }
}
}
