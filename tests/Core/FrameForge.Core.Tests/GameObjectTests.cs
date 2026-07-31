using FrameForge.Foundation.FixedPoint;

namespace FrameForge.Core.Tests {

/// <summary>
/// Tests for <see cref="GameObject"/> creation, component management,
/// hierarchy, activate/deactivate, and destroy.
/// </summary>
public class GameObjectTests
{
    private class TestComponent : Component
    {
        public int UpdateCount;
        public override void Update() { UpdateCount++; }
    }

    // ========================================================================
    // Creation
    // ========================================================================

    [Fact]
    public void CreateGameObject_HasUniqueId()
    {
        Scene scene = new();
        GameObject a = scene.CreateGameObject();
        GameObject b = scene.CreateGameObject();

        Assert.NotEqual(a.Id, b.Id);
        Assert.True(a.Id < b.Id);
    }

    [Fact]
    public void CreateGameObject_HasDefaultName()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();

        Assert.Equal("GameObject", obj.Name);
    }

    [Fact]
    public void CreateGameObject_WithCustomName()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject("Player");

        Assert.Equal("Player", obj.Name);
    }

    [Fact]
    public void CreateGameObject_ActiveDefaultsTrue()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();

        Assert.True(obj.Active);
    }

    [Fact]
    public void CreateGameObject_HasSceneReference()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();

        Assert.Same(scene, obj.Scene);
    }

    [Fact]
    public void CreateGameObject_HasTransform()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();

        Assert.NotNull(obj.Transform);
    }

    // ========================================================================
    // Component Management
    // ========================================================================

    [Fact]
    public void AddComponent_ReturnsNewInstance()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        TestComponent comp = obj.AddComponent<TestComponent>();

        Assert.NotNull(comp);
        Assert.Same(obj, comp.GameObject);
    }

    [Fact]
    public void GetComponent_FindsAddedComponent()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        TestComponent added = obj.AddComponent<TestComponent>();

        TestComponent? found = obj.GetComponent<TestComponent>();
        Assert.Same(added, found);
    }

    [Fact]
    public void GetComponents_ReturnsAllOfType()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        obj.AddComponent<TestComponent>();
        obj.AddComponent<TestComponent>();
        obj.AddComponent<TestComponent>();

        List<TestComponent> all = obj.GetComponents<TestComponent>();
        Assert.Equal(3, all.Count);
    }

    [Fact]
    public void RemoveComponent_RemovesAndReturnsTrue()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        obj.AddComponent<TestComponent>();

        bool removed = obj.RemoveComponent<TestComponent>();
        Assert.True(removed);

        Assert.Null(obj.GetComponent<TestComponent>());
    }

    [Fact]
    public void RemoveComponent_NotFound_ReturnsFalse()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();

        bool removed = obj.RemoveComponent<TestComponent>();
        Assert.False(removed);
    }

    [Fact]
    public void AddComponent_ToDestroyed_Throws()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        obj.Destroy();
        scene.Update(); // Process destroy

        Assert.Throws<InvalidOperationException>(() => obj.AddComponent<TestComponent>());
    }

    // ========================================================================
    // Active / Inactive
    // ========================================================================

    [Fact]
    public void InactiveObject_DoesNotUpdateComponents()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();
        TestComponent comp = obj.AddComponent<TestComponent>();
        obj.Active = false;

        scene.Update();

        Assert.Equal(0, comp.UpdateCount);
    }

    [Fact]
    public void InactiveObject_ChildrenDoNotUpdate()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("Parent");
        TestComponent parentComp = parent.AddComponent<TestComponent>();

        GameObject child = scene.CreateGameObject("Child");
        child.SetParent(parent);
        TestComponent childComp = child.AddComponent<TestComponent>();

        parent.Active = false;

        scene.Update();

        Assert.Equal(0, parentComp.UpdateCount);
        Assert.Equal(0, childComp.UpdateCount);
    }

    // ========================================================================
    // Hierarchy
    // ========================================================================

    [Fact]
    public void SetParent_EstablishesParentChildRelationship()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("Parent");
        GameObject child = scene.CreateGameObject("Child");

        child.SetParent(parent);

        Assert.Same(parent, child.Parent);
        Assert.Contains(child, parent.Children);
    }

    [Fact]
    public void SetParent_Null_Unparents()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("Parent");
        GameObject child = scene.CreateGameObject("Child");
        child.SetParent(parent);

        child.SetParent(null);

        Assert.Null(child.Parent);
        Assert.DoesNotContain(child, parent.Children);
    }

    [Fact]
    public void SetParent_CircularReference_Throws()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("Parent");
        GameObject child = scene.CreateGameObject("Child");
        child.SetParent(parent);

        Assert.Throws<InvalidOperationException>(() => parent.SetParent(child));
    }

    [Fact]
    public void SetParent_SelfParent_Throws()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();

        Assert.Throws<InvalidOperationException>(() => obj.SetParent(obj));
    }

    [Fact]
    public void FindChild_ByName_Works()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("Parent");
        GameObject child = scene.CreateGameObject("ChildA");
        child.SetParent(parent);

        GameObject? found = parent.FindChild("ChildA");
        Assert.Same(child, found);
    }

    [Fact]
    public void FindChild_NotFound_ReturnsNull()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("Parent");

        Assert.Null(parent.FindChild("NonExistent"));
    }

    [Fact]
    public void Children_OrderedByParenting()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("Parent");
        GameObject a = scene.CreateGameObject("A");
        GameObject b = scene.CreateGameObject("B");
        GameObject c = scene.CreateGameObject("C");

        a.SetParent(parent);
        b.SetParent(parent);
        c.SetParent(parent);

        Assert.Equal(3, parent.Children.Count);
        Assert.Same(a, parent.Children[0]);
        Assert.Same(b, parent.Children[1]);
        Assert.Same(c, parent.Children[2]);
    }

    // ========================================================================
    // Destroy Behavior
    // ========================================================================

    [Fact]
    public void Destroy_MarksAsDestroyed()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();

        Assert.False(obj.IsDestroyed);
        obj.Destroy();
        Assert.True(obj.IsDestroyed);
    }

    [Fact]
    public void Destroy_RecursivelyDestroysChildren()
    {
        Scene scene = new();
        GameObject parent = scene.CreateGameObject("Parent");
        GameObject child = scene.CreateGameObject("Child");
        child.SetParent(parent);

        parent.Destroy();

        Assert.True(parent.IsDestroyed);
        Assert.True(child.IsDestroyed);
    }

    [Fact]
    public void Destroy_DoubleCall_IsSafe()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject();

        obj.Destroy();
        obj.Destroy(); // Should not throw

        Assert.True(obj.IsDestroyed);
    }

    [Fact]
    public void Destroy_ObjectStillAccessible_SameFrame()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject("Test");
        obj.Destroy();

        // Still findable this frame
        GameObject? found = scene.FindByName("Test");
        Assert.Same(obj, found);
    }

    [Fact]
    public void Destroy_ObjectRemoved_NextFrame()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject("Test");
        obj.Destroy();
        scene.Update(); // Process destroy

        Assert.Null(scene.FindByName("Test"));
        Assert.Equal(0, scene.ObjectCount);
    }

    // ========================================================================
    // Name/Tag Index Updates
    // ========================================================================

    [Fact]
    public void Name_Change_UpdatesIndex()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject("OldName");

        Assert.Same(obj, scene.FindByName("OldName"));

        obj.Name = "NewName";

        Assert.Null(scene.FindByName("OldName"));
        Assert.Same(obj, scene.FindByName("NewName"));
    }

    [Fact]
    public void Tag_Change_UpdatesIndex()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject("Test");
        obj.Tag = "OldTag";

        Assert.Single(scene.FindByTag("OldTag"));

        obj.Tag = "NewTag";

        Assert.Empty(scene.FindByTag("OldTag"));
        Assert.Single(scene.FindByTag("NewTag"));
    }
}
}
