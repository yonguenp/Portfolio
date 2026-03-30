# 게임 기획서 v7 — "Star Sweeper: Chaos Edition"

**기준 버전**: v6 (hotfix 포함)
**작성일**: 2026-03-30
**변경 트리거**: 사용자 피드백 — 게임 단순함·랜덤성 부족·재미 부족 전면 개편 요청

---

## 기획 방향 전환 선언

v1~v6 기반 핵심 루프(떨어지는 별 → 버킷 수집 → 별자리 완성)는 유지하되,
**아래 3축**을 전면 강화하여 "또 하고 싶은 게임"으로 개편한다.

| 축 | 문제 | v7 해법 |
|---|---|---|
| **랜덤성** | 매 게임 동일한 패턴 반복 | 특수별 타입 5종 + 랜덤 이벤트 2종 |
| **재미** | 버킷 좌우 이동만 반복 | 파워업 3종 + 콤보 강화 + 긴장감 요소(폭탄별) |
| **반복 플레이** | 클리어 후 할 게 없음 | 최고점수 갱신 동기 + 해금 비선형 루트 + 숨겨진 비밀별자리 |

---

## V7 변경/추가 기능 목록

| ID | 분류 | 내용 | 우선순위 |
|---|---|---|---|
| V7-01 | 핵심 메카닉 | 특수별 타입 시스템 (Rainbow·Bomb·Speed·Magnet·Ghost) | Critical |
| V7-02 | 핵심 메카닉 | 파워업 낙하 아이템 (Shield·SlowTime·Wildcard) | Critical |
| V7-03 | 콤보 강화 | 콤보 배율 단계 강화 (x3/x5/x10) + 화면 연출 | Major |
| V7-04 | 동적 난이도 | 점수 기반 낙하 속도 + 동시 다중 낙하 | Major |
| V7-05 | 랜덤 이벤트 | 유성우(Meteor Shower) + 중력 역전(Gravity Flip) | Major |
| V7-06 | 재플레이 | 하이스코어 DB + Wave별 최고 기록 + 비밀 별자리 해금 | Major |
| V7-07 | 연출 강화 | 별 수집 충격파 이펙트 + 버킷 차징 오라 + 별자리 완성 폭발 | Minor |
| V7-08 | 기술 부채 | v6 미구현분(M-WP-01, n-05, BossWarningPanel) 이월 해소 | Critical |

---

## V7-01: 특수별 타입 시스템

### StarType 열거형 (신규)

```typescript
enum StarType {
    NORMAL  = 'normal',   // 기존 5색 일반별
    RAINBOW = 'rainbow',  // 무지개별 — 어느 슬롯에나 맞음
    BOMB    = 'bomb',     // 폭탄별   — 수집 시 생명력 -1
    SPEED   = 'speed',    // 가속별   — 수집 시 버킷 속도 5초 2배
    MAGNET  = 'magnet',   // 자석별   — 수집 시 3초간 인접 별 자동 흡수
    GHOST   = 'ghost',    // 유령별   — 반투명, 잡으면 슬롯 색 랜덤 변경
}
```

### 등장 확률 (Wave별)

| StarType | Wave 1~3 | Wave 4~6 | Wave 7+ |
|---|---|---|---|
| NORMAL | 95% | 82% | 68% |
| RAINBOW | 3% | 7% | 10% |
| BOMB | 2% | 6% | 10% |
| SPEED | — | 3% | 5% |
| MAGNET | — | 2% | 4% |
| GHOST | — | — | 3% |

### 각 타입 처리 로직

**RAINBOW별**:
- 현재 슬롯 패턴에서 비어 있는 슬롯 중 하나를 랜덤 선택, 채움
- 점수: 해당 슬롯 기본 점수 × 2
- 이펙트: 수집 시 무지개색 방사형 폭발 (effect_rainbow_burst.png)

