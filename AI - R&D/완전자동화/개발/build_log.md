# 빌드 로그 v3 (Iteration 2)
**날짜**: 2026-03-26
**시도자**: 개발봇 (AI 자동화 파이프라인)

---

## 빌드 시도 결과: **SKIP (CocosCreator.exe 미탐지)**

### 시도한 탐색 경로

| 경로 | 결과 |
|---|---|
| `C:/Program Files/Cocos/Creator/3.8.8/` | 없음 |
| `C:/Users/SANDBOX/AppData/Local/Programs/cocos-creator/` | 없음 |
| `C:/Users/SANDBOX/AppData/Local/Programs/CocosCreator/` | 없음 |
| `C:/CocosDashboard/` | 없음 |
| `D:/CocosDashboard/` | 없음 |
| `C:/Cocos/` | 있음 (Cocos Studio, Cocos2d-x — **Creator 3.8.8 아님**) |
| Registry HKLM:\Software\Cocos* | 탐색 결과 없음 |
| MCP (`mcp__unityMCP__execute_menu_item`) | 사용 불가 — Unity MCP이므로 Cocos Creator 빌드 불지원 |

### 결론

CocosCreator 3.8.8 CLI 실행 파일(`CocosCreator.exe`)을 자동 탐지하지 못했습니다.
시스템에 Cocos Studio / Cocos2d-x는 설치되어 있으나 **Creator 3.8.8은 별도 경로**에 설치된 것으로 추정됩니다.

---

## 수동 빌드 방법

### 방법 A — Cocos Creator 에디터 GUI 빌드

1. Cocos Creator 3.8.8 에디터에서 프로젝트 열기:
   `C:/Users/SANDBOX/Desktop/이직/Portfolio/AI - R&D/완전자동화/프로젝트`
2. 상단 메뉴 → **Project** → **Build...**
3. Platform: **Web Mobile** 선택
4. Build Path: `./build/web-mobile`
5. **Build** 버튼 클릭

### 방법 B — CLI 빌드 (경로 확인 후)

```bash
# CocosCreator.exe 경로 확인 후 아래 명령 실행
export COCOS_CREATOR_PATH="[실제 경로]/CocosCreator.exe"
bash "C:/Users/SANDBOX/Desktop/이직/Portfolio/AI - R&D/완전자동화/배포/build.sh"
```

### 방법 C — 배포 스크립트 직접 활용

```
배포/build.sh  → COCOS_CREATOR_PATH 환경변수 설정 후 실행
```

---

## 빌드 전 체크리스트 (에디터 수동 연결 필요 항목)

| 항목 | 내용 | 우선순위 |
|---|---|---|
| ConstellationManager | `slotNodes` 배열에 ConstellationUI 하위 Sprite 노드 연결 (m-01) | 필수 |
| StarSpawner | `starPool` 슬롯에 ObjectPool 컴포넌트 연결 (m-02) | 필수 |
| ObjectPool | `prefab` 슬롯에 StarFragment Prefab 연결 (m-02) | 필수 |
| TitleScene | `bookButton` 슬롯에 도감 버튼 Node 연결 | 필수 |
| ConstellationBookScene | `cardUnlockedPrefab`, `cardLockedPrefab` Prefab 연결 | 권장 |
| AudioManager | `sfxBookUnlock` 슬롯에 AudioClip 연결 | 선택 |
| ConstellationBookScene | `ConstellationBookScene` 씬을 Build Settings 씬 목록에 추가 | 필수 |
