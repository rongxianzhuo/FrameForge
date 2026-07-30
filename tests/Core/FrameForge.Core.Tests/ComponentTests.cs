namespace FrameForge.Core.Tests;

/// <summary>
/// Tests for <see cref="Component"/> lifecycle, properties, and GameObject integration.
/// </summary>
public class ComponentTests
{
    [Fact]
    public void Component_AddedToGameObject_HasBackReference()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject("Test");
        TestComponent comp = obj.AddComponent<TestComponent>();

        Assert.Same(obj, comp.GameObject);
        Assert.Same(obj.Transform, comp.Transform);
    }

    [Fact]
    public void Component_GetComponent_ReturnsCorrectType()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject("Test");
        TestComponent added = obj.AddComponent<TestComponent>();

        TestComponent? found = obj.GetComponent<TestComponent>();
        Assert.Same(added, found);
    }

    [Fact]
    public void Component_GetComponent_NotFound_ReturnsNull()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject("Test");
        Assert.Null(obj.GetComponent<TestComponent>());
    }

    [Fact]
    public void Component_Enabled_DefaultsTrue()
    {
        TestComponent comp = new();
        Assert.True(comp.Enabled);
    }

    [Fact]
    public void Component_Disabled_DoesNotUpdate()
    {
        Scene scene = new();
        List<string> log = new();
        GameObject obj = scene.CreateGameObject("Test");
        TrackComponent comp = obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "C"; });
        comp.Enabled = false;

        scene.Update();
        scene.Update();

        Assert.DoesNotContain("Update:C", log);
        Assert.Contains("Start:C", log);
    }

    [Fact]
    public void Component_Awake_CalledImmediately()
    {
        Scene scene = new();
        List<string> log = new();
        GameObject obj = scene.CreateGameObject("Test");
        obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "C"; });

        Assert.Contains("Awake:C", log);
    }

    [Fact]
    public void Component_Start_CalledOnFirstUpdate()
    {
        Scene scene = new();
        List<string> log = new();
        GameObject obj = scene.CreateGameObject("Test");
        obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "C"; });

        scene.Update();
        Assert.Contains("Start:C", log);

        log.Clear();
        scene.Update();
        Assert.DoesNotContain("Start:C", log);
    }

    [Fact]
    public void Component_Update_CalledEveryFrame()
    {
        Scene scene = new();
        List<string> log = new();
        GameObject obj = scene.CreateGameObject("Test");
        obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "C"; });

        scene.Update();
        scene.Update();
        scene.Update();

        List<string> updates = log.Where(s => s == "Update:C").ToList();
        Assert.Equal(3, updates.Count);
    }

    [Fact]
    public void Component_OnDestroy_CalledAtFrameEnd()
    {
        Scene scene = new();
        List<string> log = new();
        GameObject obj = scene.CreateGameObject("Test");
        obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "C"; });

        scene.Update();
        obj.Destroy();
        Assert.DoesNotContain("Destroy:C", log);

        scene.Update();
        Assert.Contains("Destroy:C", log);
    }

    [Fact]
    public void Component_GetComponent_FromComponent_Works()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject("Test");
        TestComponent compA = obj.AddComponent<TestComponent>();
        TestComponent compB = obj.AddComponent<TestComponent>();

        TestComponent? found = compA.GetComponent<TestComponent>();
        Assert.NotNull(found);
    }

    [Fact]
    public void Component_GetComponents_FromComponent_ReturnsAll()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject("Test");
        obj.AddComponent<TestComponent>();
        obj.AddComponent<TestComponent>();

        TestComponent first = obj.GetComponent<TestComponent>()!;
        List<TestComponent> all = first.GetComponents<TestComponent>();

        Assert.Equal(2, all.Count);
    }

    private class TestComponent : Component { }
}
