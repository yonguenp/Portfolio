using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 맞고(2인 고스톱) 규칙 엔진. UI/MonoBehaviour와 완전히 분리돼 있어
/// unity-cli에서 화면 없이 로직만 검증할 수 있다.
///
/// v1 범위: 기본 매칭·따닥·싹쓸이·기본 점수표·고/스톱 배수까지만 구현한다.
/// 쪽·뻑의 피 뺏기, 흔들기, 폭탄, 광박/피박 벌점은 <b>의도적으로 뺐다</b> —
/// 이 게임이 실제로 이기고 지는 데는 지장이 없지만 판을 화려하게 만드는
/// 부가 규칙들이라, 기본 골격이 검증된 뒤 얹는 게 안전하다.
/// </summary>
public static class GoStopRules
{
    public const int CAPTURE_LINE = 7;   // 이 점수부터 고/스톱을 선택할 수 있다

    // ── 딜링 ─────────────────────────────────────────────

    /// <summary>표준 48장 + 보너스 조커 2장(총 50장)을 함께 섞은 풀.
    /// 2026-08-23: "조커도 손패로 나와야 한다"는 요청 전까지는 조커를
    /// 표준 48장으로 손패/필드를 다 나눈 뒤 더미에만 강제로 끼워 넣었다
    /// (조커는 월이 없어 손/필드에 있으면 매칭 로직을 못 타서 처리할
    /// 방법이 없었기 때문). 이제는 손패/필드/더미 어디든 동등한 확률로
    /// 갈 수 있게 처음부터 50장을 통째로 섞는다 — 필드에 떨어진 조커는
    /// 딜링 함수가 직접 걸러내 별도로 돌려주고(아래 각 Deal 클래스의
    /// <c>jokersInField</c>), 호출자가 선(딜러)에게 즉시 피로 지급한다
    /// (더미에서 뒤집힐 때 즉시 그 사람 피로 들어가는 기존 규칙과 같은
    /// 원리를 딜링 시점에도 적용 — 월이 없어 아무도 못 먹는 카드가 필드에
    /// 영원히 남는 것을 막는다). 손패에 떨어진 조커는 그대로 둔다 — 이제
    /// 손패에서 조커를 직접 낼 수 있다(캡으로 즉시 이동 + 다음 뒷패를
    /// 대신 손으로 가져오는 처리는 GoStop3PGame.cs/GoStopGame.cs 쪽).</summary>
    static List<HwatuCard> BuildFullDeckWithJokers()
    {
        var deck = GoStopDeck.BuildFull();
        deck.Add(new HwatuCard(0, HwatuKind.Pi, "Joker_1", piValue: 1, isJoker: true));
        deck.Add(new HwatuCard(0, HwatuKind.Pi, "Joker_2", piValue: 2, isJoker: true));
        GoStopDeck.Shuffle(deck);
        return deck;
    }

    /// <summary>필드에서 조커를 걷어낸 만큼(선에게 지급하고 나면) 필드
    /// 장수가 원래보다 비게 된다 — 더미에서 그만큼 채워 넣어 필드 장수를
    /// 딜 규칙대로 맞춘다(사용자 확인, 2026-08-23). 채우는 카드가 또
    /// 조커면 같은 문제가 재발하므로(월이 없어 아무도 못 먹는 카드가
    /// 필드에 남는다), 더미에서 <b>조커가 아닌</b> 카드만 골라서 채운다 —
    /// 더미는 이미 완전히 섞여 있으므로 앞에서부터 순서대로 걸러 뽑아도
    /// 무작위성이 깨지지 않는다.</summary>
    static void RefillFieldFromDrawPile(List<HwatuCard> field, List<HwatuCard> drawPile, int count)
    {
        for (int i = 0; i < count; i++)
        {
            int idx = drawPile.FindIndex(c => !c.isJoker);
            if (idx < 0) break; // 이론상 안 일어난다 — 더미에 조커 아닌 카드가 항상 넉넉히 남는다
            field.Add(drawPile[idx]);
            drawPile.RemoveAt(idx);
        }
    }

    public class Deal
    {
        public List<HwatuCard> playerHand, aiHand, field, drawPile;
        public List<HwatuCard> jokersInField = new(); // 딜링 결과 필드에 떨어진 조커 — 즉시 선에게 지급할 것
    }

    /// <summary>10장씩 손패, 8장 필드, 나머지가 더미(50장 기준 22장). 순서는 셔플로 이미 무작위라 상관없다.</summary>
    public static Deal DealNew()
    {
        var deck = BuildFullDeckWithJokers();

        var d = new Deal
        {
            playerHand = deck.Take(10).ToList(),
            aiHand     = deck.Skip(10).Take(10).ToList(),
            field      = deck.Skip(20).Take(8).ToList(),
            drawPile   = deck.Skip(28).ToList(),
        };

        d.jokersInField = d.field.Where(c => c.isJoker).ToList();
        foreach (var j in d.jokersInField) d.field.Remove(j);
        RefillFieldFromDrawPile(d.field, d.drawPile, d.jokersInField.Count);

        return d;
    }

