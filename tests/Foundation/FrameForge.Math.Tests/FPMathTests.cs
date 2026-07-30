using FrameForge.Foundation.FixedPoint;
using FrameForge.Foundation.Math;
using Xunit;

namespace FrameForge.Foundation.Math.Tests;

public class FPMathTests
{
    // ========================================================================
    // Constants
    // ========================================================================

    [Fact] public void Constants_PI_IsPositive() => Assert.True(FPMath.PI > FP.Zero);
    [Fact] public void Constants_Deg2Rad_Rad2Deg_Inverse() { (FPMath.Deg2Rad * FPMath.Rad2Deg).ShouldBeApproximately(FP.One); }
    [Fact] public void Constants_Epsilon_IsSmallestPositive() => Assert.True(FPMath.Epsilon > FP.Zero);

    // ========================================================================
    // Abs
    // ========================================================================
    [Fact] public void Abs_Positive_ReturnsSame() => Assert.Equal(FP.FromInt(5), FPMath.Abs(FP.FromInt(5)));
    [Fact] public void Abs_Negative_ReturnsPositive() => Assert.Equal(FP.FromInt(5), FPMath.Abs(FP.FromInt(-5)));
    [Fact] public void Abs_Int_Works() => Assert.Equal(5, FPMath.Abs(-5));

    // ========================================================================
    // Sign
    // ========================================================================
    [Fact] public void Sign_Positive_ReturnsOne() => Assert.Equal(FP.One, FPMath.Sign(FP.FromInt(42)));
    [Fact] public void Sign_Negative_ReturnsMinusOne() => Assert.Equal(FP.MinusOne, FPMath.Sign(FP.FromInt(-42)));
    [Fact] public void Sign_Zero_ReturnsZero() => Assert.Equal(FP.Zero, FPMath.Sign(FP.Zero));
    [Fact] public void Sign_Int_Works() { Assert.Equal(1, FPMath.Sign(5)); Assert.Equal(-1, FPMath.Sign(-5)); Assert.Equal(0, FPMath.Sign(0)); }

    // ========================================================================
    // Min / Max
    // ========================================================================
    [Fact] public void Min_FP_Works() => Assert.Equal(FP.FromInt(3), FPMath.Min(FP.FromInt(3), FP.FromInt(5)));
    [Fact] public void Max_FP_Works() => Assert.Equal(FP.FromInt(5), FPMath.Max(FP.FromInt(3), FP.FromInt(5)));
    [Fact] public void Min_Int_Works() => Assert.Equal(3, FPMath.Min(3, 5));
    [Fact] public void Max_Int_Works() => Assert.Equal(5, FPMath.Max(3, 5));

