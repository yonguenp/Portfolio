# 디자인 노트 v1
**게임**: Star Sweeper
**Iteration**: 0
**작성일**: 2026-03-25
**디자이너**: 디자인봇 (AI 자동화 파이프라인)

---

## 1. 디자인 컨셉

**테마**: 어두운 우주를 배경으로 한 밝고 생동감 있는 별빛 캐주얼 게임
**키워드**: 신비로운 / 따뜻한 금빛 / 모바일 친화적 / 심플하고 명확한 구분

배경의 어두운 우주 톤 위에 각 별 조각의 색상이 선명하게 대비되도록 설계했습니다.
버킷은 황금빛으로 제작하여 별을 받는 고귀한 느낌을 강조합니다.

---

## 2. 컬러 팔레트

컬러 코드 전체: `디자인/color_palette_v1.json`

| 용도 | 대표 색상 |
|---|---|
| 배경 | #000008 ~ #0a0a2e (딥 우주 그라디언트) |
| 버킷 | #ffd700 황금빛 계열 |
| 별 - 빨강 | #ff2020 기본 / #ff4444 발광 |
| 별 - 파랑 | #1a7fff 기본 / #4488ff 발광 |
| 별 - 노랑 | #ffdd00 기본 / #ffee44 발광 |
| 별 - 초록 | #00cc44 기본 / #44ff88 발광 |
| 별 - 보라 | #aa22ff 기본 / #cc66ff 발광 |
| 검은 별 | #111111 기본 / #880000 위협적 발광 |
| UI 버튼 | #6644ff ~ #2200aa 파랑-보라 그라디언트 |
| 타이틀 텍스트 | #ffd700 황금빛 |

---

## 3. 리소스 파일 목록

| 파일명 | 크기 | 용도 | 경로 |
|---|---|---|---|
| `bg_space.svg` | 960×640 | 게임 배경 (우주+은하수+별) | `assets/resources/bg_space.svg` |
| `bucket.svg` | 120×80 | 플레이어 조작 버킷 | `assets/resources/bucket.svg` |
| `star_red.svg` | 64×64 | 별 조각 - 빨강 (10점) | `assets/resources/star_red.svg` |
| `star_blue.svg` | 64×64 | 별 조각 - 파랑 (10점) | `assets/resources/star_blue.svg` |
| `star_yellow.svg` | 64×64 | 별 조각 - 노랑 (15점) | `assets/resources/star_yellow.svg` |
| `star_green.svg` | 64×64 | 별 조각 - 초록 (15점) | `assets/resources/star_green.svg` |
| `star_purple.svg` | 64×64 | 별 조각 - 보라 (20점) | `assets/resources/star_purple.svg` |
| `star_dark.svg` | 64×64 | Dark Star - 보스 웨이브 (-2 라이프) | `assets/resources/star_dark.svg` |
| `icon_life.svg` | 48×48 | 라이프 아이콘 (하트+별 조합) | `assets/resources/icon_life.svg` |
| `ui_constellation_slot.svg` | 40×40 | 별자리 목표 빈 슬롯 | `assets/resources/ui_constellation_slot.svg` |
| `ui_button.svg` | 200×60 | 범용 버튼 배경 | `assets/resources/ui_button.svg` |
| `logo_title.svg` | 480×160 | "Star Sweeper" 타이틀 로고 | `assets/resources/logo_title.svg` |

총 12종 완성.

---

## 4. 개발봇에게 전달할 에셋 경로 정보

**리소스 루트**: `assets/resources/`

### Cocos Creator 에셋 로딩 예시

```typescript
// 배경 스프라이트
const bgPath = "bg_space";               // SpriteFrame

// 버킷
const bucketPath = "bucket";             // SpriteFrame

// 별 조각 (색상별)
const starPaths: Record<string, string> = {
  RED:    "star_red",
  BLUE:   "star_blue",
  YELLOW: "star_yellow",
  GREEN:  "star_green",
  PURPLE: "star_purple",
  DARK:   "star_dark",
};

// UI
const uiPaths = {
  lifeIcon:         "icon_life",
  constellationSlot:"ui_constellation_slot",
  button:           "ui_button",
  logo:             "logo_title",
};
```

### 에셋 크기 기준 (Cocos Content Size 설정 권장)

| 에셋 | contentSize 권장 |
|---|---|
| bg_space | 960 × 640 |
| bucket | 120 × 80 |
| star_* (5색) | 64 × 64 |
| star_dark | 64 × 64 |
| icon_life | 48 × 48 |
| ui_constellation_slot | 40 × 40 |
| ui_button | 200 × 60 |
| logo_title | 480 × 160 |

### 별자리 슬롯 사용법 (ConstellationUI)
- `ui_constellation_slot.svg`를 빈 슬롯(미수집)으로 사용
- 수집 완료 시 해당 색상의 `star_*.svg`로 교체
- 슬롯 간격: 44px (슬롯 크기 40 + 여백 4)

---

## 5. 디자인 의도 및 특이사항

### bg_space.svg
- 960×640 전체 화면 크기로 제작
- 3개의 성운 레이어(radialGradient) + 은하수 띠 + 90여개 별 배치
- 하단 페이드 처리로 버킷/UI와 자연스럽게 구분됨

### bucket.svg
- 황금빛 바구니, 손잡이(아치형) 포함
- 내부는 어두운 우주색으로 별이 담기는 느낌 강조
- 좌우 별 장식으로 수집 도구임을 직관적으로 표현

### star_*.svg (5색 별)
- 모두 동일한 5각별 형태, 색상만 구분
- radialGradient로 입체감과 광택 효과 적용
- 배경과 명확히 대비되는 채도 높은 색상 선택

### star_dark.svg
- 8각별로 더 날카롭고 위협적인 형태
- 검은 몸체 + 붉은 발광 테두리 + 붉은 눈 형태 중앙
- 일반 별들과 즉시 구분 가능한 실루엣

### ui_constellation_slot.svg
- 점선 원형으로 "비어있음" 상태를 직관적으로 표현
- 별 실루엣 내부 표시로 용도 안내
- 40×40 소형 크기로 HUD 상단에 여러 개 배치 적합

---

## 6. 다음 Iteration (v2) 개선 제안

1. **애니메이션 준비**: 별 반짝임 효과를 위한 keyframe 기반 SVG 애니메이션 추가 (또는 Cocos Creator tween 대응용 다중 프레임 스프라이트)
2. **별자리 완성 이펙트**: 폭발/파티클용 `effect_star_burst.svg` 추가 (현재 미포함)
3. **Dark Star 강화 이펙트**: 보스 웨이브 경고 아이콘 `icon_warning.svg` 추가
4. **버킷 업그레이드 스킨**: Wave 진행에 따른 버킷 외형 변화를 위한 `bucket_silver.svg`, `bucket_rainbow.svg`
5. **배경 레이어 분리**: 패럴랙스 스크롤을 위한 `bg_stars_near.svg` / `bg_stars_far.svg` 분리 버전
6. **폰트 통일**: 타이틀 로고와 UI 버튼의 폰트를 웹폰트 또는 커스텀 폰트로 교체 시 텍스트 SVG 경로화 필요
7. **콤보 UI**: 연속 3개 수집 콤보 시 표시할 `ui_combo_badge.svg` 추가
