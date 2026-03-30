# 게임 기획서 v2

**기준 버전**: v1 (iteration 0)
**작성일**: 2026-03-26
**변경 트리거**: QA 리포트 v2 (8.2/10) — Major 4건 반영

---

## 게임 개요

- 제목: **Star Sweeper** (별빛 청소부)
- 장르: 캐주얼 퍼즐 아케이드
- 타겟 플랫폼: 모바일 (Android/iOS)
- 해상도: 960x640
- 핵심 컨셉 (1줄): 떨어지는 별 조각을 받아 별자리를 완성하며 은하를 구하는 원터치 캐주얼 게임

---

## 핵심 메카닉

### 메카닉 1: 버킷 이동 & 별 수집

- 화면 하단에 버킷(바구니) 오브젝트가 위치하며, 플레이어는 좌우 드래그 또는 탭으로 버킷을 이동시킨다.
- 하늘에서 다양한 색상의 별 조각(Star Fragment)이 랜덤한 속도와 위치로 낙하한다.
- 버킷으로 별 조각을 받으면 점수 획득, 놓치면 라이프 1개 감소.
- 라이프는 총 3개이며 모두 소진되면 게임 오버.
- **Dark Star(검은 별)를 놓쳐도 라이프 감소 없음** (Dark Star는 수집 시에만 패널티 적용).

### 메카닉 2: 별자리 완성 시스템

- 화면 상단에 현재 목표 별자리 패턴이 표시된다.
- 버킷에 담긴 별 조각 색상이 요구 조건을 충족하면 별자리가 완성되며 보너스 점수 획득.
- **각 Wave의 별자리 패턴은 반드시 해당 Wave의 스폰 가능 색상 범위 내에서만 색상을 요구한다** (Wave 색상 설계표 참조).
- 별자리를 완성할수록 스테이지(Wave)가 진행되고 낙하 속도 및 패턴이 다양해진다.
- Wave 3마다 보스 웨이브 발생: 빠른 속도의 검은 별(Dark Star)이 등장하며, 받으면 라이프 2 감소.

### 메카닉 3: 콤보 시스템 (v2 HUD 표시 추가)

- 별을 연속 3개 이상 수집하면 ×1.5 점수 배수 활성화.
- **콤보 활성 시 화면 상단 우측에 "COMBO ×1.5!" 팝업 텍스트가 1.5초간 표시된다.**
- 별을 한 번 놓치거나 Dark Star를 수집하면 콤보 초기화.

### 게임 흐름

1. 타이틀 화면 → 게임 시작
2. Wave 1~N 반복: 별 수집 → 별자리 완성 → 다음 Wave
3. 라이프 0이 되면 결과 화면 표시 (최고 점수 갱신 가능)
4. 재시작 또는 타이틀 복귀

---

## Wave 색상 설계표 (신규 - 필수)

> **원칙**: 각 Wave의 별자리 패턴 요구 색상은 반드시 해당 Wave의 스폰 색상 목록 내에 포함되어야 한다.

| Wave | 스폰 색상 | 별자리 이름 | 별자리 요구 색상 (총 개수) |
|---|---|---|---|
| 1 | RED, BLUE | 오리온자리 | RED×3, BLUE×2 (총 5개) |
| 2 | RED, BLUE, YELLOW | 큰곰자리 | BLUE×2, YELLOW×2, RED×1 (총 5개) |
| 3 | RED, BLUE, YELLOW, GREEN | 카시오페이아 | GREEN×2, RED×2, BLUE×1 (총 5개) |
| 4 | RED, BLUE, YELLOW, GREEN | 사자자리 | GREEN×2, YELLOW×2, RED×2 (총 6개) |
| 5 | RED, BLUE, YELLOW, GREEN, PURPLE | 전갈자리 | PURPLE×2, GREEN×2, RED×2 (총 6개) |
| 6 | RED, BLUE, YELLOW, GREEN, PURPLE | 황소자리 | PURPLE×2, BLUE×2, YELLOW×2, RED×1 (총 7개) |
| 7+ | 전체 5색 + DarkStar 비율 증가 | 무작위 조합 | Wave 번호에서 사용 가능한 색상 내 무작위 생성 (6~7개) |

