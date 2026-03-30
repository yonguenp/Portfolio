# QA 리포트 v2 (iter0 최종)
**게임**: Star Sweeper
**분석일**: 2026-03-26
**분석 대상**: TypeScript 스크립트 15개 + 씬 파일 3개
**기준 문서**: spec_v1.md, dev_notes_v1.md
**이전 리포트**: report_v1_final.md (7.8/10)

---

## 종합 평가

**점수: 8.2 / 10**

v1_final 대비 Critical 2건 중 2건 모두 수정 완료되어 점수가 상승했습니다. UIManager 콜백 메모리 누수(C-NEW-01)가 `onDestroy()` 정리 로직 추가로 해소되었고, GameManager.instance null assertion(C-REMAIN-02)도 안전한 null 반환 방식으로 교체되었습니다. 씬 파일 3개 구조는 이전과 동일하게 유효하며, 코드 품질은 전반적으로 안정권에 진입했습니다. 잔여 이슈는 Major 4건·Minor 5건으로, 게임 플레이 자체를 막는 Critical은 0건입니다.

---

## 이전 이슈 수정 확인

### report_v1_final Critical 이슈

| ID | 내용 | 수정 여부 | 확인 근거 |
|---|---|---|---|
| C-NEW-01 | UIManager 콜백(onScoreChanged, onLivesChanged, onGameOver) onDestroy 정리 누락 | **수정 완료** | UIManager.ts: `_onScoreCb`, `_onLivesCb`, `_onGameOverCb` 참조를 멤버 변수로 보관하고, `onDestroy()`에서 `gm.onScoreChanged === this._onScoreCb` 비교 후 null 처리 + Wave 리스너 `removeWaveChangedListener` 해제 확인 |
| C-REMAIN-02 | GameManager.instance! null assertion | **수정 완료** | GameManager.ts L56-61: `_instance`가 null이면 console.error 후 `return null` 반환. Non-null assertion 제거 확인 |

### report_v1_final Major 이슈

| ID | 내용 | 수정 여부 | 확인 근거 |
|---|---|---|---|
| M-NEW-01 | Wave 2 큰곰자리 GREEN 요구 / Wave 3 카시오페이아 PURPLE 요구 → 해당 Wave 스폰에 해당 색상 없음 | **미수정** | ConstellationManager.ts `buildPattern()`: 패턴 배열 변경 없음. 큰곰자리 `{ BLUE:2, GREEN:2, YELLOW:1 }`, 카시오페이아 `{ RED:2, PURPLE:2, BLUE:1 }` 그대로 유지. Wave 2 WAVE_CONFIGS에 GREEN 없음, Wave 3에 PURPLE 없음 |
| M-REMAIN-03 | SFX 호출 미연결 (별 수집음, 별자리 완성음, 라이프 감소음) | **미수정** | StarSpawner.ts `_onStarCaught()`, ConstellationManager.ts `_checkCompletion()`, GameManager.ts `loseLife()`에 AudioManager 호출 없음. ResultScene.ts의 playGameOver()만 연결 상태 유지 |
| M-REMAIN-01 | UIManager / HUDController 역할 중복 | **미수정** | UIManager.ts: scoreLabel, waveLabel, lifeIconsRoot 프로퍼티 보유. HUDController.ts: scoreLabel, waveLabel, lifeIcons 별도 보유. 씬 구조상 두 컴포넌트 모두 GameScene에 배치됨. 에디터에서 실제 연결에 따라 충돌 가능 |
| M-REMAIN-06 | director.pause()/resume() 사용 시 UI tween도 멈추는 부작용 | **미수정** | GameManager.ts `pauseGame()`: `director.pause()` 그대로 사용. WaveManager, UIManager의 tween 애니메이션도 정지됨 |

### 이전 Critical(report_v1 기준) 수정 유지 확인

