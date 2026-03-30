# QA 리포트 v1_final (report_v1 업그레이드)
**게임**: Star Sweeper
**분석일**: 2026-03-25
**분석 대상**: TypeScript 스크립트 15개 + 씬 파일 3개 + SVG 리소스 12종
**기준 문서**: spec_v1.md, dev_notes_v1.md
**이전 리포트**: report_v1.md (6.5/10)
**이전 Critical 버그 수정 확인 포함**

---

## 종합 평가

**점수: 7.8 / 10**

이전 QA(report_v1)에서 발견된 Critical 4건 중 3건이 수정되었습니다. 특히 게임 진행을 막던 Wave 1 별자리 패턴 불일치(M-04), WaveManager 미연결(M-02), 콜백 덮어쓰기(C-01), 씬 전환 콜백 메모리 누수(M-05) 모두 개발봇이 수정 완료했습니다. 씬 파일 3개가 신규 생성되어 에디터 없이도 구조 검증이 가능해졌습니다. SVG 리소스 12종 전량 완성되었으며 그래픽 품질이 준수합니다. 잔여 이슈로는 UIManager 콜백 메모리 누수, SFX 미연결, UIManager/HUDController 역할 중복, 코드 일부 타입 약점이 남아 있습니다.

---

## 이전 QA Critical 버그 수정 확인

| 버그 ID | 내용 | 수정 여부 | 확인 방법 |
|---|---|---|---|
| C-01 | onWaveChanged 단일 콜백 덮어쓰기 | **수정 완료** | GameManager.ts: 배열 기반 `_onWaveChangedListeners` + `addWaveChangedListener/removeWaveChangedListener` 도입. GameScene.ts에서 배열 구독 방식 사용 확인 |
| C-02 | GameManager.instance! null assertion | **미수정** | GameManager.ts L57: 여전히 `return GameManager._instance!` — 런타임 null 접근 시 크래시 위험 잔존 |
| C-03 | ConstellationManager onLoad에서 GameManager.instance null 접근 | **부분 수정** | `GameManager.instance?.currentWave ?? 1` 옵셔널 체이닝 사용으로 null 안전하나, wave가 IDLE 상태 wave=0을 반환할 수 있는 구조는 잔존 |
| C-04 | catch/miss 동시 발생 가능 | **구조적으로 안전** | StarFragment.update()에서 `_active = false` 후 onMiss 호출하므로 catch()에서 isActive 체크로 이중 처리 방지됨. 설계상 안전 |
| M-02 | WaveManager GameScene 미연결 | **수정 완료** | GameScene.ts에 `waveManagerNode` 프로퍼티 추가, `addWaveChangedListener`로 `announceWave` 구독 확인 |
| M-04 | Wave 1 별자리 패턴에 YELLOW 포함 | **수정 완료** | ConstellationManager.ts: 오리온자리 패턴 `{ RED: 3, BLUE: 2 }` (totalStars: 5)로 수정. YELLOW 제거 확인 |
| M-05 | 씬 전환 시 GameManager 콜백 메모리 누수 | **수정 완료** | GameScene.ts: `onDestroy()`에서 `removeWaveChangedListener` 호출 확인 |

**이전 Critical 수정률: 5/7 (71%)**

---

## 씬 구성 완성도

### 씬 파일 존재 여부
| 씬 | 파일 존재 | 포맷 검증 |
|---|---|---|
| TitleScene.scene | 존재 | cc.SceneAsset + cc.Scene + Canvas 구조 확인, Cocos Creator 3.x JSON 형식 일치 |
| GameScene.scene | 존재 | cc.SceneAsset + cc.Scene + Canvas 구조 확인, 11개 자식 노드 배치 확인 |
| ResultScene.scene | 존재 | cc.SceneAsset + cc.Scene + Canvas 구조 확인, 7개 자식 노드 배치 확인 |

