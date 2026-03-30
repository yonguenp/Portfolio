# 개발 노트 v5 (Iteration 4 - CC 3.8.8 완전 재구성)
**게임**: Star Sweeper
**작성일**: 2026-03-26
**개발봇**: AI 자동화 파이프라인 (Iteration 4)
**기준 문서**: spec_v5.md

---

## 개요

기존 씬 파일이 Cocos Creator 2.4.x 포맷이었고 CC 3.8.8에서 열리지 않아 **전면 재구성** 진행.
TitleScene을 먼저 재구성했으며, 나머지 씬(GameScene, ResultScene, ConstellationBookScene)은 **다음 봇이 이어서 작업**.

---

## CC 3.8.8 씬 파일 핵심 구조

### 1. 파일 포맷: Flat JSON Array
CC 3.8.8 `.scene` 파일은 **단일 JSON 배열**로 구성됨. 각 원소가 고유한 `__id__`(배열 인덱스)를 가지며 서로 참조.

```json
[
  { "__type__": "cc.SceneAsset", "scene": {"__id__": 1} },       // [0]
  { "__type__": "cc.Scene", "_children": [{"__id__": 2}], ... }, // [1]
  { "__type__": "cc.Node", "_name": "Canvas", ... },             // [2]
  ...
]
```

**중요**: 객체 간 참조는 항상 `{"__id__": N}` 형식. 배열 내 순서가 곧 인덱스.

### 2. 필수 최소 구조 (2D 씬)
```
[0] cc.SceneAsset
[1] cc.Scene           ← _globals→[N] 참조 필수
[2] cc.Node "Canvas"   ← _children에 Camera 포함 필수!
    ├─ [N] cc.UITransform  (Canvas 컴포넌트)
    └─ [N] cc.Canvas       ← _cameraComponent→[Camera컴포넌트__id__]
[X] cc.Node "Camera"   ← Canvas의 child node로 추가 (컴포넌트 X)
    └─ [X+1] cc.Camera
[N] cc.SceneGlobals    ← + 8개 하위 객체 (AmbientInfo 등)
```

---

## 카메라 설정 - 핵심 오류 이력

### ❌ 오류 1: cc.Camera를 Canvas의 컴포넌트로 추가
**증상**: 씬이 렌더링되지 않음 (검은 화면)
**원인**: CC 3.8.8에서 2D 카메라는 **독립 자식 노드**로 존재해야 함
**잘못된 구조**:
```json
{ "__type__": "cc.Node", "_name": "Canvas", "_components": [..., {"__id__": CAM_IDX}] }
{ "__type__": "cc.Camera", "node": {"__id__": CANVAS_IDX} }  // ← 틀림!
```
**올바른 구조**:
```json
{ "__type__": "cc.Node", "_name": "Canvas", "_children": [..., {"__id__": CAM_NODE_IDX}] }
{ "__type__": "cc.Node", "_name": "Camera", "_components": [{"__id__": CAM_COMP_IDX}] }
{ "__type__": "cc.Camera", "node": {"__id__": CAM_NODE_IDX} }
```

### ❌ 오류 2: cc.Canvas._cameraComponent 미설정
**증상**: 씬 열려도 UI 렌더링 안 됨
**수정**: cc.Canvas 컴포넌트의 `_cameraComponent` 필드에 cc.Camera 컴포넌트의 `__id__` 지정
```json
{ "__type__": "cc.Canvas", "_cameraComponent": {"__id__": CAM_COMP_IDX} }
```

### ✅ 동작하는 cc.Camera 설정값 (TitleScene 기준)
```json
{
  "__type__": "cc.Camera",
  "node": {"__id__": CAM_NODE_IDX},
  "_projection": 1,
  "_orthoHeight": 431.5657620041754,
  "_near": 1,
  "_far": 1000,
  "_color": {"__type__": "cc.Color", "r": 50, "g": 50, "b": 80, "a": 255},
  "_clearFlags": 7,
  "_visibility": 1408237568,
  "_priority": 0,
  "_rect": {"__type__": "cc.Rect", "x": 0, "y": 0, "width": 1, "height": 1}
}
```
- `_projection: 1` = ORTHO (2D 카메라)
- `_visibility: 1408237568` = UI 레이어 포함 값 (에디터에서 Visibility Layer 수동 설정 후 저장된 값)
- Camera 노드 position: `z: 1000`
- Camera 노드 `_layer: 33554432`

