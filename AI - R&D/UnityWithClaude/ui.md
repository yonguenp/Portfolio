# 고스톱 게임 UI 리디자인 및 Unity GameObject · Prefab 구조 개편 통합 기획서

## 1. 프로젝트 목적

현재 고스톱 게임은 기능적으로 동작하지만 UI 디자인과 Unity 프로젝트 구조 측면에서 다음과 같은 문제가 있다.

### UI 디자인 문제

* 플레이어 정보창마다 색상과 스타일이 일관되지 않음
* 청록색, 노란색, 빨간색, 초록색, 보라색 등 강조색이 과도하게 사용됨
* 화투패 자체가 강한 시각 요소인데 UI까지 강한 색상을 사용하여 화면이 복잡함
* 버튼마다 색상과 스타일이 달라 중요도 체계가 불명확함
* 게임 화면과 승리/결과 모달의 디자인 언어가 서로 다름
* 패널, 버튼, 카드의 테두리/그림자/Radius가 통일되어 있지 않음
* 전체적으로 하나의 게임이라기보다 서로 다른 UI 요소를 조합한 느낌이 강함

### Unity 구조 문제

* 일부 UI/GameObject가 코드에서 런타임으로 생성됨
* UI의 위치와 크기가 코드에 하드코딩되어 있음
* UI 디자인을 수정하려면 C# 코드를 직접 수정해야 하는 경우가 있음
* 반복적으로 사용되는 UI가 Prefab으로 분리되어 있지 않음
* Scene Hierarchy에서 게임 화면의 구조를 직관적으로 확인하기 어려움
* 디자이너/개발자가 Unity Editor에서 직접 수정하기 어려움

---

# 2. 이번 작업의 핵심 목표

이번 작업은 단순한 UI 색상 변경이 아니다.

다음 두 가지를 동시에 달성한다.

## 목표 A — 하나의 디자인 시스템으로 UI 통일

현재 게임의 레이아웃과 게임 플레이 구조는 최대한 유지하면서,

> "전통적인 화투의 느낌 + 현대적인 게임 UI"

를 하나의 디자인 언어로 통일한다.

---

## 목표 B — Unity Editor 중심의 수정 가능한 구조

게임의 시각적 요소는 가능한 한 Unity의 GameObject와 Prefab으로 구성한다.

> C# = 게임의 데이터와 동작
> Unity GameObject / Prefab = 화면에 보이는 것과 레이아웃

이라는 구조를 만든다.

최종적으로 개발자가 코드를 수정하지 않고도 Unity Editor에서 다음 작업을 할 수 있어야 한다.

* UI 위치 변경
* UI 크기 변경
* 색상 변경
* 폰트 변경
* 이미지 변경
* 버튼 크기 변경
* 카드 간격 변경
* 패널 크기 변경
* 카드 선택 효과 변경
* 모달 크기 변경
* UI 배치 변경
* Prefab 수정 및 재사용

---

# 3. 절대 유지할 것

이번 작업은 게임을 새로 만드는 작업이 아니다.

다음 요소는 기존 기능을 최대한 유지한다.

* 게임 규칙
* 점수 계산
* 턴 시스템
* 카드 데이터
* 카드 이미지
* 게임 상태 관리
* 승패 로직
* 플레이어 수
* 카드 종류
* 게임 진행 방식

특히 **UI 리디자인을 위해 게임 로직을 불필요하게 수정하지 않는다.**

---

# 4. 현재 게임 레이아웃 유지

현재 게임의 전체적인 배치는 기본적으로 유지한다.

```text
┌────────────────────────────────────────────┐
│                 AI-B                       │
│                                            │
│ AI-C                    중앙 게임     AI-A │
│                                            │
│                                            │
│                  중앙 카드                 │
│                                            │
│                내 플레이어                 │
│                                            │
│              [ 내 손패 ]                   │
└────────────────────────────────────────────┘
```

유지 대상:

* 상단 AI
* 좌측 AI
* 우측 AI
* 하단 플레이어
* 중앙 덱
* 중앙에 놓이는 화투
* 좌우 먹은 패
* 하단 손패

레이아웃을 완전히 새로 설계하지 않는다.

단, UI가 자연스럽게 보이도록 크기/간격/정렬은 필요한 범위에서 개선할 수 있다.

---

# 5. 디자인 컨셉

## Modern Traditional Go-Stop