| ID | 내용 | 유지 여부 |
|---|---|---|
| C-01 | onWaveChanged 단일 콜백 덮어쓰기 | 유지 — 배열 기반 `_onWaveChangedListeners` 정상 운용 |
| M-02 | WaveManager GameScene 미연결 | 유지 — GameScene.ts `waveManagerNode` + `addWaveChangedListener` 연결 유지 |
| M-04 | Wave 1 별자리 패턴 YELLOW 포함 | 유지 — 오리온자리 `{ RED:3, BLUE:2 }` 유지 |
| M-05 | 씬 전환 WaveChanged 콜백 누수 | 유지 — GameScene.ts `onDestroy()`에서 removeWaveChangedListener 호출 유지 |

---

## 잔여 이슈

### Critical (즉시 수정 필요)

**없음** — 이번 iteration에서 Critical 0건 달성

### Major (다음 iteration 필수)

| ID | 위치 | 내용 | 영향 |
|---|---|---|---|
| M-01 | ConstellationManager.ts `buildPattern()` | Wave 2 큰곰자리: GREEN 2개 요구 → Wave 2 스폰 색상에 GREEN 없어 달성 불가. Wave 3 카시오페이아: PURPLE 2개 요구 → Wave 3 스폰에 PURPLE 없어 달성 불가. Wave가 자동으로 진행되지 않는 상태 발생 가능 | Wave 2~3 진행 불가 버그로 연결될 수 있음 |
| M-02 | StarSpawner.ts, ConstellationManager.ts, GameManager.ts | SFX 3종 미호출: 별 수집음(`playCatch`), 별자리 완성음(`playConstellation`), 라이프 감소음(`playLoseLife`). AudioManager 메서드는 구현되어 있으나 호출 지점 없음 | 게임 피드백 크게 저하 |
| M-03 | UIManager.ts, HUDController.ts | HUD 역할 중복: 두 컴포넌트 모두 scoreLabel/waveLabel 보유. UIManager는 콜백 기반 갱신, HUDController는 메서드 직접 호출 방식. 에디터에서 동일 노드를 참조하면 중복 갱신 발생 | HUD 값 불일치 또는 이중 쓰기 가능 |
| M-04 | GameManager.ts `pauseGame()` | `director.pause()` 전역 일시정지가 WaveManager, UIManager의 tween 애니메이션(Wave 팝업 페이드, 페이드 오버레이)을 모두 멈춤. 일시정지 UI 자체 연출 불가 | 일시정지 상태에서 UI 응답성 저하 |

### Minor (개선 권장)

| ID | 위치 | 내용 |
|---|---|---|
| m-01 | ConstellationManager.ts L117-122 | `_colorSymbol()` 모든 색상이 동일한 '★' 반환. 색상별 진행 현황 시각 구분 불가 |
| m-02 | StarSpawner.ts L174 | `_constellationManager: any` 타입. `ConstellationManager \| null`로 교체 권장 |
| m-03 | ObjectPool.ts | 전혀 사용되지 않는 죽은 코드. StarSpawner 자체 인라인 풀이 대체. 제거 또는 실제 연결 필요 |
| m-04 | GameManager.ts, DataManager.ts | 최고 점수 이중 저장: `GameManager._saveBestScore()`와 `DataManager.saveBestScore()` 동시 존재. 동일 키(`star_sweeper_best`) 사용 — 중복이나 충돌 위험 낮으나 코드 혼란 유발 |
| m-05 | HUDController.ts, UIManager.ts | 콤보 3 이상 ×1.5 배수 발동 시 HUD에 콤보 상태 미표시. 플레이어가 보너스 발생 여부 인지 불가 |

---

## 기획서 충족도

