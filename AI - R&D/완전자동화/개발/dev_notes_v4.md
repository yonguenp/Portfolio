# 개발 노트 v4 (Iteration 3)
**게임**: Star Sweeper
**작성일**: 2026-03-26
**개발봇**: AI 자동화 파이프라인 (Iteration 3)
**기준 문서**: spec_v4.md, design_notes_v4.md, QA/latest_report.md (9.4/10)

---

## 수정/추가된 파일 목록

| 파일 | 작업 구분 | 수정 항목 |
|---|---|---|
| `assets/scenes/TitleScene.scene` | **수정** | M-NEW-01: MissingScript → cc.TitleScene 교체, BookButton 노드 신규 추가, bestScoreLabel 연결 |
| `assets/scenes/ConstellationBookScene.scene` | **수정** | n-02: BackButton clickEvents[0].component "ConstellationBookScene" 명시 |
| `assets/scripts/ConstellationBookScene.ts` | **수정** | n-01: _createFallbackCard() UITransform 추가 |
| `assets/scripts/HUDController.ts` | **수정** | spec_v4 신규: waveProgressNode/waveProgressFill @property, updateWaveProgress() 메서드 추가 |
| `assets/scripts/AudioManager.ts` | **수정** | spec_v4 신규: sfxProgressComplete @property, playProgressComplete() 메서드 추가 |

---

## 주요 변경 사항 설명

### [Major - M-NEW-01] TitleScene.scene MissingScript 수정 + bookButton 노드 추가

**문제**: TitleScriptNode의 component가 `cc.MissingScript` 상태 → TitleScene.ts 연결 완전 끊김.
bookButton 노드 없음, bestScoreLabel null.

**수정 내용** (`assets/scenes/TitleScene.scene`):
- `[5]` cc.MissingScript → **`cc.TitleScene`** 교체
  - `bestScoreLabel: {"__id__": 14}` 연결 (cc.Label 컴포넌트)
  - `fadeOverlay: {"__id__": 21}` 유지 (FadeOverlay 노드)
  - `bookButton: {"__id__": 26}` 신규 연결 (BookButton 노드)
- **BookButton 노드 신규 추가** (`[26]`, y=-140):
  - UITransform (200×60), Button 컴포넌트 (`[28]`, `[29]`)
  - clickEvents[0]: target=TitleScriptNode, component="TitleScene", handler="onBookButtonClicked"
  - BookButtonLabel 자식 노드 (`[27]`): Label "별자리 도감", fontSize=24
- Canvas children 배열에 `{"__id__": 26}` 추가
- Scene globals 참조 수정: `__id__: 29 → 32` (노드 삽입으로 인한 인덱스 밀림 반영)
- SceneGlobals 내부 참조 수정: ambient(30→33), shadows(31→34), skybox(32→35), fog(33→36), octree(34→37), skin(35→38), lightProbeInfo(36→39)

**결과**: M-NEW-01 해소. 도감 버튼 클릭 동작 및 bestScoreLabel 표시 정상화 예정.

---

### [Minor - n-02] ConstellationBookScene.scene BackButton component 명시

**문제**: clickEvents[0].component 필드 빈 문자열 → 런타임 핸들러 탐색 불안정.

**수정 내용** (`assets/scenes/ConstellationBookScene.scene`):
- BackButton → cc.Button 컴포넌트의 `clickEvents[0].component`: `""` → `"ConstellationBookScene"` 명시

**결과**: 뒤로가기 버튼 핸들러(onBackButtonClicked) 런타임 탐색 안정화.

---

### [Minor - n-01] ConstellationBookScene._createFallbackCard() UITransform 추가

**문제**: `new Node()` 생성 시 UITransform 미추가 → 레이아웃 그리드 내 크기 0×0.

**수정 내용** (`assets/scripts/ConstellationBookScene.ts`):
- import에 `UITransform` 추가
- `_createFallbackCard()` 내부:
  ```typescript
  const uiTransform = node.addComponent(UITransform);
  uiTransform.setContentSize(200, 120);
  ```
  contentSize를 design_notes_v4 권장 크기 200×120 기준으로 설정.

**결과**: Prefab 미연결 fallback 카드가 레이아웃 그리드에서 정상 크기로 배치됨.

---

### [신규 기능] Wave 진행도 표시 시스템 (spec_v4 메카닉 4)

#### HUDController.ts

**추가된 @property**:
```typescript
@property({ type: Node })
waveProgressNode: Node | null = null;   // ui_progress_bg.svg 배경 노드

@property({ type: Sprite })
waveProgressFill: Sprite | null = null; // ui_progress_fill.svg 채움 Sprite
```

**추가된 메서드**:
- `_initWaveProgress()`: start() 시 진행도 바 너비 0으로 초기화
- `updateWaveProgress(current: number, total: number)`:
  - `waveProgressFill` 노드의 UITransform.width를 `(current/total) * 120` px로 설정
  - `current >= total` (Wave 클리어) 시: 0.3초 tween으로 width=120 → `AudioManager.playProgressComplete()` → 0.5초 후 width=0 초기화
  - 일반 갱신: 즉시 너비 설정

**import 추가**: `UITransform`, `AudioManager`

#### AudioManager.ts

**추가된 @property**:
```typescript
@property({ type: AudioClip })
sfxProgressComplete: AudioClip | null = null;
```

