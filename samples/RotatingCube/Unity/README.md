# Unity 适配层使用说明

## 前置条件

1. Unity 2022.3 或更高版本（支持 .NET Standard 2.1）
2. FrameForge 框架已编译（`dotnet build` 在仓库根目录）

## Unity 工程设置

### 1. 创建 Unity 工程

打开 Unity Hub → 新建 3D 项目。

### 2. 复制适配器代码

将本目录下的 `FrameForgeHost.cs` 复制到 `Assets/Scripts/FrameForge/`。

### 3. 引用 FrameForge DLL

将以下 DLL 复制到 `Assets/Plugins/FrameForge/`：

```
src/Foundation/FrameForge.FixedPoint/bin/Release/net10.0/FrameForge.Foundation.FixedPoint.dll
src/Foundation/FrameForge.Math/bin/Release/net10.0/FrameForge.Foundation.Math.dll
src/Core/FrameForge.Core/bin/Release/net10.0/FrameForge.Core.dll
samples/RotatingCube/Logic/bin/Release/net10.0/RotatingCube.Logic.dll
```

> 或者使用 Unity 的 Package Manager 引用本地项目。

### 4. 搭建场景

1. 在 Hierarchy 中创建一个空 GameObject，命名为 `"FrameForge"` → 挂上 `FrameForgeHost.cs`
2. 创建一个 `Cube`（GameObject → 3D Object → Cube），**确保名称恰好为 `"Cube"`**
3. 将 Cube 放在场景中合适的位置（初始位置会被 FrameForge 逻辑覆盖）

### 5. 运行

点击 Play → Cube 应每秒绕 Y 轴旋转一周。

---

## 工作原理

```
Unity Update
    │
    ▼
FrameForgeHost.Update()
    ├── 1. 测量 Time.deltaTime → FP
    ├── 2. Scene.Update()  ← 驱动所有 Component
    │       └── RotatorComponent.Update()
    │           └── Transform.Rotation *= Quaternion.Euler(0, 2π×dt, 0)
    └── 3. SyncTransforms()  ← FP → float, 写入 Unity Transform
            └── Unity 渲染
```

用户只需：
- 在 Unity 中放美术资源（模型、贴图）
- 确保名称匹配
- 其余全部由 FrameForge 逻辑驱动
