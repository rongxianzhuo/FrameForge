using FrameForge.Foundation.FixedPoint;
using FrameForge.Foundation.Math;
using Xunit;

namespace FrameForge.Foundation.Math.Tests;

public class Vector2Tests
{
    // ========================================================================
    // Constants
    // ========================================================================

    [Fact]
    public void Constants_Zero_IsZeroVector() { Assert.Equal(Vector2.Zero, new Vector2(FP.Zero, FP.Zero)); }

    [Fact]
    public void Constants_One_IsAllOnes() { Assert.Equal(Vector2.One, new Vector2(FP.One, FP.One)); }

    [Fact]
    public void Constants_Up_IsPositiveY() { Assert.Equal(Vector2.Up, new Vector2(FP.Zero, FP.One)); }

    [Fact]
    public void Constants_Down_IsNegativeY() { Assert.Equal(Vector2.Down, new Vector2(FP.Zero, FP.MinusOne)); }

    [Fact]
    public void Constants_Left_IsNegativeX() { Assert.Equal(Vector2.Left, new Vector2(FP.MinusOne, FP.Zero)); }

    [Fact]
    public void Constants_Right_IsPositiveX() { Assert.Equal(Vector2.Right, new Vector2(FP.One, FP.Zero)); }

    // ========================================================================
    // Constructors & Properties
    // ========================================================================

    [Fact]
    public void Constructor_SetsComponents() { var v = new Vector2(FP.FromInt(3), FP.FromInt(4)); Assert.Equal(FP.FromInt(3), v.X); Assert.Equal(FP.FromInt(4), v.Y); }

    [Fact]
    public void Magnitude_UnitVector_ReturnsOne() { Assert.Equal(FP.One, Vector2.Right.Magnitude); }

    [Fact]
    public void Magnitude_346Triangle_Returns5() { var v = new Vector2(FP.FromInt(3), FP.FromInt(4)); v.Magnitude.ShouldBeApproximately(FP.FromInt(5)); }

    [Fact]
    public void SqrMagnitude_AvoidsSqrt() { var v = new Vector2(FP.FromInt(3), FP.FromInt(4)); Assert.Equal(FP.FromInt(25), v.SqrMagnitude); }

    [Fact]
    public void Normalized_UnitVector_ReturnsSame() { var v = Vector2.Right.Normalized; v.ShouldBeApproximately(Vector2.Right); }

    [Fact]
    public void Normalized_ZeroVector_ReturnsZero() { Assert.Equal(Vector2.Zero, Vector2.Zero.Normalized); }

    [Fact]
    public void Normalized_NonUnit_LengthIsOne() { var v = new Vector2(FP.FromInt(3), FP.FromInt(4)).Normalized; v.Magnitude.ShouldBeApproximately(FP.One); }

    // ========================================================================
    // Operators
    // ========================================================================

    [Fact]
    public void Add_TwoVectors_Works() { var a = new Vector2(FP.One, FP.Two); var b = new Vector2(FP.FromInt(3), FP.FromInt(4)); Assert.Equal(new Vector2(FP.FromInt(4), FP.FromInt(6)), a + b); }

    [Fact]
    public void Subtract_TwoVectors_Works() { var a = new Vector2(FP.FromInt(5), FP.FromInt(7)); var b = new Vector2(FP.FromInt(2), FP.FromInt(3)); Assert.Equal(new Vector2(FP.FromInt(3), FP.FromInt(4)), a - b); }

    [Fact]
    public void Negate_ReversesSigns() { var v = new Vector2(FP.FromInt(3), FP.FromInt(-4)); Assert.Equal(new Vector2(FP.FromInt(-3), FP.FromInt(4)), -v); }

    [Fact]
    public void Multiply_Scalar_Works() { var v = new Vector2(FP.FromInt(2), FP.FromInt(3)); Assert.Equal(new Vector2(FP.FromInt(4), FP.FromInt(6)), v * FP.FromInt(2)); }

    [Fact]
    public void Multiply_ScalarLeft_Works() { var v = new Vector2(FP.FromInt(2), FP.FromInt(3)); Assert.Equal(new Vector2(FP.FromInt(4), FP.FromInt(6)), FP.FromInt(2) * v); }

    [Fact]
    public void Divide_Scalar_Works() { var v = new Vector2(FP.FromInt(4), FP.FromInt(6)); var result = v / FP.FromInt(2); result.ShouldBeApproximately(new Vector2(FP.FromInt(2), FP.FromInt(3))); }

    [Fact]
    public void Equality_Same_ReturnsTrue() { Assert.True(new Vector2(FP.One, FP.Two) == new Vector2(FP.One, FP.Two)); }

