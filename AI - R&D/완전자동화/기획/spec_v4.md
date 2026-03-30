# 게임 기획서 v4

**기준 버전**: v3 (iteration 2)
**작성일**: 2026-03-26
**변경 트리거**: QA 리포트 v4 (9.4/10) — Major 1건 + Minor 3건 반영 + 신규 기능 추가

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
- **패턴 외 색상 별 수집 시**: 점수만 부여되며 별자리 진행에는 기여하지 않는다. 이를 플레이어에게 명확히 인지시키기 위해 별자리 슬롯 UI에 아무런 반응이 없도록 한다 (슬롯 흔들림 없음).

### 메카닉 3: 콤보 시스템

- 별을 연속 3개 이상 수집하면 ×1.5 점수 배수 활성화.
- **콤보 활성 시 화면 상단 우측에 "COMBO ×1.5!" 팝업 텍스트가 1.5초간 표시된다.**
- **콤보 재활성 조건**: 콤보 배수가 이미 활성화된 상태(3개 이상)에서 추가 수집 시에는 팝업을 재표시하지 않는다. 팝업은 `_comboCount`가 정확히 3이 되는 순간에만 발화한다.
- 별을 한 번 놓치거나 Dark Star를 수집하면 콤보 초기화.

### 메카닉 4: Wave 진행도 표시 (신규)

- GameScene HUD 상단 좌측 Wave 표시 영역 하단에 **Wave 진행도 바(Progress Bar)**를 추가한다.
- 진행도 바는 현재 Wave에서 필요한 별자리 완성 횟수 대비 현재까지 완성한 횟수 비율로 채워진다.
  - Wave 1~6: 별자리 패턴 1종을 1회 완성하면 Wave 클리어 → 진행도 0% → 100%로 1스텝 증가.
  - Wave 7+: 랜덤 패턴 생성이므로 진행도 바는 "현재 패턴 내 수집 완료 별 수 / 총 요구 별 수" 비율로 실시간 갱신.
- **색상**: 진행도 미달 구간은 회색 반투명, 채워진 구간은 별빛 노란색 → 황금 그라디언트.
- **Wave 클리어 순간**: 진행도 바가 100%까지 빠르게 채워지는 0.3초 tween 연출 후 Wave 전환 팝업 표시.
- 진행도 바는 `HUDController.ts`에서 관리하며, `updateWaveProgress(current: number, total: number)` 메서드 신규 추가.

### 게임 흐름

1. 타이틀 화면 → 게임 시작
2. Wave 1~N 반복: 별 수집 → 별자리 완성 → 다음 Wave
3. 라이프 0이 되면 결과 화면 표시 (최고 점수 갱신 가능)
4. 재시작 또는 타이틀 복귀

---

## Wave 색상 설계표

> **원칙**: 각 Wave의 별자리 패턴 요구 색상은 반드시 해당 Wave의 스폰 색상 목록 내에 포함되어야 한다.

| Wave | 스폰 색상 | 별자리 이름 | 별자리 요구 색상 (총 개수) |
|---|---|---|---|
| 1 | RED, BLUE | 오리온자리 | RED×3, BLUE×2 (총 5개) |
| 2 | RED, BLUE, YELLOW | 큰곰자리 | BLUE×2, YELLOW×2, RED×1 (총 5개) |
| 3 | RED, BLUE, YELLOW, GREEN | 카시오페이아 | GREEN×2, RED×2, BLUE×1 (총 5개) |
| 4 | RED, BLUE, YELLOW, GREEN | 사자자리 | GREEN×2, YELLOW×2, RED×2 (총 6개) |
| 5 | RED, BLUE, YELLOW, GREEN, PURPLE | 전갈자리 | PURPLE×2, GREEN×2, RED×2 (총 6개) |
| 6 | RED, BLUE, YELLOW, GREEN, PURPLE | 황소자리 | PURPLE×2, BLUE×2, YELLOW×2, RED×1 (총 7개) |
| 7+ | 전체 5색 + DarkStar 비율 증가 | 무작위 조합 | **랜덤 패턴 생성 규칙 적용** (아래 상세 참조) |