**BOMB별**:
- 수집(버킷에 닿음) 시: `GameManager.loseLife()` 즉시 호출
- 회피(화면 하단 통과) 시: 패널티 없음 → 피하는 게 정답
- 이펙트: 수집 시 붉은 폭발 + 화면 붉은 플래시 0.3초 (effect_bomb_explode.png)
- 비주얼: 어두운 자주색 + 해골 문양 스프라이트

**SPEED별**:
- 수집 시: `BucketController.activateSpeedBoost(5.0)` — 버킷 이동 속도 2배, 5초간
- 속도 부스트 중 버킷 테두리 노란 오라 표시 (effect_bucket_speed_aura.png)

**MAGNET별**:
- 수집 시: 3초간 화면 내 모든 NORMAL별이 버킷 방향으로 이동 (이동 보조 벡터 추가)
- 활성 중 버킷 주변 파란 자기장 링 표시
- BOMB별은 마그넷 효과 면역 (자석에 안 붙음)

**GHOST별**:
- 수집 시: 현재 슬롯 패턴에서 랜덤 슬롯 1개의 색을 다른 색으로 변경
- 때로는 유리한 색으로 바뀌고 때로는 불리한 색으로 바뀜 (진짜 랜덤)
- 이펙트: 반투명 자주빛 소용돌이 (effect_ghost_swirl.png)

---

## V7-02: 파워업 낙하 아이템

### 개요
별과는 별개로 3~8초마다 파워업 아이템이 화면 상단에서 낙하.
버킷으로 수집 시 효과 즉시 발동.

### 파워업 종류

| 아이템 | 아이콘 | 효과 | 지속시간 |
|---|---|---|---|
| **Shield** | icon_powerup_shield.png | 다음 BOMB별 1회 무력화 (생명 보호) | 다음 폭탄까지 |
| **SlowTime** | icon_powerup_slow.png | 모든 별 낙하 속도 50% 감소 | 6초 |
| **Wildcard** | icon_powerup_wildcard.png | 현재 패턴의 랜덤 슬롯 2개 즉시 채움 (공짜 완성) | 즉발 |

### 등장 규칙
- Wave 2 이상부터 파워업 낙하 활성화
- 동시에 최대 1개만 화면에 존재
- 60초 게임 플레이당 평균 4~6회 등장
- 파워업 낙하 속도: 일반별 대비 70% (조금 느리게)

### 파워업 관리 컴포넌트
`PowerupManager.ts` (신규):
```typescript
class PowerupManager {
    @property({ type: Node }) powerupContainer: Node = null;
    spawnInterval: number = 8;      // 초
    spawnVariance: number = 5;      // ±5초 랜덤

    scheduleNextSpawn(): void       // 다음 파워업 예약
    spawnPowerup(type: PowerupType): void  // 특정 타입 생성
    _onPowerupCaught(type: PowerupType): void  // 수집 처리
}
```

---

## V7-03: 콤보 배율 단계 강화

### 기존 콤보 (v6)
콤보 3이상 → 황금색 텍스트 표시

### v7 콤보 단계

| 콤보 수 | 배율 | 연출 | 이름 |
|---|---|---|---|
| 1~2 | ×1.0 | 흰색 +score | — |
| 3~4 | ×1.5 | 황금색 +score + COMBO 팝업 | ⭐ Hot |
| 5~9 | ×2.0 | 오렌지 +score + 불꽃 파티클 | 🔥 Fire |
| 10~14 | ×3.0 | 빨간 +score + 화면 가장자리 불 오라 | 💥 Blazing |
| 15+ | ×5.0 | 무지개 +score + 전체 화면 섬광 0.1초 | 🌈 LEGENDARY |

### 콤보 유지 조건 변경
- v6: BOMB별을 못 피하면 콤보 리셋 (새로운 긴장 요소)
- v7: 일반별 미수집도 콤보 리셋 (기존 유지)

---

## V7-04: 동적 난이도 시스템

