# 디자인 노트 v2
**게임**: Star Sweeper
**Iteration**: 1
**작성일**: 2026-03-26
**디자이너**: 디자인봇 (AI 자동화 파이프라인)

---

## 1. iteration 1 작업 개요

v1 기존 리소스(12종)는 그대로 유지하고, v2 기획서 "디자인 요청사항" 신규 항목만 추가 제작했습니다.

**추가 배경**:
- spec_v2.md QA Major 이슈 [M-04] 대응: 일시정지 버튼 UI 아이콘 필요
- spec_v2.md [NEW-01] 콤보 HUD 표시 기능: 팝업 배경 리소스 필요
- spec_v2.md 별자리 슬롯 색상별 채워짐 상태 표현: 색상 슬롯 5종 필요
- spec_v2.md GameScene HUD: `ui_constellation_slot_[color].svg` 명시

---

## 2. 신규 제작 리소스 목록

| 파일명 | 크기 | 용도 | 경로 |
|---|---|---|---|
| `ui_combo_popup.svg` | 300×80 | 콤보 ×1.5 팝업 텍스트 배경 | `assets/resources/ui_combo_popup.svg` |
| `icon_pause.svg` | 60×60 | 일시정지 버튼 아이콘 | `assets/resources/icon_pause.svg` |
| `slot_red.svg` | 40×40 | 별자리 슬롯 채움 - 빨강 | `assets/resources/slot_red.svg` |
| `slot_blue.svg` | 40×40 | 별자리 슬롯 채움 - 파랑 | `assets/resources/slot_blue.svg` |
| `slot_yellow.svg` | 40×40 | 별자리 슬롯 채움 - 노랑 | `assets/resources/slot_yellow.svg` |
| `slot_green.svg` | 40×40 | 별자리 슬롯 채움 - 초록 | `assets/resources/slot_green.svg` |
| `slot_purple.svg` | 40×40 | 별자리 슬롯 채움 - 보라 | `assets/resources/slot_purple.svg` |
| `ui_constellation_slot_red.svg` | 40×40 | 별자리 슬롯 (HUD용) - 빨강 채움 | `assets/resources/ui_constellation_slot_red.svg` |
| `ui_constellation_slot_blue.svg` | 40×40 | 별자리 슬롯 (HUD용) - 파랑 채움 | `assets/resources/ui_constellation_slot_blue.svg` |
| `ui_constellation_slot_yellow.svg` | 40×40 | 별자리 슬롯 (HUD용) - 노랑 채움 | `assets/resources/ui_constellation_slot_yellow.svg` |
| `ui_constellation_slot_green.svg` | 40×40 | 별자리 슬롯 (HUD용) - 초록 채움 | `assets/resources/ui_constellation_slot_green.svg` |
| `ui_constellation_slot_purple.svg` | 40×40 | 별자리 슬롯 (HUD용) - 보라 채움 | `assets/resources/ui_constellation_slot_purple.svg` |

**신규 추가 총 12종** (기존 12종 유지)

---

## 3. 컬러 일관성 유지 내역

모든 신규 리소스는 `color_palette_v1.json` 기준 컬러를 그대로 준용했습니다.

### ui_combo_popup.svg
- 텍스트/배경: `text.title_gold_top(#ffe866)` / `text.title_gold_mid(#ffc800)` / `text.title_gold_bot(#ff9900)` 그라디언트
- 패널 배경: `background.space_deep(#000008)` 계열 어두운 배경
- 테두리/장식: `bucket.rim_base(#ffd700)` 황금 계열
- 서브텍스트: `#ffcc44` (텍스트 황금 중간)

### icon_pause.svg
- 원형 배경: `ui.button_top(#6644ff)` ~ `background.nebula_blue(#0a1a3a)` 반투명
- 테두리: `ui.button_border_top(#9977ff)` ~ `ui.button_bottom(#2200aa)`
- 바 색상: `text.title_white(#ffffff)` ~ `stars_bg.mid(#b8ccff)`

### slot_*/ui_constellation_slot_* (5색 공통 구조)
- 각 색상별 `highlight / base / shadow / glow / stroke` 값은 `stars.[color]` 팔레트 그대로 사용
- 배경 원: 각 색상의 `shadow` 계열 극단 어두운 배경
- 테두리: `stars.[color].stroke` 값
- 내부 별 심볼: `#ffffff` 50~55% 불투명도 (모든 배경에서 가독)
- 하이라이트: `#ffffff` 22~30% 불투명 타원 (입체감)

---

## 4. 리소스별 디자인 의도

### ui_combo_popup.svg (300×80)
- "COMBO" + "×1.5!" 두 파트로 텍스트 구성, 강약 대비
- 황금빛 글로우 필터(`textGlow`)로 발광 효과 — 모바일 화면에서 즉시 인식 가능
- 좌우 8각 별 + 소형 스파클 장식으로 축제 느낌 강조
- 배경 패널 반투명(opacity 0.97) + 둥근 모서리(rx=14)로 게임 UI와 자연스럽게 통합
- 하단 "SCORE MULTIPLIER ACTIVE" 서브텍스트로 기능 안내

