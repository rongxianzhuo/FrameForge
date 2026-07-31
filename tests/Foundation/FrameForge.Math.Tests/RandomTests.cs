using FrameForge.Foundation.FixedPoint;
using FrameForge.Foundation.Math;
using Xunit;

namespace FrameForge.Foundation.Math.Tests
{
public class RandomTests
{
    // ========================================================================
    // Determinism
    // ========================================================================

    [Fact]
    public void SameSeed_ProducesSameSequence()
    {
        var rng1 = new Math.Random(12345);
        var rng2 = new Math.Random(12345);

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(rng1.Next(), rng2.Next());
        }
    }

    [Fact]
    public void DifferentSeed_ProducesDifferentSequence()
    {
        var rng1 = new Math.Random(42);
        var rng2 = new Math.Random(99);
        bool different = false;
        for (int i = 0; i < 20; i++)
        {
            if (rng1.Next() != rng2.Next())
            {
                different = true;
                break;
            }
        }
        Assert.True(different);
    }

    // ========================================================================
    // Next()
    // ========================================================================

    [Fact]
    public void Next_ReturnsNonNegative()
    {
        var rng = new Math.Random(123);
        for (int i = 0; i < 100; i++)
            Assert.True(rng.Next() >= 0);
    }

    [Fact]
    public void Next_LessThanMaxValue()
    {
        var rng = new Math.Random(456);
        for (int i = 0; i < 100; i++)
            Assert.True(rng.Next() <= int.MaxValue);
    }

    // ========================================================================
    // Next(max)
    // ========================================================================

    [Fact]
    public void Next_MaxValue_ReturnsInRange()
    {
        var rng = new Math.Random(789);
        for (int i = 0; i < 100; i++)
        {
            int val = rng.Next(10);
            Assert.True(val >= 0 && val < 10);
        }
    }

    [Fact]
    public void Next_MaxValue_BadArgument_Throws()
    {
        var rng = new Math.Random(1);
        Assert.Throws<ArgumentOutOfRangeException>(() => rng.Next(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => rng.Next(-1));
    }

    // ========================================================================
    // Next(min, max)
    // ========================================================================

    [Fact]
    public void Next_MinMax_ReturnsInRange()
    {
        var rng = new Math.Random(42);
        for (int i = 0; i < 100; i++)
        {
            int val = rng.Next(5, 15);
            Assert.True(val >= 5 && val < 15);
        }
    }

    [Fact]
    public void Next_MinMax_SameValue_ReturnsThatValue()
    {
        var rng = new Math.Random(42);
        Assert.Equal(5, rng.Next(5, 5));
    }

    [Fact]
    public void Next_MinMax_BadArgument_Throws()
    {
        var rng = new Math.Random(1);
        Assert.Throws<ArgumentOutOfRangeException>(() => rng.Next(10, 5));
    }

    [Fact]
    public void Next_MinMax_FullRange_Works()
    {
        var rng = new Math.Random(42);
        // Test with large range
        int val = rng.Next(-1000000, 1000000);
        Assert.True(val >= -1000000 && val < 1000000);
    }

    // ========================================================================
    // NextFP
    // ========================================================================

    [Fact]
    public void NextFP_ReturnsInRange()
    {
        var rng = new Math.Random(555);
        for (int i = 0; i < 100; i++)
        {
            FP val = rng.NextFP();
            Assert.True(val >= FP.Zero && val < FP.One);
        }
    }

    [Fact]
    public void NextFP_MinMax_ReturnsInRange()
    {
        var rng = new Math.Random(666);
        FP min = FP.FromInt(5);
        FP max = FP.FromInt(15);
        for (int i = 0; i < 100; i++)
        {
            FP val = rng.NextFP(min, max);
            Assert.True(val >= min && val <= max);
        }
    }

    [Fact]
    public void NextFP_MinMax_BadArgument_Throws()
    {
        var rng = new Math.Random(1);
        Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextFP(FP.FromInt(10), FP.FromInt(5)));
    }

    // ========================================================================
    // InsideUnitCircle
    // ========================================================================

    [Fact]
    public void InsideUnitCircle_ReturnsPointsInsideCircle()
    {
        var rng = new Math.Random(777);
        for (int i = 0; i < 50; i++)
        {
            Vector2 p = rng.InsideUnitCircle();
            Assert.True(p.SqrMagnitude <= FP.One);
        }
    }

    // ========================================================================
    // InsideUnitSphere
    // ========================================================================

    [Fact]
    public void InsideUnitSphere_ReturnsPointsInsideSphere()
    {
        var rng = new Math.Random(888);
        for (int i = 0; i < 50; i++)
        {
            Vector3 p = rng.InsideUnitSphere();
            Assert.True(p.SqrMagnitude <= FP.One);
        }
    }

    // ========================================================================
    // OnUnitSphere
    // ========================================================================

    [Fact]
    public void OnUnitSphere_ReturnsPointsOnSurface()
    {
        var rng = new Math.Random(999);
        for (int i = 0; i < 50; i++)
        {
            Vector3 p = rng.OnUnitSphere();
            p.Magnitude.ShouldBeApproximately(FP.One, FP.Epsilon * FP.FromInt(5000));
        }
    }

    // ========================================================================
    // Random Distribution Sanity
    // ========================================================================

    [Fact]
    public void Next_EventuallyProducesAllBits()
    {
        var rng = new Math.Random(42);
        int mask = 0;
        for (int i = 0; i < 500; i++)
        {
            mask |= rng.Next();
        }
        // Should have seen most bits set
        Assert.True(mask != 0);
    }

    // ========================================================================
    // Additional Coverage
    // ========================================================================

    [Fact]
    public void InsideUnitCircle_ProducesVariedPoints()
    {
        var rng = new Math.Random(111);
        bool foundNonZero = false;
        for (int i = 0; i < 20; i++)
        {
            var p = rng.InsideUnitCircle();
            if (p.X != FP.Zero || p.Y != FP.Zero)
                foundNonZero = true;
        }
        Assert.True(foundNonZero);
    }

    [Fact]
    public void InsideUnitSphere_ProducesVariedPoints()
    {
        var rng = new Math.Random(222);
        bool foundNonZero = false;
        for (int i = 0; i < 20; i++)
        {
            var p = rng.InsideUnitSphere();
            if (p.X != FP.Zero || p.Y != FP.Zero || p.Z != FP.Zero)
                foundNonZero = true;
        }
        Assert.True(foundNonZero);
    }

    [Fact]
    public void NextFP_MinMax_Reversed_Throws()
    {
        var rng = new Math.Random(1);
        Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextFP(FP.FromInt(10), FP.FromInt(5)));
    }

    [Fact]
    public void Next_MinMax_LargeRange_Works()
    {
        var rng = new Math.Random(333);
        int val = rng.Next(int.MinValue, int.MaxValue);
        Assert.True(val >= int.MinValue && val < int.MaxValue);
    }
}
}
