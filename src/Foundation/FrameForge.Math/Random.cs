using FrameForge.Foundation.FixedPoint;

namespace FrameForge.Foundation.Math
{
/// <summary>
/// A deterministic pseudo-random number generator based on xorshift128+.
/// Given the same seed, produces the exact same sequence on all platforms.
/// </summary>
public class Random
{
    // ========================================================================
    // State
    // ========================================================================

    private ulong _state0;
    private ulong _state1;

    // ========================================================================
    // Constructors
    // ========================================================================

    /// <summary>
    /// Creates a new <see cref="Random"/> instance with the given seed.
    /// Same seed always produces the same sequence.
    /// </summary>
    public Random(int seed)
    {
        // SplitMix64 to seed xorshift from a single int
        ulong z = (ulong)(unchecked((uint)seed));
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        _state0 = z ^ (z >> 31);
        z = z * 0xBF58476D1CE4E5B9UL;
        _state1 = z ^ (z >> 31);

        // Ensure non-zero state
        if (_state0 == 0 && _state1 == 0)
            _state0 = 1;
    }

    // ========================================================================
    // Core: Next raw 64-bit
    // ========================================================================

    /// <summary>
    /// Returns a uniformly distributed <see cref="ulong"/> in [0, 2^64-1].
    /// </summary>
    private ulong NextRaw64()
    {
        ulong s1 = _state0;
        ulong s0 = _state1;
        ulong result = s0 + s1;
        _state0 = s0;
        s1 ^= s1 << 23;
        _state1 = s1 ^ s0 ^ (s1 >> 18) ^ (s0 >> 5);
        return result;
    }

    // ========================================================================
    // Integer Random
    // ========================================================================

    /// <summary>
    /// Returns a non-negative random integer in [0, <see cref="int.MaxValue"/>].
    /// </summary>
    public int Next()
    {
        return (int)(NextRaw64() & 0x7FFFFFFFUL);
    }

    /// <summary>
    /// Returns a non-negative random integer in [0, <paramref name="maxValue"/>).
    /// </summary>
    public int Next(int maxValue)
    {
        if (maxValue <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxValue),
                "maxValue must be positive.");
        return Next(0, maxValue);
    }

    /// <summary>
    /// Returns a random integer in [<paramref name="minValue"/>,
    /// <paramref name="maxValue"/>).
    /// </summary>
    public int Next(int minValue, int maxValue)
    {
        if (minValue > maxValue)
            throw new ArgumentOutOfRangeException(nameof(minValue),
                "minValue must be <= maxValue.");

        long range = (long)maxValue - minValue;
        if (range == 0)
            return minValue;

        if (range <= int.MaxValue)
        {
            // Standard method: rejection sampling
            uint rng;
            uint maxAccept = (uint)((0x80000000UL / (uint)range) * (uint)range - 1);
            do
            {
                rng = (uint)(NextRaw64() & 0x7FFFFFFFUL);
            } while (rng > maxAccept);

            return minValue + (int)(rng % (uint)range);
        }
        else
        {
            // Large range (close to int.MaxValue - int.MinValue)
            ulong rng;
            ulong bigRange = (ulong)range;
            ulong maxAccept = ulong.MaxValue - (ulong.MaxValue % bigRange) - 1;
            do
            {
                rng = NextRaw64();
            } while (rng > maxAccept);

            return (int)(minValue + (long)(rng % bigRange));
        }
    }

    // ========================================================================
    // Fixed-Point Random
    // ========================================================================

    /// <summary>
    /// Returns a random <see cref="FP"/> in [0, 1).
    /// High-quality: uses the full 64-bit state for maximum precision.
    /// </summary>
    public FP NextFP()
    {
        // Use 32 high bits for the fractional part
        // This gives uniform distribution across [0, 1) with Q32.32 precision
        uint bits = (uint)(NextRaw64() >> 32);
        return FP.FromRaw(bits);
    }

    /// <summary>
    /// Returns a random <see cref="FP"/> in [<paramref name="minValue"/>,
    /// <paramref name="maxValue"/>).
    /// </summary>
    public FP NextFP(FP minValue, FP maxValue)
    {
        if (minValue > maxValue)
            throw new ArgumentOutOfRangeException(nameof(minValue),
                "minValue must be <= maxValue.");

        FP t = NextFP();
        return FP.LerpUnclamped(minValue, maxValue, t);
    }

    // ========================================================================
    // Geometric Random
    // ========================================================================

    /// <summary>
    /// Returns a random point inside the unit circle (radius 1).
    /// Uses rejection sampling.
    /// </summary>
    public Vector2 InsideUnitCircle()
    {
        FP angle = NextFP() * FP.Two * FP.PI;
        // Rejection sampling for radius to get uniform distribution
        FP r;
        do
        {
            FP u1 = NextFP();
            FP u2 = NextFP();
            r = FP.Sqrt(u1); // sqrt for uniform area distribution
            if (u2 <= FP.One) // always true since u2 in [0,1], but keeps pattern
                break;
        } while (true);

        return new Vector2(
            r * FP.Cos(angle),
            r * FP.Sin(angle)
        );
    }

    /// <summary>
    /// Returns a random point inside the unit sphere (radius 1).
    /// Uses rejection sampling for uniform volumetric distribution.
    /// </summary>
    public Vector3 InsideUnitSphere()
    {
        Vector3 result;
        do
        {
            // Pick point in [-1,1] cube and reject if outside sphere
            FP x = NextFP() * FP.Two - FP.One;
            FP y = NextFP() * FP.Two - FP.One;
            FP z = NextFP() * FP.Two - FP.One;
            result = new Vector3(x, y, z);
        } while (result.SqrMagnitude > FP.One);

        return result;
    }

    /// <summary>
    /// Returns a random point on the surface of the unit sphere.
    /// Uses Marsaglia's method for uniform distribution.
    /// </summary>
    public Vector3 OnUnitSphere()
    {
        // Marsaglia's method: pick two uniform angles
        FP theta = NextFP() * FP.Two * FP.PI;
        FP phi = FP.Acos(FP.Two * NextFP() - FP.One);

        FP sinPhi = FP.Sin(phi);
        return new Vector3(
            sinPhi * FP.Cos(theta),
            sinPhi * FP.Sin(theta),
            FP.Cos(phi)
        );
    }
}
}
