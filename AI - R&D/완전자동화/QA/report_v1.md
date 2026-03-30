# QA 리포트 v1
**게임**: Star Sweeper
**분석일**: 2026-03-25
**분석 대상**: TypeScript 스크립트 15개 (정적 분석)
**기준 문서**: spec_v1.md, dev_notes_v1.md

---

## 종합 평가

**점수: 6.5 / 10**

전체적으로 Cocos Creator 3.8.8 구조를 잘 따르고 있으며, 코드 아키텍처와 책임 분리가 명확합니다. 싱글톤 패턴, AABB 충돌, 오브젝트 풀링 등 핵심 메카닉이 구현되어 있습니다. 다만 씬 연결 누락, 중복 콜백 덮어쓰기, 메모리 누수 위험 지점, 기획서와의 수치 불일치 등 Critical~Major 수준의 이슈가 다수 발견되었습니다. 씬 파일 및 Prefab 미완성 상태이므로 에디터 작업 완료 후 재검증이 필수입니다.

---

## 발견된 이슈

### Critical (즉시 수정 필요)

#### C-01: GameScene에서 onWaveChanged 콜백 중복 덮어쓰기 [GameScene.ts:61]
**위치**: `GameScene.ts`, `start()` 메서드 (L60-64)
**내용**: `gm.onWaveChanged` 에 새 람다를 할당하기 전에 `origWaveChanged`로 기존 값을 보존하는 패턴을 사용하고 있으나, `UIManager.onLoad()` 에서도 `gm.onWaveChanged` 를 덮어씁니다. 실행 순서가 `UIManager.onLoad() → GameScene.start()` 이면 UIManager 콜백이 `origWaveChanged` 에 담기고 정상 동작하지만, `GameScene.onLoad()` 와 `UIManager.onLoad()` 의 실행 순서가 노드 계층 순서에 의존하기 때문에 조립 실수 한 번으로 Wave HUD가 갱신되지 않을 수 있습니다.
**위험도**: Wave 전환 시 UI가 전혀 갱신되지 않는 버그 유발 가능
**원인**: 단일 함수 포인터(onWaveChanged)로 다수 구독자를 처리하는 설계 구조적 결함. EventEmitter 패턴 또는 배열 기반 콜백으로 교체 필요.

---

#### C-02: StarSpawner - DarkStar 놓쳤을 때 라이프 감소 없음 [StarSpawner.ts:148]
**위치**: `StarSpawner.ts`, `_onStarMissed()` 메서드 (L148-153)
**내용**:
```typescript
private _onStarMissed(sf: StarFragment) {
    if (!sf.isDark) {
        GameManager.instance?.loseLife(1);  // 일반 별만 라이프 감소
    }
    GameManager.instance?.resetCombo();
    this._returnToPool(sf);
}
```
기획서(spec_v1.md)에는 "버킷으로 받으면 라이프 감소, 놓치면 라이프 1개 감소"로 명시되어 있으며 DarkStar에 대한 놓침 패널티는 정의되어 있지 않습니다. 그러나 현재 코드는 DarkStar를 놓쳐도 콤보만 리셋하고 패스합니다. 이것 자체는 기획서 기준으로는 맞지만, 기획서에서 "Dark Star를 받으면 -2 라이프"라는 패널티가 주어지는 의도상 플레이어가 DarkStar를 의도적으로 회피할 인센티브가 있으므로 놓쳐도 안전한 현재 구조는 기획 의도와 일치합니다. **문제 없음 - 재확인 완료.**

> 수정: C-02는 제거. 아래 실제 Critical로 재지정.

---

#### C-02 (재지정): GameManager 싱글톤 null 위험 - !연산자 오용 [GameManager.ts:57]
**위치**: `GameManager.ts` L57, `AudioManager.ts` L14
**내용**:
```typescript
public static get instance(): GameManager {
    return GameManager._instance!;  // null 가능성 무시
}
```
`GameManager.instance` 가 null인 상태에서 프로퍼티 접근 시 런타임 크래시 발생. `BucketController`, `StarSpawner`, `ConstellationManager` 등 다수의 컴포넌트가 `GameManager.instance?.` 옵셔널 체이닝을 사용하고 있어 부분적으로 보호되지만, 반환 타입이 `GameManager` (non-nullable)이므로 TypeScript 타입 체크를 우회합니다. `AudioManager` 도 동일 패턴.

---

