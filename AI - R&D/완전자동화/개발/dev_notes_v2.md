# 개발 노트 v2 (Iteration 1)
**게임**: Star Sweeper
**작성일**: 2026-03-26
**개발봇**: AI 자동화 파이프라인 (Iteration 1)
**기준 문서**: spec_v2.md, QA/latest_report.md (8.2/10)

---

## 수정된 파일 목록

| 파일 | 수정 항목 |
|---|---|
| `ConstellationManager.ts` | M-01 Wave 색상 동기화, M-02 별자리 완성 SFX |
| `GameManager.ts` | M-01 WAVE_CONFIGS 색상 수정, M-02 loseLife/waveClear/combo SFX, M-04 director.pause() 제거, NEW-01 onComboActivated 콜백 |
| `StarSpawner.ts` | M-02 수집 SFX, M-04 PAUSED 체크 명시, m-02 타입 수정 |
| `StarFragment.ts` | M-04 update() PAUSED 체크 추가 |
| `AudioManager.ts` | M-02 sfxDarkCatch/sfxWaveClear/sfxCombo 프로퍼티 + 메서드 7종 완비 |
| `HUDController.ts` | M-03 단독 HUD 콜백 등록, NEW-01 showComboEffect() 구현 |
| `UIManager.ts` | M-03 scoreLabel/waveLabel/lifeIconsRoot 중복 프로퍼티 제거, Wave 팝업 + 페이드 전담 |

---

## 각 수정 내용 요약

### M-01: Wave 색상 동기화 (ConstellationManager + GameManager)

**ConstellationManager.ts `buildPattern()`**:
| Wave | 수정 전 | 수정 후 (spec_v2 기준) |
|---|---|---|
| 2 큰곰자리 | `{ BLUE:2, GREEN:2, YELLOW:1 }` | `{ BLUE:2, YELLOW:2, RED:1 }` (GREEN 제거) |
| 3 카시오페이아 | `{ RED:2, PURPLE:2, BLUE:1 }` | `{ GREEN:2, RED:2, BLUE:1 }` (PURPLE 제거) |
| 4 사자자리 | `{ RED:2, YELLOW:2, GREEN:2, BLUE:1 }` (7개) | `{ GREEN:2, YELLOW:2, RED:2 }` (6개) |
| 5 전갈자리 | `{ PURPLE:3, RED:2, GREEN:2 }` (7개) | `{ PURPLE:2, GREEN:2, RED:2 }` (6개) |
| 6 황소자리 | 없음 | `{ PURPLE:2, BLUE:2, YELLOW:2, RED:1 }` (7개) 신규 추가 |

**GameManager.ts `WAVE_CONFIGS`**:
| Wave | 수정 전 | 수정 후 |
|---|---|---|
| 2 | `[RED, BLUE, YELLOW, GREEN]` | `[RED, BLUE, YELLOW]` (GREEN 제거) |
| 3 | `[RED, BLUE, YELLOW, GREEN, PURPLE]` | `[RED, BLUE, YELLOW, GREEN]` (PURPLE 제거) |
| 6 | 없음 (Wave 5 반복) | `[RED, BLUE, YELLOW, GREEN, PURPLE]` 보스 웨이브 신규 추가 |

### M-02: SFX 7종 트리거 연결 완비

| 이벤트 | 메서드 | 호출 위치 |
|---|---|---|
| 일반 별 수집 | `playCatch()` | `StarSpawner._onStarCaught()` — isDark false 분기 |
| Dark Star 수집 | `playDarkCatch()` | `StarSpawner._onStarCaught()` — isDark true 분기 |
| 별자리 완성 | `playConstellation()` | `ConstellationManager._checkCompletion()` — 완성 판정 직후 |
| 라이프 감소 | `playLoseLife()` | `GameManager.loseLife()` — 차감 직후 |
| Wave 클리어 | `playWaveClear()` | `GameManager._advanceWave()` — Wave 번호 상승 직후 |
| 게임 오버 | `playGameOver()` | 기존 연결 유지 (ResultScene.ts) |
| 콤보 ×1.5 | `playCombo()` | `GameManager.incrementCombo()` — _comboCount === 3 달성 시 |

