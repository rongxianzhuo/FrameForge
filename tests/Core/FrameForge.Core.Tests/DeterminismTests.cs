namespace FrameForge.Core.Tests {

/// <summary>
/// Tests verifying deterministic behaviour: traversal order of objects
/// and components is consistent and predictable across repeated runs.
/// </summary>
public class DeterminismTests
{
    [Fact]
    public void GameObjectIds_AreMonotonicallyIncreasing()
    {
        Scene scene = new();
        List<long> ids = new();

        for (int i = 0; i < 100; i++)
            ids.Add(scene.CreateGameObject().Id);

        for (int i = 1; i < ids.Count; i++)
            Assert.True(ids[i] > ids[i - 1]);
    }

    [Fact]
    public void RootObjects_AlwaysInCreationOrder()
    {
        for (int run = 0; run < 5; run++)
        {
            Scene scene = new();
            List<long> expectedOrder = new();
            for (int i = 0; i < 20; i++)
                expectedOrder.Add(scene.CreateGameObject($"Obj{i}").Id);

            for (int i = 0; i < scene.RootObjects.Count; i++)
                Assert.Equal(expectedOrder[i], scene.RootObjects[i].Id);
        }
    }

    [Fact]
    public void ComponentUpdateOrder_MatchesAttachmentOrder()
    {
        for (int run = 0; run < 5; run++)
        {
            Scene scene = new();
            List<string> log = new();
            GameObject obj = scene.CreateGameObject("Test");

            string[] labels = { "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9" };
            foreach (string label in labels)
                obj.AddComponent<TrackComponent>(c => { c.Log = log; c.Label = label; });

            scene.Update();

            List<string> updates = log.Where(s => s.StartsWith("Update:")).ToList();
            Assert.Equal(labels.Length, updates.Count);
            for (int i = 0; i < labels.Length; i++)
                Assert.Equal($"Update:{labels[i]}", updates[i]);
        }
    }

    [Fact]
    public void FindByName_SameName_ReturnsFirstCreated()
    {
        for (int run = 0; run < 5; run++)
        {
            Scene scene = new();
            GameObject first = scene.CreateGameObject("Dup");
            scene.CreateGameObject("Dup");
            scene.CreateGameObject("Dup");
            Assert.Same(first, scene.FindByName("Dup"));
        }
    }

    [Fact]
    public void FindAllByName_ReturnsInCreationOrder()
    {
        for (int run = 0; run < 5; run++)
        {
            Scene scene = new();
            List<long> expectedIds = new();
            for (int i = 0; i < 10; i++)
                expectedIds.Add(scene.CreateGameObject("Same").Id);

            List<GameObject> results = scene.FindAllByName("Same");
            Assert.Equal(expectedIds.Count, results.Count);
            for (int i = 0; i < expectedIds.Count; i++)
                Assert.Equal(expectedIds[i], results[i].Id);
        }
    }

    [Fact]
    public void FindByTag_ReturnsInCreationOrder()
    {
        for (int run = 0; run < 5; run++)
        {
            Scene scene = new();
            List<long> expectedIds = new();
            for (int i = 0; i < 10; i++)
            {
                GameObject obj = scene.CreateGameObject($"Obj{i}");
                obj.Tag = "SameTag";
                expectedIds.Add(obj.Id);
            }

            List<GameObject> results = scene.FindByTag("SameTag");
            Assert.Equal(expectedIds.Count, results.Count);
            for (int i = 0; i < expectedIds.Count; i++)
                Assert.Equal(expectedIds[i], results[i].Id);
        }
    }

    [Fact]
    public void Destroy_ChildrenCallbacksBeforeParents()
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

        List<string> destroys = log.Where(s => s.StartsWith("Destroy:")).ToList();
        Assert.Equal(2, destroys.Count);

        int childIdx = destroys.IndexOf("Destroy:Child");
        int parentIdx = destroys.IndexOf("Destroy:Parent");
        Assert.True(childIdx >= 0 && parentIdx >= 0);
        Assert.True(childIdx < parentIdx);
    }

    [Fact]
    public void ObjectsCreatedDuringUpdate_SameOrderAcrossRuns()
    {
        for (int run = 0; run < 3; run++)
        {
            Scene scene = new();
            List<string> log = new();

            GameObject spawner = scene.CreateGameObject("Spawner");
            var multiSpawner = new MultiSpawnerComponent();
            multiSpawner.Init(scene, log, 3);
            // Use the pre-configured instance approach
            // AddComponent doesn't support passing existing instances, so we use the configure action
            spawner.AddComponent<MultiSpawnerComponent>(c => c.Init(scene, log, 3));

            scene.Update();
            log.Clear();
            scene.Update();

            List<string> updates = log.Where(s => s.StartsWith("Update:Spawned")).ToList();
            Assert.Equal(3, updates.Count);
            Assert.Equal("Update:Spawned0", updates[0]);
            Assert.Equal("Update:Spawned1", updates[1]);
            Assert.Equal("Update:Spawned2", updates[2]);
        }
    }
}
}
