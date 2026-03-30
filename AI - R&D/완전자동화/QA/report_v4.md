# QA 리포트 v4 (iter3)
**게임**: Star Sweeper
**분석일**: 2026-03-26
**분석 대상**: TypeScript 스크립트 18개, 씬 파일 4개
**기준 문서**: spec_v4.md, dev_notes_v4.md
**이전 리포트**: QA/latest_report.md (iter2 — 9.4/10)

---

## 종합 평가

**점수: 9.0 / 10**

iter3에서 이전 이슈 3건(M-NEW-01, n-01, n-02) 모두 코드 레벨에서 수정 완료되었으며, spec_v4 신규 기능(Wave 진행도 바 HUDController 구현, AudioManager.playProgressComplete)도 정상 구현되었습니다. 그러나 두 가지 이슈로 점수가 9.4 → 9.0으로 소폭 하락하였습니다.

**하락 원인**:
1. **ConstellationBookScene.scene 파일 구조 불완전**: 씬 JSON 배열이 37개 항목만 존재하는 반면 Canvas가 참조하는 자식 노드 ID(40, 60, 80)와 다수 컴포넌트 ID(101~118, 200~206)가 배열 범위를 초과하여 존재하지 않습니다. 에디터에서 씬 로드 시 대다수 노드/컴포넌트가 null로 처리되어 도감 씬이 실질적으로 동작하지 않을 위험이 있습니다. (이전 iteration부터 지속된 구조적 결함)
2. **updateWaveProgress() 호출 시점 미연결**: GameManager, ConstellationManager, GameScene 어디에서도 `HUDController.updateWaveProgress()`를 호출하지 않아 Wave 진행도 바가 런타임에 실제로 갱신되지 않습니다.

---

## 이전 이슈 수정 확인

### M-NEW-01: TitleScene.scene MissingScript 해소

| 항목 | 수정 여부 | 확인 근거 |
|---|---|---|
| `cc.TitleScene` 컴포넌트 연결 (`data[5]`) | **수정 완료** | `data[5].__type__ === "cc.TitleScene"` 확인 |
| `bestScoreLabel` 연결 (`__id__:14`) | **수정 완료** | `data[14].__type__ === "cc.Label"` 확인 |
| `fadeOverlay` 연결 (`__id__:21`) | **수정 완료** | `data[21]._name === "FadeOverlay"` 확인 |
| `bookButton` 연결 (`__id__:26`) | **수정 완료** | `data[26]._name === "BookButton"` 확인 |
| BookButton 노드 신규 추가 | **수정 완료** | `data[26]` BookButton 노드 존재, UITransform/Button/Label 자식 확인 |
| BookButton clickEvents component `"TitleScene"` 명시 | **수정 완료** | `data[29]._clickEvents[0].component === "TitleScene"` 확인 |
| Canvas children에 BookButton 추가 | **수정 완료** | `data[2]._children` 배열에 `{__id__:26}` 포함 확인 |
| SceneGlobals 내부 참조 인덱스 수정 (33~39) | **수정 완료** | `data[32]` SceneGlobals ambient~lightProbeInfo → __id__ 33~39 정상 |
| 모든 __id__ 참조 배열 범위 내 정합성 | **수정 완료** | TitleScene.scene 전체 __id__ 참조 전수 검사 통과 (max valid id: 39) |

**판정: M-NEW-01 핵심 항목 전부 수정 완료. TitleScene.scene 참조 정합성 이상 없음.**

---

### n-02: ConstellationBookScene.scene BackButton clickEvents[0].component 명시

| 항목 | 수정 여부 | 확인 근거 |
|---|---|---|
| `clickEvents[0].component` = `"ConstellationBookScene"` 명시 | **수정 완료** | `data[22].clickEvents[0].component === "ConstellationBookScene"` 확인 |
| `clickEvents[0].handler` = `"onBackButtonClicked"` | **수정 완료** | handler 값 정상 |