전통적인 화투의 시각적 특징을 유지하면서 현대적인 게임 UI로 정리한다.

핵심 키워드:

* 전통적
* 고급스러움
* 차분함
* 명확한 정보 위계
* 낮은 시각적 피로도
* 화투 중심
* 게임판 중심

가장 중요한 디자인 원칙:

> **화투가 가장 강한 시각 요소가 되어야 한다.**

UI가 화투보다 더 튀어서는 안 된다.

---

# 6. 컬러 시스템

전체 UI는 아래 컬러 시스템을 기준으로 통일한다.

## Table

### Deep Green

```text
#24452F
```

용도:

* 게임 테이블
* 주요 게임 배경

---

### Dark Green

```text
#193523
```

용도:

* 중앙 게임 영역
* 먹은 패 영역
* 보조 영역
* 어두운 Surface

---

## Surface

### Warm Cream

```text
#F3EBDD
```

용도:

* 플레이어 정보 패널
* 모달
* 정보창
* 기본 UI Surface

---

### Cream White

```text
#FFFDF8
```

용도:

* 밝은 텍스트
* 카드 주변 영역
* 특별한 밝은 UI

---

## Accent

### Hwatu Red

```text
#C93A32
```

용도:

* 주요 액션
* 승리
* 중요한 상태
* 위험/경고
* 핵심 강조

Red는 과도하게 사용하지 않는다.

---

### Traditional Gold

```text
#D5A43A
```

용도:

* 현재 턴
* 선택 상태
* 보상
* 특별한 상태
* 중요한 보조 액션

---

## Text

### Primary

```text
#20251F
```

### Secondary

```text
#687066
```

### Light

```text
#FFFDF8
```

---

# 7. 컬러 사용 규칙

UI에서 임의의 새로운 색상을 추가하지 않는다.

기본적으로 다음 컬러 계열만 사용한다.

```text
Deep Green
Dark Green
Warm Cream
Cream White
Hwatu Red
Traditional Gold
Primary Text
Secondary Text
```

특히 현재 UI의 다음 색상 사용을 제거하거나 최소화한다.

* 청록색 플레이어 헤더
* 밝은 노란색 플레이어 패널
* 보라색 상태 표시
* 형광 초록 버튼
* 불필요한 파란색
* 순백색 대형 모달

색상은 장식이 아니라 정보의 의미를 전달하기 위한 수단으로 사용한다.

---

# 8. 색상의 의미

색상은 다음과 같은 의미를 갖는다.

```text
Green
= 게임판 / 안정 / 기본 영역

Cream
= 정보 / Surface / UI

Red
= 중요 / 승리 / 주요 액션

Gold
= 선택 / 현재 턴 / 보상 / 특별한 상태
```

플레이어마다 색상을 다르게 지정하지 않는다.

---

# 9. PlayerPanel 리디자인

현재 플레이어 정보창의 청록색 헤더와 플레이어별 다른 색상 패널을 제거한다.

모든 플레이어가 동일한 PlayerPanel 디자인을 사용한다.

## 구조

```text
PlayerPanel
├── Background
├── Header
│   ├── PlayerName
│   └── Money
├── Score
└── GoStopStatus
    ├── Gwang
    ├── Meong
    └── Pi
```

시각적으로는 다음과 같은 형태를 목표로 한다.

```text
┌──────────────────────────────┐
│ AI-C                  96,350원│
│──────────────────────────────│
│ 오고 0점       ○광 ○멍 ○피   │
└──────────────────────────────┘
```

### 기본 스타일

* Background: Warm Cream
* Text: Primary Text
* Border: Dark Green
* Player Name: Bold
* Money: Medium
* Score: Bold
* 상태 정보: 작은 아이콘/텍스트

---

# 10. 내 플레이어 강조

"나"만 별도의 노란색 패널을 사용하지 않는다.

기본 상태:

```text
Warm Cream
```

현재 턴:

```text
Gold Border
+
약한 Gold Glow
```

예:

```text
╔══════════════════════════════╗
║ 나                     99,800원║
║──────────────────────────────║
║ 오고 0점       ○광 ○멍 ○피   ║
╚══════════════════════════════╝
```

현재 턴을 나타내기 위해 패널 전체 색상을 변경하지 않는다.

---

# 11. Card / 화투패 디자인

화투패는 화면에서 가장 중요한 요소이므로 UI가 화투보다 튀면 안 된다.

## 기본 카드