    // ── 딜링 (3인) ───────────────────────────────────────
    /// <summary>
    /// 3인 고스톱(정식 명칭 "고스톱" — 2인은 "맞고"). 7장씩 손패×3, 필드 6장,
    /// 나머지 21장이 더미(48-21-6=21). 나무위키·검색으로 교차 확인한 표준
    /// 구성이다.
    /// </summary>
    public class Deal3P
    {
        public List<HwatuCard> hand0, hand1, hand2, field, drawPile;
        public List<HwatuCard> jokersInField = new();
    }

    public static Deal3P DealNew3P()
    {
        var deck = BuildFullDeckWithJokers();

        var d = new Deal3P
        {
            hand0    = deck.Take(7).ToList(),
            hand1    = deck.Skip(7).Take(7).ToList(),
            hand2    = deck.Skip(14).Take(7).ToList(),
            field    = deck.Skip(21).Take(6).ToList(),
            drawPile = deck.Skip(27).ToList(), // 50-27=23
        };

        d.jokersInField = d.field.Where(c => c.isJoker).ToList();
        foreach (var j in d.jokersInField) d.field.Remove(j);
        RefillFieldFromDrawPile(d.field, d.drawPile, d.jokersInField.Count);

        return d;
    }

    // ── 딜링 (4인, 광판다) ──────────────────────────────
    public class Deal4P
    {
        public List<HwatuCard>[] hands; // 길이 4, 전부 7장 실제 손패
        public List<HwatuCard> field, drawPile;
        public List<HwatuCard> jokersInField = new();
    }

    /// <summary>
    /// 4인 딜 — 광판다 여부를 정하기 전에 **4명 전원이 진짜 7장 손패를
    /// 받는다**(사용자 확인 규칙: "패를 4인이 전부 받는다"). 그중 한 명은
    /// 이후 선언 절차(<see cref="GoStop3PGame"/>의 순차 참가 선언)에서 밀려나면
    /// 이 손패를 광 개수만 확인하는 데 쓰고 버린다 — 실제 플레이엔 못 들어간다.
    /// 48 = 7×4(전원 손패) + 6(필드) + 14(더미). 3인일 때보다 손패 배분에
    /// 더 많이 쓰인 만큼 더미가 얇아져(21→14) 판이 다소 짧아진다 — 딜 자체가
    /// 다르므로 자연스러운 차이다.
    /// </summary>
    public static Deal4P DealNew4PFull()
    {
        var deck = BuildFullDeckWithJokers();

        var hands = new List<HwatuCard>[4];
        int idx = 0;
        for (int s = 0; s < 4; s++)
        {
            hands[s] = deck.Skip(idx).Take(7).ToList();
            idx += 7;
        }

        var d = new Deal4P
        {
            hands    = hands,
            field    = deck.Skip(idx).Take(6).ToList(),
            drawPile = deck.Skip(idx + 6).ToList(), // 50-28-6=16
        };

        d.jokersInField = d.field.Where(c => c.isJoker).ToList();
        foreach (var j in d.jokersInField) d.field.Remove(j);
        RefillFieldFromDrawPile(d.field, d.drawPile, d.jokersInField.Count);

        return d;
    }

    // ── 캡처 ─────────────────────────────────────────────
    public class CaptureResult
    {
        public List<HwatuCard> captured = new(); // 이번에 손에 들어온 카드(낸 카드 포함)
        public bool placedOnField;               // 못 먹고 필드에 놓였는가
        public bool sweep;                       // 이 캡처로 필드가 완전히 비었는가(싹쓸이)
        public int  matchCount;                  // 필드에서 매칭된 장수(1=기본, 2=선택 대기, 3=싹쓸이성)
        /// <summary>matchCount==2일 때만 채워진다 — 아직 아무것도 캡처되지 않은
        /// "선택 대기" 상태. 호출자가 이 중 하나를 골라 <see cref="ResolveChoice"/>로
        /// 마무리해야 한다.</summary>
        public List<HwatuCard> choiceCandidates;
    }

    /// <summary>
    /// 카드 한 장을 필드에 내서 매칭을 해결한다. 필드 리스트를 직접 수정한다.
    /// 손패에서 내는 경우/더미에서 뒤집는 경우 모두 이 함수 하나로 처리한다 —
    /// 화투 규칙상 두 경우의 매칭 로직이 완전히 같기 때문이다.
    /// <br/>
    /// <b>필드에 같은 달이 2장 있으면 자동으로 둘 다 가져가지 않는다</b> —
    /// 어느 쪽을 가져갈지 플레이어가 직접 고른다(전통 규칙의 "따닥 자동 획득"
    /// 대신 이 프로젝트가 채택한 규칙). 그래서 2장 매칭은 필드도 안 건드리고
    /// 캡처도 안 한 채로 `choiceCandidates`만 채워 돌려준다 — 실제 캡처 확정은
    /// <see cref="ResolveChoice"/>가 한다. 1장(그냥 짝)과 3장(뻑 해소 — 이미
    /// 필드에 쌓여 있던 걸 통째로 쓸어가는 것이라 고를 여지가 없다)은 그대로
    /// 자동으로 전부 가져간다.
    /// </summary>
    public static CaptureResult Resolve(HwatuCard played, List<HwatuCard> field)
    {
        var matches = field.Where(c => c.month == played.month).ToList();
        var result = new CaptureResult();

        if (matches.Count == 0)
        {
            field.Add(played);
            result.placedOnField = true;
            return result;
        }
        result.matchCount = matches.Count;

        if (matches.Count == 2)
        {
            result.choiceCandidates = matches;
            return result;
        }

        // 1장 매칭(기본 짝) 또는 3장 매칭(뻑 해소 — 필드에 쌓여 있던 걸 통째로
        // 쓸어감, 고를 게 없다)은 "필드의 매칭 카드 + 낸 카드를 전부 획득"으로
        // 바로 처리된다.
        foreach (var m in matches) field.Remove(m);
        result.captured.Add(played);
        result.captured.AddRange(matches);
        result.sweep = field.Count == 0;
        return result;
    }