| # | 요구사항 | 구현 여부 | 비고 |
|---|---|---|---|
| R01 | 버킷 좌우 드래그/탭 이동 | 완료 | TOUCH_MOVE 기반, onEnable/onDisable 등록 |
| R02 | 화면 경계 클램핑 | 완료 | view.getVisibleSize() 동적 대응 |
| R03 | 별 조각 낙하 (색상별, 랜덤 속도/위치) | 완료 | STAR_RESOURCE_MAP 6색 매핑 |
| R04 | 별 수집 시 점수 획득 | 완료 | 색상별 점수 (RED=10, YELLOW=15, PURPLE=20 등) 정확 |
| R05 | 별 놓침 시 라이프 -1 | 완료 | DarkStar 놓침은 라이프 감소 없음(기획 해석 여지 있음) |
| R06 | 라이프 3개, 0이 되면 게임 오버 | 완료 | `loseLife()` → `triggerGameOver()` 연결 |
| R07 | 별자리 목표 패턴 표시 (상단) | 부분 | 텍스트 라벨 기반, 색상별 아이콘 구분 없음 |
| R08 | 별자리 완성 시 +200점 | 완료 | `onConstellationDone()` → `addScore(200)` |
| R09 | Wave 진행 (별자리 3개마다 +1) | 완료 | `_constellationsCompleted % 3 === 0` |
| R10 | Wave 3마다 보스 웨이브 (DarkStar 30%) | 완료 | `isBossWave: true`, `_pickColor()` 30% |
| R11 | DarkStar 수집 시 -2 라이프 | 완료 | `_onStarCaught()` isDark → `loseLife(2)` |
| R12 | 콤보 3개 이상 ×1.5 점수 | 완료 | 로직 정확, HUD 표시만 미구현 |
| R13 | Wave별 낙하 속도/스폰 간격 수치 | 완료 | WAVE_CONFIGS 기획 수치 일치 |
| R14 | Wave 1에서 RED/BLUE만 스폰 | 완료 | WAVE_CONFIGS[0] 확인 |
| R15 | Wave 1 별자리 패턴 달성 가능 | 완료 | 오리온자리 {RED:3, BLUE:2} — Wave 1 색상 일치 |
| R16 | Wave 2~3 별자리 패턴 달성 가능 | 미충족 | 큰곰자리 GREEN 요구, 카시오페이아 PURPLE 요구 — 해당 Wave 스폰 불가 |
| R17 | TitleScene 구현 | 완료 | 씬 파일 + 스크립트 완성 |
| R18 | GameScene 구현 | 완료 | 씬 파일 + 11개 자식 노드 구조 완성, 에디터 수동 연결 잔존 |
| R19 | ResultScene 구현 | 완료 | 씬 파일 + 스크립트 완성 |
| R20 | 씬 전환 페이드 효과 | 완료 | UIManager.fadeIn/Out(), TitleScene/ResultScene 독립 구현 |
| R21 | localStorage 최고 점수 저장 | 완료 | DataManager + GameManager 이중 구조 |
| R22 | BGM 루프 재생 | 완료(코드) | AudioManager 구현 완성, 클립 에디터 연결 필요 |
| R23 | SFX 4종 | 부분 | 게임오버음 1종만 호출 연결, 나머지 3종 미호출 |
| R24 | Wave 전환 연출 (팝업) | 완료 | WaveManager 연결, 보스 경고 패널 별도 구현 |
| R25 | 일시정지/재개 | 완료 | `pauseGame()/resumeGame()` + PausePanel, UI tween 멈춤 부작용 잔존 |
| R26 | 오브젝트 풀링 | 완료 | StarSpawner 내부 풀, 풀 고갈 시 자동 확장 |
| R27 | SVG 리소스 12종 | 완료 | 전량 생성, 품질 양호~우수 |

**기획 충족률**: 22/27 (81.5%) — v1_final과 동일 (Critical 수정이 충족률보다 안정성 향상에 기여)

---

## 코드 품질 세부 분석

