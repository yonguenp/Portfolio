# QA 리포트 v3 (iter1)
**게임**: Star Sweeper
**분석일**: 2026-03-26
**분석 대상**: TypeScript 스크립트 15개
**기준 문서**: spec_v2.md, dev_notes_v2.md
**이전 리포트**: report_v2.md (8.2/10)

---

## 종합 평가

**점수: 9.0 / 10**

iter1에서 Major 이슈 4건 전부 수정 완료되어 점수가 8.2 → 9.0으로 상승했습니다. Wave 색상 동기화(M-01)와 SFX 연결(M-02)이 코드 상 정확히 구현되었고, HUDController/UIManager 역할 분리(M-03)와 GameState.PAUSED 플래그 방식 일시정지(M-04) 모두 기획 의도에 맞게 반영되었습니다. 신규 기능 콤보 HUD 팝업(NEW-01)도 tween 기반으로 올바르게 구현되었습니다. 잔여 이슈는 Major 0건, Minor 5건이며 그중 일부는 에디터 수동 연결에 의존합니다.

---

## 이전 이슈 수정 확인

### M-01 Wave 색상 불일치 — ConstellationManager + GameManager

| 항목 | 수정 여부 | 확인 근거 |
|---|---|---|
| Wave 2 큰곰자리 GREEN → YELLOW 교체 | **수정 완료** | `ConstellationManager.ts` `buildPattern()` L22: `{ BLUE:2, YELLOW:2, RED:1 }` — GREEN 없음 |
| Wave 3 카시오페이아 PURPLE → GREEN 교체 | **수정 완료** | `ConstellationManager.ts` L23: `{ GREEN:2, RED:2, BLUE:1 }` — PURPLE 없음 |
| Wave 4 사자자리 6개로 수정 | **수정 완료** | L24: `{ GREEN:2, YELLOW:2, RED:2 }` (totalStars:6) |
| Wave 5 전갈자리 6개로 수정 | **수정 완료** | L25: `{ PURPLE:2, GREEN:2, RED:2 }` (totalStars:6) |
| Wave 6 황소자리 신규 추가 | **수정 완료** | L26: `{ PURPLE:2, BLUE:2, YELLOW:2, RED:1 }` (totalStars:7) |
| GameManager WAVE_CONFIGS Wave 2 GREEN 제거 | **수정 완료** | `GameManager.ts` L47: `[RED, BLUE, YELLOW]` |
| GameManager WAVE_CONFIGS Wave 3 PURPLE 제거 | **수정 완료** | L48: `[RED, BLUE, YELLOW, GREEN]` |
| GameManager WAVE_CONFIGS Wave 6 신규 추가 | **수정 완료** | L51: 보스 웨이브 5색 전체 |

### M-02 SFX 미연결 — AudioManager 호출 지점

| SFX 이벤트 | 메서드 | 수정 여부 | 확인 근거 |
|---|---|---|---|
| 일반 별 수집 | `playCatch()` | **수정 완료** | `StarSpawner.ts` L170: isDark false 분기에서 호출 |
| Dark Star 수집 | `playDarkCatch()` | **수정 완료** | `StarSpawner.ts` L165: isDark true 분기에서 호출 |
| 별자리 완성 | `playConstellation()` | **수정 완료** | `ConstellationManager.ts` L92: 완성 판정 직후 호출 |
| 라이프 감소 | `playLoseLife()` | **수정 완료** | `GameManager.ts` L186: `loseLife()` 차감 직후 호출 |
| Wave 클리어 | `playWaveClear()` | **수정 완료** | `GameManager.ts` L221: `_advanceWave()` 내 호출 |
| 콤보 활성 | `playCombo()` | **수정 완료** | `GameManager.ts` L197: `_comboCount === 3` 달성 시 호출 |
| AudioManager 신규 메서드 | `playDarkCatch`, `playWaveClear`, `playCombo` | **수정 완료** | `AudioManager.ts` L99~104: 7종 전부 구현 |

