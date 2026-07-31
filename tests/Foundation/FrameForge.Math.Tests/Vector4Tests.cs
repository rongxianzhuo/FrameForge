using FrameForge.Foundation.FixedPoint;
using FrameForge.Foundation.Math;
using Xunit;

namespace FrameForge.Foundation.Math.Tests
{
public class Vector4Tests
{
    [Fact] public void Constants_Zero_IsZero() => Assert.Equal(Vector4.Zero, new Vector4(FP.Zero, FP.Zero, FP.Zero, FP.Zero));
    [Fact] public void Constants_One_IsAllOnes() => Assert.Equal(Vector4.One, new Vector4(FP.One, FP.One, FP.One, FP.One));
    [Fact] public void Constructor_SetsComponents() { var v = new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4)); Assert.Equal(FP.One, v.X); Assert.Equal(FP.FromInt(4), v.W); }
    [Fact] public void Magnitude_Works() { var v = new Vector4(FP.Two, FP.Zero, FP.Zero, FP.Zero); v.Magnitude.ShouldBeApproximately(FP.Two); }
    [Fact] public void SqrMagnitude_Works() => Assert.Equal(FP.FromInt(30), new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4)).SqrMagnitude);
    [Fact] public void Normalized_LengthIsOne() { var v = new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4)).Normalized; v.Magnitude.ShouldBeApproximately(FP.One); }
    [Fact] public void Normalized_Zero_ReturnsZero() => Assert.Equal(Vector4.Zero, Vector4.Zero.Normalized);

    [Fact] public void Add_Works() => Assert.Equal(new Vector4(FP.FromInt(4), FP.FromInt(6), FP.FromInt(8), FP.FromInt(10)), new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4)) + new Vector4(FP.FromInt(3), FP.FromInt(4), FP.FromInt(5), FP.FromInt(6)));
    [Fact] public void Subtract_Works() => Assert.Equal(new Vector4(FP.One, FP.One, FP.One, FP.One), new Vector4(FP.FromInt(3), FP.FromInt(4), FP.FromInt(5), FP.FromInt(6)) - new Vector4(FP.Two, FP.FromInt(3), FP.FromInt(4), FP.FromInt(5)));
    [Fact] public void Negate_Works() => Assert.Equal(new Vector4(FP.MinusOne, FP.One, FP.FromInt(-3), FP.FromInt(-4)), -new Vector4(FP.One, FP.MinusOne, FP.FromInt(3), FP.FromInt(4)));
    [Fact] public void Multiply_Scalar_Works() => Assert.Equal(new Vector4(FP.Two, FP.FromInt(4), FP.FromInt(6), FP.FromInt(8)), new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4)) * FP.Two);
    [Fact] public void Multiply_ScalarLeft_Works() => Assert.Equal(new Vector4(FP.FromInt(2), FP.FromInt(4), FP.FromInt(6), FP.FromInt(8)), FP.Two * new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4)));
    [Fact] public void Divide_Scalar_Works() { var r = new Vector4(FP.Two, FP.FromInt(4), FP.FromInt(6), FP.FromInt(8)) / FP.Two; r.ShouldBeApproximately(new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4))); }
    [Fact] public void Equality_Works() => Assert.True(new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4)) == new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4)));

    [Fact] public void Dot_Works() => Assert.Equal(FP.FromInt(30), Vector4.Dot(new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4)), new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4))));
    [Fact] public void Dot_Instance_Works() => Assert.Equal(FP.FromInt(30), new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4)).Dot(new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4))));

    [Fact] public void Distance_Works() { var d = Vector4.Distance(Vector4.Zero, new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4))); d.ShouldBeApproximately(FP.Sqrt(FP.FromInt(30))); }
    [Fact] public void SqrDistance_Works() => Assert.Equal(FP.FromInt(30), Vector4.SqrDistance(Vector4.Zero, new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4))));

    [Fact] public void Lerp_THalf_ReturnsMidpoint() { var r = Vector4.Lerp(Vector4.Zero, Vector4.One, FP.Half); r.ShouldBeApproximately(new Vector4(FP.Half, FP.Half, FP.Half, FP.Half)); }
    [Fact] public void MoveTowards_Clamped_ReturnsPartial() { var r = Vector4.MoveTowards(Vector4.Zero, new Vector4(FP.One, FP.Zero, FP.Zero, FP.Zero), FP.Half); r.X.ShouldBeApproximately(FP.Half); }

    [Fact] public void Scale_Works() => Assert.Equal(new Vector4(FP.One, FP.FromInt(6), FP.FromInt(15), FP.FromInt(28)), Vector4.Scale(new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4)), new Vector4(FP.One, FP.FromInt(3), FP.FromInt(5), FP.FromInt(7))));

    [Fact] public void Max_Works() => Assert.Equal(new Vector4(FP.FromInt(3), FP.FromInt(5), FP.FromInt(7), FP.FromInt(9)), Vector4.Max(new Vector4(FP.One, FP.FromInt(5), FP.FromInt(3), FP.FromInt(9)), new Vector4(FP.FromInt(3), FP.FromInt(2), FP.FromInt(7), FP.FromInt(4))));
    [Fact] public void Min_Works() => Assert.Equal(new Vector4(FP.One, FP.FromInt(2), FP.FromInt(3), FP.FromInt(4)), Vector4.Min(new Vector4(FP.One, FP.FromInt(5), FP.FromInt(3), FP.FromInt(9)), new Vector4(FP.FromInt(3), FP.FromInt(2), FP.FromInt(7), FP.FromInt(4))));

    [Fact] public void Equals_Works() => Assert.True(new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4)).Equals(new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4))));
    [Fact] public void Equals_DifferentType_ReturnsFalse() => Assert.False(Vector4.Zero.Equals("hello"));
    [Fact] public void GetHashCode_Consistent() => Assert.Equal(new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4)).GetHashCode(), new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4)).GetHashCode());
    [Fact] public void ToString_ContainsComponents() { var s = new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4)).ToString(); Assert.Contains("1.000000", s); Assert.Contains("4.000000", s); }
    [Fact] public void Parse_RoundTrip() { var v = new Vector4(FP.Half, FP.One, FP.Two, FP.FromInt(3)); var parsed = Vector4.Parse(v.ToString()); parsed.ShouldBeApproximately(v); }
    [Fact] public void Parse_Invalid_Throws() => Assert.Throws<FormatException>(() => Vector4.Parse("bad"));

    // ========================================================================
    // Additional Coverage
    // ========================================================================

    [Fact] public void Scale_Identity_ReturnsSelf() { var v = new Vector4(FP.One, FP.Two, FP.FromInt(3), FP.FromInt(4)); var r = Vector4.Scale(v, Vector4.One); r.ShouldBeApproximately(v); }
    [Fact] public void MoveTowards_ZeroDelta_Stays() => Assert.Equal(Vector4.One, Vector4.MoveTowards(Vector4.One, Vector4.Zero, FP.Zero));
    [Fact] public void Dot_ZeroVector_ReturnsZero() => Assert.Equal(FP.Zero, Vector4.Dot(Vector4.Zero, Vector4.One));
    [Fact] public void TryParse_EmptyString_ReturnsFalse() => Assert.False(Vector4.TryParse("", out _));
    [Fact] public void TryParse_TooFewComponents_ReturnsFalse() => Assert.False(Vector4.TryParse("(1,2,3)", out _));
}
}
