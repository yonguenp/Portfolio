# 박용근 | 게임 클라이언트 개발 포트폴리오

Unity/C#과 Cocos2d-x/C++로 모바일 게임을 개발해 온 12년차 클라이언트 개발자입니다. 10여 개 게임의 출시와 라이브 서비스를 담당했습니다.

[yonguen@naver.com](mailto:yonguen@naver.com)

**이 포트폴리오에서 보여주고 싶은 것은 변화하는 요구를 빠르게 구현하고, 다음 업데이트도 쉽게 만드는 개발 방식입니다.** 게임 화면과 전투 로직을 직접 만들고, 필요하면 WebView·서버 연동·도구·빌드 흐름까지 손봐서 팀의 작업 시간을 줄였습니다.

## 먼저 볼 코드

| 보고 싶은 역량 | 시작할 코드 | 확인할 내용 |
| --- | --- | --- |
| 전투와 캐릭터 스킬 | [스킬 발동 흐름](코드샘플/메타토이드래곤즈/유니티/Battle/BattleStateLogicSkill.cs), [효과 공통 구조](코드샘플/메타토이드래곤즈/유니티/Battle/SkillEffect/EffectInfo.cs) | 발동 조건과 대상 판정, 효과 실행을 분리한 전투 구조 |
| 변경이 잦은 UI | [Unity WebView 연결](코드샘플/메타토이드래곤즈/유니티/WebView/SBWebViewController.cs), [React 화면](코드샘플/메타토이드래곤즈/인게임웹뷰_리액트/components) | 앱 안에서 웹 화면을 열고 운영 UI를 교체하는 방식 |
| 모바일 UI와 성능 | [앵커·Stretch 크기 제어](코드샘플/양어장고양이/유니티/UI/UISizeController.cs), [오브젝트 풀](코드샘플/공포의술래잡기/유니티/Managers/Core/PoolManager.cs) | 해상도 변화와 반복 객체 생성에 대응하는 코드 |
| 라이브 리소스 | [AssetBundle 관리자](코드샘플/메타토이드래곤즈/유니티/bundle/AssetBundleManager.cs) | CDN 리소스 목록·해시·다운로드를 다루는 흐름 |

아래에는 **상황 → 직접 맡은 일 → 결과 → 확인할 코드** 순서로 사례를 정리했습니다. 이 저장소의 코드는 전체 게임 실행본이 아닌 주요 구현 발췌본입니다.

## 1. 메타 토이 드래곤즈 사가

**Unity 수집형 RPG · 샌드박스네트워크 · 개발팀 리드**

### 보여주고 싶은 문제 해결

초기 Cocos Creator 웹게임을 Unity 모바일 게임으로 전환하며 전투·성장·운영 UI를 서비스 구조에 맞게 구현했습니다. 타운 건설, 월드 탐험, 보스 레이드, PvP, 길드 콘텐츠 개발과 라이브 운영을 맡았습니다.

- **캐릭터 스킬:** 발동 조건·대상 선정·효과 적용을 나눠 여러 스킬을 같은 전투 흐름에서 처리했습니다. [발동 및 대상 판정](코드샘플/메타토이드래곤즈/유니티/Battle/BattleStateLogicSkill.cs) → [효과 공통 데이터](코드샘플/메타토이드래곤즈/유니티/Battle/SkillEffect/EffectInfo.cs) → [회복](코드샘플/메타토이드래곤즈/유니티/Battle/SkillEffect/HealEffect.cs)·[기절](코드샘플/메타토이드래곤즈/유니티/Battle/SkillEffect/StunEffect.cs) 구현 순서로 볼 수 있습니다.
- **온라인 전투 검증:** 고정 시드, Fixed Update, 고정 소수점 연산과 서버 재시뮬레이션으로 전투 결과를 검증했습니다. [전투 상태 로직](코드샘플/메타토이드래곤즈/유니티/Battle/BattleStateLogic.cs)과 [시드·로그 처리](코드샘플/메타토이드래곤즈/유니티/Battle/ChampionLogManager.cs)를 공개했습니다.
- **운영 UI:** Google Sheets의 기획 데이터를 빌드에 자동 반영하고, React 인게임 WebView로 아웃게임 화면을 앱 재배포 없이 수정할 수 있게 했습니다. [Unity WebView 컨트롤러](코드샘플/메타토이드래곤즈/유니티/WebView/SBWebViewController.cs)와 [React 컴포넌트](코드샘플/메타토이드래곤즈/인게임웹뷰_리액트/components)를 확인할 수 있습니다.
- **리소스 업데이트:** CDN으로 내려받는 번들의 목록과 버전을 관리했습니다. [AssetBundle 관리자](코드샘플/메타토이드래곤즈/유니티/bundle/AssetBundleManager.cs).

