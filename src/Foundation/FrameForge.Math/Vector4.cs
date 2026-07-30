using System;
using System.Globalization;
using FrameForge.Foundation.FixedPoint;

namespace FrameForge.Foundation.Math;

/// <summary>
/// Represents a four-dimensional vector using <see cref="FP"/> fixed-point numbers.
/// All operations are deterministic across platforms.
/// </summary>
public readonly partial struct Vector4 : IEquatable<Vector4>
{
    // ========================================================================
    // Fields
    // ========================================================================

    /// <summary>X component.</summary>
    public readonly FP X;

    /// <summary>Y component.</summary>
    public readonly FP Y;

    /// <summary>Z component.</summary>
    public readonly FP Z;

    /// <summary>W component.</summary>
    public readonly FP W;

    // ========================================================================
    // Constants
    // ========================================================================

    /// <summary>Zero vector (0, 0, 0, 0).</summary>
    public static readonly Vector4 Zero = new(FP.Zero, FP.Zero, FP.Zero, FP.Zero);

    /// <summary>One vector (1, 1, 1, 1).</summary>
    public static readonly Vector4 One = new(FP.One, FP.One, FP.One, FP.One);

    // ========================================================================
    // Constructors
    // ========================================================================

    /// <summary>
    /// Creates a new <see cref="Vector4"/> with the given components.
    /// </summary>
    public Vector4(FP x, FP y, FP z, FP w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    // ========================================================================
    // Properties
    // ========================================================================

    /// <summary>
    /// Returns the length (magnitude) of this vector.
    /// Computed as Sqrt(X*X + Y*Y + Z*Z + W*W).
    /// </summary>
    public FP Magnitude => FP.Sqrt(SqrMagnitude);

    /// <summary>
    /// Returns the squared length (X*X + Y*Y + Z*Z + W*W).
    /// Faster than <see cref="Magnitude"/> because it avoids a square root.
    /// </summary>
    public FP SqrMagnitude => X * X + Y * Y + Z * Z + W * W;

    /// <summary>
    /// Returns a normalized copy of this vector (length = 1).
    /// Returns <see cref="Zero"/> if the vector is zero.
    /// </summary>
    public Vector4 Normalized
    {
        get
        {
            FP sqrMag = SqrMagnitude;
            if (sqrMag == FP.Zero)
                return Zero;
            FP invMag = FP.One / FP.Sqrt(sqrMag);
            return new Vector4(X * invMag, Y * invMag, Z * invMag, W * invMag);
        }
    }

    // ========================================================================
    // Operators
    // ========================================================================

    /// <summary>Component-wise addition.</summary>
    public static Vector4 operator +(Vector4 a, Vector4 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);

    /// <summary>Component-wise subtraction.</summary>
    public static Vector4 operator -(Vector4 a, Vector4 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);

    /// <summary>Unary negation (reverses direction).</summary>
    public static Vector4 operator -(Vector4 v) => new(-v.X, -v.Y, -v.Z, -v.W);

    /// <summary>Scalar multiplication (vector * scalar).</summary>
    public static Vector4 operator *(Vector4 v, FP scalar) => new(v.X * scalar, v.Y * scalar, v.Z * scalar, v.W * scalar);

    /// <summary>Scalar multiplication (scalar * vector).</summary>
    public static Vector4 operator *(FP scalar, Vector4 v) => new(v.X * scalar, v.Y * scalar, v.Z * scalar, v.W * scalar);

    /// <summary>Scalar division (vector / scalar).</summary>
    public static Vector4 operator /(Vector4 v, FP scalar) => new(v.X / scalar, v.Y / scalar, v.Z / scalar, v.W / scalar);

    /// <summary>Component-wise equality.</summary>
    public static bool operator ==(Vector4 a, Vector4 b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;

    /// <summary>Component-wise inequality.</summary>
    public static bool operator !=(Vector4 a, Vector4 b) => !(a == b);

    // ========================================================================
    // Instance Methods
    // ========================================================================

    /// <summary>
    /// Returns the dot product of this vector and <paramref name="other"/>.
    /// </summary>
    public FP Dot(Vector4 other) => X * other.X + Y * other.Y + Z * other.Z + W * other.W;

    // ========================================================================
    // Static Methods
    // ========================================================================

    /// <summary>
    /// Returns the dot product of <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    public static FP Dot(Vector4 a, Vector4 b) => a.Dot(b);

    /// <summary>
    /// Returns the Euclidean distance between two points.
    /// </summary>
    public static FP Distance(Vector4 a, Vector4 b) => (a - b).Magnitude;

    /// <summary>
    /// Returns the squared Euclidean distance between two points.
    /// Faster than <see cref="Distance"/> because it avoids a square root.
    /// </summary>
    public static FP SqrDistance(Vector4 a, Vector4 b) => (a - b).SqrMagnitude;

    /// <summary>
    /// Linearly interpolates between two vectors. <paramref name="t"/> is clamped to [0, 1].
    /// </summary>
    public static Vector4 Lerp(Vector4 a, Vector4 b, FP t)
    {
        t = FP.Clamp01(t);
        return LerpUnclamped(a, b, t);
    }

    /// <summary>
    /// Linearly interpolates between two vectors without clamping <paramref name="t"/>.
    /// </summary>
    public static Vector4 LerpUnclamped(Vector4 a, Vector4 b, FP t)
    {
        return new Vector4(
            FP.LerpUnclamped(a.X, b.X, t),
            FP.LerpUnclamped(a.Y, b.Y, t),
            FP.LerpUnclamped(a.Z, b.Z, t),
            FP.LerpUnclamped(a.W, b.W, t));
    }

    /// <summary>
    /// Moves <paramref name="current"/> towards <paramref name="target"/> by at most
    /// <paramref name="maxDistanceDelta"/>. Won't overshoot.
    /// </summary>
    public static Vector4 MoveTowards(Vector4 current, Vector4 target, FP maxDistanceDelta)
    {
        Vector4 diff = target - current;
        FP sqrDist = diff.SqrMagnitude;

        if (sqrDist == FP.Zero || (maxDistanceDelta >= FP.Zero && sqrDist <= maxDistanceDelta * maxDistanceDelta))
            return target;

        FP dist = FP.Sqrt(sqrDist);
        return current + diff / dist * maxDistanceDelta;
    }

    /// <summary>
    /// Returns the component-wise product (scale) of <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    public static Vector4 Scale(Vector4 a, Vector4 b)
    {
        return new Vector4(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
    }

    /// <summary>
    /// Returns a vector where each component is the maximum of the corresponding
    /// components of <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    public static Vector4 Max(Vector4 a, Vector4 b)
    {
        return new Vector4(FP.Max(a.X, b.X), FP.Max(a.Y, b.Y), FP.Max(a.Z, b.Z), FP.Max(a.W, b.W));
    }

    /// <summary>
    /// Returns a vector where each component is the minimum of the corresponding
    /// components of <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    public static Vector4 Min(Vector4 a, Vector4 b)
    {
        return new Vector4(FP.Min(a.X, b.X), FP.Min(a.Y, b.Y), FP.Min(a.Z, b.Z), FP.Min(a.W, b.W));
    }

    // ========================================================================
    // Equality & Hashing
    // ========================================================================

    /// <inheritdoc />
    public bool Equals(Vector4 other) => this == other;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Vector4 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);

    // ========================================================================
    // String Representation
    // ========================================================================

    /// <summary>
    /// Returns a string representation of this vector in the format "(X, Y, Z, W)".
    /// Each component uses 6 decimal places, allowing lossless round-trip via <see cref="Parse"/>.
    /// </summary>
    public override string ToString()
    {
        return $"({X}, {Y}, {Z}, {W})";
    }

    /// <summary>
    /// Parses a string representation like "(1.000000, 2.500000, 3.000000, 4.000000)" into a <see cref="Vector4"/>.
    /// </summary>
    public static Vector4 Parse(string s)
    {
        if (!TryParse(s, out Vector4 result))
            throw new FormatException($"Cannot parse '{s}' as a Vector4.");
        return result;
    }

    /// <summary>
    /// Attempts to parse a string representation into a <see cref="Vector4"/>.
    /// </summary>
    public static bool TryParse(string s, out Vector4 result)
    {
        result = Zero;
        if (string.IsNullOrWhiteSpace(s))
            return false;

        s = s.Trim();
        if (!s.StartsWith('(') || !s.EndsWith(')'))
            return false;

        s = s[1..^1].Trim();
        string[] parts = s.Split(',');
        if (parts.Length != 4)
            return false;

        if (!FP.TryParse(parts[0].Trim(), out FP x) ||
            !FP.TryParse(parts[1].Trim(), out FP y) ||
            !FP.TryParse(parts[2].Trim(), out FP z) ||
            !FP.TryParse(parts[3].Trim(), out FP w))
            return false;

        result = new Vector4(x, y, z, w);
        return true;
    }
}
