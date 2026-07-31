using System;
using System.Collections.Generic;

namespace FrameForge.Core {

/// <summary>
/// The root container for all <see cref="GameObject"/> instances. A Scene drives
/// the deterministic lifecycle (Awake → Start → Update → OnDestroy) and provides
/// lookup methods for finding objects by name, tag, or component type.
/// </summary>
/// <remarks>
/// <para>The Scene is the entry point for frame-based execution. Call
/// <see cref="Update"/> once per frame to drive the lifecycle of all objects.</para>
/// <para>Destroyed objects are deferred to the end of the frame so that other
/// systems can still access them during the frame. Objects created during
/// <see cref="Update"/> are added immediately for lookup purposes but do not
/// receive their first <see cref="Component.Update"/> until the next frame.</para>
/// </remarks>
public class Scene
{
    // ========================================================================
    // Fields
    // ========================================================================

    private readonly List<GameObject> _rootObjects = new();
    private readonly Dictionary<long, GameObject> _allObjects = new();
    private readonly Dictionary<string, List<GameObject>> _nameIndex = new();
    private readonly Dictionary<string, List<GameObject>> _tagIndex = new();
    private readonly List<GameObject> _pendingAdd = new();
    private readonly List<GameObject> _pendingDestroy = new();
    private long _nextId;
    private bool _updating;

    // ========================================================================
    // Properties
    // ========================================================================

    /// <summary>
    /// The root GameObjects in this scene, in creation order. Children are
    /// accessed via their parent's <see cref="GameObject.Children"/> list.
    /// </summary>
    public IReadOnlyList<GameObject> RootObjects => _rootObjects;

    /// <summary>
    /// The total number of live (non-destroyed) GameObjects in the scene,
    /// including children.
    /// </summary>
    public int ObjectCount => _allObjects.Count;

    /// <summary>
    /// When true (the default), parent GameObjects receive
    /// <see cref="Component.Update"/> before their children. When false,
    /// all objects are updated in flat creation-ID order.
    /// </summary>
    public bool ParentBeforeChildren { get; set; } = true;

    // ========================================================================
    // GameObject Creation
    // ========================================================================

    /// <summary>
    /// Creates a new <see cref="GameObject"/> with the given name and adds it
    /// to the scene. The object's <see cref="Component.Awake"/> is called
    /// immediately. <see cref="Component.Start"/> is deferred to the next
    /// <see cref="Update"/>.
    /// </summary>
    /// <param name="name">The name for the new GameObject. Defaults to "GameObject".</param>
    /// <returns>The newly created GameObject.</returns>
    public GameObject CreateGameObject(string name = "GameObject")
    {
        long id = _nextId++;
        GameObject obj = new(this, id, name);

        // Register in the all-objects dictionary
        _allObjects[id] = obj;

        // Index by name
        IndexByName(obj);

        // Awake immediately
        obj.AwakeNewComponents();

        if (_updating)
        {
            // During Update: defer adding to root objects until next frame
            _pendingAdd.Add(obj);
        }
        else
        {
            // Not updating: add to root objects immediately
            _rootObjects.Add(obj);
        }

        return obj;
    }

    // ========================================================================
    // Lookup Methods
    // ========================================================================

    /// <summary>
    /// Finds the first <see cref="GameObject"/> with the given name.
    /// If multiple objects share the name, the one with the lowest creation ID
    /// (earliest creation order) is returned. This is deterministic across platforms.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <returns>The first matching GameObject, or null if none found.</returns>
    public GameObject? FindByName(string name)
    {
        if (_nameIndex.TryGetValue(name, out List<GameObject>? list) && list.Count > 0)
            return list[0];
        return null;
    }

    /// <summary>
    /// Finds all <see cref="GameObject"/> instances with the given name.
    /// Results are returned in creation order, which is deterministic.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <returns>A list of matching GameObjects. Returns an empty list if none found.</returns>
    public List<GameObject> FindAllByName(string name)
    {
        if (_nameIndex.TryGetValue(name, out List<GameObject>? list))
            return new List<GameObject>(list);
        return new List<GameObject>();
    }

    /// <summary>
    /// Finds all <see cref="GameObject"/> instances with the given tag.
    /// Results are returned in creation order, which is deterministic.
    /// </summary>
    /// <param name="tag">The tag to search for.</param>
    /// <returns>A list of matching GameObjects. Returns an empty list if none found.</returns>
    public List<GameObject> FindByTag(string tag)
    {
        if (_tagIndex.TryGetValue(tag, out List<GameObject>? list))
            return new List<GameObject>(list);
        return new List<GameObject>();
    }

