using System;
using System.Collections.Generic;

namespace FrameForge.Core {

/// <summary>
/// The fundamental container for <see cref="Component"/> instances in the scene.
/// Each GameObject has a unique ID (determining its creation-order position),
/// a <see cref="Transform"/>, and an optional parent-child hierarchy.
/// </summary>
/// <remarks>
/// <para>GameObjects are created via <see cref="Scene.CreateGameObject"/> and
/// destroyed via <see cref="Destroy"/>. Destroyed objects are removed at the
/// end of the current frame to preserve deterministic access for the remainder
/// of the frame.</para>
/// <para>Components are updated in the order they were added. Objects are
/// updated in creation order (or parent-before-child order when configured).</para>
/// </remarks>
public class GameObject
{
    // ========================================================================
    // Fields
    // ========================================================================

    private readonly List<Component> _components = new();
    private GameObject? _parent;
    private readonly List<GameObject> _children = new();
    private bool _active = true;
    private bool _destroyed;
    private bool _started;

    // ========================================================================
    // Properties
    // ========================================================================

    /// <summary>
    /// Unique identifier for this GameObject. IDs are assigned in monotonically
    /// increasing order, providing deterministic traversal across platforms.
    /// </summary>
    public long Id { get; }

    private string _name = string.Empty;

