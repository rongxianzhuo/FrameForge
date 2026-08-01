using FrameForge.Core;
using FrameForge.Foundation.FixedPoint;
using UnityEngine;

namespace FrameForge.Adapters.Unity
{
    /// <summary>
    /// FrameForge 与 Unity 之间的桥梁。
    /// 挂到任意 Unity GameObject 上，自动：
    /// 1. 驱动 FrameForge Scene 的 Update 循环
    /// 2. 将 FP Transform 同步到 Unity Transform（渲染）
    /// 3. 按名称自动匹配 FrameForge GameObject ↔ Unity GameObject
    /// </summary>
    /// <remarks>
    /// 约定：FrameForge GameObject.Name 必须与场景中的 Unity GameObject.name 完全匹配。
    /// 不匹配的对象不会绑定，也不会报错（允许有纯逻辑对象不做渲染）。
    /// </remarks>
    public class FrameForgeHost : MonoBehaviour
    {
        /// <summary>
        /// FrameForge 场景实例。
        /// </summary>
        public Scene Scene { get; private set; } = null!;

        /// <summary>
        /// FrameForge GameObject → Unity GameObject 的映射表。
        /// Key 由 FindByName 维护，Value 在 Awake 时扫描场景建立。
        /// </summary>
        private readonly Dictionary<string, UnityEngine.GameObject> _unityObjects = new();

        private void Awake()
        {
            // 扫描 Unity 场景中所有 GameObject，建立名称索引
            var allObjects = UnityEngine.Object.FindObjectsByType<UnityEngine.GameObject>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var obj in allObjects)
            {
                if (!_unityObjects.ContainsKey(obj.name))
                {
                    _unityObjects[obj.name] = obj;
                }
            }

            // 创建 FrameForge Scene 并构建逻辑
            Scene = new Scene();
            BuildScene();
        }

        private void Update()
        {
            // 将 Unity 的 float deltaTime 转为 FP
            // 注意：FP.FromFloat 标记为 ⚠️ 仅编辑器/工具用
            // 此处从 Unity Time.deltaTime 读取是不可避免的桥接
            FP deltaTime = FP.FromFloat(UnityEngine.Time.deltaTime);

            // 驱动 FrameForge 场景
            RotatingCube.Logic.RotatorComponent.DeltaTime = deltaTime;
            Scene.Update();

            // 同步 Transform
            SyncTransforms();
        }

        /// <summary>
        /// 构建 FrameForge 逻辑场景。
        /// 覆写此方法添加自定义逻辑对象和组件。
        /// </summary>
        protected virtual void BuildScene()
        {
            // Demo: 创建一个旋转正方体
            var cube = Scene.CreateGameObject("Cube");
            cube.Transform.LocalPosition = new FrameForge.Foundation.Math.Vector3(
                FP.Zero, FP.Zero, FP.FromInt(5));
            cube.AddComponent<RotatingCube.Logic.RotatorComponent>();
        }

        /// <summary>
        /// 将所有匹配的 FrameForge Transform 同步到 Unity Transform。
        /// 仅同步在双方都存在的同名对象。
        /// </summary>
        private void SyncTransforms()
        {
            foreach (var root in Scene.RootObjects)
            {
                SyncGameObjectRecursive(root);
            }
        }

        /// <summary>
        /// 递归同步单个 GameObject 及其子对象的 Transform。
        /// </summary>
        private void SyncGameObjectRecursive(FrameForge.Core.GameObject ffObj)
        {
            // 查找匹配的 Unity 对象
            if (_unityObjects.TryGetValue(ffObj.Name, out var unityObj))
            {
                var ffTransform = ffObj.Transform;
                var unityTransform = unityObj.transform;

                // FP → float 转换（仅渲染用，不参与逻辑）
                unityTransform.localPosition = new UnityEngine.Vector3(
                    (float)(double)ffTransform.LocalPosition.X,
                    (float)(double)ffTransform.LocalPosition.Y,
                    (float)(double)ffTransform.LocalPosition.Z);

                unityTransform.localRotation = new UnityEngine.Quaternion(
                    (float)(double)ffTransform.LocalRotation.X,
                    (float)(double)ffTransform.LocalRotation.Y,
                    (float)(double)ffTransform.LocalRotation.Z,
                    (float)(double)ffTransform.LocalRotation.W);

                unityTransform.localScale = new UnityEngine.Vector3(
                    (float)(double)ffTransform.LocalScale.X,
                    (float)(double)ffTransform.LocalScale.Y,
                    (float)(double)ffTransform.LocalScale.Z);
            }

            // 递归子对象
            foreach (var child in ffObj.Children)
            {
                SyncGameObjectRecursive(child);
            }
        }
    }
}
