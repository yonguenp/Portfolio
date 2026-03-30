# 디자인 노트 v7 — 전면 리디자인 "Chaos Edition"
**게임**: Star Sweeper
**Iteration**: v7
**작성일**: 2026-03-30
**디자이너**: 디자인봇 (AI 자동화 파이프라인)

---

## 1. v7 디자인 방향

사용자 피드백: "90년대 플래시 게임보다 못한 리소스"
→ 단순 원형/사각형 SVG 수준에서 **글로우·그라디언트·광원 효과가 살아있는 PNG** 전면 전환.

### 디자인 원칙
- **광원 중심 설계**: 모든 오브젝트는 내부 발광 코어를 가진다
- **레이어드 글로우**: 코어 → 중간 글로우 → 외곽 헤일로 3단 구조
- **색상 대비 극대화**: 어두운 우주 배경 vs 빛나는 오브젝트
- **SVG 탈피**: 모든 신규·리디자인 에셋은 PNG 래스터 포맷 (Cocos 호환성 우선)

---

## 2. 전면 리디자인 에셋 (기존 교체)

### star_*.png — 별 스프라이트 5종 (64×64)

**기존 문제**: 단색 원형, 글로우 없음, 밋밋함
**v7 설계**:
- Layer 1 (외곽 헤일로): 반경 32px, 해당 색 opacity 0.2, 블러 효과
- Layer 2 (글로우 링): 반경 22px, 해당 색 opacity 0.55
- Layer 3 (본체 원): 반경 14px, 중심→외곽 방사형 그라디언트 (밝음→해당색)
- Layer 4 (코어): 반경 6px, 흰색 opacity 0.9
- Layer 5 (빛줄기): 4방향 얇은 라인 (+자 형태), 길이 28px, opacity 0.6

| 파일 | 코어색 | 글로우색 | 헤일로색 |
|---|---|---|---|
| `star_red.png` | #ffffff | #ff4444 | #ff000055 |
| `star_blue.png` | #ffffff | #4488ff | #0044ff55 |
| `star_yellow.png` | #ffffff | #ffee44 | #ffcc0055 |
| `star_green.png` | #ffffff | #44dd66 | #00aa4455 |
| `star_purple.png` | #ffffff | #bb44ff | #7700ff55 |

### bucket.png — 버킷 (120×80)