**비고**:
- Wave 1에서 YELLOW/GREEN/PURPLE은 절대 스폰되지 않는다.
- Wave 2에서 GREEN/PURPLE은 절대 스폰되지 않는다.
- Wave 3에서 PURPLE은 절대 스폰되지 않는다.
- 보스 웨이브(Wave 3, 6, 9...)는 스폰 색상에 추가로 DarkStar 30% 비율로 섞인다.
- 개발봇은 `ConstellationManager.ts buildPattern()` 구현 시 반드시 이 표를 기준으로 색상 동기화해야 한다.

---

## 씬 구성

| Scene 이름 | 역할 |
|---|---|
| `TitleScene` | 타이틀 화면, 게임 시작 버튼, 최고 점수 표시 |
| `GameScene` | 핵심 게임플레이 씬 (버킷, 별 낙하, UI 전부 포함) |
| `ResultScene` | 게임 오버 후 점수 결과 표시, 재시작/타이틀 버튼 |

---

## 게임 오브젝트 설계

### Node 구성 (GameScene 기준)

| Node 이름 | 역할 |
|---|---|
| `Background` | 배경 우주 이미지 (정적 Sprite) |
| `StarSpawner` | 별 조각 생성 및 오브젝트 풀 관리 |
| `Bucket` | 플레이어 조작 버킷 (Sprite + Collider) |
| `StarFragment` (Prefab) | 낙하하는 별 조각 단일 오브젝트 |
| `DarkStar` (Prefab) | 보스 웨이브 검은 별 |
| `ConstellationUI` | 현재 목표 별자리 패턴 표시 UI |
| `HUD` | 점수, 라이프, 현재 Wave 표시 |
| `GameManager` | 게임 전체 상태 관리 (Singleton) |

### 스크립트 파일 목록

| 파일명 | 역할 |
|---|---|
| `GameManager.ts` | 게임 상태(시작/진행/일시정지/종료), Wave 관리, 점수/라이프 관리. **일시정지는 `GameState.PAUSED` 플래그 방식으로 처리 (director.pause 미사용)** |
| `BucketController.ts` | 터치/드래그 입력 처리, 버킷 이동 로직, 충돌 처리 |
| `StarFragment.ts` | 별 조각 낙하 속도, 색상 타입, 화면 이탈 이벤트 처리 |
| `StarSpawner.ts` | Wave별 스폰 패턴 정의, 오브젝트 풀링, 타이머 기반 생성 |
| `ConstellationManager.ts` | 별자리 목표 패턴 정의, 수집 현황 비교, 완성 판정. **Wave 색상 설계표 기준으로 패턴 정의** |
| `HUDController.ts` | 점수/라이프/Wave HUD 갱신 (단일 책임). **UIManager의 HUD 중복 프로퍼티 제거 후 HUDController 단독 담당** |
| `UIManager.ts` | 화면 전환 연출(페이드 인/아웃), 팝업 제어. HUD 갱신은 HUDController에 위임 |
| `ObjectPool.ts` | 오브젝트 풀 범용 유틸리티 (StarFragment/DarkStar 재사용) |
| `SceneLoader.ts` | 씬 전환 유틸리티 (페이드 인/아웃 연출 포함) |
| `AudioManager.ts` | BGM / SFX 재생 관리 (Singleton) |
| `DataManager.ts` | 최고 점수 저장/불러오기 (cc.sys.localStorage). **GameManager 내 중복 저장 로직 제거, DataManager 단독 처리** |

---

## UI 설계

### TitleScene UI

- 게임 타이틀 텍스트 (Star Sweeper)
- [게임 시작] 버튼
- 최고 점수 표시 라벨

### GameScene HUD

- 상단 좌: 현재 Wave 표시 (Wave 1, Wave 2 ...)
- 상단 중앙: 별자리 목표 패턴 아이콘 + 수집 현황 (색상별 구분 슬롯 — `ui_constellation_slot_[color].svg` 사용)
- 상단 우: 현재 점수
- **상단 우 (점수 하단)**: 콤보 활성 시 "COMBO ×1.5!" 텍스트 팝업 (1.5초 후 자동 소멸, 노란색 볼드)
- 하단 좌: 라이프 아이콘 × 3
- 일시정지 버튼 (상단 우측 모서리, `icon_pause.svg` 사용)