    /// <summary>
    /// <see cref="Resolve"/>가 2장 매칭으로 선택을 미뤘을 때, 실제로 고른 한
    /// 장으로 캡처를 확정한다. 보너스(따닥 피 스틸)는 없다 — 그냥 평범한
    /// 1:1 매칭과 동일하게 취급한다.
    /// </summary>
    /// <summary>
    /// 보너스피(조커)가 얹힌 뻑을 해소한다. 조커는 월이 없어(<see cref="HwatuCard.isJoker"/>)
    /// <see cref="Resolve"/>의 월 매칭 필터에 절대 안 걸린다 — 그래서 "anchor+extra
    /// 2장 + 조커 1장"이 함께 필드에 쌓여 있어도 <c>Resolve</c>는 월매칭 2장만 보고
    /// "선택 캡처"(matches.Count==2)로 잘못 분기한다. 그 상태에서 그냥 <see cref="ResolveChoice"/>로
    /// 고르게 하면 고르지 않은 1장과 조커가 필드에 영원히 남아버린다(아무도 못
    /// 가져가는 미아 카드 — "필드에 홀수 개가 남는다"는 신고의 원인). 호출자가
    /// 이 상황(그 달에 <c>ppeokBonusPi</c> 항목이 있음)을 감지하면 이 함수로
    /// 조커까지 포함해 3장(월매칭 2장+조커) 전부를 통째로 쓸어간다 — 일반 뻑
    /// 해소(matchCount==3)와 똑같이 처리되도록 matchCount를 3으로 맞춘다.
    /// </summary>
    public static CaptureResult ResolveJokerPpeok(HwatuCard played, List<HwatuCard> matched, HwatuCard joker, List<HwatuCard> field)
    {
        foreach (var m in matched) field.Remove(m);
        field.Remove(joker);
        var result = new CaptureResult { matchCount = 3 };
        result.captured.Add(played);
        result.captured.AddRange(matched);
        result.captured.Add(joker);
        result.sweep = field.Count == 0;
        return result;
    }

    public static CaptureResult ResolveChoice(HwatuCard played, HwatuCard chosen, List<HwatuCard> field)
    {
        field.Remove(chosen);
        var result = new CaptureResult { matchCount = 1 };
        result.captured.Add(played);
        result.captured.Add(chosen);
        result.sweep = field.Count == 0;
        return result;
    }

    /// <summary>
    /// 손패를 낼 때 폭탄인지 확인하고 처리한다. 폭탄은 전통 규칙 그대로
    /// <b>딱 한 조합만</b> 인정한다 — 손에 같은 달 3장(낼 카드 포함) +
    /// 필드에 정확히 1장. (한 달은 항상 정확히 4장이라 이 조합이면 그 달
    /// 카드가 전부 계산에 들어온다.)
    /// <br/>
    /// "손 2장 + 필드 2장"을 폭탄으로 치는 온라인 변형도 한때 넣었었지만,
    /// 이 조합은 확률상 훨씬 자주 발생해서(자연스러운 페어 매칭과 구분이
    /// 잘 안 된다) "이게 왜 폭탄이냐"는 신고를 받아 도로 뺐다 — 이제
    /// 손 2장+필드 2장은 그냥 <see cref="Resolve"/>의 2장 매칭(따닥)으로
    /// 처리된다. 손에 2장 + 필드에 1장만 있는 경우도 마찬가지로 폭탄이
    /// 아니다 — 나머지 1장이 아직 상대 손/덱에 남아 있어 조합이 완성되지
    /// 않았으므로 일반 1장 매칭으로 처리하고 남은 손패 1장은 손에 그대로 둔다.
    /// </summary>
    public static CaptureResult ResolveWithBomb(HwatuCard card, List<HwatuCard> hand,
                                                List<HwatuCard> field, out bool wasBomb)
    {
        var handPartners = hand.Where(c => c != card && c.month == card.month).ToList();
        var fieldMatches = field.Where(f => f.month == card.month).ToList();

        bool bomb3 = handPartners.Count == 2 && fieldMatches.Count == 1;

        if (bomb3)
        {
            wasBomb = true;
            hand.Remove(card);
            foreach (var p in handPartners) hand.Remove(p);
            foreach (var m in fieldMatches) field.Remove(m);

            var result = new CaptureResult { matchCount = fieldMatches.Count };
            result.captured.Add(card);
            result.captured.AddRange(handPartners);
            result.captured.AddRange(fieldMatches);
            result.sweep = field.Count == 0;
            return result;
        }

        wasBomb = false;
        hand.Remove(card);
        return Resolve(card, field);
    }

    // ── 점수 계산 ────────────────────────────────────────
    public class Score
    {
        public int gwang, godori, hongdan, chodan, cheongdan, ddi, yeolkkeut, pi, sweep;
        public int Total => gwang + godori + hongdan + chodan + cheongdan + ddi + yeolkkeut + pi + sweep;
    }

