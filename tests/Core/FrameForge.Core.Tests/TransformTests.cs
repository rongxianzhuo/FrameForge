using FrameForge.Foundation.FixedPoint;
using FrameForge.Foundation.Math;

namespace FrameForge.Core.Tests;

/// <summary>
/// Tests for <see cref="Transform"/> local/world position, rotation, scale,
/// direction vectors, and hierarchical transform propagation.
/// </summary>
public class TransformTests
{
    // ========================================================================
    // Local Properties
    // ========================================================================

    [Fact]
    public void LocalPosition_DefaultsToZero()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        Assert.Equal(Vector3.Zero, obj.Transform.LocalPosition);
    }

    [Fact]
    public void LocalRotation_DefaultsToIdentity()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        Assert.Equal(Quaternion.Identity, obj.Transform.LocalRotation);
    }

    [Fact]
    public void LocalScale_DefaultsToOne()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        Assert.Equal(Vector3.One, obj.Transform.LocalScale);
    }

    [Fact]
    public void LocalPosition_SetGet_RoundTrip()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        Vector3 pos = new(FP.FromInt(3), FP.FromInt(4), FP.FromInt(5));

        obj.Transform.LocalPosition = pos;
        Assert.Equal(pos, obj.Transform.LocalPosition);
    }

    // ========================================================================
    // World Properties (No Parent)
    // ========================================================================

    [Fact]
    public void Position_NoParent_EqualsLocalPosition()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        obj.Transform.LocalPosition = new Vector3(FP.One, FP.Two, FP.FromInt(3));

        Assert.Equal(obj.Transform.LocalPosition, obj.Transform.Position);
    }

    [Fact]
    public void Position_Set_NoParent_SetsLocalPosition()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        Vector3 worldPos = new(FP.FromInt(10), FP.FromInt(20), FP.FromInt(30));

        obj.Transform.Position = worldPos;
        Assert.Equal(worldPos, obj.Transform.LocalPosition);
    }

    [Fact]
    public void Rotation_NoParent_EqualsLocalRotation()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        Quaternion rot = Quaternion.Euler(FP.FromInt(45), FP.FromInt(30), FP.FromInt(60));

        obj.Transform.LocalRotation = rot;
        obj.Transform.Rotation.ShouldBeApproximately(rot);
    }

    [Fact]
    public void Scale_NoParent_EqualsLocalScale()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        Vector3 scale = new(FP.FromInt(2), FP.FromInt(3), FP.FromInt(4));

        obj.Transform.LocalScale = scale;
        Assert.Equal(scale, obj.Transform.Scale);
    }

    // ========================================================================
    // World Properties (With Parent)
    // ========================================================================

    [Fact]
    public void Position_WithParent_TransformedByParent()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("Parent");
        parent.Transform.Position = new Vector3(FP.FromInt(5), FP.Zero, FP.Zero);

        GameObject child = scene.CreateGameObject("Child");
        child.SetParent(parent);
        child.Transform.LocalPosition = new Vector3(FP.FromInt(3), FP.Zero, FP.Zero);

        // Child world position = parent position + local position (no rotation)
        child.Transform.Position.ShouldBeApproximately(new Vector3(FP.FromInt(8), FP.Zero, FP.Zero));
    }

    [Fact]
    public void Position_WithRotatedParent_TransformedCorrectly()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("Parent");
        parent.Transform.Position = new Vector3(FP.FromInt(1), FP.Zero, FP.Zero);
        parent.Transform.Rotation = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero); // Yaw 90

        GameObject child = scene.CreateGameObject("Child");
        child.SetParent(parent);
        child.Transform.LocalPosition = new Vector3(FP.One, FP.Zero, FP.Zero); // 1 unit in local X

        // After 90° yaw, local X becomes world -Z
        // World position = (1, 0, 0) + (-1, 0, 0)... wait.
        // Actually, parent at (1,0,0), rotation 90° around Y.
        // Child local (1,0,0) after parent rotation: local X(1,0,0) rotated 90° Y → (0, 0, -1)
        // World = parent.position + rotated local = (1, 0, 0) + (0, 0, -1) = (1, 0, -1)
        child.Transform.Position.ShouldBeApproximately(
            new Vector3(FP.One, FP.Zero, FP.MinusOne));
    }

    [Fact]
    public void Rotation_WithParent_Combined()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("Parent");
        parent.Transform.Rotation = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero);

        GameObject child = scene.CreateGameObject("Child");
        child.SetParent(parent);
        child.Transform.LocalRotation = Quaternion.Euler(FP.Zero, FP.FromInt(45), FP.Zero);

        // World rotation = parent (90) * local (45) = 135 around Y
        Vector3 euler = child.Transform.Rotation.EulerAngles;
        // Euler extraction from combined quaternions accumulates fixed-point error;
        // use a generous tolerance
        FP looseTol = FP.Epsilon * FP.FromInt(500000);
        euler.Y.ShouldBeApproximately(FP.FromInt(135), looseTol);
    }

    [Fact]
    public void Scale_WithParent_Multiplied()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("Parent");
        parent.Transform.LocalScale = new Vector3(FP.Two, FP.FromInt(3), FP.FromInt(4));

        GameObject child = scene.CreateGameObject("Child");
        child.SetParent(parent);
        child.Transform.LocalScale = new Vector3(FP.FromInt(5), FP.FromInt(6), FP.FromInt(7));

        Vector3 worldScale = child.Transform.Scale;
        worldScale.X.ShouldBeApproximately(FP.FromInt(10));  // 2 * 5
        worldScale.Y.ShouldBeApproximately(FP.FromInt(18));  // 3 * 6
        worldScale.Z.ShouldBeApproximately(FP.FromInt(28));  // 4 * 7
    }

    // ========================================================================
    // Setting World Properties (With Parent)
    // ========================================================================

    [Fact]
    public void Position_Set_WithParent_AdjustsLocal()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("Parent");
        parent.Transform.Position = new Vector3(FP.FromInt(5), FP.Zero, FP.Zero);

        GameObject child = scene.CreateGameObject("Child");
        child.SetParent(parent);

        // Set world position
        child.Transform.Position = new Vector3(FP.FromInt(8), FP.Zero, FP.Zero);

        // Local should be (8 - 5) = 3
        child.Transform.LocalPosition.ShouldBeApproximately(new Vector3(FP.FromInt(3), FP.Zero, FP.Zero));
    }

    // ========================================================================
    // Direction Vectors
    // ========================================================================

    [Fact]
    public void Forward_Identity_ReturnsZ()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        obj.Transform.Forward.ShouldBeApproximately(Vector3.Forward);
    }

    [Fact]
    public void Forward_Rotated90Yaw_ReturnsNegX()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        obj.Transform.Rotation = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero);

        // Forward (0,0,1) rotated 90° around Y → (-1, 0, 0) ... wait
        // Yaw 90: rotation around Y axis. Forward (Z) rotates to X? Let me think.
        // Z-forward, Y-up, right-handed. Yaw 90° means rotate around Y.
        // (0,0,1) rotated 90° around Y → (1, 0, 0). That's right.
        obj.Transform.Forward.ShouldBeApproximately(Vector3.Right);
    }

    [Fact]
    public void Up_Identity_ReturnsY()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        obj.Transform.Up.ShouldBeApproximately(Vector3.Up);
    }

    [Fact]
    public void Right_Identity_ReturnsX()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        obj.Transform.Right.ShouldBeApproximately(Vector3.Right);
    }

    [Fact]
    public void Back_Down_Left_AllOpposites()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();

        obj.Transform.Back.ShouldBeApproximately(Vector3.Back);
        obj.Transform.Down.ShouldBeApproximately(Vector3.Down);
        obj.Transform.Left.ShouldBeApproximately(Vector3.Left);
    }

    // ========================================================================
    // TransformPoint / TransformDirection
    // ========================================================================

    [Fact]
    public void TransformPoint_NoParent_LocalIsWorld()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        obj.Transform.LocalPosition = new Vector3(FP.FromInt(10), FP.Zero, FP.Zero);

        Vector3 world = obj.Transform.TransformPoint(Vector3.Zero);
        world.ShouldBeApproximately(new Vector3(FP.FromInt(10), FP.Zero, FP.Zero));
    }

    [Fact]
    public void TransformPoint_WithParent_Accumulates()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("P");
        parent.Transform.Position = new Vector3(FP.FromInt(5), FP.Zero, FP.Zero);

        GameObject child = scene.CreateGameObject("C");
        child.SetParent(parent);
        child.Transform.LocalPosition = new Vector3(FP.FromInt(3), FP.Zero, FP.Zero);

        Vector3 world = child.Transform.TransformPoint(Vector3.Zero);
        world.ShouldBeApproximately(new Vector3(FP.FromInt(8), FP.Zero, FP.Zero));
    }

    [Fact]
    public void TransformDirection_NoParent_PreservesDirection()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        obj.Transform.Rotation = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero);

        Vector3 worldDir = obj.Transform.TransformDirection(Vector3.Forward);
        worldDir.ShouldBeApproximately(Vector3.Right);
    }

    // ========================================================================
    // SetPositionAndRotation
    // ========================================================================

    [Fact]
    public void SetPositionAndRotation_NoParent_SetsBoth()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        Vector3 pos = new(FP.FromInt(1), FP.FromInt(2), FP.FromInt(3));
        Quaternion rot = Quaternion.Euler(FP.FromInt(90), FP.Zero, FP.Zero);

        obj.Transform.SetPositionAndRotation(pos, rot);

        obj.Transform.Position.ShouldBeApproximately(pos);
        obj.Transform.Rotation.ShouldBeApproximately(rot);
    }

    // ========================================================================
    // Transform is always present and cannot be removed
    // ========================================================================

    [Fact]
    public void Transform_AlwaysPresent()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();

        Assert.NotNull(obj.Transform);
        Assert.Same(obj.Transform, obj.GetComponent<Transform>());
    }

    [Fact]
    public void Transform_CannotAddSecond()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();

        Assert.Throws<InvalidOperationException>(() => obj.AddComponent<Transform>());
    }

    [Fact]
    public void Transform_CannotRemove()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();

        Assert.Throws<InvalidOperationException>(() => obj.RemoveComponent<Transform>());
    }

    // ========================================================================
    // Deep Hierarchy
    // ========================================================================

    [Fact]
    public void ThreeLevelHierarchy_PositionAccumulates()
    {
        Scene scene = new();
        GameObject root = scene.CreateGameObject("Root");
        root.Transform.Position = new Vector3(FP.One, FP.Zero, FP.Zero);

        GameObject child = scene.CreateGameObject("Child");
        child.SetParent(root);
        child.Transform.LocalPosition = new Vector3(FP.One, FP.Zero, FP.Zero);

        GameObject grandchild = scene.CreateGameObject("Grandchild");
        grandchild.SetParent(child);
        grandchild.Transform.LocalPosition = new Vector3(FP.One, FP.Zero, FP.Zero);

        grandchild.Transform.Position.ShouldBeApproximately(
            new Vector3(FP.FromInt(3), FP.Zero, FP.Zero));
    }

    [Fact]
    public void ThreeLevelHierarchy_ScaleMultiplies()
    {
        Scene scene = new();
        GameObject root = scene.CreateGameObject("Root");
        root.Transform.LocalScale = new Vector3(FP.Two, FP.Two, FP.Two);

        GameObject child = scene.CreateGameObject("Child");
        child.SetParent(root);
        child.Transform.LocalScale = new Vector3(FP.FromInt(3), FP.FromInt(3), FP.FromInt(3));

        GameObject grandchild = scene.CreateGameObject("Grandchild");
        grandchild.SetParent(child);
        grandchild.Transform.LocalScale = new Vector3(FP.FromInt(5), FP.FromInt(5), FP.FromInt(5));

        Vector3 scale = grandchild.Transform.Scale;
        scale.X.ShouldBeApproximately(FP.FromInt(30));  // 2 * 3 * 5
    }

    // ========================================================================
    // SafeDiv: zero scale handling
    // ========================================================================

    [Fact]
    public void Scale_ZeroParentScale_SettingWorldPositionDoesNotThrow()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("Parent");
        parent.Transform.LocalScale = Vector3.Zero;

        GameObject child = scene.CreateGameObject("Child");
        child.SetParent(parent);

        // Setting world position with zero parent scale should not throw
        child.Transform.Position = new Vector3(FP.One, FP.One, FP.One);
        Assert.Equal(FP.Zero, child.Transform.LocalPosition.X);
    }

    // ========================================================================
    // Setting World Rotation & Scale (With Parent)
    // ========================================================================

    [Fact]
    public void Rotation_Set_WithParent_AdjustsLocal()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("Parent");
        parent.Transform.Rotation = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero);

        GameObject child = scene.CreateGameObject("Child");
        child.SetParent(parent);

        // Set world rotation to identity
        child.Transform.Rotation = Quaternion.Identity;

        // Local should be inverse of parent = -90° Y
        FP looseTol = FP.Epsilon * FP.FromInt(500000);
        child.Transform.LocalRotation.EulerAngles.Y.ShouldBeApproximately(FP.FromInt(-90), looseTol);
    }

    [Fact]
    public void Scale_Set_WithParent_AdjustsLocal()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("Parent");
        parent.Transform.LocalScale = new Vector3(FP.Two, FP.Two, FP.Two);

        GameObject child = scene.CreateGameObject("Child");
        child.SetParent(parent);

        // Set world scale to (4, 6, 8)
        child.Transform.Scale = new Vector3(FP.FromInt(4), FP.FromInt(6), FP.FromInt(8));

        // Local should be world / parent = (2, 3, 4)
        child.Transform.LocalScale.ShouldBeApproximately(
            new Vector3(FP.FromInt(2), FP.FromInt(3), FP.FromInt(4)));
    }

    [Fact]
    public void SetPositionAndRotation_WithParent_Works()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("Parent");
        parent.Transform.Position = new Vector3(FP.FromInt(5), FP.Zero, FP.Zero);

        GameObject child = scene.CreateGameObject("Child");
        child.SetParent(parent);

        child.Transform.SetPositionAndRotation(
            new Vector3(FP.FromInt(10), FP.Zero, FP.Zero),
            Quaternion.Identity);

        // Local position should be (5, 0, 0) since parent is at 5
        child.Transform.LocalPosition.ShouldBeApproximately(
            new Vector3(FP.FromInt(5), FP.Zero, FP.Zero));
    }

    // ========================================================================
    // TransformPoint with scale & rotation
    // ========================================================================

    [Fact]
    public void TransformPoint_WithScaleAndRotation()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        obj.Transform.LocalScale = new Vector3(FP.Two, FP.One, FP.One);
        obj.Transform.LocalRotation = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero);

        // Point (1, 0, 0) in local: scaled to (2, 0, 0), rotated 90° Y → (0, 0, -2)
        Vector3 world = obj.Transform.TransformPoint(new Vector3(FP.One, FP.Zero, FP.Zero));
        world.ShouldBeApproximately(new Vector3(FP.Zero, FP.Zero, FP.MinusOne * FP.Two));
    }

    // ========================================================================
    // TransformDirection with parent hierarchy
    // ========================================================================

    [Fact]
    public void TransformDirection_WithParent_Accumulates()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("P");
        parent.Transform.Rotation = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero);

        GameObject child = scene.CreateGameObject("C");
        child.SetParent(parent);
        child.Transform.LocalRotation = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero);

        // Forward (0,0,1): child rotates to (1,0,0), parent rotates to (0,0,-1)
        Vector3 worldDir = child.Transform.TransformDirection(Vector3.Forward);
        worldDir.ShouldBeApproximately(Vector3.Back);
    }

    // ========================================================================
    // Direction vectors with parent
    // ========================================================================

    [Fact]
    public void Forward_WithRotatedParent()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("P");
        parent.Transform.Rotation = Quaternion.Euler(FP.Zero, FP.FromInt(90), FP.Zero);

        GameObject child = scene.CreateGameObject("C");
        child.SetParent(parent);

        // Child has no local rotation, but parent rotates 90° Y
        // Child's world forward = (0,0,1) rotated by parent's 90° Y = (1, 0, 0)
        child.Transform.Forward.ShouldBeApproximately(Vector3.Right);
    }

    [Fact]
    public void Up_WithRotatedParent()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("P");
        parent.Transform.Rotation = Quaternion.Euler(FP.FromInt(90), FP.Zero, FP.Zero); // Pitch 90

        GameObject child = scene.CreateGameObject("C");
        child.SetParent(parent);

        // Child's world Up = (0,1,0) rotated by parent's 90° X pitch ≈ (0, 0, 1)
        child.Transform.Up.ShouldBeApproximately(Vector3.Forward);
    }
}