### PausePanel UI

- **일시정지 상태에서도 UI 애니메이션(tween, 페이드 오버레이 등)은 계속 재생된다.**
- 게임 로직(StarSpawner 타이머, 별 이동, 충돌 판정)만 `GameState.PAUSED` 플래그로 정지.
- [재개] 버튼 / [타이틀로] 버튼

### ResultScene UI

- "GAME OVER" 타이틀 텍스트
- 이번 점수 / 최고 점수 표시
- [다시 시작] 버튼
- [타이틀로] 버튼

---

## 오디오 설계 (신규 - 필수)

### BGM

| 씬 | BGM 설명 |
|---|---|
| TitleScene | 잔잔하고 몽환적인 우주 테마 루프 (120~130 BPM) |
| GameScene | 경쾌하고 긴장감 있는 아케이드 루프. 보스 웨이브 진입 시 템포 업 버전 전환 권장 |

### SFX 트리거 명세

| 이벤트 | SFX 설명 | 트리거 위치 |
|---|---|---|
| 별 수집 (일반) | 경쾌한 "딩~" 단음. 짧고 맑은 고음 (0.2~0.3초) | `StarSpawner._onStarCaught()` — 비DarkStar 수집 시 |
| 별 수집 (DarkStar) | 낮고 탁한 "쿵" 효과음. 위협적 느낌 (0.3초) | `StarSpawner._onStarCaught()` — isDark 수집 시 |
| 별자리 완성 | 밝고 화려한 팡파레 짧은 버전 (0.8~1.0초), 별이 쏟아지는 느낌 | `ConstellationManager._checkCompletion()` — 완성 판정 직후 |
| 라이프 감소 | 낮고 둔탁한 "쿵~" 또는 심장박동 느낌 (0.4초) | `GameManager.loseLife()` — 라이프 차감 직후 |
| Wave 클리어 | 상승하는 음계 짧은 멜로디 (1.0초). 다음 Wave 진입 느낌 | `WaveManager` — Wave 전환 팝업 표시 시 |
| 게임 오버 | 슬프고 처지는 하강 멜로디 (1.5~2.0초) | `GameManager.triggerGameOver()` |
| 콤보 활성 (×1.5) | 밝은 "챙!" 짧은 효과음 (0.2초). 연속 수집 성공 피드백 | `GameManager` — 콤보 3 달성 시 |

**AudioManager 메서드 매핑**:

| SFX 이벤트 | AudioManager 메서드명 |
|---|---|
| 별 수집 (일반) | `playCatch()` |
| 별 수집 (DarkStar) | `playDarkCatch()` |
| 별자리 완성 | `playConstellation()` |
| 라이프 감소 | `playLoseLife()` |
| Wave 클리어 | `playWaveClear()` |
| 게임 오버 | `playGameOver()` |
| 콤보 활성 | `playCombo()` |

---

## 데이터 설계

### 별 조각 색상 타입

| 색상 | 상수명 | 점수 |
|---|---|---|
| 빨강 | `RED` | 10 |
| 파랑 | `BLUE` | 10 |
| 노랑 | `YELLOW` | 15 |
| 초록 | `GREEN` | 15 |
| 보라 | `PURPLE` | 20 |

### Wave 밸런스

| Wave | 낙하 속도 (px/s) | 스폰 간격 (s) | 특이사항 |
|---|---|---|---|
| 1 | 200 | 1.5 | RED/BLUE만 등장 |
| 2 | 240 | 1.3 | YELLOW 추가 |
| 3 | 280 | 1.1 | GREEN 추가, 보스 웨이브 (DarkStar 30%) |
| 4 | 320 | 1.0 | (4색 유지) |
| 5 | 360 | 0.9 | PURPLE 추가 |
| 6+ | +20/wave | -0.05/wave | 5색 유지, 보스 웨이브 주기 3Wave마다 반복, 난이도 점진 상승 |

