# 디자인 노트 v3
**게임**: Star Sweeper
**Iteration**: 2
**작성일**: 2026-03-26
**디자이너**: 디자인봇 (AI 자동화 파이프라인)

---

## 1. iteration 2 작업 개요

v1~v2 기존 리소스(24종)는 그대로 유지하고, spec_v3.md "디자인 요청사항" 신규 항목만 추가 제작했습니다.

**추가 배경**:
- spec_v3.md [신규 기능] 별자리 도감(ConstellationBookScene) 추가
- TitleScene에 [별자리 도감] 버튼 신설 → `icon_book.svg` 필요
- ConstellationBookScene 전용 배경 필요 → `bg_book.svg`
- 도감 카드 UI 2종 필요 → `card_constellation.svg` (완성), `card_locked.svg` (미완성)
- GameScene HUD 슬롯 미수집 상태 표현 강화 → `slot_empty.svg` 신규 제작
- 태스크 요청 추가 리소스 → `book_cover.svg`, `book_entry_locked.svg`, `book_entry_unlocked.svg`

---

## 2. 신규 제작 리소스 목록

| 파일명 | 크기 | 용도 | 경로 |
|---|---|---|---|
| `slot_empty.svg` | 40×40 | 빈 별자리 슬롯 (미수집 상태, 점선 원형) | `assets/resources/slot_empty.svg` |
| `icon_book.svg` | 60×60 | 별자리 도감 진입 버튼 아이콘 | `assets/resources/icon_book.svg` |
| `bg_book.svg` | 960×640 | 별자리 도감 씬 배경 | `assets/resources/bg_book.svg` |
| `card_constellation.svg` | 280×160 | 도감 카드 - 완성 상태 (황금 테두리) | `assets/resources/card_constellation.svg` |
| `card_locked.svg` | 280×160 | 도감 카드 - 미완성 상태 (어두운 회색) | `assets/resources/card_locked.svg` |
| `book_cover.svg` | 200×280 | 별자리 도감 커버 (우주 테마 책 표지) | `assets/resources/book_cover.svg` |
| `book_entry_locked.svg` | 120×120 | 미해금 도감 항목 (물음표 + 어두운 배경) | `assets/resources/book_entry_locked.svg` |
| `book_entry_unlocked.svg` | 120×120 | 해금된 도감 항목 배경 (빛나는 황금 프레임) | `assets/resources/book_entry_unlocked.svg` |

**신규 추가 총 8종** (기존 24종 유지)

---

## 3. 컬러 일관성 유지 내역

모든 신규 리소스는 `color_palette_v1.json` 기준 컬러를 준용했습니다.

### slot_empty.svg
- 배경 원: `background.space_deep(#000008)` ~ `background.space_surface(#0a0a2e)` radialGradient
- 점선 테두리: `ui.slot_dot(#4466aa)`, opacity 0.75
- 내부 보조 점선: `ui.slot_bg(#1a1050)` 계열 `#2a3a66`
- 중앙 점: `ui.slot_dot(#4466aa)` opacity 0.4
- 기존 `ui_constellation_slot.svg`의 빈 슬롯 역할을 계승하되, spec_v3 파일명 체계 `slot_empty.svg`로 통일

### icon_book.svg
- 원형 버튼 배경: `ui.button_top(#6644ff)` ~ `ui.button_bottom(#2200aa)` (icon_pause.svg와 색조 동일)
- 테두리: `ui.button_border_top(#9977ff)`
- 책 표지: `ui.button_mid(#4422dd)` ~ `background.nebula_purple(#1a0a3a)`
- 책 등: `background.nebula_purple(#1a0a3a)`
- 황금 별 장식: `text.title_gold_top(#ffe866)` ~ `text.title_gold_mid(#ffc800)`
- 텍스트 라인 장식: `text.subtitle_top(#ccbbff)`

