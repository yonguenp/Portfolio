# 개발 노트 v3 (Iteration 2)
**게임**: Star Sweeper
**작성일**: 2026-03-26
**개발봇**: AI 자동화 파이프라인 (Iteration 2)
**기준 문서**: spec_v3.md, QA/latest_report.md (9.0/10)

---

## 수정된 파일 목록

| 파일 | 수정 항목 |
|---|---|
| `ConstellationManager.ts` | m-01 색상별 SVG 슬롯 방식 교체, m-05 주석 명시, m-05 buildPattern Wave 7+ 랜덤 생성, NEW-02 도감 연동 |
| `StarSpawner.ts` | m-02 인라인 풀 제거 → ObjectPool.get()/put() 방식 교체 |
| `GameManager.ts` | m-03 _saveBestScore()/_loadBestScore() 완전 제거, DataManager 단독 호출 |
| `HUDController.ts` | m-04 tween 중첩 방지 (기존 tween stop 후 재시작) |
| `AudioManager.ts` | NEW-02 sfxBookUnlock @property 및 playBookUnlock() 추가 |
| `TitleScene.ts` | bookButton @property 추가, onBookButtonClicked() 구현 |

### 신규 생성 파일

| 파일 | 내용 |
|---|---|
| `ConstellationBookManager.ts` | localStorage 기반 별자리 도감 기록 관리 (static 메서드) |
| `ConstellationBookScene.ts` | 별자리 도감 씬 컨트롤러, 카드 그리드 UI |
| `assets/scenes/ConstellationBookScene.scene` | 도감 씬 JSON (Canvas, ScrollView, CardContainer, BackButton, FadeOverlay) |
| `배포/build.sh` | CocosCreator CLI 빌드 자동화 스크립트 |
| `개발/build_log.md` | 빌드 시도 결과 기록 |

---

## 각 수정 내용 요약

### m-01: ConstellationManager._colorSymbol() 색상별 SVG 슬롯 교체

**ConstellationManager.ts**:
- `_colorSymbol()` 메서드 완전 제거
- `constellationProgressLabel` @property 제거 (텍스트 라벨 방식 폐기)
- `slotNodes: Node[]` @property 추가 — 에디터에서 슬롯 노드 배열 연결
- `_preloadSlotFrames()`: `slot_empty/spriteFrame`, `slot_red/spriteFrame` 등 5종 사전 로드
- `_updateUI()`: slotNodes 배열의 각 Sprite에 수집 상태에 따라 SpriteFrame 설정
  - 수집 완료 → `slot_[color].svg` SpriteFrame
  - 미수집 → `slot_empty.svg` SpriteFrame

### m-02: StarSpawner 인라인 풀 → ObjectPool.get()/put() 교체

**StarSpawner.ts**:
- `starFragmentPrefab`, `poolSize` @property 제거
- `starPool: ObjectPool` @property 추가
- `_pool`, `_buildPool()`, `_getFromPool()` 완전 제거
- `_spawnStar()`: `this.starPool.get()` 호출
- `_returnToPool()`: `sf.reset()` 후 `this.starPool.put(sf.node)` 호출
- `clearAll()`: `this.starPool.put(sf.node)` 호출

### m-03: GameManager 최고 점수 저장 DataManager 일원화

**GameManager.ts**:
- `import { sys }` 제거, `import { DataManager }` 추가
- `_loadBestScore()` 메서드 완전 제거
- `_saveBestScore()` 메서드 완전 제거
- `onLoad()`: `DataManager.loadBestScore()` 단독 호출
- `triggerGameOver()`: `DataManager.saveBestScore(this._score)` 단독 호출

### m-04: HUDController.showComboEffect() tween 중첩 방지

**HUDController.ts**:
- `showComboEffect()` 진입 시 `popup.active` 체크
- 활성 상태이면 `tween(popup).stop()` + `tween(opacity).stop()` 후 재시작
- opacity.opacity = 255 초기화 후 tween 재실행

### m-05: ConstellationManager.addStar() 패턴 외 색상 무시 주석 명시

**ConstellationManager.ts**:
- `addStar()` 내 패턴 외 색상 조용히 무시하는 로직에 주석 추가:
  `// 패턴 외 색상 — 조용히 무시 (슬롯 반응 없음) (spec_v3 m-05 의도된 설계)`

### m-05 (buildPattern): Wave 7+ 랜덤 패턴 생성 구현

**ConstellationManager.ts** `buildPattern()` + `_buildRandomPattern()`:
- `wave >= 7` 조건에서 순환 방식(`(wave-1) % patterns.length`) 제거
- `_buildRandomPattern(wave)` 함수 신규 구현:
  - 총 별 수: `min(6 + Math.floor((wave-7)/2), 10)`
  - 5색 중 2~4종 랜덤 선택
  - 각 색상 최소 1개 배분 후 잔여를 랜덤 배분
  - 단일 색상 50% 초과 배정 불가 (`maxPerColor = floor(totalStars * 0.5)`)
  - `Math.random()` 기반 순수 랜덤 (시드 없음)