### TypeScript 문법 / import
- 15개 파일 전체 문법 오류 없음
- `@ccclass`, `@property` 데코레이터 전 컴포넌트 정상 적용
- `enum`, `interface`, `Record<>`, `Partial<Record<>>` 적절 사용
- 옵셔널 체이닝(`?.`) 및 null 병합(`??`) 대부분의 GameManager 접근에서 사용

### Cocos Creator 3.8.8 API 사용 적절성
- `input.on/off(Input.EventType.TOUCH_MOVE)`: onEnable/onDisable 쌍 — 올바름
- `view.getVisibleSize()`: 해상도 동적 획득 — 올바름
- `tween().to().call().start()`: UIOpacity 대상 — 올바름
- `director.addPersistRootNode()`: GameManager, AudioManager 싱글톤 유지 — 올바름
- `resources.load('path/spriteFrame', SpriteFrame, callback)`: StarFragment, HUDController — 올바름
- `sys.localStorage.getItem/setItem`: DataManager — 올바름
- `scheduleOnce()`: ConstellationManager 패턴 딜레이 — 올바름
- `director.pause()/resume()`: 전역 일시정지 부작용 — 기능은 동작하나 UI tween 영향 주의

### null 체크 / 메모리 누수
- **GameManager** (persist 싱글톤): `onLoad()` 중복 방지 + `onDestroy()` _instance null 처리 — 완전
- **AudioManager** (persist 싱글톤): 동일 패턴 적용 — 완전
- **UIManager.onDestroy()**: `_onScoreCb`, `_onLivesCb`, `_onGameOverCb` 참조 비교 후 null 처리 + Wave 리스너 배열 해제 — **v1_final 대비 수정 완료**
- **GameScene.onDestroy()**: `_onWaveChangedCb` removeWaveChangedListener 호출 — 완전
- **StarFragment.reset()**: `onMiss`, `onCatch` null 처리 — 완전
- **DataManager**: persist 아님, 씬 전환 시 재생성됨. static 메서드 방식이므로 인스턴스 의존성 없음 — 안전
- **SceneLoader**: persist 아님, 씬 전환마다 재생성. `_instance` 관리 — 안전
- **WaveManager**: `onDestroy()` 없음. GameScene을 통해 구독하므로 GameScene.onDestroy()에서 처리됨 — 간접적으로 안전

### 씬 파일 유효성

| 씬 | 포맷 | 최상위 노드 | 자식 노드 수 | 스크립트 연결 |
|---|---|---|---|---|
| TitleScene.scene | cc.SceneAsset + cc.Scene 정상 | Canvas | 5 | TitleScene.ts 연결 구조 확인 |
| GameScene.scene | cc.SceneAsset + cc.Scene 정상 | Canvas | 11 | GameScene.ts 포함 전체 컴포넌트 구조 확인 |
| ResultScene.scene | cc.SceneAsset + cc.Scene 정상 | Canvas | 7 | ResultScene.ts 연결 구조 확인 |

GameScene 필수 노드 체크: GameSceneController, GameManager, AudioManager, StarSpawner, Bucket, ConstellationManager, WaveManager, HUD, UIManager, PausePanel, FadeOverlay — 전체 존재 확인

---

## 다음 iteration 권장사항

### 기획봇에게

1. **Wave별 별자리 패턴 색상 매핑 규칙 명문화**
   - spec에 "각 Wave의 별자리 패턴은 해당 Wave의 availableColors 범위 내에서만 색상 요구" 원칙 명시
   - Wave 2(큰곰자리): GREEN 제거 또는 Wave 2 availableColors에 GREEN 추가 방향 결정 필요
   - Wave 3(카시오페이아): PURPLE 제거 또는 Wave 3에 PURPLE 추가 방향 결정 필요

2. **콤보 HUD 표시 스펙 추가**
   - 콤보 카운터 및 ×1.5 활성 여부를 어느 위치(상단/별 수집 팝업)에 표시할지 UI 스펙 추가

