# 디자인 노트 v4
**게임**: Star Sweeper
**Iteration**: 3
**작성일**: 2026-03-26
**디자이너**: 디자인봇 (AI 자동화 파이프라인)

---

## 1. iteration 3 작업 개요

v1~v3 기존 리소스(32종)는 그대로 유지하고, spec_v4.md "디자인 요청사항" 신규 항목 및 개선 요청 항목만 작업했습니다.

**작업 내역**:
- spec_v4.md [신규 기능] Wave 진행도 바 시스템 → `ui_progress_bg.svg` / `ui_progress_fill.svg` 신규 제작
- spec_v4.md 개선 요청: `card_constellation.svg` — nameLabel/waveLabel/dateLabel 레이아웃 명시 업데이트
- spec_v4.md 개선 요청: `card_locked.svg` — nameLabel 위치 "???" 표시 명시, dateLabel/waveLabel placeholder 추가

---

## 2. 신규 제작 / 업데이트 리소스 목록

| 파일명 | 크기 | 작업 구분 | 경로 |
|---|---|---|---|
| `ui_progress_bg.svg` | 120×12 | **신규** | `assets/resources/ui_progress_bg.svg` |
| `ui_progress_fill.svg` | 120×12 | **신규** | `assets/resources/ui_progress_fill.svg` |
| `card_constellation.svg` | 280×160 | **업데이트** (nameLabel/waveLabel/dateLabel 레이아웃 추가) | `assets/resources/card_constellation.svg` |
| `card_locked.svg` | 280×160 | **업데이트** ("???" 표시 + waveLabel/dateLabel placeholder 추가) | `assets/resources/card_locked.svg` |

**신규 2종 + 업데이트 2종** (기존 32종 유지)

---

## 3. 리소스별 디자인 상세

### ui_progress_bg.svg (120×12) — [신규]

Wave 진행도 바의 배경 트랙입니다.

- 배경: `#0a0a22` 어두운 외곽 + `background.space_surface(#2a2a55)` ~ `#111133` linearGradient 트랙
- 테두리: `ui.slot_dot(#3a3a66)` 0.8px 라인, opacity 0.8
- 상단 내부 하이라이트: `ui.slot_dot(#4466aa)` opacity 0.12 (미세 광택)
- 전체 opacity: 반투명 처리로 HUD 위에 자연스럽게 올라감
- rx/ry = 6 (완전 둥근 캡슐 형태)

### ui_progress_fill.svg (120×12) — [신규]

Wave 진행도 바의 채움 그래픽입니다.

- 메인 그라디언트 (좌→우): `text.title_gold_top(#ffe866)` → `bucket.rim_base(#ffd700)` → `#ffb800` → `text.title_gold_bot(#ff9900)`
- 상단 광택: `#ffffff` opacity 0.35 → 0 (하이라이트 shine 효과)
- 하단 그림자: `#000000` opacity 0.15 (볼륨감)
- fillGlow 필터: feGaussianBlur 1px로 미세 발광
- 테두리: `#ffc800` 0.6px, opacity 0.7
- 끝단 스파클: 우측 끝에 `#ffe866` 소형 원 + 링 (진행 포인트 강조)
- rx/ry = 6 (캡슐 형태, 배경 바와 동일)

> **개발봇 사용 가이드**: `waveProgressFill` 노드의 UITransform.width를 `(current/total) * 120` 으로 동적 변경. anchorPoint를 (0, 0.5)로 설정하여 좌측 기준 확장되도록 설정 권장.

### card_constellation.svg (280×160) — [업데이트]

spec_v4 요청: nameLabel/waveLabel/dateLabel 레이아웃 명시.

- **변경사항**: 우측 텍스트 영역을 3개 라벨 영역으로 명확히 구분
  - nameLabel (y=18~32): 별자리 이름 표시 영역 — 황금색 계열 placeholder
  - waveLabel (y=40~51): Wave 클리어 정보 — 연보라 계열 placeholder
  - dateLabel (y=58~69): 완성 날짜 정보 — 어두운 보라 계열 placeholder