* 기존 화투 이미지 유지
* 얇은 Cream/White Border
* 약한 Shadow
* 강한 빨간색 외곽선 제거 또는 최소화

기본 카드에 Red Border를 사용하지 않는다.

---

## 선택된 카드

선택 상태는 Gold를 사용한다.

```text
Gold Border
+
약한 Gold Glow
+
필요하다면 약간 위로 이동
```

Red Border는 카드 선택 상태에 사용하지 않는다.

---

# 12. Card Shadow

기본:

```text
0 2px 6px rgba(0, 0, 0, 0.25)
```

선택:

```text
0 4px 10px rgba(213, 164, 58, 0.35)
```

전체 카드에서 동일한 Shadow 규칙을 사용한다.

---

# 13. Player Hand

하단 손패는 핵심 조작 영역이므로 명확하고 깔끔하게 구성한다.

## 기본 상태

```text
[Card][Card][Card][Card][Card]
```

* 카드 간격 일정
* 카드 자체가 가장 잘 보이도록 구성
* 과도한 테두리 제거

## 선택 상태

```text
[Card][Card]
          ↑
        선택
```

* Gold Border
* Gold Glow
* 약간 위로 이동 가능

---

# 14. Hand 설정값은 Inspector에서 수정 가능해야 함

다음 값은 코드에 하드코딩하지 않는다.

* Card Spacing
* Card Scale
* Horizontal Offset
* Vertical Offset
* Max Fan Angle
* Selected Card Offset
* Selected Card Scale

예:

```csharp
[SerializeField] private float cardSpacing = 80f;
[SerializeField] private float selectedCardOffset = 30f;
[SerializeField] private float cardScale = 1.0f;
```

이러한 값은 Unity Inspector에서 수정할 수 있어야 한다.

---

# 15. 좌우 먹은 패 영역

현재의 회색 세로 박스 느낌을 제거한다.

먹은 패가 실제 게임판 위에 쌓여 있는 것처럼 보이도록 한다.

권장:

* Background: Dark Green
* Border: 매우 낮은 대비
* 카드 자연스러운 겹침
* 불필요한 회색 박스 제거

목표:

> UI 박스 안의 카드가 아니라 게임판 위에 쌓인 카드처럼 보이게 한다.

---

# 16. 중앙 게임 영역

현재 중앙 게임 영역의 구조는 유지한다.

단, 현재의 검은색/짙은 영역이 지나치게 튀지 않도록 한다.

```text
Background
#24452F

Center Area
#193523
```

배경과의 대비는 자연스럽게 유지한다.

---

# 17. Button 디자인 시스템

모든 버튼을 동일한 디자인 시스템으로 통일한다.

## Radius

```text
Card: 4px
Button: 6px
Panel: 6px
Modal: 10px
```

---

## Primary Button

```text
#C93A32
```

주요 행동에 사용한다.

예:

```text
┌──────────────────────────┐
│        다시 시작          │
└──────────────────────────┘
```

---

## Secondary Button

```text
Warm Cream
Dark Green Text
Dark Green Border
```

---

## Special Button

```text
#D5A43A
```

보상/특별한 기능 등에 제한적으로 사용한다.

---

# 18. 버튼 중요도

한 화면에 Primary Button은 원칙적으로 1개만 사용한다.

예:

```text
Primary
[ 다시 시작 ]

Secondary
[ 점수 상세 ]

Tertiary
[ 타이틀 ]
```

현재처럼 초록/노랑/회색 등 기능 의미와 관계없이 각각 다른 색을 사용하는 방식을 제거한다.

---

# 19. 승리/패배 모달

현재 결과 모달은 게임 화면과 디자인 언어가 다르므로 전면 통일한다.

## Overlay

```text
rgba(0, 0, 0, 0.65)
```

## Modal

```text
Background
#F3EBDD

Border
#D5A43A
```

Shadow:

```text
0 12px 40px rgba(0,0,0,0.35)
```

---

# 20. Result Modal 구조

```text
╔════════════════════════════════════╗
║                                    ║
║             AI-C 승리              ║
║                                    ║
║                7점                 ║
║                                    ║
║        내 머니 100,000원            ║
║                                    ║
║       ┌──────────────────┐         ║
║       │     다시 시작     │         ║
║       └──────────────────┘         ║
║                                    ║
║      점수 상세          타이틀       ║
║                                    ║
╚════════════════════════════════════╝
```