3. **DarkStar 놓침 시 라이프 처리 명확화**
   - 현재 코드: DarkStar를 놓쳐도 라이프 감소 없음 (`_onStarMissed()`에서 `isDark` 예외 처리). 기획서에 명시 없음. 의도인지 확인 필요

4. **일시정지 버튼 리소스 및 SVG 명세 추가**
   - `icon_pause.svg` 별도 제공 또는 `ui_button.svg` 재사용 여부 결정

### 디자인봇에게

1. **별자리 슬롯 색상 구분 버전 제작**
   - `ui_constellation_slot.svg` 색상별 버전 6종 (RED/BLUE/YELLOW/GREEN/PURPLE + 빈 슬롯). 현재 모든 별자리 진행이 동일한 '★' 기호로 표시됨

2. **콤보 이펙트 리소스**
   - "COMBO ×1.5!" 팝업 텍스트 이펙트 SVG 또는 레이아웃 가이드 제공

3. **icon_pause.svg 제공**
   - 현재 미제공. PauseButton 노드가 씬에 존재하나 비주얼 리소스 없음

4. **icon_life.svg 소형 버전**
   - 32×32 이하 표시 시 하트+별 조합 디테일 뭉개짐 가능 — 32×32 단순화 버전 권장

### 개발봇에게

1. **[Major - 우선순위 1] Wave 2~3 별자리 패턴 색상 불일치 수정**
   - 기획봇의 결정을 반영하여 `ConstellationManager.ts` `buildPattern()` 수정
   - 옵션 A: 큰곰자리 GREEN → YELLOW로 교체 (`{ BLUE:2, YELLOW:2, RED:1 }`), 카시오페이아 PURPLE → GREEN으로 교체
   - 옵션 B: `GameManager.getCurrentWaveConfig().availableColors`를 참조하여 달성 가능한 패턴 필터링 로직 추가

2. **[Major - 우선순위 2] SFX 3종 호출 연결**
   - `StarSpawner._onStarCaught()` 비DarkStar 수집 시: `AudioManager.instance?.playCatch()`
   - `ConstellationManager._checkCompletion()` 완성 직전: `AudioManager.instance?.playConstellation()`
   - `GameManager.loseLife()` 라이프 차감 직후: `AudioManager.instance?.playLoseLife()`

3. **[Major - 우선순위 3] UIManager / HUDController 역할 정리**
   - 권장: UIManager가 콜백을 받아 HUDController의 메서드를 호출하는 단방향 구조로 통합
   - 또는 UIManager의 중복 프로퍼티(scoreLabel, waveLabel, lifeIconsRoot) 제거하고 HUDController 참조로 위임

4. **[Major - 우선순위 4] director.pause() 부작용 대응**
   - `director.pause()` 대신 게임 내 플래그(GameState.PAUSED) 기반으로 update() 실행 여부를 직접 제어하는 방식 검토
   - 또는 tween을 Tween 인스턴스로 관리하여 일시정지 시 pause(), 재개 시 resume() 호출

5. **[Minor] StarSpawner `_constellationManager: any` 타입 수정**
   - `ConstellationManager | null`로 교체

6. **[Minor] ObjectPool.ts 정리**
   - 미사용 파일 제거 또는 StarSpawner 내부 풀을 ObjectPool로 교체하여 실제 활용

---

## 종합 점수 요약

| 항목 | 점수 | v1_final 대비 |
|---|---|---|
| 종합 평가 | **8.2 / 10** | +0.4 |
| 씬 구성 완성도 | **8.5 / 10** | 동일 |
| 그래픽 품질 | **8.0 / 10** | 동일 |
| 조작감 (코드 분석) | **8.5 / 10** | 동일 |
| 코드 품질 | **8.2 / 10** | +0.7 (Critical 2건 수정) |
| 기획 충족도 | **81.5%** | 동일 |
| Critical 잔여 | **0건** | -2건 |
| Major 잔여 | **4건** | 동일 |
| Minor 잔여 | **5건** | 동일 |
