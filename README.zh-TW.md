<div align="center">
<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# {'en': 'GameFrameX OperationKeyboard Package', 'zh-CN': 'GameFrameX OperationKeyboard 键盘组件', 'zh-TW': 'GameFrameX OperationKeyboard 鍵盤組件', 'ja': 'GameFrameX OperationKeyboard パッケージ', 'ko': 'GameFrameX OperationKeyboard 패키지'}

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.operationkeyboard)](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.operationkeyboard)](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2019.4-black?logo=unity)](https://unity.com/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

獨立遊戲前後端一體化解決方案 · 獨立遊戲開發者的圓夢大使

<br />

[文檔](https://gameframex.doc.alianblank.com) · [快速開始](#快速開始) · QQ群: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | **繁體中文** | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## 目錄導航

- [項目簡介](#項目簡介)
- [快速開始](#快速開始)
- [使用範例](#使用範例)
- [文檔與資源](#文檔與資源)
- [社區與支援](#社區與支援)
- [開源協議](#開源協議)

---

## 項目簡介

GameFrameX OperationKeyboard 是 GameFrameX 框架的鍵盤核心包：渠道註冊表驅動小遊戲平台鍵盤（微信 / 支付寶 / 快手 / 抖音的 showKeyboard 家族），UI 無關字段契約驅動輸入框，靜態門面持有字段會話單槽仲裁。

### 核心特性

- **字段會話單槽仲裁** - 同一時刻僅一個 `IOperationKeyboardField` 持有鍵盤；新字段 Attach 時自動收起鍵盤並結束舊字段會話（從根上修復“每字段各自訂閱全局鍵盤事件、雙字段並發即衝突”的適配器缺陷）
- **渠道註冊表** - `OperationKeyboardRegistry` 註冊序互斥解析、`Register` 冪等（同 id 保留首個並告警）、`Reset` 掛 `SubsystemRegistration`、工廠懶實例化並緩存
- **maxLength 恆 -1** - 小遊戲鍵盤按拼音組合串計數，硬上限會在拼音組合中途掐斷中文輸入（真機實證）；統一傳 -1 不限長，長度裁剪留給遊戲側
- **未註冊渠道 no-op** - Editor / 原生平台 / 未裝小遊戲 SDK 的工程：門面不攔截任何事件，原生 `TouchScreenKeyboard` 路徑完整保留

### 系統需求

- Unity 2019.4 或更高版本

---

## 快速開始

### 安裝

選擇以下任一方式：

1. 編輯 Unity 項目的 `Packages/manifest.json`，添加 `scopedRegistries` 部分：
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

   `scopes` 控制哪些包通過此註冊表解析。只有以 `com.gameframex` 開頭的包才會從這個註冊表獲取。

2. 直接在 `manifest.json` 的 `dependencies` 節點下添加以下內容：
   ```json
   {
      "com.gameframex.unity.operationkeyboard": "https://github.com/gameframex/com.gameframex.unity.operationkeyboard.git"
   }
   ```
3. 在 Unity 的 `Package Manager` 中使用 `Git URL` 的方式添加庫，地址為： `https://github.com/gameframex/com.gameframex.unity.operationkeyboard.git`
4. 直接下載倉庫放置到 Unity 項目的 `Packages` 目錄下，會自動加載識別。

## 使用範例

### 基本使用

```csharp
using GameFrameX.OperationKeyboard.Runtime;

// UI 綁定包（com.gameframex.unity.operationkeyboard.ugui）在聚焦 / 失焦時自動驅動會話，
// 遊戲代碼通常不需要直接調用門面。

// 鍵盤拉起期間的程序化更新
OperationKeyboard.UpdateValue("程序賦值");

// 鍵盤高度供避讓佈局消費（0 = 收起；無此 API 的平台為空實現）
OperationKeyboard.OnHeightChanged(height => { /* 在此抬升 UI */ });
```

### 字段會話與單槽仲裁

`OperationKeyboard.Attach(field)` 從字段契約推導鍵盤參數（`DefaultValue` = 字段當前文本、`MaxLength` 恆 -1、`Multiple`、`ConfirmType`），並頂替已持有會話的舊字段（`Hide()` + 對舊字段觸發一次 `TriggerEndEdit()`）。事件流與原生移動端一致，業務回調零改動：

- **input** - `field.Text = value`（`onValueChanged` 照常觸發）+ 光標置尾；
- **confirm / complete** - 終值落定、鍵盤收起、觸發 `EndEdit`（與原生 Done 一致）；
- **失焦** - `Detach`（收起鍵盤；失焦 `EndEdit` 由字段自身失焦鏈路產生，不重複觸發）。

### 渠道註冊（Bootstrap 自註冊）

各渠道包通過自身的 `XxxOperationKeyboardBootstrap`（`RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`）把渠道工廠註冊進 `OperationKeyboardRegistry`；未安裝的渠道包被宏牆裁剪，註冊不會發生（引用即註冊）。未註冊任何渠道時門面 no-op——原生鍵盤路徑就是兜底。

### UI 綁定軸

註冊表回答“哪個平台鍵盤”，UI 綁定包回答“哪個 UI 工具集”——兩軸正交，組合即自動適配。`com.gameframex.unity.operationkeyboard.ugui` 綁定 `InputField` + `TMP_InputField` 並自帶自動掛載器（零面板改動）；FairyGUI 綁定另行發布。

### 主要類和介面

- **`IOperationKeyboardManager`**：平台契約 `Show(OperationKeyboardShowOption)` / `Hide()` / `UpdateValue(string)` 與單槽事件 `OnInput` / `OnConfirm` / `OnComplete` / `OnHeightChanged`；`Hide` 時必須註銷全部平台事件。
- **`IOperationKeyboardField`**：UI 無關字段契約 `Text` / `IsFocused` / `Multiline` / `ConfirmType` / `MoveTextEnd()` / `TriggerEndEdit()`。
- **`OperationKeyboardShowOption`**：拉起參數；`MaxLength` 恆為 -1。
- **`OperationKeyboardRegistry`** / **`OperationKeyboardChannelRegistration`**：渠道註冊表與註冊描述（id + 展示名 + 工廠）。
- **`OperationKeyboard`**：靜態門面；`Attach` / `Detach` / `CurrentField` 單槽會話，`Hide` / `UpdateValue` / `OnHeightChanged` 轉發。

### 平台支援

| 平台 | 實現 |
|---|---|
| Editor / Standalone / 移動原生 | 門面 no-op，原生 `TouchScreenKeyboard` 路徑保留 |
| 微信 / 支付寶 / 快手 / 抖音小遊戲 | 渠道包 |

---

## 文檔與資源

- **完整文檔**: [https://gameframex.doc.alianblank.com](https://gameframex.doc.alianblank.com)
- **問題反饋**: [GitHub Issues](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/issues)

---

## 社區與支援

- **QQ 討論群**: [467608841](https://qm.qq.com/cgi-bin/qm/qr?k=sYFd1nv6m2KZIWFLorZ5pBR0AE5ZhbuL&jump_from=webapi&authKey=oCu+uoL3n35fT5SEt7iLgGtROPxh31n/rHUxRlp0w1f+j38W4tKBuWyRH3KEdwHN)
- **功能建議**: [GitHub Discussions](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/discussions)

---

## 依賴
| 包 | 說明 |
|----|------|
| com.gameframex.unity | GameFrameX 核心 |

## 更新日誌

查看 [Releases](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/releases) 了解更新日誌。

## 開源協議

詳見 [LICENSE.md](LICENSE.md) 文件。