**비고**:
- Wave 1에서 YELLOW/GREEN/PURPLE은 절대 스폰되지 않는다.
- Wave 2에서 GREEN/PURPLE은 절대 스폰되지 않는다.
- Wave 3에서 PURPLE은 절대 스폰되지 않는다.
- 보스 웨이브(Wave 3, 6, 9...)는 스폰 색상에 추가로 DarkStar 30% 비율로 섞인다.
- 개발봇은 `ConstellationManager.ts buildPattern()` 구현 시 반드시 이 표를 기준으로 색상 동기화해야 한다.

### Wave 7+ 무한 진행 랜덤 패턴 생성 규칙

Wave 7 이상에서는 순환 패턴 재사용 없이 매 Wave마다 다음 규칙에 따라 별자리 패턴을 동적으로 생성한다:

1. **사용 가능 색상**: 5색 전체 (RED, BLUE, YELLOW, GREEN, PURPLE)
2. **총 요구 별 수**: 6~8개 (Wave 번호가 높아질수록 상한 증가: `min(6 + Math.floor((wave-7)/2), 10)`)
3. **색상별 배분**: 사용 가능 색상 5종 중 무작위로 2~4종을 선택하여 총 개수를 배분
4. **최소 1종 색상당 최소 요구 수**: 1개 이상
5. **특정 색상 편중 방지**: 단일 색상이 전체 요구 수의 50% 초과 배정 불가
6. **시드(Seed) 미사용**: 매 Wave 호출마다 `Math.random()` 기반 순수 랜덤 생성 (재현 불필요)

> **구현 지침** (`ConstellationManager.ts buildPattern(wave: number)` 수정):
> - `wave >= 7` 조건에서 위 규칙 적용
> - `GameManager.getCurrentWaveConfig().availableColors`에서 색상 목록을 가져와 사용
> - 순환 방식(`(wave-1) % patterns.length`) 제거 및 대체

### "은하의 심연" 해금 조건 명확화

Wave 7+ 랜덤 패턴으로 완성하는 별자리는 모두 도감에서 **"은하의 심연"** 단일 항목으로 집계된다.

- **의도**: Wave 7+ 이후 등장하는 모든 무작위 별자리는 "은하의 심연"이라는 하나의 도감 슬롯에 귀속된다. 개별 이름을 부여하지 않으며 재해금 개념이 없다.
- **해금 조건**: Wave 7 이상에서 랜덤 패턴 별자리를 **최초 1회** 완성하면 해금. 이후 Wave 7+ 완성은 도감 기록을 갱신하지 않는다 (중복 등록 방지 정책 동일 적용).
- **도감 표시**: 해금 후 카드에 "은하의 심연 — Wave {최초 완성 Wave 번호}" 표시.
- **개발 지침**: `ConstellationBookManager.isUnlocked("은하의 심연")`이 `true`이면 `recordCompletion("은하의 심연", wave)` 호출을 건너뛴다. `ConstellationManager._checkCompletion()` 내에서 Wave >= 7인 경우 전달하는 name 파라미터를 항상 `"은하의 심연"`으로 고정.

---

## 씬 구성

| Scene 이름 | 역할 |
|---|---|
| `TitleScene` | 타이틀 화면, 게임 시작 버튼, 최고 점수 표시, **별자리 도감 버튼** |
| `GameScene` | 핵심 게임플레이 씬 (버킷, 별 낙하, UI 전부 포함, **Wave 진행도 바** 포함) |
| `ResultScene` | 게임 오버 후 점수 결과 표시, 재시작/타이틀 버튼 |
| `ConstellationBookScene` | 별자리 도감 씬 — 완성한 별자리 목록 및 상세 정보 표시 |

---

## 게임 오브젝트 설계

### Node 구성 (GameScene 기준)

