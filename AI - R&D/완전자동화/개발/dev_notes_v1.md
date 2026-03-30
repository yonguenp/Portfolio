# 개발 노트 v1
**게임**: Star Sweeper
**Iteration**: 0
**작성일**: 2026-03-25
**개발자**: 개발봇 (AI 자동화 파이프라인)

---

## 1. 구현된 파일 목록

| 파일명 | 경로 | 상태 |
|---|---|---|
| `GameManager.ts` | `assets/scripts/GameManager.ts` | 완료 |
| `BucketController.ts` | `assets/scripts/BucketController.ts` | 완료 |
| `StarFragment.ts` | `assets/scripts/StarFragment.ts` | 완료 |
| `StarSpawner.ts` | `assets/scripts/StarSpawner.ts` | 완료 |
| `ConstellationManager.ts` | `assets/scripts/ConstellationManager.ts` | 완료 |
| `UIManager.ts` | `assets/scripts/UIManager.ts` | 완료 |
| `HUDController.ts` | `assets/scripts/HUDController.ts` | 완료 |
| `GameScene.ts` | `assets/scripts/GameScene.ts` | 완료 |
| `WaveManager.ts` | `assets/scripts/WaveManager.ts` | 완료 |
| `ObjectPool.ts` | `assets/scripts/ObjectPool.ts` | 완료 |
| `SceneLoader.ts` | `assets/scripts/SceneLoader.ts` | 완료 |
| `AudioManager.ts` | `assets/scripts/AudioManager.ts` | 완료 |
| `DataManager.ts` | `assets/scripts/DataManager.ts` | 완료 |
| `TitleScene.ts` | `assets/scripts/TitleScene.ts` | 완료 |
| `ResultScene.ts` | `assets/scripts/ResultScene.ts` | 완료 |

**총 15개 TypeScript 스크립트 생성 완료**

---

## 2. 주요 게임 로직 설명

### 2.1 싱글톤 패턴 (GameManager, AudioManager)
- `onLoad()`에서 `director.addPersistRootNode(this.node)` 적용
- 씬 전환 후에도 인스턴스 유지
- 이미 인스턴스 존재 시 `this.node.destroy()`로 중복 방지

### 2.2 Wave 시스템
- `GameManager.getCurrentWaveConfig()` 로 Wave별 설정 반환
- Wave 1~5: WAVE_CONFIGS 정적 배열 사용
- Wave 6+: 마지막 Wave 기준에서 점진적 속도 증가 (+20 px/s), 간격 감소 (-0.05s, 최소 0.4s)
- Wave 3의 배수마다 보스 웨이브 (DarkStar 30% 확률 스폰)
- 별자리 3개 완성마다 Wave 상승

### 2.3 충돌 처리 (AABB 수동 검사)
- `BucketController.getBounds()` 로 버킷 경계 반환
- `StarSpawner.update()` 에서 매 프레임 활성 별 전수 검사
- 별 중심 ± 32px 반경 vs 버킷 AABB 비교
- Physics2D 미사용 → 퍼포먼스 예측 용이

### 2.4 오브젝트 풀링
- `StarSpawner` 내 인라인 풀 구현 (`_pool: Node[]`)
- `ObjectPool.ts` 는 범용 컴포넌트로 별도 Prefab 연결 방식도 지원
- `StarFragment.reset()` 으로 상태 초기화 후 풀 반환

### 2.5 콤보 시스템
- `GameManager._comboCount` 카운터 보유
- 별 수집 성공 시 `incrementCombo()`
- 별 놓침 / DarkStar 수집 시 `resetCombo()`
- 콤보 3 이상일 때 점수 ×1.5 배수 적용

### 2.6 별자리 완성 시스템
- `ConstellationManager`가 Wave별 패턴 정의 (5종 순환)
- `addStar(color)` 호출 시 요구 색상 충족 여부 확인
- 완성 시 `GameManager.onConstellationDone()` → +200점 + Wave 진행 판단
- 0.5초 딜레이 후 다음 패턴 로드 (Wave 연출 대기)

### 2.7 씬 전환 페이드
- `UIManager.fadeIn() / fadeOut()` : tween + UIOpacity 활용
- `TitleScene`, `ResultScene` 각각 독립적 페이드 처리
- `SceneLoader.ts` : 공통 씬 전환 유틸리티 (선택적 사용)

