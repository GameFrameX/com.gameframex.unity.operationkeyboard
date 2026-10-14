<div align="center">
<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# {'en': 'GameFrameX OperationKeyboard Package', 'zh-CN': 'GameFrameX OperationKeyboard 键盘组件', 'zh-TW': 'GameFrameX OperationKeyboard 鍵盤組件', 'ja': 'GameFrameX OperationKeyboard パッケージ', 'ko': 'GameFrameX OperationKeyboard 패키지'}

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.operationkeyboard)](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.operationkeyboard)](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2019.4-black?logo=unity)](https://unity.com/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

All-in-One Solution for Indie Game Development · Empowering Indie Developers' Dreams

<br />

[Documentation](https://gameframex.doc.alianblank.com) · [Quick Start](#quick-start) · QQ Group: 467608841 / 233840761

<br />

**English** | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## Table of Contents

- [Project Overview](#project-overview)
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
- [Documentation & Resources](#documentation--resources)
- [Community & Support](#community--support)
- [License](#license)

---

## Project Overview

GameFrameX OperationKeyboard is the keyboard core package of the GameFrameX framework: a channel registry drives the mini-game platform keyboards (WeChat / Alipay / Kuaishou / Douyin `showKeyboard` family), a UI-agnostic field contract drives input fields, and the static facade owns single-slot field arbitration.

### Key Features

- **Single-Slot Field Arbitration** - only one `IOperationKeyboardField` holds the keyboard at a time; attaching a new field automatically hides the keyboard and ends the previous field's session (fixes the double-subscription conflict of per-field adapters at the root)
- **Channel Registry** - `OperationKeyboardRegistry` with registration-order mutually exclusive resolution, idempotent `Register` (first wins with a warning), `Reset` at `SubsystemRegistration`, lazy factory instantiation with caching
- **maxLength Always -1** - mini-game keyboards count the pinyin composition string, so a hard cap truncates Chinese input mid-composition (verified on device); the cap is always passed as -1 and length clamping is left to game code
- **No-Op Without Channel** - Editor / native platforms / projects without a mini-game SDK: the facade intercepts nothing and the native `TouchScreenKeyboard` path is fully preserved

### System Requirements

- Unity 2019.4 or higher

---

## Quick Start

### Installation

Choose one of the following methods:

1. Edit your Unity project's `Packages/manifest.json` and add the `scopedRegistries` section:
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

   `scopes` controls which packages are resolved through this registry. Only packages whose names start with `com.gameframex` will be fetched from it.

2. Add to `manifest.json` dependencies:
   ```json
   {
      "com.gameframex.unity.operationkeyboard": "https://github.com/gameframex/com.gameframex.unity.operationkeyboard.git"
   }
   ```
3. Use **Package Manager** in Unity with **Git URL**: `https://github.com/gameframex/com.gameframex.unity.operationkeyboard.git`
4. Clone the repository into your Unity project's `Packages` directory. It will be loaded automatically.

## Usage Examples

### Basic Usage

```csharp
using GameFrameX.OperationKeyboard.Runtime;

// UI binding packages (com.gameframex.unity.operationkeyboard.ugui) drive the session
// automatically on focus / blur - game code normally never touches the facade directly.

// Programmatic update while the keyboard is shown
OperationKeyboard.UpdateValue("programmatic value");

// Keyboard height for avoidance layout (0 = collapsed; empty implementation on platforms without the API)
OperationKeyboard.OnHeightChanged(height => { /* lift the UI here */ });
```

### Field Session and Single-Slot Arbitration

`OperationKeyboard.Attach(field)` derives the keyboard option from the field contract (`DefaultValue` = field text, `MaxLength` always -1, `Multiple`, `ConfirmType`) and preempts any previous field (`Hide()` + one `TriggerEndEdit()` on the old field). The event flow mirrors native mobile so business callbacks need zero changes:

- **input** - `field.Text = value` (`onValueChanged` fires as usual) + caret moved to the end;
- **confirm / complete** - final value assigned, keyboard hidden, `EndEdit` triggered (same as the native Done button);
- **blur** - `Detach` (keyboard hidden; the blur `EndEdit` is produced by the field's own blur chain, never duplicated).

### Channel Registration (Bootstrap Self-Registration)

Each channel package registers its factory into `OperationKeyboardRegistry` via its own `XxxOperationKeyboardBootstrap` (`RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`); uninstalled channel packages are stripped by the macro wall, so no registration happens (referencing means registering). When nothing is registered the facade is a no-op - the native keyboard path IS the fallback.

### UI Binding Axis

The registry answers "which platform keyboard"; UI binding packages answer "which UI toolkit" - the two axes are orthogonal and combine into automatic adaptation. `com.gameframex.unity.operationkeyboard.ugui` binds `InputField` + `TMP_InputField` with an auto-binder (zero panel changes); the FairyGUI binding ships separately.

### Main Classes and Interfaces

- **`IOperationKeyboardManager`**: platform contract `Show(OperationKeyboardShowOption)` / `Hide()` / `UpdateValue(string)` + single-slot events `OnInput` / `OnConfirm` / `OnComplete` / `OnHeightChanged`; `Hide` must unregister every platform event.
- **`IOperationKeyboardField`**: UI-agnostic field contract `Text` / `IsFocused` / `Multiline` / `ConfirmType` / `MoveTextEnd()` / `TriggerEndEdit()`.
- **`OperationKeyboardShowOption`**: show parameters; `MaxLength` is always -1.
- **`OperationKeyboardRegistry`** / **`OperationKeyboardChannelRegistration`**: channel registry and registration descriptor (id + display name + factory).
- **`OperationKeyboard`**: static facade; `Attach` / `Detach` / `CurrentField` single-slot session, `Hide` / `UpdateValue` / `OnHeightChanged` forwarding.

### Platform Support

| Platform | Implementation |
|---|---|
| Editor / Standalone / Mobile native | facade no-op, native `TouchScreenKeyboard` path preserved |
| WeChat / Alipay / Kuaishou / Douyin Mini Games | channel packages |

---

## Documentation & Resources

- **Documentation**: [https://gameframex.doc.alianblank.com](https://gameframex.doc.alianblank.com)
- **Issue Tracker**: [GitHub Issues](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/issues)

---

## Community & Support

- **QQ Group**: [467608841](https://qm.qq.com/cgi-bin/qm/qr?k=sYFd1nv6m2KZIWFLorZ5pBR0AE5ZhbuL&jump_from=webapi&authKey=oCu+uoL3n35fT5SEt7iLgGtROPxh31n/rHUxRlp0w1f+j38W4tKBuWyRH3KEdwHN)
- **Feature Requests**: [GitHub Discussions](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/discussions)

---

## Dependencies
| Package | Description |
|----|------|
| com.gameframex.unity | GameFrameX core |

## Changelog

See [Releases](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/releases) for changelog.

## License

See [LICENSE.md](LICENSE.md) for license information.
