using FrameForge.Core;
using FrameForge.Foundation.FixedPoint;
using FrameForge.Foundation.Math;

namespace RotatingCube.Logic
{
    /// <summary>
    /// 每秒绕 Y 轴旋转 360° 的组件。
    /// </summary>
    /// <remarks>
    /// deltaTime 通过静态属性 <see cref="DeltaTime"/> 从外部注入。
    /// </remarks>
    public class RotatorComponent : Component
    {
        /// <summary>
        /// 每帧在 Update 前由 GameMain 设置的 deltaTime。
        /// </summary>
        public static FP DeltaTime { get; set; }

        public override void Update()
        {
            // 每秒旋转 360 度
            FP rotationDegreesPerSecond = FP.FromInt(360);
            FP rotationThisFrame = rotationDegreesPerSecond * DeltaTime;
            Transform.LocalRotation *= Quaternion.Euler(FP.Zero, rotationThisFrame, FP.Zero);
        }
    }
}