### ❌ 오류 3: Visibility Layer / 2D 카메라 옵션 누락
**증상**: 스크립트로 카메라 구성해도 UI가 보이지 않음
**원인**: 에디터의 [Inspector] 탭에서 Camera 컴포넌트의 "Projection" 옵션을 Orthographic으로, Visibility를 UI 레이어로 수동 선택해야 했음
**해결**: TitleScene을 에디터에서 열고 Camera 노드 선택 → Inspector에서 직접 설정 후 저장 → 저장된 값을 이후 씬에 재사용

---

## SVG → PNG 변환 이슈

**문제**: CC 3.8.8은 SVG 파일을 SpriteFrame 에셋으로 지원하지 않음
**해결**: `npx sharp-cli`를 사용해 `assets/resources/` 내 SVG 34개를 PNG로 일괄 변환

```bash
# 변환 예시
npx sharp-cli --input star.svg --output star.png
```

변환 후 CC 에디터에서 자동으로 `.meta` 파일 생성됨.

---

## SpriteFrame UUID 참조 방법

씬 JSON에서 SpriteFrame을 참조할 때 PNG 파일의 `.meta`에서 UUID를 가져와야 함.

**`.meta` 파일 구조**:
```json
{
  "subMetas": {
    "f9941": {
      "uuid": "8b764a19-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
    }
  }
}
```

**씬 JSON 참조 형식**:
```json
"_spriteFrame": {
  "__uuid__": "8b764a19-xxxx-xxxx-xxxx-xxxxxxxxxxxx@f9941",
  "__expectedType__": "cc.SpriteFrame"
}
```
→ UUID 뒤에 `@f9941` 접미사 필수.

---

## 스크립트 Short UUID 압축 알고리즘

CC 3.8.8은 스크립트를 씬 JSON에서 short UUID(23자)로 참조함.

**압축 공식**:
```
1. UUID 하이픈 제거 → 32개 hex 문자
2. 앞 5글자(hex) 보존
3. bytes[2..15] (14바이트)를 4비트 왼쪽 시프트
   shifted[i] = ((remaining[i] & 0x0f) << 4) | ((remaining[i+1] >> 4) & 0x0f)
   shifted[13] = (remaining[13] & 0x0f) << 4
4. 14바이트를 base64 인코딩 → 앞 18자 취득
5. 결과: prefix(5) + base64(18) = 23자
```

**주요 스크립트 Short UUID 목록**:
| 스크립트 | Short UUID |
|---|---|
| TitleScene | `318a6QGl8VI8JwZK0z0uBFc` |
| ResultScene | `b590cEuclFJmKYESdloOBZf` |
| GameScene | `3260fPuZHROurA57bzdi5DU` |
| ConstellationBookScene | `b36240UaWNC1666qHmN0Khm` |
| HUDController | `50d09x2vCVDDoARlHE3E7SW` |
| UIManager | `6726bcbbmVLUYSjX7bn69te` |
| WaveManager | `072d8IxbfBICIwnpcX+kTQf` |
| GameManager | `44d30jQce5LbIPdzviphlCL` |
| StarSpawner | `7dddfNgwI1BJ6QN4oelphXp` |
| ConstellationManager | `9bff7hpZhpJXKksYK2V3NKw` |
| BucketController | `c166beSRGpPGryUMYqDjCHf` |
| SceneLoader | `d1b7f4xf8NK0Lp5H4vIDCl6` |
| DataManager | `f6c20JXWSlHKKNWCDbz5DcZ` |
| AudioManager | `bde30epQ3RMYIyiBGqBaKHM` |
| StarFragment | `3bbe6uZSbVKkY82Z//KbEBv` |

