---

# Portfolio

게임 클라이언트 개발자 박용근의 포트폴리오입니다.
Unity, C++, Cocos2d-x 기반의 클라이언트 개발 경력 11년, 팀 리딩 및 라이브 서비스 운영 경험을 보유하고 있습니다.

---

## 주요 개발 프로젝트 샘플 코드

### 메타토이드래곤즈사가 (샌드박스네트워크) | 수집형 RPG | DAU 최대 10만
고정 랜덤 시드(Seed), Fixed Update, 고정 소수점 연산 기반의 결정론적 동기화 로직으로 웹 서버에서 실시간 대전 토너먼트 시스템을 구현한 프로젝트입니다.
- [유니티 코드 샘플](https://github.com/yonguenp/Portfolio/tree/main/%EC%BD%94%EB%93%9C%EC%83%98%ED%94%8C/%EB%A9%94%ED%83%80%ED%86%A0%EC%9D%B4%EB%93%9C%EB%9E%98%EA%B3%A4%EC%A6%88/%EC%9C%A0%EB%8B%88%ED%8B%B0) — 결정론적 동기화 및 토너먼트 시스템 로직
- [웹 리액트 코드 샘플](https://github.com/yonguenp/Portfolio/tree/main/%EC%BD%94%EB%93%9C%EC%83%98%ED%94%8C/%EB%A9%94%ED%83%80%ED%86%A0%EC%9D%B4%EB%93%9C%EB%9E%98%EA%B3%A4%EC%A6%88/%EC%9D%B8%EA%B2%8C%EC%9E%84%EC%9B%B9%EB%B7%B0_%EB%A6%AC%EC%95%A1%ED%8A%B8/components) — 인게임 웹뷰 React 컴포넌트
- [코코스크리에이터 코드 샘플](https://github.com/yonguenp/Portfolio/tree/main/%EC%BD%94%EB%93%9C%EC%83%98%ED%94%8C/%EB%A9%94%ED%83%80%ED%86%A0%EC%9D%B4%EB%93%9C%EB%9E%98%EA%B3%A4%EC%A6%88/%EC%BD%94%EC%BD%94%EC%8A%A4%ED%81%AC%EB%A6%AC%EC%97%90%EC%9D%B4%ED%84%B0/Scripts) — Cocos Creator 기반 게임 로직

### 공포의 술래잡기 (샌드박스네트워크) | 실시간 PvP | DAU 최대 3만
C# 기반 자체 서버 연동 및 실시간 전투, 연출, 배포 시스템 전반을 설계한 프로젝트입니다. PM, 디렉터를 겸임하며 1년간 라이브 서비스를 운영했습니다.
- [유니티 코드 샘플](https://github.com/yonguenp/Portfolio/tree/main/%EC%BD%94%EB%93%9C%EC%83%98%ED%94%8C/%EA%B3%B5%ED%8F%AC%EC%9D%98%EC%88%A0%EB%9E%98%EC%9E%A1%EA%B8%B0/%EC%9C%A0%EB%8B%88%ED%8B%B0) — 실시간 PvP 전투 및 서버 연동 로직

### 양어장 고양이 (샌드박스네트워크) | FMV 장르 | DAU 최대 1만
Play Asset Delivery(PAD)를 활용해 2GB 이상의 고용량 영상 리소스를 외부 CDN 비용 없이 서비스한 프로젝트입니다.
- [유니티 코드 샘플](https://github.com/yonguenp/Portfolio/tree/main/%EC%BD%94%EB%93%9C%EC%83%98%ED%94%8C/%EC%96%91%EC%96%B4%EC%9E%A5%EA%B3%A0%EC%96%91%EC%9D%B4/%EC%9C%A0%EB%8B%88%ED%8B%B0) — PAD 기반 대용량 리소스 처리 로직
- [PHP 코드 샘플](https://github.com/yonguenp/Portfolio/tree/main/%EC%BD%94%EB%93%9C%EC%83%98%ED%94%8C/%EC%96%91%EC%96%B4%EC%9E%A5%EA%B3%A0%EC%96%91%EC%9D%B4/PHP%EC%83%98%ED%94%8C/script) — 서버 사이드 로직

### 계정관리 SDK (샌드박스네트워크)
회원가입, 소셜(채팅/친구), 광고, 결제, 지표 추적 모듈을 통합한 범용 Unity SDK입니다. 웹뷰 기반 프로토콜 설계로 앱 업데이트 없이 아웃게임 시스템을 유연하게 변경할 수 있는 구조를 구현했습니다.
- [유니티 코드 샘플](https://github.com/yonguenp/Portfolio/tree/main/%EC%BD%94%EB%93%9C%EC%83%98%ED%94%8C/%EA%B3%84%EC%A0%95%EA%B4%80%EB%A6%ACSDK/%EC%9C%A0%EB%8B%88%ED%8B%B0) — SDK 핵심 모듈 구현
- [웹 코코스크리에이터 코드 샘플](https://github.com/yonguenp/Portfolio/tree/main/%EC%BD%94%EB%93%9C%EC%83%98%ED%94%8C/%EA%B3%84%EC%A0%95%EA%B4%80%EB%A6%ACSDK/%EC%9B%B9_%EC%BD%94%EC%BD%94%EC%8A%A4%ED%81%AC%EB%A6%AC%EC%97%90%EC%9D%B4%ED%84%B0) — 웹뷰 연동 프로토콜 구현

---

## AI R&D 샘플

생성형 AI를 개발 워크플로우에 접목하는 실험적 프로젝트입니다.

### Unity MCP + Claude Code — 러닝 게임 1일 개발
Unity MCP와 Claude Code를 활용한 바이브 코딩으로 하루 만에 러닝 게임을 완성한 실험입니다. AI 기반 개발 보조 도구가 실제 게임 개발 속도에 미치는 영향을 검증했습니다.
- [코드 및 결과물 보기](https://github.com/yonguenp/Portfolio/tree/main/AI%20-%20R%26D/unity_mcp%20%ED%99%9C%EC%9A%A9%201%EC%9D%BC%EA%B0%9C%EB%B0%9C)

### Cocos Creator MCP + Claude Code — 타워 디펜스 게임 1일 개발
Cocos Creator MCP와 Claude Code를 활용해 하루 만에 타워 디펜스 게임을 제작한 실험입니다. 빌드 및 배포 자동화까지 포함하여 AI 보조 개발 파이프라인 전체를 검증했습니다.
- [코드 및 결과물 보기](https://github.com/yonguenp/Portfolio/tree/main/AI%20-%20R%26D/cocos_mcp%20%ED%99%9C%EC%9A%A9%201%EC%9D%BC%EA%B0%9C%EB%B0%9C)
- [빌드 및 배포 자동화 샘플 (웹 플레이)](https://yonguenp.github.io/Portfolio/AI%20-%20R%26D/cocos_mcp%20%ED%99%9C%EC%9A%A9%201%EC%9D%BC%EA%B0%9C%EB%B0%9C/build/web-desktop/)

---

## 주요 개발 프로젝트 영상 링크

### 메타토이드래곤즈사가
- [트레일러 1](https://www.youtube.com/watch?v=O6LPHZbqoA8)
- [트레일러 2](https://www.youtube.com/watch?v=ocvZkbXv6hI)

### 양어장 고양이
- [소개 영상](https://www.youtube.com/watch?v=CUUZ9LrLLco)
- [플레이 영상](https://www.youtube.com/watch?v=3YrTkEd3PZ4)

### 공포의 술래잡기
- [소개 영상](https://www.youtube.com/watch?v=I2k832B3NTU&list=PLxlA7knZ2zb6_Tdz8YZTL-bDCJlo2Fu4Y)
- [유저 플레이 영상](https://www.youtube.com/watch?v=4zYNsM1SnWI)

---