| Node 이름 | 역할 |
|---|---|
| `Background` | 배경 우주 이미지 (정적 Sprite) |
| `StarSpawner` | 별 조각 생성 및 ObjectPool 관리 |
| `Bucket` | 플레이어 조작 버킷 (Sprite + Collider) |
| `StarFragment` (Prefab) | 낙하하는 별 조각 단일 오브젝트 — **ObjectPool로 재사용 관리** |
| `DarkStar` (Prefab) | 보스 웨이브 검은 별 — **ObjectPool로 재사용 관리** |
| `ConstellationUI` | 현재 목표 별자리 패턴 표시 UI (색상별 SVG 슬롯 사용) |
| `HUD` | 점수, 라이프, 현재 Wave 표시, **Wave 진행도 바** |
| `GameManager` | 게임 전체 상태 관리 (Singleton) |

### Node 구성 (TitleScene 기준) — v4 수정

| Node 이름 | 역할 |
|---|---|
| `TitleScriptNode` | `TitleScene.ts` 컴포넌트 **반드시 재연결** (M-NEW-01 해소) |
| `startButton` | 게임 시작 버튼 |
| `bookButton` | **별자리 도감 버튼 — 신규 노드 추가 필수** |
| `bestScoreLabel` | 최고 점수 표시 라벨 |

### 스크립트 파일 목록

| 파일명 | 역할 |
|---|---|
| `GameManager.ts` | 게임 상태(시작/진행/일시정지/종료), Wave 관리, 점수/라이프 관리. **일시정지는 `GameState.PAUSED` 플래그 방식으로 처리 (director.pause 미사용)** |
| `BucketController.ts` | 터치/드래그 입력 처리, 버킷 이동 로직, 충돌 처리 |
| `StarFragment.ts` | 별 조각 낙하 속도, 색상 타입, 화면 이탈 이벤트 처리. **ObjectPool에 반납 시 `reset()` 호출 보장** |
| `StarSpawner.ts` | Wave별 스폰 패턴 정의, **ObjectPool.ts를 통한 오브젝트 관리**, 타이머 기반 생성 |
| `ConstellationManager.ts` | 별자리 목표 패턴 정의, 수집 현황 비교, 완성 판정. **Wave 색상 설계표 기준으로 패턴 정의. Wave 7+ 랜덤 패턴 생성 적용. Wave 7+ 완성 시 name="은하의 심연" 고정 전달** |
| `HUDController.ts` | 점수/라이프/Wave HUD 갱신 (단일 책임). **`updateWaveProgress(current, total)` 신규 메서드 추가. Wave 진행도 바 tween 연출 포함** |
| `UIManager.ts` | 화면 전환 연출(페이드 인/아웃), 팝업 제어. HUD 갱신은 HUDController에 위임 |
| `ObjectPool.ts` | **오브젝트 풀 범용 유틸리티 — StarFragment 및 DarkStar Prefab 재사용에 실제 활용** |
| `SceneLoader.ts` | 씬 전환 유틸리티 (페이드 인/아웃 연출 포함) |
| `AudioManager.ts` | BGM / SFX 재생 관리 (Singleton) |
| `DataManager.ts` | **최고 점수 저장/불러오기 단독 관리** (`cc.sys.localStorage` 키: `star_sweeper_best`). GameManager 내 중복 저장 로직 제거됨 |
| `TitleScene.ts` | 타이틀 씬 컨트롤러. **bookButton @property 연결 포함** |
| `ConstellationBookManager.ts` | 별자리 도감 데이터 관리 — 완성 기록 저장/불러오기 |
| `ConstellationBookScene.ts` | 별자리 도감 씬 컨트롤러. **_createFallbackCard() UITransform 추가** |

---

## UI 설계

### TitleScene UI

- 게임 타이틀 텍스트 (Star Sweeper)
- [게임 시작] 버튼 (`startButton` 노드)
- 최고 점수 표시 라벨 (`bestScoreLabel` 노드)
- **[별자리 도감] 버튼** (`bookButton` 노드) — `ConstellationBookScene`으로 이동
  - 아이콘: `icon_book.svg`
  - **v4 필수**: TitleScene.scene에서 `bookButton` 노드가 반드시 존재해야 하며, `TitleScene.ts`의 `@property bookButton` 에 연결되어야 한다.

