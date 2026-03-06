using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public struct ChampionResultUpdate
{
    public static ChampionResultUpdate e;

    public static void Send()
    {
        EventManager.TriggerEvent(e);
    }
}
public class ChampionLeagueTable : UIBehaviour, EventListener<ChampionResultUpdate>, EventListener<PopupEvent>
{
    [Header("PRIZE LAYER")]
    [SerializeField]
    private GameObject prizeLayer = null;
    [SerializeField]
    private Text winner_prize = null;
    [SerializeField]
    private Text finalist_prize = null;

    [SerializeField]
    private Text winner_magnite = null;

    [SerializeField]
    private Text finalist_magnite = null;

    [SerializeField]
    private Text semifinalist_prize = null;

    public enum ROUND_INDEX {
        NONE = -1,

        ROUND16_START = 1,
        ROUND16_A = ROUND16_START,
        ROUND16_B,
        ROUND16_C,
        ROUND16_D,
        ROUND16_E,
        ROUND16_F,
        ROUND16_G,
        ROUND16_H,
        ROUND16_MAX = ROUND16_H,

        ROUND8_START = 11,
        ROUND8_A = ROUND8_START,
        ROUND8_B,
        ROUND8_C,
        ROUND8_D,
        ROUND8_MAX = ROUND8_D,

        SEMI_FINAL_START = 101,
        SEMI_FINAL_A = SEMI_FINAL_START,
        SEMI_FINAL_B,
        SEMI_FINAL_MAX = SEMI_FINAL_B,

        FINAL_START = 1001,
        FINAL = FINAL_START,
        FINAL_MAX = FINAL,
    }

    public enum UI_INDEX
    {
        NONE = -1,
        ROUND16_A = 0,
        ROUND16_B,
        ROUND16_C,
        ROUND16_D,
        ROUND16_E,
        ROUND16_F,
        ROUND16_G,
        ROUND16_H,
        ROUND8_A,
        ROUND8_B,
        ROUND8_C,
        ROUND8_D,
        SEMI_FINAL_A,
        SEMI_FINAL_B,
        FINAL,
    }

    [SerializeField]
    ChampionTableRound[] RoundSlots;

    [SerializeField]
    Text TitleText;

    [SerializeField]
    ScrollRect TableScroll;

    [SerializeField]
    GameObject ShowResultAllButton = null;

    Vector2 prevCanvasSize = Vector2.zero;

    UI_INDEX GetUIIndexByRoundIndex(ROUND_INDEX index)
    {
        switch (index)
        {
            case ROUND_INDEX.ROUND16_A: return UI_INDEX.ROUND16_A;
            case ROUND_INDEX.ROUND16_B: return UI_INDEX.ROUND16_B;
            case ROUND_INDEX.ROUND16_C: return UI_INDEX.ROUND16_C;
            case ROUND_INDEX.ROUND16_D: return UI_INDEX.ROUND16_D;
            case ROUND_INDEX.ROUND16_E: return UI_INDEX.ROUND16_E;
            case ROUND_INDEX.ROUND16_F: return UI_INDEX.ROUND16_F;
            case ROUND_INDEX.ROUND16_G: return UI_INDEX.ROUND16_G;
            case ROUND_INDEX.ROUND16_H: return UI_INDEX.ROUND16_H;
            case ROUND_INDEX.ROUND8_A: return UI_INDEX.ROUND8_A;
            case ROUND_INDEX.ROUND8_B: return UI_INDEX.ROUND8_B;
            case ROUND_INDEX.ROUND8_C: return UI_INDEX.ROUND8_C;
            case ROUND_INDEX.ROUND8_D: return UI_INDEX.ROUND8_D;
            case ROUND_INDEX.SEMI_FINAL_A: return UI_INDEX.SEMI_FINAL_A;
            case ROUND_INDEX.SEMI_FINAL_B: return UI_INDEX.SEMI_FINAL_B;
            case ROUND_INDEX.FINAL: return UI_INDEX.FINAL;
        }

        return UI_INDEX.NONE;
    }
    protected override void OnEnable()
    {
        Init();
        EventManager.AddListener<ChampionResultUpdate>(this);
        EventManager.AddListener<PopupEvent>(this);
    }

