using FrameForge.Foundation.FixedPoint;
using FrameForge.Foundation.Math;
using Xunit;

namespace FrameForge.Foundation.Math.Tests;

public class MathExTests
{
    // ========================================================================
    // Map
    // ========================================================================

    [Fact]
    public void Map_ZeroToTen_ToZeroToOne_Midpoint_ReturnsHalf()
    {
        var result = MathEx.Map(FP.FromInt(5), FP.Zero, FP.FromInt(10), FP.Zero, FP.One);
        result.ShouldBeApproximately(FP.Half);
    }

    [Fact]
    public void Map_OutsideRange_Extrapolates()
    {
        var result = MathEx.Map(FP.FromInt(15), FP.Zero, FP.FromInt(10), FP.Zero, FP.One);
        result.ShouldBeApproximately(FP.FromDouble(1.5));
    }

    [Fact]
    public void Map_SameFromBounds_ReturnsToMin()
    {
        var result = MathEx.Map(FP.FromInt(5), FP.FromInt(3), FP.FromInt(3), FP.Zero, FP.One);
        result.ShouldBeApproximately(FP.Zero);
    }

    // ========================================================================
    // IsPowerOfTwo
    // ========================================================================

    [Fact]
    public void IsPowerOfTwo_PowersOfTwo_ReturnsTrue()
    {
        Assert.True(MathEx.IsPowerOfTwo(1));
        Assert.True(MathEx.IsPowerOfTwo(2));
        Assert.True(MathEx.IsPowerOfTwo(4));
        Assert.True(MathEx.IsPowerOfTwo(8));
        Assert.True(MathEx.IsPowerOfTwo(1024));
        Assert.True(MathEx.IsPowerOfTwo(1 << 30));
    }

    [Fact]
    public void IsPowerOfTwo_NonPowers_ReturnsFalse()
    {
        Assert.False(MathEx.IsPowerOfTwo(0));
        Assert.False(MathEx.IsPowerOfTwo(3));
        Assert.False(MathEx.IsPowerOfTwo(5));
        Assert.False(MathEx.IsPowerOfTwo(6));
        Assert.False(MathEx.IsPowerOfTwo(7));
        Assert.False(MathEx.IsPowerOfTwo(-1));
        Assert.False(MathEx.IsPowerOfTwo(-8));
    }

    // ========================================================================
    // NextPowerOfTwo
    // ========================================================================

    [Fact]
    public void NextPowerOfTwo_NonPositive_ReturnsOne()
    {
        Assert.Equal(1, MathEx.NextPowerOfTwo(0));
        Assert.Equal(1, MathEx.NextPowerOfTwo(-1));
    }

    [Fact]
    public void NextPowerOfTwo_AlreadyPowerOfTwo_ReturnsSame()
    {
        Assert.Equal(1, MathEx.NextPowerOfTwo(1));
        Assert.Equal(2, MathEx.NextPowerOfTwo(2));
        Assert.Equal(4, MathEx.NextPowerOfTwo(4));
        Assert.Equal(256, MathEx.NextPowerOfTwo(256));
    }

    [Fact]
    public void NextPowerOfTwo_NotPowerOfTwo_ReturnsNext()
    {
        Assert.Equal(4, MathEx.NextPowerOfTwo(3));
        Assert.Equal(8, MathEx.NextPowerOfTwo(5));
        Assert.Equal(8, MathEx.NextPowerOfTwo(7));
        Assert.Equal(16, MathEx.NextPowerOfTwo(9));
        Assert.Equal(128, MathEx.NextPowerOfTwo(100));
    }
}