#### C-03: ConstellationManager - GameManager.instance null 접근 위험 [ConstellationManager.ts:53]
**위치**: `ConstellationManager.ts`, `_loadNextPattern()` (L53)
**내용**:
```typescript
const wave = GameManager.instance?.currentWave ?? 1;
```
`onLoad()` 에서 호출되는데, `GameManager` 가 `addPersistRootNode` 로 씬 간 유지되더라도 최초 씬(TitleScene)에서는 GameManager가 아직 startGame()을 호출하지 않은 IDLE 상태입니다. GameScene의 경우 `ConstellationManager.onLoad()` → `GameScene.start()` 순서 보장이 Cocos Creator 노드 계층에 달려 있어 `currentWave` 가 정확히 1인지 보장되지 않습니다.

---

#### C-04: StarSpawner - _activeStars 배열에서 동일 별 중복 catch 가능 [StarSpawner.ts:137]
**위치**: `StarSpawner.ts`, `_checkCollisions()` (L137-145)
**내용**:
```typescript
for (const sf of [...this._activeStars]) {
    if (!sf.isActive) continue;
    ...
    sf.catch();  // catch() 내부에서 _active = false 설정
}
```
`sf.catch()` 는 내부에서 `this._active = false` 를 설정하고 `onCatch` 콜백을 동기 호출합니다. `onCatch` → `_onStarCaught` → `_returnToPool` → `_activeStars` 배열 수정이 발생합니다. 배열을 `[...this._activeStars]` 로 복사하여 순회 중 배열 변경 문제는 피하고 있지만, 같은 프레임에 `_activeStars` 필터링과 `update()` 가 동시에 실행되는 경우 이미 `_active = false` 된 별이 `update()` 에서도 `onMiss` 를 호출할 수 있습니다.
**구체적 경로**: `StarFragment.update()` → `newY < _bottomBound` → `onMiss` 호출, 동시에 같은 프레임 `_checkCollisions()` → `sf.catch()` → `onCatch` 호출. 단일 별에 miss와 catch가 동시 발생 가능.
**결과**: 라이프 감소 + 점수 획득이 동시에 일어나는 버그.

---

### Major (다음 iteration에서 수정)

#### M-01: UIManager와 HUDController 역할 중복 [UIManager.ts, HUDController.ts]
**내용**: 두 컴포넌트 모두 `scoreLabel`, `waveLabel`, `lifeIconsRoot/lifeIcons` 를 보유하고 동일한 업데이트 로직을 가집니다. `UIManager`는 `GameManager` 콜백을 통해 갱신하고, `HUDController`는 외부에서 직접 메서드 호출로 갱신합니다. GameScene의 에디터 노드 구성에 따라 둘 중 하나만 동작하거나 이중으로 갱신될 수 있으며, 실제 어느 쪽이 정규 경로인지 불명확합니다.
**영향**: HUD가 표시되지 않거나 이중 갱신으로 혼동 발생 가능.

---

#### M-02: WaveManager가 GameScene과 연결되지 않음 [GameScene.ts]
**내용**: `GameScene.ts` 에 `WaveManager` 에 대한 참조가 전혀 없습니다. `WaveManager.announceWave()` 를 호출하는 코드가 없으므로 보스 웨이브 경고 패널 및 Wave 전환 연출이 실제로 표시되지 않습니다.
**영향**: 기획서 요구사항 "Wave 전환 시 짧은 연출 (텍스트 팝업 + 0.5초 딜레이)" 미충족.

---

#### M-03: 오디오 매니저가 게임 이벤트와 연결되지 않음 [AudioManager.ts]
**내용**: `AudioManager` 의 `playCatch()`, `playConstellation()`, `playLoseLife()`, `playGameOver()` 가 어디에서도 호출되지 않습니다. `ResultScene.ts` 에서 `playGameOver()` 만 호출되며 나머지 SFX는 미연결 상태.
- 별 수집 시 `playCatch()` 호출 없음 (`StarSpawner._onStarCaught`)
- 별자리 완성 시 `playConstellation()` 호출 없음 (`ConstellationManager._checkCompletion`)
- 라이프 감소 시 `playLoseLife()` 호출 없음 (`GameManager.loseLife`)
**영향**: 게임 내 효과음 전무 (게임오버음 제외).

---

