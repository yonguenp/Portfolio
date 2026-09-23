# UnityWithClaude 작업 가이드

## 프로젝트 개요

- Unity `6000.3.11f1` / Universal Render Pipeline `17.3.0` 기반의 세로형 모바일 미니게임 포트폴리오다.
- 주요 게임: 1 to 50, 2048, 1010, Color Sort, Brick Breaker 3D, 고스톱(3~4인·네트워크), 삼국 디펜스.
- 활성 빌드 씬은 `ProjectSettings/EditorBuildSettings.asset`에서 확인한다. 시작 흐름은 `SplashScene` → `TitleScene`이다.
- 상세한 게임 규칙, UI 좌표, 과거 결정과 배포 절차는 `CLAUDE.md`를 먼저 검색해서 필요한 부분만 읽는다. 이 파일의 간결한 작업 규칙이 우선이다.

## 작업 원칙

- 사용자 작업 트리가 더티일 수 있다. 요청과 무관한 파일·변경은 절대 되돌리거나 포맷하지 않는다.
- 씬(`.unity`)과 프리팹(`.prefab`)은 Unity 직렬화 파일이다. 텍스트로 수정할 때는 최소 범위만 변경하고 `.meta` GUID를 보존한다.
- 스크립트는 `Assets/Scripts/`, 에디터 도구는 `Assets/Editor/`에 둔다. 런타임 코드에서 `UnityEditor` 네임스페이스를 참조하지 않는다.
- 새 에셋 또는 스크립트에는 대응 `.meta` 파일이 필요하다. 가능하면 Unity Editor가 생성하게 하고, 불가피한 수동 생성은 피한다.
- 기존 UI는 공용 `GameUI`/`GameUIManager` 및 게임별 UI 매니저 구조를 우선 재사용한다. 고스톱 UI는 `GoStopUIManager`와 `Assets/Resources/Prefabs/GoStop/`을 우선 확인한다.
- `CLAUDE.md`에 적힌 캔버스·Safe Area·세로 화면 규칙을 지킨다. 화면 배치나 게임 규칙을 추정하지 말고 관련 씬/코드/문서를 확인한다.

## Unity 제어와 검증

프로젝트에는 Unity 공식 CLI와 Pipeline 패키지(`com.unity.pipeline`)가 설치돼 있다. 에디터가 열려 있고 Pipeline 서버가 연결된 경우에만 아래 명령을 사용한다.

```bash
unity pipeline list
unity command recompile
unity command recompile_status
unity command console '{"count":50}'
unity command editor_play
unity command editor_stop
unity command editor_status
```

- `unity pipeline list`에서 `Server Reachable: true`인지 먼저 확인한다. false이거나 포트가 없으면 Unity Editor를 재시작한 뒤 다시 확인한다.
- C# 변경 뒤에는 `recompile`과 `recompile_status` 또는 콘솔 조회로 컴파일 오류를 확인한다.
- Play 모드 전환, 빌드 타겟 전환, 전체 빌드, 패키지 설치·갱신은 프로젝트 상태와 시간이 크게 바뀔 수 있다. 사용자의 요청 범위와 현재 더티 상태를 확인한 뒤 수행한다.
- `switch_build_target`은 전체 리임포트를 유발한다. 명시적 요청 없이는 실행하지 않는다.
- Game View 캡처는 Screen Space Overlay Canvas UI를 검증하지 못한다. UI 상태는 에디터/계층 또는 Pipeline `eval`로 확인한다.

## 완료 기준

- 변경한 파일만 요약하고, 실행한 검증과 결과를 보고한다.
- 컴파일 또는 실행 검증을 못 했다면 이유와 사용자가 수행할 한 가지 다음 조치를 명확히 적는다.
- 빌드·배포·Git 커밋·외부 서비스 변경은 사용자가 명시적으로 요청한 경우에만 한다.