    // ========================================================================
    // Pow / Sqrt
    // ========================================================================
    [Fact] public void Sqrt_PerfectSquare_Works() => Assert.Equal(FP.FromInt(4), FPMath.Sqrt(FP.FromInt(16)));
    [Fact] public void Pow_TwoToThree_EqualsEight() { var r = FPMath.Pow(FP.FromInt(2), FP.FromInt(3)); r.ShouldBeApproximately(FP.FromInt(8)); }
    [Fact] public void Pow_ZeroToZero_ReturnsOne() => Assert.Equal(FP.One, FPMath.Pow(FP.Zero, FP.Zero));
    [Fact] public void Pow_ZeroToPositive_ReturnsZero() => Assert.Equal(FP.Zero, FPMath.Pow(FP.Zero, FP.One));
    [Fact] public void Pow_ZeroToNegative_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => FPMath.Pow(FP.Zero, FP.MinusOne));

    // ========================================================================
    // Lerp / InverseLerp
    // ========================================================================
    [Fact] public void Lerp_THalf_ReturnsMidpoint() { var r = FPMath.Lerp(FP.Zero, FP.FromInt(10), FP.Half); r.ShouldBeApproximately(FP.FromInt(5)); }
    [Fact] public void Lerp_TClamped() => Assert.Equal(FP.FromInt(10), FPMath.Lerp(FP.Zero, FP.FromInt(10), FP.FromInt(2)));
    [Fact] public void InverseLerp_Midpoint_ReturnsHalf() { var r = FPMath.InverseLerp(FP.Zero, FP.FromInt(10), FP.FromInt(5)); r.ShouldBeApproximately(FP.Half); }
    [Fact] public void InverseLerp_SameBounds_ReturnsZero() => Assert.Equal(FP.Zero, FPMath.InverseLerp(FP.One, FP.One, FP.One));

    // ========================================================================
    // SmoothStep
    // ========================================================================
    [Fact] public void SmoothStep_TZero_ReturnsA() => Assert.Equal(FP.Zero, FPMath.SmoothStep(FP.Zero, FP.One, FP.Zero));
    [Fact] public void SmoothStep_TOne_ReturnsB() => Assert.Equal(FP.One, FPMath.SmoothStep(FP.Zero, FP.One, FP.One));
    [Fact] public void SmoothStep_THalf_ReturnsMidpoint() { var r = FPMath.SmoothStep(FP.Zero, FP.One, FP.Half); r.ShouldBeApproximately(FP.Half); }

    // ========================================================================
    // SmoothDamp
    // ========================================================================
    [Fact]
    public void SmoothDamp_ConvergesToTarget()
    {
        FP current = FP.Zero;
        FP velocity = FP.Zero;
        FP target = FP.FromInt(10);
        FP smoothTime = FP.FromDouble(0.3);
        FP deltaTime = FP.FromDouble(0.02);

        for (int i = 0; i < 100; i++)
        {
            current = FPMath.SmoothDamp(current, target, ref velocity, smoothTime, deltaTime);
        }

        current.ShouldBeApproximately(target, FP.Epsilon * FP.FromInt(2000000));
    }

    [Fact]
    public void SmoothDamp_MaxSpeed_CapsVelocity()
    {
        FP current = FP.Zero;
        FP velocity = FP.Zero;
        FP target = FP.FromInt(100);
        FP smoothTime = FP.FromDouble(0.3);
        FP maxSpeed = FP.FromInt(5);
        FP deltaTime = FP.FromDouble(0.02);

        // After one step with maxSpeed cap, should move at most maxSpeed * deltaTime
        current = FPMath.SmoothDamp(current, target, ref velocity, smoothTime, maxSpeed, deltaTime);
        Assert.True(current <= maxSpeed * deltaTime + FP.Epsilon * FP.FromInt(50000));
    }

    // ========================================================================
    // MoveTowards / MoveTowardsAngle
    // ========================================================================
    [Fact] public void MoveTowards_WithinDelta_ReturnsTarget() => Assert.Equal(FP.FromInt(5), FPMath.MoveTowards(FP.FromInt(3), FP.FromInt(5), FP.FromInt(2)));
    [Fact] public void MoveTowards_Clamped_ReturnsPartial() { var r = FPMath.MoveTowards(FP.FromInt(3), FP.FromInt(10), FP.FromInt(2)); r.ShouldBeApproximately(FP.FromInt(5)); }
    [Fact] public void MoveTowards_NegativeDirection_Works() { var r = FPMath.MoveTowards(FP.FromInt(10), FP.FromInt(3), FP.FromInt(2)); r.ShouldBeApproximately(FP.FromInt(8)); }
    [Fact] public void MoveTowardsAngle_WrapsCorrectly() { var r = FPMath.MoveTowardsAngle(FP.FromInt(350), FP.FromInt(10), FP.FromInt(30)); r.ShouldBeApproximatelyTrig(FP.FromInt(10)); }
    [Fact] public void MoveTowardsAngle_WithinDelta_ReturnsTarget() { var r = FPMath.MoveTowardsAngle(FP.FromInt(10), FP.FromInt(20), FP.FromInt(30)); r.ShouldBeApproximatelyTrig(FP.FromInt(20)); }

    // ========================================================================
    // Clamp / Clamp01
    // ========================================================================
    [Fact] public void Clamp_Within_ReturnsValue() => Assert.Equal(FP.FromInt(5), FPMath.Clamp(FP.FromInt(5), FP.Zero, FP.FromInt(10)));
    [Fact] public void Clamp_Below_ReturnsMin() => Assert.Equal(FP.Zero, FPMath.Clamp(FP.MinusOne, FP.Zero, FP.FromInt(10)));
    [Fact] public void Clamp_Above_ReturnsMax() => Assert.Equal(FP.FromInt(10), FPMath.Clamp(FP.FromInt(15), FP.Zero, FP.FromInt(10)));
    [Fact] public void Clamp_Int_Works() { Assert.Equal(5, FPMath.Clamp(5, 0, 10)); Assert.Equal(0, FPMath.Clamp(-1, 0, 10)); Assert.Equal(10, FPMath.Clamp(15, 0, 10)); }
    [Fact] public void Clamp01_Works() => Assert.Equal(FP.Half, FPMath.Clamp01(FP.Half));

    // ========================================================================
    // Repeat / PingPong
    // ========================================================================
    [Fact] public void Repeat_WithinRange_ReturnsSame() => Assert.Equal(FP.FromInt(3), FPMath.Repeat(FP.FromInt(3), FP.FromInt(10)));
    [Fact] public void Repeat_Overflow_Wraps() { var r = FPMath.Repeat(FP.FromInt(13), FP.FromInt(10)); r.ShouldBeApproximately(FP.FromInt(3)); }
    [Fact] public void Repeat_Zero_ReturnsZero() => Assert.Equal(FP.Zero, FPMath.Repeat(FP.FromInt(5), FP.Zero));
    [Fact] public void PingPong_AtZero_ReturnsZero() => Assert.Equal(FP.Zero, FPMath.PingPong(FP.Zero, FP.FromInt(10)));
    [Fact] public void PingPong_AtLength_ReturnsLength() { var r = FPMath.PingPong(FP.FromInt(10), FP.FromInt(10)); r.ShouldBeApproximately(FP.FromInt(10)); }
    [Fact] public void PingPong_AtDoubleLength_ReturnsZero() { var r = FPMath.PingPong(FP.FromInt(20), FP.FromInt(10)); r.ShouldBeApproximately(FP.Zero); }

    // ========================================================================
    // Trigonometry
    // ========================================================================
    [Fact] public void Sin_Zero_ReturnsZero() => Assert.Equal(FP.Zero, FPMath.Sin(FP.Zero));
    [Fact] public void Cos_Zero_ReturnsOne() => Assert.Equal(FP.One, FPMath.Cos(FP.Zero));
    [Fact] public void SinCos_Identity() { var s = FPMath.Sin(FP.PI / FP.FromInt(4)); var c = FPMath.Cos(FP.PI / FP.FromInt(4)); (s * s + c * c).ShouldBeApproximatelyTrig(FP.One); }
    [Fact] public void Atan2_QuadrantI_Returns45() { var r = FPMath.Atan2(FP.One, FP.One); r.ShouldBeApproximatelyTrig(FP.PI / FP.FromInt(4)); }

    // ========================================================================
    // DeltaAngle
    // ========================================================================
    [Fact] public void DeltaAngle_Same_ReturnsZero() => Assert.Equal(FP.Zero, FPMath.DeltaAngle(FP.FromInt(45), FP.FromInt(45)));
    [Fact] public void DeltaAngle_ShortPath_Works() { var r = FPMath.DeltaAngle(FP.FromInt(350), FP.FromInt(10)); r.ShouldBeApproximatelyTrig(FP.FromInt(20)); }
    [Fact] public void DeltaAngle_Negative_Works() { var r = FPMath.DeltaAngle(FP.FromInt(10), FP.FromInt(350)); r.ShouldBeApproximatelyTrig(FP.FromInt(-20)); }

    // ========================================================================
    // Rounding
    // ========================================================================
    [Fact] public void Floor_Works() => Assert.Equal(FP.FromInt(3), FPMath.Floor(FP.FromInt(3) + FP.Half));
    [Fact] public void FloorToInt_Works() => Assert.Equal(3, FPMath.FloorToInt(FP.FromInt(3) + FP.Half));
    [Fact] public void Ceil_Works() => Assert.Equal(FP.FromInt(4), FPMath.Ceil(FP.FromInt(3) + FP.Half));
    [Fact] public void CeilToInt_Works() => Assert.Equal(4, FPMath.CeilToInt(FP.FromInt(3) + FP.Half));
    [Fact] public void Round_Works() => Assert.Equal(FP.FromInt(4), FPMath.Round(FP.FromInt(3) + FP.Half));
    [Fact] public void RoundToInt_Works() => Assert.Equal(4, FPMath.RoundToInt(FP.FromInt(3) + FP.Half));

    // ========================================================================
    // Approximately
    // ========================================================================
    [Fact] public void Approximately_Equal_ReturnsTrue() => Assert.True(FPMath.Approximately(FP.One, FP.One));
    [Fact] public void Approximately_Different_ReturnsFalse() => Assert.False(FPMath.Approximately(FP.Zero, FP.One));
    [Fact] public void Approximately_WithTolerance_Works() => Assert.True(FPMath.Approximately(FP.One, FP.One + FP.Epsilon, FP.Epsilon * FP.Two));

    // ========================================================================
    // Additional Coverage
    // ========================================================================

    [Fact] public void Pow_NegativeExponent_Works() { var r = FPMath.Pow(FP.FromInt(2), FP.MinusOne); r.ShouldBeApproximately(FP.Half, FP.Epsilon * FP.FromInt(500)); }
    [Fact] public void Pow_FractionalExponent_Works() { var r = FPMath.Pow(FP.FromInt(4), FP.Half); r.ShouldBeApproximately(FP.Two, FP.Epsilon * FP.FromInt(500)); }
    [Fact] public void Tan_Works() { var r = FPMath.Tan(FP.PI / FP.FromInt(4)); r.ShouldBeApproximately(FP.One, FP.Epsilon * FP.FromInt(5000)); }
    [Fact] public void Asin_Acos_Works() { FPMath.Asin(FP.Half).ShouldBeApproximatelyTrig(FP.PI / FP.FromInt(6)); FPMath.Acos(FP.Half).ShouldBeApproximatelyTrig(FP.PI / FP.FromInt(3)); }
    [Fact] public void SmoothStep_BelowRange_ClampsToA() => Assert.Equal(FP.Zero, FPMath.SmoothStep(FP.Zero, FP.One, FP.MinusOne));
    [Fact] public void SmoothStep_AboveRange_ClampsToB() => Assert.Equal(FP.One, FPMath.SmoothStep(FP.Zero, FP.One, FP.Two));
    [Fact] public void InverseLerp_Below_ReturnsZero() => Assert.Equal(FP.Zero, FPMath.InverseLerp(FP.Zero, FP.FromInt(10), FP.MinusOne));
    [Fact] public void InverseLerp_Above_ReturnsOne() => Assert.Equal(FP.One, FPMath.InverseLerp(FP.Zero, FP.FromInt(10), FP.FromInt(20)));
    [Fact] public void MoveTowards_AlreadyAtTarget_ReturnsTarget() => Assert.Equal(FP.FromInt(5), FPMath.MoveTowards(FP.FromInt(5), FP.FromInt(5), FP.One));
    [Fact] public void SmoothDamp_NoMaxSpeed_Overload_Works()
    {
        FP current = FP.Zero; FP velocity = FP.Zero;
        current = FPMath.SmoothDamp(current, FP.FromInt(5), ref velocity, FP.FromDouble(0.3), FP.FromDouble(0.02));
        Assert.True(current > FP.Zero);
    }
    [Fact] public void PingPong_ZeroLength_ReturnsZero() => Assert.Equal(FP.Zero, FPMath.PingPong(FP.FromInt(5), FP.Zero));
    [Fact] public void PingPong_Midpoint_ReturnsHalfLength() { var r = FPMath.PingPong(FP.FromInt(5), FP.FromInt(10)); r.ShouldBeApproximately(FP.FromInt(5)); }
    [Fact] public void Repeat_NegativeValue_Wraps() { var r = FPMath.Repeat(FP.FromInt(-3), FP.FromInt(10)); r.ShouldBeApproximately(FP.FromInt(7)); }
    [Fact] public void MoveTowardsAngle_NegativeDirection_Works() { var r = FPMath.MoveTowardsAngle(FP.FromInt(10), FP.FromInt(350), FP.FromInt(30)); r.ShouldBeApproximatelyTrig(FP.FromInt(350)); }
    [Fact] public void Floor_Negative_Works() => Assert.Equal(FP.FromInt(-4), FPMath.Floor(-(FP.FromInt(3) + FP.Half)));

    // ========================================================================
    // Coverage: More edge cases
    // ========================================================================

    [Fact] public void Tan_Near90_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => FPMath.Tan(FP.PI / FP.Two));
    [Fact] public void Asin_OutOfRange_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => FPMath.Asin(FP.FromInt(2)));
    [Fact] public void Acos_OutOfRange_Throws() => Assert.Throws<ArgumentOutOfRangeException>(() => FPMath.Acos(FP.FromInt(2)));
    [Fact] public void Pow_IntegerExponent_Works() { var r = FPMath.Pow(FP.FromInt(3), FP.FromInt(3)); r.ShouldBeApproximately(FP.FromInt(27), FP.Epsilon * FP.FromInt(5000)); }
    [Fact] public void SmoothDamp_MinSmoothTime_Clamped()
    {
        FP current = FP.Zero; FP velocity = FP.Zero;
        current = FPMath.SmoothDamp(current, FP.FromInt(100), ref velocity, FP.Zero, FP.FromDouble(0.02));
        Assert.True(current > FP.Zero);
    }
    [Fact] public void DeltaAngle_180Degrees_Returns180() { var r = FPMath.DeltaAngle(FP.Zero, FP.FromInt(180)); r.ShouldBeApproximatelyTrig(FP.FromInt(180)); }
    [Fact] public void MoveTowardsAngle_Same_ReturnsSame() => Assert.Equal(FP.FromInt(45), FPMath.MoveTowardsAngle(FP.FromInt(45), FP.FromInt(45), FP.One));
    [Fact] public void PingPong_HalfPeriod_Works() { var r = FPMath.PingPong(FP.FromInt(15), FP.FromInt(10)); r.ShouldBeApproximately(FP.FromInt(5)); }
    [Fact] public void Repeat_ExactMultiple_ReturnsZero() => Assert.Equal(FP.Zero, FPMath.Repeat(FP.FromInt(20), FP.FromInt(10)));
    [Fact] public void CeilToInt_Negative_Works() => Assert.Equal(-3, FPMath.CeilToInt(-(FP.FromInt(3) + FP.Half)));
    [Fact] public void RoundToInt_Negative_Works() => Assert.Equal(-4, FPMath.RoundToInt(-(FP.FromInt(3) + FP.Half)));
    [Fact] public void FloorToInt_Negative_Works() => Assert.Equal(-4, FPMath.FloorToInt(-(FP.FromInt(3) + FP.Half)));
}
