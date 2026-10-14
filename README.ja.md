<div align="center">
<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# {'en': 'GameFrameX OperationKeyboard Package', 'zh-CN': 'GameFrameX OperationKeyboard 键盘组件', 'zh-TW': 'GameFrameX OperationKeyboard 鍵盤組件', 'ja': 'GameFrameX OperationKeyboard パッケージ', 'ko': 'GameFrameX OperationKeyboard 패키지'}

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.operationkeyboard)](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.operationkeyboard)](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2019.4-black?logo=unity)](https://unity.com/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

インディーゲーム開発のオールインワンソリューション · インディーデベロッパーの夢を応援

<br />

[ドキュメント](https://gameframex.doc.alianblank.com) · [クイックスタート](#クイックスタート) · QQグループ: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | **日本語** | [한국어](README.ko.md)

</div>

## 目次

- [プロジェクト概要](#プロジェクト概要)
- [クイックスタート](#クイックスタート)
- [使用例](#使用例)
- [ドキュメントとリソース](#ドキュメントとリソース)
- [コミュニティとサポート](#コミュニティとサポート)
- [ライセンス](#ライセンス)

---

## プロジェクト概要

GameFrameX OperationKeyboard は GameFrameX フレームワークのキーボードコアパッケージです。チャネルレジストリがミニゲーム平台キーボード（微信 / Alipay / Kuaishou / Douyin の showKeyboard 系）を駆動し、UI 非依存のフィールド契約が入力フィールドを駆動し、静的ファサードがフィールドセッションの単一スロット調停を担います。

### 主な機能

- **フィールドセッション単一スロット調停** - 同時にキーボードを保持できる `IOperationKeyboardField` は 1 つだけ。新しいフィールドの Attach 时にキーボードを自動で畳み、旧フィールドのセッションを終了します（フィールドごとにグローバルキーボードイベントを購読して競合する従来アダプタの欠陥を根本から解消）
- **チャネルレジストリ** - `OperationKeyboardRegistry` は登録順の排他解決、冪等な `Register`（同 id は先着優先かつ警告）、`SubsystemRegistration` での `Reset`、ファクトリの遅延インスタンス化とキャッシュを提供
- **maxLength は常に -1** - ミニゲームキーボードはピンイン組成文字列をカウントするため、ハード上限は中国語入力の組成中に切断します（実機検証済み）。常に -1 を渡し、長さ制限はゲーム側に委ねます
- **チャネル未登録時は no-op** - Editor / ネイティブ平台 / ミニゲーム SDK 未導入のプロジェクトでは、ファサードは何もインターセプトせず、ネイティブ `TouchScreenKeyboard` の経路が完全に保持されます

### システム要件

- Unity 2019.4 以上

---

## クイックスタート

### インストール

以下のいずれかの方法を選択してください：

1. Unity プロジェクトの `Packages/manifest.json` を編集し、`scopedRegistries` セクションを追加します：
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

   `scopes` はこのレジストリで解決されるパッケージを制御します。`com.gameframex` で始まるパッケージのみこのレジストリから取得されます。

2. `manifest.json` の `dependencies` に以下を追加します：
   ```json
   {
      "com.gameframex.unity.operationkeyboard": "https://github.com/gameframex/com.gameframex.unity.operationkeyboard.git"
   }
   ```
3. Unity の `Package Manager` で `Git URL` を使って追加します： `https://github.com/gameframex/com.gameframex.unity.operationkeyboard.git`
4. リポジトリをクローンして Unity プロジェクトの `Packages` ディレクトリに配置すると自動的に読み込まれます。

## 使用例

### 基本的な使い方

```csharp
using GameFrameX.OperationKeyboard.Runtime;

// UI バインディングパッケージ（com.gameframex.unity.operationkeyboard.ugui）が
// フォーカス / ブラー時にセッションを自動駆動するため、ゲームコードが直接
// ファサードを呼ぶことは通常ありません。

// キーボード表示中のプログラムによる更新
OperationKeyboard.UpdateValue("プログラム値");

// 回避レイアウト向けのキーボード高度（0 = 畳まれた状態；API が無い平台は空実装）
OperationKeyboard.OnHeightChanged(height => { /* ここで UI を持ち上げる */ });
```

### フィールドセッションと単一スロット調停

`OperationKeyboard.Attach(field)` はフィールド契約からキーボードオプションを導出し（`DefaultValue` = フィールドの現在テキスト、`MaxLength` は常に -1、`Multiple`、`ConfirmType`）、既存フィールドのセッションを排他的に終了します（`Hide()` + 旧フィールドに 1 回 `TriggerEndEdit()`）。イベントフローはネイティブモバイルと一致し、ビジネスコールバックの変更は不要です：

- **input** - `field.Text = value`（`onValueChanged` は通常どおり発火）+ キャレットを末尾へ；
- **confirm / complete** - 最終値の確定、キーボードを畳む、`EndEdit` を発火（ネイティブの Done と同じ）；
- **ブラー** - `Detach`（キーボードを畳む；ブラー時の `EndEdit` はフィールド自身のブラー経路が生成し、重複発火しません）。

### チャネル登録（Bootstrap 自己登録）

各チャネルパッケージは自身の `XxxOperationKeyboardBootstrap`（`RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`）でファクトリを `OperationKeyboardRegistry` に登録します。未インストールのチャネルはマクロ壁で除去され、登録は発生しません（参照即登録）。何も登録されていない場合ファサードは no-op であり、ネイティブのキーボード経路がフォールバックです。

### UI バインディング軸

レジストリは「どの平台キーボードか」を、UI バインディングパッケージは「どの UI ツールキットか」を答えます。2 つの軸は直交し、組み合わせで自動適応を実現します。`com.gameframex.unity.operationkeyboard.ugui` は `InputField` + `TMP_InputField` を自動バインダーで束ねます（パネル変更ゼロ）。FairyGUI バインディングは別途提供されます。

### 主要なクラスとインターフェース

- **`IOperationKeyboardManager`**：平台契約 `Show(OperationKeyboardShowOption)` / `Hide()` / `UpdateValue(string)` と単一スロットイベント `OnInput` / `OnConfirm` / `OnComplete` / `OnHeightChanged`。`Hide` 时に全ての平台イベントを解除しなければなりません。
- **`IOperationKeyboardField`**：UI 非依存フィールド契約 `Text` / `IsFocused` / `Multiline` / `ConfirmType` / `MoveTextEnd()` / `TriggerEndEdit()`。
- **`OperationKeyboardShowOption`**：表示パラメータ。`MaxLength` は常に -1。
- **`OperationKeyboardRegistry`** / **`OperationKeyboardChannelRegistration`**：チャネルレジストリと登録記述子（id + 表示名 + ファクトリ）。
- **`OperationKeyboard`**：静的ファサード。`Attach` / `Detach` / `CurrentField` の単一スロットセッション、`Hide` / `UpdateValue` / `OnHeightChanged` の転送。

### プラットフォーム対応

| プラットフォーム | 実装 |
|---|---|
| Editor / Standalone / モバイルネイティブ | ファサード no-op、ネイティブ `TouchScreenKeyboard` 経路を保持 |
| 微信 / Alipay / Kuaishou / Douyin ミニゲーム | チャネルパッケージ |

---

## ドキュメントとリソース

- **ドキュメント**: [https://gameframex.doc.alianblank.com](https://gameframex.doc.alianblank.com)
- **課題報告**: [GitHub Issues](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/issues)

---

## コミュニティとサポート

- **QQ グループ**: [467608841](https://qm.qq.com/cgi-bin/qm/qr?k=sYFd1nv6m2KZIWFLorZ5pBR0AE5ZhbuL&jump_from=webapi&authKey=oCu+uoL3n35fT5SEt7iLgGtROPxh31n/rHUxRlp0w1f+j38W4tKBuWyRH3KEdwHN)
- **機能リクエスト**: [GitHub Discussions](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/discussions)

---

## 依存関係
| パッケージ | 説明 |
|----|------|
| com.gameframex.unity | GameFrameX コア |

## 変更履歴

更新履歴は [Releases](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/releases) を参照してください。

## ライセンス

ライセンスの詳細は [LICENSE.md](LICENSE.md) を参照してください。