### 낙하 속도 공식 (개선)

```
dropSpeed = baseSpeed + (waveBonus × wave) + (scoreBonus × floor(score/500))
```

- `baseSpeed`: 150px/s
- `waveBonus`: 20px/s per wave
- `scoreBonus`: 10px/s per 500점 (최대 +80px/s 추가)
- 상한선: 450px/s (이 이상은 인간이 반응 불가)

### 동시 낙하 별 수

| Wave | 동시 낙하 별 수 | 별 간격 |
|---|---|---|
| 1~3 | 1개 | — |
| 4~6 | 1~2개 (랜덤) | 좌우 무작위 레인 |
| 7 (보스) | 2~3개 | 조밀한 간격 |

### 레인 시스템
화면을 5개 레인(x = -240, -120, 0, 120, 240)으로 나눠 별이 특정 레인을 따라 낙하.
Wave 4부터 다중 레인 동시 사용.

---

## V7-05: 랜덤 이벤트 시스템

### 유성우 이벤트 (Meteor Shower)

**발동 조건**: 임의로 30~90초 간격 + 콤보 10 이상 달성 시 확률 30%

**효과**:
- 5초간 별이 3배 속도로 쏟아짐 (한 번에 4~6개)
- 이 기간 수집 점수 2배
- 화면 배경이 밝아짐 + 별 비 사운드 효과

**개발 구현**: `WaveManager.triggerMeteorShower()` — 5초 후 자동 종료

### 중력 역전 이벤트 (Gravity Flip)

**발동 조건**: Wave 5 이상에서 확률 15% (1분에 1회 이하)

**효과**:
- 4초간 별 낙하 방향 역전 → 화면 하단에서 위로 올라옴
- 버킷이 화면 상단으로 이동 (위치 반전)
- 경고: 발동 전 2초간 "⚠ GRAVITY FLIP" 텍스트 표시

**개발 구현**: `StarFragment.setGravityFlip(true)` — Y 방향 반전, BucketController Y 이동 범위 반전

---

## V7-06: 재플레이 시스템

### 하이스코어 저장
`DataManager`에 추가:
```typescript
saveBestScore(score: number): void
getBestScore(): number
saveWaveBest(wave: number, score: number): void    // Wave별 최고 점수
getWaveBest(wave: number): number
```

타이틀 화면에 "BEST: N" 표시 (기존 bestScoreLabel 활용)

### 비밀 별자리 해금 조건

| 별자리 | 해금 조건 | 힌트 |
|---|---|---|
| 봉황자리 | 콤보 15 이상 달성 후 클리어 | "전설의 연타를 완성하라" |
| 용자리 | BOMB별 10개 회피 누적 | "어둠을 피해 빛을 모아라" |
| 불사조자리 | 생명 하나도 잃지 않고 Wave 7 클리어 | "상처 없는 별빛" |
| 은하단자리 | 유성우 이벤트 3회 경험 | "폭풍 속에서 별을 모은 자" |

`DataManager`에 해금 조건 누적 카운터 추가:
- `maxComboReached: number`
- `bombsAvoided: number`
- `perfectWaveClears: number`
- `meteorShowerCount: number`

### 결과 화면 강화
ResultScene에 표시 항목 추가:
- 이번 게임 최대 콤보
- 피한 폭탄 수
- 발동된 이벤트 종류
- 해금된 비밀 별자리 (있을 경우)

---

## V7-07: 연출 강화

### 별 수집 충격파
별 수집 시 작은 원형 충격파 이펙트 방사 (effect_shockwave.png)
반경 40px까지 0.2초 확장 후 페이드아웃.

### 버킷 차징 오라
콤보 5 이상 시 버킷 주변에 오라 이펙트 표시:
- 콤보 5~9: 노란 오라
- 콤보 10~14: 주황 오라
- 콤보 15+: 무지개 오라 (애니메이션)

