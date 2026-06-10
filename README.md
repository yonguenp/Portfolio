---

# 박용근 — 게임 클라이언트 개발자 포트폴리오

게임 클라이언트 개발자 박용근의 포트폴리오입니다.<br>
Unity · C++ · Cocos2d-x 기반 클라이언트 개발 경력 **11년**, 팀 리딩 및 라이브 서비스 운영 경험을 보유하고 있습니다.

| | |
|---|---|
| **주요 기술** | Unity (C#) · Cocos Creator (TypeScript) · Cocos2d-x (C++) · React · PHP · MySQL · Android (Java) |
| **경력** | 샌드박스네트워크 · 게임 클라이언트 개발 11년 |
| **역할** | 게임 클라이언트 개발 · SDK · 서버 연동 · 웹페이지 · PHP서버 개발 · 인게임 지표 추적 |
| **연락처** | yonguen@naver.com |

---

## 목차

1. [주요 개발 프로젝트](#주요-개발-프로젝트)
   - [메타토이드래곤즈사가](#1-메타토이드래곤즈사가--수집형-rpg--다운로드-수-100만--동시접속자-10만)
   - [공포의 술래잡기](#2-공포의-술래잡기--실시간-pvp--다운로드-수-50만--동시접속자-5만)
   - [양어장 고양이](#3-양어장-고양이--fmv-장르--다운로드-수-50만--동시접속자-최대-3만)
   - [계정관리 SDK](#4-계정관리-sdk)
2. [AI R&D 실험 프로젝트](#ai-rd-실험-프로젝트)
   - [AI 멀티에이전트 완전 자동화 게임 개발 파이프라인](#1-ai-멀티에이전트-완전-자동화-게임-개발-파이프라인)
   - [Unity MCP — 러닝 게임 1일 개발](#2-unity-mcp--claude-code--러닝-게임-1일-개발)
   - [Cocos Creator MCP — 타워 디펜스 1일 개발](#3-cocos-creator-mcp--claude-code--타워-디펜스-게임-1일-개발)
3. [영상 링크](#영상-링크)

---

## 주요 개발 프로젝트

### 1. 메타토이드래곤즈사가 | 수집형 RPG | 다운로드 수 100만 | 동시접속자 10만

> 샌드박스네트워크 | Unity · Cocos Creator · React

타운에 건물을 짓고 자원을 생산·증축하며, 다양한 드래곤을 수집하고 실시간 대전 토너먼트에서 겨루는 수집형 RPG입니다.<br>
클라이언트 리드로 참여하여 1년간 라이브 서비스를 리딩했습니다.

**핵심 기술 포인트**

| 기술 | 상세 |
|---|---|
| **결정론적 동기화** | 고정 랜덤 시드(Seed) + Fixed Update + 고정 소수점 연산으로 클라이언트·서버 간 완전 동일한 전투 결과 보장 |
| **토너먼트 시스템** | 웹 서버의 랜덤 시드만으로 전투를 시뮬레이션 → 부정 조작 원천 차단 |
| **CDN 리소스 관리** | 서버에서 관리되는 데이터/리소스 버전관리로 스토어 앱 업데이트 최소화 |
| **전투,성장 시스템** | 구글시트로 관리되는 기획 디자인 데이터로 전투 로직을 구성하여 유기적인 전투,성장 밸런싱 |
| **인게임 웹뷰** | React 컴포넌트 기반 웹뷰로 앱 업데이트 없이 UI 변경 가능한 하이브리드 아키텍처 |

**코드 샘플**
- [Unity 코드 샘플](https://github.com/yonguenp/Portfolio/tree/main/%EC%BD%94%EB%93%9C%EC%83%98%ED%94%8C/%EB%A9%94%ED%83%80%ED%86%A0%EC%9D%B4%EB%93%9C%EB%9E%98%EA%B3%A4%EC%A6%88/%EC%9C%A0%EB%8B%88%ED%8B%B0) — 결정론적 동기화, 토너먼트, 배틀, 소켓 네트워크, 가챠, 길드, 월드보스 등
- [웹 React 코드 샘플](https://github.com/yonguenp/Portfolio/tree/main/%EC%BD%94%EB%93%9C%EC%83%98%ED%94%8C/%EB%A9%94%ED%83%80%ED%86%A0%EC%9D%B4%EB%93%9C%EB%9E%98%EA%B3%A4%EC%A6%88/%EC%9D%B8%EA%B2%8C%EC%9E%84%EC%9B%B9%EB%B7%B0_%EB%A6%AC%EC%95%A1%ED%8A%B8/components) — 인게임 웹뷰 React 컴포넌트
- [Cocos Creator 코드 샘플](https://github.com/yonguenp/Portfolio/tree/main/%EC%BD%94%EB%93%9C%EC%83%98%ED%94%8C/%EB%A9%94%ED%83%80%ED%86%A0%EC%9D%B4%EB%93%9C%EB%9E%98%EA%B3%A4%EC%A6%88/%EC%BD%94%EC%BD%94%EC%8A%A4%ED%81%AC%EB%A6%AC%EC%97%90%EC%9D%B4%ED%84%B0/Scripts) — Cocos Creator 기반 게임 로직

---

### 2. 공포의 술래잡기 | 실시간 PvP | 다운로드 수 50만 | 동시접속자 5만

> 샌드박스네트워크 | Unity · C# 자체 서버 | PM·디렉터 겸임

**실시간 PvP 전투**와 **라이브 서비스 운영 전반**을 담당한 프로젝트입니다.<br>
클라이언트 개발을 넘어 PM·디렉터를 겸임하며 **1년간 서비스를 리딩**했습니다.

**핵심 기술 포인트**

| 기술 | 상세 |
|---|---|
| **실시간 PvP** | C# 소켓 기반 자체 서버 연동, 실시간 플레이어 위치 동기화 및 전투 판정 |
| **FOV 시스템** | 술래와 도망자의 시야각(Field of View)을 코드로 구현하여 긴장감 있는 PvP 연출 |
| **연출 시스템** | 이펙트, 사운드, 카메라 연출 등 인게임 피드백 시스템 설계 |
| **배포 자동화** | 빌드·배포 파이프라인 구축, 라이브 서비스 중 무중단 업데이트 운영 |
| **PM·디렉터 겸임** | 기획·개발·QA·운영 프로세스 총괄, 1년간 DAU 3만 규모 라이브 서비스 운영 |

**코드 샘플**
- [Unity 코드 샘플](https://github.com/yonguenp/Portfolio/tree/main/%EC%BD%94%EB%93%9C%EC%83%98%ED%94%8C/%EA%B3%B5%ED%8F%AC%EC%9D%98%EC%88%A0%EB%9E%98%EC%9E%A1%EA%B8%B0/%EC%9C%A0%EB%8B%88%ED%8B%B0) — 실시간 PvP 전투, FOV, 이벤트, 서버 연동 로직

---

### 3. 양어장 고양이 | FMV 장르 | 다운로드 수 50만 | 동시접속자 최대 3만

> 샌드박스네트워크 | Unity · PHP

**FMV(Full Motion Video) 장르**의 모바일 게임으로, 핵심 과제는 **2GB 이상의 고용량 영상 리소스를 CDN 비용 없이 서비스**하는 것이었습니다.
2개월 정도의 개발기간으로 런칭한 프로젝트입니다.

**핵심 기술 포인트**

| 기술 | 상세 |
|---|---|
| **Play Asset Delivery (PAD)** | Google Play의 PAD를 활용해 2GB+ 영상 리소스를 외부 CDN 없이 안정적으로 배포, 인프라 비용 절감 |
| **대용량 리소스 관리** | 앱 설치 후 필요 시점에 On-demand 방식으로 리소스 다운로드, 초기 앱 용량 최소화 |
| **서버 연동** | PHP 서버 사이드 로직으로 콘텐츠 잠금/해제, 진행도 저장, 결제 연동 구현 |
| **UGUI 커스텀 컴포넌트** |	ScrollRect 기반 동적 리스트, RectTransform 애니메이션, CutoutMask·ImageBlur 등 커스텀 UI 셰이더 컴포넌트 직접 구현
| **Addressable Asset System** |	Unity Addressables로 리소스를 원격 관리 — 앱 재배포 없이 에셋 교체·추가 가능하며 PAD와 연동하여 On-demand 다운로드 구현
| **다양한 콘텐츠 시스템** | 고양이 수집, 카드 도감, 요리, 미니게임, 상점, 랭킹 등 복합 콘텐츠 구조 |

**코드 샘플**
- [Unity 코드 샘플](https://github.com/yonguenp/Portfolio/tree/main/%EC%BD%94%EB%93%9C%EC%83%98%ED%94%8C/%EC%96%91%EC%96%B4%EC%9E%A5%EA%B3%A0%EC%96%91%EC%9D%B4/%EC%9C%A0%EB%8B%88%ED%8B%B0) — PAD 리소스 처리, 콘텐츠 시스템, UI 로직
- [PHP 코드 샘플](https://github.com/yonguenp/Portfolio/tree/main/%EC%BD%94%EB%93%9C%EC%83%98%ED%94%8C/%EC%96%91%EC%96%B4%EC%9E%A5%EA%B3%A0%EC%96%91%EC%9D%B4/PHP%EC%83%98%ED%94%8C/script) — 서버 사이드 로직

---

### 4. 계정관리 SDK

> 샌드박스네트워크 | Unity · Android (Java) · Cocos Creator (JavaScript)

**사내 게임 전체에서 공통으로 사용하는 범용 Unity SDK**입니다.<br>
회원가입부터 소셜, 광고, 결제, 지표 추적까지 아웃게임 전 기능을 단일 SDK로 통합했습니다.

**핵심 기술 포인트**

| 기술 | 상세 |
|---|---|
| **웹뷰 기반 프로토콜 설계** | 아웃게임 UI를 웹뷰로 분리 → **앱 업데이트 없이** 로그인·결제·채팅 화면 실시간 변경 가능 |
| **멀티 플랫폼 통합** | Unity C# + Android 네이티브(Java) + Cocos Creator(JS) 동시 지원 |
| **소셜 기능** | 채팅(1:1/단체방), 친구, 프로필, 오버레이 채팅 등 완전한 소셜 레이어 구현 |
| **결제·광고 통합** | Google Play 결제, 소셜 로그인(Google 등), 광고 SDK 통합 |
| **지표 추적** | 유저 행동 이벤트 트래킹 모듈로 마케팅·분석 데이터 자동 수집 |

**코드 샘플**
- [Unity 코드 샘플](https://github.com/yonguenp/Portfolio/tree/main/%EC%BD%94%EB%93%9C%EC%83%98%ED%94%8C/%EA%B3%84%EC%A0%95%EA%B4%80%EB%A6%ACSDK/%EC%9C%A0%EB%8B%88%ED%8B%B0) — SDK 핵심 모듈, 결제·광고·소셜 연동
- [웹 Cocos Creator 코드 샘플](https://github.com/yonguenp/Portfolio/tree/main/%EC%BD%94%EB%93%9C%EC%83%98%ED%94%8C/%EA%B3%84%EC%A0%95%EA%B4%80%EB%A6%ACSDK/%EC%9B%B9_%EC%BD%94%EC%BD%94%EC%8A%A4%ED%81%AC%EB%A6%AC%EC%97%90%EC%9D%B4%ED%84%B0) — 웹뷰 연동 프로토콜, 채팅·계정 UI

---

## AI R&D 실험 프로젝트

생성형 AI를 게임 개발 워크플로우에 접목하는 실험적 프로젝트들입니다.<br>
"AI가 개발 속도와 품질에 실제로 얼마나 기여할 수 있는가"를 직접 검증하는 것을 목표로 합니다.

---

### 1. AI 멀티에이전트 완전 자동화 게임 개발 파이프라인

> Claude Code · Cocos Creator 3 · TypeScript

**기획 → 디자인 → 개발 → QA** 전 단계를 AI 에이전트가 자율적으로 수행하는 **완전 자동화 파이프라인** 실험입니다.<br>
인간 개입 없이 반복 iteration을 돌며 게임 품질을 자동으로 끌어올리는 것을 목표로 했습니다.

**파이프라인 구조**

```
기획봇 (spec 작성)
  └→ 디자인봇 (SVG 에셋 생성)
       └→ 개발봇 (TypeScript 코드 구현)
            └→ QA봇 (코드·씬 검증, 점수 산출, 다음 iteration 피드백)
                 └→ 기획봇 (spec 업데이트) → 반복
```

**실험 결과 (Star Sweeper — 별자리 캐주얼 게임)**

| 지표 | 수치 |
|---|---|
| 완료 iteration | 4회 |
| QA 종합 점수 (최종) | **9.0 / 10** |
| 기획 충족률 | **96.7%** (29/30 요구사항) |
| TypeScript 문법 오류 | **0건** |
| 자동 생성 스크립트 수 | 18개 |
| 자동 생성 씬 파일 수 | 4개 |

**기술적 특징**
- QA봇이 씬 파일의 `__id__` 참조 정합성까지 전수 검사하는 정밀한 자동화 QA
- 이전 iteration 이슈를 다음 iteration에 반영하는 **자기개선(self-improving) 루프**
- 기획 명세(spec)·디자인 노트·개발 노트·QA 리포트가 모두 자동 생성·버전 관리됨

**한계 및 인사이트**
- 씬 파일 구조 재작성 등 **에디터 직접 조작**이 필요한 작업은 자동화 한계 존재
- AI 에이전트는 코드 로직은 잘 생성하나, 씬 JSON의 복잡한 참조 관계 유지는 취약
- 기획·개발·QA 역할 분리가 명확할수록 에이전트 간 협업 품질이 향상됨

- [파이프라인 코드 및 결과물 보기](https://github.com/yonguenp/Portfolio/tree/main/AI%20-%20R%26D/%EC%99%84%EC%A0%84%EC%9E%90%EB%8F%99%ED%99%94)

---

### 2. Unity MCP + Claude Code — 러닝 게임 1일 개발

> Unity MCP · Claude Code · C#

Unity MCP와 Claude Code를 활용한 **바이브 코딩(vibe coding)** 으로 하루 만에 러닝 게임을 완성한 실험입니다.<br>
AI가 Unity 에디터를 직접 제어하며 씬 구성·스크립트 작성·오브젝트 배치까지 수행했습니다.

**실험 목적**: AI 보조 도구가 실제 게임 개발 속도에 미치는 영향 검증

- [코드 및 결과물 보기](https://github.com/yonguenp/Portfolio/tree/main/AI%20-%20R%26D/unity_mcp%20%ED%99%9C%EC%9A%A9%201%EC%9D%BC%EA%B0%9C%EB%B0%9C)

---

### 3. Cocos Creator MCP + Claude Code — 타워 디펜스 게임 1일 개발

> Cocos Creator MCP · Claude Code · TypeScript

Cocos Creator MCP와 Claude Code를 활용해 하루 만에 타워 디펜스 게임을 제작한 실험입니다.<br>
게임 개발에 그치지 않고 **빌드·배포 자동화**까지 포함하여 AI 보조 개발 파이프라인 전체를 검증했습니다.

**구현 요소**: 타워 배치, 적 웨이브, 경로탐색(PathFinder), 발사체, 게임오버·메인메뉴 씬 전환, 웹 빌드 자동화

- [코드 및 결과물 보기](https://github.com/yonguenp/Portfolio/tree/main/AI%20-%20R%26D/cocos_mcp%20%ED%99%9C%EC%9A%A9%201%EC%9D%BC%EA%B0%9C%EB%B0%9C)
- [웹 플레이 (라이브 데모)](https://yonguenp.github.io/Portfolio/AI%20-%20R%26D/cocos_mcp%20%ED%99%9C%EC%9A%A9%201%EC%9D%BC%EA%B0%9C%EB%B0%9C/build/web-desktop/)

---

## 영상 링크

### 메타토이드래곤즈사가 - 샌드박스 네트워크
- [트레일러 1](https://www.youtube.com/watch?v=O6LPHZbqoA8)
- [트레일러 2](https://www.youtube.com/watch?v=ocvZkbXv6hI)

### 양어장 고양이 - 샌드박스 네트워크
- [소개 영상](https://www.youtube.com/watch?v=CUUZ9LrLLco)
- [플레이 영상](https://www.youtube.com/watch?v=3YrTkEd3PZ4)

### 공포의 술래잡기 - 샌드박스 네트워크
- [소개 영상](https://www.youtube.com/watch?v=I2k832B3NTU&list=PLxlA7knZ2zb6_Tdz8YZTL-bDCJlo2Fu4Y)
- [유저 플레이 영상](https://www.youtube.com/watch?v=4zYNsM1SnWI)

### 옐언니 옷입히기 - 샌드박스 네트워크
- [트레일러](https://www.youtube.com/watch?v=rt0VsQ2QuH4)

### 셀프어쿠스틱 시리즈 - 샌드박스 네트워크
- [네일샵](https://www.youtube.com/watch?v=rJ9k5k8SLyM&list=PLxlA7knZ2zb76LeUBEk4kU579NzmgknvV)
- [헤어샵](https://www.youtube.com/watch?v=CPqEhyBWdBg)
- [캠핑장](https://www.youtube.com/watch?v=C1CYlrEDeR0)
- [화장하기](https://www.youtube.com/watch?v=hGcn45izybs)
- [아이스크림가게](https://www.youtube.com/watch?v=wM13fk0lVtA)
- [핫도그가게](https://www.youtube.com/watch?v=0kenMfcXZGU)
  
### 드래곤 빌리지 - 하이브로
- [유저플레이영상](https://www.youtube.com/watch?v=k6c1Yv_GXN0)

### 지하철이야기 - 하이브로
- [유저 플레이 영상](https://www.youtube.com/watch?v=UtUfj-K9B1U)

### 드래곤 빌리지2 - 하이브로
- [트레일러](https://www.youtube.com/watch?v=kVn0Sc6vUQw)

### 드래곤 빌리지 - 하이브로
- [유저플레이영상](https://www.youtube.com/watch?v=k6c1Yv_GXN0)

### 피닉스다트 - 홍인터네셔날
- [VSS UI리뉴얼](https://www.youtube.com/watch?v=4jQgMthDDQ8)

---