### TitleScene 노드 구성 검증
- Canvas: 존재, 5개 자식 노드 (TitleScriptNode, Background, TitleLogo, BestScoreLabel, StartButton/FadeOverlay 포함)
- TitleScene.ts 스크립트 컴포넌트 연결: 씬 JSON에 TitleScriptNode 참조 구조 확인
- FadeOverlay 노드: 구성 문서(dev_notes)에 명시, 씬 JSON 5번째 자식으로 배치
- 판정: **완성도 높음** (에디터 연결값 확인은 에디터 오픈 후 필요)

### GameScene 노드 구성 검증
- Canvas: 존재, 11개 자식 노드
- 필수 노드 목록 대조:

| 필수 노드 | 배치 여부 | 비고 |
|---|---|---|
| GameSceneController (GameScene.ts) | 확인 | 씬 JSON __id__:4 |
| GameManager | 확인 | 씬 JSON __id__:7 |
| AudioManager | 확인 | 씬 JSON __id__:11 |
| StarSpawner | 확인 | 씬 JSON __id__:16 |
| Bucket (BucketController.ts) | 확인 | y=-260 |
| ConstellationManager | 확인 | |
| WaveManager | 확인 | 씬 JSON에 포함 (M-02 수정 결과) |
| HUD (HUDController.ts) | 확인 | y=290 |
| UIManager | 확인 | |
| PausePanel | 확인 | 기본 비활성 |
| FadeOverlay | 확인 | UIOpacity + Widget |

- Background 노드 SVG 연결: **에디터에서 bg_space.svg 수동 연결 필요** (자동화 불가, 알려진 제한사항)
- StarFragment Prefab: **에디터에서 생성 필요** (알려진 제한사항)
- 판정: **구조 완성. 에디터 수동 작업 2건 잔존**

### ResultScene 노드 구성 검증
- Canvas: 존재, 7개 자식 노드
- GameOverTitle, CurrentScoreLabel, BestScoreLabel, RestartButton, TitleButton, FadeOverlay 모두 포함 확인
- ResultScene.ts 스크립트 연결 구조 확인
- 판정: **완성**

**씬 구성 점수: 8.5 / 10**
(감점 요소: Prefab 미생성, SVG SpriteFrame 수동 연결 필요, 씬 JSON 내 스크립트 컴포넌트 직렬화 세부값 에디터 검증 필요)

---

## 그래픽 품질 평가 (SVG 파일 직접 분석)

### 리소스 목록 완성도
기획서 요구 12종 중 **12종 전량 존재** (100%)