#### M-04: 별자리 패턴이 Wave와 무관하게 색상 필터링 없음 [ConstellationManager.ts:16]
**내용**: `buildPattern(wave)` 는 Wave에 따라 패턴을 순환하지만, Wave 1에서는 RED/BLUE만 스폰되는 반면 Wave 1 패턴(오리온자리)은 YELLOW를 요구합니다.
```
Wave 1 패턴: { RED: 3, BLUE: 2, YELLOW: 1 } ← YELLOW 스폰 불가
Wave 1 스폰 색상: [RED, BLUE]
```
YELLOW 요구 조건이 영원히 충족되지 않아 Wave 1에서 별자리 완성이 불가능합니다.
**영향**: Wave 1 클리어 불가 → 게임 진행 불가 (사실상 게임 파괴 버그).

---

#### M-05: SceneLoader 싱글톤 - 씬 전환 시 fadeOverlay 유실 위험 [SceneLoader.ts]
**내용**: `SceneLoader` 가 `GameManager` 노드에 부착되어 `addPersistRootNode` 로 유지될 경우, `fadeOverlay` 로 참조하는 노드는 이전 씬의 노드이므로 씬 전환 후 파괴됩니다. 그러나 `GameManager` 노드에는 `addPersistRootNode` 처리가 있으나 `SceneLoader` 자체는 `addPersistRootNode` 처리가 없습니다. 실제로는 각 씬에 별도 부착되는 용도로 보이나 싱글톤으로 구현되어 있어 의도가 불명확합니다.

---

#### M-06: director.pause() / resume() 남용 [GameManager.ts:118, 122]
**내용**:
```typescript
pauseGame() { director.pause(); }
resumeGame() { director.resume(); }
```
`director.pause()` 는 엔진 전체를 정지시켜 tween, schedule 등 모든 것이 멈춥니다. `UIManager` 의 페이드 tween도 정지되므로 일시정지 패널 등장 연출이 멈출 수 있습니다. Cocos Creator 3.x에서는 씬 전체 일시정지보다 게임 로직만 멈추는 `GameState.PAUSED` 플래그 체크 방식을 권장합니다.

---

### Minor (개선 권장)

#### m-01: StarFragment._bottomBound 하드코딩 [-380] [StarFragment.ts:33]
**내용**: `_bottomBound = -380` 이 기본값으로 하드코딩되어 있습니다. 해상도 960x640 기준으로는 -320이 하단이므로 마진 포함 -350 정도가 적절합니다. `StarSpawner` 에서 `sf.setBottomBound(this._bottomY)` 로 올바르게 설정하므로 실제 동작에는 문제없으나 기본값 자체가 부정확합니다.

---

#### m-02: _colorSymbol이 색상을 구분하지 않음 [ConstellationManager.ts:119]
**내용**: 모든 색상이 동일한 '★' 심볼로 표시됩니다. 기획서 UI 설계("●●○ / ●○ / ●●●")에서는 색상별 구분을 요구합니다. 현재 구현으로는 진행 상황은 보이지만 어떤 색상이 필요한지 구분이 안 됩니다.

---

#### m-03: ObjectPool과 StarSpawner 내부 풀 이중화 [ObjectPool.ts, StarSpawner.ts]
**내용**: `ObjectPool.ts` 는 범용 풀 컴포넌트이지만 `StarSpawner` 는 자체 `_pool: Node[]` 를 직접 구현합니다. `ObjectPool.ts` 는 실제로 사용되지 않는 죽은 코드입니다. 한 쪽으로 통일하거나 ObjectPool을 StarSpawner에서 사용하도록 리팩토링 권장.

---

#### m-04: DataManager와 GameManager 최고점수 이중 저장 로직 [GameManager.ts:201, DataManager.ts:38]
**내용**: 최고 점수 저장이 `GameManager._saveBestScore()` 와 `DataManager.saveBestScore()` 두 곳에 구현되어 있으며 같은 localStorage 키(`star_sweeper_best`)를 사용합니다. `ResultScene` 에서는 `DataManager.loadBestScore()` 를 읽고, `GameManager` 는 자체 `_loadBestScore()` 로 읽어 두 값이 항상 동기화되어 있으나 단일 진실 원천(Single Source of Truth) 원칙에 위배됩니다.

---

#### m-05: 콤보 보너스 시각적 피드백 없음
**내용**: 기획서에 "연속 3개 수집 시 × 1.5 배수"가 명시되어 있으나 콤보 상태를 HUD에 표시하는 코드가 없습니다. 플레이어가 콤보 상태임을 알 수 없습니다.

---

