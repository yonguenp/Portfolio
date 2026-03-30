# 게임 기획서 v6

**기준 버전**: v5 (iteration 4 + hotfix)
**작성일**: 2026-03-27
**변경 트리거**: QA 리포트 v4 잔여 이슈(M-WP-01, n-05) 해소 + 신규 기능 2건 (보스 웨이브 경고 UI, 점수 팝업)

---

## 게임 개요 (변경 없음)

- 제목: **Star Sweeper** (별빛 청소부)
- 장르: 캐주얼 퍼즐 아케이드
- 타겟 플랫폼: 모바일 (Android/iOS)
- 해상도: 1280x720
- 핵심 컨셉: 떨어지는 별 조각을 받아 별자리를 완성하며 은하를 구하는 원터치 캐주얼 게임

---

## v6 변경/추가 기능 목록

| ID | 분류 | 내용 | 우선순위 |
|---|---|---|---|
| V6-01 | 기술 부채 | M-WP-01 완전 해소: ConstellationManager에서 updateWaveProgress() 호출 연결 | Critical |
| V6-02 | 기술 부채 | n-05 해소: Wave>=7 isUnlocked 체크 로직 추가 | Minor |
| V6-03 | 신규 기능 | 보스 웨이브 경고 패널 (BossWarningPanel) 씬 연결 및 기능 활성화 | Major |
| V6-04 | 신규 기능 | 별 수집 시 점수 팝업 텍스트 (+score floater) | Major |
| V6-05 | 신규 기능 | ConstellationBookScene 별자리 카드 표시 개선 | Major |

---

## V6-01: Wave 진행도 바 호출 연결 (M-WP-01 완전 해소)

v5 spec에 호출 시점이 이미 명문화되었으나 구현이 누락된 상태. v6에서 반드시 구현.

### ConstellationManager → HUDController 연결

`ConstellationManager.ts`에 이미 `@property({ type: HUDController }) hudController` 연결이 씬 파일에 되어 있음.

### 호출 시점 (v5 spec 동일)

**Wave 1~6:**
```typescript
// _checkCompletion() 내 패턴 완성 확정 직후
if (this.hudController) this.hudController.updateWaveProgress(1, 1);
```

**Wave 7+ (addStar 내):**
```typescript
// 유효 별 수집 후 현재 진행도 실시간 갱신
const current = this._getCurrentCollectedCount();
const total = this._getCurrentTotalRequired();
if (this.hudController) this.hudController.updateWaveProgress(current, total);
```

**Wave 7+ 패턴 완성 시:**
```typescript
// _checkCompletion() 내 100% 트리거
if (this.hudController) this.hudController.updateWaveProgress(total, total);
```

---

## V6-02: Wave>=7 isUnlocked 체크 (n-05 해소)

`ConstellationManager._checkCompletion()` 내에서 "은하의 심연" 완성 시:
- `ConstellationBookManager.isUnlocked("은하의 심연") === true` 이면 → `recordCompletion()` 호출 **스킵**
- false 이면 → 기존 로직대로 `recordCompletion()` 호출 후 은하의 심연 클리어 연출

---

## V6-03: 보스 웨이브 경고 패널 기능 활성화

### 현재 상태
- `WaveManager.ts` 구현 완료 (`_showBossWarning()`)
- `WaveManager.bossWarningPanel`, `WaveManager.bossWarningLabel` null → 연결 필요

### 씬 작업 (개발봇)
GameScene.scene에 BossWarningPanel 노드 추가:
```
Canvas
  └── BossWarningPanel (active:false, 중앙, 480x120)
        ├── Sprite: ui_boss_warning.png (배경)
        └── BossWarningLabel (Label, "⚠ BOSS WAVE ⚠", fontSize 36, 빨간색)
```

- WaveManager 컴포넌트 `bossWarningPanel` → BossWarningPanel 노드 연결
- WaveManager 컴포넌트 `bossWarningLabel` → BossWarningLabel Label 컴포넌트 연결

---

## V6-04: 별 수집 점수 팝업 (Score Floater)

### 기능 설명
별을 버킷으로 수집하는 순간, 수집 위치 근처에 "+score" 텍스트가 잠깐 떠오르며 사라지는 연출.

### 구현 방식
`StarSpawner.ts` 또는 `BucketController.ts`에서 수집 이벤트 시 호출:
```typescript
ScoreFloater.show(catchPosition, scoreValue);
```

### ScoreFloater (신규 컴포넌트)
`assets/scripts/ScoreFloater.ts` 신규 작성:
- `static show(worldPos: Vec3, score: number)` — 팝업 생성
- Label(`+{score}`) 노드 생성 → 1초간 Y방향으로 60px 부드럽게 이동 → 페이드아웃 → 삭제
- 색상: 일반 별 = 흰색, 콤보 시 = 황금색 (`GameManager.isComboActive` 체크)
- 최대 동시 표시: 5개 (초과 시 가장 오래된 것 삭제)

### 씬 작업 없음
런타임 동적 생성 방식. 별도 노드 연결 불필요.

---

## V6-05: ConstellationBookScene 카드 표시 개선

### 현재 상태
`ConstellationBookScene.ts`에 `_createFallbackCard()` (텍스트만 표시) 구현됨.
실제 카드 Prefab(`cardUnlockedPrefab`, `cardLockedPrefab`) 미연결 → fallback 텍스트만 표시.

### v6 방향: 프리팹 없이 런타임 카드 생성
Prefab 연결 없이 `_createFallbackCard()`를 개선하여 시각적으로 완성도 높은 카드를 동적 생성:
- 카드 배경: `card_constellation.png` 또는 `card_locked.png` (Sprite)
- 별자리 이름 Label
- 완성 날짜 또는 "잠금" 텍스트 Label
- `UITransform` 200x120

### 카드 배치 개선
CardContainer 내 `GridLayout` 방식 적용 (현재 단순 위치 배치 → 4열 그리드).

---

## 씬 변경 없는 기능 목록 (코드만 수정)

| 기능 | 파일 | 작업 |
|---|---|---|
| V6-01 updateWaveProgress 호출 | ConstellationManager.ts | 수정 |
| V6-02 isUnlocked 체크 | ConstellationManager.ts | 수정 |
| V6-04 ScoreFloater | ScoreFloater.ts (신규) | 신규 |
| V6-04 BucketController 호출 | BucketController.ts | 수정 |
| V6-05 카드 개선 | ConstellationBookScene.ts | 수정 |

## 씬 변경 필요 목록

| 씬 | 작업 |
|---|---|
| GameScene.scene | BossWarningPanel 노드 추가 + WaveManager 연결 |

---

## 디자인봇에게

### 필요 신규 에셋

| 에셋명 | 용도 | 크기 권장 | 스타일 |
|---|---|---|---|
| `ui_boss_warning.png` | 보스 웨이브 경고 패널 배경 | 480x120 | 붉은 계열 반투명, 경고 느낌 |

### 기존 에셋 재확인
- `card_constellation.png` — ConstellationBookScene 카드 배경으로 사용 (OK)
- `card_locked.png` — 잠금 카드 배경 (OK)
- `ui_combo_popup.png` — ComboPopupNode에서 사용 (OK)
- `icon_life.png` — LifeIcon Sprite (동적 로드, OK)

---

## 기획 충족률 목표

| 항목 | v5 | v6 목표 |
|---|---|---|
| 종합 완성도 | 9.0/10 | 9.5/10 |
| M-WP-01 해소 | ❌ | ✅ |
| n-05 해소 | ❌ | ✅ |
| 보스 웨이브 UI | ❌ | ✅ |
| Score Floater | ❌ (미기획) | ✅ |
