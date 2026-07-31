namespace FrameForge.Core {

/// <summary>
/// Base class for all logic components. Each <see cref="Component"/> is attached to
/// exactly one <see cref="GameObject"/> and participates in the deterministic
/// lifecycle driven by the <see cref="Scene"/>.
/// </summary>
/// <remarks>
/// Components are updated in the order they were added to their GameObject.
/// Override the lifecycle methods to add behaviour:
/// <list type="bullet">
///   <item><see cref="Awake"/> — called immediately when the component is created or added</item>
///   <item><see cref="Start"/> — called once before the first <see cref="Update"/></item>
///   <item><see cref="Update"/> — called every frame while enabled and active</item>
///   <item><see cref="OnDestroy"/> — called when the component is destroyed (frame-end)</item>
/// </list>
/// </remarks>
public class Component
{
    // ========================================================================
    // Backing Fields
    // ========================================================================

    private GameObject _gameObject = null!;
    private bool _enabled = true;
    private bool _awakeCalled;
    private bool _started;

    // ========================================================================
    // Properties
    // ========================================================================

    /// <summary>
    /// The <see cref="GameObject"/> this component is attached to.
    /// Never null after the component is added to a GameObject.
    /// </summary>
    public GameObject GameObject
    {
        get => _gameObject;
        internal set => _gameObject = value;
    }

    /// <summary>
    /// Shortcut to <see cref="GameObject"/>'s <see cref="Transform"/>.
    /// </summary>
    public Transform Transform => _gameObject.Transform;

    /// <summary>
    /// Whether this component is enabled. Disabled components do not receive
    /// <see cref="Update"/> calls, but still receive <see cref="Awake"/>,
    /// <see cref="Start"/>, and <see cref="OnDestroy"/>.
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    /// <summary>
    /// Whether <see cref="Awake"/> has been called on this component.
    /// </summary>
    internal bool AwakeCalled
    {
        get => _awakeCalled;
        set => _awakeCalled = value;
    }

    /// <summary>
    /// Whether <see cref="Start"/> has been called on this component.
    /// </summary>
    internal bool Started
    {
        get => _started;
        set => _started = value;
    }

    // ========================================================================
    // Lifecycle Methods (virtual — override in subclasses)
    // ========================================================================

    /// <summary>
    /// Called immediately after the component is created or added to a GameObject,
    /// before the first frame. Use for initialization that does not depend on
    /// other components being fully initialised.
    /// </summary>
    public virtual void Awake() { }

    /// <summary>
    /// Called once before the first <see cref="Update"/>, after all components
    /// have had <see cref="Awake"/> called. Use for initialisation that depends
    /// on other components.
    /// </summary>
    public virtual void Start() { }

    /// <summary>
    /// Called every frame while the component is enabled and its GameObject is active.
    /// Components on the same GameObject are updated in attachment order.
    /// </summary>
    public virtual void Update() { }

    /// <summary>
    /// Called when the component (or its GameObject) is destroyed, at the end
    /// of the current frame. Use for cleanup.
    /// </summary>
    public virtual void OnDestroy() { }

    // ========================================================================
    // Convenience Methods
    // ========================================================================

    /// <summary>
    /// Returns the first component of type <typeparamref name="T"/> attached to
    /// the same <see cref="GameObject"/>, or null if none is found.
    /// </summary>
    public T? GetComponent<T>() where T : Component
    {
        return _gameObject.GetComponent<T>();
    }

    /// <summary>
    /// Returns all components of type <typeparamref name="T"/> attached to
    /// the same <see cref="GameObject"/>.
    /// </summary>
    public List<T> GetComponents<T>() where T : Component
    {
        return _gameObject.GetComponents<T>();
    }
}
}