### GameScene HUD

- 상단 좌: 현재 Wave 표시 (Wave 1, Wave 2 ...)
  - **[신규] Wave 표시 하단**: Wave 진행도 바 (`waveProgressBar` 노드, 너비 120px, 높이 12px)
    - 미달 구간: 회색 반투명 (`slot_empty.svg` 색상 참고)
    - 달성 구간: 노란색 → 황금 그라디언트
    - Wave 클리어 순간: 0.3초 tween으로 100% → 다음 Wave 초기화
- 상단 중앙: 별자리 목표 패턴 아이콘 + 수집 현황
  - **색상별 SVG 슬롯 Sprite 사용**: 미수집 슬롯은 `slot_empty.svg`, 수집 완료 슬롯은 해당 색상의 `slot_red.svg` / `slot_blue.svg` / `slot_yellow.svg` / `slot_green.svg` / `slot_purple.svg`로 표시
  - 텍스트 기호('★') 방식 폐기
- 상단 우: 현재 점수
- **상단 우 (점수 하단)**: 콤보 활성 시 "COMBO ×1.5!" 텍스트 팝업 (1.5초 후 자동 소멸, 노란색 볼드). **`_comboCount === 3` 달성 순간에만 1회 발화**
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

### ConstellationBookScene UI

- "별자리 도감" 타이틀 텍스트
- 완성한 별자리 목록 (스크롤 뷰)
  - 각 항목: 별자리 이름 + 완성한 Wave 번호 + 완성 날짜
  - 미완성 별자리: 실루엣("???") 표시
- [뒤로가기] 버튼 — TitleScene 복귀
  - **v4 필수**: `ConstellationBookScene.scene`의 BackButton clickEvents[0].component 필드에 반드시 `"ConstellationBookScene"` 문자열을 명시. (n-02 해소)

---

## 오디오 설계

### BGM

| 씬 | BGM 설명 |
|---|---|
| TitleScene | 잔잔하고 몽환적인 우주 테마 루프 (120~130 BPM) |
| GameScene | 경쾌하고 긴장감 있는 아케이드 루프. 보스 웨이브 진입 시 템포 업 버전 전환 권장 |
| ConstellationBookScene | TitleScene BGM 재사용 또는 별도 잔잔한 루프 |

### SFX 트리거 명세

| 이벤트 | SFX 설명 | 트리거 위치 |
|---|---|---|
| 별 수집 (일반) | 경쾌한 "딩~" 단음. 짧고 맑은 고음 (0.2~0.3초) | `StarSpawner._onStarCaught()` — 비DarkStar 수집 시 |
| 별 수집 (DarkStar) | 낮고 탁한 "쿵" 효과음. 위협적 느낌 (0.3초) | `StarSpawner._onStarCaught()` — isDark 수집 시 |
| 별자리 완성 | 밝고 화려한 팡파레 짧은 버전 (0.8~1.0초) | `ConstellationManager._checkCompletion()` — 완성 판정 직후 |
| 라이프 감소 | 낮고 둔탁한 "쿵~" 또는 심장박동 느낌 (0.4초) | `GameManager.loseLife()` — 라이프 차감 직후 |
| Wave 클리어 | 상승하는 음계 짧은 멜로디 (1.0초) | `WaveManager` — Wave 전환 팝업 표시 시 |
| 게임 오버 | 슬프고 처지는 하강 멜로디 (1.5~2.0초) | `GameManager.triggerGameOver()` |
| 콤보 활성 (×1.5) | 밝은 "챙!" 짧은 효과음 (0.2초). 연속 수집 성공 피드백 | `GameManager` — `_comboCount === 3` 달성 시 (1회만) |
| 별자리 도감 신규 등록 | 밝고 짧은 "띵동" 효과음 (0.3초) | `ConstellationBookManager.recordCompletion()` — 신규 별자리 최초 완성 시 |
| **Wave 진행도 100% 도달** | 짧은 상승음 "삐잉~" (0.2초), Wave 클리어 SFX와 구분 | **[신규]** `HUDController.updateWaveProgress()` — progress === total 순간 |

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
| 도감 신규 등록 | `playBookUnlock()` |
| **Wave 진행도 100%** | **`playProgressComplete()`** |

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
| 별자리 1개당 요구 별 수 | 5~8개 (Wave 1~6 고정, Wave 7+ 랜덤) |
| 콤보 보너스 | 연속 3개 수집 시 × 1.5 배수 |