### Typography

승리 제목:

```text
32~40px / Bold
#20251F
```

점수:

```text
48px / Bold
#C93A32
```

보조 정보:

```text
14~16px
#687066
```

---

# 21. Typography System

가능하면 하나의 폰트 패밀리를 사용한다.

## Display

```text
32~40px
Bold
```

용도:

* 승리
* 큰 점수

## Heading

```text
20~24px
Bold
```

용도:

* 플레이어 이름
* 주요 제목

## Body

```text
14~16px
Medium
```

용도:

* 상태
* 설명
* 금액

## Caption

```text
11~12px
Regular
```

용도:

* 광/멍/피
* 보조 정보

---

# 22. Border System

기본:

```text
1px solid rgba(25,53,35,0.25)
```

강조:

```text
2px solid #D5A43A
```

중요:

```text
2px solid #C93A32
```

불필요하게 두꺼운 외곽선을 사용하지 않는다.

---

# 23. Shadow System

Shadow는 기본적으로 2종류만 사용한다.

## Small

```text
0 2px 6px rgba(0,0,0,0.20)
```

## Large

```text
0 8px 30px rgba(0,0,0,0.30)
```

---

# 24. Unity GameObject / Prefab 구조 원칙

## 핵심 원칙

### "보이는 것은 GameObject / Prefab으로 만든다."

UI의 시각적 구조를 C# 코드로 조립하지 않는다.

---

## 지양

```csharp
new GameObject(...)
AddComponent<Image>()
AddComponent<Text>()
AddComponent<Button>()
RectTransform 설정
Image 색상 설정
```

이런 방식으로 UI를 런타임에 조립하지 않는다.

---

## 권장

```text
Unity Scene
    ↓
GameObject
    ↓
Component
    ↓
Prefab
    ↓
Script는 데이터/동작 제어
```

즉:

```text
C# = 무엇을 할 것인가
Prefab = 어떻게 보일 것인가
Scene = 어디에 존재하는가
```

---

# 25. Scene Hierarchy 구조

가능한 경우 다음과 유사한 구조를 만든다.

```text
Game
├── GameTable
│   ├── TableBackground
│   ├── CenterArea
│   ├── DeckPosition
│   ├── CenterCardPosition
│   ├── PlayerCapturedArea
│   ├── AI_A_CapturedArea
│   ├── AI_B_CapturedArea
│   └── AI_C_CapturedArea
│
├── Players
│   ├── Player
│   ├── AI_A
│   ├── AI_B
│   └── AI_C
│
├── PlayerHands
│   ├── PlayerHand
│   ├── AI_A_Hand
│   ├── AI_B_Hand
│   └── AI_C_Hand
│
├── UI
│   ├── PlayerPanels
│   ├── GameButtons
│   ├── TurnIndicator
│   └── GameMessage
│
└── Modals
    ├── ResultModal
    ├── ScoreModal
    └── ConfirmModal
```

실제 프로젝트의 기존 구조가 있다면 그것을 우선적으로 활용한다.

목적은 **Hierarchy에서 게임 화면의 구조를 한눈에 볼 수 있게 하는 것**이다.

---

# 26. Prefab으로 만들어야 하는 요소

반복적으로 사용하는 요소는 Prefab으로 분리한다.

## PlayerPanel

```text
PlayerPanel
├── Background
├── Header
│   ├── PlayerName
│   └── Money
├── Score
├── GoStopStatus
│   ├── Gwang
│   ├── Meong
│   └── Pi
└── TurnIndicator
```

하나의 PlayerPanel Prefab을 사용하여:

```text
PlayerPanel_Player
PlayerPanel_AI_A
PlayerPanel_AI_B
PlayerPanel_AI_C
```

를 구성한다.

각 플레이어마다 별도의 UI 구조를 만들지 않는다.

---

# 27. HwatuCard Prefab

화투패도 Prefab 기반으로 관리한다.

```text
HwatuCard
├── CardBackground
├── CardImage
├── SelectionHighlight
├── SpecialEffect
└── DebugInfo
```

C#은 카드 데이터를 관리한다.

예:

```text
CardData
- cardId
- month
- type
- score
```

카드의 실제 시각적 디자인은 Prefab에서 관리한다.

---

# 28. PlayerHand 구조

```text
PlayerHand
├── HandAnchor
├── CardContainer
└── HandHighlight
```

카드 배치 알고리즘은 C#에서 처리하되,

