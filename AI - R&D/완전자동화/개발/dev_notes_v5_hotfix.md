# 개발 노트 v5 Hotfix (긴급 버그 수정)
**게임**: Star Sweeper
**작성일**: 2026-03-27
**담당**: AI 자동화 파이프라인 (Hotfix 세션)
**대상**: v5 구성 이후 런타임 버그 대량 수정

---

## 수정 개요

v5에서 씬 4개를 전면 재구성한 이후, Cocos Creator 에디터 및 런타임 테스트에서 다수의 버그가 발견되어 긴급 수정함.

---

## 수정 목록

### 1. Canvas 좌표계 버그 (전체 씬)
**증상**: 버튼이 시각적 위치와 다른 곳에서 반응 (TitleScene BookButton: Y=-90 클릭 시 StartButton 반응)
**원인**: Canvas 노드 `_lpos`가 `{x: 640, y: 360}` 으로 저장됨. `_alignCanvasWithScreen: true` 환경에서 히트 영역과 렌더 위치 불일치 발생
**수정**: 전체 씬 Canvas `_lpos` → `{x: 0, y: 0, z: 0}`
**대상**: TitleScene.scene, GameScene.scene, ResultScene.scene, ConstellationBookScene.scene

---

### 2. GameScene UIManager.fadeOverlay 미연결
**증상**: 씬 페이드인/아웃 없음, 게임오버 → ResultScene 전환 동작 안 함
**원인**: UIManager `@property fadeOverlay` null
**수정**: UIManager 컴포넌트 `fadeOverlay` → FadeOverlay 노드 연결

---

### 3. GameScene PausePanel 버튼 clickEvents 누락
**증상**: 일시정지 패널에서 재개/타이틀 버튼 무반응
**수정**: ResumeButton → `onResumeButtonClicked`, TitleButton → `onTitleButtonClicked`, PauseButton → `onPauseButtonClicked`
타겟: GameScriptNode, 컴포넌트: `3260fPuZHROurA57bzdi5DU`

---

### 4. GameScene PauseButton 위치 구조 오류
**증상**: 게임 중 일시정지 불가 (PauseButton이 비활성 PausePanel 내부에 있어 클릭 불가)
**수정**: PauseButton을 PausePanel 자식 → Canvas 직속 자식으로 이동, 위치 `(380, 320)` (HUD 우상단)

---

### 5. GameScene 빈 _id 필드 (53개 컴포넌트/노드)
**증상**: WaveProgressNode 배경 스프라이트 미표시, LifeIcon/Slot 노드 렌더 불안정
**원인**: 에디터 외부에서 노드/컴포넌트를 프로그래밍 방식으로 추가할 때 `_id: ""` (빈 문자열) 저장됨. CC3는 `_id`가 없는 컴포넌트 로드/렌더 불안정
**수정**: 빈 `_id` 53개 항목 전체에 23자 UUID 자동 생성 후 할당
**영향**: WaveProgressNode bg 스프라이트, LifeIcon 1~3, ConstellationUIRoot + Slot 1~10, ComboPopupNode, WaveAnnouncementLabel 등

---

## 유저가 에디터에서 추가한 항목 (이번 세션 이전)

에디터에서 직접 추가되어 있었으며, _id 수정으로 정상 작동 예상:

| 항목 | 연결 상태 |
|---|---|
| LifeIcon1/2/3 (HUDNode 자식) | HUDController.lifeIcons ✅ |
| ConstellationUIRoot + Slot 1~10 | ConstellationManager.slotNodes ✅ |
| ComboPopupNode | HUDController.comboPopupNode ✅ |
| WaveAnnouncementLabel | WaveManager.waveAnnouncementLabel ✅ |
| ConstellationManager.hudController | HUDController 컴포넌트 ✅ |

---

## 현재 씬별 상태 (Hotfix 이후)

| 씬 | 구성 | 버튼 이벤트 | @property | 비고 |
|---|---|---|---|---|
| TitleScene | ✅ | ✅ (BookButton, StartButton) | ✅ | Canvas 좌표 수정 완료 |
| GameScene | ✅ | ✅ (Pause/Resume/Title) | ✅ (전체) | _id 53개 수정, PauseButton HUD 이동 |
| ResultScene | ✅ | ✅ (Restart, Title) | ✅ | Canvas 좌표 수정 완료 |
| ConstellationBookScene | ✅ | ✅ (Back) | ✅ | Canvas 좌표 수정 완료 |

---

## 남은 이슈 (다음 iteration으로 이관)

| ID | 내용 | 우선순위 |
|---|---|---|
| M-WP-01 | `HUDController.updateWaveProgress()` 호출 시점 미연결 (ConstellationManager에서 호출 필요) | Major |
| n-05 | ConstellationManager Wave>=7 완성 시 isUnlocked 체크 미구현 | Minor |
| WaveManager.bossWarningPanel/Label | null 상태 (보스 웨이브 UI 미표시) | Minor |

---

## 핵심 구조 레퍼런스 (다음 봇 참조용)

### 스크립트 Short UUID
```
GameScene: 3260fPuZHROurA57bzdi5DU  (GameScriptNode: 49)
HUDController: 50d09x2vCVDDoARlHE3E7SW  (HUDNode: 24, comp: 37)
UIManager: 6726bcbbmVLUYSjX7bn69te  (UIManagerNode: 38, comp: 43)
ConstellationManager: 9bff7hpZhpJXKksYK2V3NKw  (ConstellationManagerNode: 18, comp: 20)
WaveManager: 072d8IxbfBICIwnpcX+kTQf  (WaveManagerNode: 21, comp: 23)
TitleScene: 318a6QGl8VI8JwZK0z0uBFc
ResultScene: b590cEuclFJmKYESdloOBZf
```

### GameScene 주요 노드 인덱스 (현재 기준)
```
Canvas: 7          FadeOverlay: 44      GameScriptNode: 49
HUDNode: 24        PausePanel: 52       PauseButton: 70
ConstellationManagerNode: 18            HUDController comp: 37
```

### ClickEvent 주의사항
- `component` AND `_componentId` 양쪽 모두 Short UUID 설정 필수
- EventHandler 배열 타입: MCP `component_set_component_property` 미지원 → JSON 직접 편집 필요
