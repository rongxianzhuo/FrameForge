using FrameForge.Foundation.FixedPoint;
using FrameForge.Foundation.Math;
using Xunit;

namespace FrameForge.Foundation.Math.Tests;

public class Vector3Tests
{
    [Fact] public void Constants_Zero_IsZero() => Assert.Equal(Vector3.Zero, new Vector3(FP.Zero, FP.Zero, FP.Zero));
    [Fact] public void Constants_Forward_IsZ() => Assert.Equal(Vector3.Forward, new Vector3(FP.Zero, FP.Zero, FP.One));
    [Fact] public void Constants_Back_IsNegZ() => Assert.Equal(Vector3.Back, new Vector3(FP.Zero, FP.Zero, FP.MinusOne));
    [Fact] public void Constants_Up_IsY() => Assert.Equal(Vector3.Up, new Vector3(FP.Zero, FP.One, FP.Zero));
    [Fact] public void Constants_Down_IsNegY() => Assert.Equal(Vector3.Down, new Vector3(FP.Zero, FP.MinusOne, FP.Zero));
    [Fact] public void Constants_Right_IsX() => Assert.Equal(Vector3.Right, new Vector3(FP.One, FP.Zero, FP.Zero));
    [Fact] public void Constants_Left_IsNegX() => Assert.Equal(Vector3.Left, new Vector3(FP.MinusOne, FP.Zero, FP.Zero));

    [Fact] public void Constructor_SetsComponents() { var v = new Vector3(FP.One, FP.Two, FP.FromInt(3)); Assert.Equal(FP.One, v.X); Assert.Equal(FP.Two, v.Y); Assert.Equal(FP.FromInt(3), v.Z); }
    [Fact] public void Magnitude_345Triangle_Returns5() { var v = new Vector3(FP.FromInt(3), FP.FromInt(4), FP.Zero); v.Magnitude.ShouldBeApproximately(FP.FromInt(5)); }
    [Fact] public void SqrMagnitude_Works() { var v = new Vector3(FP.FromInt(2), FP.FromInt(3), FP.FromInt(6)); Assert.Equal(FP.FromInt(49), v.SqrMagnitude); }
    [Fact] public void Normalized_LengthIsOne() { var v = new Vector3(FP.FromInt(3), FP.FromInt(4), FP.FromInt(5)).Normalized; v.Magnitude.ShouldBeApproximately(FP.One); }
    [Fact] public void Normalized_Zero_ReturnsZero() => Assert.Equal(Vector3.Zero, Vector3.Zero.Normalized);

    [Fact] public void Add_Works() => Assert.Equal(new Vector3(FP.FromInt(3), FP.FromInt(5), FP.FromInt(7)), new Vector3(FP.One, FP.Two, FP.FromInt(3)) + new Vector3(FP.Two, FP.FromInt(3), FP.FromInt(4)));
    [Fact] public void Subtract_Works() => Assert.Equal(new Vector3(FP.One, FP.One, FP.One), new Vector3(FP.FromInt(3), FP.FromInt(4), FP.FromInt(5)) - new Vector3(FP.Two, FP.FromInt(3), FP.FromInt(4)));
    [Fact] public void Negate_Works() => Assert.Equal(new Vector3(FP.MinusOne, FP.One, FP.FromInt(-3)), -new Vector3(FP.One, FP.MinusOne, FP.FromInt(3)));
    [Fact] public void Multiply_Scalar_Works() => Assert.Equal(new Vector3(FP.Two, FP.FromInt(4), FP.FromInt(6)), new Vector3(FP.One, FP.Two, FP.FromInt(3)) * FP.Two);
    [Fact] public void Multiply_ScalarLeft_Works() => Assert.Equal(new Vector3(FP.FromInt(3), FP.FromInt(6), FP.FromInt(9)), FP.FromInt(3) * new Vector3(FP.One, FP.Two, FP.FromInt(3)));
    [Fact] public void Divide_Scalar_Works() { var r = new Vector3(FP.Two, FP.FromInt(4), FP.FromInt(6)) / FP.Two; r.ShouldBeApproximately(new Vector3(FP.One, FP.Two, FP.FromInt(3))); }
    [Fact] public void Equality_Works() { Assert.True(new Vector3(FP.One, FP.Two, FP.FromInt(3)) == new Vector3(FP.One, FP.Two, FP.FromInt(3))); Assert.False(new Vector3(FP.One, FP.Two, FP.FromInt(3)) == new Vector3(FP.One, FP.Two, FP.FromInt(4))); }

    [Fact] public void Dot_Perpendicular_ReturnsZero() => Assert.Equal(FP.Zero, Vector3.Right.Dot(Vector3.Up));
    [Fact] public void Dot_Static_MatchesInstance() { var a = new Vector3(FP.One, FP.Two, FP.FromInt(3)); var b = new Vector3(FP.FromInt(4), FP.FromInt(5), FP.FromInt(6)); Assert.Equal(a.Dot(b), Vector3.Dot(a, b)); }