다음 값은 Inspector에서 조정할 수 있어야 한다.

```text
Card Spacing
Card Scale
Horizontal Offset
Vertical Offset
Max Fan Angle
Selected Card Offset
```

---

# 29. GameTable 구조

```text
GameTable
├── Background
├── CenterArea
├── DeckPosition
├── CenterCardPosition
├── PlayerCapturedPosition
├── AI_A_CapturedPosition
├── AI_B_CapturedPosition
└── AI_C_CapturedPosition
```

C#에서 절대 좌표를 직접 지정하지 않는다.

지양:

```csharp
transform.position = new Vector3(123, 456, 0);
```

권장:

```csharp
[SerializeField] private Transform deckPosition;
[SerializeField] private Transform centerCardPosition;
[SerializeField] private Transform playerCapturedPosition;
```

Scene의 Transform을 참조하여 사용한다.

---

# 30. UI 위치값 하드코딩 금지

다음과 같은 값은 코드에 직접 작성하지 않는다.

* Panel Position
* Button Position
* Card Position
* Modal Position
* Width
* Height
* Padding
* Spacing

가능한 한 Unity의:

* RectTransform
* Anchor
* Pivot
* Layout Group
* Grid Layout Group
* Content Size Fitter

등을 활용한다.

---

# 31. Inspector에서 조정 가능해야 하는 값

다음 값은 SerializeField 등을 활용하여 Inspector에서 수정할 수 있도록 한다.

## PlayerPanel

* Width
* Height
* Padding
* Font Size
* Border Width
* Corner Radius
* Spacing

## Card

* Width
* Height
* Scale
* Selected Offset
* Selected Scale
* Shadow

## Hand

* Card Spacing
* Fan Angle
* Selected Offset
* Hand Position

## Modal

* Width
* Height
* Padding
* Button Spacing
* Title Size
* Score Size

---

# 32. ScriptableObject를 활용한 UI Theme

여러 Prefab에서 공유되는 디자인 값은 필요하면 ScriptableObject로 분리한다.

예:

```text
UITheme
├── PrimaryColor
├── AccentColor
├── BackgroundColor
├── SurfaceColor
├── TextColor
├── SecondaryTextColor
├── PrimaryFont
├── BodyFont
└── CaptionFont
```

현재 디자인 시스템:

```text
Primary
#C93A32

Accent
#D5A43A

Table
#24452F

Table Dark
#193523

Surface
#F3EBDD

Surface Light
#FFFDF8

Text
#20251F

Secondary Text
#687066
```

색상을 변경할 경우 Theme 하나를 수정하여 전체 UI에 반영할 수 있도록 한다.

단, 현재 프로젝트가 ScriptableObject를 과도하게 사용하지 않는 구조라면 불필요하게 복잡하게 만들지 않는다.

---

# 33. Script와 View 역할 분리

## C# Script 담당

* 게임 데이터
* 게임 상태
* 점수
* 턴
* 카드 선택 상태
* 승패 상태
* 버튼 이벤트
* 카드 이동
* UI 상태 변경

## GameObject / Prefab 담당

* 위치
* 크기
* 색상
* 폰트
* 이미지
* 배경
* 테두리
* 그림자
* Layout
* 시각적 애니메이션 구조

---

# 34. 런타임 Instantiate는 허용

모든 오브젝트를 Scene에 미리 배치해야 한다는 의미는 아니다.

반복적으로 생성되는 요소는 Prefab을 Instantiate할 수 있다.

예:

```text
HwatuCard.prefab
        ↓
게임 시작
        ↓
필요한 카드 Instantiate
        ↓
PlayerHand/CardContainer에 배치
```

중요한 것은:

> Instantiate하는 대상의 시각적 구조가 Prefab 내부에 이미 존재해야 한다.

즉, C#에서 Instantiate 후 Image/Text/Button/Panel 등을 새로 조립하지 않는다.

---

# 35. Initialize 패턴

Prefab에 데이터를 연결할 때 Initialize 패턴을 권장한다.

예:

```csharp
public void Initialize(PlayerData data)
{
    playerName.text = data.playerName;
    moneyText.text = FormatMoney(data.money);
    scoreText.text = data.score.ToString();
}
```

Script는 데이터를 전달한다.

폰트, 색상, 위치, 패딩, 디자인 등은 Prefab이 담당한다.

---

# 36. Prefab 수정 Workflow

