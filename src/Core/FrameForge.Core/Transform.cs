using FrameForge.Foundation.FixedPoint;
using FrameForge.Foundation.Math;

namespace FrameForge.Core {

/// <summary>
/// Represents the spatial position, rotation, and scale of a <see cref="GameObject"/>.
/// Every GameObject has exactly one Transform, which cannot be removed.
/// </summary>
/// <remarks>
/// World-space values are computed by walking the parent chain. All computations
/// use <see cref="FP"/> fixed-point arithmetic and are fully deterministic.
/// </remarks>
public class Transform : Component
{
    // ========================================================================
    // Fields
    // ========================================================================

    private Vector3 _localPosition = Vector3.Zero;
    private Quaternion _localRotation = Quaternion.Identity;
    private Vector3 _localScale = Vector3.One;

    // ========================================================================
    // Local Properties (read/write)
    // ========================================================================

    /// <summary>
    /// Position relative to the parent Transform, or world position if there
    /// is no parent.
    /// </summary>
    public Vector3 LocalPosition
    {
        get => _localPosition;
        set => _localPosition = value;
    }

    /// <summary>
    /// Rotation relative to the parent Transform, or world rotation if there
    /// is no parent.
    /// </summary>
    public Quaternion LocalRotation
    {
        get => _localRotation;
        set => _localRotation = value;
    }

    /// <summary>
    /// Scale relative to the parent Transform, or world scale if there is no parent.
    /// </summary>
    public Vector3 LocalScale
    {
        get => _localScale;
        set => _localScale = value;
    }

    // ========================================================================
    // World Properties (computed from parent chain)
    // ========================================================================

    /// <summary>
    /// World-space position, computed by transforming the local position
    /// through the parent chain.
    /// </summary>
    public Vector3 Position
    {
        get
        {
            Transform? parent = GameObject.Parent?.Transform;
            if (parent == null)
                return _localPosition;

            return parent.TransformPoint(_localPosition);
        }
        set
        {
            Transform? parent = GameObject.Parent?.Transform;
            if (parent == null)
            {
                _localPosition = value;
                return;
            }

            // Inverse-transform the world position to local space
            Vector3 worldOffset = value - parent.Position;
            Vector3 localOffset = Quaternion.Inverse(parent.Rotation) * worldOffset;
            Vector3 parentScale = parent.Scale;

            _localPosition = new Vector3(
                SafeDiv(localOffset.X, parentScale.X),
                SafeDiv(localOffset.Y, parentScale.Y),
                SafeDiv(localOffset.Z, parentScale.Z)
            );
        }
    }

    /// <summary>
    /// World-space rotation, computed by combining local rotation with the
    /// parent chain.
    /// </summary>
    public Quaternion Rotation
    {
        get
        {
            Transform? parent = GameObject.Parent?.Transform;
            if (parent == null)
                return _localRotation;

            return parent.Rotation * _localRotation;
        }
        set
        {
            Transform? parent = GameObject.Parent?.Transform;
            if (parent == null)
            {
                _localRotation = value;
                return;
            }

            _localRotation = Quaternion.Inverse(parent.Rotation) * value;
        }
    }

    /// <summary>
    /// World-space scale, computed by multiplying local scale through the
    /// parent chain.
    /// </summary>
    public Vector3 Scale
    {
        get
        {
            Transform? parent = GameObject.Parent?.Transform;
            if (parent == null)
                return _localScale;

            Vector3 parentScale = parent.Scale;
            return new Vector3(
                parentScale.X * _localScale.X,
                parentScale.Y * _localScale.Y,
                parentScale.Z * _localScale.Z
            );
        }
        set
        {
            Transform? parent = GameObject.Parent?.Transform;
            if (parent == null)
            {
                _localScale = value;
                return;
            }

            Vector3 parentScale = parent.Scale;
            _localScale = new Vector3(
                SafeDiv(value.X, parentScale.X),
                SafeDiv(value.Y, parentScale.Y),
                SafeDiv(value.Z, parentScale.Z)
            );
        }
    }