    // ========================================================================
    // Cross Product
    // ========================================================================

    [Fact] public void Cross_RightUp_EqualsForward() { var result = Vector3.Cross(Vector3.Right, Vector3.Up); result.ShouldBeApproximately(Vector3.Forward); }
    [Fact] public void Cross_UpForward_EqualsRight() { var result = Vector3.Cross(Vector3.Up, Vector3.Forward); result.ShouldBeApproximately(Vector3.Right); }
    [Fact] public void Cross_Parallel_ReturnsZero() { var result = Vector3.Cross(Vector3.Right, Vector3.Right); result.ShouldBeApproximately(Vector3.Zero); }
    [Fact] public void Cross_AntiCommutative() { var a = new Vector3(FP.One, FP.Two, FP.FromInt(3)); var b = new Vector3(FP.FromInt(4), FP.FromInt(5), FP.FromInt(6)); var ab = Vector3.Cross(a, b); var ba = Vector3.Cross(b, a); (ab + ba).ShouldBeApproximately(Vector3.Zero); }

    // ========================================================================
    // Distance
    // ========================================================================

    [Fact] public void Distance_Works() { var d = Vector3.Distance(Vector3.Zero, new Vector3(FP.FromInt(3), FP.FromInt(4), FP.Zero)); d.ShouldBeApproximately(FP.FromInt(5)); }
    [Fact] public void SqrDistance_Works() => Assert.Equal(FP.FromInt(25), Vector3.SqrDistance(Vector3.Zero, new Vector3(FP.FromInt(3), FP.FromInt(4), FP.Zero)));

    // ========================================================================
    // Angle
    // ========================================================================

    [Fact] public void Angle_RightAngle_Returns90() { var a = Vector3.Angle(Vector3.Right, Vector3.Up); a.ShouldBeApproximatelyTrig(FP.FromInt(90)); }
    [Fact] public void Angle_Opposite_Returns180() { var a = Vector3.Angle(Vector3.Forward, Vector3.Back); a.ShouldBeApproximatelyTrig(FP.FromInt(180)); }

    // ========================================================================
    // Interpolation
    // ========================================================================

    [Fact] public void Lerp_THalf_ReturnsMidpoint() { var r = Vector3.Lerp(Vector3.Zero, Vector3.One, FP.Half); r.ShouldBeApproximately(new Vector3(FP.Half, FP.Half, FP.Half)); }
    [Fact] public void LerpUnclamped_Extrapolates() { var r = Vector3.LerpUnclamped(Vector3.Zero, Vector3.One, FP.Two); r.ShouldBeApproximately(new Vector3(FP.Two, FP.Two, FP.Two)); }

    // ========================================================================
    // MoveTowards
    // ========================================================================

    [Fact] public void MoveTowards_Clamped_ReturnsPartial() { var r = Vector3.MoveTowards(Vector3.Zero, Vector3.Right, FP.Half); r.X.ShouldBeApproximately(FP.Half); }

    // ========================================================================
    // Reflect
    // ========================================================================

    [Fact] public void Reflect_AgainstUp_FlipsY() { var r = Vector3.Reflect(new Vector3(FP.One, FP.MinusOne, FP.Zero), Vector3.Up); r.Y.ShouldBeApproximately(FP.One); }

    // ========================================================================
    // Project / ProjectOnPlane
    // ========================================================================

    [Fact] public void Project_OntoXAxis_ReturnsXComponent() { var r = Vector3.Project(new Vector3(FP.FromInt(3), FP.FromInt(4), FP.FromInt(5)), Vector3.Right); r.ShouldBeApproximately(new Vector3(FP.FromInt(3), FP.Zero, FP.Zero)); }
    [Fact] public void Project_ZeroNormal_ReturnsZero() { Assert.Equal(Vector3.Zero, Vector3.Project(Vector3.One, Vector3.Zero)); }
    [Fact] public void ProjectOnPlane_RemovesNormalComponent() { var r = Vector3.ProjectOnPlane(new Vector3(FP.FromInt(3), FP.FromInt(4), FP.Zero), Vector3.Up); r.Y.ShouldBeApproximately(FP.Zero); }

    // ========================================================================
    // Slerp
    // ========================================================================