### bg_book.svg
- 배경: `background.nebula_blue(#0a1a3a)` ~ `background.space_mid(#050518)` (bg_space.svg보다 청색 계열 비중 높여 차분한 분위기)
- 성운 광채: `background.nebula_purple(#1a0a3a)` / `background.nebula_blue(#0a1a3a)` 반투명 ellipse
- 별들: `stars_bg.bright(#ffffff)` / `stars_bg.mid(#b8ccff)` — bg_space.svg 대비 밀도를 약 60% 수준으로 낮춰 안정감 부여
- 은하수 밴드: `stars_bg.mid(#b8ccff)` 대각선 은은한 선형 광채 (opacity 2.5%)
- 구분 장식선: `bucket.rim_base(#ffd700)` opacity 12% (상단/하단 테두리 느낌)

### card_constellation.svg (완성 카드)
- 배경: `ui.slot_bg(#1a1050)` ~ `background.space_deep(#000008)` (어두운 우주 톤)
- 황금 테두리: `text.title_gold_top(#ffe866)` → `text.title_gold_mid(#ffc800)` → `text.title_gold_bot(#ff9900)` 그라디언트 + glow 필터
- 이중 테두리: 내부 innerGold (절반 투명도)
- 코너 별 장식: `text.title_gold_*` 계열 4방향 마름모 폴리곤
- 중앙 별 아이콘: 8각 별, `text.title_gold_*` 그라디언트
- 하단 체크 원: `bucket.rim_base(#ffd700)` 원형 테두리 + 체크 마크

### card_locked.svg (미완성 카드)
- 배경: 짙은 어둠 계열 `#0f0f22` ~ `#050510` (card_constellation보다 더 어둡고 채도 낮음)
- 테두리: 회색 그라디언트 `#3a3a5a` ~ `#1a1a33` (황금 없음)
- 코너 장식: 작은 `#2a2a44` L자형 꺾임 선 (금속 느낌)
- 중앙 물음표: `#3a3a66` serif 폰트, 점선 원 프레임
- 텍스트 플레이스홀더: 회색 rect 블록 (내용 비공개 암시)
- 자물쇠 아이콘: `#2a2a55` 채움 + `#3a3a66` 테두리

### book_cover.svg
- 책 몸체: `background.nebula_blue(#0a1a3a)` ~ `background.space_deep(#000008)` linearGradient
- 책 등: `background.nebula_purple(#1a0a3a)` ~ `background.nebula_blue(#0a1a3a)`
- 황금 이중 테두리: `text.title_gold_*` 계열 + goldGlow 필터 (glow 효과)
- 중앙 8각 별: `text.title_gold_*` 계열, starGlow 필터 (별 발광)
- 타이틀 "별자리 도감": `text.title_gold_*` 계열 serif bold + textGlow
- 부제목 "CONSTELLATION BOOK": `text.subtitle_top(#ccbbff)`
- 하단 3개 별: `life_icon.star_gold_top(#ffe866)` / `life_icon.star_gold_bot(#ffc000)`

### book_entry_unlocked.svg
- 배경: `ui.slot_bg(#1a1050)` ~ `background.space_deep(#050518)` radialGradient
- 황금 외곽 프레임: `text.title_gold_*` + frameGlow 필터 (강한 발광)
- 내부 이중 프레임: innerGoldFrame (절반 투명도)
- 코너 삼각형 4종: `text.title_gold_*` + sparkleGlow 필터
- 중앙 광채 방사선 8방향: `text.title_gold_top(#ffe866)` / `text.title_gold_mid(#ffc800)`
- 중앙 8각 별: `text.title_gold_*` 계열
- 하단 스파클 3개: `text.title_gold_*` 계열 마름모 폴리곤

### book_entry_locked.svg
- 배경: `background.space_mid(#0f0f28)` ~ `background.space_mid(#050518)` (어두운 배경)
- 테두리: `#2a2a55` ~ `#111133` linearGradient
- 물음표: `#4466aa` opacity 0.6 (ui.slot_dot 계열)
- 자물쇠: `#2a2a55` / `#050518` 구성
- 배경 별: `ui.slot_dot(#4466aa)` opacity 0.2~0.3