---

## Button clickEvents 구조 (CC 3.8.8)

**씬 JSON에서 cc.Button의 clickEvents 형식**:
```json
"clickEvents": [
  {
    "__type__": "cc.ClickEvent",
    "target": {"__id__": SCRIPT_NODE_IDX},
    "component": "SHORT_UUID_OF_SCRIPT",
    "handler": "methodName",
    "customEventData": ""
  }
]
```

**TitleScene 버튼 연결 정보** (에디터에서 수동 설정 필요):
| 버튼 노드 | 이벤트 핸들러 | 타겟 노드 | 컴포넌트 |
|---|---|---|---|
| BookButton | `onBookButtonClicked` | TitleScriptNode | `TitleScene` |
| StartButton | `onStartButtonClicked` | TitleScriptNode | `TitleScene` |

**MCP로 설정 불가한 이유**:
`component_set_component_property`가 EventHandler 배열 타입을 지원하지 않음.
`debug_execute_script`는 내부적으로 `execute-scene-script {name: 'console', ...}` 를 호출하는데 'console' scene script가 프로젝트에 없어서 실패.
→ **에디터 Inspector에서 직접 설정** 또는 씬 JSON 파일 직접 편집 필요.

---

## TitleScene 구성 현황

**씬 파일**: `assets/scenes/TitleScene.scene`
**Library 캐시**: `library/a1/a1b2c3d4-0001-4000-8001-aabbccddeef0.json`

### 노드 구조 (Canvas 기준)
| __id__ | 이름 | 타입 | 비고 |
|---|---|---|---|
| 2 | Canvas | cc.Node | 루트 UI 노드 |
| 3 | Background | cc.Node | bg_space.png 적용 완료 |
| 6 | TitleLogo | cc.Node | logo_title.png 적용 완료 |
| 9 | TitleScriptNode | cc.Node | TitleScene 스크립트 연결 |
| 12 | BestScoreLabel | cc.Node | cc.Label [11] 참조 |
| 14 | FadeOverlay | cc.Node | UITransform+Sprite+UIOpacity+Widget |
| 19 | BookButton | cc.Node | ⚠️ clickEvents 미설정 |
| 26 | StartButton | cc.Node | ⚠️ clickEvents 미설정 |
| 33 | Camera | cc.Node | cc.Camera [34] 포함 |

### 스크립트 @property 연결 현황
```
TitleScene script [10]:
  bestScoreLabel → [11] cc.Label  ✅
  fadeOverlay    → [14] FadeOverlay 노드  ✅
  bookButton     → [19] BookButton 노드  ✅
  startButton    → 미연결 (스크립트가 @property 없이 이름으로 찾는 방식)
```

---

## 나머지 씬 재구성 가이드 (다음 봇 인계)

### ResultScene

**파일**: `assets/scenes/ResultScene.scene`
**Library**: `library/a1/a1b2c3d4-0003-4000-8003-aabbccddeef2.json`
**스크립트 Short UUID**: `b590cEuclFJmKYESdloOBZf`

**@property 연결 필요**:
```typescript
currentScoreLabel: Label    // cc.Label 컴포넌트 참조
bestScoreLabel: Label       // cc.Label 컴포넌트 참조
fadeOverlay: Node           // 노드 참조
```

**버튼 핸들러**:
- `RestartButton` → `onRestartButtonClicked`
- `TitleButton` → `onTitleButtonClicked`

**노드 구성 (권장)**:
```
Canvas
  ├── Background (Sprite: bg_space)
  ├── ResultScriptNode (ResultScene 스크립트)
  ├── CurrentScoreLabel (Label, "Score: 0")
  ├── BestScoreLabel (Label, "Best: 0")
  ├── RestartButton (Button + Sprite + Label child)
  ├── TitleButton (Button + Sprite + Label child)
  ├── FadeOverlay (Sprite, UIOpacity, Widget - 전체 화면)
  └── Camera (cc.Camera 컴포넌트)
```

---

### GameScene

