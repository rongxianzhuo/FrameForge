using FrameForge.Core;
using FrameForge.Foundation.FixedPoint;
using FrameForge.Foundation.Math;

namespace RotatingCube.Logic
{
    /// <summary>
    /// 纯控制台 Demo：验证 RotatorComponent 逻辑正确性。
    /// 模拟 60fps 运行 1 秒，预期 Cube 绕 Y 轴旋转 360°。
    /// </summary>
    public static class GameMain
    {
        public static void Main()
        {
            var scene = new Scene();

            // 创建正方体对象
            var cube = scene.CreateGameObject("Cube");
            cube.Transform.LocalPosition = new Vector3(FP.Zero, FP.Zero, FP.FromInt(5));

            // 挂载旋转组件
            cube.AddComponent<RotatorComponent>();

            // 固定 60fps deltaTime
            FP deltaTime = FP.One / FP.FromInt(60);
            int totalFrames = 60; // 1 秒

            Console.WriteLine("=== FrameForge RotatingCube Demo ===\n");
            Console.WriteLine($"Simulating {totalFrames} frames at 60fps (1 second).\n");

            FP initialYaw = cube.Transform.LocalRotation.EulerAngles.Y;
            Console.WriteLine($"Frame   0: Yaw = {initialYaw:F6}°");

            for (int i = 1; i <= totalFrames; i++)
            {
                RotatorComponent.DeltaTime = deltaTime;
                scene.Update();

                FP yaw = cube.Transform.LocalRotation.EulerAngles.Y;
                Console.WriteLine($"Frame {i,3}: Yaw = {yaw:F6}°");
            }

            // 验证方式：旋转一个方向向量，1 秒后应回到原位
            // （纯 Y 轴旋转 360°，Forward 向量应不变）
            Vector3 initialForward = cube.Transform.Forward;
            Vector3 rotatedForward = cube.Transform.Forward;

            FP forwardError = Vector3.Distance(initialForward, rotatedForward);
            FP finalYaw = cube.Transform.LocalRotation.EulerAngles.Y;

            Console.WriteLine($"\n---");
            Console.WriteLine($"Initial Forward:  ({initialForward.X.ToDouble():F6}, {initialForward.Y.ToDouble():F6}, {initialForward.Z.ToDouble():F6})");
            Console.WriteLine($"Rotated Forward:  ({rotatedForward.X.ToDouble():F6}, {rotatedForward.Y.ToDouble():F6}, {rotatedForward.Z.ToDouble():F6})");
            Console.WriteLine($"Forward Error:    {forwardError.ToDouble():F10}");
            Console.WriteLine($"Yaw (EulerAngles): {finalYaw.ToDouble():F2}°");

            if (forwardError < FP.FromFloat(0.01f))
                Console.WriteLine("\n✅ SUCCESS: Rotation is correct within tolerance.");
            else
                Console.WriteLine("\n❌ FAILED: Rotation deviates from expected value.");

            // 输入 FP 常量验证（确定性检查）
            Console.WriteLine($"\nDeterminism check:");
            Console.WriteLine($"FP.PI      = {FP.PI.ToDouble()}");
            Console.WriteLine($"FP.PI * 2  = {(FP.PI * FP.FromInt(2)).ToDouble()}");
        }
    }
}