    /// <summary>
    /// Finds all <see cref="GameObject"/> instances that have a component of
    /// type <typeparamref name="T"/> attached. Results are returned in creation
    /// order, which is deterministic.
    /// </summary>
    /// <typeparam name="T">The component type to search for.</typeparam>
    /// <returns>A list of GameObjects that have the specified component attached.</returns>
    public List<GameObject> FindObjectsWithComponent<T>() where T : Component
    {
        List<GameObject> result = new();
        foreach (KeyValuePair<long, GameObject> kv in _allObjects)
        {
            if (kv.Value.GetComponent<T>() != null)
                result.Add(kv.Value);
        }
        return result;
    }

    /// <summary>
    /// Finds all components of type <typeparamref name="T"/> across the entire
    /// scene. Results are ordered by the creation ID of their owning GameObject
    /// (and within a GameObject, by component attachment order).
    /// </summary>
    /// <typeparam name="T">The component type to search for.</typeparam>
    /// <returns>A list of matching components.</returns>
    public List<T> FindComponentsOfType<T>() where T : Component
    {
        List<T> result = new();
        foreach (KeyValuePair<long, GameObject> kv in _allObjects)
        {
            List<T> onObj = kv.Value.GetComponents<T>();
            result.AddRange(onObj);
        }
        return result;
    }

    /// <summary>
    /// Returns the <see cref="GameObject"/> with the given ID, or null if
    /// no such object exists (or it has been destroyed).
    /// </summary>
    public GameObject? FindById(long id)
    {
        _allObjects.TryGetValue(id, out GameObject? obj);
        return obj;
    }

    // ========================================================================
    // Lifecycle — Update (the main frame driver)
    // ========================================================================

    /// <summary>
    /// Drives the lifecycle for one frame. Must be called once per frame.
    ///
    /// <para>Execution order:</para>
    /// <list type="number">
    ///   <item>Merge pending-add objects into the root list</item>
    ///   <item>Call <see cref="Component.Start"/> on all un-started objects</item>
    ///   <item>Call <see cref="Component.Update"/> on all active objects</item>
    ///   <item>Process pending destroys (call <see cref="Component.OnDestroy"/> and remove)</item>
    /// </list>
    /// </summary>
    public void Update()
    {
        _updating = true;

        try
        {
            // Step 1: Merge pending adds from the previous frame
            MergePendingAdds();

            // Step 2: Call Start on all objects that need it
            CallStartOnAll();

            // Step 3: Update all active objects
            if (ParentBeforeChildren)
                UpdateHierarchical();
            else
                UpdateFlat();

            // Step 4: Process pending destroys at frame end
            ProcessDestroys();
        }
        finally
        {
            _updating = false;
        }
    }

    // ========================================================================
    // Internal: Index Management
    // ========================================================================

    /// <summary>
    /// Adds a GameObject to the name index.
    /// </summary>
    internal void IndexByName(GameObject obj)
    {
        if (string.IsNullOrEmpty(obj.Name))
            return;

        if (!_nameIndex.TryGetValue(obj.Name, out List<GameObject>? list))
        {
            list = new List<GameObject>();
            _nameIndex[obj.Name] = list;
        }
        list.Add(obj);
    }

    /// <summary>
    /// Updates the name index when a GameObject's name changes.
    /// </summary>
    internal void UpdateNameIndex(GameObject obj, string oldName)
    {
        // Remove from old name
        if (!string.IsNullOrEmpty(oldName) && _nameIndex.TryGetValue(oldName, out List<GameObject>? oldList))
        {
            oldList.Remove(obj);
            if (oldList.Count == 0)
                _nameIndex.Remove(oldName);
        }

        // Add to new name
        IndexByName(obj);
    }

    /// <summary>
    /// Adds a GameObject to the tag index.
    /// </summary>
    internal void IndexByTag(GameObject obj)
    {
        if (string.IsNullOrEmpty(obj.Tag))
            return;

        if (!_tagIndex.TryGetValue(obj.Tag, out List<GameObject>? list))
        {
            list = new List<GameObject>();
            _tagIndex[obj.Tag] = list;
        }
        list.Add(obj);
    }

    /// <summary>
    /// Updates the tag index when a GameObject's tag changes.
    /// </summary>
    internal void UpdateTagIndex(GameObject obj, string oldTag)
    {
        // Remove from old tag
        if (!string.IsNullOrEmpty(oldTag) && _tagIndex.TryGetValue(oldTag, out List<GameObject>? oldList))
        {
            oldList.Remove(obj);
            if (oldList.Count == 0)
                _tagIndex.Remove(oldTag);
        }

        // Add to new tag
        IndexByTag(obj);
    }

    /// <summary>
    /// Removes a GameObject from all indices (called during Destroy processing).
    /// </summary>
    internal void RemoveFromIndices(GameObject obj)
    {
        _allObjects.Remove(obj.Id);

        if (!string.IsNullOrEmpty(obj.Name) && _nameIndex.TryGetValue(obj.Name, out List<GameObject>? nameList))
        {
            nameList.Remove(obj);
            if (nameList.Count == 0)
                _nameIndex.Remove(obj.Name);
        }

        if (!string.IsNullOrEmpty(obj.Tag) && _tagIndex.TryGetValue(obj.Tag, out List<GameObject>? tagList))
        {
            tagList.Remove(obj);
            if (tagList.Count == 0)
                _tagIndex.Remove(obj.Tag);
        }
    }

