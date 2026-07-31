using System.Collections.Generic;
using FrameForge.Core;
using FrameForge.Foundation.FixedPoint;
using FrameForge.Foundation.Math;
using UnityEngine;
using UTransform = UnityEngine.Transform;
using UGameObject = UnityEngine.GameObject;
using UObject = UnityEngine.Object;

namespace FrameForge.Adapters.Unity
{

/// <summary>
/// The bridge MonoBehaviour that connects FrameForge's logical scene to Unity's
/// rendering engine. Attach this to a single GameObject in your Unity scene.
/// </summary>
/// <remarks>
/// <para><b>How it works</b></para>
/// <para>
/// Every frame (in <see cref="Update"/>), <c>FrameForgeHost</c>:
/// <list type="number">
///   <item>
///     Reads <c>UnityEngine.Time.deltaTime</c>, converts it to
///     <see cref="FP"/>, and stores it in <see cref="TimeProvider.DeltaTime"/>.
///   </item>
///   <item>
///     Calls <see cref="Scene.Update"/> to drive the entire FrameForge
///     lifecycle: Awake → Start → Update → OnDestroy.
///   </item>
///   <item>
///     Synchronises all matching FrameForge <see cref="Transform"/> values
///     (position, rotation, scale) to their Unity counterparts so the
///     renderer can draw the result.
///   </item>
/// </list>
/// </para>
///
/// <para><b>Name-based binding convention</b></para>
/// <para>
/// FrameForge <see cref="GameObject"/> instances are matched to Unity
/// <see cref="UGameObject"/> instances by <b>name</b>. If a FrameForge object
/// named <c>"Player"</c> exists, the adapter looks for a Unity GameObject
/// also named <c>"Player"</c>. The match is case-sensitive.
/// </para>
/// <para>
/// Objects that exist only on one side are silently ignored — this allows
/// pure-logic objects (no visual representation) and pure-visual objects
/// (decoration, UI) to coexist.
/// </para>
///
/// <para><b>Lifecycle mapping</b></para>
/// <list type="table">
///   <listheader>
///     <term>Unity</term>
///     <description>FrameForge</description>
///   </listheader>
///   <item>
///     <term><c>Awake()</c></term>
///     <description>
///       Scans the Unity scene for all GameObjects, builds the name→object
///       lookup table, creates the FrameForge <see cref="Scene"/>,
///       calls <see cref="BuildScene"/>.
///     </description>
///   </item>
///   <item>
///     <term><c>Start()</c></term>
///     <description>
///       (Reserved for future use — currently a no-op.)
///     </description>
///   </item>
///   <item>
///     <term><c>Update()</c></term>
///     <description>
///       Sets <see cref="TimeProvider.DeltaTime"/>, calls
///       <see cref="Scene.Update"/> (which calls Start / Update on all
///       FrameForge components), then syncs Transforms.
///     </description>
///   </item>
///   <item>
///     <term><c>OnDestroy()</c></term>
///     <description>
///       (FrameForge cleanup happens inside <see cref="Scene.Update"/>
///       via deferred destruction at frame-end.)
///     </description>
///   </item>
/// </list>
/// </remarks>
public class FrameForgeHost : MonoBehaviour
{
    // ========================================================================
    // Fields
    // ========================================================================

    // --- Configuration ---

    /// <summary>
    /// If true, <see cref="Update"/> uses <c>Time.fixedDeltaTime</c> instead
    /// of <c>Time.deltaTime</c> to produce deterministic, frame-rate-independent
    /// logic ticks. Enable this for lockstep networking or deterministic replays.
    /// Defaults to false.
    /// </summary>
    /// <remarks>
    /// When enabled, every call to <see cref="Update"/> uses the same deltaTime.
    /// The physics timestep in Unity (<c>Edit → Project Settings → Time → Fixed Timestep</c>)
    /// controls the actual tick rate.
    /// </remarks>
    [SerializeField]
    [Tooltip("Use Time.fixedDeltaTime for deterministic, frame-rate-independent ticks.")]
    public bool FixedDeltaTime;

    // --- Runtime ---

    /// <summary>
    /// The FrameForge <see cref="Scene"/> driving all logic this frame.
    /// Available after <see cref="Awake"/>.
    /// </summary>
    public Scene Scene { get; private set; } = null!;