    [Fact] public void Slerp_TZero_ReturnsA() { var a = Vector3.Right; var b = Vector3.Up; var r = Vector3.Slerp(a, b, FP.Zero); r.ShouldBeApproximately(a); }
    [Fact] public void Slerp_TOne_ReturnsB() { var a = Vector3.Right; var b = Vector3.Up; var r = Vector3.Slerp(a, b, FP.One); r.ShouldBeApproximately(b); }
    [Fact] public void Slerp_THalf_45Degrees() { var a = Vector3.Right; var b = Vector3.Forward; var r = Vector3.Slerp(a, b, FP.Half); var angle = Vector3.Angle(Vector3.Right, r); angle.ShouldBeApproximatelyTrig(FP.FromInt(45)); }

    // ========================================================================
    // RotateTowards
    // ========================================================================

    [Fact] public void RotateTowards_LargeDelta_SnapsToTarget() { var r = Vector3.RotateTowards(Vector3.Right, Vector3.Up, FP.FromInt(10), FP.One); r.ShouldBeApproximately(Vector3.Up); }
    [Fact] public void RotateTowards_ZeroDelta_StaysPut() { var r = Vector3.RotateTowards(Vector3.Right, Vector3.Up, FP.Zero, FP.One); r.ShouldBeApproximately(Vector3.Right); }

    // ========================================================================
    // Max / Min
    // ========================================================================
    [Fact] public void Max_Works() => Assert.Equal(new Vector3(FP.FromInt(3), FP.FromInt(5), FP.FromInt(7)), Vector3.Max(new Vector3(FP.One, FP.FromInt(5), FP.FromInt(2)), new Vector3(FP.FromInt(3), FP.FromInt(2), FP.FromInt(7))));
    [Fact] public void Min_Works() => Assert.Equal(new Vector3(FP.One, FP.FromInt(2), FP.FromInt(2)), Vector3.Min(new Vector3(FP.One, FP.FromInt(5), FP.FromInt(2)), new Vector3(FP.FromInt(3), FP.FromInt(2), FP.FromInt(7))));

    // ========================================================================
    // Equality & Hashing & String
    // ========================================================================
    [Fact] public void Equals_Works() => Assert.True(new Vector3(FP.One, FP.Two, FP.FromInt(3)).Equals(new Vector3(FP.One, FP.Two, FP.FromInt(3))));
    [Fact] public void GetHashCode_Consistent() => Assert.Equal(new Vector3(FP.One, FP.Two, FP.FromInt(3)).GetHashCode(), new Vector3(FP.One, FP.Two, FP.FromInt(3)).GetHashCode());
    [Fact] public void ToString_ContainsComponents() { var s = new Vector3(FP.One, FP.Two, FP.FromInt(3)).ToString(); Assert.Contains("1.000000", s); Assert.Contains("2.000000", s); Assert.Contains("3.000000", s); }
    [Fact] public void Parse_RoundTrip() { var v = new Vector3(FP.Half, FP.One, FP.Two); var parsed = Vector3.Parse(v.ToString()); parsed.ShouldBeApproximately(v); }
    [Fact] public void Parse_Invalid_Throws() => Assert.Throws<FormatException>(() => Vector3.Parse("bad"));
    [Fact] public void TryParse_Invalid_ReturnsFalse() => Assert.False(Vector3.TryParse("bad", out _));

    // ========================================================================
    // Additional Coverage
    // ========================================================================

    [Fact] public void Slerp_ZeroVectors_ReturnsZero() { var r = Vector3.Slerp(Vector3.Zero, Vector3.Zero, FP.Half); r.ShouldBeApproximately(Vector3.Zero); }
    [Fact] public void SlerpUnclamped_Works() { var r = Vector3.SlerpUnclamped(Vector3.Right, Vector3.Up, FP.Half); r.Magnitude.ShouldBeApproximately(FP.One, FP.Epsilon * FP.FromInt(5000)); }
    [Fact] public void RotateTowards_ZeroCurrent_ReturnsTowardsTarget() { var r = Vector3.RotateTowards(Vector3.Zero, Vector3.Right, FP.One, FP.One); r.X.ShouldBeApproximately(FP.One); }
    [Fact] public void ProjectOnPlane_ZeroNormal_ReturnsVector() { var r = Vector3.ProjectOnPlane(Vector3.One, Vector3.Zero); r.ShouldBeApproximately(Vector3.One); }
    [Fact] public void Reflect_SameAsIncoming_WhenNormalIsUp() { var r = Vector3.Reflect(new Vector3(FP.One, FP.Zero, FP.Zero), Vector3.Up); r.ShouldBeApproximately(new Vector3(FP.One, FP.Zero, FP.Zero)); }
    [Fact] public void MoveTowards_ZeroDelta_Stays() => Assert.Equal(Vector3.One, Vector3.MoveTowards(Vector3.One, Vector3.Zero, FP.Zero));
    [Fact] public void Equals_Null_ReturnsFalse() => Assert.False(Vector3.Zero.Equals(null));
    [Fact] public void TryParse_EmptyString_ReturnsFalse() => Assert.False(Vector3.TryParse("", out _));
}