### icon_pause.svg (60×60)
- 두 개의 흰색 세로 막대(rx=3 둥근 처리)로 전통적인 pause 심볼 표현
- 반투명 원형 배경(opacity ~0.73)으로 배경 우주 이미지가 비쳐보이는 느낌 유지
- 버튼 계열 파랑-보라 그라디언트로 `ui_button.svg`와 색조 통일
- 글로우 필터로 바의 발광 효과 추가

### slot_*. svg / ui_constellation_slot_*.svg (40×40)
- `ui_constellation_slot.svg`(빈 슬롯)와 쌍을 이루는 채워짐 상태 표현
- radialGradient + 중앙 발광 필터로 "수집됨"의 밝고 생동감 있는 상태 표현
- 5색 모두 동일한 레이아웃 구조 유지 (원형 배경 + 내부 채움원 + 별 심볼 + 하이라이트)
- `slot_*` 계열: ConstellationUI 외부 독립 슬롯 용도로도 재사용 가능

---

## 5. 개발봇에게 전달할 에셋 경로

**리소스 루트**: `assets/resources/`

### 콤보 HUD (HUDController.showComboEffect)
```typescript
// ui_combo_popup.svg — 콤보 팝업 배경 스프라이트
const comboPopupPath = "ui_combo_popup";  // SpriteFrame, 300×80
```

### 일시정지 버튼 (PausePanel / HUD 우측 상단)
```typescript
// icon_pause.svg — 일시정지 아이콘
const pauseIconPath = "icon_pause";       // SpriteFrame, 60×60
```

### 별자리 슬롯 — 채워진 상태 (ConstellationUI)
```typescript
// ui_constellation_slot_[color].svg — 색상별 채워진 슬롯
const constellationSlotFilled: Record<string, string> = {
  RED:    "ui_constellation_slot_red",
  BLUE:   "ui_constellation_slot_blue",
  YELLOW: "ui_constellation_slot_yellow",
  GREEN:  "ui_constellation_slot_green",
  PURPLE: "ui_constellation_slot_purple",
};

// 빈 슬롯 (기존 유지)
const constellationSlotEmpty = "ui_constellation_slot";  // 회색 점선 원
```

### slot_* 계열 (범용 색상 채움 슬롯)
```typescript
// 독립 슬롯 오브젝트 또는 인벤토리 UI용
const colorSlots: Record<string, string> = {
  RED:    "slot_red",
  BLUE:   "slot_blue",
  YELLOW: "slot_yellow",
  GREEN:  "slot_green",
  PURPLE: "slot_purple",
};
```

### 에셋 크기 기준 (Cocos Content Size 설정 권장)

| 에셋 | contentSize 권장 |
|---|---|
| `ui_combo_popup` | 300 × 80 |
| `icon_pause` | 60 × 60 |
| `slot_*` (5색) | 40 × 40 |
| `ui_constellation_slot_*` (5색) | 40 × 40 |

---

## 6. 전체 리소스 현황 (iteration 1 기준)

| 파일명 | Iteration | 상태 |
|---|---|---|
| `bg_space.svg` | 0 | 유지 |
| `bucket.svg` | 0 | 유지 |
| `star_red.svg` | 0 | 유지 |
| `star_blue.svg` | 0 | 유지 |
| `star_yellow.svg` | 0 | 유지 |
| `star_green.svg` | 0 | 유지 |
| `star_purple.svg` | 0 | 유지 |
| `star_dark.svg` | 0 | 유지 |
| `icon_life.svg` | 0 | 유지 |
| `ui_constellation_slot.svg` | 0 | 유지 |
| `ui_button.svg` | 0 | 유지 |
| `logo_title.svg` | 0 | 유지 |
| `ui_combo_popup.svg` | **1** | **신규** |
| `icon_pause.svg` | **1** | **신규** |
| `slot_red.svg` | **1** | **신규** |
| `slot_blue.svg` | **1** | **신규** |
| `slot_yellow.svg` | **1** | **신규** |
| `slot_green.svg` | **1** | **신규** |
| `slot_purple.svg` | **1** | **신규** |
| `ui_constellation_slot_red.svg` | **1** | **신규** |
| `ui_constellation_slot_blue.svg` | **1** | **신규** |
| `ui_constellation_slot_yellow.svg` | **1** | **신규** |
| `ui_constellation_slot_green.svg` | **1** | **신규** |
| `ui_constellation_slot_purple.svg` | **1** | **신규** |

**총계**: 기존 12종 유지 + 신규 12종 = **총 24종**

---

## 7. 다음 Iteration (v3) 개선 제안

1. **별 반짝임 애니메이션**: SVG `<animate>` 또는 Cocos Creator SpriteAnimation용 스프라이트시트
2. **별자리 완성 이펙트**: `effect_star_burst.svg` — 파티클 폭발용 방사형 스파크
3. **보스 웨이브 경고**: `icon_warning.svg` — 붉은 삼각형 경고 아이콘
4. **배경 레이어 분리**: `bg_stars_near.svg` / `bg_stars_far.svg` — 패럴랙스 스크롤 대응
5. **버킷 업그레이드 스킨**: `bucket_silver.svg`, `bucket_rainbow.svg`