    /// <summary>
    /// Adds a root-level GameObject, inserting it in creation-ID order.
    /// </summary>
    internal void AddRootObject(GameObject obj)
    {
        // Insert in ID order to keep _rootObjects sorted by creation order
        int insertIndex = _rootObjects.Count;
        for (int i = 0; i < _rootObjects.Count; i++)
        {
            if (_rootObjects[i].Id > obj.Id)
            {
                insertIndex = i;
                break;
            }
        }
        _rootObjects.Insert(insertIndex, obj);
    }

    /// <summary>
    /// Removes a GameObject from the root objects list.
    /// </summary>
    internal void RemoveRootObject(GameObject obj)
    {
        _rootObjects.Remove(obj);
    }

    /// <summary>
    /// Re-registers a child in the indices after SetParent changes its
    /// accessibility. This ensures name/tag lookups still find child objects.
    /// Child objects remain in the indices; this is a no-op for the current
    /// design where children are always indexed.
    /// </summary>
    internal void RegisterChildInIndices(GameObject obj)
    {
        // Children are already in indices; nothing extra needed.
        // This hook exists for future extensibility.
    }

    /// <summary>
    /// Unregisters a child from parent-specific tracking. Symmetric with
    /// <see cref="RegisterChildInIndices"/>.
    /// </summary>
    internal void UnregisterChildFromIndices(GameObject obj)
    {
        // Children remain in indices; nothing extra needed.
    }

    // ========================================================================
    // Internal: Destroy Queue
    // ========================================================================

    /// <summary>
    /// Queues a GameObject for destruction at the end of the current frame.
    /// Called by <see cref="GameObject.Destroy"/>.
    /// </summary>
    internal void QueueDestroy(GameObject obj)
    {
        if (!_pendingDestroy.Contains(obj))
            _pendingDestroy.Add(obj);
    }

    // ========================================================================
    // Private: Lifecycle Steps
    // ========================================================================

    /// <summary>
    /// Moves objects from the pending-add list into the root objects list.
    /// </summary>
    private void MergePendingAdds()
    {
        if (_pendingAdd.Count == 0)
            return;

        for (int i = 0; i < _pendingAdd.Count; i++)
        {
            AddRootObject(_pendingAdd[i]);
        }
        _pendingAdd.Clear();
    }

    /// <summary>
    /// Calls Start on all objects (root + children) that haven't been started yet.
    /// </summary>
    private void CallStartOnAll()
    {
        for (int i = 0; i < _rootObjects.Count; i++)
        {
            _rootObjects[i].CallStart();
        }
    }

    /// <summary>
    /// Updates all objects in hierarchical order (parent before children).
    /// </summary>
    private void UpdateHierarchical()
    {
        for (int i = 0; i < _rootObjects.Count; i++)
        {
            _rootObjects[i].CallUpdate();
        }
    }

    /// <summary>
    /// Updates all objects in flat creation-ID order.
    /// </summary>
    private void UpdateFlat()
    {
        // Collect all live objects and sort by ID
        List<GameObject> allLive = new(_allObjects.Count);
        foreach (KeyValuePair<long, GameObject> kv in _allObjects)
        {
            allLive.Add(kv.Value);
        }

        // Sort by ID (creation order)
        allLive.Sort((a, b) => a.Id.CompareTo(b.Id));

        for (int i = 0; i < allLive.Count; i++)
        {
            GameObject obj = allLive[i];
            if (obj.IsDestroyed || !obj.Active)
                continue;

            // Only update components, not children (flat mode = each object independently)
            obj.CallUpdateComponentsOnly();
        }
    }

    /// <summary>
    /// Processes all pending destroys: calls OnDestroy on components, then
    /// removes the objects from the scene.
    /// </summary>
    private void ProcessDestroys()
    {
        if (_pendingDestroy.Count == 0)
            return;

        // Call OnDestroy callbacks (in reverse creation order for deterministic cleanup)
        // We iterate forward through _pendingDestroy but each object's children
        // are already included. The order matters: children are destroyed before parents
        // because Destroy() recursively marks children first, so _pendingDestroy
        // has children before parents. Process in that order.
        for (int i = 0; i < _pendingDestroy.Count; i++)
        {
            _pendingDestroy[i].CallDestroy();
        }

        // Remove from scene structures
        for (int i = 0; i < _pendingDestroy.Count; i++)
        {
            GameObject obj = _pendingDestroy[i];

            // Remove from root or parent
            if (obj.Parent == null)
            {
                _rootObjects.Remove(obj);
            }
            else
            {
                // The parent-child link is already broken by recursive Destroy,
                // but let's be safe
            }

            // Remove from indices
            RemoveFromIndices(obj);
        }

        _pendingDestroy.Clear();
    }
}
}