**파일**: `assets/scenes/GameScene.scene`
**Library**: `library/a1/a1b2c3d4-0002-4000-8002-aabbccddeef1.json`
**스크립트 Short UUID**: `3260fPuZHROurA57bzdi5DU`

**GameScene.ts @property**:
```typescript
starSpawnerNode: Node
constellationManagerNode: Node
uiManagerNode: Node
waveManagerNode: Node
pausePanel: Node
```

**버튼 핸들러**: `onPauseButtonClicked`, `onResumeButtonClicked`, `onTitleButtonClicked`

**HUDController.ts @property** (Short UUID: `50d09x2vCVDDoARlHE3E7SW`):
```typescript
scoreLabel: Label
waveLabel: Label
maxLives: number (CCInteger, default 3)
lifeIcons: Node[]
comboPopupNode: Node
waveProgressNode: Node
waveProgressFill: Sprite
```

**UIManager.ts @property** (Short UUID: `6726bcbbmVLUYSjX7bn69te`):
```typescript
fadeOverlay: Node
wavePopupLabel: Label
fadeDuration: number (CCFloat, default 0.4)
```

**BucketController.ts @property** (Short UUID: `c166beSRGpPGryUMYqDjCHf`):
```typescript
halfWidth: number (CCFloat, default 60)
halfHeight: number (CCFloat, default 30)
```

**노드 구성 (권장)**:
```
Canvas
  ├── Background (Sprite: bg_space)
  ├── BucketNode (BucketController 스크립트)
  ├── StarSpawnerNode (StarSpawner 스크립트)
  ├── ConstellationManagerNode (ConstellationManager 스크립트)
  ├── HUDNode (HUDController 스크립트)
  │     ├── ScoreLabel (Label)
  │     ├── WaveLabel (Label)
  │     ├── LifeIconContainer (lifeIcons 배열)
  │     ├── ComboPopupNode
  │     └── WaveProgressNode
  │           └── WaveProgressFill (Sprite)
  ├── UIManagerNode (UIManager 스크립트)
  │     └── WavePopupLabel (Label)
  ├── WaveManagerNode (WaveManager 스크립트)
  ├── GameScriptNode (GameScene 스크립트)
  ├── PausePanel (PauseButton, ResumeButton, TitleButton 포함)
  ├── FadeOverlay
  └── Camera
```

---

### ConstellationBookScene

**파일**: `assets/scenes/ConstellationBookScene.scene`
**Library**: `library/16/1650c4ee-cc81-43f3-aab1-54477aafcaef.json`
**스크립트 Short UUID**: `b36240UaWNC1666qHmN0Khm`

**@property 연결 필요**:
```typescript
titleLabel: Label
cardContainer: Node
cardUnlockedPrefab: Prefab  // Prefab 에셋 - null 가능
cardLockedPrefab: Prefab    // Prefab 에셋 - null 가능
backButton: Node
fadeOverlay: Node
```

**버튼 핸들러**: `onBackButtonClicked`

**노드 구성 (권장)**:
```
Canvas
  ├── Background (Sprite: bg_space)
  ├── BookScriptNode (ConstellationBookScene 스크립트)
  ├── TitleLabelNode (Label, "별자리 도감")
  ├── CardContainer (스크롤 영역 내 카드 배치용)
  ├── BackButton (Button + Label)
  ├── FadeOverlay
  └── Camera
```

---

## MCP 서버 활용 가이드

**엔드포인트**: `http://localhost:3000/mcp` (HTTP JSON-RPC 2.0)

### 씬 작업 워크플로우
```
1. scene_open_scene → 씬 열기
2. node_find_node_by_name → UUID 확인
3. component_get_components → 컴포넌트 UUID 확인
4. node_create_node → 새 노드 생성 (parentUuid 필수)
5. component_add_component → 컴포넌트 추가
6. component_set_component_property → 속성 설정
7. component_attach_script → 스크립트 연결
8. scene_save_scene → 저장
9. project_refresh_assets → 에셋 DB 갱신
```

