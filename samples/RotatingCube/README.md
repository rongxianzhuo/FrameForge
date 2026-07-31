# RotatingCube — FrameForge 第一个 Demo

## 目标

验证 FrameForge 的「逻辑与渲染分离」理念。用户只需：
1. 编写纯逻辑 Component（`RotatorComponent`）
2. 在 Unity 中放置美术资源（一个正方体模型）
3. 运行游戏 → 正方体每秒绕 Y 轴旋转一周

**零渲染代码，零 MonoBehaviour 写法。**

---

## 目录结构

```
samples/RotatingCube/
├── Logic/                       # 纯 FrameForge 逻辑（控制台可运行验证）
│   ├── Logic.csproj
│   ├── GameMain.cs              # 入口：创建 Scene + 驱动循环
│   └── RotatorComponent.cs      # 自定义组件：每秒绕 Y 轴旋转 360°
├── Unity/                       # Unity 适配器 + 使用说明
│   ├── FrameForgeHost.cs        # 适配器 MonoBehaviour
│   └── README.md                # Unity 使用步骤
└── README.md                    # 本文件
```

---

## 快速开始

### 1. 纯逻辑验证（控制台）

```bash
cd samples/RotatingCube/Logic
dotnet run
```

预期输出约 60 行，Y 轴旋转角从 0° 逐渐增加到 ≈ 360°。

### 2. Unity 运行

参见 `Unity/README.md`。

---

## 设计要点

| 关注点 | 实现 |
|--------|------|
| 逻辑层 | `RotatorComponent` 只写 `Transform.LocalRotation *= Quaternion.Euler(0, rotationAmount, 0)` |
| 适配层 | `FrameForgeHost` 读取 `FP deltaTime`，驱动 `Scene.Update()`，同步 Transform 到 Unity |
| 资源绑定 | FrameForge GameObject 与 Unity GameObject **按名称**自动匹配 |
| 用户工作 | 写 Component → 放模型 → 运行。结束。 |