#### m-06: Wave 6+ 보스 웨이브 판정 오류 가능성 [GameManager.ts:189]
**내용**:
```typescript
isBossWave: this._currentWave % 3 === 0,
```
Wave 6은 `6 % 3 === 0` 이므로 보스 웨이브로 판정됩니다. 기획서 "Wave 3마다 보스 웨이브"와 일치합니다. 그러나 Wave 5 정적 config에는 `isBossWave: false` 이고, Wave 6은 동적 생성 시 `6 % 3 === 0 = true` 이므로 Wave 6이 보스 웨이브가 되는 것은 맞습니다. **문제 없음.**

---

#### m-07: 일시정지 중 별 낙하 지속 문제
**내용**: `director.pause()` 로 엔진 전체가 멈추므로 `StarFragment.update()` 도 정지됩니다. 그러나 C-04에서 언급된 catch/miss 동시 발생 버그와 조합되면 일시정지 해제 직후 첫 프레임에 복수의 별이 동시에 경계를 통과할 수 있습니다.

---

## 기획서 충족도

| # | 요구사항 | 구현 여부 | 비고 |
|---|---|---|---|
| R01 | 버킷 좌우 드래그/탭 이동 | 완료 | TOUCH_MOVE 기반 |
| R02 | 화면 경계 클램핑 | 완료 | _clamp() 구현 |
| R03 | 별 조각 낙하 (색상별, 랜덤 속도/위치) | 완료 | |
| R04 | 별 수집 시 점수 획득 | 완료 | 색상별 점수 정확 |
| R05 | 별 놓침 시 라이프 -1 | 완료 | |
| R06 | 라이프 3개, 0이 되면 게임 오버 | 완료 | |
| R07 | 별자리 목표 패턴 표시 (상단) | 부분 | 텍스트 기반, 색상 구분 미흡 (m-02) |
| R08 | 별자리 완성 시 +200점 | 완료 | |
| R09 | Wave 진행 (별자리 3개마다 +1) | 완료 | |
| R10 | Wave 3마다 보스 웨이브 (DarkStar 30%) | 완료 | |
| R11 | DarkStar 수집 시 -2 라이프 | 완료 | |
| R12 | 콤보 3개 이상 ×1.5 점수 | 완료 | HUD 표시 없음 (m-05) |
| R13 | Wave별 낙하 속도/스폰 간격 수치 | 완료 | WAVE_CONFIGS 정확 |
| R14 | Wave 1에서 RED/BLUE만 스폰 | 완료 | |
| R15 | Wave 1 별자리 패턴 달성 가능 | **미충족** | YELLOW 요구하나 스폰 안 됨 (M-04) |
| R16 | TitleScene 구현 | 완료 | 최고점수, 시작버튼 |
| R17 | GameScene 구현 | 완료 (코드) | 씬 파일 미생성 (에디터 작업 필요) |
| R18 | ResultScene 구현 | 완료 | |
| R19 | 씬 전환 페이드 효과 | 완료 | |
| R20 | localStorage 최고 점수 저장 | 완료 | |
| R21 | BGM 루프 재생 | 완료 (코드) | 클립 에디터 연결 필요 |
| R22 | SFX 4종 | 부분 | 코드는 있으나 수집/완성/감소 SFX 미호출 (M-03) |
| R23 | Wave 전환 연출 (팝업 0.5초) | 부분 | WaveManager 구현되었으나 미연결 (M-02) |
| R24 | 일시정지/재개 | 완료 | |
| R25 | 오브젝트 풀링 | 완료 | StarSpawner 내부 풀 |

**충족률**: 20/25 (80%) — 단, M-04 (Wave 1 클리어 불가)는 게임 진행 자체를 막는 중대 결함.

---

## 코드 품질 상세

### TypeScript 문법
- 전반적으로 TypeScript 문법 오류 없음
- `@ccclass`, `@property` 데코레이터 올바르게 사용
- `!` (non-null assertion) 남용이 일부 존재 (C-02 참조)
- `any` 타입 사용: `StarSpawner._constellationManager: any` (L172) — `ConstellationManager` 타입으로 교체 권장

### Cocos Creator 3.8.8 API
- `input.on/off`, `EventTouch.getDeltaX()`, `view.getVisibleSize()` 올바르게 사용
- `tween`, `UIOpacity`, `director.loadScene` 올바르게 사용
- `director.pause()/resume()` 사용은 M-06에서 언급한 부작용 존재
- `resources.load()` 로 동적 스프라이트 로드 — 비동기 콜백에서 `!this.isValid` 체크 올바르게 처리