### component_set_component_property 지원 타입
`string`, `number`, `boolean`, `color`, `vec2`, `vec3`, `size`, `node`, `component`, `spriteFrame`, `prefab`, `asset`, `nodeArray`, `numberArray`, `stringArray`
→ **EventHandler 배열 (clickEvents) 미지원** → 에디터 직접 설정 필요

### SpriteFrame 설정 예시
```json
{
  "nodeUuid": "...",
  "componentType": "cc.Sprite",
  "property": "spriteFrame",
  "propertyType": "spriteFrame",
  "value": "8b764a19-xxxx-xxxx-xxxx-xxxxxxxxxxxx@f9941"
}
```

---

## FadeOverlay 버튼 차단 이슈 (TitleScene)

### ❌ 오류: 시작 버튼이 눌리지 않음

**증상**: 씬이 정상 렌더링되지만 StartButton / BookButton 클릭 무반응

**원인**: FadeOverlay 노드가 씬 초기값 `active: true`, `UIOpacity: 255`, 크기 `1280×720`으로 전체 화면을 덮고 있었음.
CC3에서 `UITransform`이 있는 활성 노드는 SpriteFrame이 null이어도 터치 이벤트를 차단함.

TitleScene.ts의 `start()`에서 `_fadeIn()`을 호출해 0.4초 후 `active = false`로 만들어야 하는데,
그 이전에 DataManager/AudioManager 등에서 에러가 나면 `_fadeIn()`이 실행되지 않아 FadeOverlay가 영원히 버튼을 막음.

**수정**: MCP `node_set_node_property`로 FadeOverlay 초기값을 `active: false`로 변경 후 저장
```
node_set_node_property(nodeUuid: "e75e36zlxDqK/KZps0r5dP", property: "active", value: false)
```

**FadeOverlay 올바른 초기 상태**:
- `_active: false` ← 씬 초기값
- 스크립트가 필요할 때 `active = true` → 페이드 → `active = false` 처리

**규칙 (다른 씬 구성 시 동일 적용)**:
> FadeOverlay는 항상 씬 초기값 `active: false`로 설정. 스크립트가 런타임에 직접 제어.

---

## 씬 파일 직접 편집 시 라이브러리 캐시 동기화

씬 파일을 직접 수정한 경우 library 캐시도 동일하게 복사해야 함:
```
assets/scenes/TitleScene.scene
→ library/a1/a1b2c3d4-0001-4000-8001-aabbccddeef0.json  (동일 내용)
```

그 후 MCP로:
```
project_refresh_assets → scene_open_scene
```

---

## 현재 상태 요약

| 씬 | 구성 | 스프라이트 | @property 연결 | 버튼이벤트 |
|---|---|---|---|---|
| TitleScene | ✅ 완료 | ✅ 완료 | ✅ 완료 | ⚠️ 에디터 수동 설정 필요 (clickEvents 타입 MCP 미지원) |
| ResultScene | ✅ 완료 (2026-03-27) | ✅ bg_space 적용 | ✅ currentScoreLabel, bestScoreLabel, fadeOverlay | ⚠️ 에디터 수동 설정 필요 |
| GameScene | ✅ 완료 (2026-03-27) | ✅ bg_space 적용 | ✅ 스크립트 7개 + HUD/UIManager @property | ⚠️ 에디터 수동 설정 필요 |
| ConstellationBookScene | ✅ 완료 (2026-03-27) | ✅ bg_space 적용 | ✅ titleLabel, cardContainer, backButton, fadeOverlay | ⚠️ 에디터 수동 설정 필요 |

---

## Iteration 5 작업 이력 (2026-03-27)

### 작업 방법
MCP 도구(`component_set_component_property`)의 String/Number 타입 미지원 문제로, **씬 파일 직접 JSON 작성** 방식 채택.
TitleScene.scene의 구조를 참고해 Node.js 스크립트(`C:/tmp/build_*_scene.js`)로 각 씬 파일을 프로그래밍 방식으로 생성.