### 최고 점수 저장

- `DataManager.ts` **단독** 처리 (`sys.localStorage` 키: `star_sweeper_best`)
- `GameManager` 내 `_saveBestScore()` 메서드 제거 (중복 저장 완전 폐기)
- `GameManager.triggerGameOver()` 내에서 `DataManager.saveBestScore(this._score)` 단독 호출

### 별자리 도감 데이터

- 저장 키: `star_sweeper_book` (localStorage)
- 저장 형식: JSON 배열 — `[{ name: string, wave: number, date: string }, ...]`
- `ConstellationBookManager.ts` 단독 관리 (읽기/쓰기 모두)
- 동일 별자리를 재완성해도 최초 완성 기록만 유지 (중복 등록 방지)

### Wave 진행도 데이터 (신규)

- 런타임 전용 (저장 불필요)
- `currentProgress: number` — 현재 수집 완료 별 수
- `totalRequired: number` — 현재 패턴 총 요구 별 수
- Wave 전환 시 양쪽 모두 초기화

---

## 별자리 도감 시스템

### 개요

플레이어가 게임 플레이 중 완성한 별자리를 영구 기록하여, 타이틀에서 도감을 열람할 수 있는 콘텐츠 기능.

### 완성 조건 및 기록 시점

- `ConstellationManager._checkCompletion()` 에서 별자리 완성 판정 직후
- `ConstellationBookManager.recordCompletion(name, wave)` 호출
- 해당 별자리가 **최초 완성**인 경우: 도감에 등록 + `AudioManager.playBookUnlock()` 호출
- **재완성**(이미 등록된 별자리): 무음 처리 (중복 등록 없음)

### 도감 열람 흐름

1. TitleScene → [별자리 도감] 버튼 → `ConstellationBookScene` 로드
2. 도감 목록에 완성 별자리 카드 표시 (Wave 1~6 별자리 6종 + "은하의 심연" 1종)
3. 미완성 항목은 실루엣과 "???" 표시
4. [뒤로가기] → TitleScene 복귀

### 도감 수록 별자리 목록