| 파일 | viewBox | 크기 | 완성도 | 메모 |
|---|---|---|---|---|
| bg_space.svg | 0 0 960 640 | 960×640 | 우수 | 3중 성운 레이어, 90여 개 별 배치, 은하수 띠, 밝은 별 8개, 반짝이는 별 3개(십자 라인), 하단 그라디언트 |
| bucket.svg | 0 0 120 80 | 120×80 | 우수 | 사다리꼴 황금빛 버킷, 내부 어두운 우주색, 림 그라디언트, 손잡이, 별 장식 2개, 그림자 |
| star_red.svg | 0 0 64 64 | 64×64 | 양호 | 5각형 별, 라디알 그라디언트 (#ff9090→#ff2020→#880000), 외부 발광, 광택 하이라이트 |
| star_blue.svg | 0 0 64 64 | 64×64 | 양호 | 5각형 별, 파란 계열 (#90c8ff→#1a7fff→#003a99), 구조 일관성 |
| star_yellow.svg | 0 0 64 64 | 64×64 | 양호 | 5각형 별, 노란 계열, glow 강도 가장 밝음 (stdDeviation 3.5), 하이라이트 강조 |
| star_green.svg | 0 0 64 64 | 64×64 | 양호 | 5각형 별, 초록 계열 (#aaffaa→#00cc44→#006622) |
| star_purple.svg | 0 0 64 64 | 64×64 | 양호 | 5각형 별, 보라 계열 (#e8aaff→#aa22ff→#550088) |
| star_dark.svg | 0 0 64 64 | 64×64 | 우수 | 8각형(더 뾰족), 붉은 외부 발광, 어두운 코어, 불길한 눈 모양, 균열 라인 — 위협적 표현 적절 |
| icon_life.svg | 0 0 48 48 | 48×48 | 양호 | 하트+별 조합, 흰색/금색 계열 |
| ui_constellation_slot.svg | 0 0 40 40 | 40×40 | 양호 | 점선 원형 슬롯, 별 실루엣 중앙 배치, 빈 슬롯 표현 명확 |
| ui_button.svg | 0 0 200 60 | 200×60 | 우수 | 파란-보라 그라디언트, 둥근 직사각형, 그림자+발광 효과, 별 장식 양쪽 |
| logo_title.svg | 0 0 480 160 | 480×160 | 우수 | "STAR" 62pt + "SWEEPER" 38pt, 금색 그라디언트 텍스트, 별 장식 7개, 하단 라인 |

### 모바일 해상도 (960×640) 가독성 평가

**bg_space.svg**
- viewBox가 960×640으로 정확히 일치. 씬 해상도와 완벽 매칭.
- 별 밝기(0.5~1.0 opacity, r=0.7~1.2)는 64×64 별 오브젝트와 충분히 구분되는 크기.
- 하단 어두운 그라디언트가 버킷/HUD와의 가독성 확보에 기여.
- 평가: **양호**

**별 5종 색상 구분 명확성**
- RED: #ff2020 (진빨강), BLUE: #1a7fff (파랑), YELLOW: #ffdd00 (노랑), GREEN: #00cc44 (초록), PURPLE: #aa22ff (보라)
- 5색 모두 색상환 상에서 충분한 간격을 가짐 (빨→파→초→노→보).
- 다만 모든 별이 동일한 5각형 폴리곤 좌표를 공유: `points="32,4 39,24 60,24 44,37 50,58 32,46 14,58 20,37 4,24 25,24"` - 색상만 다르고 형태가 동일. 이것이 기획서 의도이므로 허용되나, 색약 사용자를 위한 형태 차별화는 없음.
- 노랑(YELLOW)의 glow stdDeviation이 3.5로 가장 강해 눈에 잘 띔.
- 평가: **색상 구분 명확 / 형태 구분 없음** (기획 범위 이내)

**Dark Star 위협적 표현**
- 8각형(18-point polygon) 사용으로 일반 5각 별과 형태 차별화 성공.
- 붉은 외부 발광 + 어두운 코어 + 불길한 눈 모양 중앙 장식.
- 일반 별 대비 시각적 위협감 충분히 전달됨.
- 평가: **우수**

**UI 터치 타겟 크기**
- 버킷: 120×80 SVG, 충돌 halfWidth=60, halfHeight=30 → 실제 터치 가능 영역 120×60px. 모바일 권장 최소 44px 기준 초과. **양호**
- 별 조각: 64×64 SVG, halfStar=32 충돌 판정. 모바일 최소 기준 충족. **양호**
- 라이프 아이콘: 48×48. 인터랙티브 요소 아님. **양호**
- 별자리 슬롯: 40×40. 인터랙티브 요소 아님. **양호**
- 버튼: 200×60. 충분한 터치 타겟. **우수**
- 일시정지 버튼: 씬 JSON에 PauseButton 노드 존재하나 SVG 미제공. 에디터에서 크기 지정 필요.

**그래픽 잠재 이슈**
- logo_title.svg: Arial Black/Impact 폴백 폰트 사용. 모바일에서 해당 폰트 미지원 시 sans-serif로 렌더링되어 디자인 의도와 다를 수 있음. SVG로 임포트 시 브라우저/엔진 폰트 렌더링에 의존.
- icon_life.svg (48×48): HUD에서 소형으로 표시될 경우 하트+별 조합 디테일이 뭉개질 수 있음. 32×32 이하에서 가독성 재확인 권장.

**그래픽 품질 점수: 8.0 / 10**
(감점 요인: 별 5종 형태 동일성, 로고 폰트 폴백 위험, 일시정지 버튼 SVG 미제공)

---

## 조작감 평가 (BucketController.ts 코드 분석)

### 터치 입력 반응성
- 이벤트 타입: `Input.EventType.TOUCH_MOVE` 사용 (기획서 요구사항 일치)
- `input.on()/off()` 를 `onEnable()/onDisable()` 에서 처리 — 컴포넌트 비활성화 시 자동 해제. **설계 우수**
- 게임 상태 체크: `GameManager.instance?.state !== GameState.PLAYING` 가드 적용. 일시정지/게임오버 중 입력 무시. **올바름**

### 버킷 이동 속도/가속도
- 이동 방식: `event.getDeltaX()` 를 위치에 직접 더하는 **즉시 이동** 방식 (가속도 없음).
- 드래그 델타값이 그대로 위치에 반영되므로 터치 속도 = 버킷 속도.
- 가속도/감속도 없음 → 빠른 스와이프 시 오버슛(overshooting) 없음. 반응이 즉각적이고 예측 가능.
- 캐주얼 게임 특성상 적절한 조작 방식. 다만 고속 스와이프 시 클램핑에서 즉각 멈추는 느낌이 다소 단조로울 수 있음.

### 화면 경계 클램핑
```typescript
const newX = this._clamp(pos.x + delta,
    -this._screenHalfW + this.halfWidth,
    this._screenHalfW - this.halfWidth);
```
- `view.getVisibleSize()` 로 동적 화면 크기 획득. 하드코딩 없음. **양호**
- halfWidth(60)를 경계에서 빼 버킷이 화면 밖으로 나가지 않음. **올바름**
- 기획서 요구사항 "화면 경계 클램핑" 완전 충족.

### 충돌 박스 크기 적절성
- 버킷 충돌: halfWidth=60 / halfHeight=30 (기본값). 실제 SVG 크기 120×80 대비 충돌 영역 = SVG 크기 정확히 일치.
- 별 충돌 반경: `halfStar = 32` (StarSpawner.ts L140). 64×64 SVG의 절반 = 정확히 SVG 크기 기준.
- **주의**: 별 충돌은 AABB이지만 별 모양은 5각형이므로 별 꼭짓점 바깥 공간에서도 충돌이 발생할 수 있음. 시각적으로 살짝 빗나간 것처럼 보일 수 있는 픽셀이 발생 가능. 캐주얼 게임 기준으로는 허용 범위.
- 버킷 충돌 세로(halfHeight=30)가 SVG 높이(80)의 37.5%에 불과 — 실제로는 버킷의 "입" 부분만 유효 수집 영역으로 처리됩니다. 이는 오히려 게임플레이 의도에 적합함(버킷 입구에 맞아야 수집).

**조작감 점수: 8.5 / 10**
(감점 요인: 가속도 없는 즉시 이동으로 관성감 부재, AABB 충돌이 별 모양과 불일치 — 둘 다 캐주얼 게임 기준 허용)

---

## 코드 품질 평가

### TypeScript 문법
- 15개 파일 전체 TypeScript 문법 오류 없음 (정적 분석 기준)
- `@ccclass`, `@property` 데코레이터 모든 컴포넌트에서 올바르게 사용
- `enum`, `interface`, `Record<>`, `Partial<>` 타입 적절하게 활용
- 옵셔널 체이닝 (`?.`) 대부분의 GameManager 접근에서 사용

### Cocos Creator 3.8.8 API 사용
- `input.on/off (Input.EventType.TOUCH_MOVE)`: 올바름
- `view.getVisibleSize()`: 올바름
- `tween().to().call().start()`: 올바름
- `director.addPersistRootNode()`: 올바름 (싱글톤 유지)
- `resources.load('path/spriteFrame', SpriteFrame, callback)`: 올바름
- `sys.localStorage.getItem/setItem`: 올바름
- `UIOpacity` 컴포넌트를 tween 대상으로 사용: 올바름
- `scheduleOnce()`: 올바름 (ConstellationManager 패턴 딜레이)
- `director.pause()/resume()` 전역 일시정지: **주의 필요** (M-06 이슈 잔존 — UI tween도 멈춤)

### 잔여 버그 및 이슈

#### [잔존 Critical] UIManager 콜백 메모리 누수
**위치**: `UIManager.ts`, `onLoad()` (L36-43)
**내용**: `gm.onScoreChanged`, `gm.onLivesChanged`, `gm.onGameOver` 를 직접 함수 참조로 GameManager에 할당. GameManager는 persist 노드이므로 씬 전환 후에도 UIManager의 람다가 GameManager 콜백으로 남습니다. 다음 씬 진입 시 이전 씬의 UIManager 인스턴스가 파괴되었음에도 GameManager가 해당 콜백을 보유합니다.
- `onWaveChanged`는 배열 기반으로 수정됨 (C-01 수정 완료)
- 그러나 `onScoreChanged`, `onLivesChanged`, `onGameOver`는 여전히 단일 함수 포인터 방식
- `UIManager.onDestroy()`가 구현되지 않음 → 콜백 미정리
**위험도**: 씬 전환 후 점수 변경 시 파괴된 UIManager가 `scoreLabel.string = ...` 접근 시도 → 크래시 또는 null 접근 경고

#### [잔존 Major] SFX 호출 미연결
**위치**: `StarSpawner.ts`, `ConstellationManager.ts`, `GameManager.ts`
**내용**: AudioManager에 playCatch(), playConstellation(), playLoseLife() 메서드는 구현되어 있으나 실제 호출 지점이 없음.
- 별 수집 시 수집음 없음 (`StarSpawner._onStarCaught`)
- 별자리 완성 시 완성음 없음 (`ConstellationManager._checkCompletion`)
- 라이프 감소 시 감소음 없음 (`GameManager.loseLife`)
- 결과 화면 게임오버음만 연결됨 (`ResultScene.start()`)

#### [잔존 Major] UIManager / HUDController 역할 중복
**위치**: `UIManager.ts`, `HUDController.ts`
**내용**: 두 컴포넌트 모두 scoreLabel, waveLabel 프로퍼티를 보유. UIManager는 GameManager 콜백 기반, HUDController는 메서드 직접 호출 방식. GameScene.scene에 두 노드 모두 배치됨. 실제 씬에서 어느 쪽이 레이블 노드를 참조하느냐에 따라 중복 갱신 또는 미갱신 발생 가능.

#### [잔존 Minor] StarSpawner._constellationManager any 타입
**위치**: `StarSpawner.ts`, L172
```typescript
private _constellationManager: any = null;
```
`ConstellationManager` 타입으로 교체해야 타입 안전성 확보 가능.

#### [잔존 Minor] ObjectPool 미사용 (죽은 코드)
**위치**: `ObjectPool.ts`
StarSpawner가 자체 `_pool: Node[]`를 직접 구현하므로 ObjectPool.ts는 실제로 사용되지 않음.

#### [잔존 Minor] 콤보 HUD 미표시
콤보 3 이상 시 ×1.5 점수 배수가 적용되나 HUD에 콤보 상태 표시 없음. 플레이어가 콤보 보너스 발생 여부를 알 수 없음.

### 코드 품질 점수: 7.5 / 10
(감점: UIManager 콜백 누수, SFX 미연결, any 타입 사용, 죽은 코드, HUD 역할 중복)

---

## 게임 밸런스 분석

### Wave별 스폰 속도/패턴 (코드 확인)
| Wave | 낙하 속도 | 스폰 간격 | 색상 | 보스 | 기획 일치 |
|---|---|---|---|---|---|
| 1 | 200 | 1.5s | RED, BLUE | N | 일치 |
| 2 | 240 | 1.3s | RED, BLUE, YELLOW | N | 일치 |
| 3 | 280 | 1.1s | RED, BLUE, YELLOW | Y (30% DARK) | 일치 |
| 4 | 320 | 1.0s | RED, BLUE, YELLOW, GREEN | N | 일치 |
| 5 | 360 | 0.9s | 전체 5색 | N | 일치 |
| 6+ | last+20×n | max(0.4, last-0.05×n) | 5색 | wave%3==0 | 일치 |

### 별자리 패턴과 Wave 색상 매칭 (M-04 수정 확인)
```
오리온자리 (Wave 1): { RED: 3, BLUE: 2 } → Wave 1 스폰 색상 [RED, BLUE] → 달성 가능
큰곰자리 (Wave 2):   { BLUE: 2, GREEN: 2, YELLOW: 1 } → Wave 2 스폰 [RED,BLUE,YELLOW], GREEN 미포함
```
**신규 발견 이슈 (Medium)**: 큰곰자리 패턴이 Wave 2에서 등장할 경우 GREEN 2개를 요구하지만 Wave 2 스폰 색상에 GREEN이 없습니다. Wave 4 이후로 별자리 순환 시 큰곰자리가 다시 등장하면 달성 가능하나, Wave 2 첫 등장 시점에는 달성 불가. M-04 수정으로 Wave 1 문제는 해결되었지만 Wave 2-3 범위의 동일한 문제가 잔존합니다.

- 카시오페이아 (Wave 3): { RED: 2, PURPLE: 2, BLUE: 1 } → Wave 3 스폰에 PURPLE 없음 → Wave 5 이전 달성 불가
- 사자자리 (Wave 4): { RED: 2, YELLOW: 2, GREEN: 2, BLUE: 1 } → Wave 4부터 GREEN 추가 → 달성 가능
- 전갈자리 (Wave 5): { PURPLE: 3, RED: 2, GREEN: 2 } → Wave 5부터 PURPLE 추가 → 달성 가능

**정리**: Wave 1 수정은 완료되었으나 별자리 패턴 순환이 Wave별 가용 색상과 완전히 동기화되지 않은 문제가 Wave 2~3 구간에서 잔존합니다. Wave 진행에 따라 패턴 인덱스가 증가하는 단순 순환 방식으로는 색상 불일치를 완전히 해소하기 어렵습니다.

### 난이도 곡선
- Wave 1→5: 낙하 속도 200→360(+160, 80%), 스폰 간격 1.5→0.9(-40%). 자연스러운 점진적 상승.
- Wave 5→6+: 속도 +20/wave, 간격 -0.05/wave(하한 0.4s). 이론적으로 무한 진행 가능한 구조.
- spawnInterval 0.4s 하한 + 낙하 속도 무제한 증가 → 고 Wave에서 화면에 별이 동시에 10개 이상 존재 가능. 초반 플레이어에게 이른 난이도 스파이크 없음.
- Wave 전환 조건 "별자리 3개 완성마다": 별자리가 5종 순환이므로 Wave 완료당 약 3×5=15개 별 필요. Wave 1(1.5s 간격)기준 약 22.5초. 적절한 플레이 타임.

---

## 잔여 이슈 목록

### Critical (즉시 수정)
| ID | 위치 | 내용 |
|---|---|---|
| C-NEW-01 | UIManager.ts, GameManager.ts | UIManager가 GameManager 콜백(onScoreChanged, onLivesChanged, onGameOver)을 등록 후 onDestroy에서 정리하지 않음 → 씬 전환 후 메모리 누수 및 파괴된 컴포넌트 접근 크래시 위험 |
| C-REMAIN-02 | GameManager.ts L57, AudioManager.ts L14 | `return GameManager._instance!` null assertion — 아직 수정 미완료 |

### Major (다음 iteration)
| ID | 위치 | 내용 |
|---|---|---|
| M-NEW-01 | ConstellationManager.ts | Wave 2~3 별자리 패턴 색상 불일치 잔존 (큰곰자리 GREEN 2개 요구 → Wave 2 스폰에 GREEN 없음; 카시오페이아 PURPLE 2개 요구 → Wave 3 스폰에 PURPLE 없음) |
| M-REMAIN-03 | StarSpawner.ts, ConstellationManager.ts, GameManager.ts | SFX 호출 미연결 (별 수집음, 별자리 완성음, 라이프 감소음) |
| M-REMAIN-01 | UIManager.ts, HUDController.ts | 역할 중복 — 통합 또는 명확한 분리 필요 |
| M-REMAIN-06 | GameManager.ts | director.pause()/resume() UI tween 멈춤 부작용 |

### Minor (개선 권장)
| ID | 위치 | 내용 |
|---|---|---|
| m-REMAIN-02 | ConstellationManager.ts | _colorSymbol이 모든 색상에 동일 '★' 반환 — 색상별 구분 불가 |
| m-REMAIN-03 | ObjectPool.ts | 미사용 죽은 코드 |
| m-REMAIN-04 | GameManager.ts, DataManager.ts | 최고 점수 이중 저장 로직 |
| m-REMAIN-05 | HUDController.ts, UIManager.ts | 콤보 보너스 HUD 표시 없음 |
| m-NEW-01 | StarSpawner.ts L172 | _constellationManager: any 타입 — ConstellationManager로 교체 권장 |
| m-NEW-02 | logo_title.svg | Arial Black/Impact 폰트 폴백 — 모바일 렌더링 결과 확인 필요 |
| m-NEW-03 | icon_life.svg | 48×48 HUD 소형 표시 시 디테일 뭉개짐 가능 — 32×32 버전 필요 여부 검토 |

---

## 다음 Iteration 권장사항

### 개발봇 (우선순위 순)

1. **[Critical] UIManager.onDestroy() 콜백 정리 추가**
   - `onDestroy()`에서 `gm.onScoreChanged = null`, `gm.onLivesChanged = null`, `gm.onGameOver = null` 실행
   - 또는 onWaveChanged처럼 배열 기반 리스너 방식으로 통일

2. **[Major] Wave 2~3 별자리 패턴 색상 불일치 수정**
   - `buildPattern(wave)`에서 해당 Wave의 `availableColors`를 참조하여 달성 가능한 패턴만 반환하도록 필터링 로직 추가
   - 또는 기획봇과 협의하여 패턴과 Wave를 완전히 매핑

3. **[Major] SFX 호출 연결**
   - `StarSpawner._onStarCaught()` → `AudioManager.instance?.playCatch()`
   - `ConstellationManager._checkCompletion()` → `AudioManager.instance?.playConstellation()`
   - `GameManager.loseLife()` → `AudioManager.instance?.playLoseLife()`

4. **[Minor] StarSpawner._constellationManager 타입 수정**
   - `any` → `ConstellationManager | null`

5. **[Minor] UIManager/HUDController 역할 정리**
   - 에디터에서 어느 쪽이 실제 레이블을 참조하는지 명확히 하고, 사용하지 않는 쪽의 중복 프로퍼티 제거

### 기획봇

1. **Wave별 별자리 패턴 달성 가능성 보장 원칙 명시**
   - 각 Wave에서 등장하는 별자리 패턴은 해당 Wave의 가용 색상만 사용해야 함을 spec에 규칙으로 추가
   - 또는 패턴과 Wave의 완전한 매핑 테이블 제공

2. **콤보 HUD 표시 스펙 추가**
   - 콤보 카운터와 ×1.5 활성 여부를 어디에 표시할지 UI 스펙 추가

3. **일시정지 버튼 리소스 명세**
   - `ui_button.svg`를 일시정지 버튼으로 사용할지, 별도 `icon_pause.svg` 추가 여부 명시

### 디자인봇

1. **별자리 슬롯 색상 구분 버전**
   - `ui_constellation_slot.svg` 6색 버전 (각 별 색상 + 빈 슬롯) 제작

2. **콤보 이펙트 텍스트 리소스**
   - "COMBO x1.5!" 팝업 텍스트 이펙트 SVG

3. **일시정지 아이콘 별도 제공**
   - `icon_pause.svg` (현재 미제공)

4. **icon_life.svg 32×32 소형 버전**
   - HUD 소형 표시 대응

---

## 기획서 충족도 (최신)

| # | 요구사항 | 구현 여부 | 비고 |
|---|---|---|---|
| R01 | 버킷 좌우 드래그/탭 이동 | 완료 | TOUCH_MOVE 기반 |
| R02 | 화면 경계 클램핑 | 완료 | 동적 해상도 대응 |
| R03 | 별 조각 낙하 (색상별, 랜덤 속도/위치) | 완료 | |
| R04 | 별 수집 시 점수 획득 | 완료 | 색상별 점수 정확 |
| R05 | 별 놓침 시 라이프 -1 | 완료 | DarkStar 놓침 제외 |
| R06 | 라이프 3개, 0이 되면 게임 오버 | 완료 | |
| R07 | 별자리 목표 패턴 표시 (상단) | 부분 | 텍스트 기반, 색상 구분 미흡 |
| R08 | 별자리 완성 시 +200점 | 완료 | |
| R09 | Wave 진행 (별자리 3개마다 +1) | 완료 | |
| R10 | Wave 3마다 보스 웨이브 (DarkStar 30%) | 완료 | |
| R11 | DarkStar 수집 시 -2 라이프 | 완료 | |
| R12 | 콤보 3개 이상 ×1.5 점수 | 완료 | HUD 표시 없음 |
| R13 | Wave별 낙하 속도/스폰 간격 수치 | 완료 | 기획 수치 정확 |
| R14 | Wave 1에서 RED/BLUE만 스폰 | 완료 | |
| R15 | Wave 1 별자리 패턴 달성 가능 | **완료** | M-04 수정 완료 |
| R16 | Wave 2~3 별자리 패턴 달성 가능 | **미충족** | GREEN/PURPLE 색상 불일치 잔존 |
| R17 | TitleScene 구현 | 완료 | 씬 파일 생성 완료 |
| R18 | GameScene 구현 | 완료 | 씬 파일 생성 완료, 에디터 연결 필요 |
| R19 | ResultScene 구현 | 완료 | 씬 파일 생성 완료 |
| R20 | 씬 전환 페이드 효과 | 완료 | |
| R21 | localStorage 최고 점수 저장 | 완료 | |
| R22 | BGM 루프 재생 | 완료 (코드) | 클립 에디터 연결 필요 |
| R23 | SFX 4종 | 부분 | 게임오버음만 연결, 나머지 미호출 |
| R24 | Wave 전환 연출 (팝업) | 완료 | WaveManager 연결 완료 (M-02 수정) |
| R25 | 일시정지/재개 | 완료 | |
| R26 | 오브젝트 풀링 | 완료 | StarSpawner 내부 풀 |
| R27 | SVG 리소스 12종 | 완료 | 전량 생성 |

**충족률**: 22/27 (81.5%) — 이전 20/25(80%) 대비 소폭 향상

---

## 종합 점수 요약

| 항목 | 점수 |
|---|---|
| 종합 평가 | **7.8 / 10** |
| 씬 구성 완성도 | **8.5 / 10** |
| 그래픽 품질 | **8.0 / 10** |
| 조작감 (코드 분석) | **8.5 / 10** |
| 코드 품질 | **7.5 / 10** |
| 기획 충족도 | **81.5%** |
| Critical 잔여 | **2건** |
| Major 잔여 | **4건** |
| Minor 잔여 | **7건** |
