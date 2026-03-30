# 디자인 노트 v6
**게임**: Star Sweeper
**Iteration**: 5
**작성일**: 2026-03-27
**디자이너**: 디자인봇 (AI 자동화 파이프라인)

---

## 1. iteration 5 작업 개요

v1~v4 기존 리소스(34종)는 그대로 유지하고, spec_v6.md "디자인봇에게" 신규 요청 항목만 작업했습니다.

**작업 내역**:
- spec_v6.md [V6-03] 보스 웨이브 경고 패널 배경 → `ui_boss_warning.png` 신규 제작

---

## 2. 신규 제작 리소스 목록

| 파일명 | 크기 | 작업 구분 | 경로 |
|---|---|---|---|
| `ui_boss_warning.png` | 480×120 | **신규** | `assets/resources/ui_boss_warning.png` |

**신규 1종** (기존 34종 유지)

---

## 3. 리소스별 디자인 상세

### ui_boss_warning.png (480×120) — [신규]

보스 웨이브 진입 시 화면 중앙에 표시되는 경고 패널 배경입니다.

- **배경 그라디언트 (상→하)**:
  - 상단: `#c81414` (밝은 경고 빨강) — 시선 집중
  - 하단: `#640505` (어두운 심연 레드) — 중압감 연출
  - 상단 6px 하이라이트 밴드: `#e83e3e` 미세 광택

- **테두리**:
  - 외곽 1px: `#ff3c3c` (bright red) — 불꽃 경고 테두리
  - 내측 2px: `#e03838` — 중첩 프레임 느낌

- **경고 패턴**:
  - 대각선 스트라이프 (x+y mod 24, 10~12 범위): 미세 하이라이트 줄무늬
  - 압박감과 위험 신호를 시각적으로 암시

- **투명도**: alpha 220 (반투명) — 게임 화면 위에 자연스럽게 오버레이

- **색상 의도**: 기존 팔레트 `background.space_mid(#2a2a55)` 계열과 대비를 극대화하는 적색 계열로 선택. 스페이스 테마의 어두운 배경 위에서 즉각적 위험 인식 유도.

> **개발봇 사용 가이드**: WaveManager.bossWarningPanel 노드에 Sprite 컴포넌트로 연결. 노드 크기 480×120, UITransform 동일 설정. BossWarningLabel은 fontSize 36, 빨간색(`#ff4444`)으로 "⚠ BOSS WAVE ⚠" 표시.

---

## 4. 컬러 팔레트 (color_palette_v1.json 준용 + 신규 경고색)

### ui_boss_warning
| 역할 | 컬러 | 비고 |
|---|---|---|
| 그라디언트 상단 | `#c81414` | 밝은 경고 빨강 |
| 그라디언트 하단 | `#640505` | 어두운 위기 레드 |
| 하이라이트 밴드 | `#e83e3e` | 상단 6px 광택 |
| 외곽 테두리 | `#ff3c3c` | 불꽃 경고 라인 |
| 내측 테두리 | `#e03838` | 중첩 프레임 |
| 스트라이프 | `+40, +8, +8` | 대각선 패턴 오프셋 |

---

## 5. 기존 에셋 재확인 (spec_v6.md 요청)

| 에셋 | 상태 | 비고 |
|---|---|---|
| `card_constellation.png` | ✅ 존재 | ConstellationBookScene 카드 배경 (V6-05) |
| `card_locked.png` | ✅ 존재 | 잠금 카드 배경 (V6-05) |
| `ui_combo_popup.png` | ✅ 존재 | ComboPopupNode 사용 중 |
| `icon_life.png` | ✅ 존재 | LifeIcon 동적 로드 중 |

---

## 6. 개발봇에게 전달할 에셋 경로 정보

**리소스 루트**: `assets/resources/`

### 보스 웨이브 경고 패널 (WaveManager)
```typescript
// ui_boss_warning.png — 보스 웨이브 경고 패널 배경
// BossWarningPanel Sprite.spriteFrame으로 연결 (씬에서 직접 연결)
// 노드 크기: 480×120, 위치: Canvas 중앙 (0, 0)
// active 기본값: false (WaveManager._showBossWarning() 에서 활성화)
```

### ConstellationBookScene 카드 (V6-05 fallback 개선)
```typescript
// card_constellation.png — 완성 카드 배경 (200×120으로 스케일 사용 가능)
const cardUnlockedPath = "card_constellation";  // SpriteFrame

// card_locked.png — 잠금 카드 배경 (200×120으로 스케일 사용 가능)
const cardLockedPath = "card_locked";  // SpriteFrame
```

---

## 7. 전체 리소스 현황 (iteration 5 기준)

| 파일명 | Iteration | 상태 |
|---|---|---|
| `bg_space.png` | 0 | 유지 |
| `bucket.png` | 0 | 유지 |
| `star_red.png` | 0 | 유지 |
| `star_blue.png` | 0 | 유지 |
| `star_yellow.png` | 0 | 유지 |
| `star_green.png` | 0 | 유지 |
| `star_purple.png` | 0 | 유지 |
| `star_dark.png` | 0 | 유지 |
| `icon_life.png` | 0 | 유지 |
| `ui_constellation_slot.png` | 0 | 유지 (레거시) |
| `ui_button.png` | 0 | 유지 |
| `logo_title.png` | 0 | 유지 |
| `ui_combo_popup.png` | 1 | 유지 |
| `icon_pause.png` | 1 | 유지 |
| `slot_red.png` | 1 | 유지 |
| `slot_blue.png` | 1 | 유지 |
| `slot_yellow.png` | 1 | 유지 |
| `slot_green.png` | 1 | 유지 |
| `slot_purple.png` | 1 | 유지 |
| `ui_constellation_slot_red.png` | 1 | 유지 (레거시) |
| `ui_constellation_slot_blue.png` | 1 | 유지 (레거시) |
| `ui_constellation_slot_yellow.png` | 1 | 유지 (레거시) |
| `ui_constellation_slot_green.png` | 1 | 유지 (레거시) |
| `ui_constellation_slot_yellow.png` | 1 | 유지 (레거시) |
| `slot_empty.png` | 2 | 유지 |
| `icon_book.png` | 2 | 유지 |
| `bg_book.png` | 2 | 유지 |
| `card_constellation.png` | 3 | 유지 |
| `card_locked.png` | 3 | 유지 |
| `book_cover.png` | 2 | 유지 |
| `book_entry_locked.png` | 2 | 유지 |
| `book_entry_unlocked.png` | 2 | 유지 |
| `ui_progress_bg.png` | 3 | 유지 |
| `ui_progress_fill.png` | 3 | 유지 |
| `ui_boss_warning.png` | **5** | **신규** |

**총계**: 기존 34종 유지 + 신규 1종 = **총 35종**

---

## 8. 다음 Iteration (v7) 개선 제안

1. **ScoreFloater 텍스트 이펙트 강화**: `fx_score_glow.png` — Score Floater "+N" 텍스트 배경 글로우 효과
2. **콤보 황금 파티클**: `fx_combo_spark.png` — 콤보 3연속 달성 시 황금 스파크 이펙트
3. **보스 웨이브 진입 연출 강화**: `bg_boss_overlay.png` — 보스 웨이브 중 화면 가장자리 붉은 빈넷 오버레이
4. **별자리 도감 배경 개선**: `bg_book_v2.png` — 별자리 도감 배경에 은하수 텍스처 추가