### M-03 HUD 역할 중복 — UIManager/HUDController 분리

| 항목 | 수정 여부 | 확인 근거 |
|---|---|---|
| UIManager `scoreLabel` 프로퍼티 제거 | **수정 완료** | `UIManager.ts` 전체 — scoreLabel/waveLabel/lifeIconsRoot 없음 |
| UIManager `_updateScore()`, `_updateLives()` 메서드 제거 | **수정 완료** | UIManager는 fadeIn/Out, wavePopup, gameOver 전환만 잔류 |
| HUDController 단독 HUD 콜백 등록 | **수정 완료** | `HUDController.ts` L47~50: onScoreChanged, onLivesChanged, addWaveChangedListener, onComboActivated 직접 등록 |
| HUDController onDestroy 정리 | **수정 완료** | L56~59: 콜백 참조 비교 후 null 처리 및 removeWaveChangedListener 해제 |

### M-04 director.pause() 부작용 — GameState.PAUSED 플래그 방식

| 항목 | 수정 여부 | 확인 근거 |
|---|---|---|
| GameManager `pauseGame()`에서 `director.pause()` 제거 | **수정 완료** | `GameManager.ts` L153~158: `this._state = GameState.PAUSED` 만 설정 |
| `resumeGame()`에서 `director.resume()` 제거 | **수정 완료** | L161~163: `this._state = GameState.PLAYING` 만 설정 |
| StarSpawner `update()` PAUSED 체크 | **수정 완료** | `StarSpawner.ts` L96: `state !== GameState.PLAYING` 체크 |
| StarFragment `update()` PAUSED 체크 신규 추가 | **수정 완료** | `StarFragment.ts` L69: `state !== GameState.PLAYING` 체크 |
| UI tween(UIManager, WaveManager) 비정지 보장 | **수정 완료** | 정책적으로 director.pause() 미사용이므로 tween 계속 동작 |

### 이전 수정 유지 확인 (report_v2 기준)

| ID | 내용 | 유지 여부 |
|---|---|---|
| C-NEW-01 (v1_final) | UIManager 콜백 onDestroy 정리 | 유지 — UIManager.ts onDestroy에서 waveChangedListener, onGameOver 해제 확인 |
| C-REMAIN-02 (v1_final) | GameManager.instance null assertion 제거 | 유지 — L66~70 null 반환 방식 유지 |
| C-01 (v1) | onWaveChanged 배열 기반 다중 구독 | 유지 — `_onWaveChangedListeners` 배열 정상 운용 |

---

## 신규 이슈

### Critical (즉시 수정 필요)

**없음** — Critical 0건 유지

### Major (다음 iteration 필수)

**없음** — iter1에서 전건 해소

### Minor (개선 권장)

| ID | 위치 | 내용 | 영향도 |
|---|---|---|---|
| m-01 | `ConstellationManager.ts` L127~130 | `_colorSymbol()` 모든 색상이 동일한 '★' 반환. 색상별 진행 현황 시각 구분 불가. spec_v2.md에서 색상별 슬롯 SVG 5종을 요구하나 코드 미반영 | 플레이어 UX 저하 (색상 구분 인지 불가) |
| m-02 | `ObjectPool.ts` | 파일 존재하나 전혀 사용되지 않음. `StarSpawner`가 자체 인라인 풀 운용 중. dev_notes_v2에서도 v3 정리 권장으로 미뤄짐 | 코드 혼란, 빌드 포함 여부 불필요 |
| m-03 | `GameManager.ts` L249~254 + `DataManager.ts` | 최고 점수 이중 저장 잔존. `GameManager._saveBestScore()`와 `DataManager.saveBestScore()` 동시 존재. 동일 키(`star_sweeper_best`) 사용. `ResultScene.ts`는 `DataManager.loadBestScore()`로 읽으나 `GameManager`도 동일 키에 쓰기 | 기능 충돌 없으나 코드 명확성 저하 |
| m-04 | `HUDController.ts` `showComboEffect()` L118~130 | tween 중복 실행 방어 로직 없음. 콤보가 4번 연속(3→4→... 달성 시 매번 `_comboCount === 3`을 통과하지 않으므로 실제 중복 발화는 없으나), 빠른 연속 수집으로 comboPopupNode가 이미 활성화 중일 때 `showComboEffect()`가 다시 호출되면 tween이 중첩되어 팝업이 예상보다 일찍 사라질 수 있음 | Minor UX 버그 (드문 엣지 케이스) |
| m-05 | `ConstellationManager.ts` `addStar()` L73~77 | 현재 패턴에서 요구하지 않는 색상의 별을 수집해도 `_collected`에 기록되지 않음 (조용히 무시). 기획 의도에 부합하나, 점수는 `GameManager.addScore()`를 통해 부여됨. 별자리와 무관한 수집 별은 점수만 오르고 패턴에 기여하지 않는다는 사실이 UI에 명시되지 않음 | 플레이어 혼란 가능 (Minor UX) |