    /// <summary>
    /// 획득한 패 더미에서 기본 점수(고/스톱 배수 적용 전)를 계산한다.
    /// 매 턴 이 값으로 7점 도달 여부를 확인하고, 최종 정산에서도 같은 함수를 쓴다.
    /// </summary>
    public static Score CalcScore(List<HwatuCard> captured, int sweepBonusCount)
    {
        var s = new Score();

        int gwangCount = captured.Count(c => c.kind == HwatuKind.Gwang);
        // 3광은 12월(비광) 포함 여부로 갈린다 — 비광 없이 3장(1·3·8·11월 중 3장)이면
        // 3점, 비광을 포함하면 2점("비삼광"). 4광·5광은 비광 포함 여부와 무관하다.
        bool hasBiGwang = captured.Any(c => c.kind == HwatuKind.Gwang && c.month == 12);
        s.gwang = gwangCount switch
        {
            5 => 15,
            4 => 4,
            3 => hasBiGwang ? 2 : 3,
            _ => 0,
        };

        bool godori = captured.Count(c => c.godori) == 3;
        s.godori = godori ? 5 : 0;

        bool HasSet(int m1, int m2, int m3) =>
            captured.Any(c => c.kind == HwatuKind.Ddi && c.month == m1) &&
            captured.Any(c => c.kind == HwatuKind.Ddi && c.month == m2) &&
            captured.Any(c => c.kind == HwatuKind.Ddi && c.month == m3);
        s.hongdan   = HasSet(1, 2, 3)  ? 3 : 0;
        s.chodan    = HasSet(4, 5, 7)  ? 3 : 0;
        s.cheongdan = HasSet(6, 9, 10) ? 3 : 0;

        int ddiCount = captured.Count(c => c.kind == HwatuKind.Ddi);
        s.ddi = ddiCount >= 5 ? ddiCount - 4 : 0;

        // 9월 열끗(국화 술잔)은 useAsPi 선택에 따라 열끗/피 어느 쪽 집계에도
        // 안 들어가거나 둘 중 하나에만 들어간다 — EffectiveKind/EffectivePiValue로
        // 실제 역할을 반영한다.
        int yeolCount = captured.Count(c => c.EffectiveKind == HwatuKind.Yeolkkeut);
        s.yeolkkeut = yeolCount >= 5 ? yeolCount - 4 : 0;

        int piTotal = captured.Where(c => c.EffectiveKind == HwatuKind.Pi).Sum(c => c.EffectivePiValue);
        s.pi = piTotal >= 10 ? piTotal - 9 : 0;

        // 2026-08-23(design.md §23 확정): 싹쓸이는 점수를 가산하지 않는다 — 상대 피를
        // 뺏어오는 효과(StealPi 계열, ApplyMatchBonus에서 별도 처리)만 있고 점수는 없다.
        // sweepBonusCount 파라미터/Score.sweep 필드는 호출부 호환을 위해 남겨두되
        // 항상 0으로 고정해 Total에 반영되지 않게 한다. (과거엔 1회당 +1점이었음)
        s.sweep = 0;

        return s;
    }

    /// <summary>"왜 이 점수가 나왔는지" 화면에 보여줄 항목별 줄 목록 — 0점인
    /// 항목은 뺀다. UI 쪽(GoStopGame.cs/GoStop3PGame.cs)에서 이 위에 고
    /// 보너스·배수·최종 점수 줄을 이어 붙인다.</summary>
    public static List<string> FormatScoreLines(Score s)
    {
        var lines = new List<string>();
        void Add(string label, int pts) { if (pts != 0) lines.Add($"{label}  {pts}점"); }
        Add("광", s.gwang);
        Add("고도리", s.godori);
        Add("홍단", s.hongdan);
        Add("초단", s.chodan);
        Add("청단", s.cheongdan);
        Add("띠", s.ddi);
        Add("열끗", s.yeolkkeut);
        Add("피", s.pi);
        Add("싹쓸이", s.sweep);
        if (lines.Count == 0) lines.Add("(기본 점수 없음)");
        return lines;
    }

    /// <summary>점수 항목 한 줄 + 그 점수에 실제로 관여한 카드 목록. "광 3점"이라고만
    /// 적으면 뭐가 광 3장인지 안 보여서, 점수 상세 팝업이 항목 옆에 카드 실물을
    /// 같이 그릴 수 있도록 만들었다(사용자 요청 — "광3점이면 광3점에 관여한 패
    /// 3장이 같이 보였으면"). 카드 목록은 <see cref="FormatScoreLines"/>와 같은
    /// 판정 조건(<see cref="CalcScore"/>)을 그대로 재사용해서 텍스트와 카드가
    /// 어긋날 수 없다. 싹쓸이는 특정 카드가 아니라 "필드를 비웠다"는 이벤트라
    /// cards가 항상 빈 리스트다.</summary>
    public class ScoreLine
    {
        public string label;
        public int points;
        public List<HwatuCard> cards;
    }

