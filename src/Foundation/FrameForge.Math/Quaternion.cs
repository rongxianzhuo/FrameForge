using System;
using System.Globalization;
using FrameForge.Foundation.FixedPoint;

namespace FrameForge.Foundation.Math
{
/// <summary>
/// Represents a rotation as a quaternion using <see cref="FP"/> fixed-point numbers.
/// All operations are deterministic across platforms.
/// Uses Unity-compatible ZXY Euler angle convention and left-handed coordinate system.
/// </summary>
public readonly partial struct Quaternion : IEquatable<Quaternion>
{
    // ========================================================================
    // Fields
    // ========================================================================

    /// <summary>X component (imaginary i).</summary>
    public readonly FP X;

    /// <summary>Y component (imaginary j).</summary>
    public readonly FP Y;

    /// <summary>Z component (imaginary k).</summary>
    public readonly FP Z;

    /// <summary>W component (real part).</summary>
    public readonly FP W;

    // ========================================================================
    // Constants
    // ========================================================================

    /// <summary>Identity quaternion representing no rotation (0, 0, 0, 1).</summary>
    public static readonly Quaternion Identity = new(FP.Zero, FP.Zero, FP.Zero, FP.One);

    // ========================================================================
    // Constructors
    // ========================================================================

    /// <summary>
    /// Creates a quaternion with the given components.
    /// The quaternion will be normalized if you intend to use it for rotation.
    /// </summary>
    public Quaternion(FP x, FP y, FP z, FP w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    /// <summary>
    /// Creates a rotation quaternion from Euler angles (in degrees).
    /// Uses Unity-compatible ZXY rotation order.
    /// </summary>
    /// <param name="x">Rotation around X axis (pitch), in degrees.</param>
    /// <param name="y">Rotation around Y axis (yaw), in degrees.</param>
    /// <param name="z">Rotation around Z axis (roll), in degrees.</param>
    public static Quaternion Euler(FP x, FP y, FP z)
    {
        // Convert to radians
        FP halfX = x * FP.Deg2Rad * FP.Half;
        FP halfY = y * FP.Deg2Rad * FP.Half;
        FP halfZ = z * FP.Deg2Rad * FP.Half;

        FP cx = FP.Cos(halfX);
        FP sx = FP.Sin(halfX);
        FP cy = FP.Cos(halfY);
        FP sy = FP.Sin(halfY);
        FP cz = FP.Cos(halfZ);
        FP sz = FP.Sin(halfZ);

        // ZXY order: q = qY * qX * qZ
        // qY = (0, sy, 0, cy)
        // qX = (sx, 0, 0, cx)
        // qZ = (0, 0, sz, cz)
        //
        // Multiplying (qY * qX) * qZ:
        // qY * qX = (sy*sx, sy*cx, cy*sx, cy*cx) simplified... let's be explicit.

        // q1 = qY * qX
        FP q1x = cy * sx;
        FP q1y = sy * cx;
        FP q1z = -sy * sx;
        FP q1w = cy * cx;

        // result = q1 * qZ
        return new Quaternion(
            q1x * cz + q1y * sz,
            -q1x * sz + q1y * cz,
            q1z * cz + q1w * sz,
            -q1z * sz + q1w * cz
        );
    }

    /// <summary>
    /// Creates a rotation quaternion from an angle (in degrees) and an axis.
    /// The axis must be normalized.
    /// </summary>
    public static Quaternion AngleAxis(FP angleDegrees, Vector3 axis)
    {
        FP halfAngleRad = angleDegrees * FP.Deg2Rad * FP.Half;
        FP s = FP.Sin(halfAngleRad);
        FP c = FP.Cos(halfAngleRad);

        Vector3 n = axis.Normalized;
        return new Quaternion(n.X * s, n.Y * s, n.Z * s, c);
    }

    /// <summary>
    /// Creates a rotation that orients an object to look in <paramref name="forward"/>
    /// direction with <paramref name="upwards"/> as the up vector.
    /// </summary>
    public static Quaternion LookRotation(Vector3 forward, Vector3 upwards)
    {
        forward = forward.Normalized;
        if (forward == Vector3.Zero)
            return Identity;

        Vector3 right = Vector3.Cross(upwards, forward).Normalized;
        if (right == Vector3.Zero)
        {
            // Forward is parallel to upwards; pick an arbitrary perpendicular
            Vector3 arbitrary = FP.Abs(forward.X) < FP.Abs(forward.Z)
                ? new Vector3(FP.One, FP.Zero, FP.Zero)
                : new Vector3(FP.Zero, FP.Zero, -FP.One);
            right = Vector3.Cross(forward, arbitrary).Normalized;
        }
        Vector3 up = Vector3.Cross(forward, right).Normalized;

        // Build quaternion from rotation matrix columns
        return FromRotationMatrix(right, up, forward);
    }

    /// <summary>
    /// Creates a rotation that rotates from <paramref name="fromDirection"/>
    /// to <paramref name="toDirection"/>.
    /// </summary>
    public static Quaternion FromToRotation(Vector3 fromDirection, Vector3 toDirection)
    {
        fromDirection = fromDirection.Normalized;
        toDirection = toDirection.Normalized;

        if (fromDirection == Vector3.Zero || toDirection == Vector3.Zero)
            return Identity;

        FP dot = Vector3.Dot(fromDirection, toDirection);

        // Vectors are parallel
        if (dot > FP.One - FP.Epsilon * FP.FromInt(100))
            return Identity;

        // Vectors are opposite
        if (dot < FP.MinusOne + FP.Epsilon * FP.FromInt(100))
        {
            // Pick an arbitrary perpendicular axis
            Vector3 axis = FP.Abs(fromDirection.X) < FP.Abs(fromDirection.Z)
                ? new Vector3(FP.One, FP.Zero, FP.Zero)
                : new Vector3(FP.Zero, FP.Zero, -FP.One);
            axis = Vector3.Cross(fromDirection, axis).Normalized;
            return AngleAxis(FP.FromInt(180), axis);
        }

        Vector3 cross = Vector3.Cross(fromDirection, toDirection);
        FP s = FP.Sqrt((FP.One + dot) * FP.Two);
        FP invS = FP.One / s;

        return new Quaternion(
            cross.X * invS,
            cross.Y * invS,
            cross.Z * invS,
            s * FP.Half
        );
    }

    // ========================================================================
    // Private: Build quaternion from rotation matrix columns
    // ========================================================================

    private static Quaternion FromRotationMatrix(Vector3 columnX, Vector3 columnY, Vector3 columnZ)
    {
        // Matrix:
        // | r00 r01 r02 |   where columns are right, up, forward
        // | r10 r11 r12 |
        // | r20 r21 r22 |

        FP r00 = columnX.X, r10 = columnX.Y, r20 = columnX.Z;
        FP r01 = columnY.X, r11 = columnY.Y, r21 = columnY.Z;
        FP r02 = columnZ.X, r12 = columnZ.Y, r22 = columnZ.Z;

        FP trace = r00 + r11 + r22;

        FP qx, qy, qz, qw;

        if (trace > FP.Zero)
        {
            FP s = FP.Sqrt(trace + FP.One) * FP.Two;
            FP invS = FP.One / s;
            qw = s * FP.FromInt(1) / FP.FromInt(4);
            qx = (r21 - r12) * invS;
            qy = (r02 - r20) * invS;
            qz = (r10 - r01) * invS;
        }
        else if (r00 > r11 && r00 > r22)
        {
            FP s = FP.Sqrt(FP.One + r00 - r11 - r22) * FP.Two;
            FP invS = FP.One / s;
            qw = (r21 - r12) * invS;
            qx = s * FP.FromInt(1) / FP.FromInt(4);
            qy = (r01 + r10) * invS;
            qz = (r02 + r20) * invS;
        }
        else if (r11 > r22)
        {
            FP s = FP.Sqrt(FP.One + r11 - r00 - r22) * FP.Two;
            FP invS = FP.One / s;
            qw = (r02 - r20) * invS;
            qx = (r01 + r10) * invS;
            qy = s * FP.FromInt(1) / FP.FromInt(4);
            qz = (r12 + r21) * invS;
        }
        else
        {
            FP s = FP.Sqrt(FP.One + r22 - r00 - r11) * FP.Two;
            FP invS = FP.One / s;
            qw = (r10 - r01) * invS;
            qx = (r02 + r20) * invS;
            qy = (r12 + r21) * invS;
            qz = s * FP.FromInt(1) / FP.FromInt(4);
        }

        return new Quaternion(qx, qy, qz, qw).Normalized;
    }

    // ========================================================================
    // Properties
    // ========================================================================

    /// <summary>
    /// Returns the Euler angle representation of this quaternion (in degrees).
    /// Uses Unity-compatible ZXY extraction order.
    /// </summary>
    public Vector3 EulerAngles
    {
        get
        {
            // Extract Euler angles from rotation matrix R = R_Y(yaw) * R_X(pitch) * R_Z(roll)
            //
            // From the quaternion rotation matrix:
            //   R[1,2] = 2*y*z - 2*w*x → pitch = asin(-R[1,2]) = asin(2*(w*x - y*z))
            //   R[1,0]/R[1,1] = (2*x*y+2*w*z)/(1-2*x*x-2*z*z) → roll = atan2(x*y+w*z, 1-x*x-z*z) * 2
            //   R[0,2]/R[2,2] = (2*x*z+2*w*y)/(1-2*x*x-2*y*y) → yaw = atan2(x*z+w*y, 1-x*x-y*y) * 2

            // Pitch (X) = asin(2*(w*x - y*z))
            FP sinPitch = FP.Two * (W * X - Y * Z);
            FP pitch = FP.Asin(FP.Clamp(sinPitch, FP.MinusOne, FP.One));

            // Yaw (Y) = atan2(2*(w*y + x*z), 1 - 2*(x*x + y*y))
            FP sinYaw = FP.Two * (W * Y + X * Z);
            FP cosYaw = FP.One - FP.Two * (X * X + Y * Y);
            FP yaw = FP.Atan2(sinYaw, cosYaw);

            // Roll (Z) = atan2(2*(w*z + x*y), 1 - 2*(z*z + x*x))
            FP sinRoll = FP.Two * (W * Z + X * Y);
            FP cosRoll = FP.One - FP.Two * (Z * Z + X * X);
            FP roll = FP.Atan2(sinRoll, cosRoll);

            return new Vector3(
                pitch * FP.Rad2Deg,
                yaw * FP.Rad2Deg,
                roll * FP.Rad2Deg
            );
        }
    }

    /// <summary>
    /// Returns a normalized copy of this quaternion (magnitude = 1).
    /// Returns <see cref="Identity"/> if the quaternion is zero.
    /// </summary>
    public Quaternion Normalized
    {
        get
        {
            FP sqrMag = X * X + Y * Y + Z * Z + W * W;
            if (sqrMag == FP.Zero)
                return Identity;
            FP invMag = FP.One / FP.Sqrt(sqrMag);
            return new Quaternion(X * invMag, Y * invMag, Z * invMag, W * invMag);
        }
    }

    // ========================================================================
    // Operators
    // ========================================================================

    /// <summary>
    /// Combines two rotations. <c>a * b</c> applies rotation <c>b</c> first,
    /// then rotation <c>a</c> (like Unity: <c>lhs * rhs</c>).
    /// </summary>
    public static Quaternion operator *(Quaternion a, Quaternion b)
    {
        return new Quaternion(
            a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
            a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
            a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W,
            a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z
        );
    }

    /// <summary>
    /// Rotates a point by this quaternion.
    /// </summary>
    public static Vector3 operator *(Quaternion rotation, Vector3 point)
    {
        // Standard: q * v * q^(-1)
        // Optimized: v' = v + 2 * cross(q.xyz, cross(q.xyz, v) + q.w * v)

        FP twoX = FP.Two * rotation.X;
        FP twoY = FP.Two * rotation.Y;
        FP twoZ = FP.Two * rotation.Z;

        FP xx = rotation.X * twoX;
        FP yy = rotation.Y * twoY;
        FP zz = rotation.Z * twoZ;
        FP xy = rotation.X * twoY;
        FP xz = rotation.X * twoZ;
        FP yz = rotation.Y * twoZ;
        FP wx = rotation.W * twoX;
        FP wy = rotation.W * twoY;
        FP wz = rotation.W * twoZ;

        return new Vector3(
            (FP.One - (yy + zz)) * point.X + (xy - wz) * point.Y + (xz + wy) * point.Z,
            (xy + wz) * point.X + (FP.One - (xx + zz)) * point.Y + (yz - wx) * point.Z,
            (xz - wy) * point.X + (yz + wx) * point.Y + (FP.One - (xx + yy)) * point.Z
        );
    }

    /// <summary>Component-wise equality.</summary>
    public static bool operator ==(Quaternion a, Quaternion b)
    {
        return a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
    }

    /// <summary>Component-wise inequality.</summary>
    public static bool operator !=(Quaternion a, Quaternion b) => !(a == b);

    // ========================================================================
    // Static Methods
    // ========================================================================

    /// <summary>
    /// Returns the dot product of two quaternions.
    /// </summary>
    public static FP Dot(Quaternion a, Quaternion b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
    }

    /// <summary>
    /// Returns the angle in degrees between two quaternions.
    /// Result is always in [0, 180] degrees.
    /// </summary>
    public static FP Angle(Quaternion a, Quaternion b)
    {
        FP dot = FP.Clamp(FP.Abs(Dot(a, b)), FP.Zero, FP.One);
        return FP.Acos(dot) * FP.Two * FP.Rad2Deg;
    }

    /// <summary>
    /// Returns the inverse of the quaternion. For unit quaternions this equals
    /// the conjugate and represents the opposite rotation.
    /// </summary>
    public static Quaternion Inverse(Quaternion q)
    {
        FP sqrMag = q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W;
        if (sqrMag == FP.Zero)
            return Identity;
        FP invSqrMag = FP.One / sqrMag;
        return new Quaternion(-q.X * invSqrMag, -q.Y * invSqrMag, -q.Z * invSqrMag, q.W * invSqrMag);
    }

    /// <summary>
    /// Normalizes the given quaternion to unit length.
    /// </summary>
    public static Quaternion Normalize(Quaternion q) => q.Normalized;

    /// <summary>
    /// Spherically interpolates between two quaternions.
    /// <paramref name="t"/> is clamped to [0, 1].
    /// </summary>
    public static Quaternion Slerp(Quaternion a, Quaternion b, FP t)
    {
        t = FP.Clamp01(t);
        return SlerpUnclamped(a, b, t);
    }

    /// <summary>
    /// Spherically interpolates between two quaternions without clamping <paramref name="t"/>.
    /// </summary>
    public static Quaternion SlerpUnclamped(Quaternion a, Quaternion b, FP t)
    {
        FP dot = Dot(a, b);

        // If the dot product is negative, negate one quaternion to take
        // the shorter path
        Quaternion bAdjusted = b;
        if (dot < FP.Zero)
        {
            dot = -dot;
            bAdjusted = new Quaternion(-b.X, -b.Y, -b.Z, -b.W);
        }
        if (dot > FP.One - FP.Epsilon * FP.FromInt(100))
        {
            // Lerp and normalize
            Quaternion lerped = new Quaternion(
                FP.LerpUnclamped(a.X, bAdjusted.X, t),
                FP.LerpUnclamped(a.Y, bAdjusted.Y, t),
                FP.LerpUnclamped(a.Z, bAdjusted.Z, t),
                FP.LerpUnclamped(a.W, bAdjusted.W, t)
            );
            return lerped.Normalized;
        }

        FP angleRad = FP.Acos(dot);
        FP sinAngle = FP.Sin(angleRad);
        FP invSinAngle = FP.One / sinAngle;

        FP weightA = FP.Sin((FP.One - t) * angleRad) * invSinAngle;
        FP weightB = FP.Sin(t * angleRad) * invSinAngle;

        return new Quaternion(
            weightA * a.X + weightB * bAdjusted.X,
            weightA * a.Y + weightB * bAdjusted.Y,
            weightA * a.Z + weightB * bAdjusted.Z,
            weightA * a.W + weightB * bAdjusted.W
        );
    }

    /// <summary>
    /// Linearly interpolates between two quaternions.
    /// <paramref name="t"/> is clamped to [0, 1]. Result is NOT normalized.
    /// Use <see cref="Slerp"/> for constant angular speed.
    /// </summary>
    public static Quaternion Lerp(Quaternion a, Quaternion b, FP t)
    {
        t = FP.Clamp01(t);
        return LerpUnclamped(a, b, t);
    }

    /// <summary>
    /// Linearly interpolates between two quaternions without clamping <paramref name="t"/>.
    /// Result is NOT normalized. Use <see cref="SlerpUnclamped"/> for constant angular speed.
    /// </summary>
    public static Quaternion LerpUnclamped(Quaternion a, Quaternion b, FP t)
    {
        // Take the shortest path
        FP dot = Dot(a, b);
        if (dot < FP.Zero)
        {
            b = new Quaternion(-b.X, -b.Y, -b.Z, -b.W);
        }

        return new Quaternion(
            FP.LerpUnclamped(a.X, b.X, t),
            FP.LerpUnclamped(a.Y, b.Y, t),
            FP.LerpUnclamped(a.Z, b.Z, t),
            FP.LerpUnclamped(a.W, b.W, t)
        );
    }

    /// <summary>
    /// Rotates <paramref name="from"/> towards <paramref name="to"/> by at most
    /// <paramref name="maxDegreesDelta"/> degrees.
    /// </summary>
    public static Quaternion RotateTowards(Quaternion from, Quaternion to, FP maxDegreesDelta)
    {
        FP angle = Angle(from, to);
        if (angle == FP.Zero)
            return to;

        FP t = FP.Min(FP.One, maxDegreesDelta / angle);
        return SlerpUnclamped(from, to, t);
    }

    // ========================================================================
    // Equality & Hashing
    // ========================================================================

    /// <inheritdoc />
    public bool Equals(Quaternion other) => this == other;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Quaternion other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);

    // ========================================================================
    // String Representation
    // ========================================================================

    /// <summary>
    /// Returns a string representation of this quaternion in the format "(X, Y, Z, W)".
    /// Each component uses 6 decimal places.
    /// </summary>
    public override string ToString()
    {
        return $"({X}, {Y}, {Z}, {W})";
    }

    /// <summary>
    /// Parses a string representation into a <see cref="Quaternion"/>.
    /// </summary>
    public static Quaternion Parse(string s)
    {
        if (!TryParse(s, out Quaternion result))
            throw new FormatException($"Cannot parse '{s}' as a Quaternion.");
        return result;
    }

    /// <summary>
    /// Attempts to parse a string representation into a <see cref="Quaternion"/>.
    /// </summary>
    public static bool TryParse(string s, out Quaternion result)
    {
        result = Identity;
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

        result = new Quaternion(x, y, z, w);
        return true;
    }
}
}