    protected override void OnDisable()
    {
        EventManager.RemoveListener<ChampionResultUpdate>(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        EventManager.RemoveListener<PopupEvent>(this);
    }

    protected override void Start()
    {
        base.Start();

        RefreshSize();
    }

    public void OnEvent(ChampionResultUpdate eventType)
    {
        RefreshUI();
    }

    public void OnEvent(PopupEvent eventType)
    {
        Invoke("BlurCheck", 0.01f);
    }

    void BlurCheck()
    {
        gameObject.SetActive(!UICanvas.Instance.BlurOpening);
    }

    void Clear()
    {
        for (ROUND_INDEX i = ROUND_INDEX.ROUND16_A; i <= ROUND_INDEX.FINAL; i++)
        {
            int uiindex = (int)GetUIIndexByRoundIndex(i);
            if (uiindex < 0 || uiindex >= RoundSlots.Length || RoundSlots[uiindex] == null)
                continue;

            RoundSlots[uiindex].SetRoundInfo(i, null, null, SandboxNetwork.eChampionWinType.None);
        }
    }

    void Init()
    {
        Clear();
        TitleText.text = StringData.GetStringFormatByStrKey("챔피언대전이름", ChampionManager.Instance.CurChampionInfo.CurSeason);

        RefreshUI();
    }

    void RefreshUI()
    {
        bool resultHide = false;
        for (ROUND_INDEX i = ROUND_INDEX.ROUND16_A; i <= ROUND_INDEX.FINAL; i++)
        {
            int uiindex = (int)GetUIIndexByRoundIndex(i);
            if (uiindex < 0 || uiindex >= RoundSlots.Length || RoundSlots[uiindex] == null)
                continue;

            if (!ChampionManager.Instance.CurChampionInfo.MatchData.ContainsKey(i))
            {
                RoundSlots[uiindex].SetRoundInfo(i, ChampionMatchData.GetASideUser(i), ChampionMatchData.GetBSideUser(i), eChampionWinType.None);
                continue;
            }

            var data = ChampionManager.Instance.CurChampionInfo.MatchData[i];
            RoundSlots[uiindex].SetRoundInfo(i, data.A_SIDE, data.B_SIDE, data.MatchResult);

            if (data.MatchResult == eChampionWinType.SIDE_A_WIN || data.MatchResult == eChampionWinType.SIDE_B_WIN)
            {
                if (!ChampionMatchData.IsShowResult(i))
                {
                    resultHide = true;
                }
            }
        }

        ShowResultAllButton.SetActive(resultHide);

        SetPrizeText();
    }
    public void SetPrizeText()
    {
        if (winner_prize == null || finalist_prize == null || semifinalist_prize == null) return;

        JObject prize = ChampionManager.Instance.CurChampionInfo.TotalPrize; 
        
        if (prize == null || !prize.ContainsKey("total") || prize["total"].Value<int>() <= 0 || !User.Instance.ENABLE_P2E)
        {
            prizeLayer.SetActive(false);
            return;
        }

        winner_prize.text = "";
        finalist_prize.text = "";
        semifinalist_prize.text = "";

        winner_magnite.text = "0";// SBFunc.CommaFromNumber(GameConfigTable.GetConfigIntValue("CHAMPION_WINNER_MAGNITE", 1000));
        finalist_magnite.text = "0";// SBFunc.CommaFromNumber(GameConfigTable.GetConfigIntValue("CHAMPION_FINALIST_MAGNITE", 1000));

        if (prize.ContainsKey("final_win"))
        {
            string txt = "";
            string[] prize_array = prize["final_win"].Value<string>().Split('.');
            if(prize_array.Length > 0)
                txt += SBFunc.CommaFromNumber(int.Parse(prize_array[0]));
            if (prize_array.Length > 1)
                txt += "." + prize_array[1];

            winner_prize.text = txt;
        }
        if (prize.ContainsKey("final_lost"))
        {
            string txt = "";
            string[] prize_array = prize["final_lost"].Value<string>().Split('.');
            if (prize_array.Length > 0)
                txt += SBFunc.CommaFromNumber(int.Parse(prize_array[0]));
            if (prize_array.Length > 1)
                txt += "." + prize_array[1];

            finalist_prize.text = txt;
        }
        if (prize.ContainsKey("semi_final"))
        {
            string txt = "";
            string[] prize_array = prize["semi_final"].Value<string>().Split('.');
            if (prize_array.Length > 0)
                txt += SBFunc.CommaFromNumber(int.Parse(prize_array[0]));
            if (prize_array.Length > 1)
                txt += "." + prize_array[1];

            semifinalist_prize.text = txt;
        }

        winner_magnite.text = SBFunc.CommaFromNumber(ChampionManager.Instance.CurChampionInfo.SurpportInfo.FinalPrize);
        finalist_magnite.text = SBFunc.CommaFromNumber(ChampionManager.Instance.CurChampionInfo.SurpportInfo.SemiPrize);
    }

    public void OnClickPrizePopup()
    {
        //ChampionPrizePopup.OpenPopup();
    }

    public void RefreshSize()
    {
        var canvas = transform.GetComponentInParent<Canvas>();
        if (canvas == null)
            return;

        Vector2 canvasSize = (canvas.transform as RectTransform).sizeDelta;
        if (prevCanvasSize == canvasSize)
            return;

        prevCanvasSize = canvasSize;
        Vector2 mySize = (transform as RectTransform).sizeDelta;
        
        float ratio = 1.0f;
        if (mySize.x < canvasSize.x)
        {
            ratio = Mathf.Max(ratio, canvasSize.x / mySize.x);
        }

        if (mySize.y < canvasSize.y)
        {
            ratio = Mathf.Max(ratio, canvasSize.y / mySize.y);
        }

        if (ratio != (transform as RectTransform).localScale.x)
        {
            (transform as RectTransform).localScale = Vector3.one * ratio;
            TableScroll.normalizedPosition = new Vector2(0.5f, 1.0f);
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        RefreshSize();
    }

    public void OnResultShowAll()
    {
        for (ROUND_INDEX i = ROUND_INDEX.ROUND16_A; i <= ROUND_INDEX.FINAL; i++)
        {
            if (!ChampionManager.Instance.CurChampionInfo.MatchData.ContainsKey(i))
            {
                continue;
            }

            var data = ChampionManager.Instance.CurChampionInfo.MatchData[i];
            if (data != null)
            {
                if (data.MatchResult == eChampionWinType.SIDE_A_WIN || data.MatchResult == eChampionWinType.SIDE_B_WIN)
                {
                    data.ShowResult(false, false);
                }
            }
        }

        ShowResultAllButton.SetActive(false);
        ChampionResultUpdate.Send();
    }
}
