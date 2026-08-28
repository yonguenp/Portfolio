using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 화투 48장 전체 구성표.
///
/// 월별로 광/열끗/띠/피 장수가 다르다(8월·11월엔 띠가 없고, 11월·12월엔
/// 쌍피가 있는 식). 이 표는 나무위키 고스톱 문서로 검증한 표준 구성과
/// 정확히 일치하도록 만들었다 — 광 5장, 열끗 9장, 띠 10장, 피 24장(피 값
/// 합계 28) 총 48장.
/// </summary>
public static class GoStopDeck
{
    static readonly string[] MonthName =
    {
        "", "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December",
    };

    public static List<HwatuCard> BuildFull()
    {
        var d = new List<HwatuCard>(48);

        void Gwang(int m)                    => d.Add(new HwatuCard(m, HwatuKind.Gwang, S(m, "Hikari")));
        void Tane(int m, bool godori = false, bool dualPi = false) =>
            d.Add(new HwatuCard(m, HwatuKind.Yeolkkeut, S(m, "Tane"), godori: godori, dualPi: dualPi));
        void Ddi(int m, DdiColor c, string suffix = "Tanzaku") => d.Add(new HwatuCard(m, HwatuKind.Ddi, S(m, suffix), ddi: c));
        void Pi(int m, string suffix, int val = 1) => d.Add(new HwatuCard(m, HwatuKind.Pi, S(m, suffix), piValue: val));

        // 1월 소나무: 광, 홍단, 피×2
        Gwang(1); Ddi(1, DdiColor.Hong); Pi(1, "Kasu_1"); Pi(1, "Kasu_2");
        // 2월 매화: 열끗(고도리·휘파람새), 홍단, 피×2
        Tane(2, godori: true); Ddi(2, DdiColor.Hong); Pi(2, "Kasu_1"); Pi(2, "Kasu_2");
        // 3월 벚꽃: 광, 홍단, 피×2
        Gwang(3); Ddi(3, DdiColor.Hong); Pi(3, "Kasu_1"); Pi(3, "Kasu_2");
        // 4월 흑싸리: 열끗(고도리·두견새), 초단, 피×2
        Tane(4, godori: true); Ddi(4, DdiColor.Cho); Pi(4, "Kasu_1"); Pi(4, "Kasu_2");
        // 5월 난초: 열끗, 초단, 피×2
        Tane(5); Ddi(5, DdiColor.Cho); Pi(5, "Kasu_1"); Pi(5, "Kasu_2");
        // 6월 모란: 열끗, 청단, 피×2
        Tane(6); Ddi(6, DdiColor.Cheong); Pi(6, "Kasu_1"); Pi(6, "Kasu_2");
        // 7월 홍싸리: 열끗, 초단, 피×2
        Tane(7); Ddi(7, DdiColor.Cho); Pi(7, "Kasu_1"); Pi(7, "Kasu_2");
        // 8월 공산: 광, 열끗(고도리·기러기), 피×2 — 띠 없음
        Gwang(8); Tane(8, godori: true); Pi(8, "Kasu_1"); Pi(8, "Kasu_2");
        // 9월 국화: 열끗(술잔 — 열끗/쌍피 선택 가능), 청단, 피×2
        Tane(9, dualPi: true); Ddi(9, DdiColor.Cheong); Pi(9, "Kasu_1"); Pi(9, "Kasu_2");
        // 10월 단풍: 열끗, 청단, 피×2
        Tane(10); Ddi(10, DdiColor.Cheong); Pi(10, "Kasu_1"); Pi(10, "Kasu_2");
        // 11월 오동: 광, 피×2 + 쌍피×1 — 열끗·띠 없음
        Gwang(11); Pi(11, "Kasu_1"); Pi(11, "Kasu_2"); Pi(11, "Kasu_3", val: 2);
        // 12월 비: 광, 열끗(제비 — 고도리 아님), 띠(색 없음, "비띠"), 쌍피×1
        Gwang(12); Tane(12); Ddi(12, DdiColor.None); Pi(12, "Kasu", val: 2);

        return d;
    }

    static string S(int month, string suffix) => $"{MonthName[month]}_{suffix}";

    public static void Shuffle(List<HwatuCard> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ── 네트워크 대전용 카드 조회 ────────────────────────
    // 2026-08-19: 게스트 기기는 실제 판정을 안 하고 호스트가 보내주는
    // 스냅샷(GoStopStateSnapshot)을 그대로 그리기만 한다 — 그 스냅샷은
    // 카드를 통째로 안 보내고 spriteName 문자열만 보낸다(48장+조커
    // 전부 유일해서 그것만으로 카드 한 장을 완전히 특정할 수 있다,
    // GoStopNetMessage 문서 참고). 받는 쪽이 그 이름으로 실제 HwatuCard
    // 인스턴스를 다시 만들 수 있어야 하므로 이 조회 테이블을 둔다.
    static Dictionary<string, HwatuCard> templateBySprite;

    static void EnsureTemplates()
    {
        if (templateBySprite != null) return;
        templateBySprite = new Dictionary<string, HwatuCard>();
        foreach (var c in BuildFull()) templateBySprite[c.spriteName] = c;
        templateBySprite["Joker_1"] = new HwatuCard(0, HwatuKind.Pi, "Joker_1", piValue: 1, isJoker: true);
        templateBySprite["Joker_2"] = new HwatuCard(0, HwatuKind.Pi, "Joker_2", piValue: 2, isJoker: true);
    }

    /// <summary>카드를 네트워크로 실어 보낼 문자열로 인코딩한다. 9월 열끗
    /// (dualPi)은 열끗/피 중 어느 쪽으로 선택했는지(useAsPi)가 카드
    /// 인스턴스에만 있는 상태라 spriteName만으로는 복원이 안 된다 —
    /// "|pi" 접미사를 붙여 같이 실어 보낸다.</summary>
    public static string Encode(HwatuCard c) =>
        (c.dualPi && c.useAsPi) ? c.spriteName + "|pi" : c.spriteName;

    /// <summary>Encode의 역과정 — 매번 새 인스턴스를 만든다(같은 달의 두
    /// 홑피처럼 spriteName이 같아도 리스트 안에서 서로 다른 카드로 다뤄야
    /// 한다는 이 프로젝트의 기존 원칙과 동일한 이유). 못 찾으면 null —
    /// 호출부가 방어적으로 무시할 수 있게.</summary>
    public static HwatuCard Decode(string encoded)
    {
        if (string.IsNullOrEmpty(encoded)) return null;
        EnsureTemplates();
        bool asPi = encoded.EndsWith("|pi");
        string name = asPi ? encoded.Substring(0, encoded.Length - 3) : encoded;
        if (!templateBySprite.TryGetValue(name, out var t)) return null;
        var card = new HwatuCard(t.month, t.kind, t.spriteName, t.ddi, t.piValue, t.godori, t.dualPi, t.isJoker);
        if (asPi) card.useAsPi = true;
        return card;
    }

    public static string[] EncodeAll(IEnumerable<HwatuCard> cards)
    {
        var list = new List<string>();
        foreach (var c in cards) list.Add(Encode(c));
        return list.ToArray();
    }

    public static List<HwatuCard> DecodeAll(string[] encoded)
    {
        var list = new List<HwatuCard>();
        if (encoded == null) return list;
        foreach (var e in encoded)
        {
            var c = Decode(e);
            if (c != null) list.Add(c);
        }
        return list;
    }
}