    public static List<ScoreLine> BuildScoreLines(List<HwatuCard> captured, Score s)
    {
        var lines = new List<ScoreLine>();
        void Add(string label, int pts, IEnumerable<HwatuCard> cards)
        {
            if (pts == 0) return;
            lines.Add(new ScoreLine { label = label, points = pts, cards = cards.ToList() });
        }
        Add("광", s.gwang, captured.Where(c => c.kind == HwatuKind.Gwang));
        Add("고도리", s.godori, captured.Where(c => c.godori));
        Add("홍단", s.hongdan, captured.Where(IsHongdan));
        Add("초단", s.chodan, captured.Where(IsChodan));
        Add("청단", s.cheongdan, captured.Where(IsCheongdan));
        // 띠 5장부터의 "관여 카드"는 홍단/초단/청단에 쓰인 카드와 겹칠 수 있다
        // (예: 1월 띠가 홍단에도, 띠 5장 보너스에도 동시에 들어간다) — 실제
        // 점수 규칙상 한 카드가 여러 보너스에 동시에 기여하는 게 맞으므로
        // 의도된 중복이다.
        Add("띠", s.ddi, captured.Where(c => c.kind == HwatuKind.Ddi));
        Add("열끗", s.yeolkkeut, captured.Where(c => c.EffectiveKind == HwatuKind.Yeolkkeut));
        Add("피", s.pi, captured.Where(c => c.EffectiveKind == HwatuKind.Pi));
        Add("싹쓸이", s.sweep, Enumerable.Empty<HwatuCard>());
        return lines;
    }

    // ── 피 뺏기 (따닥·쪽·싹쓸이·폭탄 공통) ─────────────────
    /// <summary>
    /// 상대가 이미 획득한 더미에서 피를 빼앗는다. 실제로 존재하는 만큼만 옮긴다 —
    /// 상대가 피를 아직 하나도 못 모았으면 빚(음수 피)으로 남기지 않고 그냥 스킵한다.
    /// 홑피부터 내주고 쌍피는 홑피가 다 떨어졌을 때만 최후에 내준다 — 일반적인
    /// 고스톱 관례(사용자 확인). 예전엔 반대(쌍피부터)로 짜여 있었다.
    /// </summary>
    public static int StealPi(List<HwatuCard> from, List<HwatuCard> to, int count)
    {
        int moved = 0;
        for (int i = 0; i < count; i++)
        {
            // 2026-08-20 정정(사용자 신고 — "뻑 해소할 때 피를 안 뺏어온다") —
            // 여기만 kind/piValue(원본)를 썼다. 9월 열끗을 쌍피로 쓰기로
            // 정한 카드는 kind가 여전히 Yeolkkeut라서, 상대의 피 후보가
            // 그 카드 하나뿐이면 이 필터에 전혀 안 걸려 훔칠 게 있는데도
            // 못 찾고 그냥 빈손으로 끝났다(CalcScore 등 이 프로젝트의 다른
            // 모든 곳은 EffectiveKind/EffectivePiValue를 쓰는데 여기만
            // 빠져 있었다).
            var pi = from.Where(c => c.EffectiveKind == HwatuKind.Pi).OrderBy(c => c.EffectivePiValue).FirstOrDefault();
            if (pi == null) break;
            from.Remove(pi); to.Add(pi); moved++;
        }
        return moved;
    }

    // ── 세트 진행 상황 (홍단/청단/초단/고도리) ──────────────
    public enum SetState { Alive, Achieved, Blocked }

    /// <summary>
    /// 이 세트를 아직 모을 수 있는지 판정한다. 필요한 카드 중 하나라도 상대의
    /// 획득 더미에 있으면 <b>다시는 못 모은다</b>(Blocked) — 남은 곳(내 손패/필드/
    /// 상대 손패/더미)에 있는지는 알 방법이 없어도, "상대가 이미 가져간 것"만은
    /// 확실히 관측 가능한 정보라서 이것만으로 막힘을 판단한다.
    /// </summary>
    public static (SetState state, int have) CheckSet(
        List<HwatuCard> mine, List<HwatuCard> theirs, System.Func<HwatuCard, bool> pred, int need = 3)
    {
        int have = mine.Count(pred);
        if (have >= need) return (SetState.Achieved, have);
        if (theirs.Count(pred) > 0) return (SetState.Blocked, have);
        return (SetState.Alive, have);
    }

    public static bool IsHongdan(HwatuCard c)   => c.kind == HwatuKind.Ddi && (c.month == 1 || c.month == 2 || c.month == 3);
    public static bool IsChodan(HwatuCard c)    => c.kind == HwatuKind.Ddi && (c.month == 4 || c.month == 5 || c.month == 7);
    public static bool IsCheongdan(HwatuCard c) => c.kind == HwatuKind.Ddi && (c.month == 6 || c.month == 9 || c.month == 10);
    public static bool IsGodori(HwatuCard c)    => c.godori;