- 좌측 별 아이콘 원 크기 조정 (r=28, 이전 r=20) — 더 넉넉한 영역 확보
- 하단 체크 마크 위치: 중앙 정렬 (x=172) → 텍스트 영역 중심에 맞춤
- 나머지 디자인 요소(황금 테두리, 코너 장식, glow 필터)는 유지

### card_locked.svg (280×160) — [업데이트]

spec_v4 요청: nameLabel 위치 "???" 표시, waveLabel/dateLabel placeholder 추가.

- **변경사항**:
  - nameLabel 영역에 `"???"` 텍스트를 회색 계열로 명시 표시 (`#3a3a66` opacity 0.55)
  - waveLabel placeholder 텍스트 표시 (`#2a2a55` opacity 0.35)
  - dateLabel placeholder 텍스트 표시 (`#2a2a55` opacity 0.30)
  - 코너 L자 장식을 더 명확하게 (width 10px → 기존 유지, 명도 소폭 조정)
  - 자물쇠 아이콘 크기 미세 조정 (width 20px, 이전 18px)
  - card_constellation과 레이아웃 구조를 동일하게 맞춰 개발봇 Label 연결 일관성 확보
- 색상 체계(어두운 회색 계열)는 완전 유지

---

## 4. 컬러 팔레트 (color_palette_v1.json 준용)

### ui_progress_bg
| 역할 | 컬러 | 참조 |
|---|---|---|
| 외곽 배경 | `#0a0a22` | background.space_mid 계열 |
| 트랙 그라디언트 상단 | `#2a2a55` | ui.slot_bg 파생 |
| 트랙 그라디언트 하단 | `#111133` | background.space_mid 파생 |
| 테두리 | `#3a3a66` | ui.slot_dot 파생 |
| 하이라이트 | `#4466aa` | ui.slot_dot |

### ui_progress_fill
| 역할 | 컬러 | 참조 |
|---|---|---|
| 그라디언트 시작 | `#ffe866` | text.title_gold_top |
| 그라디언트 중간 | `#ffd700` | bucket.rim_base |
| 그라디언트 중간2 | `#ffb800` | bucket.body_mid 파생 |
| 그라디언트 끝 | `#ff9900` | text.title_gold_bot |
| 테두리 | `#ffc800` | text.title_gold_mid |
| 스파클 | `#ffe866` | text.title_gold_top |

---

## 5. 개발봇에게 전달할 에셋 경로 정보

**리소스 루트**: `assets/resources/`

### Wave 진행도 바 (HUDController 신규 기능)
```typescript
// ui_progress_bg.svg — 진행도 바 배경 트랙
const progressBgPath = "ui_progress_bg";  // SpriteFrame, 120×12

// ui_progress_fill.svg — 진행도 바 채움 그래픽
const progressFillPath = "ui_progress_fill";  // SpriteFrame, 120×12
```

**HUDController 설정 가이드**:
```typescript
// waveProgressFill 노드 설정 권장
// anchorPoint: (0, 0.5) — 좌측 기준 확장
// 초기 width: 0
// updateWaveProgress(current, total) {
//   const ratio = current / total;
//   const fillNode = this.waveProgressFill;
//   fillNode.getComponent(UITransform).width = Math.round(ratio * 120);
// }
```

### 도감 카드 (ConstellationBookScene) — 레이아웃 업데이트
```typescript
// card_constellation.svg — 완성 카드 (nameLabel/waveLabel/dateLabel 영역 명시)
// nameLabel:  x=88, y=18, width=168, height=14  (런타임 Label 덮어쓰기)
// waveLabel:  x=88, y=40, width=110, height=11
// dateLabel:  x=88, y=58, width=90,  height=11
const cardUnlockedPath = "card_constellation";  // SpriteFrame, 280×160

// card_locked.svg — 미완성 카드 ("???" 표시, 동일 레이아웃 구조)
// nameLabel 위치: x=88, y=18 → "???" 표시용
// waveLabel/dateLabel: placeholder rect 포함 (비공개 암시)
const cardLockedPath = "card_locked";  // SpriteFrame, 280×160
```