### 2.8 데이터 저장
- `DataManager` (static 메서드) + `GameManager._saveBestScore()` 이중 처리
- `sys.localStorage` 키: `star_sweeper_best`, `star_sweeper_settings`
- 설정 저장 지원 (BGM/SFX 볼륨)

---

## 3. MCP 활용 내역

| 작업 | MCP 툴 | 결과 |
|---|---|---|
| 스크립트 생성 시도 | `mcp__unityMCP__create_script` | **실패** - `.ts` 확장자 미지원 (`.cs`만 허용) |
| 씬 생성 | `mcp__unityMCP__manage_scene` | 미시도 (파일 직접 생성 방식 선택) |

**결론**: Cocos Creator용 MCP 서버가 Unity MCP 프로토콜을 사용하고 있어 `.ts` 파일 생성이 불가능했습니다. 모든 스크립트를 파일 직접 쓰기 방식으로 저장했습니다.

---

## 4. 알려진 제한사항

1. **씬 파일 미생성**: `.scene` 파일은 Cocos Creator 에디터에서 직접 노드 구성이 필요합니다. 스크립트만 완성된 상태입니다.
2. **오디오 클립 미연결**: `AudioManager`의 BGM/SFX 클립은 에디터에서 Inspector를 통해 직접 연결해야 합니다. (오디오 리소스는 디자인봇 산출물에 미포함)
3. **Prefab 미생성**: `StarFragment`, `DarkStar` Prefab은 에디터에서 생성 후 `StarSpawner.starFragmentPrefab` 프로퍼티에 연결 필요
4. **SVG 리소스**: `assets/resources/` 의 SVG 파일들은 디자인봇이 생성했으나 에디터 Import 확인 필요
5. **DarkStar 별도 Prefab**: 현재 구현에서는 DarkStar가 StarFragment의 `StarColor.DARK` 타입으로 처리됩니다. 별도 DarkStar.ts/Prefab 분리 없이 동일 컴포넌트 사용.

---

## 5. QA봇에게 전달할 테스트 포인트

### 필수 테스트 항목

| # | 테스트 항목 | 예상 결과 | 스크립트 |
|---|---|---|---|
| T01 | 게임 시작 시 라이프 3, 점수 0, Wave 1 초기화 | HUD에 정확히 표시 | `GameManager.startGame()` |
| T02 | 버킷 터치 드래그 - 좌우 이동 | 버킷이 드래그 방향으로 이동 | `BucketController._onTouchMove()` |
| T03 | 버킷 화면 경계 클램핑 | 버킷이 화면 밖으로 나가지 않음 | `BucketController._clamp()` |
| T04 | 별 조각 낙하 및 화면 하단 이탈 | 라이프 1 감소, 별 풀 반환 | `StarFragment.update()` |
| T05 | 버킷과 별 충돌 시 점수 획득 | 색상별 점수 적용 (RED=10 등) | `StarSpawner._checkCollisions()` |
| T06 | DarkStar 수집 시 라이프 2 감소 | 라이프 2 감소, 즉시 게임오버 가능 | `StarSpawner._onStarCaught()` |
| T07 | 콤보 3 이상 시 점수 ×1.5 | 계산 결과 1.5배 반영 | `GameManager.addScore()` |
| T08 | 별자리 패턴 완성 시 +200점 | 점수 +200 + 다음 패턴 로드 | `ConstellationManager._checkCompletion()` |
| T09 | 별자리 3개 완성 시 Wave 상승 | Wave 카운터 +1, 스폰 속도 증가 | `GameManager.onConstellationDone()` |
| T10 | Wave 3 보스 웨이브 - DarkStar 30% 등장 | 경고 팝업 표시, 검은 별 출현 | `StarSpawner._pickColor()` |
| T11 | 라이프 0 → 게임 오버 → ResultScene 전환 | 페이드 아웃 후 ResultScene 로드 | `GameManager.triggerGameOver()` |
| T12 | ResultScene 점수 표시 | 이번 점수 및 최고 점수 정확 표시 | `ResultScene.start()` |
| T13 | 최고 점수 localStorage 저장/불러오기 | 재시작 후에도 최고 점수 유지 | `DataManager.saveBestScore()` |
| T14 | 일시정지 / 재개 | 게임 멈춤/재개 정상 동작 | `GameManager.pauseGame/resumeGame()` |
| T15 | TitleScene → GameScene → ResultScene 씬 전환 | 페이드 효과 포함 정상 전환 | `SceneLoader`, `UIManager` |
| T16 | 오브젝트 풀 재사용 | 스폰 후 풀 반환 시 노드 재활용 | `StarSpawner._getFromPool()` |
| T17 | Wave 6+ 점진적 난이도 상승 | fallSpeed +20/wave, interval -0.05/wave | `GameManager.getCurrentWaveConfig()` |
| T18 | 페이드 인/아웃 연출 | 씬 전환 시 검은 화면 전환 효과 | `UIManager.fadeIn/fadeOut()` |

