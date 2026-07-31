using FrameForge.Foundation.FixedPoint;

namespace FrameForge.Adapters.Unity;

/// <summary>
/// Provides deterministic deltaTime to FrameForge components at runtime.
/// The adapter (<c>FrameForgeHost</c>) sets <see cref="DeltaTime"/>
/// every frame from Unity's <c>Time.deltaTime</c> before calling
/// <see cref="FrameForge.Core.Scene.Update"/>.
/// </summary>
/// <remarks>
/// <para>Components that need per-frame time should read <see cref="DeltaTime"/>
/// directly in their <see cref="FrameForge.Core.Component.Update"/> method.</para>
///
/// <para>
///   <b>Why is this in the Adapter layer?</b>
///   FrameForge.Core is intentionally free of a built-in clock — different
///   adapters (Unity, Godot, headless console) all have different time sources.
///   <c>TimeProvider</c> sits in the adapter and bridges Unity's floating-point
///   clock into the FP world. Components reference it by importing
///   <c>FrameForge.Adapters.Unity</c>.
/// </para>
///
/// <para>
///   <b>Determinism note:</b>
///   <see cref="DeltaTime"/> is derived from <c>UnityEngine.Time.deltaTime</c>
///   via <see cref="FP.FromFloat"/>. This conversion loses some precision and
///   is inherently non-deterministic across frame rates. For lockstep networking
///   or replay use-cases, use a fixed timestep via <c>FrameForgeHost.FixedDeltaTime</c>.
/// </para>
/// </remarks>
public static class TimeProvider
{
    /// <summary>
    /// Delta time for the current frame, in seconds, as <see cref="FP"/>.
    /// Read this in <see cref="FrameForge.Core.Component.Update"/> for
    /// frame-rate-independent logic.
    /// </summary>
    /// <example>
    /// <code>
    /// public override void Update()
    /// {
    ///     FP speed = FP.FromInt(10);
    ///     Transform.LocalPosition += Vector3.Forward * speed * TimeProvider.DeltaTime;
    /// }
    /// </code>
    /// </example>
    public static FP DeltaTime { get; internal set; }

    /// <summary>
    /// The raw <c>UnityEngine.Time.deltaTime</c> value that was used to
    /// produce <see cref="DeltaTime"/> this frame.
    /// For diagnostics and debugging only — do NOT use in game logic.
    /// </summary>
    public static float RawUnityDeltaTime { get; internal set; }
}