### 라이프 & 별자리

| 항목 | 수치 |
|---|---|
| 초기 라이프 | 3 |
| 별 놓침 패널티 | -1 라이프 |
| Dark Star 수집 패널티 | -2 라이프 |
| **Dark Star 놓침 패널티** | **없음 (0 라이프 감소)** |
| 별자리 완성 보너스 | +200 점 |
| 별자리 1개당 요구 별 수 | 4~7개 (Wave 증가에 따라 증가) |
| 콤보 보너스 | 연속 3개 수집 시 × 1.5 배수 |

### 최고 점수 저장

- `DataManager.ts` 단독 처리 (`sys.localStorage` 키: `star_sweeper_best`)
- `GameManager` 내 `_saveBestScore()` 중복 메서드 제거

---

## 디자인 요청사항

디자인봇에게 요청할 SVG 기반 리소스 목록:

| 리소스명 | 파일명 | 설명 |
|---|---|---|
| 배경 | `bg_space.svg` | 어두운 우주 배경, 은하수 느낌, 작은 별 다수 배치 |
| 버킷 | `bucket.svg` | 둥근 황금빛 바구니, 반짝이는 테두리 |
| 별 조각 - 빨강 | `star_red.svg` | 5각 별 모양, 빨간 계열, 광택 효과 |
| 별 조각 - 파랑 | `star_blue.svg` | 5각 별 모양, 파란 계열, 광택 효과 |
| 별 조각 - 노랑 | `star_yellow.svg` | 5각 별 모양, 노란 계열, 광택 효과 |
| 별 조각 - 초록 | `star_green.svg` | 5각 별 모양, 초록 계열, 광택 효과 |
| 별 조각 - 보라 | `star_purple.svg` | 5각 별 모양, 보라 계열, 광택 효과 |
| 검은 별 | `star_dark.svg` | 뾰족하고 어두운 별, 붉은 테두리, 위협적 느낌 |
| 라이프 아이콘 | `icon_life.svg` | 하트+별 조합, 32×32 단순화 버전 (소형 표시 최적화) |
| 별자리 슬롯 - 공통 | `ui_constellation_slot.svg` | 빈 원형 슬롯 (목표 별 표시용, 회색) |
| 별자리 슬롯 - RED | `ui_constellation_slot_red.svg` | 빨강 채워진 슬롯 |
| 별자리 슬롯 - BLUE | `ui_constellation_slot_blue.svg` | 파랑 채워진 슬롯 |
| 별자리 슬롯 - YELLOW | `ui_constellation_slot_yellow.svg` | 노랑 채워진 슬롯 |
| 별자리 슬롯 - GREEN | `ui_constellation_slot_green.svg` | 초록 채워진 슬롯 |
| 별자리 슬롯 - PURPLE | `ui_constellation_slot_purple.svg` | 보라 채워진 슬롯 |
| 버튼 배경 | `ui_button.svg` | 둥근 직사각형, 파란-보라 그라디언트 |
| 타이틀 로고 | `logo_title.svg` | "Star Sweeper" 텍스트, 별빛 장식 포함 |
| 일시정지 아이콘 | `icon_pause.svg` | 두 개의 세로 막대, 흰색, 32×32 |
| 콤보 이펙트 | `ui_combo_popup.svg` | "COMBO ×1.5!" 텍스트, 노란색 볼드, 별 장식 포함 |

---

## 개발 요청사항

### 기존 구현 유지 사항

- Cocos Creator 3.8.8 TypeScript 엄격 모드
- `cc.Component` 기반, `@ccclass` / `@property` 데코레이터
- `input.on(Input.EventType.TOUCH_MOVE)` 버킷 이동
- AABB 또는 `PhysicsSystem2D` 충돌 처리
- `ObjectPool.ts` `get()` / `put()` 오브젝트 풀링
- Wave 전환 팝업 + 0.5초 딜레이 연출
- 씬 전환 페이드 효과

### QA Major 이슈 수정 항목 (필수)