### NEW-02: 별자리 도감 시스템

**ConstellationBookManager.ts** (신규):
- `recordCompletion(name, wave): boolean` — 신규 true, 중복 false
- `getRecords(): ConstellationRecord[]` — 전체 기록 반환
- `isUnlocked(name): boolean` — 해금 여부
- localStorage 키: `star_sweeper_book`
- JSON 배열 형식: `[{ name, wave, date(ISO) }, ...]`

**ConstellationBookScene.ts** (신규):
- 별자리 7종 마스터 데이터 정의
- `_buildCardGrid()`: 해금/미해금 상태에 따라 카드 Prefab 인스턴스화
- 완성 카드: nameLabel, waveLabel, dateLabel 채우기
- 잠김 카드: "???", "미완성" 표시
- Prefab 미연결 시 Label fallback 카드 생성
- `onBackButtonClicked()`: 페이드 아웃 후 TitleScene 복귀

**ConstellationBookScene.scene** (신규):
- Canvas (960×640) > Background, TitleLabel, ScrollView(CardContainer), BackButton, BookSceneController, FadeOverlay
- ConstellationBookScene 컴포넌트를 BookSceneController 노드에 부착
- BackButton clickEvents → BookSceneController.onBackButtonClicked()

**ConstellationManager.ts** 수정:
- `_checkCompletion()` 내 `ConstellationBookManager.recordCompletion()` 호출
- 신규 등록(true) 시 `AudioManager.instance?.playBookUnlock()` 호출

**AudioManager.ts** 수정:
- `sfxBookUnlock: AudioClip` @property 추가
- `playBookUnlock()` 메서드 추가

**TitleScene.ts** 수정:
- `bookButton: Node` @property 추가
- `onBookButtonClicked()` 메서드 추가 (페이드 후 ConstellationBookScene 로드)

---

## 빌드 시도 결과

**결과**: SKIP — CocosCreator 3.8.8 CLI 실행 파일 자동 탐지 실패

| 시도 방법 | 결과 |
|---|---|
| CocosCreator.exe 경로 자동 탐색 (6개 경로) | 모두 실패 |
| MCP (`mcp__unityMCP__execute_menu_item`) | Unity MCP으로 Cocos Creator 불지원 |

**빌드 스크립트 저장 위치**: `배포/build.sh`

**수동 빌드 방법**: Cocos Creator 에디터 > Project > Build > Web Mobile

---

## 배포 URL

빌드 미완료로 배포 URL 없음. 빌드 성공 후 아래 명령으로 gh-pages 배포:

```bash
cd 프로젝트 && npx gh-pages -d build/web-mobile
```

---

## 에디터에서 수동 연결 필요 항목

| 씬/컴포넌트 | 연결 항목 | 내용 |
|---|---|---|
| **GameScene** | `ConstellationManager.slotNodes` | ConstellationUI 하위 Sprite 노드 배열 (8~10개) |
| **GameScene** | `StarSpawner.starPool` | ObjectPool 컴포넌트가 붙은 노드 |
| **GameScene** | `ObjectPool.prefab` | StarFragment Prefab |
| **TitleScene** | `TitleScene.bookButton` | icon_book.svg 버튼 노드 |
| **ConstellationBookScene** | `ConstellationBookScene.cardUnlockedPrefab` | card_constellation.svg 카드 Prefab |
| **ConstellationBookScene** | `ConstellationBookScene.cardLockedPrefab` | card_locked.svg 카드 Prefab |
| **AudioManager** | `AudioManager.sfxBookUnlock` | 도감 해금 SFX AudioClip |
| **Build Settings** | 씬 목록 | `ConstellationBookScene` 씬 추가 필요 |

---

## 완료 체크리스트

- [x] m-01: ConstellationManager._colorSymbol() → slot_[color].svg Sprite 슬롯 방식 교체
- [x] m-02: StarSpawner 인라인 풀 → ObjectPool.get()/put() 방식 교체
- [x] m-03: GameManager._saveBestScore() 제거, DataManager.saveBestScore() 단독 호출
- [x] m-04: HUDController.showComboEffect() tween 중첩 방지 (stop 후 재시작)
- [x] m-05: buildPattern() Wave 7+ 랜덤 패턴 생성 + addStar() 주석 명시
- [x] ConstellationBookManager.ts 신규 생성
- [x] ConstellationBookScene.ts 신규 생성
- [x] ConstellationBookScene.scene 신규 생성
- [x] TitleScene.ts bookButton 추가 및 onBookButtonClicked() 구현
- [x] AudioManager.ts playBookUnlock() 추가
- [x] 빌드 시도 결과 기록 (build_log.md)
- [x] 빌드 스크립트 저장 (배포/build.sh)
- [ ] 빌드 성공 (CocosCreator.exe 미탐지로 SKIP — 수동 빌드 필요)
