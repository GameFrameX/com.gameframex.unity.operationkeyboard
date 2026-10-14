<div align="center">
<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# {'en': 'GameFrameX OperationKeyboard Package', 'zh-CN': 'GameFrameX OperationKeyboard 键盘组件', 'zh-TW': 'GameFrameX OperationKeyboard 鍵盤組件', 'ja': 'GameFrameX OperationKeyboard パッケージ', 'ko': 'GameFrameX OperationKeyboard 패키지'}

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.operationkeyboard)](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.operationkeyboard)](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2019.4-black?logo=unity)](https://unity.com/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

独立游戏前后端一体化解决方案 · 独立游戏开发者的圆梦大使

<br />

[文档](https://gameframex.doc.alianblank.com) · [快速开始](#快速开始) · QQ群: 467608841 / 233840761

<br />

[English](README.md) | **简体中文** | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## 目录导航

- [项目简介](#项目简介)
- [快速开始](#快速开始)
- [使用示例](#使用示例)
- [文档与资源](#文档与资源)
- [社区与支持](#社区与支持)
- [开源协议](#开源协议)

---

## 项目简介

GameFrameX OperationKeyboard 是 GameFrameX 框架的键盘核心包：渠道注册表驱动小游戏平台键盘（微信 / 支付宝 / 快手 / 抖音的 showKeyboard 家族），UI 无关字段契约驱动输入框，静态门面持有字段会话单槽仲裁。

### 核心特性

- **字段会话单槽仲裁** - 同一时刻仅一个 `IOperationKeyboardField` 持有键盘；新字段 Attach 时自动收起键盘并结束旧字段会话（从根上修复“每字段各自订阅全局键盘事件、双字段并发即冲突”的适配器缺陷）
- **渠道注册表** - `OperationKeyboardRegistry` 注册序互斥解析、`Register` 幂等（同 id 保留首个并告警）、`Reset` 挂 `SubsystemRegistration`、工厂懒实例化并缓存
- **maxLength 恒 -1** - 小游戏键盘按拼音组合串计数，硬上限会在拼音组合中途掐断中文输入（真机实证）；统一传 -1 不限长，长度裁剪留给游戏侧
- **未注册渠道 no-op** - Editor / 原生平台 / 未装小游戏 SDK 的工程：门面不拦截任何事件，原生 `TouchScreenKeyboard` 路径完整保留

### 系统要求

- Unity 2019.4 或更高版本

---

## 快速开始

### 安装

选择以下任一方式：

1. 编辑 Unity 项目的 `Packages/manifest.json`，添加 `scopedRegistries` 部分：
   ```json
   {
     "scopedRegistries": [
       {
         "name": "GameFrameX",
         "url": "https://gameframex.upm.alianblank.uk",
         "scopes": [
           "com.gameframex"
         ]
       }
     ],
     "dependencies": {
       "com.gameframex.unity.operationkeyboard": "1.0.0"
     }
   }
   ```

   `scopes` 控制哪些包通过此注册表解析。只有以 `com.gameframex` 开头的包才会从这个注册表获取。

2. 直接在 `manifest.json` 的 `dependencies` 节点下添加以下内容：
   ```json
   {
      "com.gameframex.unity.operationkeyboard": "https://github.com/gameframex/com.gameframex.unity.operationkeyboard.git"
   }
   ```
3. 在 Unity 的 `Package Manager` 中使用 `Git URL` 的方式添加库，地址为： `https://github.com/gameframex/com.gameframex.unity.operationkeyboard.git`
4. 直接下载仓库放置到 Unity 项目的 `Packages` 目录下，会自动加载识别。

## 使用示例

### 基本使用

```csharp
using GameFrameX.OperationKeyboard.Runtime;

// UI 绑定包（com.gameframex.unity.operationkeyboard.ugui）在聚焦 / 失焦时自动驱动会话，
// 游戏代码通常不需要直接调用门面。

// 键盘拉起期间的程序化更新
OperationKeyboard.UpdateValue("程序赋值");

// 键盘高度供避让布局消费（0 = 收起；无此 API 的平台为空实现）
OperationKeyboard.OnHeightChanged(height => { /* 在此抬升 UI */ });
```

### 字段会话与单槽仲裁

`OperationKeyboard.Attach(field)` 从字段契约推导键盘参数（`DefaultValue` = 字段当前文本、`MaxLength` 恒 -1、`Multiple`、`ConfirmType`），并顶替已持有会话的旧字段（`Hide()` + 对旧字段触发一次 `TriggerEndEdit()`）。事件流与原生移动端一致，业务回调零改动：

- **input** - `field.Text = value`（`onValueChanged` 照常触发）+ 光标置尾；
- **confirm / complete** - 终值落定、键盘收起、触发 `EndEdit`（与原生 Done 一致）；
- **失焦** - `Detach`（收起键盘；失焦 `EndEdit` 由字段自身失焦链路产生，不重复触发）。

### 渠道注册（Bootstrap 自注册）

各渠道包通过自身的 `XxxOperationKeyboardBootstrap`（`RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`）把渠道工厂注册进 `OperationKeyboardRegistry`；未安装的渠道包被宏墙裁剪，注册不会发生（引用即注册）。未注册任何渠道时门面 no-op——原生键盘路径就是兜底。

### UI 绑定轴

注册表回答“哪个平台键盘”，UI 绑定包回答“哪个 UI 工具集”——两轴正交，组合即自动适配。`com.gameframex.unity.operationkeyboard.ugui` 绑定 `InputField` + `TMP_InputField` 并自带自动挂载器（零面板改动）；FairyGUI 绑定另行发布。

### 主要类和接口

- **`IOperationKeyboardManager`**：平台契约 `Show(OperationKeyboardShowOption)` / `Hide()` / `UpdateValue(string)` 与单槽事件 `OnInput` / `OnConfirm` / `OnComplete` / `OnHeightChanged`；`Hide` 时必须注销全部平台事件。
- **`IOperationKeyboardField`**：UI 无关字段契约 `Text` / `IsFocused` / `Multiline` / `ConfirmType` / `MoveTextEnd()` / `TriggerEndEdit()`。
- **`OperationKeyboardShowOption`**：拉起参数；`MaxLength` 恒为 -1。
- **`OperationKeyboardRegistry`** / **`OperationKeyboardChannelRegistration`**：渠道注册表与注册描述（id + 展示名 + 工厂）。
- **`OperationKeyboard`**：静态门面；`Attach` / `Detach` / `CurrentField` 单槽会话，`Hide` / `UpdateValue` / `OnHeightChanged` 转发。

### 平台支持

| 平台 | 实现 |
|---|---|
| Editor / Standalone / 移动原生 | 门面 no-op，原生 `TouchScreenKeyboard` 路径保留 |
| 微信 / 支付宝 / 快手 / 抖音小游戏 | 渠道包 |

---

## 文档与资源

- **完整文档**: [https://gameframex.doc.alianblank.com](https://gameframex.doc.alianblank.com)
- **问题反馈**: [GitHub Issues](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/issues)

---

## 社区与支持

- **QQ 讨论群**: [467608841](https://qm.qq.com/cgi-bin/qm/qr?k=sYFd1nv6m2KZIWFLorZ5pBR0AE5ZhbuL&jump_from=webapi&authKey=oCu+uoL3n35fT5SEt7iLgGtROPxh31n/rHUxRlp0w1f+j38W4tKBuWyRH3KEdwHN)
- **功能建议**: [GitHub Discussions](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/discussions)

---

## 依赖
| 包 | 说明 |
|----|------|
| com.gameframex.unity | GameFrameX 核心 |

## 更新日志

查看 [Releases](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/releases) 了解更新日志。

## 开源协议

详见 [LICENSE.md](LICENSE.md) 文件。