    // ── 실시간 피박/광박 위험 표시 ────────────────────────
    // "지금 판이 끝나면 이 사람이 피박/광박을 맞는가"를 매 턴 화면에 보여주기
    // 위한 함수 — 최종 정산(FinalScore/FinalScoreMulti)이 승자 확정 후에만
    // 계산하던 것과 같은 조건을, 아직 승자가 정해지지 않은 진행 중에도
    // "혹시 이 상대 중 누구라도 지금 바로 이기면"이라는 가정으로 미리
    // 계산한다. 판정 조건 자체는 최종 정산과 동일하게 맞춰서 실제 결과와
    // 어긋나지 않게 한다.
    /// <summary>
    /// <paramref name="myThreshold"/> — 피박 인정 상한(2인 맞고는 7,
    /// 3인 이상 고스톱은 <see cref="PI_BAK_THRESHOLD_3P"/>=5). 내 피가 0보다
    /// 크고 이 값 이하인 상태에서, 상대 중 누구라도 피 10장(<see cref="HwatuCard.EffectivePiValue"/>
    /// 합) 이상을 모았으면 위험.
    /// </summary>
    public static bool IsLivePiBakRisk(List<HwatuCard> mine, IEnumerable<List<HwatuCard>> others, int myThreshold)
    {
        int myPi = mine.Where(c => c.EffectiveKind == HwatuKind.Pi).Sum(c => c.EffectivePiValue);
        if (myPi <= 0 || myPi > myThreshold) return false;
        return others.Any(o => o.Where(c => c.EffectiveKind == HwatuKind.Pi).Sum(c => c.EffectivePiValue) >= 10);
    }

    /// <summary>내가 광이 하나도 없는데 상대 중 누구라도 광 3장 이상을 모았으면 위험.</summary>
    public static bool IsLiveGwangBakRisk(List<HwatuCard> mine, IEnumerable<List<HwatuCard>> others)
    {
        if (mine.Count(c => c.kind == HwatuKind.Gwang) > 0) return false;
        return others.Any(o => o.Count(c => c.kind == HwatuKind.Gwang) >= 3);
    }

    /// <summary>멍박 실시간 위험 판정 — 정식 "멍따"(동물 그림 열끗) 점수 규칙은
    /// 이 프로젝트가 의도적으로 안 넣었으므로(2인판 문서 참고), 열끗 전체를
    /// "멍" 패로 취급하는 단순화된 기준을 쓴다(피박/광박과 같은 성격의
    /// 실시간 안내 배지 — 실제 정산 로직에 연결된 페널티는 아니다). 덱에
    /// 열끗이 총 9장뿐이라(광5·열끗9·띠10·피24) <see cref="MEONG_BAK_THRESHOLD"/>
    /// (5장)를 과반 기준으로 잡았다. 내가 열끗 0장인데 상대 중 누구라도
    /// 그 이상을 모았으면 위험.</summary>
    public const int MEONG_BAK_THRESHOLD = 5;
    public static bool IsLiveMeongBakRisk(List<HwatuCard> mine, IEnumerable<List<HwatuCard>> others)
    {
        if (mine.Count(c => c.EffectiveKind == HwatuKind.Yeolkkeut) > 0) return false;
        return others.Any(o => o.Count(c => c.EffectiveKind == HwatuKind.Yeolkkeut) >= MEONG_BAK_THRESHOLD);
    }

    /// <summary>총통 — 딜 받은 손패에 같은 달 4장(그 달 전부)이 통째로 있는가.
    /// 조커(month==0)는 애초에 손패에 안 들어가므로 신경 안 써도 되지만
    /// 방어적으로 제외한다.</summary>
    public static bool IsChongtong(List<HwatuCard> hand) =>
        hand.Where(c => c.month != 0).GroupBy(c => c.month).Any(g => g.Count() == 4);

    // ── 고/스톱 배수 ─────────────────────────────────────
    /// <summary>고를 부른 횟수에 따른 기본 배수(역고가 아닐 때). 1~2회는 그대로, 3회부터 매판 2배씩.</summary>
    public static int GoMultiplier(int goCount)
    {
        if (goCount <= 2) return 1;
        int m = 1;
        for (int i = 0; i < goCount - 2; i++) m *= 2;
        return m;
    }

    /// <summary>
    /// 최종 점수 = 기본점수 × (고/역고 배수 × 흔들기 배수 × 폭탄 배수 × 광박 × 피박).
    /// 광박/피박은 <b>상대(진 쪽) 더미</b>를 봐야 판정되므로 여기서만 계산할 수 있다 —
    /// 매 턴 7점 체크에 쓰는 CalcScore와는 분리된 이유다.
    /// <br/>
    /// <paramref name="reversalCount"/> — 이번 판에서 "고를 부르는 쪽"이 바뀐(역전된)
    /// 횟수. 0이면 평범한 <see cref="GoMultiplier"/>를 쓰고, 1 이상이면 역고
    /// 배수(1회 역전=x2부터 시작, 역전마다 배로, 그 뒤로 내가 부른 고 한 번마다
    /// 추가로 x2)를 쓴다.
    /// <br/>
    /// <paramref name="overrideBaseScore"/> — 3연뻑(고정 3점 즉시 승리)이나
    /// 총통(딜 직후 즉시 승리, 손패에서 캡처 점수를 낼 수 없다)처럼 실제
    /// 캡처 점수 대신 고정 점수로 정산해야 할 때 쓴다. null이면 평소처럼
    /// <see cref="CalcScore"/>로 계산한다.
    /// </summary>
    /// <paramref name="extraMultiplier"/> — 총통(x4)처럼 위 조건들과 무관하게
    /// 통째로 곱해야 하는 고정 배수. 평소엔 1(영향 없음).
    public static int FinalScore(List<HwatuCard> myCaptured, int mySweeps, int myGoCount, int myHeundeulCount,
                                 int myBombCount, List<HwatuCard> opponentCaptured, int reversalCount,
                                 int? overrideBaseScore = null, int extraMultiplier = 1) =>
        FinalScoreBreakdown(myCaptured, mySweeps, myGoCount, myHeundeulCount, myBombCount,
            opponentCaptured, reversalCount, overrideBaseScore, extraMultiplier).finalScore;