최종적으로 다음 workflow가 가능해야 한다.

```text
Project
└── Prefabs
    ├── UI
    │   ├── PlayerPanel.prefab
    │   ├── ResultModal.prefab
    │   ├── ScoreModal.prefab
    │   ├── GameButton.prefab
    │   └── StatusIndicator.prefab
    │
    └── Cards
        ├── HwatuCard.prefab
        └── CapturedCard.prefab
```

예를 들어 PlayerPanel의 디자인을 변경하면:

```text
PlayerPanel.prefab
↓
디자인 수정
↓
Save
↓
모든 PlayerPanel에 동일한 변경 반영
```

이 구조를 목표로 한다.

---

# 37. Scene Object와 Prefab 구분

모든 것을 Prefab으로 만들 필요는 없다.

## Prefab

반복/재사용되는 요소:

* PlayerPanel
* HwatuCard
* Button
* Modal
* CapturedCard
* StatusIndicator

## Scene Object

게임에서 하나만 존재하는 요소:

* GameTable
* DeckPosition
* CenterArea
* PlayerHandContainer
* MainCanvas
* Camera
* GameManager 연결용 Object

---

# 38. GameObject 이름 규칙

Hierarchy에서 구조를 쉽게 파악할 수 있도록 이름을 명확하게 한다.

권장 예:

```text
GameTable
CenterArea
DeckPosition
CenterCardPosition

Player
AI_A
AI_B
AI_C

PlayerHand
AI_A_Hand
AI_B_Hand
AI_C_Hand

PlayerPanel_Player
PlayerPanel_AI_A
PlayerPanel_AI_B
PlayerPanel_AI_C

ResultModal
ScoreModal
```

기존 프로젝트의 네이밍 규칙이 있다면 기존 규칙을 우선한다.

---

# 39. UI 생성 코드 리팩터링

기존 코드에서 다음과 같은 패턴이 발견되면 리팩터링을 검토한다.

```csharp
new GameObject(...)
AddComponent<Image>()
AddComponent<Text>()
AddComponent<Button>()
GetComponent<RectTransform>().sizeDelta = ...
GetComponent<Image>().color = ...
```

가능한 경우 Prefab 참조 방식으로 변경한다.

예:

```csharp
[SerializeField] private PlayerPanel playerPanelPrefab;
```

그리고:

```csharp
var panel = Instantiate(playerPanelPrefab, parent);
panel.Initialize(playerData);
```

PlayerPanel의 시각적 구성은 Prefab에서 관리한다.

---

# 40. 기존 코드 분석 후 리팩터링

바로 코드를 수정하지 말고 먼저 현재 프로젝트 구조를 분석한다.

반드시 확인할 것:

1. 어떤 UI가 코드에서 생성되는가?
2. 어떤 UI가 이미 Scene에 존재하는가?
3. 어떤 UI가 Prefab인가?
4. 어떤 UI가 런타임 Instantiate되는가?
5. UI 생성 책임을 가진 Script는 무엇인가?
6. 카드 생성/배치 코드는 어디에 있는가?
7. 플레이어 패널 생성 코드는 어디에 있는가?
8. 승리 모달 생성 코드는 어디에 있는가?
9. 현재 UI의 스타일을 담당하는 파일은 어디인가?
10. 중복된 UI 코드/스타일은 어디에 존재하는가?

분석 결과를 기준으로 기존 구조를 최대한 활용하면서 필요한 부분만 리팩터링한다.

---

# 41. 리팩터링 우선순위

다음 순서로 작업한다.

## Phase 1 — 프로젝트 구조 분석

* 현재 Scene
* Prefab
* UI Script
* Card Script
* GameManager
* UI 생성 코드

분석

---

## Phase 2 — Design Token 구축

전역적으로 다음을 정의한다.

```text
Colors
Typography
Spacing
Radius
Border
Shadow
```

---

## Phase 3 — GameTable

* Background
* CenterArea
* Deck
* CenterCards
* CapturedArea

---

## Phase 4 — PlayerPanel Prefab

* Player Name
* Money
* Score
* Go/Stop Status
* Turn Indicator

모든 플레이어에게 공통 적용

---

## Phase 5 — HwatuCard Prefab

* Card Image
* Selection
* Shadow
* Special State

---

## Phase 6 — PlayerHand

* Card Spacing
* Scale
* Selection Offset
* Fan Angle