    [Fact]
    public void Equality_Different_ReturnsFalse() { Assert.False(new Vector2(FP.One, FP.Two) == new Vector2(FP.One, FP.FromInt(3))); }

    [Fact]
    public void Inequality_Works() { Assert.True(new Vector2(FP.One, FP.Two) != new Vector2(FP.Two, FP.One)); }

    // ========================================================================
    // Dot Product
    // ========================================================================

    [Fact]
    public void Dot_Perpendicular_ReturnsZero() { Assert.Equal(FP.Zero, Vector2.Right.Dot(Vector2.Up)); }

    [Fact]
    public void Dot_Parallel_ReturnsProduct() { Assert.Equal(FP.One, Vector2.Right.Dot(Vector2.Right)); }

    [Fact]
    public void Dot_Static_MatchesInstance() { var a = new Vector2(FP.FromInt(2), FP.FromInt(3)); var b = new Vector2(FP.FromInt(4), FP.FromInt(5)); Assert.Equal(a.Dot(b), Vector2.Dot(a, b)); }

    // ========================================================================
    // Distance
    // ========================================================================

    [Fact]
    public void Distance_SamePoint_ReturnsZero() { Assert.Equal(FP.Zero, Vector2.Distance(Vector2.One, Vector2.One)); }

    [Fact]
    public void Distance_DifferentPoints_Works() { var d = Vector2.Distance(Vector2.Zero, new Vector2(FP.FromInt(3), FP.FromInt(4))); d.ShouldBeApproximately(FP.FromInt(5)); }

    [Fact]
    public void SqrDistance_AvoidsSqrt() { Assert.Equal(FP.FromInt(25), Vector2.SqrDistance(Vector2.Zero, new Vector2(FP.FromInt(3), FP.FromInt(4)))); }

    // ========================================================================
    // Angle
    // ========================================================================

    [Fact]
    public void Angle_SameDirection_ReturnsZero() { Assert.Equal(FP.Zero, Vector2.Angle(Vector2.Right, Vector2.Right)); }

    [Fact]
    public void Angle_RightAngle_Returns90() { var angle = Vector2.Angle(Vector2.Right, Vector2.Up); angle.ShouldBeApproximatelyTrig(FP.FromInt(90)); }

    [Fact]
    public void Angle_Opposite_Returns180() { var angle = Vector2.Angle(Vector2.Right, Vector2.Left); angle.ShouldBeApproximatelyTrig(FP.FromInt(180)); }

    [Fact]
    public void Angle_ZeroVector_ReturnsZero() { Assert.Equal(FP.Zero, Vector2.Angle(Vector2.Zero, Vector2.Right)); }

    // ========================================================================
    // Interpolation
    // ========================================================================

    [Fact]
    public void Lerp_TZero_ReturnsA() { var a = Vector2.Zero; var b = Vector2.One; Assert.Equal(a, Vector2.Lerp(a, b, FP.Zero)); }

    [Fact]
    public void Lerp_TOne_ReturnsB() { Assert.Equal(Vector2.One, Vector2.Lerp(Vector2.Zero, Vector2.One, FP.One)); }

    [Fact]
    public void Lerp_THalf_ReturnsMidpoint() { var result = Vector2.Lerp(Vector2.Zero, Vector2.One, FP.Half); result.ShouldBeApproximately(new Vector2(FP.Half, FP.Half)); }

    [Fact]
    public void Lerp_TClamped_StaysInRange() { Assert.Equal(Vector2.One, Vector2.Lerp(Vector2.Zero, Vector2.One, FP.FromInt(2))); }

    [Fact]
    public void LerpUnclamped_Two_Extrapolates() { var result = Vector2.LerpUnclamped(Vector2.Zero, Vector2.One, FP.FromInt(2)); result.ShouldBeApproximately(new Vector2(FP.Two, FP.Two)); }

    // ========================================================================
    // MoveTowards
    // ========================================================================

    [Fact]
    public void MoveTowards_AlreadyAtTarget_ReturnsTarget() { Assert.Equal(Vector2.One, Vector2.MoveTowards(Vector2.One, Vector2.One, FP.One)); }

    [Fact]
    public void MoveTowards_WithinDelta_ReturnsTarget() { var result = Vector2.MoveTowards(Vector2.Zero, Vector2.Right, FP.FromInt(2)); Assert.Equal(Vector2.Right, result); }

    [Fact]
    public void MoveTowards_Clamped_ReturnsPartial() { var result = Vector2.MoveTowards(Vector2.Zero, Vector2.Right, FP.Half); result.X.ShouldBeApproximately(FP.Half); result.Y.ShouldBeApproximately(FP.Zero); }

    // ========================================================================
    // Reflect
    // ========================================================================