**추가된 메서드**:
```typescript
playProgressComplete() { this._playSFX(this.sfxProgressComplete); }
```

---

## 알려진 제한사항

| 항목 | 내용 |
|---|---|
| n-03 (미수정) | ConstellationBookScene의 cardUnlockedPrefab/cardLockedPrefab null — 에디터에서 Prefab 연결 필요 (이번 iteration 범위 외) |
| waveProgressNode 씬 연결 | GameScene.scene에 ui_progress_bg/fill 노드 추가 및 HUDController @property 연결은 에디터 수동 작업 필요 |
| updateWaveProgress 호출 시점 | GameManager/ConstellationManager 내 `hud.updateWaveProgress()` 호출 로직 추가 필요 (GameManager 수정 별도 작업) |
| sfxProgressComplete | AudioClip 에셋 없음 — 에디터에서 AudioClip 연결 필요 |
| tween width 애니메이션 | UITransform.width에 직접 tween을 적용하므로 Cocos 3.8.8 tween 타입 추론 이슈로 `as any` 캐스팅 사용 |
| 빌드 | CocosCreator.exe 미탐지로 자동빌드 불가 — 수동 빌드 필요 |

---

## 에디터에서 수동 연결 필요 항목

| 씬/컴포넌트 | 연결 항목 | 내용 |
|---|---|---|
| **GameScene** | `HUDController.waveProgressNode` | ui_progress_bg.svg Sprite 노드 |
| **GameScene** | `HUDController.waveProgressFill` | ui_progress_fill.svg Sprite 컴포넌트 (anchorPoint 0, 0.5 권장) |
| **AudioManager** | `AudioManager.sfxProgressComplete` | Wave 진행도 완료 SFX AudioClip |
| **ConstellationBookScene** | `cardUnlockedPrefab` / `cardLockedPrefab` | card_constellation.svg / card_locked.svg 기반 Prefab |
| **GameScene** | `ConstellationManager` → `HUDController` 참조 | updateWaveProgress() 호출 경로 |

---

## QA봇에게 전달할 테스트 포인트

### [M-NEW-01 검증]
1. TitleScene 로드 시 TitleScriptNode에 `cc.TitleScene` 컴포넌트가 정상 부착되는지 확인 (MissingScript 아이콘 없어야 함)
2. BestScoreLabel에 최고 점수가 정상 표시되는지 확인 (`DataManager.loadBestScore()` 결과)
3. BookButton 클릭 시 `onBookButtonClicked()` 호출 → ConstellationBookScene으로 페이드 전환되는지 확인
4. TitleScene.scene JSON 배열 인덱스 정합성 확인 (globals → 32, SceneGlobals 내부 refs → 33~39)

### [n-02 검증]
5. ConstellationBookScene에서 뒤로가기 버튼 클릭 시 `onBackButtonClicked()` 정상 호출 → TitleScene 복귀 확인

### [n-01 검증]
6. Prefab 미연결 상태(cardUnlockedPrefab=null)에서 도감 씬 진입 시 fallback 카드들이 레이아웃 그리드에서 200×120 크기로 정상 배치되는지 확인

### [Wave 진행도 바 검증]
7. GameScene HUD에 waveProgressNode/waveProgressFill 연결 후 `updateWaveProgress(3, 7)` 호출 시 너비 약 51px로 설정되는지 확인
8. `updateWaveProgress(7, 7)` 호출 시 0.3초 내 width=120 tween → playProgressComplete() 호출 → 0.5초 후 width=0 초기화 흐름 확인
9. AudioManager.sfxProgressComplete 미연결 시 null 체크로 오류 없이 무시되는지 확인

---

## 완료 체크리스트

- [x] M-NEW-01: TitleScene.scene MissingScript → cc.TitleScene 교체
- [x] M-NEW-01: TitleScene.scene bookButton 노드(BookButton) 추가
- [x] M-NEW-01: TitleScene.scene bestScoreLabel 연결 (__id__:14 → cc.Label)
- [x] M-NEW-01: TitleScene.scene bookButton @property 연결 (__id__:26)
- [x] M-NEW-01: TitleScene.scene clickEvents component "TitleScene" 명시
- [x] M-NEW-01: TitleScene.scene SceneGlobals 인덱스 참조 수정
- [x] n-02: ConstellationBookScene.scene BackButton clickEvents[0].component 명시
- [x] n-01: ConstellationBookScene.ts _createFallbackCard() UITransform 추가
- [x] spec_v4: HUDController.ts waveProgressNode/waveProgressFill @property 추가
- [x] spec_v4: HUDController.ts updateWaveProgress() 메서드 구현
- [x] spec_v4: HUDController.ts _initWaveProgress() 구현
- [x] spec_v4: AudioManager.ts sfxProgressComplete @property 추가
- [x] spec_v4: AudioManager.ts playProgressComplete() 메서드 추가
- [x] dev_notes_v4.md 작성
- [ ] GameScene.scene waveProgressNode/waveProgressFill 노드 추가 (에디터 수동)
- [ ] GameManager/ConstellationManager에서 updateWaveProgress() 호출 시점 연결 (별도 작업)
- [ ] 빌드 성공 (CocosCreator.exe 미탐지로 SKIP — 수동 빌드 필요)