Inspector에서 수정 가능하도록 구성

---

## Phase 7 — Buttons

Primary / Secondary / Tertiary

---

## Phase 8 — ResultModal

* Victory
* Defeat
* Score
* Restart
* Score Detail
* Title

---

## Phase 9 — 전체 Polish

* 색상
* 정렬
* 여백
* 폰트
* Shadow
* Border
* Radius
* 카드 가독성
* 화면 비율

---

# 42. 반응형 / 화면 크기

가능하면 UI 위치를 절대 좌표로 고정하지 않는다.

Unity의 Anchor와 RectTransform을 활용한다.

특히:

* 상단 AI
* 좌측 AI
* 우측 AI
* 하단 Player
* 손패
* Result Modal

은 화면 크기 변화에 대응할 수 있도록 구성한다.

---

# 43. 하지 말아야 할 것

다음은 이번 작업에서 금지하거나 최소화한다.

### UI 관련

* UI마다 임의의 색상 추가
* 플레이어마다 다른 디자인
* 강한 Red Border 남발
* 과도한 Drop Shadow
* 컴포넌트마다 다른 Radius
* 모든 버튼을 다른 색으로 만들기
* 화투보다 UI가 더 눈에 띄게 만들기

### 코드 관련

* UI를 C#에서 직접 조립
* UI 위치를 Vector3/Vector2로 하드코딩
* UI 크기를 코드에서 직접 지정
* 폰트 크기를 코드에 하드코딩
* 색상을 여러 Script에 직접 작성
* 동일한 UI를 여러 코드에서 별도로 생성

### 프로젝트 구조 관련

* 기존 게임 로직을 불필요하게 수정
* 카드 데이터를 변경
* 게임 규칙 변경
* 기존 기능 삭제
* UI 리디자인 때문에 전체 프로젝트를 과도하게 재구성

---

# 44. 최종 Unity Editor Workflow

작업이 완료되면 다음 작업이 코드 수정 없이 가능해야 한다.

## 플레이어 패널 크기 변경

```text
Unity Hierarchy
→ PlayerPanel Prefab
→ RectTransform
→ Width / Height 수정
```

---

## 플레이어 패널 색상 변경

```text
PlayerPanel Prefab
→ Background
→ Color 변경
```

또는 UITheme을 사용하는 경우:

```text
UITheme
→ Surface Color 변경
```

---

## 카드 선택 효과 변경

```text
HwatuCard.prefab
→ SelectionHighlight
→ Color / Image / Position 변경
```

---

## 덱 위치 변경

```text
GameTable
→ DeckPosition
→ Transform 이동
```

---

## 손패 간격 변경

```text
PlayerHand
→ Card Spacing
→ Inspector에서 값 변경
```

---

## 승리 모달 크기 변경

```text
ResultModal.prefab
→ RectTransform
→ Width / Height 변경
```

이러한 수정에 C# 코드 수정이 필요하지 않아야 한다.

---

# 45. 최종 프로젝트 구조 목표

가능하면 다음과 유사한 구조를 목표로 한다.

```text
Assets
├── Prefabs
│   ├── UI
│   │   ├── PlayerPanel.prefab
│   │   ├── ResultModal.prefab
│   │   ├── ScoreModal.prefab
│   │   ├── GameButton.prefab
│   │   └── StatusIndicator.prefab
│   │
│   └── Cards
│       ├── HwatuCard.prefab
│       └── CapturedCard.prefab
│
├── Scenes
│   └── GameScene.unity
│
├── Scripts
│   ├── Game
│   ├── Cards
│   ├── Players
│   └── UI
│
├── ScriptableObjects
│   └── UITheme.asset
│
└── Art
    ├── Cards
    ├── UI
    └── Fonts
```

단, 기존 프로젝트 구조가 있다면 불필요하게 전체 폴더 구조를 변경하지 않는다.

---

# 46. 완료 조건

## UI 디자인

* [ ] 청록색 플레이어 헤더 제거
* [ ] 플레이어별 서로 다른 UI 색상 제거
* [ ] Green / Cream / Red / Gold 중심으로 통일
* [ ] PlayerPanel 디자인 통일
* [ ] 내 플레이어 강조 방식 통일
* [ ] 화투 기본 카드의 강한 빨간 외곽선 제거/완화
* [ ] 카드 선택 상태 Gold로 통일
* [ ] 먹은 패 영역 단순화
* [ ] 중앙 게임 영역 자연스럽게 정리
* [ ] Button 스타일 통일
* [ ] Primary / Secondary / Tertiary hierarchy 적용
* [ ] ResultModal을 게임 화면과 동일한 디자인 언어로 변경
* [ ] Typography 통일
* [ ] Border 통일
* [ ] Shadow 통일
* [ ] Radius 통일

