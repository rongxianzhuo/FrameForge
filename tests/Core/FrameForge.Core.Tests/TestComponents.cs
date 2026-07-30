namespace FrameForge.Core.Tests;

/// <summary>
/// A component that tracks lifecycle calls into a shared log.
/// Configure via the <c>configure</c> parameter of <see cref="GameObject.AddComponent{T}"/>.
/// </summary>
public class TrackComponent : Component
{
    public List<string> Log = null!;
    public string Label = "";

    public override void Awake() => Log?.Add($"Awake:{Label}");
    public override void Start() => Log?.Add($"Start:{Label}");
    public override void Update() => Log?.Add($"Update:{Label}");
    public override void OnDestroy() => Log?.Add($"Destroy:{Label}");
}

/// <summary>
/// A component that spawns a new GameObject during its first Update.
/// Configure via the <c>configure</c> parameter of <see cref="GameObject.AddComponent{T}"/>.
/// </summary>
public class SpawnerComponent : Component
{
    private Scene? _scene;
    private List<string>? _log;
    private bool _spawned;

    /// <summary>
    /// Configures this spawner. Use via AddComponent's configure parameter.
    /// </summary>
    public static void Configure(Scene scene, List<string> log) { }

    public void Init(Scene scene, List<string> log)
    {
        _scene = scene;
        _log = log;
    }

    public override void Update()
    {
        if (!_spawned && _scene != null && _log != null)
        {
            _spawned = true;
            GameObject spawned = _scene.CreateGameObject("Spawned");
            spawned.AddComponent<TrackComponent>(c =>
            {
                c.Log = _log;
                c.Label = "Spawned";
            });
            _log.Add("Spawner:Spawned");
        }
    }
}

/// <summary>
/// A component that spawns multiple objects during its first Update.
/// </summary>
public class MultiSpawnerComponent : Component
{
    private Scene? _scene;
    private List<string>? _log;
    private int _count;
    private bool _spawned;

    public void Init(Scene scene, List<string> log, int count)
    {
        _scene = scene;
        _log = log;
        _count = count;
    }

    public override void Update()
    {
        if (!_spawned && _scene != null && _log != null)
        {
            _spawned = true;
            for (int i = 0; i < _count; i++)
            {
                int index = i; // Capture for lambda
                GameObject obj = _scene.CreateGameObject($"Spawned{i}");
                obj.AddComponent<TrackComponent>(c =>
                {
                    c.Log = _log;
                    c.Label = $"Spawned{index}";
                });
            }
        }
    }
}
