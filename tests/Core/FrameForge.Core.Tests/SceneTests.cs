namespace FrameForge.Core.Tests;

/// <summary>
/// Tests for <see cref="Scene"/> object management, lookup methods,
/// parent-before-children configuration, flat update mode,
/// and deferred creation during Update.
/// </summary>
public class SceneTests
{
    // ========================================================================
    // Object Creation & Counting
    // ========================================================================

    [Fact]
    public void CreateGameObject_IncreasesCount()
    {
        Scene scene = new();
        Assert.Equal(0, scene.ObjectCount);

        scene.CreateGameObject();
        Assert.Equal(1, scene.ObjectCount);

        scene.CreateGameObject();
        Assert.Equal(2, scene.ObjectCount);
    }

    [Fact]
    public void RootObjects_InCreationOrder()
    {
        Scene scene = new();
        GameObject a = scene.CreateGameObject("A");
        GameObject b = scene.CreateGameObject("B");
        GameObject c = scene.CreateGameObject("C");

        Assert.Equal(3, scene.RootObjects.Count);
        Assert.Same(a, scene.RootObjects[0]);
        Assert.Same(b, scene.RootObjects[1]);
        Assert.Same(c, scene.RootObjects[2]);
    }

    // ========================================================================
    // Lookup Methods
    // ========================================================================

    [Fact]
    public void FindByName_ReturnsCorrectObject()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject("Player");