    /// <summary>
    /// The name of this GameObject. Used for lookup via <see cref="Scene.FindByName"/>.
    /// Changing the name updates the scene's name index.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
                return;
            string oldName = _name;
            _name = value;
            Scene.UpdateNameIndex(this, oldName);
        }
    }

    private string _tag = string.Empty;

    /// <summary>
    /// A user-defined tag for categorising GameObjects. Used for lookup via
    /// <see cref="Scene.FindByTag"/>. Changing the tag updates the scene's tag index.
    /// </summary>
    public string Tag
    {
        get => _tag;
        set
        {
            if (_tag == value)
                return;
            string oldTag = _tag;
            _tag = value;
            Scene.UpdateTagIndex(this, oldTag);
        }
    }

    /// <summary>
    /// Whether this GameObject is active. Inactive objects (and their components)
    /// do not receive <see cref="Component.Update"/> calls.
    /// </summary>
    public bool Active
    {
        get => _active;
        set => _active = value;
    }

    /// <summary>
    /// The <see cref="Scene"/> this GameObject belongs to.
    /// </summary>
    public Scene Scene { get; }

    /// <summary>
    /// The <see cref="Transform"/> component attached to this GameObject.
    /// Always present; cannot be removed.
    /// </summary>
    public Transform Transform { get; }

    /// <summary>
    /// The parent <see cref="GameObject"/> in the hierarchy, or null if this
    /// is a root object.
    /// </summary>
    public GameObject? Parent => _parent;

    /// <summary>
    /// An ordered list of child GameObjects, in the order they were parented.
    /// </summary>
    public IReadOnlyList<GameObject> Children => _children;

    /// <summary>
    /// Whether <see cref="Destroy"/> has been called on this object.
    /// Once marked for destruction it cannot be un-marked.
    /// </summary>
    public bool IsDestroyed => _destroyed;

    /// <summary>
    /// Whether <see cref="Component.Start"/> has been called on this object.
    /// </summary>
    internal bool Started
    {
        get => _started;
        set => _started = value;
    }

    // ========================================================================
    // Constructor (internal — created by Scene)
    // ========================================================================

    /// <summary>
    /// Creates a new GameObject. This constructor is internal; use
    /// <see cref="Scene.CreateGameObject"/> to create instances.
    /// </summary>
    internal GameObject(Scene scene, long id, string name)
    {
        Scene = scene;
        Id = id;
        // Set backing field directly to avoid triggering Scene index update
        // during construction (Scene.CreateGameObject handles indexing)
        _name = name;

        // Every GameObject always has a Transform
        Transform = new Transform();
        AttachComponentInternal(Transform);
    }

    // ========================================================================
    // Component Management
    // ========================================================================

    /// <summary>
    /// Adds a new component of type <typeparamref name="T"/> to this GameObject
    /// and returns it. The component is immediately awoken.
    /// </summary>
    /// <typeparam name="T">The type of component to add. Must have a parameterless constructor.</typeparam>
    /// <param name="configure">Optional action to configure the component before <see cref="Component.Awake"/> is called.</param>
    /// <returns>The newly created component.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the object is already destroyed
    /// or if attempting to add a second <see cref="Transform"/>.</exception>
    public T AddComponent<T>(Action<T>? configure = null) where T : Component, new()
    {
        if (_destroyed)
            throw new InvalidOperationException(
                $"Cannot add component to destroyed GameObject '{Name}'.");

        if (typeof(T) == typeof(Transform))
            throw new InvalidOperationException(
                "Cannot add a second Transform to a GameObject. Use the existing Transform property.");

        T component = new T();
        AttachComponentInternal(component);

        // Allow caller to configure the component before Awake
        configure?.Invoke(component);

        // Awake immediately
        component.AwakeCalled = true;
        component.Awake();

        return component;
    }

    /// <summary>
    /// Returns the first component of type <typeparamref name="T"/> attached to
    /// this GameObject, or null if none is found.
    /// </summary>
    public T? GetComponent<T>() where T : Component
    {
        for (int i = 0; i < _components.Count; i++)
        {
            if (_components[i] is T typed)
                return typed;
        }
        return null;
    }

    /// <summary>
    /// Returns all components of type <typeparamref name="T"/> attached to
    /// this GameObject. The returned list is a new list; modifying it does
    /// not affect the component collection.
    /// </summary>
    public List<T> GetComponents<T>() where T : Component
    {
        List<T> result = new();
        for (int i = 0; i < _components.Count; i++)
        {
            if (_components[i] is T typed)
                result.Add(typed);
        }
        return result;
    }

    /// <summary>
    /// Removes the first component of type <typeparamref name="T"/> from this
    /// GameObject. The <see cref="Transform"/> cannot be removed.
    /// </summary>
    /// <typeparam name="T">The type of component to remove.</typeparam>
    /// <returns>True if a component was removed; false otherwise.</returns>
    /// <exception cref="InvalidOperationException">Thrown if attempting to
    /// remove the <see cref="Transform"/>.</exception>
    public bool RemoveComponent<T>() where T : Component
    {
        if (typeof(T) == typeof(Transform))
            throw new InvalidOperationException(
                "Cannot remove the Transform from a GameObject.");

        for (int i = 0; i < _components.Count; i++)
        {
            if (_components[i] is T)
            {
                _components.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    // ========================================================================
    // Hierarchy Management
    // ========================================================================

    /// <summary>
    /// Sets the parent of this GameObject. Pass null to make it a root object.
    /// If the new parent is the same as the current parent, this is a no-op.
    /// </summary>
    /// <param name="parent">The new parent, or null to unparent.</param>
    /// <exception cref="InvalidOperationException">Thrown if attempting to
    /// create a circular hierarchy reference.</exception>
    public void SetParent(GameObject? parent)
    {
        if (parent == _parent)
            return;

        // Check for self-parenting
        if (parent == this)
            throw new InvalidOperationException(
                "Cannot set parent: a GameObject cannot be its own parent.");

        // Check for circular references
        if (parent != null && IsAncestorOf(parent))
            throw new InvalidOperationException(
                "Cannot set parent: circular hierarchy detected.");

        // Remove from old parent
        if (_parent != null)
        {
            _parent._children.Remove(this);
            _parent.Scene.UnregisterChildFromIndices(this);
        }
        else
        {
            Scene.RemoveRootObject(this);
        }

        // Set new parent
        _parent = parent;

        // Add to new parent
        if (_parent != null)
        {
            _parent._children.Add(this);
            _parent.Scene.RegisterChildInIndices(this);
        }
        else
        {
            Scene.AddRootObject(this);
        }
    }

    /// <summary>
    /// Returns the first child with the given name, or null if none is found.
    /// Searches immediate children only (not recursive).
    /// </summary>
    public GameObject? FindChild(string name)
    {
        for (int i = 0; i < _children.Count; i++)
        {
            if (_children[i].Name == name)
                return _children[i];
        }
        return null;
    }

    // ========================================================================
    // Destroy
    // ========================================================================

    /// <summary>
    /// Marks this GameObject for destruction at the end of the current frame.
    /// The object remains accessible for the rest of the frame; its components'
    /// <see cref="Component.OnDestroy"/> methods are called during the frame-end
    /// cleanup pass. All children are also recursively marked for destruction.
    /// </summary>
    public void Destroy()
    {
        if (_destroyed)
            return;

        _destroyed = true;

        // Recursively destroy children
        for (int i = _children.Count - 1; i >= 0; i--)
        {
            _children[i].Destroy();
        }

        Scene.QueueDestroy(this);
    }

    // ========================================================================
    // Internal Lifecycle Methods (called by Scene)
    // ========================================================================

    /// <summary>
    /// Calls <see cref="Component.Awake"/> on all components that haven't
    /// been awoken yet (e.g. components added after the GameObject was created).
    /// </summary>
    internal void AwakeNewComponents()
    {
        for (int i = 0; i < _components.Count; i++)
        {
            Component c = _components[i];
            if (!c.AwakeCalled)
            {
                c.AwakeCalled = true;
                c.Awake();
            }
        }
    }

    /// <summary>
    /// Calls <see cref="Component.Start"/> on all components, then recurses
    /// into children.
    /// </summary>
    internal void CallStart()
    {
        if (_started || _destroyed)
            return;

        _started = true;
        for (int i = 0; i < _components.Count; i++)
        {
            Component c = _components[i];
            if (!c.Started)
            {
                c.Started = true;
                c.Start();
            }
        }

        // Start children
        for (int i = 0; i < _children.Count; i++)
        {
            _children[i].CallStart();
        }
    }

    /// <summary>
    /// Calls <see cref="Component.Update"/> on all enabled components in
    /// attachment order, then recurses into active children.
    /// </summary>
    internal void CallUpdate()
    {
        // Objects not yet started (created mid-frame) do not update
        if (_destroyed || !_active || !_started)
            return;

        UpdateComponents();

        // Snapshot children to handle hierarchy modifications during Update
        GameObject[] snapshot = _children.ToArray();
        for (int i = 0; i < snapshot.Length; i++)
        {
            snapshot[i].CallUpdate();
        }
    }

    /// <summary>
    /// Calls <see cref="Component.Update"/> on this object's enabled components
    /// only, without recursing into children. Used by flat update mode.
    /// </summary>
    internal void CallUpdateComponentsOnly()
    {
        if (_destroyed || !_active || !_started)
            return;

        UpdateComponents();
    }

    /// <summary>
    /// Updates all enabled components on this object in attachment order.
    /// </summary>
    private void UpdateComponents()
    {
        for (int i = 0; i < _components.Count; i++)
        {
            Component c = _components[i];
            if (c.Enabled)
                c.Update();
        }
    }

    /// <summary>
    /// Calls <see cref="Component.OnDestroy"/> on all components.
    /// </summary>
    internal void CallDestroy()
    {
        for (int i = 0; i < _components.Count; i++)
        {
            _components[i].OnDestroy();
        }
    }

    // ========================================================================
    // Private Helpers
    // ========================================================================

    /// <summary>
    /// Attaches a component to this GameObject, setting its back-reference.
    /// </summary>
    private void AttachComponentInternal(Component component)
    {
        component.GameObject = this;
        _components.Add(component);
    }

    /// <summary>
    /// Checks whether this GameObject is an ancestor of <paramref name="other"/>.
    /// </summary>
    private bool IsAncestorOf(GameObject other)
    {
        GameObject? current = other._parent;
        while (current != null)
        {
            if (current == this)
                return true;
            current = current._parent;
        }
        return false;
    }
}
}