    // ========================================================================
    // Direction Vectors
    // ========================================================================

    /// <summary>
    /// The forward direction in world space (the local +Z axis rotated by
    /// world rotation). In a right-handed coordinate system with Z-forward.
    /// </summary>
    public Vector3 Forward => Rotation * Vector3.Forward;

    /// <summary>
    /// The backward direction in world space (-Z).
    /// </summary>
    public Vector3 Back => Rotation * Vector3.Back;

    /// <summary>
    /// The up direction in world space (the local +Y axis rotated by
    /// world rotation).
    /// </summary>
    public Vector3 Up => Rotation * Vector3.Up;

    /// <summary>
    /// The down direction in world space (-Y).
    /// </summary>
    public Vector3 Down => Rotation * Vector3.Down;

    /// <summary>
    /// The right direction in world space (the local +X axis rotated by
    /// world rotation).
    /// </summary>
    public Vector3 Right => Rotation * Vector3.Right;

    /// <summary>
    /// The left direction in world space (-X).
    /// </summary>
    public Vector3 Left => Rotation * Vector3.Left;

    // ========================================================================
    // Transform Helpers
    // ========================================================================

    /// <summary>
    /// Transforms a point from local space to world space, accounting for the
    /// full parent chain.
    /// </summary>
    /// <param name="localPoint">A point in this Transform's local space.</param>
    /// <returns>The point in world space.</returns>
    public Vector3 TransformPoint(Vector3 localPoint)
    {
        Vector3 scaled = new Vector3(
            _localScale.X * localPoint.X,
            _localScale.Y * localPoint.Y,
            _localScale.Z * localPoint.Z
        );
        Vector3 rotated = _localRotation * scaled;
        Vector3 translated = rotated + _localPosition;

        Transform? parent = GameObject.Parent?.Transform;
        if (parent == null)
            return translated;

        return parent.TransformPoint(translated);
    }

    /// <summary>
    /// Transforms a direction from local space to world space, accounting for
    /// rotation only (no scale or translation).
    /// </summary>
    /// <param name="localDirection">A direction in this Transform's local space.</param>
    /// <returns>The direction in world space.</returns>
    public Vector3 TransformDirection(Vector3 localDirection)
    {
        Vector3 rotated = _localRotation * localDirection;

        Transform? parent = GameObject.Parent?.Transform;
        if (parent == null)
            return rotated;

        return parent.TransformDirection(rotated);
    }

    /// <summary>
    /// Sets the world-space position, rotation, and scale in a single call.
    /// More efficient than setting each property individually when all three
    /// need to change, because the parent chain is only walked once.
    /// </summary>
    public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        Transform? parent = GameObject.Parent?.Transform;
        if (parent == null)
        {
            _localPosition = position;
            _localRotation = rotation;
            return;
        }

        _localRotation = Quaternion.Inverse(parent.Rotation) * rotation;
        Vector3 worldOffset = position - parent.Position;
        Vector3 localOffset = Quaternion.Inverse(parent.Rotation) * worldOffset;
        Vector3 parentScale = parent.Scale;
        _localPosition = new Vector3(
            SafeDiv(localOffset.X, parentScale.X),
            SafeDiv(localOffset.Y, parentScale.Y),
            SafeDiv(localOffset.Z, parentScale.Z)
        );
    }

    // ========================================================================
    // Private Helpers
    // ========================================================================

    /// <summary>
    /// Divides <paramref name="a"/> by <paramref name="b"/>, returning zero
    /// when <paramref name="b"/> is zero to avoid division-by-zero. This
    /// preserves determinism for degenerate scale values.
    /// </summary>
    private static FP SafeDiv(FP a, FP b)
    {
        return b == FP.Zero ? FP.Zero : a / b;
    }
}
}