## Unity 구조

* [ ] 주요 반복 UI가 Prefab으로 분리됨
* [ ] PlayerPanel이 공통 Prefab으로 구성됨
* [ ] HwatuCard가 Prefab으로 구성됨
* [ ] ResultModal이 Prefab으로 구성됨
* [ ] Button이 공통 Prefab/컴포넌트 구조를 가짐
* [ ] Scene Hierarchy에서 게임 구조를 파악할 수 있음
* [ ] 중요한 위치가 Transform/RectTransform으로 관리됨
* [ ] UI 위치가 코드에 하드코딩되지 않음
* [ ] UI 크기가 코드에 하드코딩되지 않음
* [ ] UI 디자인 값이 Inspector에서 수정 가능함
* [ ] 반복 UI는 동일한 Prefab을 사용함
* [ ] UI 생성 코드가 불필요하게 GameObject를 조립하지 않음
* [ ] Script는 데이터와 동작 중심으로 동작함
* [ ] Prefab은 시각적 구조를 담당함
* [ ] 기존 게임 로직이 정상 작동함
* [ ] 기존 카드 데이터와 이미지가 정상 작동함

---

# 47. 최종 디자인 판단 기준

이번 작업에서 가장 중요한 기준은 개별 UI가 예쁜지가 아니다.

### 최우선 기준

> **"모든 UI 요소가 하나의 게임에서 만들어진 것처럼 보이는가?"**

그리고 Unity 구조에 대해서는 다음 질문을 기준으로 한다.

> **"이 UI를 수정하려면 C# 코드를 찾아야 하는가?"**

완료 후 이 질문에 **아니오**가 되어야 한다.

---

# 48. Claude Code 작업 지시

이 문서를 기준으로 실제 프로젝트를 수정한다.

작업 시작 전에 반드시 현재 프로젝트 구조를 분석한다.

분석 없이 기존 코드를 무작정 삭제하거나 새 UI 시스템을 처음부터 만들지 않는다.

### 작업 원칙

1. 기존 게임 로직 유지
2. 기존 카드 데이터 유지
3. 기존 카드 이미지 유지
4. 기존 게임 플레이 구조 유지
5. 현재 레이아웃 최대한 유지
6. UI 디자인 시스템 통일
7. 시각적 요소는 GameObject / Prefab 중심으로 구성
8. 반복 UI는 Prefab화
9. UI 위치와 크기는 Inspector에서 수정 가능하게 구성
10. UI 디자인 값은 코드에 하드코딩하지 않음
11. C#은 데이터와 동작 중심으로 사용
12. Scene은 게임 오브젝트 구조와 위치를 관리
13. Prefab은 반복되는 시각적 요소를 관리
14. 공통 디자인은 Theme/Prefab으로 통일
15. 불필요한 프로젝트 구조 변경을 하지 않음

---

# 49. 가장 중요한 최종 요구사항

이번 작업은 단순히 현재 화면을 코드로 똑같이 다시 만드는 작업이 아니다.

다음 구조를 만드는 것이 최종 목표다.

```text
                 GAME LOGIC
                     │
                     ▼
              C# / Game State
                     │
                     ▼
             UI State / Data
                     │
                     ▼
        ┌────────────────────────┐
        │   Unity GameObject     │
        │                        │
        │   Scene / Prefab       │
        │                        │
        │   Inspector            │
        └────────────────────────┘
                     │
                     ▼
                  화면
```

즉,

### C#

"플레이어가 이겼다."

### UI

"승리 모달을 보여준다."

### Prefab

"승리 모달은 이렇게 생겼다."

### Scene

"승리 모달은 이 위치에 있다."

### Inspector

"크기, 간격, 색상, 폰트 등을 내가 직접 조절한다."

라는 명확한 책임 분리를 구현한다.

최종 결과는 **디자인적으로 통일된 현대적인 고스톱 UI**이면서 동시에 **Unity Editor에서 디자이너/개발자가 직접 수정하고 조정할 수 있는 GameObject + Prefab 중심 프로젝트 구조**여야 한다.