**AudioManager 신규 추가 항목**:
- `@property sfxDarkCatch: AudioClip`
- `@property sfxWaveClear: AudioClip`
- `@property sfxCombo: AudioClip`
- `playDarkCatch()`, `playWaveClear()`, `playCombo()` 메서드

### M-03: HUDController / UIManager 역할 완전 분리

**HUDController.ts**:
- `onLoad()`에서 `GameManager.onScoreChanged`, `onLivesChanged`, `addWaveChangedListener`, `onComboActivated` 직접 등록
- `onDestroy()`에서 콜백 참조 비교 후 정리 (메모리 누수 없음)
- `start()`에서 초기 HUD 값 반영

**UIManager.ts**:
- `scoreLabel`, `waveLabel`, `lifeIconsRoot` `@property` 제거
- `_updateScore()`, `_updateLives()`, `_updateWaveLabel()` 메서드 제거
- `_onScoreCb`, `_onLivesCb` 콜백 제거
- 남은 역할: Wave 팝업 tween 연출, 게임 오버 시 fadeOut + ResultScene 전환, fadeIn/fadeOut 공개 API

### M-04: director.pause() 제거 — GameState.PAUSED 플래그 방식

**GameManager.ts `pauseGame()`**:
- `director.pause()` 제거 → `this._state = GameState.PAUSED` 만 설정
- `resumeGame()`에서 `director.resume()` 제거 → `this._state = GameState.PLAYING` 만 설정

**StarSpawner.ts `update()`**:
- 기존 `state !== GameState.PLAYING` 체크 유지 (PAUSED 포함 모든 비PLAYING 상태에서 스폰/충돌 정지)
- 주석으로 "UI tween과 무관" 정책 명시

**StarFragment.ts `update()`**:
- `GameManager.instance?.state !== GameState.PLAYING` 체크 신규 추가
- PAUSED 상태에서 낙하 이동 및 화면 이탈 판정 정지
- UI tween(UIManager, WaveManager 팝업)은 이 체크와 무관하므로 계속 동작

### NEW-01: 콤보 HUD 팝업 구현

**HUDController.ts `showComboEffect()`**:
- `comboPopupNode` (@property Node) 활성화
- `UIOpacity` opacity 255 → delay(1.5) → to(0.3, { opacity:0 }) → active false 순서로 tween
- UI tween 방식이므로 PAUSED 상태에서도 재생됨 (M-04 정책 준수)
- `GameManager.onComboActivated` 콜백으로 연결 (`incrementCombo()` → `_comboCount === 3` 달성 시 발화)

**에디터 연결 필요**:
- HUD 노드에 `comboPopupNode` 슬롯에 `ui_combo_popup.svg` Sprite 노드 할당
- 노드에 `UIOpacity` 컴포넌트 추가 필수

---

## 잔여 제한사항

| ID | 내용 | 이유 |
|---|---|---|
| m-01 | `ConstellationManager._colorSymbol()` 모든 색상 동일 '★' | 이번 iteration 범위 외. Minor 이슈. v3에서 색상별 기호 또는 UI 슬롯으로 개선 권장 |
| m-03 | `ObjectPool.ts` 미사용 | 이번 iteration 범위 외. StarSpawner 내부 풀 안정 동작 중이므로 v3에서 통합 정리 권장 |
| m-04 | `GameManager._saveBestScore()` DataManager와 이중 저장 | 이번 iteration 범위 외. 기능적 충돌은 없으나 v3에서 DataManager 단독 처리로 정리 권장 |
| AudioClip 연결 | sfxDarkCatch, sfxWaveClear, sfxCombo 클립은 에디터에서 수동 연결 필요 | 실제 오디오 파일 미제공 — 클립 할당 전까지 해당 SFX 무음 처리됨 (null-safe하여 에러 없음) |
| comboPopupNode | HUD 노드의 comboPopupNode 에디터 수동 할당 필요 | 씬 파일 자동 수정 범위 외 |