    [Fact]
    public void Reflect_AgainstUp_FlipsY() { var result = Vector2.Reflect(new Vector2(FP.One, FP.MinusOne), Vector2.Up); result.X.ShouldBeApproximately(FP.One); result.Y.ShouldBeApproximately(FP.One); }

    [Fact]
    public void Reflect_AgainstNormal_Works() { var incoming = new Vector2(FP.One, -FP.One); var normal = Vector2.Up; var reflected = Vector2.Reflect(incoming, normal); var dot = Vector2.Dot(reflected, normal); Assert.True(dot > FP.Zero); }

    // ========================================================================
    // Max / Min
    // ========================================================================

    [Fact]
    public void Max_ReturnsComponentWiseMax() { var result = Vector2.Max(new Vector2(FP.One, FP.FromInt(5)), new Vector2(FP.FromInt(3), FP.FromInt(2))); Assert.Equal(new Vector2(FP.FromInt(3), FP.FromInt(5)), result); }

    [Fact]
    public void Min_ReturnsComponentWiseMin() { var result = Vector2.Min(new Vector2(FP.One, FP.FromInt(5)), new Vector2(FP.FromInt(3), FP.FromInt(2))); Assert.Equal(new Vector2(FP.One, FP.FromInt(2)), result); }

    // ========================================================================
    // Equality & Hashing
    // ========================================================================

    [Fact]
    public void Equals_SameValue_ReturnsTrue() { Assert.True(new Vector2(FP.One, FP.Two).Equals(new Vector2(FP.One, FP.Two))); }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse() { Assert.False(Vector2.Zero.Equals("hello")); }

    [Fact]
    public void Equals_Null_ReturnsFalse() { Assert.False(Vector2.Zero.Equals(null)); }

    [Fact]
    public void GetHashCode_SameValues_HaveSameHash() { Assert.Equal(new Vector2(FP.One, FP.Two).GetHashCode(), new Vector2(FP.One, FP.Two).GetHashCode()); }

    // ========================================================================
    // String Parsing
    // ========================================================================

    [Fact]
    public void ToString_ReturnsFormattedString() { var s = new Vector2(FP.One, FP.Two).ToString(); Assert.Contains("1.000000", s); Assert.Contains("2.000000", s); }

    [Fact]
    public void Parse_RoundTrip_Works() { var v = new Vector2(FP.FromInt(1) + FP.Half, FP.FromInt(3)); var s = v.ToString(); var parsed = Vector2.Parse(s); parsed.ShouldBeApproximately(v); }

    [Fact]
    public void Parse_Invalid_Throws() { Assert.Throws<FormatException>(() => Vector2.Parse("not a vector")); }

    [Fact]
    public void TryParse_Valid_ReturnsTrue() { Assert.True(Vector2.TryParse("(1.000000, 2.000000)", out var result)); }

    [Fact]
    public void TryParse_Invalid_ReturnsFalse() { Assert.False(Vector2.TryParse("garbage", out _)); }

    // ========================================================================
    // Additional Coverage
    // ========================================================================

    [Fact] public void MoveTowards_ZeroDelta_ReturnsCurrent() { Assert.Equal(Vector2.Zero, Vector2.MoveTowards(Vector2.Zero, Vector2.One, FP.Zero)); }
    [Fact] public void MoveTowards_NegativeDirection_Works() { var r = Vector2.MoveTowards(Vector2.Right, Vector2.Left, FP.Half); r.X.ShouldBeApproximately(FP.Half); }
    [Fact] public void SqrMagnitude_Zero_ReturnsZero() => Assert.Equal(FP.Zero, Vector2.Zero.SqrMagnitude);
    [Fact] public void Distance_Zero_ReturnsZero() => Assert.Equal(FP.Zero, Vector2.Distance(Vector2.Zero, Vector2.Zero));
    [Fact] public void Angle_SameDirection_WithLargeVectors() { var a = Vector2.Angle(new Vector2(FP.FromInt(100), FP.Zero), new Vector2(FP.FromInt(200), FP.Zero)); a.ShouldBeApproximately(FP.Zero); }
    [Fact] public void Reflect_45Degree_Works() { var incoming = new Vector2(FP.One, FP.MinusOne).Normalized; var normal = Vector2.Up; var reflected = Vector2.Reflect(incoming, normal); reflected.Y.ShouldBeApproximately(FP.Sqrt(FP.FromInt(2)) / FP.Two); }
    [Fact] public void TryParse_EmptyOrWhitespace_ReturnsFalse() { Assert.False(Vector2.TryParse("", out _)); Assert.False(Vector2.TryParse("   ", out _)); }
    [Fact] public void TryParse_MissingClosingParen_ReturnsFalse() => Assert.False(Vector2.TryParse("(1.0, 2.0", out _));
}