        GameObject? found = scene.FindByName("Player");
        Assert.Same(obj, found);
    }

    [Fact]
    public void FindByName_NotFound_ReturnsNull()
    {
        Scene scene = new();
        Assert.Null(scene.FindByName("NonExistent"));
    }

    [Fact]
    public void FindByName_MultipleMatches_ReturnsFirstCreated()
    {
        Scene scene = new();
        GameObject first = scene.CreateGameObject("Enemy");
        scene.CreateGameObject("Enemy");

        Assert.Same(first, scene.FindByName("Enemy"));
    }

    [Fact]
    public void FindAllByName_ReturnsAllInCreationOrder()
    {
        Scene scene = new();
        GameObject a = scene.CreateGameObject("Enemy");
        GameObject b = scene.CreateGameObject("Enemy");
        GameObject c = scene.CreateGameObject("Enemy");

        List<GameObject> all = scene.FindAllByName("Enemy");
        Assert.Equal(3, all.Count);
        Assert.Same(a, all[0]);
        Assert.Same(b, all[1]);
        Assert.Same(c, all[2]);
    }

    [Fact]
    public void FindByTag_ReturnsMatchingObjects()
    {
        Scene scene = new();
        GameObject a = scene.CreateGameObject("A");
        a.Tag = "Player";
        GameObject b = scene.CreateGameObject("B");
        b.Tag = "Enemy";
        GameObject c = scene.CreateGameObject("C");
        c.Tag = "Player";

        List<GameObject> players = scene.FindByTag("Player");
        Assert.Equal(2, players.Count);
        Assert.Same(a, players[0]);
        Assert.Same(c, players[1]);
    }

    [Fact]
    public void FindByTag_EmptyTag_ReturnsEmpty()
    {
        Scene scene = new();
        scene.CreateGameObject();

        List<GameObject> result = scene.FindByTag("");
        Assert.Empty(result);
    }

    [Fact]
    public void FindObjectsWithComponent_ReturnsCorrectObjects()
    {
        Scene scene = new();
        GameObject a = scene.CreateGameObject("A");
        a.AddComponent<TrackComponent>(c => { c.Log = new List<string>(); c.Label = "A"; });

        scene.CreateGameObject("B"); // No TrackComponent

        GameObject c = scene.CreateGameObject("C");
        c.AddComponent<TrackComponent>(c => { c.Log = new List<string>(); c.Label = "C"; });

        List<GameObject> withComp = scene.FindObjectsWithComponent<TrackComponent>();
        Assert.Equal(2, withComp.Count);
    }

    [Fact]
    public void FindComponentsOfType_ReturnsAllInstances()
    {
        Scene scene = new();
        GameObject a = scene.CreateGameObject("A");
        a.AddComponent<TrackComponent>(c => { c.Log = new List<string>(); c.Label = "A1"; });
        a.AddComponent<TrackComponent>(c => { c.Log = new List<string>(); c.Label = "A2"; });

        GameObject b = scene.CreateGameObject("B");
        b.AddComponent<TrackComponent>(c => { c.Log = new List<string>(); c.Label = "B"; });

        List<TrackComponent> all = scene.FindComponentsOfType<TrackComponent>();
        Assert.Equal(3, all.Count);
    }

    [Fact]
    public void FindById_ReturnsCorrectObject()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject("Test");

        GameObject? found = scene.FindById(obj.Id);
        Assert.Same(obj, found);
    }

    [Fact]
    public void FindById_AfterDestroy_ReturnsNull()
    {
        Scene scene = new();
        GameObject obj = scene.CreateGameObject("Test");
        long id = obj.Id;
        obj.Destroy();
        scene.Update();

        Assert.Null(scene.FindById(id));
    }

    // ========================================================================
    // ParentBeforeChildren
    // ========================================================================

    [Fact]
    public void ParentBeforeChildren_DefaultTrue()
    {
        Scene scene = new();
        Assert.True(scene.ParentBeforeChildren);
    }

    [Fact]
    public void ParentBeforeChildren_ParentUpdatesBeforeChild()
    {
        Scene scene = new();
        List<string> log = new();

        GameObject parent = scene.CreateGameObject("Parent");
        parent.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "Parent"; });

        GameObject child = scene.CreateGameObject("Child");
        child.SetParent(parent);
        child.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "Child"; });

        scene.Update();

        int parentIdx = log.IndexOf("Update:Parent");
        int childIdx = log.IndexOf("Update:Child");
        Assert.True(parentIdx >= 0);
        Assert.True(childIdx >= 0);
        Assert.True(parentIdx < childIdx, $"Parent ({parentIdx}) should update before child ({childIdx})");
    }

    // ========================================================================
    // Flat Update Mode
    // ========================================================================

    [Fact]
    public void FlatUpdate_UpdatesInCreationOrder()
    {
        Scene scene = new();
        scene.ParentBeforeChildren = false;
        List<string> log = new();

        GameObject a = scene.CreateGameObject("A");
        a.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "A"; });

        GameObject b = scene.CreateGameObject("B");
        b.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "B"; });

        GameObject c = scene.CreateGameObject("C");
        c.SetParent(a);
        c.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = "C"; });

        scene.Update();

        int idxA = log.IndexOf("Update:A");
        int idxB = log.IndexOf("Update:B");
        int idxC = log.IndexOf("Update:C");

        Assert.True(idxA >= 0 && idxB >= 0 && idxC >= 0);
        Assert.True(idxA < idxB);
        Assert.True(idxB < idxC);
    }

    // ========================================================================
    // Deferred Add During Update
    // ========================================================================

    [Fact]
    public void ObjectCreatedDuringUpdate_DoesNotUpdateThisFrame()
    {
        Scene scene = new();
        List<string> log = new();

        GameObject spawnerObj = scene.CreateGameObject("Spawner");
        spawnerObj.AddComponent<SpawnerComponent>().Init(scene, log);

        scene.Update();

        // The spawned object was awoken but NOT updated this frame
        Assert.Contains("Awake:Spawned", log);
        Assert.DoesNotContain("Start:Spawned", log);
        Assert.DoesNotContain("Update:Spawned", log);
    }

    [Fact]
    public void ObjectCreatedDuringUpdate_UpdatesNextFrame()
    {
        Scene scene = new();
        List<string> log = new();

        GameObject spawnerObj = scene.CreateGameObject("Spawner");
        spawnerObj.AddComponent<SpawnerComponent>().Init(scene, log);

        scene.Update(); // Spawns the object, doesn't update it
        log.Clear();
        scene.Update(); // Now spawned object should Start + Update

        Assert.Contains("Start:Spawned", log);
        Assert.Contains("Update:Spawned", log);
    }
}