    /// <summary>
    /// Lookup table: Unity GameObject name → Unity GameObject.
    /// Built once in <see cref="Awake"/> by scanning the scene.
    /// </summary>
    private readonly Dictionary<string, UGameObject> _unityObjects = new();

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    /// <summary>
    /// Scans all Unity GameObjects in the scene (including inactive ones),
    /// builds the name lookup, creates the FrameForge Scene, and calls
    /// <see cref="BuildScene"/>.
    /// </summary>
    protected virtual void Awake()
    {
        // Step 1 — Scan Unity scene, index by name
        // Note: in case of duplicate names, the last one wins.
        var allObjects = UObject.FindObjectsByType<UGameObject>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var obj in allObjects)
        {
            _unityObjects[obj.name] = obj;
        }

        // Step 2 — Create the FrameForge logical scene
        Scene = new Scene();

        // Step 3 — Let the user build their logic
        BuildScene();
    }

    /// <summary>
    /// Unity's main-loop callback. Called every rendered frame.
    /// <list type="number">
    ///   <item>Compute deltaTime (fixed or variable) → <see cref="TimeProvider"/></item>
    ///   <item>Drive <see cref="Scene.Update"/></item>
    ///   <item>Synchronise all Transforms to Unity</item>
    /// </list>
    /// </summary>
    protected virtual void Update()
    {
        // Step 1 — Bridge Unity's float deltaTime into FP world
        float rawDt = FixedDeltaTime
            ? UnityEngine.Time.fixedDeltaTime
            : UnityEngine.Time.deltaTime;

        TimeProvider.RawUnityDeltaTime = rawDt;
        TimeProvider.DeltaTime = FP.FromFloat(rawDt);

        // Step 2 — Drive the FrameForge logical scene
        Scene.Update();

        // Step 3 — Push FP transforms back to Unity for rendering
        SyncTransforms();
    }

    // ========================================================================
    // Extensibility — Override in subclass to build your logic scene
    // ========================================================================

    /// <summary>
    /// Override this method to create your FrameForge <see cref="GameObject"/>
    /// instances and attach <see cref="Component"/> instances.
    /// Called once during <see cref="Awake"/>, after the Scene is created.
    /// </summary>
    /// <example>
    /// <code>
    /// protected override void BuildScene()
    /// {
    ///     var player = Scene.CreateGameObject("Player");
    ///     player.Transform.LocalPosition = new Vector3(FP.Zero, FP.One, FP.Zero);
    ///     player.AddComponent&lt;MyPlayerController&gt;();
    /// }
    /// </code>
    /// </example>
    protected virtual void BuildScene()
    {
        // No-op by default. Users override this to set up their logic.
    }

    // ========================================================================
    // Transform Synchronisation
    // ========================================================================

    /// <summary>
    /// Walks all root FrameForge GameObjects (and their children recursively)
    /// and pushes their <see cref="FrameForge.Core.Transform"/> values to the
    /// matching Unity <see cref="UTransform"/>.
    /// </summary>
    /// <remarks>
    /// Only objects that exist in <em>both</em> the FrameForge scene and the
    /// Unity scene (matched by name) are synchronised.
    /// </remarks>
    private void SyncTransforms()
    {
        foreach (var root in Scene.RootObjects)
        {
            SyncGameObjectRecursive(root);
        }
    }

    /// <summary>
    /// Recursively synchronises a single FrameForge <see cref="GameObject"/>
    /// and all its children to Unity.
    /// </summary>
    /// <param name="ffObj">The FrameForge GameObject to sync.</param>
    private void SyncGameObjectRecursive(FrameForge.Core.GameObject ffObj)
    {
        // Try to find a matching Unity object by name
        if (_unityObjects.TryGetValue(ffObj.Name, out var unityObj))
        {
            var ffTransform = ffObj.Transform;
            var unityTransform = unityObj.transform;

            // Local position: FP → float (safe — this is render-only, not logic)
            unityTransform.localPosition = new UnityEngine.Vector3(
                (float)(double)ffTransform.LocalPosition.X,
                (float)(double)ffTransform.LocalPosition.Y,
                (float)(double)ffTransform.LocalPosition.Z);

            // Local rotation: FP → float
            unityTransform.localRotation = new UnityEngine.Quaternion(
                (float)(double)ffTransform.LocalRotation.X,
                (float)(double)ffTransform.LocalRotation.Y,
                (float)(double)ffTransform.LocalRotation.Z,
                (float)(double)ffTransform.LocalRotation.W);

            // Local scale: FP → float
            unityTransform.localScale = new UnityEngine.Vector3(
                (float)(double)ffTransform.LocalScale.X,
                (float)(double)ffTransform.LocalScale.Y,
                (float)(double)ffTransform.LocalScale.Z);
        }

        // Recurse into children
        foreach (var child in ffObj.Children)
        {
            SyncGameObjectRecursive(child);
        }
    }
}
}