### 회귀 테스트
- 게임 오버 후 재시작 시 모든 상태 완전 초기화 확인
- 빠른 연속 탭 시 별 중복 충돌 없는지 확인
- Wave 수십 회 반복 시 메모리 증가 없는지 확인 (오브젝트 풀)

---

## 6. 에디터 세팅 가이드 (QA봇/다음 봇 참고)

### GameScene 노드 구성
```
Canvas
├── Background (Sprite: bg_space)
├── GameManager (GameManager.ts, SceneLoader.ts)
├── AudioManager (AudioManager.ts)
├── StarSpawnerRoot (StarSpawner.ts)
│   └── [StarFragment Prefabs - pooled]
├── Bucket (BucketController.ts, Sprite: bucket)
├── ConstellationUI (ConstellationManager.ts)
│   ├── NameLabel (Label)
│   └── ProgressLabel (Label)
├── HUD (HUDController.ts)
│   ├── WaveLabel (Label)
│   ├── ScoreLabel (Label)
│   ├── LifeIcons
│   │   ├── Life1 (Sprite: icon_life)
│   │   ├── Life2 (Sprite: icon_life)
│   │   └── Life3 (Sprite: icon_life)
│   └── PauseButton (Button)
├── WaveManager (WaveManager.ts)
│   ├── WaveAnnouncementLabel (Label)
│   └── BossWarningPanel
├── PausePanel (비활성화 기본)
│   └── ResumeButton / TitleButton
├── FadeOverlay (UIOpacity: 검은 Sprite, Canvas 전체 크기)
└── UIManager (UIManager.ts)
```

### 에셋 연결 경로
- 리소스 루트: `assets/resources/`
- Prefab 폴더: `assets/prefabs/` (에디터에서 생성 필요)

---

## 7. 씬 구성 완료 내역 (씬봇 - 2026-03-25)

### 생성된 씬 파일

| 씬 파일 | 경로 | 상태 |
|---|---|---|
| `TitleScene.scene` | `assets/scenes/TitleScene.scene` | 완료 |
| `GameScene.scene` | `assets/scenes/GameScene.scene` | 완료 |
| `ResultScene.scene` | `assets/scenes/ResultScene.scene` | 완료 |

### TitleScene 노드 구성
```
Canvas (Canvas 컴포넌트)
├── TitleScriptNode (TitleScene.ts, fadeOverlay 연결됨)
├── Background (Sprite)
├── TitleLogo (Label: "Star Sweeper", 금색 64pt bold)
├── BestScoreLabel (Label: "Best: 0", 연보라 32pt)
├── StartButton (Button → onStartButtonClicked)
│   └── StartButtonLabel (Label: "게임 시작")
└── FadeOverlay (Sprite 검은색 + UIOpacity + Widget 전체화면)
```

### GameScene 노드 구성
```
Canvas (Canvas 컴포넌트)
├── GameSceneController (GameScene.ts - 모든 노드 참조 연결됨)
├── GameManager (GameManager.ts)
├── AudioManager (AudioManager.ts)
├── DataManager (DataManager.ts)
├── Background (Sprite: bg_space 연결 필요)
├── StarSpawner (StarSpawner.ts)
├── Bucket (BucketController.ts, y=-260 하단 배치)
├── ConstellationManager (ConstellationManager.ts)
│   ├── NameLabel (Label: 별자리 이름)
│   └── ProgressLabel (Label: 수집 현황)
├── WaveManager (WaveManager.ts)
│   ├── WaveAnnouncementLabel (Label, 기본 비활성)
│   └── BossWarningPanel (기본 비활성)
│       └── BossWarningLabel (Label)
├── PausePanel (기본 비활성)
│   ├── ResumeButton (Button → onResumeButtonClicked)
│   └── TitleButton (Button → onTitleButtonClicked)
├── UIManager (UIManager.ts, fadeOverlay 연결됨)
├── HUD (HUDController.ts, y=290 상단)
│   ├── WaveLabel (Label: "Wave 1", 좌측)
│   ├── ScoreLabel (Label: "0", 우측 금색)
│   └── LifeLabel (Label: "HP: 3", 좌하단)
└── FadeOverlay (Sprite 검은색 + UIOpacity + Widget 전체화면)
```