---

## 4. 리소스별 디자인 의도

### slot_empty.svg (40×40)
- 기존 `ui_constellation_slot.svg`를 대체하는 spec_v3 표준 파일명
- 점선 원(4px dash, 3px gap)으로 "채워지기를 기다리는 빈 공간" 시각화
- 내부 이중 점선 원(더 작고 희미)으로 깊이감 표현
- 중앙 작은 점으로 슬롯의 중심 위치 표시

### icon_book.svg (60×60)
- `icon_pause.svg`와 동일한 원형 버튼 형태 — UI 일관성 유지
- 책 등(spine)과 표지를 명확히 구분하여 책 실루엣 즉시 인식
- 표지 내 가로 라인 3줄로 "책 내용이 있음" 암시
- 황금 별(폴리곤 10각)이 책 위에 얹혀 "별자리 도감" 테마 연결

### bg_book.svg (960×640)
- `bg_space.svg` 대비 청색 계열(#0a1a3a) 비중을 높여 탐색/열람 분위기 조성
- 별 밀도를 bg_space 대비 약 60%로 줄여 도감 UI 요소가 배경에 묻히지 않도록 설계
- 미세한 대각선 은하수 밴드(opacity 2.5%)로 우주 공간감 유지
- 상하 황금 장식선(opacity 12%)으로 도감 씬의 격식 있는 테두리 연출

### card_constellation.svg (280×160)
- 황금 테두리 + glow 필터: 완성 달성의 성취감 강조
- 중앙 좌측 8각 별 아이콘으로 별자리 완성 상태 직관적 표현
- 우측 텍스트 플레이스홀더 rect로 개발봇이 런타임에 별자리 이름/날짜를 Label로 덮어쓸 영역 안내
- 하단 체크 원: 완성 마킹 배지 역할

### card_locked.svg (280×160)
- 회색 계열로 일관하여 card_constellation과 명확한 시각적 대조
- 물음표 + 점선 원 + 자물쇠 아이콘의 3중 잠김 표현으로 미해금 상태 명확화
- 텍스트 플레이스홀더 rect도 어두운 회색 처리하여 정보 비공개 암시
- 배경 별도 매우 희미하게 처리(opacity 0.15~0.20)하여 잠긴 상태의 무거운 느낌 강조

### book_cover.svg (200×280)
- 실제 책 표지처럼 등(spine)과 표지를 구분 — 도서 오브젝트로 즉시 인식
- 대형 8각 황금별을 중앙에 배치하여 "별자리 도감" 아이덴티티 확립
- 이중 황금 테두리 + goldGlow 필터로 고급스러운 마법서/도감 느낌 연출
- 하단 3개 별 + 구분선 + 영문 부제목 레이아웃으로 타이틀 화면 오브젝트로서 완성도 제고

### book_entry_locked.svg (120×120)
- book_entry_unlocked와 쌍을 이루는 잠긴 도감 항목 배경
- 중앙 물음표를 크게 배치 + 점선 원으로 "미지의 별자리" 표현
- 하단 소형 자물쇠로 잠김 상태 이중 전달
- 전체 어두운 배경 및 회색 계열로 통일하여 해금 항목과 명확히 대조

### book_entry_unlocked.svg (120×120)
- 황금 외곽 프레임 + 코너 삼각 장식 4종 + frameGlow 필터: "완성된 별자리" 특별함 강조
- 8방향 빛 방사선 + 8각 별: 별자리 완성의 발광 이펙트 연상
- card_constellation.svg보다 작은 크기(120×120)이지만 동일한 황금 컬러 문법 유지
- 배경 별 6개(희미)로 우주 공간감 보완

---

## 5. 개발봇에게 전달할 에셋 경로

**리소스 루트**: `assets/resources/`

### 별자리 도감 씬 (ConstellationBookScene)
```typescript
// bg_book.svg — 도감 씬 배경
const bgBookPath = "bg_book";  // SpriteFrame, 960×640

// card_constellation.svg — 완성된 별자리 카드 배경
const cardUnlockedPath = "card_constellation";  // SpriteFrame, 280×160

// card_locked.svg — 미완성 별자리 카드 배경
const cardLockedPath = "card_locked";  // SpriteFrame, 280×160

// icon_book.svg — TitleScene 도감 버튼 아이콘
const iconBookPath = "icon_book";  // SpriteFrame, 60×60
```

### 빈 슬롯 (GameScene ConstellationUI)
```typescript
// slot_empty.svg — 미수집 별자리 슬롯 (spec_v3 표준 파일명)
// 기존 코드의 "ui_constellation_slot" 경로를 "slot_empty"로 교체할 것
const slotEmptyPath = "slot_empty";  // SpriteFrame, 40×40
```

### 도감 항목 배경 (ConstellationBookScene 카드 내부)
```typescript
// book_entry_unlocked.svg — 해금된 항목 배경 프레임
const entryUnlockedPath = "book_entry_unlocked";  // SpriteFrame, 120×120

// book_entry_locked.svg — 잠긴 항목 배경 프레임
const entryLockedPath = "book_entry_locked";  // SpriteFrame, 120×120

// book_cover.svg — 도감 표지 오브젝트 (TitleScene 등 장식용)
const bookCoverPath = "book_cover";  // SpriteFrame, 200×280
```

### 에셋 크기 기준 (Cocos Content Size 설정 권장)

| 에셋 | contentSize 권장 |
|---|---|
| `bg_book` | 960 × 640 |
| `card_constellation` | 280 × 160 |
| `card_locked` | 280 × 160 |
| `icon_book` | 60 × 60 |
| `slot_empty` | 40 × 40 |
| `book_entry_unlocked` | 120 × 120 |
| `book_entry_locked` | 120 × 120 |
| `book_cover` | 200 × 280 |

> **주의**: spec_v3 변경 핵심 — `ui_constellation_slot.svg` 경로를 `slot_empty`로 교체. `ui_constellation_slot_[color].svg` 체계도 `slot_[color]`로 단축된 파일명을 이미 iteration 1에서 제작 완료.

---

## 6. 전체 리소스 현황 (iteration 2 기준)

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
| `ui_constellation_slot.svg` | 0 | 유지 (레거시, slot_empty로 대체 예정) |
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
| `slot_empty.svg` | **2** | **신규** |
| `icon_book.svg` | **2** | **신규** |
| `bg_book.svg` | **2** | **신규** |
| `card_constellation.svg` | **2** | **신규** |
| `card_locked.svg` | **2** | **신규** |
| `book_cover.svg` | **2** | **신규** |
| `book_entry_locked.svg` | **2** | **신규** |
| `book_entry_unlocked.svg` | **2** | **신규** |

**총계**: 기존 24종 유지 + 신규 8종 = **총 32종**

---

## 7. 다음 Iteration (v4) 개선 제안

1. **별 반짝임 애니메이션**: SVG `<animate>` 또는 Cocos Creator SpriteAnimation용 스프라이트시트 (`star_twinkle_sheet.png`)
2. **별자리 완성 이펙트**: `effect_star_burst.svg` — 파티클 폭발용 방사형 스파크
3. **보스 웨이브 경고**: `icon_warning.svg` — 붉은 삼각형 경고 아이콘
4. **배경 레이어 분리**: `bg_stars_near.svg` / `bg_stars_far.svg` — 패럴랙스 스크롤 대응
5. **버킷 업그레이드 스킨**: `bucket_silver.svg`, `bucket_rainbow.svg`
6. **별자리 도트 패턴**: `constellation_orion.svg` 등 각 별자리 실제 점선 패턴 SVG (도감 카드 배경 삽입용)
