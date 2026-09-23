using System;
using System.Collections.Generic;

/// <summary>
/// 좌석 정보 박스(닉네임/고+점수/금액/배지)가 표시할 데이터 — 옵저버
/// 패턴의 Subject(피관찰 대상). <see cref="GoStop3PGame"/>(컨트롤러)이
/// 매 RebuildUI마다 이 프로퍼티들만 채우고, 실제로 값이 바뀐 필드가
/// 하나라도 있으면 <see cref="NotifyIfDirty"/>가 <see cref="Changed"/>를
/// 딱 한 번 쏜다 — <see cref="GoStopStatusBoxView"/>는 <c>Bind(this)</c>로
/// 한 번만 구독해 두면, 그 이후로는 컨트롤러가 뷰를 직접 참조하지
/// 않아도(뷰의 SetXxx 메서드를 호출하지 않아도) 자동으로 최신 상태를
/// 반영한다.
///
/// 2026-09-12 도입 — 이전엔 <c>GoStop3PGame</c>이 <c>GoStopStatusBoxView</c>의
/// SetDealer/SetRisk/SetCountBadge/SetMoneyDelta/ApplyTurnState/SetDim/
/// HideAllBadges를 매턴 직접 호출하는 순수 imperative View 구조였다
/// (사용자가 "옵저버 패턴을 썼다"고 표현했지만 실제로는 이벤트 구독/발행이
/// 전혀 없었다 — CLAUDE.md 2026-09-12 리뷰 섹션 참고). 이 클래스가 그
/// 격차를 실제로 메운다: 컨트롤러는 이제 뷰의 존재 자체를 몰라도 되고,
/// "화면을 어떻게 그릴지"(색·클램프 등)는 전부 View의 Render로 옮겨갔다.
///
/// 값이 실제로 바뀔 때만 dirty 표시 → 한 번만 알림 → 뷰가 스스로 최신
/// 값을 당겨가 다시 그리는(pull-based) 구조라, 같은 프레임에 여러
/// 프로퍼티를 연달아 바꿔도(FillSlot 하나가 6~7개를 채운다) 알림은
/// NotifyIfDirty() 호출 시점에 딱 한 번만 나간다.
/// </summary>
public sealed class GoStopSeatStatus
{
    /// <summary>이 좌석의 표시 데이터가 바뀌었다는 알림. 구독자(뷰)는
    /// 인자로 받은 this에서 최신 값을 직접 읽어간다.</summary>
    public event Action<GoStopSeatStatus> Changed;

    bool dirty;

    void Set<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        dirty = true;
    }

    string name = "";
    public string Name { get => name; set => Set(ref name, value); }

    /// <summary>현재 턴(또는 고/스톱 선택 중)인 좌석 강조.</summary>
    bool highlight;
    public bool Highlight { get => highlight; set => Set(ref highlight, value); }

    // 2026-09-13: 문자열(MoneyText)이 아니라 원값(int)으로 바꿨다 —
    // 뷰가 "이전에 보여주던 값 → 새 값"을 알아야 카운팅 애니메이션을
    // 돌릴 수 있는데, 포맷된 문자열만 받으면 그 차이를 알 방법이 없다.
    // 포맷("{0:N0}원")은 뷰가 렌더 시점에 한다.
    int money;
    public int Money { get => money; set => Set(ref money, value); }

    /// <summary>빈 슬롯(좌석 자체가 없음)에서는 금액 표시를 아예 감춘다 —
    /// "0원"을 보여주는 것과 다르다.</summary>
    bool moneyVisible = true;
    public bool MoneyVisible { get => moneyVisible; set => Set(ref moneyVisible, value); }

    /// <summary>세션 시작(선 정하기 이후) 대비 누적 머니 변동 — 부호·색은
    /// 뷰가 렌더 시점에 정한다(GoStopStatusBoxView.Render 참고).</summary>
    int moneyDelta;
    public int MoneyDelta { get => moneyDelta; set => Set(ref moneyDelta, value); }

    string goScoreText = "";
    public string GoScoreText { get => goScoreText; set => Set(ref goScoreText, value); }

    bool dim;
    public bool Dim { get => dim; set => Set(ref dim, value); }

    /// <summary>이 좌석이 이번 판 배지 표시 대상이 아니면(쉬는 좌석,
    /// 빈 슬롯) true — 뷰는 이 값이 참이면 선/광멍피/흔들기/뻑 배지를
    /// 전부 숨긴 상태로 렌더한다.</summary>
    bool badgesHidden;
    public bool BadgesHidden { get => badgesHidden; set => Set(ref badgesHidden, value); }

    bool isDealer;
    public bool IsDealer { get => isDealer; set => Set(ref isDealer, value); }

    bool gwangBak, meongBak, piBak;
    public bool GwangBak { get => gwangBak; set => Set(ref gwangBak, value); }
    public bool MeongBak { get => meongBak; set => Set(ref meongBak, value); }
    public bool PiBak { get => piBak; set => Set(ref piBak, value); }

    /// <summary>흔들기/뻑 누적 횟수(원값) — 점 2개로만 표시하는 클램프는
    /// "어떻게 보여줄지"라 뷰의 렌더 단계에서 한다.</summary>
    int shakeCount, ppeokCount;
    public int ShakeCount { get => shakeCount; set => Set(ref shakeCount, value); }
    public int PpeokCount { get => ppeokCount; set => Set(ref ppeokCount, value); }

    /// <summary>이번 갱신 패스에서 바뀐 필드가 하나라도 있으면 Changed를
    /// 딱 한 번 쏜다. 컨트롤러가 한 좌석의 필드를 전부 채운 직후(FillSlot/
    /// RefreshMoneyLabelsOnly/RefreshStatusBoxIdentitiesBeforeDeal 끝)
    /// 반드시 호출해야 화면에 반영된다.</summary>
    public void NotifyIfDirty()
    {
        if (!dirty) return;
        dirty = false;
        Changed?.Invoke(this);
    }
}