---

## 신규 기능 검증 — NEW-01 콤보 팝업

| 검증 항목 | 결과 | 근거 |
|---|---|---|
| `showComboEffect()` 메서드 구현 존재 | PASS | `HUDController.ts` L108~131 |
| `comboPopupNode` @property 노드 연결 슬롯 | PASS | L28~29: `@property({ type: Node })` 선언 |
| 활성화 → 1.5초 대기 → 0.3초 페이드 아웃 → 비활성화 tween 흐름 | PASS | L118~130: tween(popup).delay(1.5).call() 내부에서 tween(opacity).to(0.3).call() 중첩 구조 |
| PAUSED 상태에서도 팝업 재생됨 (M-04 정책 준수) | PASS | UI tween이므로 director.pause() 미사용 상태에서 당연히 동작 |
| `GameManager.onComboActivated` 콜백 연결 | PASS | `GameManager.ts` L88~89, L197~198; HUDController L45, L50 |
| `playCombo()` 동시 호출 | PASS | `GameManager.ts` L197: `playCombo()` 선호출, L198: `onComboActivated?.()` 순 |
| UIOpacity 컴포넌트 null 방어 | PASS | L126: `else { popup.active = false; }` 분기로 opacity 없어도 팝업 비활성화 |

**에디터 연결 필요 사항** (코드만으로 검증 불가):
- `comboPopupNode` 슬롯에 `ui_combo_popup.svg` Sprite 노드 에디터 수동 할당 필요
- 해당 노드에 `UIOpacity` 컴포넌트 추가 필요 (없으면 opacity 페이드 없이 즉시 비활성화 fallback)

---

## 코드 품질 세부 분석

### TypeScript 문법 / import

| 파일 | 상태 | 비고 |
|---|---|---|
| ConstellationManager.ts | PASS | import 정상, StarColor/GameManager/AudioManager 의존성 정확 |
| GameManager.ts | PASS | GameState enum 신규 추가, AudioManager import 정상 |
| StarSpawner.ts | PASS | ConstellationManager import 신규 추가, 타입 `ConstellationManager \| null` 수정 완료 (m-02 이전 이슈 해소) |
| StarFragment.ts | PASS | GameState import 추가, PAUSED 체크 정상 |
| AudioManager.ts | PASS | 신규 @property 3종(sfxDarkCatch, sfxWaveClear, sfxCombo) 선언 정상 |
| HUDController.ts | PASS | UIOpacity/tween import 정상, 콜백 참조 멤버 변수 보관 및 해제 완전 |
| UIManager.ts | PASS | 중복 프로퍼티 제거 완료, 역할 명확 |
| GameScene.ts | PASS | Wave 콜백 배열 방식 사용 정상, onDestroy 해제 완전 |
| WaveManager.ts | PASS | tween 기반 연출, UIOpacity addComponent fallback 처리 있음 |
| ResultScene.ts | PASS | DataManager.loadBestScore() 사용 (DataManager 방식 참조) |
| DataManager.ts | PASS | static 메서드 방식, 인스턴스 의존성 없음 |
| BucketController.ts | 미변경 | v2 기준 정상 유지 |
| ObjectPool.ts | 미사용 | 코드 자체 문법 오류 없으나 미사용 파일 |
| TitleScene.ts | 미변경 | v2 기준 정상 유지 |
| SceneLoader.ts | 미변경 | v2 기준 정상 유지 |