### ResultScene 노드 구성
```
Canvas (Canvas 컴포넌트)
├── ResultSceneController (ResultScene.ts - currentScoreLabel, bestScoreLabel, fadeOverlay 연결됨)
├── Background (Sprite)
├── GameOverTitle (Label: "GAME OVER", 빨간 64pt bold)
├── CurrentScoreLabel (Label: "Score: 0")
├── BestScoreLabel (Label: "Best: 0", 금색)
├── RestartButton (Button → onRestartButtonClicked)
│   └── RestartLabel (Label: "다시 시작")
├── TitleButton (Button → onTitleButtonClicked)
│   └── TitleLabel (Label: "타이틀로")
└── FadeOverlay (Sprite 검은색 + UIOpacity + Widget 전체화면)
```

### 씬 빌드 설정
- 시작 씬: `TitleScene` (`settings/default-project.json` 업데이트 완료)
- 해상도: 960×640 (기존 설정 유지)

---

## 8. MCP 활용 결과 (씬봇)

| 작업 | 결과 |
|---|---|
| `mcp__unityMCP__manage_scene` 씬 생성 시도 | **실패** - Cocos Creator 에디터 미실행 (Unity Editor 인스턴스 없음 오류) |
| 씬 파일 JSON 직접 작성 | **성공** - 3개 씬 파일 Cocos Creator 3.x 포맷으로 작성 |

**결론**: MCP UnityMCP 서버가 Cocos Creator와 연결되려면 에디터가 실행 중이어야 합니다. 이번 iteration에서도 파일 직접 쓰기 방식으로 씬을 생성했습니다.

---

## 9. Critical 버그 수정 내역 (씬봇)

| 버그 ID | 내용 | 수정 파일 | 수정 내용 |
|---|---|---|---|
| M-04 | Wave 1 별자리 패턴에 YELLOW 포함 → Wave 1 스폰 색상(RED/BLUE)과 불일치 | `ConstellationManager.ts` | 오리온자리 패턴에서 YELLOW 제거: `{ RED: 3, BLUE: 2 }` (totalStars: 5) |
| C-01 | `onWaveChanged` 단일 함수 포인터 덮어쓰기 | `GameManager.ts` | 배열 기반 `_onWaveChangedListeners` + `addWaveChangedListener()` / `removeWaveChangedListener()` 도입, `set onWaveChanged` 하위 호환 유지 |
| M-02 | `GameScene.ts`에 `WaveManager` 미연결 | `GameScene.ts` | `waveManagerNode` 프로퍼티 추가, `addWaveChangedListener`로 `announceWave` 구독 연결 |
| M-05 | 씬 전환 시 GameManager 콜백 메모리 누수 | `GameScene.ts` | `onDestroy()`에서 `removeWaveChangedListener` 호출로 콜백 해제 |

---

## 10. 플레이어블 빌드 체크리스트

### 에디터 오픈 후 필수 수동 작업

- [ ] Cocos Creator에서 세 씬 파일 import 확인 (자동 인식 예상)
- [ ] **GameScene > Background**: `bg_space` SpriteFrame 연결 (`assets/resources/bg_space.svg`)
- [ ] **GameScene > Bucket**: `bucket` SpriteFrame 연결 (`assets/resources/bucket.svg`)
- [ ] **StarFragment Prefab 생성**: 빈 노드에 StarFragment.ts 부착 후 `assets/prefabs/` 에 저장
- [ ] **GameScene > StarSpawner**: `starFragmentPrefab` 프로퍼티에 StarFragment Prefab 연결
- [ ] **AudioManager**: BGM/SFX AudioClip 연결 (현재 오디오 파일 미제공 - 추후 디자인봇 요청)
- [ ] 빌드 설정에서 씬 3개 모두 포함 확인 (Build Settings > Scenes)

### 플레이 가능 조건 확인
- [ ] TitleScene: "게임 시작" 버튼 표시, 클릭 시 GameScene 전환
- [ ] GameScene: 별 낙하, 버킷 터치 조작, 별자리 카운터 동작
- [ ] GameScene: Wave 1 별자리(오리온자리 RED 3 + BLUE 2) 달성 가능
- [ ] 라이프 0 → ResultScene 전환
- [ ] ResultScene: 점수 표시, 재시작/타이틀 버튼 동작