**[M-01] Wave 2~3 별자리 색상 불일치 수정**
- `ConstellationManager.ts buildPattern()` 수정
- Wave 2 큰곰자리: `{ BLUE:2, YELLOW:2, RED:1 }` (GREEN 제거)
- Wave 3 카시오페이아: `{ GREEN:2, RED:2, BLUE:1 }` (PURPLE 제거)
- Wave 4 이후도 위 Wave 색상 설계표와 완전히 동기화

**[M-02] SFX 3종 호출 연결**
- `StarSpawner._onStarCaught()`: 비DarkStar → `AudioManager.instance?.playCatch()`, isDark → `AudioManager.instance?.playDarkCatch()`
- `ConstellationManager._checkCompletion()` 완성 직후: `AudioManager.instance?.playConstellation()`
- `GameManager.loseLife()` 차감 직후: `AudioManager.instance?.playLoseLife()`

**[M-03] HUDController / UIManager 역할 분리**
- `UIManager`에서 `scoreLabel`, `waveLabel`, `lifeIconsRoot` 중복 프로퍼티 제거
- HUD 갱신은 `HUDController` 단독 담당
- `UIManager`는 화면 전환 연출(페이드, 팝업)만 담당

**[M-04] director.pause() 부작용 수정 — 일시정지 메카닉 개선**
- `director.pause()` 전역 호출 제거
- `GameManager`에 `GameState.PAUSED` 상태 플래그 도입
- 게임 로직 정지 대상: `StarSpawner` 타이머, 별 낙하 `update()`, 충돌 판정
- UI 애니메이션 비정지 대상: `UIManager` tween, `WaveManager` 팝업 페이드, `FadeOverlay`
- `PausePanel` 표시/숨김 tween은 일시정지 상태와 무관하게 항상 동작

### 신규 기능 구현 요청

**[NEW-01] 콤보 HUD 표시**
- 콤보 3 이상 달성 시 "COMBO ×1.5!" 텍스트 노드를 점수 하단에 표시
- tween으로 1.5초 후 페이드 아웃
- 동시에 `AudioManager.instance?.playCombo()` 호출
- `HUDController.ts`에 `showComboEffect()` 메서드 추가

### Minor 이슈 개선 권장

- `StarSpawner._constellationManager: any` → `ConstellationManager | null` 타입 교체
- `ObjectPool.ts` 미사용 시 삭제 또는 StarSpawner 내부 풀을 ObjectPool로 교체
- `DataManager`로 최고 점수 저장 일원화 (`GameManager._saveBestScore()` 제거)

---

## 이번 iteration 변경사항

v1 대비 변경 및 추가 사항 요약:

### 1. Wave 색상 설계표 신규 추가 (M-01 대응)
- Wave 1~6 전체에 걸쳐 스폰 색상과 별자리 요구 색상을 표로 명문화
- "별자리 패턴은 해당 Wave의 스폰 가능 색상 내에서만 요구" 원칙 명시
- Wave 2 큰곰자리: GREEN 제거 → YELLOW 교체, Wave 3 카시오페이아: PURPLE 제거 → GREEN 교체

### 2. 오디오 설계 섹션 신규 추가 (M-02 대응)
- SFX 7종 이벤트별 설명, 트리거 위치, AudioManager 메서드명 명세
- BGM 씬별 분위기 가이드 추가

### 3. 일시정지 메카닉 변경 (M-04 대응)
- `director.pause()` 전역 정지 방식 → `GameState.PAUSED` 플래그 방식으로 전환
- UI 애니메이션은 일시정지 상태에서도 계속 재생됨을 명시

### 4. 콤보 HUD 표시 신규 기능 추가 (QA 권장사항 채택)
- 콤보 ×1.5 활성 시 "COMBO ×1.5!" 팝업 텍스트 UI 스펙 추가
- `HUDController.showComboEffect()` 구현 요청
- `ui_combo_popup.svg` 디자인 리소스 추가

### 5. 기타 명확화
- Dark Star 놓침 시 라이프 미감소 원칙 명문화
- HUDController / UIManager 역할 분리 기준 명시
- DataManager 단독 최고 점수 관리 원칙 명시
- `icon_pause.svg` 디자인 리소스 추가
- 별자리 슬롯 색상별 버전 5종 디자인 리소스 추가