### null 체크 / 메모리 누수

| 항목 | 상태 |
|---|---|
| HUDController 콜백 onDestroy 해제 | 완전 — 참조 비교 후 null 처리, Wave 리스너 배열 해제 |
| UIManager 콜백 onDestroy 해제 | 완전 — Wave 리스너 해제, onGameOver null 처리 |
| GameScene Wave 콜백 onDestroy 해제 | 완전 — removeWaveChangedListener 호출 |
| AudioManager.instance?. 옵셔널 체이닝 | 완전 — 모든 SFX 호출 지점에서 null-safe |
| GameManager.instance?. 옵셔널 체이닝 | 완전 — 대부분의 접근 지점에서 사용 |
| StarFragment.reset() 콜백 null 처리 | 완전 |
| HUDController showComboEffect() tween 중첩 방어 | 부분 — 이미 활성 상태에서 재호출 시 tween 중첩 가능 (m-04) |

### Cocos Creator 3.8.8 API 적합성

| API | 사용처 | 상태 |
|---|---|---|
| `tween(node).delay().call().start()` | UIManager, WaveManager, HUDController | 올바름 |
| `tween(opacity).to(duration, {opacity}).call().start()` | HUDController showComboEffect | 올바름 |
| `director.addPersistRootNode()` | GameManager, AudioManager | 올바름 |
| `resources.load('path/spriteFrame', SpriteFrame, cb)` | StarFragment, HUDController | 올바름 |
| `input.on(Input.EventType.TOUCH_MOVE)` | BucketController | 올바름 |
| `sys.localStorage.getItem/setItem` | GameManager, DataManager | 올바름 (이중 사용 — m-03) |
| `scheduleOnce()` | ConstellationManager | 올바름 |

### Wave 진행 흐름 검증

```
Wave 진행 경로:
[별 수집] → StarSpawner._onStarCaught()
  → ConstellationManager.addStar()
    → _checkCompletion()
      → AudioManager.playConstellation()  ← M-02 연결 완료
      → GameManager.onConstellationDone() (+200점)
        → _constellationsCompleted % 3 === 0 이면 _advanceWave()
          → AudioManager.playWaveClear()  ← M-02 연결 완료
          → _fireWaveChanged()
            → HUDController.updateWave()  ← M-03 분리 완료
            → UIManager._showWavePopup()
            → GameScene 콜백: StarSpawner.applyWaveConfig() + WaveManager.announceWave()
```

Wave 색상 동기화 경로:
- `buildPattern(wave)` → `ConstellationManager.ts` 패턴 배열 (spec_v2 기준 100% 동기화)
- `getCurrentWaveConfig()` → `GameManager.ts` WAVE_CONFIGS (spec_v2 기준 100% 동기화)
- Wave 7+ 동적 생성: `getCurrentWaveConfig()` L228~239에서 Wave 6 설정 기반으로 extraWave만큼 속도/간격 변화, 5색 유지 — 기획 부합

### Pause 로직 검증

```
pauseGame():
  this._state = GameState.PAUSED  (director.pause() 제거)
  → StarSpawner.update(): state !== PLAYING → return (스폰/충돌 정지)
  → StarFragment.update(): state !== PLAYING → return (낙하/이탈 판정 정지)
  → UIManager tween: director.pause() 미사용이므로 계속 동작 (정책 준수)
  → WaveManager tween: 계속 동작
  → HUDController showComboEffect tween: 계속 동작

resumeGame():
  this._state = GameState.PLAYING
  → 모든 update() 재개
```