[Unity 코드 전체 보기](코드샘플/메타토이드래곤즈/유니티) · [Cocos Creator 코드 보기](코드샘플/메타토이드래곤즈/코코스크리에이터/Scripts)

**플레이 영상 — 미리보기를 누르면 YouTube에서 열립니다.**

[<img src="https://img.youtube.com/vi/O6LPHZbqoA8/0.jpg" alt="메타 토이 드래곤즈 사가 영상 1 미리보기" width="300"/>](https://www.youtube.com/watch?v=O6LPHZbqoA8)
[<img src="https://img.youtube.com/vi/ocvZkbXv6hI/0.jpg" alt="메타 토이 드래곤즈 사가 영상 2 미리보기" width="300"/>](https://www.youtube.com/watch?v=ocvZkbXv6hI)

## 2. 양어장 고양이

**Unity FMV 캐주얼 게임 · 샌드박스네트워크 · 1인 개발**

기획·개발·QA·배포를 혼자 맡아 착수 후 3개월 만에 출시했습니다. 영상 진행과 수집·도감·요리·미니게임을 하나의 흐름으로 연결하고, 모바일 화면 비율에 맞는 UI를 구현했습니다.

- **다중 해상도 UI:** RectTransform의 앵커와 Stretch 설정에 맞춰 요소 크기를 조정했습니다. [UISizeController](코드샘플/양어장고양이/유니티/UI/UISizeController.cs)를 보면 해상도 변화에 따른 처리 방식을 확인할 수 있습니다.
- **UGUI 컴포넌트:** [CutoutMask](코드샘플/양어장고양이/유니티/CutoutMaskUI.cs), [ImageBlur](코드샘플/양어장고양이/유니티/ImageBlur.cs), [ScrollRect 중심 이동](코드샘플/양어장고양이/유니티/Tool/ScrollToCenter.cs)을 구현했습니다.
- **대용량 콘텐츠:** 2GB 이상 영상 리소스를 Play Asset Delivery와 Addressables로 기본 앱과 분리해 배포했습니다.

[Unity 코드 전체 보기](코드샘플/양어장고양이/유니티) · [PHP 코드 보기](코드샘플/양어장고양이/PHP샘플/script)

**플레이 영상**

[<img src="https://img.youtube.com/vi/CUUZ9LrLLco/0.jpg" alt="양어장 고양이 영상 1 미리보기" width="300"/>](https://www.youtube.com/watch?v=CUUZ9LrLLco)
[<img src="https://img.youtube.com/vi/3YrTkEd3PZ4/0.jpg" alt="양어장 고양이 영상 2 미리보기" width="300"/>](https://www.youtube.com/watch?v=3YrTkEd3PZ4)

## 3. 공포의 술래잡기

**Unity 비대칭 실시간 PvP · 샌드박스네트워크 · PM·디렉터 및 메인 클라이언트 개발**

6개월간 개발 방향이 정리되지 못했던 프로젝트에 합류해 기획·아트·개발의 작업 흐름을 재정비했습니다. 이후 6개월 만에 출시했고 DAU 3만 규모로 약 1년간 운영했습니다.

- **실시간 플레이:** 소켓 서버 연동, 플레이어 동기화, 전투 판정과 캐릭터 스킬을 구현했습니다. [플레이어 스킬](코드샘플/공포의술래잡기/유니티/PlayerController/PlayerControllerSkill.cs)과 [스킬 패킷](코드샘플/공포의술래잡기/유니티/Network/SB/SBSocketSharedLib/Packet/MessagePack/Client/CSSkillCasting.cs)을 연결해 볼 수 있습니다.
- **시야와 연출:** [시야각 처리](코드샘플/공포의술래잡기/유니티/Fov/SBFieldOfView.cs)와 [시야 렌더링](코드샘플/공포의술래잡기/유니티/Fov/SBFieldOfRender.cs)을 구현했습니다.
- **성능과 반복 작업:** [오브젝트 풀](코드샘플/공포의술래잡기/유니티/Managers/Core/PoolManager.cs)로 반복 생성 객체를 재사용했습니다. 기획 데이터 자동 반영과 에셋 빌드, Unity 에디터 도구도 개발해 변경 사항을 확인하는 시간을 줄였습니다.

[Unity 코드 전체 보기](코드샘플/공포의술래잡기/유니티)

**플레이 영상**

[<img src="https://img.youtube.com/vi/I2k832B3NTU/0.jpg" alt="공포의 술래잡기 영상 1 미리보기" width="300"/>](https://www.youtube.com/watch?v=I2k832B3NTU&list=PLxlA7knZ2zb6_Tdz8YZTL-bDCJlo2Fu4Y)
[<img src="https://img.youtube.com/vi/4zYNsM1SnWI/0.jpg" alt="공포의 술래잡기 영상 2 미리보기" width="300"/>](https://www.youtube.com/watch?v=4zYNsM1SnWI)

## 4. 공통 Unity SDK와 라이브 운영 도구

**샌드박스네트워크 · Unity/C# · Cocos Creator · Android/iOS**

여러 게임이 공통으로 쓰는 OAuth 2.0 계정 인증, 소셜·채팅·광고·결제·지표 기능을 SDK로 묶고 스토어 요구사항에 대응했습니다. 운영 데이터와 도구를 연결해 프로젝트마다 같은 작업을 반복하지 않도록 했습니다.

- [Unity WebView 인터페이스](코드샘플/계정관리SDK/유니티/Scripts/SamandaWebview.cs)와 [모바일 구현](코드샘플/계정관리SDK/유니티/Scripts/mobile/SamandaWebviewMobile.cs): Unity와 웹 UI의 연결 지점.
- [Cocos Creator 계정 화면](코드샘플/계정관리SDK/웹_코코스크리에이터/Account/AccountManager.js)과 [네트워크 응답 처리](코드샘플/계정관리SDK/웹_코코스크리에이터/ResponseHandler.js): 웹 화면의 계정·통신 흐름.
- [iOS/Android 빌드 후처리](코드샘플/계정관리SDK/유니티/Editor/SamandaPostBuild.cs): 플랫폼별 SDK 설정 예시.

Jenkins CI에는 버전·빌드·기획 데이터·아트 리소스 재가공을 통합해 iOS/Android 동시 빌드 시간을 90분에서 20분으로 줄였습니다. 설날·추석처럼 변경이 잦은 이벤트 화면은 인앱 WebView와 Cocos Creator·React를 활용해 기획·사업 부서의 요구를 앱 재배포 없이 반영했습니다. 서버 API와 DB 데이터 구조, 통신 프로토콜이 필요한 경우에는 프로토타입으로 먼저 검증해 담당자와 구현 기준을 맞췄습니다.

## 다른 출시·서비스 경험

| 프로젝트 | 담당 범위 |
| --- | --- |
| 드래곤빌리지·드래곤빌리지2·드래곤빌리지W | 3인 체제에서 PHP 서버와 Cocos2d-x/C++ 클라이언트, 기획·운영을 함께 담당. 이벤트 UI·상품 지급 도구와 2주 단위 업데이트 지원 |
| 졸리시티 | Cocos2d-x/C++ 클라이언트 리드. TMX 심리스 월드·에디터, Spine 사전 베이크와 아틀라스·팔레트 스왑으로 서로 다른 캐릭터 3,000개 동시 표시 |
| Phoenix Darts | Linux/C++ 글로벌 아케이드 서비스 UI 개편과 일부 플랫폼의 Unity 전환, WebRTC 영상·소켓 대전 구현 |

프로파일러로 프레임 드랍과 드로우콜 병목을 찾고, 원인에 따라 오브젝트 풀링·증분 로딩·아틀라스 그룹화 전략으로 개선해 왔습니다.

## 별도 실험 | AI를 활용한 개발 워크플로우

상용 게임 개발 사례와 구분해 개인 실험을 모았습니다. 개발 속도와 에디터 자동화의 가능성·한계를 확인한 작업입니다.

- [Cocos Creator 자동화 파이프라인](AI%20-%20R%26D/완전자동화): 기획·디자인·개발·QA 에이전트를 반복 실행한 실험과 결과물.
- [Unity MCP 하루 개발](AI%20-%20R%26D/unity_mcp%20활용%201일개발): 에디터 조작과 코드 생성으로 만든 러닝 게임.
- [Cocos Creator MCP 하루 개발](AI%20-%20R%26D/cocos_mcp%20활용%201일개발): 타워 디펜스 게임과 [웹 플레이](https://yonguenp.github.io/Portfolio/AI%20-%20R%26D/cocos_mcp%20%ED%99%9C%EC%9A%A9%201%EC%9D%BC%EA%B0%9C%EB%B0%9C/build/web-desktop/).
- [Unity 개인 프로토타입](unitywithclaude): 아이디어와 AI 보조 개발을 시험한 미니게임 모음 · [웹 빌드 플레이](https://yonguenp.github.io/Portfolio/unitywithclaude/).

## 다른 프로젝트 영상

아래 이미지는 영상 미리보기입니다. 클릭하면 해당 영상이 열립니다.

### 옐언니 옷입히기 · 샌드박스네트워크

크리에이터 IP 기반 패션 드레스업 모바일 게임.

[<img src="https://img.youtube.com/vi/rt0VsQ2QuH4/0.jpg" alt="옐언니 옷입히기 영상 미리보기" width="300"/>](https://www.youtube.com/watch?v=rt0VsQ2QuH4)

### 셀프어쿠스틱 시리즈 · 샌드박스네트워크

네일샵·헤어샵·캠핑장 등 크리에이터 IP를 활용한 힐링 시뮬레이션 게임.

[<img src="https://img.youtube.com/vi/rJ9k5k8SLyM/0.jpg" alt="셀프어쿠스틱 영상 1 미리보기" width="180"/>](https://www.youtube.com/watch?v=rJ9k5k8SLyM&list=PLxlA7knZ2zb76LeUBEk4kU579NzmgknvV)
[<img src="https://img.youtube.com/vi/CPqEhyBWdBg/0.jpg" alt="셀프어쿠스틱 영상 2 미리보기" width="180"/>](https://www.youtube.com/watch?v=CPqEhyBWdBg)
[<img src="https://img.youtube.com/vi/C1CYlrEDeR0/0.jpg" alt="셀프어쿠스틱 영상 3 미리보기" width="180"/>](https://www.youtube.com/watch?v=C1CYlrEDeR0)
[<img src="https://img.youtube.com/vi/hGcn45izybs/0.jpg" alt="셀프어쿠스틱 영상 4 미리보기" width="180"/>](https://www.youtube.com/watch?v=hGcn45izybs)
[<img src="https://img.youtube.com/vi/wM13fk0lVtA/0.jpg" alt="셀프어쿠스틱 영상 5 미리보기" width="180"/>](https://www.youtube.com/watch?v=wM13fk0lVtA)
[<img src="https://img.youtube.com/vi/0kenMfcXZGU/0.jpg" alt="셀프어쿠스틱 영상 6 미리보기" width="180"/>](https://www.youtube.com/watch?v=0kenMfcXZGU)

### 드래곤빌리지 · 하이브로

드래곤을 수집·합성·육성하는 모바일 RPG.

[<img src="https://img.youtube.com/vi/k6c1Yv_GXN0/0.jpg" alt="드래곤빌리지 영상 미리보기" width="300"/>](https://www.youtube.com/watch?v=k6c1Yv_GXN0)

### 지하철이야기 · 하이브로

서울을 배경으로 한 픽셀 타일맵 심리스 오픈월드 MMORPG.

[<img src="https://img.youtube.com/vi/UtUfj-K9B1U/0.jpg" alt="지하철이야기 영상 미리보기" width="300"/>](https://www.youtube.com/watch?v=UtUfj-K9B1U)

### Phoenix Darts · 홍인터내셔날

글로벌 아케이드 서비스 VSS(Virtual Sport System) UI 리뉴얼 프로젝트.

[<img src="https://img.youtube.com/vi/4jQgMthDDQ8/0.jpg" alt="Phoenix Darts 영상 미리보기" width="300"/>](https://www.youtube.com/watch?v=4jQgMthDDQ8)