| # | 별자리 이름 | 해금 조건 |
|---|---|---|
| 1 | 오리온자리 | Wave 1 완성 |
| 2 | 큰곰자리 | Wave 2 완성 |
| 3 | 카시오페이아 | Wave 3 완성 |
| 4 | 사자자리 | Wave 4 완성 |
| 5 | 전갈자리 | Wave 5 완성 |
| 6 | 황소자리 | Wave 6 완성 |
| 7 | 은하의 심연 | Wave 7+ 랜덤 패턴 최초 1회 완성. 단일 슬롯으로 고정 (재해금 없음) |

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
| 별자리 슬롯 - 빈 슬롯 | `slot_empty.svg` | 빈 원형 슬롯 (미수집 상태, 회색 반투명) |
| 별자리 슬롯 - RED | `slot_red.svg` | 빨강 채워진 원형 슬롯 (수집 완료 상태) |
| 별자리 슬롯 - BLUE | `slot_blue.svg` | 파랑 채워진 원형 슬롯 (수집 완료 상태) |
| 별자리 슬롯 - YELLOW | `slot_yellow.svg` | 노랑 채워진 원형 슬롯 (수집 완료 상태) |
| 별자리 슬롯 - GREEN | `slot_green.svg` | 초록 채워진 원형 슬롯 (수집 완료 상태) |
| 별자리 슬롯 - PURPLE | `slot_purple.svg` | 보라 채워진 원형 슬롯 (수집 완료 상태) |
| 버튼 배경 | `ui_button.svg` | 둥근 직사각형, 파란-보라 그라디언트 |
| 타이틀 로고 | `logo_title.svg` | "Star Sweeper" 텍스트, 별빛 장식 포함 |
| 일시정지 아이콘 | `icon_pause.svg` | 두 개의 세로 막대, 흰색, 32×32 |
| 콤보 이펙트 | `ui_combo_popup.svg` | "COMBO ×1.5!" 텍스트, 노란색 볼드, 별 장식 포함 |
| 도감 버튼 아이콘 | `icon_book.svg` | 별자리 도감 진입 버튼용 아이콘, 책+별 조합, 32×32 |
| 도감 배경 | `bg_book.svg` | 별자리 도감 씬 배경, 은은한 우주 테마, bg_space보다 밝고 차분한 분위기 |
| 도감 카드 - 완성 | `card_constellation.svg` | 도감 목록 카드 배경, 완성 상태 (밝은 금빛 테두리). nameLabel/waveLabel/dateLabel 레이아웃 포함 |
| 도감 카드 - 미완성 | `card_locked.svg` | 도감 목록 카드 배경, 미완성 상태 (어두운 회색). nameLabel 위치 "???" 표시용 |
| **[신규] Wave 진행도 바 배경** | **`ui_progress_bg.svg`** | **Wave 진행도 바 배경 트랙, 회색 반투명 둥근 직사각형 (120×12px)** |
| **[신규] Wave 진행도 바 채움** | **`ui_progress_fill.svg`** | **Wave 진행도 바 채움 그래픽, 노란→황금 그라디언트 둥근 직사각형 (120×12px)** |

---

## 개발 요청사항

### 기존 구현 유지 사항

- Cocos Creator 3.8.8 TypeScript 엄격 모드
- `cc.Component` 기반, `@ccclass` / `@property` 데코레이터
- `input.on(Input.EventType.TOUCH_MOVE)` 버킷 이동
- AABB 또는 `PhysicsSystem2D` 충돌 처리
- Wave 전환 팝업 + 0.5초 딜레이 연출
- 씬 전환 페이드 효과
- `GameState.PAUSED` 플래그 방식 일시정지 (director.pause 미사용)

### QA 이슈 수정 항목 (v4 필수)

**[M-NEW-01 — Major, 긴급] TitleScene.scene 스크립트 재연결 + bookButton 노드 추가**

- `TitleScene.scene`의 `TitleScriptNode`에 `TitleScene.ts` 컴포넌트를 **반드시 재연결**한다. (cc.MissingScript 상태 해소)
- 씬 내에 `bookButton` 노드를 신규 생성하고, `TitleScene.ts`의 `@property bookButton: Node` 에 연결한다.
- `bestScoreLabel` 노드도 null이 아닌지 확인하고 연결 상태를 점검한다.
- 이 수정 없이는 도감 버튼 클릭이 불가능하며 기획 충족률 100% 달성 불가.

**[n-02 — Minor, 우선순위 1] ConstellationBookScene.scene BackButton clickEvent component 필드 명시**

- `ConstellationBookScene.scene`의 BackButton `clickEvents[0].component` 필드에 `"ConstellationBookScene"` 문자열을 명시적으로 설정한다.
- 빈 문자열 상태에서는 런타임에 핸들러 탐색이 불안정하여 뒤로가기 버튼 미동작 위험이 있다.

**[n-01 — Minor, 우선순위 2] ConstellationBookScene._createFallbackCard()에 UITransform 추가**

- `ConstellationBookScene.ts`의 `_createFallbackCard()` 내 `new Node()` 생성 직후 `node.addComponent(UITransform)`을 호출하여 크기를 명시적으로 설정한다.
- 권장 크기: `uiTransform.setContentSize(200, 120)` (카드 Prefab 실제 크기에 맞게 조정).
- Prefab 미연결 fallback 케이스에서 레이아웃 그리드 내 크기 0×0 문제 해소.