**잠재적 엣지 케이스**: PAUSED 상태에서도 `_checkCollisions()`는 `update()` 전체를 막으므로 새로운 충돌이 발생하지 않음. 단 이미 `_activeStars`에 있는 별들은 `update()` 정지로 위치 이동이 멈추므로 재개 후 즉시 화면 밖 이탈 없음 — 정상.

---

## 기획 충족도

| # | 요구사항 | 구현 여부 | 비고 |
|---|---|---|---|
| R01 | 버킷 좌우 드래그/탭 이동 | 완료 | 유지 |
| R02 | 화면 경계 클램핑 | 완료 | 유지 |
| R03 | 별 조각 낙하 (색상별, 랜덤 속도/위치) | 완료 | 유지 |
| R04 | 별 수집 시 점수 획득 | 완료 | 유지 |
| R05 | 별 놓침 시 라이프 -1 (DarkStar 놓침 제외) | 완료 | spec_v2에 DarkStar 놓침 라이프 미감소 명문화 |
| R06 | 라이프 3개, 0이 되면 게임 오버 | 완료 | 유지 |
| R07 | 별자리 목표 패턴 표시 | 부분 | 텍스트 라벨 기반. 색상별 아이콘 슬롯 미구현 (m-01) |
| R08 | 별자리 완성 시 +200점 | 완료 | 유지 |
| R09 | Wave 진행 (별자리 3개마다 +1) | 완료 | 유지 |
| R10 | Wave 3마다 보스 웨이브 (DarkStar 30%) | 완료 | 유지 |
| R11 | DarkStar 수집 시 -2 라이프 | 완료 | 유지 |
| R12 | 콤보 3개 이상 ×1.5 점수 + HUD 표시 | **완료** | v3 신규 달성 — NEW-01 showComboEffect() 구현 |
| R13 | Wave별 낙하 속도/스폰 간격 수치 | 완료 | 유지 |
| R14 | Wave 1 RED/BLUE만 스폰 | 완료 | 유지 |
| R15 | Wave 1 별자리 패턴 달성 가능 | 완료 | 유지 |
| R16 | Wave 2~3 별자리 패턴 달성 가능 | **완료** | v3 신규 달성 — M-01 수정으로 해소 |
| R17 | TitleScene 구현 | 완료 | 유지 |
| R18 | GameScene 구현 | 완료 | 유지 |
| R19 | ResultScene 구현 | 완료 | 유지 |
| R20 | 씬 전환 페이드 효과 | 완료 | 유지 |
| R21 | localStorage 최고 점수 저장 | 완료 | DataManager + GameManager 이중 (m-03 잔존) |
| R22 | BGM 루프 재생 | 완료(코드) | 에디터 클립 연결 필요 |
| R23 | SFX 7종 | **완료(코드)** | v3 신규 달성 — M-02 수정으로 7종 전부 호출 연결 (에디터 클립 연결 3종 잔존) |
| R24 | Wave 전환 연출 (팝업) | 완료 | 유지 |
| R25 | 일시정지/재개 (UI tween 비정지) | **완료** | v3 신규 달성 — M-04 수정으로 부작용 해소 |
| R26 | 오브젝트 풀링 | 완료 | 유지 |
| R27 | SVG 리소스 | 완료 | 유지 |

**기획 충족률**: 25/27 (92.6%) — v2 대비 +11.1%p (+4건 해소: R12, R16, R23, R25)

---

## 다음 iteration 권장사항

### 기획봇에게

1. **별자리 패턴 불일치 색상 수집 시 플레이어 피드백 명세 추가**
   - 현재 코드: 패턴에서 요구하지 않는 색상의 별을 수집해도 점수는 오르지만 별자리에 기여 없음 (조용히 무시)
   - 기획서에 "패턴 외 색상 수집 시 점수만 부여, 별자리 무기여" 원칙을 명문화하거나, 시각적 피드백(예: 별자리 슬롯 흔들림 없음)을 UI 설계에 추가 권장