**판정: n-02 수정 완료. 단, 씬 파일 구조 불완전(out-of-bounds 참조) 이슈(M-CBS-01)로 버튼 실제 동작 보장 불가.**

---

### n-01: ConstellationBookScene._createFallbackCard() UITransform 추가

| 항목 | 수정 여부 | 확인 근거 |
|---|---|---|
| `import`에 `UITransform` 추가 | **수정 완료** | `ConstellationBookScene.ts` L1 import 확인 |
| `_createFallbackCard()` 내 `node.addComponent(UITransform)` 추가 | **수정 완료** | L139~140 `const uiTransform = node.addComponent(UITransform); uiTransform.setContentSize(200, 120);` |
| contentSize 200×120 설정 | **수정 완료** | design_notes_v4 권장 크기 준수 |

**판정: n-01 수정 완료.**

---

## 신규 기능 검증

### HUDController.ts — Wave 진행도 바

| 검증 항목 | 결과 | 비고 |
|---|---|---|
| `waveProgressNode` `@property({ type: Node })` 추가 | PASS | L33~34 확인 |
| `waveProgressFill` `@property({ type: Sprite })` 추가 | PASS | L37~38 확인 |
| `_initWaveProgress()` 구현 — start()에서 호출 | PASS | L156~161, start() L83에서 호출 확인 |
| `updateWaveProgress(current, total)` 메서드 구현 | PASS | L172~198 구현 완성 |
| Wave 클리어 시 0.3초 tween → playProgressComplete() → 0.5초 대기 → width 초기화 순서 | PASS | tween 체인 순서 spec_v4와 일치 |
| 일반 갱신: 즉시 너비 설정 | PASS | L196 `setContentSize(targetWidth, height)` |
| ratio 계산 — 0 나누기 방어 `Math.max(total, 1)` | PASS | L179 |
| `UITransform` / `AudioManager` import 추가 | PASS | L1, L4 import 확인 |
| `updateWaveProgress()` 호출 시점 연결 (GameManager / ConstellationManager) | **FAIL** | 어떤 스크립트에서도 호출 없음 — 진행도 바 실시간 갱신 불가 (M-WP-01) |

---

### AudioManager.ts — playProgressComplete()

| 검증 항목 | 결과 | 비고 |
|---|---|---|
| `sfxProgressComplete` `@property({ type: AudioClip })` 추가 | PASS | L62~63 확인 |
| `playProgressComplete()` 메서드 추가 | PASS | L116 `{ this._playSFX(this.sfxProgressComplete); }` |
| null 체크 (`_playSFX` 내부 `if (!clip) return`) | PASS | sfxProgressComplete null 시 오류 없이 무시 |

---

### Wave 진행도 tween 애니메이션 로직

| 검증 항목 | 결과 | 비고 |
|---|---|---|
| `tween(uiTransform).to(0.3, { width: 120 } as any)` 패턴 | PASS (우회) | `as any` 캐스팅으로 TypeScript 타입 추론 우회. Cocos 3.8.8 런타임 동작 가능하나 공식 지원 미정 (n-06) |
| spec_v4 진행도 바 색상 (회색 반투명 / 황금 그라디언트) | 미구현 | 코드 레벨에서 색상 처리 없음 — 에셋(ui_progress_fill.svg) 단계에서 처리 예정 |

---

## 발견된 이슈

### Critical (즉시 수정 필요)

없음

---

### Major (다음 iteration에서 수정)

| ID | 위치 | 내용 | 영향도 |
|---|---|---|---|
| M-CBS-01 | `ConstellationBookScene.scene` | 씬 JSON 배열 37개 항목 중 Canvas._children이 참조하는 `__id__:40`, `__id__:60`, `__id__:80` 및 노드별 컴포넌트 ID(101~118, 200~206)가 모두 배열 범위 초과. BackButton, BookSceneController, FadeOverlay 등 핵심 노드 및 대다수 컴포넌트가 실질적으로 누락된 상태 | 도감 씬 에디터 로드 불완전, 런타임 null 참조 오류 위험. n-02 수정 효과도 이 이슈로 인해 런타임 반영 불확실 |
| M-WP-01 | `GameManager.ts`, `ConstellationManager.ts`, `GameScene.ts` | `HUDController.updateWaveProgress()` 호출 코드 미존재 — Wave 진행도 바가 런타임에 실제로 갱신되지 않음. dev_notes_v4에도 "별도 작업 필요"로 명시됨 | spec_v4 메카닉 4 미동작 |

