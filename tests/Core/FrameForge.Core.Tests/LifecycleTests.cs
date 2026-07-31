namespace FrameForge.Core.Tests {

/// <summary>
/// Comprehensive tests for the full lifecycle order:
/// Awake → Start → Update → OnDestroy,
/// including component attachment order and hierarchy scenarios.
/// </summary>
public class LifecycleTests
{
    [Fact]
    public void SingleObject_LifecycleOrder()
    {
        Scene scene = new();
        List<string> log = new();

        GameObject obj = scene.CreateGameObject("Test");
        obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "CompA"; });
        Assert.Contains("Awake:CompA", log);

        log.Clear();
        scene.Update();
        Assert.Contains("Start:CompA", log);
        Assert.Contains("Update:CompA", log);

        log.Clear();
        scene.Update();
        Assert.DoesNotContain("Start:CompA", log);
        Assert.Contains("Update:CompA", log);

        obj.Destroy();
        log.Clear();
        scene.Update();
        Assert.Contains("Destroy:CompA", log);
    }

    [Fact]
    public void Components_UpdatedInAttachmentOrder()
    {
        Scene scene = new();
        List<string> log = new();

        GameObject obj = scene.CreateGameObject("Test");
        obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "First"; });
        obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "Second"; });
        obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "Third"; });

        scene.Update();

        List<string> updates = log.Where(s => s.StartsWith("Update:")).ToList();
        Assert.Equal(3, updates.Count);
        Assert.Equal("Update:First", updates[0]);
        Assert.Equal("Update:Second", updates[1]);
        Assert.Equal("Update:Third", updates[2]);
    }

    [Fact]
    public void Components_AwakeInAttachmentOrder()
    {
        Scene scene = new();
        List<string> log = new();

        GameObject obj = scene.CreateGameObject("Test");
        log.Clear();

        obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "A"; });
        obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "B"; });
        obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "C"; });

        List<string> awakes = log.Where(s => s.StartsWith("Awake:")).ToList();
        Assert.Equal(3, awakes.Count);
        Assert.Equal("Awake:A", awakes[0]);
        Assert.Equal("Awake:B", awakes[1]);
        Assert.Equal("Awake:C", awakes[2]);
    }

    [Fact]
    public void Components_StartInAttachmentOrder()
    {
        Scene scene = new();
        List<string> log = new();

        GameObject obj = scene.CreateGameObject("Test");
        obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "A"; });
        obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "B"; });
        obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "C"; });

        scene.Update();

        List<string> starts = log.Where(s => s.StartsWith("Start:")).ToList();
        Assert.Equal(3, starts.Count);
        Assert.Equal("Start:A", starts[0]);
        Assert.Equal("Start:B", starts[1]);
        Assert.Equal("Start:C", starts[2]);
    }

    [Fact]
    public void Hierarchy_Start_ParentBeforeChildren()
    {
        Scene scene = new();
        List<string> log = new();

        GameObject parent = scene.CreateGameObject("Parent");
        parent.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "Parent"; });

        GameObject child = scene.CreateGameObject("Child");
        child.SetParent(parent);
        child.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "Child"; });

        scene.Update();

        List<string> starts = log.Where(s => s.StartsWith("Start:")).ToList();
        Assert.Equal(2, starts.Count);
        Assert.Equal("Start:Parent", starts[0]);
        Assert.Equal("Start:Child", starts[1]);
    }

    [Fact]
    public void Hierarchy_Destroy_ChildrenDestroyedWithParent()
    {
        Scene scene = new();
        List<string> log = new();

        GameObject parent = scene.CreateGameObject("Parent");
        parent.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "Parent"; });

        GameObject child = scene.CreateGameObject("Child");
        child.SetParent(parent);
        child.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "Child"; });

        scene.Update();
        log.Clear();

        parent.Destroy();
        scene.Update();

        Assert.Contains("Destroy:Parent", log);
        Assert.Contains("Destroy:Child", log);
    }

    [Fact]
    public void MultipleRootObjects_UpdatedInCreationOrder()
    {
        Scene scene = new();
        List<string> log = new();

        GameObject a = scene.CreateGameObject("A");
        a.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "A"; });

        GameObject b = scene.CreateGameObject("B");
        b.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "B"; });

        GameObject c = scene.CreateGameObject("C");
        c.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "C"; });

        scene.Update();

        List<string> updates = log.Where(s => s.StartsWith("Update:")).ToList();
        Assert.Equal(3, updates.Count);
        Assert.Equal("Update:A", updates[0]);
        Assert.Equal("Update:B", updates[1]);
        Assert.Equal("Update:C", updates[2]);
    }

    [Fact]
    public void DisabledComponent_StillGetsAwakeStartDestroy()
    {
        Scene scene = new();
        List<string> log = new();

        GameObject obj = scene.CreateGameObject("Test");
        TrackComponent comp = obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "Comp"; });
        comp.Enabled = false;

        Assert.Contains("Awake:Comp", log);
        log.Clear();

        scene.Update();
        Assert.Contains("Start:Comp", log);
        Assert.DoesNotContain("Update:Comp", log);

        obj.Destroy();
        log.Clear();
        scene.Update();
        Assert.Contains("Destroy:Comp", log);
    }

    [Fact]
    public void InactiveObject_StillGetsAwakeStartDestroy()
    {
        Scene scene = new();
        List<string> log = new();

        GameObject obj = scene.CreateGameObject("Test");
        TrackComponent comp = obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "Comp"; });
        obj.Active = false;

        Assert.Contains("Awake:Comp", log);
        log.Clear();

        scene.Update();
        Assert.Contains("Start:Comp", log);
        Assert.DoesNotContain("Update:Comp", log);
    }

    [Fact]
    public void Destroy_DeferredToFrameEnd_ComponentsStillAccessible()
    {
        Scene scene = new();
        List<string> log = new();

        GameObject obj = scene.CreateGameObject("Test");
        obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "A"; });
        obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "B"; });

        scene.Update();

        obj.Destroy();
        Assert.True(obj.IsDestroyed);

        Assert.DoesNotContain("Destroy:A", log);
        Assert.DoesNotContain("Destroy:B", log);

        Assert.NotNull(obj.GetComponent<TrackComponent>());

        scene.Update();
        Assert.Contains("Destroy:A", log);
        Assert.Contains("Destroy:B", log);
    }
}
}
