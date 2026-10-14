<div align="center">
<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# {'en': 'GameFrameX OperationKeyboard Package', 'zh-CN': 'GameFrameX OperationKeyboard 键盘组件', 'zh-TW': 'GameFrameX OperationKeyboard 鍵盤組件', 'ja': 'GameFrameX OperationKeyboard パッケージ', 'ko': 'GameFrameX OperationKeyboard 패키지'}

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.operationkeyboard)](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.operationkeyboard)](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2019.4-black?logo=unity)](https://unity.com/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

인디 게임 개발 올인원 솔루션 · 인디 개발자의 꿈을 응원

<br />

[문서](https://gameframex.doc.alianblank.com) · [빠른 시작](#빠른-시작) · QQ그룹: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | **한국어**

</div>

## 목차

- [프로젝트 개요](#프로젝트-개요)
- [빠른 시작](#빠른-시작)
- [사용 예시](#사용-예시)
- [문서 및 자료](#문서-및-자료)
- [커뮤니티 및 지원](#커뮤니티-및-지원)
- [라이선스](#라이선스)

---

## 프로젝트 개요

GameFrameX OperationKeyboard는 GameFrameX 프레임워크의 키보드 코어 패키지입니다. 채널 레지스트리가 미니게임 플랫폼 키보드(위챗 / 알리페이 / 콰이쇼우 / 더우인의 showKeyboard 계열)를 구동하고, UI 비의존 필드 계약이 입력 필드를 구동하며, 정적 파사드가 필드 세션 단일 슬롯 조정을 담당합니다.

### 주요 기능

- **필드 세션 단일 슬롯 조정** - 동시에 키보드를 보유한 `IOperationKeyboardField`는 하나뿐입니다. 새 필드 Attach 시 키보드를 자동으로 접고 기존 필드의 세션을 종료합니다(필드마다 전역 키보드 이벤트를 구독해 동시 사용 시 충돌하던 기존 어댑터 결함을 근본적으로 해결)
- **채널 레지스트리** - `OperationKeyboardRegistry`는 등록 순 상호 배타적 해석, 멱등한 `Register`(같은 id는 선등록 우선 + 경고), `SubsystemRegistration` 시점의 `Reset`, 팩토리 지연 인스턴스화 및 캐싱 제공
- **maxLength 항상 -1** - 미니게임 키보드는 병음 조합 문자열을 기준으로 세기 때문에 하드 상한은 중국어 입력이 조합 중에 잘리는 현상을 낳습니다(실기 검증). 항상 -1을 전달하고 길이 제한은 게임 측에 맡깁니다
- **채널 미등록 시 no-op** - Editor / 네이티브 플랫폼 / 미니게임 SDK 미설치 프로젝트에서는 파사드가 아무것도 가로채지 않으며 네이티브 `TouchScreenKeyboard` 경로가 온전히 유지됩니다

### 시스템 요구사항

- Unity 2019.4 이상

---

## 빠른 시작

### 설치

다음 방법 중 하나를 선택하세요:

1. Unity 프로젝트의 `Packages/manifest.json`을 편집하여 `scopedRegistries` 섹션을 추가합니다:
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

   `scopes`는 이 레지스트리를 통해 해석되는 패키지를 제어합니다. `com.gameframex`로 시작하는 패키지만 이 레지스트리에서 가져옵니다.

2. `manifest.json`의 `dependencies`에 다음을 추가합니다:
   ```json
   {
      "com.gameframex.unity.operationkeyboard": "https://github.com/gameframex/com.gameframex.unity.operationkeyboard.git"
   }
   ```
3. Unity의 `Package Manager`에서 `Git URL`로 추가합니다: `https://github.com/gameframex/com.gameframex.unity.operationkeyboard.git`
4. 저장소를 클론하여 Unity 프로젝트의 `Packages` 디렉토리에 배치하면 자동으로 로드됩니다.

## 사용 예시

### 기본 사용법

```csharp
using GameFrameX.OperationKeyboard.Runtime;

// UI 바인딩 패키지(com.gameframex.unity.operationkeyboard.ugui)가 포커스 / 블러 시
// 세션을 자동 구동하므로 게임 코드가 직접 파사드를 호출할 일은 usually 없습니다.

// 키보드 표시 중 프로그래밍 방식 갱신
OperationKeyboard.UpdateValue("프로그램 값");

// 회피 레이아웃용 키보드 높이(0 = 접힘; 해당 API가 없는 플랫폼은 빈 구현)
OperationKeyboard.OnHeightChanged(height => { /* 여기서 UI를 들어올림 */ });
```

### 필드 세션과 단일 슬롯 조정

`OperationKeyboard.Attach(field)`는 필드 계약에서 키보드 옵션을 도출하고(`DefaultValue` = 필드 현재 텍스트, `MaxLength` 항상 -1, `Multiple`, `ConfirmType`), 이미 세션을 보유한 기존 필드를 밀어냅니다(`Hide()` + 기존 필드에 1회 `TriggerEndEdit()`). 이벤트 흐름은 네이티브 모바일과 동일하여 비즈니스 콜백 변경이 필요 없습니다:

- **input** - `field.Text = value`(`onValueChanged`는 평소처럼 발생) + 캐럿을 끝으로 이동;
- **confirm / complete** - 최종 값 확정, 키보드 접기, `EndEdit` 발생(네이티브 Done과 동일);
- **블러** - `Detach`(키보드 접기; 블러 시 `EndEdit`는 필드 자신의 블러 경로가 생성하며 중복 발생하지 않음).

### 채널 등록 (Bootstrap 자가 등록)

각 채널 패키지는 자체 `XxxOperationKeyboardBootstrap`(`RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`)로 팩토리를 `OperationKeyboardRegistry`에 등록합니다. 미설치 채널은 매크로 벽으로 제거되어 등록이 발생하지 않습니다(참조 곧 등록). 아무것도 등록되지 않으면 파사드는 no-op이며 네이티브 키보드 경로가 폴백입니다.

### UI 바인딩 축

레지스트리는 "어느 플랫폼 키보드인가"를, UI 바인딩 패키지는 "어느 UI 툴킷인가"를 답합니다. 두 축은 직교하며 조합되어 자동 적응을 이룹니다. `com.gameframex.unity.operationkeyboard.ugui`는 `InputField` + `TMP_InputField`를 자동 바인더로 묶습니다(패널 변경 없음). FairyGUI 바인딩은 별도 제공됩니다.

### 주요 클래스와 인터페이스

- **`IOperationKeyboardManager`**: 플랫폼 계약 `Show(OperationKeyboardShowOption)` / `Hide()` / `UpdateValue(string)` 및 단일 슬롯 이벤트 `OnInput` / `OnConfirm` / `OnComplete` / `OnHeightChanged`. `Hide` 시 모든 플랫폼 이벤트를 해제해야 합니다.
- **`IOperationKeyboardField`**: UI 비의존 필드 계약 `Text` / `IsFocused` / `Multiline` / `ConfirmType` / `MoveTextEnd()` / `TriggerEndEdit()`.
- **`OperationKeyboardShowOption`**: 표시 파라미터. `MaxLength`는 항상 -1.
- **`OperationKeyboardRegistry`** / **`OperationKeyboardChannelRegistration`**: 채널 레지스트리와 등록 기술자(id + 표시명 + 팩토리).
- **`OperationKeyboard`**: 정적 파사드. `Attach` / `Detach` / `CurrentField` 단일 슬롯 세션, `Hide` / `UpdateValue` / `OnHeightChanged` 전달.

### 플랫폼 지원

| 플랫폼 | 구현 |
|---|---|
| Editor / Standalone / 모바일 네이티브 | 파사드 no-op, 네이티브 `TouchScreenKeyboard` 경로 유지 |
| 위챗 / 알리페이 / 콰이쇼우 / 더우인 미니게임 | 채널 패키지 |

---

## 문서 및 자료

- **문서**: [https://gameframex.doc.alianblank.com](https://gameframex.doc.alianblank.com)
- **이슈 제보**: [GitHub Issues](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/issues)

---

## 커뮤니티 및 지원

- **QQ 그룹**: [467608841](https://qm.qq.com/cgi-bin/qm/qr?k=sYFd1nv6m2KZIWFLorZ5pBR0AE5ZhbuL&jump_from=webapi&authKey=oCu+uoL3n35fT5SEt7iLgGtROPxh31n/rHUxRlp0w1f+j38W4tKBuWyRH3KEdwHN)
- **기능 제안**: [GitHub Discussions](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/discussions)

---

## 의존성
| 패키지 | 설명 |
|----|------|
| com.gameframex.unity | GameFrameX 코어 |

## 변경 로그

변경 이력은 [Releases](https://github.com/GameFrameX/com.gameframex.unity.operationkeyboard/releases)를 참조하세요.

## 라이선스

라이선스에 대한 자세한 내용은 [LICENSE.md](LICENSE.md)를 참조하세요.