### 에셋 크기 기준 (Cocos Content Size 설정 권장)

| 에셋 | contentSize 권장 | 비고 |
|---|---|---|
| `ui_progress_bg` | 120 × 12 | HUDController.waveProgressBg |
| `ui_progress_fill` | 120 × 12 | HUDController.waveProgressFill, anchorPoint (0, 0.5) |
| `card_constellation` | 280 × 160 | 3개 Label 영역 포함 |
| `card_locked` | 280 × 160 | nameLabel "???" 위치 포함 |

---

## 6. 전체 리소스 현황 (iteration 3 기준)

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
| `ui_constellation_slot.svg` | 0 | 유지 (레거시) |
| `ui_button.svg` | 0 | 유지 |
| `logo_title.svg` | 0 | 유지 |
| `ui_combo_popup.svg` | 1 | 유지 |
| `icon_pause.svg` | 1 | 유지 |
| `slot_red.svg` | 1 | 유지 |
| `slot_blue.svg` | 1 | 유지 |
| `slot_yellow.svg` | 1 | 유지 |
| `slot_green.svg` | 1 | 유지 |
| `slot_purple.svg` | 1 | 유지 |
| `ui_constellation_slot_red.svg` | 1 | 유지 (레거시) |
| `ui_constellation_slot_blue.svg` | 1 | 유지 (레거시) |
| `ui_constellation_slot_yellow.svg` | 1 | 유지 (레거시) |
| `ui_constellation_slot_green.svg` | 1 | 유지 (레거시) |
| `ui_constellation_slot_purple.svg` | 1 | 유지 (레거시) |
| `slot_empty.svg` | 2 | 유지 |
| `icon_book.svg` | 2 | 유지 |
| `bg_book.svg` | 2 | 유지 |
| `card_constellation.svg` | **3** | **업데이트** (nameLabel/waveLabel/dateLabel 레이아웃 추가) |
| `card_locked.svg` | **3** | **업데이트** ("???" 표시 + 레이아웃 통일) |
| `book_cover.svg` | 2 | 유지 |
| `book_entry_locked.svg` | 2 | 유지 |
| `book_entry_unlocked.svg` | 2 | 유지 |
| `ui_progress_bg.svg` | **3** | **신규** |
| `ui_progress_fill.svg` | **3** | **신규** |

**총계**: 기존 32종 유지 + 신규 2종 + 업데이트 2종 = **총 34종**

---

## 7. 다음 Iteration (v5) 개선 제안

1. **별 반짝임 애니메이션 스프라이트시트**: `star_twinkle_sheet.png` — SVG animate 대신 Cocos SpriteAnimation 대응
2. **별자리 완성 이펙트**: `effect_star_burst.svg` — 파티클 폭발용 방사형 스파크 (Wave 완료 연출)
3. **보스 웨이브 경고**: `icon_warning.svg` — 붉은 삼각형 경고 아이콘 (Wave 7 "은하의 심연" 돌입 전 경고)
4. **Wave 진행도 바 마일스톤 마커**: `ui_progress_tick.svg` — 진행도 바 위에 올릴 소형 틱 마커 (수집 목표 위치 표시)
5. **별자리 도트 패턴**: `constellation_orion.svg` 등 — 각 별자리 실제 점선 패턴 SVG (도감 카드 배경 삽입용)
6. **배경 레이어 분리**: `bg_stars_near.svg` / `bg_stars_far.svg` — 패럴랙스 스크롤 구현 대응