---

## QA봇 테스트 포인트

### M-01 Wave 색상 검증
- [ ] Wave 2 진입 후 큰곰자리 패턴 표시 확인: BLUE×2, YELLOW×2, RED×1 (GREEN 슬롯 없음)
- [ ] Wave 2에서 GREEN 별 절대 스폰 안됨 확인
- [ ] Wave 3 카시오페이아 패턴: GREEN×2, RED×2, BLUE×1 (PURPLE 슬롯 없음)
- [ ] Wave 3에서 PURPLE 별 절대 스폰 안됨 확인
- [ ] Wave 2, 3 별자리 정상 완성 가능 여부 확인 (달성 불가 버그 해소)
- [ ] Wave 4 사자자리: GREEN×2, YELLOW×2, RED×2 (6개) 확인
- [ ] Wave 5 전갈자리: PURPLE×2, GREEN×2, RED×2 (6개) 확인
- [ ] Wave 6 황소자리: PURPLE×2, BLUE×2, YELLOW×2, RED×1 (7개) 확인

### M-02 SFX 트리거 검증
- [ ] 일반 별 수집 시 `playCatch()` 호출됨 (AudioManager 로그 또는 breakpoint)
- [ ] Dark Star 수집 시 `playDarkCatch()` 호출됨
- [ ] 별자리 완성 순간 `playConstellation()` 호출됨 (완성 판정 직후)
- [ ] 별 놓침으로 라이프 감소 시 `playLoseLife()` 호출됨
- [ ] 3개 별자리 완성 후 Wave 상승 시 `playWaveClear()` 호출됨
- [ ] 별 3개 연속 수집 시 `playCombo()` 호출됨 (3번째 수집 순간만)

### M-04 Pause 메카닉 검증
- [ ] 일시정지 시 별 낙하 이동 정지 확인 (StarFragment.update 체크)
- [ ] 일시정지 시 새 별 스폰 정지 확인 (StarSpawner.update 체크)
- [ ] 일시정지 시 AABB 충돌 판정 정지 확인
- [ ] 일시정지 중 Wave 팝업 tween 계속 재생됨 확인 (UIManager tween 비정지)
- [ ] 일시정지 중 FadeOverlay tween 계속 재생됨 확인
- [ ] 재개 후 별 낙하 / 스폰 정상 재개 확인
- [ ] PausePanel 표시/숨김 tween이 PAUSED 상태와 무관하게 동작함 확인

### M-03 HUD 역할 분리 검증
- [ ] 점수 변경 시 HUDController.scoreLabel만 갱신되고 UIManager에서 중복 갱신 없음 확인
- [ ] 라이프 변경 시 HUDController.lifeIcons만 갱신됨 확인
- [ ] Wave 변경 시 HUDController.waveLabel 갱신 + UIManager Wave 팝업 표시 (역할 분리 정상)
- [ ] UIManager에 scoreLabel/waveLabel/lifeIconsRoot 프로퍼티가 에디터에서 노출되지 않음 확인

### NEW-01 콤보 팝업 검증
- [ ] 별 3개 연속 수집 직후 comboPopupNode 활성화 확인
- [ ] 팝업 1.5초 유지 후 0.3초 페이드 아웃 확인
- [ ] 페이드 아웃 완료 후 노드 비활성화 확인
- [ ] 4번째 수집부터 팝업 재표시 없음 확인 (3 달성 시점만 발화)
- [ ] 일시정지 중에도 콤보 팝업 tween 계속 재생됨 확인

---

## 이번 Iteration 변경사항 요약

- 수정 파일: 7개 (ConstellationManager, GameManager, StarSpawner, StarFragment, AudioManager, HUDController, UIManager)
- Major 이슈 4건 전부 수정 완료 (M-01, M-02, M-03, M-04)
- 신규 기능 1건 구현 완료 (NEW-01 콤보 팝업)
- Minor 이슈 1건 수정 완료 (m-02 타입 수정)
- 잔여 Minor 3건 (m-01, m-03, m-04) → v3 권장