2. **콤보 재활성 조건 명문화**
   - spec_v2: "콤보 3개 이상 달성 시 활성" — 현재 코드는 `_comboCount === 3` 순간에만 팝업 발화
   - 콤보가 이미 3 이상인 상태에서 추가 수집 시 팝업 재표시 여부 불명확. 명세 추가 권장

3. **Wave 7+ 별자리 패턴 무작위 생성 규칙 상세화**
   - spec_v2: "Wave 번호에서 사용 가능한 색상 내 무작위 생성 (6~7개)" — 현재 코드는 `buildPattern(wave)`에서 `(wave - 1) % 6` 순환 방식 사용 (무작위 생성 미구현)
   - 순환 방식이 의도인지 무작위 생성이 의도인지 확인 및 명세 확정 필요

### 디자인봇에게

1. **별자리 슬롯 색상 구분 버전 SVG 5종 제작 우선 처리**
   - `ui_constellation_slot_red.svg` ~ `ui_constellation_slot_purple.svg`
   - spec_v2.md에 명세 존재하나 `ConstellationManager._colorSymbol()`이 모든 색상 '★' 동일 반환 — 코드와 리소스 모두 미완
   - v4에서 개발봇이 색상별 슬롯 Sprite로 교체할 수 있도록 리소스 선제공 권장

2. **`ui_combo_popup.svg` 제작 확인**
   - spec_v2에 명세 추가됨. 에디터에서 `comboPopupNode`에 연결 필요 — 리소스 미제공 시 팝업 노드에 아무것도 표시되지 않음

### 개발봇에게

1. **[Minor - 우선순위 1] `ConstellationManager._colorSymbol()` 색상별 기호/아이콘 교체**
   - 현재 모든 색상이 동일한 '★' 반환. 색상별 텍스트 기호(예: RED→'♥', BLUE→'♦') 또는 디자인봇이 제공하는 슬롯 Sprite로 교체
   - `_updateUI()` 텍스트 라벨 방식 → 색상별 Sprite 슬롯 노드 방식으로 리팩터링 권장

2. **[Minor - 우선순위 2] `HUDController.showComboEffect()` tween 중첩 방어 추가**
   - `showComboEffect()` 진입 시 `tween(popup).stop()` 또는 이미 활성화 중이면 early return 처리
   - 예시: `if (popup.active) { tween(popup).stop(); }` 후 재실행

3. **[Minor - 우선순위 3] `DataManager` 최고 점수 저장 일원화**
   - `GameManager._saveBestScore()` 제거, `triggerGameOver()` 내에서 `DataManager.saveBestScore(this._score)` 단독 호출로 교체
   - spec_v2.md M-04 권장사항 반영

4. **[Minor - 우선순위 4] `ObjectPool.ts` 정리**
   - 미사용 파일 제거 또는 `StarSpawner` 내부 인라인 풀을 `ObjectPool.ts`로 교체하여 활용

5. **[Minor - 우선순위 5] Wave 7+ `buildPattern()` 무작위 생성 구현**
   - 기획 확정 후 `(wave - 1) % patterns.length` 순환 방식을 Wave 7 이상 무작위 패턴 생성 방식으로 교체
   - 사용 가능 색상은 `GameManager.getCurrentWaveConfig().availableColors`에서 가져올 것

---

## 종합 점수 요약

| 항목 | 점수 | v2 대비 |
|---|---|---|
| 종합 평가 | **9.0 / 10** | +0.8 |
| 씬 구성 완성도 | **9.0 / 10** | +0.5 |
| 오디오 시스템 | **9.0 / 10** | +3.0 (SFX 7종 연결 완료) |
| 조작감 (코드 분석) | **8.5 / 10** | 동일 |
| 코드 품질 | **9.0 / 10** | +0.8 (Major 4건 수정, 역할 분리 완성) |
| 기획 충족도 | **92.6%** | +11.1%p |
| Critical 잔여 | **0건** | 동일 |
| Major 잔여 | **0건** | -4건 |
| Minor 잔여 | **5건** | 동일 (m-02 타입 수정으로 1건 감소, 신규 m-04/m-05 추가) |