**[n-03 — Minor, 우선순위 3] 도감 카드 Prefab 연결**

- `ConstellationBookScene.scene`의 `cardUnlockedPrefab` 및 `cardLockedPrefab` 프로퍼티에 Prefab을 연결한다.
- 디자인봇의 `card_constellation.svg` / `card_locked.svg` 기반 Prefab 제작 완료 후 연결 필수.

### 신규 기능 구현 요청

**[NEW-03] Wave 진행도 표시 시스템**

- `HUDController.ts`에 다음 요소 추가:
  - `@property waveProgressBg: Node` — 진행도 바 배경 노드
  - `@property waveProgressFill: Node` — 진행도 바 채움 노드
  - `updateWaveProgress(current: number, total: number): void` 메서드
    - `current / total` 비율로 `waveProgressFill` 노드의 `UITransform.width`를 조정 (최대 120px 기준)
    - `current === total` 시 0.3초 tween으로 width → 120 애니메이션 후 Wave 전환 이벤트 발생
    - 완료 시 `AudioManager.instance?.playProgressComplete()` 호출
- `GameManager.ts`에서 별 수집 / 별자리 진행 상황 변경 시마다 `HUDController.updateWaveProgress()` 호출
- Wave 전환 시 진행도 바 초기화 (width → 0, tween 즉시)
- `ConstellationManager.addStar()` 에서 수집 성공 시 현재 수집 수를 `GameManager`에 통보하는 콜백 또는 이벤트 방식 사용

**[기존 유지] 별자리 도감 시스템**

- `ConstellationBookManager.ts` 기존 구현 유지
- Wave 7+ 완성 시 `recordCompletion("은하의 심연", wave)` 고정 호출 (은하의 심연 명확화 대응)
- `AudioManager.ts`의 `playBookUnlock()` 유지

---

## 이번 iteration 변경사항

v3 대비 변경 및 추가 사항 요약:

### 1. TitleScene.scene 스크립트 재연결 + bookButton 노드 추가 (M-NEW-01 대응)

- QA에서 발견된 `TitleScriptNode`의 `cc.MissingScript` 상태를 필수 수정 항목으로 기획서에 명문화.
- `bookButton` 노드 추가 및 `TitleScene.ts @property` 연결을 개발봇 필수 작업으로 지시.
- `bestScoreLabel` null 상태 확인 및 재연결 지시 포함.

### 2. ConstellationBookScene BackButton clickEvent component 필드 명시 (n-02 대응)

- 뒤로가기 버튼 미동작 위험 제거를 위해 `clickEvents[0].component = "ConstellationBookScene"` 명시를 씬 파일 수정 필수 항목으로 격상.

### 3. ConstellationBookScene._createFallbackCard() UITransform 추가 (n-01 대응)

- Prefab 미연결 fallback 카드 생성 시 크기 0×0 문제 해소를 위한 `UITransform` 컴포넌트 추가 지시.

### 4. "은하의 심연" 해금 조건 명확화

- Wave 7+ 랜덤 패턴 별자리는 모두 "은하의 심연" 단일 슬롯으로 집계되며 최초 1회 해금 후 재해금 없음을 기획서에 공식 명문화.
- 개발봇에게 `ConstellationManager._checkCompletion()` 내 Wave >= 7 조건에서 name을 `"은하의 심연"`으로 고정하도록 지시.

### 5. 신규 기능: Wave 진행도 표시 시스템 추가

- GameScene HUD에 Wave 진행도 바를 신규 추가 (별자리 패턴 내 수집 진행도 실시간 시각화).
- `HUDController.ts`에 `updateWaveProgress(current, total)` 메서드 신규 추가.
- Wave 클리어 순간 0.3초 tween 연출 포함.
- `AudioManager.ts`에 `playProgressComplete()` 메서드 신규 추가.
- 디자인봇에게 `ui_progress_bg.svg` / `ui_progress_fill.svg` 2종 신규 요청.