    /// <summary>점수 항목별 근거를 전부 담은 결과 — "왜 이 점수가 나왔는지" 화면에
    /// 보여주기 위한 것. 계산 자체는 <see cref="FinalScore"/>와 완전히 같다(이
    /// 함수가 실제 계산을 하고 FinalScore는 finalScore만 뽑아 쓰는 얇은 래퍼) —
    /// 그래서 로직이 두 곳으로 갈라져 서로 어긋날 위험이 없다.</summary>
    public class ScoreBreakdown
    {
        public Score baseScore;      // 광/고도리/홍단/초단/청단/띠/열끗/피/싹쓸이 각 항목
        public int goCount;
        public int goBonus;          // 고 횟수만큼 점수에 더해진 값(=goCount)
        public int subtotal;         // baseScore.Total + goBonus
        public int goMultiplier;     // 고배수(역고면 역고 공식)
        public bool isReversalGo;
        public int heundeulCount, bombCount;
        public bool gwangBak, piBak;
        public int extraMultiplier;  // 총통 등 — 평소 1
        public int totalMultiplier;  // 고배수 × 흔들기 × 폭탄 × 광박 × 피박 × extraMultiplier
        public int finalScore;
    }

    public static ScoreBreakdown FinalScoreBreakdown(List<HwatuCard> myCaptured, int mySweeps, int myGoCount,
        int myHeundeulCount, int myBombCount, List<HwatuCard> opponentCaptured, int reversalCount,
        int? overrideBaseScore = null, int extraMultiplier = 1)
    {
        var b = new ScoreBreakdown
        {
            baseScore = CalcScore(myCaptured, mySweeps),
            goCount = myGoCount,
            heundeulCount = myHeundeulCount,
            bombCount = myBombCount,
            isReversalGo = reversalCount > 0,
            extraMultiplier = extraMultiplier,
        };

        // 고는 배수만이 아니라 점수 자체에도 매 고마다 1점씩 쌓인다
        // ("1고·2고: 점수에 1점씩 추가", "3고 이상: 1점씩 추가하면서 동시에 x2배씩").
        b.goBonus = myGoCount;
        b.subtotal = (overrideBaseScore ?? b.baseScore.Total) + b.goBonus;

        int mult;
        if (reversalCount > 0)
        {
            // 역고 — 상대가 먼저 고를 부른 뒤 내가 앞질러서 고를 부른 경우.
            // 역전 시점에 x2, 역전이 한 번 더 겹치면(역고의 역고) x4부터
            // 다시 시작, 그 뒤로 내가 부른 고 한 번마다 추가로 x2씩.
            mult = (1 << reversalCount) * (1 << Mathf.Max(0, myGoCount - 1));
        }
        else
        {
            mult = GoMultiplier(myGoCount);
        }
        b.goMultiplier = mult;

        for (int i = 0; i < myHeundeulCount; i++) mult *= 2;   // 흔들기 1회당 x2
        for (int i = 0; i < myBombCount; i++) mult *= 2;       // 폭탄 1회당 x2

        int myGwang = myCaptured.Count(c => c.kind == HwatuKind.Gwang);
        int oppGwang = opponentCaptured.Count(c => c.kind == HwatuKind.Gwang);
        b.gwangBak = myGwang >= 3 && oppGwang == 0;   // 광박 — 상대가 광을 하나도 못 모았다
        if (b.gwangBak) mult *= 2;

        int myPi = myCaptured.Where(c => c.EffectiveKind == HwatuKind.Pi).Sum(c => c.EffectivePiValue);
        int oppPi = opponentCaptured.Where(c => c.EffectiveKind == HwatuKind.Pi).Sum(c => c.EffectivePiValue);
        // 피박 — 맞고 기준 7장 이하. 상대가 피를 아예 한 장도 못 모았으면(0장)
        // 오히려 피박이 아니다 — "한 장도 못 먹으면 그 판 자체가 없던 일"
        // 규칙과 같은 논리다.
        b.piBak = myPi >= 10 && oppPi > 0 && oppPi <= 7;
        if (b.piBak) mult *= 2;

        mult *= extraMultiplier;
        b.totalMultiplier = mult;
        b.finalScore = b.subtotal * mult; // subtotal은 overrideBaseScore(총통·쓰리뻑)가 있으면 이미 그 값+고보너스다

        return b;
    }

    // ── 3인 이상 정산 ────────────────────────────────────
    /// <summary>맞고(2인) 7장과 다르게 3인 고스톱은 5장 이하가 피박 기준이다
    /// — 검색으로 교차 확인(맞고용 상수 7과 별개로 둔다).</summary>
    public const int PI_BAK_THRESHOLD_3P = 5;

    /// <summary>패자별 정산 결과. 독박이면 한 명만 <see cref="amounts"/>가 0이 아니고
    /// 나머지는 0이다 — 그 한 명이 전원분을 몰아서 낸다.</summary>
    public class MultiPayout
    {
        public List<int> amounts = new(); // loserCaptured와 같은 순서
        public int baseTotal;             // 광박/피박 전, 참고용