---

### Minor (개선 권장)

| ID | 위치 | 내용 | 영향도 |
|---|---|---|---|
| n-03 | `ConstellationBookScene.scene` | `cardUnlockedPrefab` / `cardLockedPrefab` null — Prefab 미연결. 에디터에서 수동 연결 필요 | 도감 카드 그래픽 미표시, fallback 텍스트만 표시 |
| n-04 | `TitleScene.scene` `data[17]` | `startButton`의 `clickEvents[0].component` 빈 문자열 — BookButton은 "TitleScene" 명시되었으나 startButton은 미명시 | 런타임 핸들러 탐색 불안정 가능성 (낮음) |
| n-05 | `ConstellationManager.ts` L171~177 | spec_v4 개발 지침: Wave >= 7 완성 시 `ConstellationBookManager.isUnlocked("은하의 심연") === true`이면 `recordCompletion()` 호출 스킵 로직 미구현. 현재는 `recordCompletion()` 내부 중복 방지 로직으로 데이터 오염은 없으나 spec 명세 미준수 | 데이터 오염 없음, spec 준수 수준 이슈 |
| n-06 | `HUDController.ts` L184~185 | `tween(uiTransform).to(0.3, { width: 120 } as any)` — UITransform 개별 프로퍼티 직접 tween은 Cocos 3.8.8 공식 권장 방식이 아님. Wave 클리어 tween 연출 미동작 가능성 존재 | Wave 클리어 연출 시각적 불완전 위험 |

---

## 코드 품질 평가

### TypeScript 문법 오류

없음 — 분석 대상 18개 스크립트 전체에서 문법 오류 없음.

### Cocos Creator 3.8.8 API 사용

| 파일 | 평가 |
|---|---|
| `HUDController.ts` | tween으로 `UITransform.width`에 직접 접근 시 `as any` 캐스팅 사용 — 공식 API 외 사용 (n-06) |
| `AudioManager.ts` | `playOneShot()` — 정상 |
| `ConstellationBookScene.ts` | `new Node()` + `addComponent(UITransform)` — 정상 |
| 기타 전 파일 | `_decorator`, `tween`, `director`, `resources.load()` 등 모두 정상 |

### 널 참조 위험 지점

| 위치 | 내용 | 위험도 |
|---|---|---|
| `HUDController.ts` L184~193 | `updateWaveProgress()` Wave 클리어 tween 도중 노드 destroy 시 `uiTransform.setContentSize()` crash 가능 | 낮음 |
| `ConstellationBookScene.scene` | M-CBS-01로 인해 씬 로드 시 다수 참조가 null — `backButton`, `fadeOverlay`, `titleLabel` 등 모두 null 처리 위험 | 높음 (씬 파일 결함) |

### 메모리 누수 가능성

| 위치 | 내용 | 위험도 |
|---|---|---|
| `HUDController.ts` `updateWaveProgress()` | Wave 클리어 0.8초 tween 진행 중 재호출 시 tween 중첩 가능 — `showComboEffect()`의 m-04 중첩 방지 패턴이 미적용 | 중간 — 추가 권장 |
| `ConstellationManager.ts` `scheduleOnce()` | 노드 destroy 후 콜백 실행 시 `_loadNextPattern()` 호출 위험 | 낮음 |

---

## 기획서 충족도

**29/30 (96.7%)**