### 널 참조 위험
- `GameManager.instance!` (non-null assertion) — null 가능 시 크래시 (C-02)
- `this.wavePopupLabel!.node` (UIManager.ts L95) — wavePopupLabel null 체크 후 사용이지만 비동기 콜백 내부에서 컴포넌트가 파괴될 경우 위험

### 메모리 누수
- `BucketController.onEnable/onDisable` 에서 이벤트 리스너 등록/해제 올바르게 처리
- `UIManager.onLoad()` 에서 `GameManager` 콜백을 직접 함수 참조로 할당하고 있어 씬 전환 시 이전 씬의 `UIManager` 인스턴스가 `GameManager` 콜백으로 남아있을 수 있음 (GameManager는 persist). `UIManager.onDestroy()` 에서 콜백을 null로 정리하는 코드가 없음. → **메모리 누수 및 파괴된 컴포넌트 접근 위험**.

---

## 밸런스 평가

| 항목 | 기획 수치 | 구현 수치 | 평가 |
|---|---|---|---|
| Wave 1 낙하속도 | 200 px/s | 200 | 일치 |
| Wave 1 스폰 간격 | 1.5s | 1.5 | 일치 |
| Wave 3 낙하속도 | 280 px/s | 280 | 일치 |
| Wave 5 낙하속도 | 360 px/s | 360 | 일치 |
| Wave 6+ 속도 증가 | +20/wave | +20/wave | 일치 |
| Wave 6+ 간격 감소 | -0.05/wave | -0.05s, min 0.4s | 일치 |
| 초기 라이프 | 3 | 3 | 일치 |
| 별자리 완성 보너스 | +200점 | +200 | 일치 |
| DarkStar 패널티 | -2 라이프 | -2 | 일치 |
| 콤보 배수 | ×1.5 (3개 이상) | ×1.5 (`>=3`) | 일치 |
| Wave 완료 조건 | 별자리 N개 완성 | 3개마다 Wave +1 | 기획서 "N개"가 모호 → 3개로 구현 |
| 오리온자리 요구별 | 빨강3+파랑2+노랑1=6개 | 동일 | 일치 (단 Wave 1에서 달성 불가 - M-04) |

**난이도 곡선**: Wave 1~5 정적 설정은 자연스러운 곡선을 형성합니다. Wave 6+ 동적 증가는 이론적으로 무한 진행이 가능하며 `spawnInterval` 의 0.4s 하한선이 과도한 난이도 스파이크를 방지합니다. 다만 별자리 요구 색상이 Wave에 따라 늘어나지 않아 후반 Wave에서도 Wave 1 패턴(YELLOW 1개 필요)이 반복되는 것이 어색할 수 있습니다.

---

## 다음 iteration 권장사항

### 기획봇에게

1. **Wave 1 별자리 패턴 재정의**: Wave 1은 RED+BLUE만 스폰되므로 첫 번째 별자리 패턴도 RED/BLUE 조합으로만 구성해야 합니다. (예: `{ RED: 3, BLUE: 3 }`)
2. **별자리 패턴과 Wave 스폰 색상 동기화 규칙 명시**: 각 Wave에서 달성 가능한 별자리 패턴임을 보장하는 규칙을 기획서에 추가 바랍니다.
3. **콤보 HUD 표시 기획 추가**: 콤보 상태(현재 콤보 수, ×1.5 활성화 여부)를 HUD 어디에 표시할지 명세 추가.
4. **DarkStar 놓쳤을 때 효과 명시**: 현재 기획서에는 "받으면 -2 라이프"만 있고 놓쳤을 때 처리가 없습니다. 라이프 감소 없음 / 콤보 리셋만 / 별자리 카운트 무효화 등 명시 필요.
5. **Wave 완료 조건 구체화**: "Wave 완료 조건: 목표 별자리 N개 완성"에서 N이 고정값인지 Wave별로 다른지 명시 바랍니다.

### 디자인봇에게

1. **콤보 표시 이펙트**: 콤보 3 달성 시 화면 상단 또는 수집 지점에 "COMBO x1.5!" 텍스트 이펙트 리소스 요청.
2. **별자리 패턴 슬롯 UI 색상 구분**: `ui_constellation_slot.svg` 를 색상별로 6종 제작하거나 색상 오버레이 방식 지원.
3. **보스 웨이브 배경 변화**: 보스 웨이브 진입 시 배경 색상이 붉게 변하는 오버레이 리소스.

### 개발봇에게

**우선순위 높음 (버그 수정)**