        // "왜 이 점수가 나왔는지" 화면에 보여주기 위한 항목별 근거. 광박/피박은
        // 패자 개인마다 갈릴 수 있어서(3인 이상 규칙 — 클래스 doc 참고)
        // amounts와 같은 순서의 리스트로 따로 둔다. 고/흔들기/폭탄 배수는
        // 승자 쪽 행동이라 모든 패자에게 공통이다.
        public Score baseScore;
        public int goCount, goBonus, subtotal, goMultiplier, heundeulCount, bombCount, extraMultiplier;
        public List<bool> gwangBakPerLoser = new();
        public List<bool> piBakPerLoser = new();

        // 독박(고박) 대상 — amounts와 같은 인덱스 체계(loserCaptured 순서).
        // -1이면 독박 없음. 점수 상세 팝업이 "왜 이 사람만 전액을 냈는지"를
        // 보여줄 수 있도록 결과에 남겨둔다 — 예전엔 amounts만 보고 "한 명만
        // 냈다"를 유추해야 해서 점수 상세에 독박 여부가 전혀 안 보였다
        // ("점수 상세에 독박이 안 나온다"는 신고로 추가).
        public int dokbakLoserIndex = -1;
    }

    /// <summary>
    /// 3인 이상 정산 — 광박/피박은 <b>패자 개인의 획득 더미</b> 기준으로 각자
    /// 따로 판정한다("패자 그룹 전체"가 아니다 — 검색으로 확인한 관례. 3인
    /// 판에서 한 명은 광을 갖고 있고 다른 한 명은 하나도 없을 수 있는데,
    /// 광 없는 그 사람만 광박을 문다). 고/흔들기/폭탄 배수는 승자 쪽 행동이라
    /// 모든 패자에게 동일하게 적용된다.
    /// <br/>
    /// <paramref name="dokbakLoserIndex"/> — 독박 대상의 <paramref name="loserCaptured"/>
    /// 인덱스. -1이면 독박 없음(각자 자기 몫만 낸다). 지정하면 그 사람이
    /// 전원분 합계를 몰아서 내고 나머지는 0원이 된다. 역고 배수는 3인 이상에서
    /// "누구의 고를 누가 앞질렀는가"가 다자간이라 애매해서 <b>이번 버전에서는
    /// 뺐다</b>(2인 맞고의 <see cref="FinalScore"/>와 달리 reversalCount 파라미터가
    /// 없다) — 항상 <see cref="GoMultiplier"/>만 쓴다.
    /// </summary>
    public static MultiPayout FinalScoreMulti(List<HwatuCard> myCaptured, int mySweeps, int myGoCount,
        int myHeundeulCount, int myBombCount, List<List<HwatuCard>> loserCaptured, int wonPerPoint,
        int dokbakLoserIndex = -1, int? overrideBaseScore = null, int extraMultiplier = 1)
    {
        var cs = CalcScore(myCaptured, mySweeps);
        int baseScore = overrideBaseScore ?? cs.Total;
        baseScore += myGoCount;

        int goMult = GoMultiplier(myGoCount);
        int mult = goMult;
        for (int i = 0; i < myHeundeulCount; i++) mult *= 2;
        for (int i = 0; i < myBombCount; i++) mult *= 2;
        mult *= extraMultiplier;

        int myGwang = myCaptured.Count(c => c.kind == HwatuKind.Gwang);
        int myPi = myCaptured.Where(c => c.EffectiveKind == HwatuKind.Pi).Sum(c => c.EffectivePiValue);

        var result = new MultiPayout
        {
            baseScore = cs,
            goCount = myGoCount,
            goBonus = myGoCount,
            subtotal = baseScore,
            goMultiplier = goMult,
            heundeulCount = myHeundeulCount,
            bombCount = myBombCount,
            extraMultiplier = extraMultiplier,
        };
        int total = 0;
        foreach (var lc in loserCaptured)
        {
            int m = mult;
            int oppGwang = lc.Count(c => c.kind == HwatuKind.Gwang);
            bool gwangBak = myGwang >= 3 && oppGwang == 0; // 광박 — 이 패자 개인 기준
            if (gwangBak) m *= 2;

            int oppPi = lc.Where(c => c.EffectiveKind == HwatuKind.Pi).Sum(c => c.EffectivePiValue);
            bool piBak = myPi >= 10 && oppPi > 0 && oppPi <= PI_BAK_THRESHOLD_3P; // 피박 — 이 패자 개인 기준
            if (piBak) m *= 2;
            result.gwangBakPerLoser.Add(gwangBak);
            result.piBakPerLoser.Add(piBak);

            // 이번 판 한 장도 못 먹은 패자는 정산에서 빠진다(2인판과 같은 논리).
            int amount = lc.Count == 0 ? 0 : baseScore * m * wonPerPoint;
            result.amounts.Add(amount);
            total += amount;
        }

        if (dokbakLoserIndex >= 0 && dokbakLoserIndex < result.amounts.Count)
        {
            for (int i = 0; i < result.amounts.Count; i++)
                result.amounts[i] = (i == dokbakLoserIndex) ? total : 0;
            result.dokbakLoserIndex = dokbakLoserIndex;
        }

        result.baseTotal = baseScore * mult;
        return result;
    }
}