### ResultScene 노드 구조
```
Canvas
  ├── Background (Sprite: bg_space, UITransform 960×640)
  ├── ResultScriptNode (b590cEuclFJmKYESdloOBZf 스크립트)
  │     @property: currentScoreLabel→[11], bestScoreLabel→[14], fadeOverlay→[29]
  ├── CurrentScoreLabel (Label "Score: 0", fontSize 36, white)
  ├── BestScoreLabel (Label "Best: 0", fontSize 28, yellow)
  ├── RestartButton (-120,-80 / 200×60 / cc.Button + Sprite)
  │     └── RestartButtonLabel ("다시 시작")
  ├── TitleButton (120,-80 / 200×60 / cc.Button + Sprite)
  │     └── TitleButtonLabel ("타이틀")
  ├── FadeOverlay (active:false / UITransform 960×640 / UIOpacity / Widget stretch)
  └── Camera (position z:1000 / cc.Camera ORTHO)
```
**검증**: debug_validate_scene → valid:true, issues:0

### GameScene 노드 구조
```
Canvas
  ├── Background (Sprite: bg_space)
  ├── BucketNode (c166beSRGpPGryUMYqDjCHf / position 0,-250)
  ├── StarSpawnerNode (7dddfNgwI1BJ6QN4oelphXp)
  ├── ConstellationManagerNode (9bff7hpZhpJXKksYK2V3NKw)
  ├── WaveManagerNode (072d8IxbfBICIwnpcX+kTQf)
  ├── GameManagerNode (44d30jQce5LbIPdzviphlCL)
  ├── HUDNode (50d09x2vCVDDoARlHE3E7SW)
  │     @property: scoreLabel→ScoreLabel, waveLabel→WaveLabel,
  │                waveProgressNode→WaveProgressNode, waveProgressFill→WaveProgressFill
  │     ├── ScoreLabel (Label "Score: 0", -380,290)
  │     ├── WaveLabel (Label "Wave: 1", 0,290)
  │     └── WaveProgressNode (UITransform 200×10)
  │           └── WaveProgressFill (Sprite: ui_progress_fill)
  ├── UIManagerNode (6726bcbbmVLUYSjX7bn69te)
  │     @property: wavePopupLabel→WavePopupLabel
  │     └── WavePopupLabel (Label "", fontSize 48)
  ├── GameScriptNode (3260fPuZHROurA57bzdi5DU)
  │     @property: starSpawnerNode, constellationManagerNode, uiManagerNode,
  │                waveManagerNode, pausePanel
  ├── PausePanel (active:false)
  │     ├── PauseButton (cc.Button "일시정지")
  │     ├── ResumeButton (cc.Button "계속")
  │     └── TitleButton (cc.Button "타이틀")
  ├── FadeOverlay (active:false)
  └── Camera (ORTHO 동일 설정)
```
**검증**: debug_validate_scene → valid:true, issues:0

### ConstellationBookScene 노드 구조
```
Canvas
  ├── Background (Sprite: bg_space)
  ├── BookScriptNode (b36240UaWNC1666qHmN0Khm)
  │     @property: titleLabel→TitleLabelNode[11], cardContainer→CardContainer[12],
  │                backButton→BackButton[15], fadeOverlay→FadeOverlay[22]
  ├── TitleLabelNode (Label "별자리 도감", fontSize 40, position 0,270)
  ├── CardContainer (UITransform 800×400)
  ├── BackButton (cc.Button "뒤로", position -350,-280)
  │     └── BackButtonLabel
  ├── FadeOverlay (active:false)
  └── Camera (ORTHO 동일 설정)
```
**검증**: debug_validate_scene → valid:true, issues:0

### 남은 작업 (에디터 수동 필요)
- 모든 씬의 Button `clickEvents` 연결 (MCP 미지원 타입)
  - ResultScene: RestartButton→onRestartButtonClicked, TitleButton→onTitleButtonClicked
  - GameScene: PauseButton→onPauseButtonClicked, ResumeButton→onResumeButtonClicked, TitleButton→onTitleButtonClicked
  - ConstellationBookScene: BackButton→onBackButtonClicked
