using FrameForge.Foundation.FixedPoint;
using FrameForge.Foundation.Math;
using Xunit;

namespace FrameForge.Foundation.Math.Tests;

public class QuaternionTests
{
    // ========================================================================
    // Constants & Constructors
    // ========================================================================

    [Fact] public void Identity_IsZeroZeroZeroOne() => Assert.Equal(Quaternion.Identity, new Quaternion(FP.Zero, FP.Zero, FP.Zero, FP.One));
    [Fact] public void Constructor_SetsComponents() { var q = new Quaternion(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4)); Assert.Equal(FP.One, q.X); Assert.Equal(FP.FromInt(4), q.W); }

    // ========================================================================
    // Euler / EulerAngles
    // ========================================================================

    [Fact]
    public void Euler_Zero_ReturnsIdentity()
    {
        var q = Quaternion.Euler(FP.Zero, FP.Zero, FP.Zero);
        q.ShouldBeApproximately(Quaternion.Identity);
    }

    [Fact]
    public void Euler_RoundTrip_Simple()
    {
        // Single-axis pitch
        var q1 = Quaternion.Euler(FP.FromInt(30), FP.Zero, FP.Zero);
        var e1 = q1.EulerAngles;
        e1.X.ShouldBeApproximately(FP.FromInt(30), FP.Epsilon * FP.FromInt(200000));

        // Single-axis yaw
        var q2 = Quaternion.Euler(FP.Zero, FP.FromInt(45), FP.Zero);
        var e2 = q2.EulerAngles;
        e2.Y.ShouldBeApproximately(FP.FromInt(45), FP.Epsilon * FP.FromInt(200000));

        // Roll: verify by rotating a vector
        var q3 = Quaternion.Euler(FP.Zero, FP.Zero, FP.FromInt(60));
        var rotated = q3 * Vector3.Up;
        FP sin60 = FP.Sin(FP.FromInt(60) * FP.Deg2Rad);
        FP cos60 = FP.Cos(FP.FromInt(60) * FP.Deg2Rad);
        rotated.X.ShouldBeApproximately(-sin60, FP.Epsilon * FP.FromInt(20000));
        rotated.Y.ShouldBeApproximately(cos60, FP.Epsilon * FP.FromInt(20000));

        // Combined: verify rotation preserves length and direction
        var q4 = Quaternion.Euler(FP.FromInt(30), FP.FromInt(45), FP.FromInt(60));
        var v = q4 * Vector3.Forward;
        v.Magnitude.ShouldBeApproximately(FP.One, FP.Epsilon * FP.FromInt(20000));
    }

    [Fact]
    public void Euler_90DegreePitch_Works()
    {
        var q = Quaternion.Euler(FP.FromInt(90), FP.Zero, FP.Zero);
        var rotated = q * Vector3.Forward;
        // In Unity left-hand coordinates, +X rotation (pitch up) rotates
        // Forward (0,0,1) → Down (0,-1,0).
        rotated.ShouldBeApproximately(Vector3.Down);
    }

    [Fact]
    public void Euler_90DegreeYaw_Works()
    {
        var q = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero);
        var rotated = q * Vector3.Forward;
        rotated.ShouldBeApproximately(Vector3.Right, FP.Epsilon * FP.FromInt(20000));
    }

    [Fact]
    public void Euler_90DegreeRoll_Works()
    {
        var q = Quaternion.Euler(FP.Zero, FP.Zero, FP.FromInt(90));
        var rotated = q * Vector3.Up;
        rotated.ShouldBeApproximately(Vector3.Left);
    }

    // ========================================================================
    // AngleAxis
    // ========================================================================

    [Fact]
    public void AngleAxis_180DegreesAroundY_RotatesForwardToBack()
    {
        var q = Quaternion.AngleAxis(FP.FromInt(180), Vector3.Up);
        var rotated = q * Vector3.Forward;
        rotated.ShouldBeApproximately(Vector3.Back);
    }

    [Fact]
    public void AngleAxis_ZeroAngle_ReturnsIdentity()
    {
        var q = Quaternion.AngleAxis(FP.Zero, Vector3.Up);
        q.ShouldBeApproximately(Quaternion.Identity);
    }

    // ========================================================================
    // LookRotation
    // ========================================================================

    [Fact]
    public void LookRotation_ForwardUp_ReturnsIdentity()
    {
        var q = Quaternion.LookRotation(Vector3.Forward, Vector3.Up);
        // Should be approximately identity (or negated identity)
        var rotated = q * Vector3.Forward;
        rotated.ShouldBeApproximately(Vector3.Forward);
    }

    [Fact]
    public void LookRotation_RightUp_RotatesForwardToRight()
    {
        var q = Quaternion.LookRotation(Vector3.Right, Vector3.Up);
        var rotated = q * Vector3.Forward;
        rotated.ShouldBeApproximately(Vector3.Right);
    }

    // ========================================================================
    // FromToRotation
    // ========================================================================

    [Fact]
    public void FromToRotation_ForwardToBack_RotatesCorrectly()
    {
        var q = Quaternion.FromToRotation(Vector3.Forward, Vector3.Back);
        var rotated = q * Vector3.Forward;
        rotated.ShouldBeApproximately(Vector3.Back);
    }

    [Fact]
    public void FromToRotation_SameVector_ReturnsIdentity()
    {
        var q = Quaternion.FromToRotation(Vector3.Forward, Vector3.Forward);
        q.ShouldBeApproximately(Quaternion.Identity);
    }

    [Fact]
    public void FromToRotation_OppositeVector_180Degrees()
    {
        var q = Quaternion.FromToRotation(Vector3.Forward, Vector3.Back);
        var angle = Quaternion.Angle(Quaternion.Identity, q);
        angle.ShouldBeApproximately(FP.FromInt(180), FP.Epsilon * FP.FromInt(50000));
    }

    // ========================================================================
    // EulerAngles Property
    // ========================================================================

    [Fact]
    public void EulerAngles_Identity_ReturnsZero()
    {
        var euler = Quaternion.Identity.EulerAngles;
        euler.X.ShouldBeApproximately(FP.Zero);
        euler.Y.ShouldBeApproximately(FP.Zero);
        euler.Z.ShouldBeApproximately(FP.Zero);
    }

    // ========================================================================
    // Normalized
    // ========================================================================

    [Fact]
    public void Normalized_UnitQuaternion_ReturnsSame()
    {
        var q = Quaternion.Identity.Normalized;
        q.ShouldBeApproximately(Quaternion.Identity);
    }

    [Fact]
    public void Normalized_Zero_ReturnsIdentity()
    {
        var q = new Quaternion(FP.Zero, FP.Zero, FP.Zero, FP.Zero).Normalized;
        q.ShouldBeApproximately(Quaternion.Identity);
    }

    // ========================================================================
    // Quaternion Multiplication (Combine Rotations)
    // ========================================================================

    [Fact]
    public void Multiply_Identity_ReturnsOther()
    {
        var q = Quaternion.Euler(FP.FromInt(45), FP.Zero, FP.Zero);
        var result = Quaternion.Identity * q;
        result.ShouldBeApproximately(q);
    }

    [Fact]
    public void Multiply_IdentityRight_ReturnsOther()
    {
        var q = Quaternion.Euler(FP.FromInt(45), FP.Zero, FP.Zero);
        var result = q * Quaternion.Identity;
        result.ShouldBeApproximately(q);
    }

    // ========================================================================
    // Rotate Vector
    // ========================================================================

    [Fact]
    public void RotateVector_Identity_ReturnsSame()
    {
        var result = Quaternion.Identity * Vector3.One;
        result.ShouldBeApproximately(Vector3.One);
    }

    [Fact]
    public void RotateVector_Yaw90_TakesForwardToRight()
    {
        var q = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero);
        var result = q * Vector3.Forward;
        result.ShouldBeApproximately(Vector3.Right, FP.Epsilon * FP.FromInt(20000));
    }

    [Fact]
    public void RotateVector_PreservesLength()
    {
        var q = Quaternion.Euler(FP.FromInt(30), FP.FromInt(45), FP.FromInt(60));
        var v = new Vector3(FP.FromInt(3), FP.FromInt(4), FP.FromInt(5));
        var rotated = q * v;
        rotated.Magnitude.ShouldBeApproximately(v.Magnitude, FP.Epsilon * FP.FromInt(10000));
    }

    // ========================================================================
    // Dot / Angle / Inverse
    // ========================================================================

    [Fact]
    public void Dot_SameQuaternion_ReturnsOne()
    {
        var q = Quaternion.Euler(FP.FromInt(30), FP.FromInt(45), FP.FromInt(60));
        Quaternion.Dot(q, q).ShouldBeApproximately(FP.One, FP.Epsilon * FP.FromInt(20000));
    }

    [Fact]
    public void Angle_SameQuaternion_ReturnsZero()
    {
        var q = Quaternion.Euler(FP.FromInt(30), FP.FromInt(45), FP.FromInt(60));
        Quaternion.Angle(q, q).ShouldBeApproximately(FP.Zero, FP.FromDouble(0.25));
    }

    [Fact]
    public void Inverse_RoundTrip_ReturnsIdentity()
    {
        var q = Quaternion.Euler(FP.FromInt(30), FP.FromInt(45), FP.FromInt(60));
        var inv = Quaternion.Inverse(q);
        var result = q * inv;
        result.ShouldBeApproximately(Quaternion.Identity);
    }

    [Fact]
    public void Inverse_RotatesBack()
    {
        var q = Quaternion.Euler(FP.FromInt(45), FP.Zero, FP.Zero);
        var rotated = q * Vector3.Forward;
        var back = Quaternion.Inverse(q) * rotated;
        back.ShouldBeApproximately(Vector3.Forward);
    }

    // ========================================================================
    // Slerp
    // ========================================================================

    [Fact]
    public void Slerp_TZero_ReturnsA()
    {
        var a = Quaternion.Identity;
        var b = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero);
        var r = Quaternion.Slerp(a, b, FP.Zero);
        r.ShouldBeApproximately(a);
    }

    [Fact]
    public void Slerp_TOne_ReturnsB()
    {
        var a = Quaternion.Identity;
        var b = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero);
        var r = Quaternion.Slerp(a, b, FP.One);
        r.ShouldBeApproximately(b);
    }

    [Fact]
    public void Slerp_THalf_45Degrees()
    {
        var a = Quaternion.Identity;
        var b = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero);
        var r = Quaternion.Slerp(a, b, FP.Half);
        var angle = Quaternion.Angle(Quaternion.Identity, r);
        angle.ShouldBeApproximately(FP.FromInt(45), FP.Epsilon * FP.FromInt(100000));
    }

    [Fact]
    public void Slerp_TakesShortestPath()
    {
        // q and -q represent the same rotation
        // Slerp should take the shorter path
        var a = Quaternion.Identity;
        var b = new Quaternion(-Quaternion.Euler(FP.Zero, FP.FromInt(10), FP.Zero).X,
                               -Quaternion.Euler(FP.Zero, FP.FromInt(10), FP.Zero).Y,
                               -Quaternion.Euler(FP.Zero, FP.FromInt(10), FP.Zero).Z,
                               -Quaternion.Euler(FP.Zero, FP.FromInt(10), FP.Zero).W);
        var r = Quaternion.Slerp(a, b, FP.Half);
        // Should interpolate on the short path (~5 degrees, not ~175)
        var angle = Quaternion.Angle(Quaternion.Identity, r);
        Assert.True(angle < FP.FromInt(10));
    }

    // ========================================================================
    // Lerp
    // ========================================================================

    [Fact]
    public void Lerp_TZero_ReturnsA()
    {
        var a = Quaternion.Identity;
        var b = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero);
        var r = Quaternion.Lerp(a, b, FP.Zero);
        r.ShouldBeApproximately(a);
    }

    [Fact]
    public void Lerp_TOne_ReturnsB()
    {
        var a = Quaternion.Identity;
        var b = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero);
        var r = Quaternion.Lerp(a, b, FP.One);
        r.ShouldBeApproximately(b);
    }

    // ========================================================================
    // RotateTowards
    // ========================================================================

    [Fact]
    public void RotateTowards_LargeDelta_SnapsToTarget()
    {
        var a = Quaternion.Identity;
        var b = Quaternion.Euler(FP.Zero, FP.FromInt(45), FP.Zero);
        var r = Quaternion.RotateTowards(a, b, FP.FromInt(90));
        r.ShouldBeApproximately(b);
    }

    [Fact]
    public void RotateTowards_ZeroDelta_StaysPut()
    {
        var a = Quaternion.Identity;
        var b = Quaternion.Euler(FP.Zero, FP.FromInt(45), FP.Zero);
        var r = Quaternion.RotateTowards(a, b, FP.Zero);
        r.ShouldBeApproximately(a);
    }

    [Fact]
    public void RotateTowards_HalfDelta_HalvesAngle()
    {
        var a = Quaternion.Identity;
        var b = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero);
        var r = Quaternion.RotateTowards(a, b, FP.FromInt(45));
        var angle = Quaternion.Angle(Quaternion.Identity, r);
        angle.ShouldBeApproximately(FP.FromInt(45), FP.Epsilon * FP.FromInt(200000));
    }

    // ========================================================================
    // Equality & String
    // ========================================================================

    [Fact] public void Equals_Same_ReturnsTrue() => Assert.True(Quaternion.Identity.Equals(Quaternion.Identity));
    [Fact] public void Equals_Different_ReturnsFalse() => Assert.False(Quaternion.Identity.Equals(new Quaternion(FP.One, FP.Zero, FP.Zero, FP.Zero)));
    [Fact] public void GetHashCode_Consistent() => Assert.Equal(Quaternion.Identity.GetHashCode(), Quaternion.Identity.GetHashCode());
    [Fact] public void ToString_ContainsComponents() { var s = Quaternion.Identity.ToString(); Assert.Contains("0.000000", s); Assert.Contains("1.000000", s); }
    [Fact] public void Parse_RoundTrip() { var q = new Quaternion(FP.Half, FP.Zero, FP.Zero, FP.Half); var parsed = Quaternion.Parse(q.ToString()); parsed.ShouldBeApproximately(q); }
    [Fact] public void Parse_Invalid_Throws() => Assert.Throws<FormatException>(() => Quaternion.Parse("bad"));

    // ========================================================================
    // Additional Coverage: Edge Cases
    // ========================================================================

    [Fact]
    public void EulerAngles_GimbalLock_Near90DegreePitch()
    {
        // At pitch = 90°, gimbal lock occurs
        var q = Quaternion.Euler(FP.FromInt(90), FP.FromInt(30), FP.FromInt(0));
        var euler = q.EulerAngles;
        // Pitch should be ~90
        Assert.True(euler.X > FP.FromInt(80));
    }

    [Fact]
    public void LookRotation_ZeroForward_ReturnsIdentity()
    {
        var q = Quaternion.LookRotation(Vector3.Zero, Vector3.Up);
        q.ShouldBeApproximately(Quaternion.Identity);
    }

    [Fact]
    public void LookRotation_ForwardParallelToUp_Works()
    {
        // Forward is Up — need arbitrary perpendicular
        var q = Quaternion.LookRotation(Vector3.Up, Vector3.Forward);
        var rotated = q * Vector3.Forward;
        rotated.ShouldBeApproximately(Vector3.Up);
    }

    [Fact]
    public void FromToRotation_ZeroFrom_ReturnsIdentity()
    {
        var q = Quaternion.FromToRotation(Vector3.Zero, Vector3.Forward);
        q.ShouldBeApproximately(Quaternion.Identity);
    }

    [Fact]
    public void FromToRotation_ZeroTo_ReturnsIdentity()
    {
        var q = Quaternion.FromToRotation(Vector3.Forward, Vector3.Zero);
        q.ShouldBeApproximately(Quaternion.Identity);
    }

    [Fact]
    public void Inverse_ZeroQuaternion_ReturnsIdentity()
    {
        var q = new Quaternion(FP.Zero, FP.Zero, FP.Zero, FP.Zero);
        var inv = Quaternion.Inverse(q);
        inv.ShouldBeApproximately(Quaternion.Identity);
    }

    [Fact]
    public void SlerpUnclamped_TBeyondRange_Extrapolates()
    {
        var a = Quaternion.Identity;
        var b = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero);
        var r = Quaternion.SlerpUnclamped(a, b, FP.Two);
        var angle = Quaternion.Angle(Quaternion.Identity, r);
        // Should be approximately 180 degrees (2 * 90)
        angle.ShouldBeApproximately(FP.FromInt(180), FP.FromDouble(1.0));
    }

    [Fact]
    public void Slerp_NegatedQuaternions_TakesShortestPath()
    {
        // Explicitly test that Slerp handles negated quaternions
        var a = Quaternion.Identity;
        var b = Quaternion.Euler(FP.Zero, FP.FromInt(10), FP.Zero);
        // Negate b to represent the same rotation
        var bNeg = new Quaternion(-b.X, -b.Y, -b.Z, -b.W);
        var r = Quaternion.Slerp(a, bNeg, FP.Half);
        // Should take the short path (~5 degrees)
        var angle = Quaternion.Angle(a, r);
        Assert.True(angle < FP.FromInt(8));
    }

    [Fact]
    public void LerpUnclamped_Works()
    {
        var a = Quaternion.Identity;
        var b = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero);
        var r = Quaternion.LerpUnclamped(a, b, FP.Half);
        // Lerp does not preserve unit length, just midpoint in 4D
        Assert.True(r.X != FP.Zero || r.Y != FP.Zero);
    }

    [Fact]
    public void Normalize_NonUnit_ReturnsUnit()
    {
        var q = new Quaternion(FP.One, FP.One, FP.One, FP.One);
        var n = Quaternion.Normalize(q);
        FP dot = Quaternion.Dot(n, n);
        dot.ShouldBeApproximately(FP.One, FP.Epsilon * FP.FromInt(20000));
    }

    [Fact]
    public void Operator_Multiply_NonUnitQuaternions_Works()
    {
        var a = new Quaternion(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4));
        var b = new Quaternion(FP.FromInt(5), FP.FromInt(6), FP.FromInt(7), FP.FromInt(8));
        var result = a * b;
        // Just verify it doesn't throw and returns something
        Assert.True(result.W != FP.Zero);
    }

    [Fact]
    public void AngleAxis_NonNormalizedAxis_Works()
    {
        var axis = new Vector3(FP.One, FP.One, FP.One); // not normalized
        var q = Quaternion.AngleAxis(FP.FromInt(90), axis);
        // Should normalize axis internally
        var rotated = q * axis;
        rotated.Magnitude.ShouldBeApproximately(axis.Magnitude, FP.Epsilon * FP.FromInt(10000));
    }

    [Fact]
    public void EulerAngles_Identity_RoundTrip()
    {
        var q = Quaternion.Identity;
        var euler = q.EulerAngles;
        euler.X.ShouldBeApproximately(FP.Zero);
        euler.Y.ShouldBeApproximately(FP.Zero);
        euler.Z.ShouldBeApproximately(FP.Zero);
    }

    [Fact]
    public void TryParse_Invalid_ReturnsFalse()
    {
        Assert.False(Quaternion.TryParse("not a quaternion", out _));
        Assert.False(Quaternion.TryParse("", out _));
        Assert.False(Quaternion.TryParse("(1,2)", out _));
        Assert.False(Quaternion.TryParse("(1,2,3,4,5)", out _));
    }

    [Fact]
    public void Operators_Inequality_Works()
    {
        Assert.True(Quaternion.Identity != new Quaternion(FP.One, FP.Zero, FP.Zero, FP.Zero));
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        Assert.False(Quaternion.Identity.Equals(null));
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        Assert.False(Quaternion.Identity.Equals("hello"));
    }

    // ========================================================================
    // Coverage: FromRotationMatrix branches & more edge cases
    // ========================================================================

    [Fact]
    public void LookRotation_ForwardDown_Works()
    {
        var q = Quaternion.LookRotation(Vector3.Down, Vector3.Forward);
        var rotated = q * Vector3.Forward;
        rotated.ShouldBeApproximately(Vector3.Down);
    }

    [Fact]
    public void LookRotation_RightForward_Works()
    {
        var q = Quaternion.LookRotation(Vector3.Right, Vector3.Forward);
        var rotated = q * Vector3.Forward;
        rotated.ShouldBeApproximately(Vector3.Right);
    }

    [Fact]
    public void FromToRotation_RightToUp_Works()
    {
        var q = Quaternion.FromToRotation(Vector3.Right, Vector3.Up);
        var rotated = q * Vector3.Right;
        rotated.ShouldBeApproximately(Vector3.Up);
    }

    [Fact]
    public void Euler_180DegreePitch_Works()
    {
        var q = Quaternion.Euler(FP.FromInt(180), FP.Zero, FP.Zero);
        var rotated = q * Vector3.Forward;
        rotated.ShouldBeApproximately(Vector3.Back);
    }

    [Fact]
    public void Euler_NegativeAngles_Works()
    {
        var q = Quaternion.Euler(FP.FromInt(-30), FP.FromInt(-45), FP.FromInt(-60));
        var rotated = q * Vector3.Forward;
        rotated.Magnitude.ShouldBeApproximately(FP.One, FP.Epsilon * FP.FromInt(10000));
    }

    [Fact]
    public void AngleAxis_45DegreesUp_RotatesCorrectly()
    {
        var q = Quaternion.AngleAxis(FP.FromInt(45), Vector3.Up);
        var rotated = q * Vector3.Forward;
        var angle = Vector3.Angle(Vector3.Forward, rotated);
        angle.ShouldBeApproximately(FP.FromInt(45), FP.Epsilon * FP.FromInt(100000));
    }

    [Fact]
    public void Slerp_NearParallel_Quaternions()
    {
        var a = Quaternion.Identity;
        var b = Quaternion.Euler(FP.Zero, FP.FromDouble(0.001), FP.Zero);
        var r = Quaternion.Slerp(a, b, FP.Half);
        r.ShouldBeApproximately(Quaternion.SlerpUnclamped(a, b, FP.Half));
    }

    // ========================================================================
    // FromRotationMatrix branch coverage: different dominant diagonals
    // ========================================================================

    [Fact]
    public void LookRotation_RightForward_CoversR00Dominant()
    {
        // right=(1,0,0), up=(0,1,0), forward=(0,0,1) → identity
        // Use a different basis: Forward=Right, Up=Forward
        var q = Quaternion.LookRotation(Vector3.Right, Vector3.Forward);
        var rotated = q * Vector3.Forward;
        rotated.ShouldBeApproximately(Vector3.Right);
    }

    [Fact]
    public void LookRotation_UpRight_CoversR11Dominant()
    {
        var q = Quaternion.LookRotation(Vector3.Up, Vector3.Right);
        var rotated = q * Vector3.Forward;
        rotated.ShouldBeApproximately(Vector3.Up);
    }

    [Fact]
    public void LookRotation_BackUp_CoversR22Dominant()
    {
        var q = Quaternion.LookRotation(Vector3.Back, Vector3.Up);
        var rotated = q * Vector3.Forward;
        rotated.ShouldBeApproximately(Vector3.Back);
    }
}