### 별자리 완성 폭발
Wave 클리어 시 별자리 슬롯 전체에서 방사형 스파크 파티클 (0.8초).
현재 단순 완성 → 화면 중앙 별자리 이름 크게 표시 + 빛 폭발 + 사운드.

---

## V7-08: v6 이월 기술 부채 해소

| ID | 내용 | v7 처리 |
|---|---|---|
| M-WP-01 | updateWaveProgress() 호출 미연결 | V7 개발봇 필수 작업 1순위 |
| n-05 | Wave>=7 isUnlocked 체크 누락 | V7 개발봇 필수 작업 |
| V6-03 | BossWarningPanel 씬 연결 | V7 개발봇 필수 작업 |
| ScoreFloater | label.isBold 빌드 오류 | V7 개발봇 즉시 수정 |

---

## 디자인봇에게 (v7 에셋 요청)

### 특수별 신규 스프라이트

| 파일명 | 크기 | 설명 |
|---|---|---|
| `star_rainbow.png` | 64×64 | 무지개별 — 흰색 코어 + 7색 홀로그래픽 외곽 |
| `star_bomb.png` | 64×64 | 폭탄별 — 어두운 자주색 + 붉은 균열 문양 |
| `star_speed.png` | 64×64 | 가속별 — 전기 파란색 + 번개 테일 |
| `star_magnet.png` | 64×64 | 자석별 — 청록색 + N/S극 표시 원형 |
| `star_ghost.png` | 64×64 | 유령별 — 연보라 반투명 + 흘러내리는 형태 |

### 파워업 아이템 아이콘

| 파일명 | 크기 | 설명 |
|---|---|---|
| `icon_powerup_shield.png` | 48×48 | 방패 — 푸른 헥사곤 방패 |
| `icon_powerup_slow.png` | 48×48 | 슬로우 — 모래시계 + 얼음 결정 |
| `icon_powerup_wildcard.png` | 48×48 | 와일드카드 — 별+번개 합성 아이콘 |

### 이펙트 에셋

| 파일명 | 크기 | 설명 |
|---|---|---|
| `effect_rainbow_burst.png` | 128×128 | 무지개별 수집 폭발 |
| `effect_bomb_explode.png` | 128×128 | 폭탄별 수집 붉은 폭발 |
| `effect_shockwave.png` | 96×96 | 별 수집 충격파 링 |
| `effect_meteor_shower.png` | 256×64 | 유성우 이벤트 배경 오버레이 |

### 기존 에셋 전면 리디자인 (퀄리티 업)

| 파일명 | 현재 문제 | v7 개선 방향 |
|---|---|---|
| `star_*.png` (5종) | 단순 단색 원형 | 코어 광원 + 외곽 글로우 + 4방향 빛줄기 |
| `bucket.png` | 단순 사다리꼴 | 헥사곤 크리스탈 용기 + 보라빛 내부 발광 |
| `bg_space.png` | 단색 어두운 배경 | 네뷸러 그라디언트 + 원거리 성운 + 다층 별빛 |
| `logo_title.png` | 평범한 텍스트형 | 황금 글로우 + 별자리 라인 장식 |
| `ui_button.png` | 단순 둥근 사각형 | 유리 재질 프로스트 글래스 효과 |
| `slot_*.png` (5종) | 단순 원형 테두리 | 보석 컷팅 느낌 + 색상별 내부 글로우 |

---

## 기획 충족률 목표

| 항목 | v6 | v7 목표 |
|---|---|---|
| 랜덤성 | 낮음 | 높음 (특수별 5종 + 이벤트 2종) |
| 재플레이성 | 낮음 | 높음 (비밀별자리 4종 + 하이스코어) |
| 긴장감 | 낮음 | 높음 (폭탄별 + 파워업 선택) |
| 시각 퀄리티 | 보통 | 높음 (전면 리디자인) |
| 종합 완성도 | 9.5/10 목표 | 9.8/10 목표 |