**기존 문제**: 단순 사다리꼴, 아무 개성 없음
**v7 설계**: 헥사곤 크리스탈 용기
- 외형: 위가 넓은 육각형 컵 형태
- 테두리: 금빛(#ffd700) 이중 테두리 (2px 외곽 + 1px 내부 하이라이트)
- 내부: 어두운 보라색(#1a0a3a) → 밝은 보라(#6644aa) 방사형 그라디언트
- 내부 발광: 보라빛 글로우 (중앙 #9966ff, opacity 0.4)
- 상단 개구부: 밝은 흰빛 하이라이트 선
- 좌우 크리스탈 면: 각도 있는 하이라이트 사선

### bg_space.png — 배경 (1280×720)

**기존 문제**: 단색 어두운 배경, 아무 깊이감 없음
**v7 설계**: 다층 네뷸러 우주
- Layer 1 (심우주): #000010 단색 기저
- Layer 2 (네뷸러A): 청보라 성운 (#0d0535 → #1a0a4a) 대형 타원 2개, opacity 0.7
- Layer 3 (네뷸러B): 청록 성운 (#031a35 → #0a1a4a) 소형 타원 1개, opacity 0.4
- Layer 4 (별밭 원거리): 2px 미만 흰 점 200개 이상, opacity 0.3~0.6 랜덤
- Layer 5 (별밭 근거리): 3~4px 밝은 점 50개, opacity 0.7~1.0 랜덤 (일부 글로우)
- 하단부: 약한 수평 성운 띠 (#0a044a, opacity 0.3)

### logo_title.png — 타이틀 로고 (640×120)

**기존 문제**: 평범한 흰색 텍스트
**v7 설계**:
- 텍스트 "STAR SWEEPER": 황금 그라디언트 (#ffe866 → #ffd700 → #ff9900)
- 외곽선: 어두운 갈색(#4a2000) 3px
- 텍스트 글로우: #ffcc00 blur 8px
- 상단 장식: 별자리 점선 패턴 (작은 별 7개 연결, #ffffff opacity 0.5)
- 하단 장식: 황금 수평선 양쪽에서 중앙으로 수렴

### ui_button.png — 버튼 (240×70)

**기존 문제**: 단순 둥근 사각형
**v7 설계**: 프로스트 글래스 버튼
- 배경: #1a1a4a opacity 0.85 + rx 16 (둥근 모서리)
- 테두리: 이중 — 외곽 #4466aa 1.5px + 내부 #6688cc 0.8px opacity 0.6
- 상단 하이라이트: 흰색 → 투명 선형 그라디언트 상단 30% 영역, opacity 0.15
- 미세 별 장식: 내부 좌우 끝에 소형 별 아이콘 (#6688cc opacity 0.4)

### slot_*.png — 슬롯 5종 (40×40)

**기존 문제**: 단순 원형 테두리 only
**v7 설계**: 보석 컷팅 슬롯
- 외곽 링: 해당 색 1.5px 테두리 + 외부 글로우 2px blur
- 중간 링: 해당 색 opacity 0.25 채움 원
- 내부: #0a0a22 어두운 배경 (별이 채워질 자리)
- 상단 하이라이트: 흰색 작은 호(arc), opacity 0.4 (보석 반사광)
- 빈 상태: 슬롯 중앙 점선 원 (#ffffff opacity 0.15)

---

## 3. 신규 에셋 — 특수별 5종

### star_rainbow.png (64×64)
- 코어: 흰색 원 반경 8px
- 빛줄기: 6방향 (60° 간격), 각 줄기 다른 무지개색
- 외곽 링: 7색 원형 그라디언트 (conic-gradient 모방)
- 헤일로: 흰색 opacity 0.2

### star_bomb.png (64×64)
- 본체: 진한 자주 (#3d0000) 원 반경 14px
- 표면 균열: 붉은 선 (#cc2200) 3~4개, opacity 0.8
- 외곽 글로우: 어두운 빨강 (#880000) opacity 0.5
- 중앙 해골/X 마크: #660000 opacity 0.6
- 불꽃 파티클 힌트: 상단에 작은 오렌지 점 3개

### star_speed.png (64×64)
- 본체: 전기 파란색 (#0088ff) 원
- 번개 무늬: #ffffff opacity 0.8 zig-zag 패턴 2개
- 외곽 글로우: 시안 (#00ccff) opacity 0.6
- 속도감 테일: 우측으로 늘어난 타원형 글로우 (모션 블러 모방)

### star_magnet.png (64×64)
- 본체: 청록 (#00ccaa) 원
- U자 자석 심볼: 흰색 두꺼운 획 (N/S 양극 표시)
- 극 색상: N극 = 파랑, S극 = 빨강
- 외곽 자기장 링: 점선 원 2개 (#00ffcc opacity 0.3)

### star_ghost.png (64×64)
- 본체: 연보라 (#cc88ff) 불규칙한 원 (약간 물결치는 외형 모방)
- 전체 opacity: 0.7 (반투명)
- 내부: #ffffff → #cc88ff 방사형, opacity 0.6
- 외곽: 흘러내리는 방울 형태 힌트 (하단 3개 소형 원)

---

## 4. 신규 에셋 — 파워업 아이콘 3종 (48×48)

### icon_powerup_shield.png
- 헥사곤 방패 외형 (#1144aa)
- 테두리: 밝은 파란 (#4488ff) 2px
- 중앙 별 문양: #88aaff
- 글로우: 파랑 #4488ff opacity 0.4

### icon_powerup_slow.png
- 모래시계 외형 (#223366)
- 테두리: 시안 (#00aadd)
- 상하 모래 영역: #4499cc 채움
- 중앙 잘록한 부분: 얼음 결정 모양 힌트 (#aaddff opacity 0.6)

### icon_powerup_wildcard.png
- 오각별 (#ffcc00) 외형
- 중앙 번개 심볼: #ffffff
- 외곽 글로우: #ffee44 opacity 0.5
- 코너 소형 별 4개 (×자 배치)

---

## 5. 신규 에셋 — 이펙트 4종

### effect_rainbow_burst.png (128×128)
- 중앙 흰색 원형 폭발 반경 20px
- 8방향 방사형 스트라이프, 각 7색 무지개 색상
- 외곽 별 파티클 힌트 8개
- 전체: 흰색 중심 → 투명 외곽

### effect_bomb_explode.png (128×128)
- 중앙 오렌지 원형 폭발 (#ff6600)
- 불규칙 폭발 외형 (8~12개 삐죽 돌출부)
- 화염 레이어: 빨강(#ff2200) 내부 + 오렌지(#ff6600) 외부 + 노랑(#ffcc00) 중심
- 스모크 힌트: 회색 반투명 구름 3개 (폭발 주변)

### effect_shockwave.png (96×96)
- 중앙 투명 + 외곽 흰색 얇은 링 (2~3px)
- 링 내부: 약한 시안 (#88eeff opacity 0.15) 채움
- 더블 링: 반경 30px + 반경 44px
- 용도: 별 수집 순간 짧게 scale up 후 fade

### effect_meteor_shower.png (256×64)
- 배경: 진한 남색 (#05021a opacity 0.5) 배너
- 대각선 유성 궤적 5개 (#ffffff → #6688ff → 투명)
- 상단 별 파티클 10개
- 화면 전체 오버레이용 (tileSize로 확장)

---

## 6. 컬러 팔레트 v2 (전면 개정)

```json
{
  "space": {
    "deep": "#000010",
    "mid": "#050520",
    "nebula_purple": "#1a0a4a",
    "nebula_blue": "#0a1a4a"
  },
  "stars": {
    "red_core": "#ff6666", "red_glow": "#ff0000",
    "blue_core": "#6699ff", "blue_glow": "#0044ff",
    "yellow_core": "#ffee88", "yellow_glow": "#ffcc00",
    "green_core": "#66ee88", "green_glow": "#00aa44",
    "purple_core": "#cc88ff", "purple_glow": "#7700ff"
  },
  "ui": {
    "button_base": "#1a1a4a",
    "button_border": "#4466aa",
    "button_highlight": "#ffffff",
    "text_gold": "#ffd700",
    "text_white": "#ffffff",
    "text_muted": "#8888bb"
  },
  "bucket": {
    "body": "#1a0a3a",
    "glow": "#9966ff",
    "rim": "#ffd700"
  }
}
```

---

## 7. 작업 완료 목록

| 파일명 | 크기 | 구분 | 상태 |
|---|---|---|---|
| `star_red.png` | 64×64 | 리디자인 | ✅ |
| `star_blue.png` | 64×64 | 리디자인 | ✅ |
| `star_yellow.png` | 64×64 | 리디자인 | ✅ |
| `star_green.png` | 64×64 | 리디자인 | ✅ |
| `star_purple.png` | 64×64 | 리디자인 | ✅ |
| `bucket.png` | 120×80 | 리디자인 | ✅ |
| `bg_space.png` | 1280×720 | 리디자인 | ✅ |
| `logo_title.png` | 640×120 | 리디자인 | ✅ |
| `ui_button.png` | 240×70 | 리디자인 | ✅ |
| `slot_red.png` | 40×40 | 리디자인 | ✅ |
| `slot_blue.png` | 40×40 | 리디자인 | ✅ |
| `slot_yellow.png` | 40×40 | 리디자인 | ✅ |
| `slot_green.png` | 40×40 | 리디자인 | ✅ |
| `slot_purple.png` | 40×40 | 리디자인 | ✅ |
| `star_rainbow.png` | 64×64 | 신규 | ✅ |
| `star_bomb.png` | 64×64 | 신규 | ✅ |
| `star_speed.png` | 64×64 | 신규 | ✅ |
| `star_magnet.png` | 64×64 | 신규 | ✅ |
| `star_ghost.png` | 64×64 | 신규 | ✅ |
| `icon_powerup_shield.png` | 48×48 | 신규 | ✅ |
| `icon_powerup_slow.png` | 48×48 | 신규 | ✅ |
| `icon_powerup_wildcard.png` | 48×48 | 신규 | ✅ |
| `effect_rainbow_burst.png` | 128×128 | 신규 | ✅ |
| `effect_bomb_explode.png` | 128×128 | 신규 | ✅ |
| `effect_shockwave.png` | 96×96 | 신규 | ✅ |
| `effect_meteor_shower.png` | 256×64 | 신규 | ✅ |

**총 26종 PNG 생성 완료**

---

## 8. 개발봇에게

### 에셋 로딩 경로
모든 에셋: `assets/resources/*.png`
로드 방법: `resources.load('파일명없는확장자', SpriteFrame, callback)`

### 특수별 StarType별 스프라이트 매핑
```typescript
const starSpritePaths = {
    [StarType.RAINBOW]: 'star_rainbow',
    [StarType.BOMB]:    'star_bomb',
    [StarType.SPEED]:   'star_speed',
    [StarType.MAGNET]:  'star_magnet',
    [StarType.GHOST]:   'star_ghost',
};
```

### 이펙트 재생 권장 방법
```typescript
// 수집 시 충격파 이펙트
EffectManager.playEffect('effect_shockwave', worldPos, 0.3);
// 폭탄 수집
EffectManager.playEffect('effect_bomb_explode', worldPos, 0.5);
// 무지개별 수집
EffectManager.playEffect('effect_rainbow_burst', worldPos, 0.4);
```
(EffectManager 신규 작성 또는 ScoreFloater 패턴 활용)

### ui_boss_warning.png 재제작 필요
기존 파일 깨짐 확인됨. 480×120 붉은 경고 패널 재생성 요청.
