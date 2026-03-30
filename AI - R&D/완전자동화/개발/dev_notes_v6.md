# 개발 노트 v6
**게임**: Star Sweeper
**작성일**: 2026-03-27
**담당**: AI 자동화 파이프라인 (v6 개발 세션)
**기준**: spec_v6.md / design_notes_v6.md / dev_notes_v5_hotfix.md

---

## 현재 프로젝트 상태 (v6 개발 시작 전)

### 씬별 상태 (Hotfix 이후 기준)

| 씬 | 구성 | @property | 버튼 이벤트 | 비고 |
|---|---|---|---|---|
| TitleScene | ✅ | ✅ | ✅ (Start, Book) | Canvas _lpos (0,0,0) 수정 완료 |
| GameScene | ✅ | ✅ (전체) | ✅ (Pause/Resume/Title) | _id 53개 수정, PauseButton HUD 이동 |
| ResultScene | ✅ | ✅ | ✅ (Restart, Title) | Canvas _lpos 수정 완료 |
| ConstellationBookScene | ✅ | ✅ | ✅ (Back) | Canvas _lpos 수정 완료 |

### 스크립트 Short UUID (핵심 참조)

```
GameScene:             3260fPuZHROurA57bzdi5DU  (GameScriptNode: 49)
HUDController:         50d09x2vCVDDoARlHE3E7SW  (HUDNode: 24, comp: 37)
UIManager:             6726bcbbmVLUYSjX7bn69te  (UIManagerNode: 38, comp: 43)
ConstellationManager:  9bff7hpZhpJXKksYK2V3NKw  (ConstellationManagerNode: 18, comp: 20)
WaveManager:           072d8IxbfBICIwnpcX+kTQf  (WaveManagerNode: 21, comp: 23)
```

### GameScene 주요 노드 인덱스 (Hotfix 이후 기준)

```
Canvas: 7          FadeOverlay: 44      GameScriptNode: 49
HUDNode: 24        PausePanel: 52       PauseButton: 70
ConstellationManagerNode: 18
```

---

## v6 구현 작업 내역

### V6-01: updateWaveProgress 호출 (M-WP-01)

**상태**: ✅ 이미 구현됨 (ConstellationManager.ts 확인 결과)

spec_v5 및 v6에 명문화된 호출 시점이 ConstellationManager.ts에 이미 구현되어 있었음:
- `addStar()` Wave 7+ 분기: lines 167-171 — 실시간 진행도 갱신
- `_checkCompletion()` Wave 7+ 완성: line 191 — 100% 표시
- `_checkCompletion()` Wave 1-6 완성: line 210 — 100% 표시

추가 코드 수정 없음.

---

### V6-02: Wave>=7 isUnlocked 체크 (n-05)

**상태**: ✅ 이미 구현됨 (ConstellationManager.ts 확인 결과)

`_checkCompletion()` Wave 7+ 분기에서 `ConstellationBookManager.isUnlocked('은하의 심연')` 체크 후 이미 해금된 경우 `recordCompletion()` 스킵 로직이 lines 194-207에 구현되어 있었음.

추가 코드 수정 없음.

---

### V6-03: BossWarningPanel 씬 연결

**상태**: ⚠️ 씬 파일 수동 작업 필요 (개발봇 코드 외 작업)

WaveManager.ts의 `_showBossWarning()` 로직은 완성된 상태. 씬 파일에서 아래 작업 필요:

**GameScene.scene에 추가할 노드 구조**:
```
Canvas
  └── BossWarningPanel (active:false, position:(0,0), UITransform:480×120)
        ├── Sprite: ui_boss_warning.png (배경)
        └── BossWarningLabel (Label, fontSize:36, color:#ff4444)
```

**WaveManager 컴포넌트 연결**:
- `bossWarningPanel` → BossWarningPanel 노드
- `bossWarningLabel` → BossWarningLabel Label 컴포넌트

> 씬 편집기에서 직접 수행하거나 JSON 직접 편집으로 수행 필요.

---

### V6-04: Score Floater (ScoreFloater.ts 신규)

**상태**: ✅ 완료

**신규 파일**: `assets/scripts/ScoreFloater.ts`

- `static show(worldPos: Vec3, score: number)` — 팝업 생성
- Canvas 루트(`find('Canvas')`)에 동적 노드 생성
- Label("+N") + UIOpacity 컴포넌트
- 콤보 활성(`comboCount >= 3`) → 황금색(#ffd700), 일반 → 흰색
- 1초간 Y+60px 부상 + 0.3초 딜레이 후 0.7초 페이드아웃 → 자동 삭제
- 최대 동시 표시 5개 (초과 시 oldest 제거)

**수정 파일**: `assets/scripts/StarSpawner.ts`

`_onStarCaught()` 내 일반 별 수집 시 `ScoreFloater.show()` 호출 추가:
```typescript
ScoreFloater.show(sf.node.getWorldPosition(), scoreVal);
```

---

### V6-05: ConstellationBookScene 카드 표시 개선

**상태**: ✅ 완료

**수정 파일**: `assets/scripts/ConstellationBookScene.ts`

1. **`_applyGridLayout()` 신규 메서드 추가**
   - `start()`에서 `_buildCardGrid()` 전에 호출
   - Layout 컴포넌트를 cardContainer에 동적 추가
   - Layout.Type.GRID, 4열 고정, 셀 210×130, 간격 10px

2. **`_createFallbackCard()` 전면 개선**
   - 배경 Sprite: `card_constellation.png` / `card_locked.png` 런타임 로드
   - nameLabel (fontSize:13, 황금/회색 분기)
   - waveLabel (fontSize:11)
   - dateLabel (fontSize:10, 해금 카드만 표시)
   - UITransform 200×120 유지

---

## 남은 작업 (씬 파일 — 개발봇 외 작업)

| ID | 내용 | 우선순위 | 작업자 |
|---|---|---|---|
| V6-03 | GameScene에 BossWarningPanel 노드 추가 + WaveManager 연결 | Major | 씬 편집 필요 |

---

## 파일별 변경 요약

| 파일 | 변경 종류 | 내용 |
|---|---|---|
| `ScoreFloater.ts` | 신규 | V6-04 Score Floater 컴포넌트 |
| `StarSpawner.ts` | 수정 | V6-04 ScoreFloater.show() 호출 추가 |
| `ConstellationBookScene.ts` | 수정 | V6-05 GridLayout 적용 + fallback 카드 Sprite 개선 |
| `ui_boss_warning.png` | 신규 (디자인봇) | V6-03 보스 경고 패널 배경 (480×120) |

---

## 기획 충족률 달성 현황

| 항목 | 달성 여부 | 비고 |
|---|---|---|
| V6-01 M-WP-01 해소 | ✅ | 이미 구현되어 있었음 |
| V6-02 n-05 해소 | ✅ | 이미 구현되어 있었음 |
| V6-03 BossWarningPanel | ⚠️ 부분 | 코드 완성, 씬 연결만 남음 |
| V6-04 ScoreFloater | ✅ | ScoreFloater.ts 신규, StarSpawner 수정 |
| V6-05 카드 표시 개선 | ✅ | GridLayout + Sprite fallback 카드 구현 |
