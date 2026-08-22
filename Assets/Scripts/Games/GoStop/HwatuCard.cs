/// <summary>화투 패 한 장의 종류.</summary>
public enum HwatuKind { Gwang, Yeolkkeut, Ddi, Pi }

/// <summary>
/// 띠(Ddi)의 색 분류. 3색 세트를 다 모으면 각각 3점 보너스(홍단/청단/초단).
/// 12월 띠("비띠")는 색이 없어 세트 보너스에 안 들어가지만 띠 장수 계산에는 들어간다.
/// </summary>
public enum DdiColor { None, Hong, Cheong, Cho }

/// <summary>
/// 화투 카드 한 장의 데이터. 48장 전체는 <see cref="GoStopDeck.BuildFull"/>에서 만든다.
///
/// 인스턴스는 매 판 <see cref="GoStopDeck.BuildFull"/>이 새로 만든다 — 카드 자체에
/// 상태(어느 더미에 있는지)를 두지 않고, 각 리스트(손패/필드/획득/deck)의 소속으로만
/// 위치를 나타낸다. 그래서 같은 달 안의 두 홑피처럼 값이 동일한 카드도
/// 리스트 안에서 각자 다른 인스턴스로 안전하게 구분된다.
/// </summary>
public class HwatuCard
{
    public readonly int       month;      // 1~12
    public readonly HwatuKind kind;
    public readonly DdiColor  ddi;        // kind==Ddi일 때만 의미
    public readonly int       piValue;    // kind==Pi일 때 1(홑피) 또는 2(쌍피)
    public readonly bool      godori;     // 2·4·8월 열끗(새) — 고도리 특수패
    public readonly string    spriteName; // Resources/Hwatu/{spriteName}.png

    /// <summary>
    /// 9월 열끗(국화 술잔)처럼 <b>열끗 또는 쌍피 중 하나로 선택해서 쓸 수 있는</b>
    /// 카드인지. true면 <see cref="UseAsPi"/>가 실제 역할을 결정한다.
    /// 카드 자체는 매 판 새로 만들어지므로(위 클래스 설명 참고) 이 선택 상태를
    /// 카드 인스턴스에 들고 있어도 "리스트 소속만으로 위치를 나타낸다"는
    /// 원칙과 충돌하지 않는다 — 위치가 아니라 역할 선택이라 다른 종류의 상태다.
    /// </summary>
    public readonly bool dualPi;
    public bool useAsPi; // dualPi인 카드에서만 의미. 기본은 열끗(false).

    /// <summary>
    /// 보너스 조커 패(월이 없는 특수 피). 표준 48장에는 없고 <see cref="GoStopRules.DealNew"/>가
    /// 더미에만 무작위로 끼워 넣는다 — 손패/필드에 섞이면 "월 매칭" 로직이
    /// 다룰 방법이 없어서(월 자체가 없으므로) 아예 더미 전용으로 제한했다.
    /// 더미에서 뒤집히면 매칭 없이 즉시 그 사람의 피로 들어간다.
    /// </summary>
    public readonly bool isJoker;

    public HwatuCard(int month, HwatuKind kind, string spriteName,
                     DdiColor ddi = DdiColor.None, int piValue = 0, bool godori = false,
                     bool dualPi = false, bool isJoker = false)
    {
        this.month      = month;
        this.kind       = kind;
        this.spriteName = spriteName;
        this.ddi        = ddi;
        this.piValue    = piValue;
        this.godori     = godori;
        this.dualPi     = dualPi;
        this.isJoker    = isJoker;
    }

    /// <summary>지금 실제로 어느 종류로 취급되는지 — 쌍피 선택 중이면 Pi, 아니면 원래 kind.</summary>
    public HwatuKind EffectiveKind => (dualPi && useAsPi) ? HwatuKind.Pi : kind;
    /// <summary>지금 실제로 적용되는 피 값 — 쌍피 선택 중인 9월 열끗은 2.</summary>
    public int EffectivePiValue => (dualPi && useAsPi) ? 2 : piValue;

    public override string ToString() => $"{month}월 {kind}({spriteName})";
}