| 요구사항 | 내용 | 구현 여부 |
|---|---|---|
| R01~R27 | v3까지 구현된 기존 요구사항 | 이전 리포트 기준 유지 |
| R28 | 별자리 도감 씬 (ConstellationBookScene) | PARTIAL — 코드 완성, 씬 파일 구조 불완전 (M-CBS-01) |
| R29 | TitleScene 도감 버튼 연결 | PASS — M-NEW-01 해소, 런타임 동작 가능 |
| R30 | Wave 진행도 바 — HUDController 구현 | PARTIAL — updateWaveProgress() 구현됨, 호출 시점 미연결 (M-WP-01) |
| — | "은하의 심연" 단일 집계 | PASS — `_buildRandomPattern()` 항상 `name: "은하의 심연"` 반환 |
| — | sfxProgressComplete + playProgressComplete() | PASS |

---

## 다음 iteration 권장사항

### 기획봇에게

1. **Wave 진행도 호출 시점 설계 명확화**: `updateWaveProgress()`를 어느 시점에 누가 호출하는지 spec에 명문화. Wave 1~6은 `ConstellationManager._checkCompletion()` 직전에 `updateWaveProgress(1, 1)`로 단순 처리 가능하나, Wave 7+는 `addStar()` 매 호출마다 실시간 갱신이 필요함을 명시.
2. **ConstellationBookScene.scene 재작성 iteration 범위 지시**: M-CBS-01 해소를 위해 씬 파일 완전 재작성이 필요함. 다음 iteration 필수 범위에 포함 여부 확인.

### 디자인봇에게

1. **도감 카드 Prefab 제작 긴급 유지**: `card_constellation.svg`(완성) + `card_locked.svg`(미완성) — nameLabel/waveLabel/dateLabel 내부 구조 포함. n-03 해소를 위해 에디터 연결 필요.
2. **Wave 진행도 바 에셋 확인**: `ui_progress_bg.svg` + `ui_progress_fill.svg` 에셋 제작 상태 및 색상 사양(회색 반투명 배경 / 노란색→황금 그라디언트 채움) 적용 여부 확인.

### 개발봇에게

1. **[Major - 긴급] M-WP-01 해소**: `ConstellationManager`에서 HUDController 참조를 @property로 받아 `addStar()` 호출마다 `updateWaveProgress(현재수집수, 총요구수)` 호출. Wave 클리어 시 `updateWaveProgress(total, total)` 호출하여 100% 연출 트리거.
2. **[Major] M-CBS-01 해소**: `ConstellationBookScene.scene` 파일을 Cocos Creator 에디터에서 처음부터 재작성. 모든 노드(Canvas, Background, TitleLabel, ScrollView, CardContainer, BackButton, BookSceneController, FadeOverlay)와 컴포넌트가 JSON 배열 내에 실제 항목으로 존재하도록 보장.
3. **[Minor] n-04 해소**: `TitleScene.scene` startButton `clickEvents[0].component = "TitleScene"` 명시.
4. **[Minor] n-05 해소**: `ConstellationManager._checkCompletion()` Wave >= 7 완성 시 `isUnlocked("은하의 심연") === true`이면 `recordCompletion()` 호출 스킵 로직 추가.
5. **[Minor] n-06 개선**: `updateWaveProgress()` Wave 클리어 분기에 m-04 패턴(tween 중첩 방지) 적용 및 tween 타겟 재검토 (`as any` 제거 방향).

---

## 최종 집계

| 항목 | 수치 |
|---|---|
| **종합 점수** | **9.0 / 10** |
| 이전 이슈 수정 완료 (M-NEW-01, n-01, n-02) | **3/3건** |
| 잔여 Critical | **0건** |
| 잔여 Major | **2건** (M-CBS-01, M-WP-01) |
| 잔여 Minor | **4건** (n-03 ~ n-06) |
| 빌드 상태 | **SKIP (수동 빌드 필요)** |
| 기획 충족률 | **96.7%** (29/30) |
| TypeScript 문법 오류 | **0건** |
| Cocos 3.8.8 API 비정상 사용 | **1건** (n-06) |