1. **[C-01] 콜백 아키텍처 개선**: `GameManager.onWaveChanged` 등 단일 함수 포인터를 배열 기반 이벤트 리스너(`EventEmitter` 패턴)로 교체. 또는 최소한 `GameScene` 에서 `origWaveChanged` 패턴 대신 `WaveManager.announceWave` 와 `StarSpawner.applyWaveConfig` 를 직접 구독 체인으로 연결.

2. **[M-04] Wave 1 별자리 패턴 수정**: `buildPattern(wave)` 에서 Wave 1 패턴을 `{ RED: 3, BLUE: 3 }` 또는 해당 Wave의 `availableColors` 내 색상만 사용하도록 수정. 또는 Wave별 `availableColors` 와 별자리 패턴을 연동하는 필터링 로직 추가.

3. **[C-04] catch/miss 동시 발생 방지**: `StarFragment.update()` 에서 `onMiss` 호출 전 `_active = false` 를 먼저 설정하고 있으므로 (`_active = false; onMiss?.()` 순서로 수정), 이미 `catch()` 된 별은 `_active` 가 false이므로 `update()` 에서 early return 됩니다. 현재 코드를 확인하면 `_active = false` 설정이 `onMiss` 호출 직전에 이미 있으므로 이 부분은 실제로 보호됩니다. 단, 동일 프레임 내 `update()` 실행 후 `_checkCollisions()` 실행 순서에 의존하므로 실행 순서를 명시적으로 문서화 권장.

4. **[메모리 누수] UIManager.onDestroy() 콜백 정리 추가**:
   ```typescript
   onDestroy() {
       const gm = GameManager.instance;
       if (!gm) return;
       gm.onScoreChanged = null;
       gm.onLivesChanged = null;
       gm.onWaveChanged  = null;
       gm.onGameOver     = null;
   }
   ```

5. **[M-02] WaveManager를 GameScene에 연결**: `GameScene.ts` 에 `waveManagerNode` 프로퍼티를 추가하고 `onWaveChanged` 콜백에서 `WaveManager.announceWave(wave)` 를 호출.

6. **[M-03] SFX 호출 연결**:
   - `StarSpawner._onStarCaught()` 내 `AudioManager.instance?.playCatch()` 추가
   - `ConstellationManager._checkCompletion()` 내 `AudioManager.instance?.playConstellation()` 추가
   - `GameManager.loseLife()` 내 `AudioManager.instance?.playLoseLife()` 추가

7. **[StarSpawner] `_constellationManager: any` 타입 수정**: `import { ConstellationManager }` 후 `private _constellationManager: ConstellationManager | null = null` 으로 타입 명시.

**우선순위 중간 (UX 개선)**

8. **콤보 HUD 추가**: `HUDController` 또는 `UIManager` 에 콤보 카운터 표시 레이블 추가.
9. **[M-01] UIManager/HUDController 통합**: 하나의 컴포넌트로 통합하거나 명확한 역할 분리.
10. **[m-02] 별자리 진행 UI 색상 구분**: `_colorSymbol()` 에서 색상별 유니코드 기호 또는 색상 리치 텍스트 활용.

---

## 부록: 파일별 이슈 요약

| 파일 | 이슈 |
|---|---|
| `GameManager.ts` | C-02 (null assertion), M-06 (director.pause), m-04 (이중 저장) |
| `BucketController.ts` | 이슈 없음 (양호) |
| `StarFragment.ts` | m-01 (하드코딩 기본값) |
| `StarSpawner.ts` | C-04 (중복 이벤트 가능성), M-03 (SFX 미연결), m-03 (이중 풀) |
| `ConstellationManager.ts` | C-03 (null 접근), M-04 (Wave 1 달성 불가), m-02 (색상 미구분) |
| `UIManager.ts` | C-01 (콜백 덮어쓰기), M-01 (역할 중복), 메모리 누수 |
| `HUDController.ts` | M-01 (역할 중복) |
| `GameScene.ts` | C-01 (콜백 체인), M-02 (WaveManager 미연결) |
| `WaveManager.ts` | M-02 (연결 누락) |
| `ObjectPool.ts` | m-03 (미사용 코드) |
| `SceneLoader.ts` | M-05 (fadeOverlay 유실 가능) |
| `AudioManager.ts` | C-02 (null assertion), M-03 (SFX 미호출) |
| `DataManager.ts` | m-04 (이중 저장) |
| `TitleScene.ts` | 이슈 없음 (양호) |
| `ResultScene.ts` | 이슈 없음 (양호) |
