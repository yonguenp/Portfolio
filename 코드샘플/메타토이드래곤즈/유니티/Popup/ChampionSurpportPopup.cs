using Coffee.UIEffects;
using Google.Impl;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Newtonsoft.Json.Linq;

namespace SandboxNetwork
{
    public class ChampionSurpportPopup : Popup<PopupBase>, EventListener<ChampionSurpportUpdate>
    {
        [SerializeField] Sprite selectedTab;
        [SerializeField] Sprite normalTab;

        [SerializeField] Image[] Tabs;

        [SerializeField] GameObject[] StatPanels;
        [SerializeField] Text CurTotalStats;
        [SerializeField] Text[] CurSurpportInfo;
        [SerializeField] Text MySurpportValue;

        [SerializeField] RectTransform TotalRect;
        [SerializeField] RectTransform AngelRect_Only;
        [SerializeField] RectTransform WonderRect_Only;
        [SerializeField] RectTransform LunaRect_Only;
        [SerializeField] RectTransform AngelRect_L;
        [SerializeField] RectTransform WonderRect_L;
        [SerializeField] RectTransform WonderRect_C;
        [SerializeField] RectTransform WonderRect_R;
        [SerializeField] RectTransform LunaRect_R;

        [SerializeField] Text AngelRate;
        [SerializeField] Text WonderRate;
        [SerializeField] Text LunaRate;

        [SerializeField] GameObject[] AngelInfoPanel;
        [SerializeField] GameObject[] WonderInfoPanel;
        [SerializeField] GameObject[] LunaInfoPanel;
        [SerializeField] Text[] StatValue;

        [SerializeField] ItemFrame[] CloneMyReward;
        [SerializeField] ItemFrame[] CloneCurReward;
        [SerializeField] Button SupportBtn;
        [SerializeField] Text BtnTimer;
        ChampionSurpportInfo.eSurpportType currentInfo;
        int remain = 0;
        public static void OpenPopup(ChampionSurpportInfo.eSurpportType type)
        {
            for (ChampionLeagueTable.ROUND_INDEX i = ChampionLeagueTable.ROUND_INDEX.ROUND16_A; i <= ChampionLeagueTable.ROUND_INDEX.FINAL; i++)
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

            WWWForm param = new WWWForm();
            NetworkManager.Send("unifiedtournament/supportinfo", param, (data) => {
                if (data.ContainsKey("support"))
                    ChampionManager.Instance.CurChampionInfo.SurpportInfo.SetData((JObject)data["support"]);

                var popup = PopupManager.OpenPopup<ChampionSurpportPopup>();
                popup.InitTab(type);

                PopupManager.Instance.Top.SetMagniteUI(true);
            });
        }

        public void InitTab(ChampionSurpportInfo.eSurpportType type)
        {
            if (ChampionSurpportInfo.eSurpportType.NONE == type)
            {
                if (currentInfo == ChampionSurpportInfo.eSurpportType.NONE)
                    currentInfo = ChampionSurpportInfo.eSurpportType.PHYS_DMG_RESIS;
            }
            else
            {
                currentInfo = type;
            }

            ChampionSurpportInfo.SurpportDetial data = ChampionManager.Instance.CurChampionInfo.SurpportInfo.GetSurpportInfo(currentInfo);
            List<ChampionMatchData> currentRoundMatchs = new List<ChampionMatchData>();
            foreach (var matchData in ChampionManager.Instance.CurChampionInfo.MatchData)
            {
                switch (matchData.Key)
                {
                    case ChampionLeagueTable.ROUND_INDEX.ROUND16_A:
                    case ChampionLeagueTable.ROUND_INDEX.ROUND16_B:
                    case ChampionLeagueTable.ROUND_INDEX.ROUND16_C:
                    case ChampionLeagueTable.ROUND_INDEX.ROUND16_D:
                    case ChampionLeagueTable.ROUND_INDEX.ROUND16_E:
                    case ChampionLeagueTable.ROUND_INDEX.ROUND16_F:
                    case ChampionLeagueTable.ROUND_INDEX.ROUND16_G:
                    case ChampionLeagueTable.ROUND_INDEX.ROUND16_H:
                        if (ChampionManager.Instance.CurChampionInfo.CurState == ChampionInfo.ROUND_STATE.ROUND_OF_16)
                            currentRoundMatchs.Add(matchData.Value);
                        break;
                    case ChampionLeagueTable.ROUND_INDEX.ROUND8_A:
                    case ChampionLeagueTable.ROUND_INDEX.ROUND8_B:
                    case ChampionLeagueTable.ROUND_INDEX.ROUND8_C:
                    case ChampionLeagueTable.ROUND_INDEX.ROUND8_D:
                        if (ChampionManager.Instance.CurChampionInfo.CurState == ChampionInfo.ROUND_STATE.QUARTER_FINALS)
                            currentRoundMatchs.Add(matchData.Value);
                        break;
                    case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_A:
                    case ChampionLeagueTable.ROUND_INDEX.SEMI_FINAL_B:
                        if (ChampionManager.Instance.CurChampionInfo.CurState == ChampionInfo.ROUND_STATE.SEMI_FINALS)
                            currentRoundMatchs.Add(matchData.Value);
                        break;
                    case ChampionLeagueTable.ROUND_INDEX.FINAL:
                        if (ChampionManager.Instance.CurChampionInfo.CurState == ChampionInfo.ROUND_STATE.FINAL)
                            currentRoundMatchs.Add(matchData.Value);
                        break;
                }
            }

            int angelCount = 0;
            int wonderCount = 0;
            int lunaCount = 0;

            foreach (var match in currentRoundMatchs)
            {
                if (match == null)
                    continue;
                if (match.A_SIDE != null)
                {
                    switch (match.A_SIDE.SERVER)
                    {
                        case 1:
                            angelCount++; break;
                        case 2:
                            wonderCount++; break;
                        case 3:
                            lunaCount++; break;
                    }
                }

                if (match.B_SIDE != null)
                {
                    switch (match.B_SIDE.SERVER)
                    {
                        case 1:
                            angelCount++; break;
                        case 2:
                            wonderCount++; break;
                        case 3:
                            lunaCount++; break;
                    }
                }
            }

            int total = 0;
            if (angelCount > 0)
                total += data.AngelSurpportValue;
            if (wonderCount > 0)
                total += data.WonderSurpportValue;
            if (lunaCount > 0)
                total += data.LunaSurpportValue;

            int angelRate = 0;
            int wonderRate = 0;
            int lunaRate = 0;
            int aliveServerCount = 0;

            for (int i = 0; i < 3; i++)
            {
                if ((int)currentInfo == i)
                {
                    Tabs[i].sprite = selectedTab;
                    StatPanels[i].SetActive(true);
                }
                else
                {
                    Tabs[i].sprite = normalTab;
                    StatPanels[i].SetActive(false);
                }
            }

            switch (currentInfo)
            {
                case ChampionSurpportInfo.eSurpportType.PHYS_DMG_RESIS:
                    CurTotalStats.text = StringData.GetStringFormatByStrKey("지원_총물리저항", "+" + data.TotalValue + "%");
                    if (ChampionManager.Instance.CurChampionInfo.CurState < ChampionInfo.ROUND_STATE.RESULT)
                    {
                        StatValue[0].text = StringData.GetStringFormatByStrKey("지원_물댐저", "+" + data.AngelStatValue + "%");
                        StatValue[1].text = StringData.GetStringFormatByStrKey("지원_물댐저", "+" + data.WonderStatValue + "%");
                        StatValue[2].text = StringData.GetStringFormatByStrKey("지원_물댐저", "+" + data.LunaStatValue + "%");
                    }
                    else
                    {
                        StatValue[0].text = StringData.GetStringFormatByStrKey("지원_종료");
                        StatValue[1].text = StringData.GetStringFormatByStrKey("지원_종료");
                        StatValue[2].text = StringData.GetStringFormatByStrKey("지원_종료");
                    }
                    break;
                case ChampionSurpportInfo.eSurpportType.ALL_ELEMENT_DMG_RESIS:
                    CurTotalStats.text = StringData.GetStringFormatByStrKey("지원_총속성저항", "+" + data.TotalValue + "%");
                    if (ChampionManager.Instance.CurChampionInfo.CurState < ChampionInfo.ROUND_STATE.RESULT)
                    {
                        StatValue[0].text = StringData.GetStringFormatByStrKey("지원_속댐저", "+" + data.AngelStatValue + "%");
                        StatValue[1].text = StringData.GetStringFormatByStrKey("지원_속댐저", "+" + data.WonderStatValue + "%");
                        StatValue[2].text = StringData.GetStringFormatByStrKey("지원_속댐저", "+" + data.LunaStatValue + "%");
                    }
                    else
                    {
                        StatValue[0].text = StringData.GetStringFormatByStrKey("지원_종료");
                        StatValue[1].text = StringData.GetStringFormatByStrKey("지원_종료");
                        StatValue[2].text = StringData.GetStringFormatByStrKey("지원_종료");
                    }
                    break;
                case ChampionSurpportInfo.eSurpportType.CRI_DMG_RESIS:
                    CurTotalStats.text = StringData.GetStringFormatByStrKey("지원_총크리저항", "+" + data.TotalValue + "%");
                    if (ChampionManager.Instance.CurChampionInfo.CurState < ChampionInfo.ROUND_STATE.RESULT)
                    {
                        StatValue[0].text = StringData.GetStringFormatByStrKey("지원_크댐저", "+" + data.AngelStatValue + "%");
                        StatValue[1].text = StringData.GetStringFormatByStrKey("지원_크댐저", "+" + data.WonderStatValue + "%");
                        StatValue[2].text = StringData.GetStringFormatByStrKey("지원_크댐저", "+" + data.LunaStatValue + "%");
                    }
                    else
                    {
                        StatValue[0].text = StringData.GetStringFormatByStrKey("지원_종료");
                        StatValue[1].text = StringData.GetStringFormatByStrKey("지원_종료");
                        StatValue[2].text = StringData.GetStringFormatByStrKey("지원_종료");
                    }
                    break;
            }


            CurSurpportInfo[0].color = Color.gray;
            if (data.AngelRateValue > 0)
            {
                angelRate = data.AngelRateValue;
                CurSurpportInfo[0].color = Color.white;
                aliveServerCount++;
            }
            CurSurpportInfo[1].color = Color.gray;
            if (data.WonderRateValue > 0)
            {
                wonderRate = data.WonderRateValue;
                CurSurpportInfo[1].color = Color.white;
                aliveServerCount++;
            }
            CurSurpportInfo[2].color = Color.gray;
            if (data.LunaRateValue > 0)
            {
                lunaRate = data.LunaRateValue;
                CurSurpportInfo[2].color = Color.white;
                aliveServerCount++;
            }

            CurSurpportInfo[0].text = SBFunc.CommaFromNumber(data.AngelSurpportValue) + " (" + angelRate + "%)";
            CurSurpportInfo[1].text = SBFunc.CommaFromNumber(data.WonderSurpportValue) + " (" + wonderRate + "%)";
            CurSurpportInfo[2].text = SBFunc.CommaFromNumber(data.LunaSurpportValue) + " (" + lunaRate + "%)";

            MySurpportValue.text = SBFunc.CommaFromNumber(data.MySurpportValue);

            var width = TotalRect.sizeDelta.x;
            var height = TotalRect.sizeDelta.y;
            var pos = new Vector3(width * -0.5f, 0f, 0f);
            float w = 0f;

            RectTransform AngelTarget = null;
            RectTransform WonderTarget = null;
            RectTransform LunaTarget = null;
            switch (aliveServerCount)
            {
                case 3:
                    AngelRect_Only.gameObject.SetActive(false);
                    WonderRect_Only.gameObject.SetActive(false);
                    LunaRect_Only.gameObject.SetActive(false);
                    AngelRect_L.gameObject.SetActive(false);
                    WonderRect_L.gameObject.SetActive(false);
                    WonderRect_C.gameObject.SetActive(false);
                    WonderRect_R.gameObject.SetActive(false);
                    LunaRect_R.gameObject.SetActive(false);

                    AngelTarget = AngelRect_L;
                    AngelRect_L.gameObject.SetActive(true);
                    w = data.AngelRateValue * 0.01f * width;
                    AngelRect_L.sizeDelta = new Vector2(w, height);
                    AngelRect_L.localPosition = pos;
                    pos.x += w;

                    WonderTarget = WonderRect_C;
                    w = data.WonderRateValue * 0.01f * width;
                    WonderRect_C.gameObject.SetActive(true);
                    WonderRect_C.sizeDelta = new Vector2(w, height);
                    WonderRect_C.localPosition = pos;
                    pos.x += w;

                    LunaTarget = LunaRect_R;
                    w = data.LunaRateValue * 0.01f * width;
                    LunaRect_R.gameObject.SetActive(true);
                    LunaRect_R.sizeDelta = new Vector2(w, height);
                    LunaRect_R.localPosition = pos;
                    pos.x += w;
                    break;
                case 2:
                    AngelRect_Only.gameObject.SetActive(false);
                    WonderRect_Only.gameObject.SetActive(false);
                    LunaRect_Only.gameObject.SetActive(false);
                    AngelRect_L.gameObject.SetActive(false);
                    WonderRect_L.gameObject.SetActive(false);
                    WonderRect_C.gameObject.SetActive(false);
                    WonderRect_R.gameObject.SetActive(false);
                    LunaRect_R.gameObject.SetActive(false);

                    RectTransform L_target = null;
                    RectTransform R_target = null;
                    int l_rate = 0;
                    int r_rate = 0;
                    if (data.AngelRateValue > 0)
                    {
                        AngelTarget = AngelRect_L;
                        AngelRect_L.gameObject.SetActive(true);
                        L_target = AngelRect_L;
                        l_rate = data.AngelRateValue;
                        if (data.WonderRateValue > 0)
                        {
                            WonderTarget = WonderRect_R;
                            WonderRect_R.gameObject.SetActive(true);
                            R_target = WonderRect_R;
                            r_rate = data.WonderRateValue;
                        }
                    }
                    if (data.LunaRateValue > 0)
                    {
                        LunaTarget = LunaRect_R;
                        LunaRect_R.gameObject.SetActive(true);
                        R_target = LunaRect_R;
                        r_rate = data.LunaRateValue;
                        if (data.WonderRateValue > 0)
                        {
                            WonderTarget = WonderRect_L;
                            WonderRect_L.gameObject.SetActive(true);
                            L_target = WonderRect_L;
                            l_rate = data.WonderRateValue;
                        }
                    }
                    w = (float)l_rate / (l_rate + r_rate) * width;
                    L_target.sizeDelta = new Vector2(w, height);
                    L_target.localPosition = pos;
                    pos.x += w;

                    w = (float)r_rate / (l_rate + r_rate) * width;
                    R_target.sizeDelta = new Vector2(w, height);
                    R_target.localPosition = pos;
                    pos.x += w;
                    break;
                case 1:
                    AngelRect_Only.gameObject.SetActive(false);
                    WonderRect_Only.gameObject.SetActive(false);
                    LunaRect_Only.gameObject.SetActive(false);
                    AngelRect_L.gameObject.SetActive(false);
                    WonderRect_L.gameObject.SetActive(false);
                    WonderRect_C.gameObject.SetActive(false);
                    WonderRect_R.gameObject.SetActive(false);
                    LunaRect_R.gameObject.SetActive(false);

                    if (data.AngelRateValue > 0)
                    {
                        AngelTarget = AngelRect_Only;
                        AngelRect_Only.gameObject.SetActive(true);
                    }
                    if (data.WonderRateValue > 0)
                    {
                        WonderTarget = WonderRect_Only;
                        WonderRect_Only.gameObject.SetActive(true);
                    }
                    if (data.LunaRateValue > 0)
                    {
                        LunaTarget = LunaRect_Only;
                        LunaRect_Only.gameObject.SetActive(true);
                    }
                    break;
                default:
                    AngelRect_Only.gameObject.SetActive(false);
                    WonderRect_Only.gameObject.SetActive(false);
                    LunaRect_Only.gameObject.SetActive(false);
                    AngelRect_L.gameObject.SetActive(false);
                    WonderRect_L.gameObject.SetActive(false);
                    WonderRect_C.gameObject.SetActive(false);
                    WonderRect_R.gameObject.SetActive(false);
                    LunaRect_R.gameObject.SetActive(false);
                    break;
            }

            AngelRate.gameObject.SetActive(false);
            WonderRate.gameObject.SetActive(false);
            LunaRate.gameObject.SetActive(false);
            if (AngelTarget != null && data.AngelRateValue > 0)
            {
                AngelRate.gameObject.SetActive(true);
                AngelRate.text = data.AngelRateValue.ToString() + "%";
                AngelRate.transform.localPosition = new Vector2(AngelTarget.localPosition.x + AngelTarget.sizeDelta.x * 0.5f, 0);
            }
            if (WonderTarget != null && data.WonderRateValue > 0)
            {
                WonderRate.gameObject.SetActive(true);
                WonderRate.text = data.WonderRateValue.ToString() + "%";
                WonderRate.transform.localPosition = new Vector2(WonderTarget.localPosition.x + WonderTarget.sizeDelta.x * 0.5f, 0);
            }
            if (LunaTarget != null && data.LunaRateValue > 0)
            {
                LunaRate.gameObject.SetActive(true);
                LunaRate.text = data.LunaRateValue.ToString() + "%";
                LunaRate.transform.localPosition = new Vector2(LunaTarget.localPosition.x + LunaTarget.sizeDelta.x * 0.5f, 0);
            }

            AngelInfoPanel[0].GetComponent<Image>().color = angelCount > 0 ? Color.white : Color.gray;
            WonderInfoPanel[0].GetComponent<Image>().color = wonderCount > 0 ? Color.white : Color.gray;
            LunaInfoPanel[0].GetComponent<Image>().color = lunaCount > 0 ? Color.white : Color.gray;

            AngelInfoPanel[1].SetActive(NetworkManager.ServerTag != 1);
            AngelInfoPanel[2].SetActive(NetworkManager.ServerTag == 1);
            AngelInfoPanel[3].SetActive(angelCount <= 0);
            AngelInfoPanel[4].SetActive(NetworkManager.ServerTag == 1);
            AngelInfoPanel[5].SetActive(true);
            AngelInfoPanel[6].SetActive(angelCount > 0 || ChampionManager.Instance.CurChampionInfo.CurState >= ChampionInfo.ROUND_STATE.RESULT);
            AngelInfoPanel[7].SetActive(angelCount <= 0 && ChampionManager.Instance.CurChampionInfo.CurState < ChampionInfo.ROUND_STATE.RESULT);

            WonderInfoPanel[1].SetActive(NetworkManager.ServerTag != 2);
            WonderInfoPanel[2].SetActive(NetworkManager.ServerTag == 2);
            WonderInfoPanel[3].SetActive(wonderCount <= 0);
            WonderInfoPanel[4].SetActive(NetworkManager.ServerTag == 2);
            WonderInfoPanel[5].SetActive(true);
            WonderInfoPanel[6].SetActive(wonderCount > 0 || ChampionManager.Instance.CurChampionInfo.CurState >= ChampionInfo.ROUND_STATE.RESULT);
            WonderInfoPanel[7].SetActive(wonderCount <= 0 && ChampionManager.Instance.CurChampionInfo.CurState < ChampionInfo.ROUND_STATE.RESULT);

            LunaInfoPanel[1].SetActive(NetworkManager.ServerTag != 3);
            LunaInfoPanel[2].SetActive(NetworkManager.ServerTag == 3);
            LunaInfoPanel[3].SetActive(lunaCount <= 0);
            LunaInfoPanel[4].SetActive(NetworkManager.ServerTag == 3);
            LunaInfoPanel[5].SetActive(true);
            LunaInfoPanel[6].SetActive(lunaCount > 0 || ChampionManager.Instance.CurChampionInfo.CurState >= ChampionInfo.ROUND_STATE.RESULT);
            LunaInfoPanel[7].SetActive(lunaCount <= 0 && ChampionManager.Instance.CurChampionInfo.CurState < ChampionInfo.ROUND_STATE.RESULT);


            var rewarded = new List<Asset>();
            foreach (var r in data.RewardRound)
            {
                rewarded.AddRange(GetSurpportReward(r, currentInfo));
            }

            for (int i = 0; i < CloneMyReward.Length; i++)
            {
                if (rewarded.Count > i)
                {
                    CloneMyReward[i].gameObject.SetActive(true);
                    CloneMyReward[i].SetFrameItem(rewarded[i]);
                }
                else
                {
                    CloneMyReward[i].gameObject.SetActive(false);
                }
            }

            var rewards = GetSurpportReward(ChampionManager.Instance.CurChampionInfo.CurState, currentInfo);
            for (int i = 0; i < CloneCurReward.Length; i++)
            {
                if (rewards.Count > i)
                {
                    CloneCurReward[i].gameObject.SetActive(true);
                    CloneCurReward[i].SetFrameItem(rewards[i]);
                }
                else
                {
                    CloneCurReward[i].gameObject.SetActive(false);
                }
            }

            switch (NetworkManager.ServerTag)
            {
                case 1:
                    SupportBtn.interactable = angelCount > 0;
                    break;
                case 2:
                    SupportBtn.interactable = wonderCount > 0;
                    break;
                case 3:
                    SupportBtn.interactable = lunaCount > 0;
                    break;
            }

            CancelInvoke("TimerUpdate");

            if (SupportBtn.interactable)
            {
                BtnTimer.gameObject.SetActive(true);
                Invoke("TimerUpdate", 1.0f);

                remain = TimeManager.GetTimeCompare(ChampionManager.Instance.CurChampionInfo.GetContentsTime(ChampionInfo.CONTENTS_TIME.HIDDEN_TEAM_SET));
            }
            else
            {
                BtnTimer.gameObject.SetActive(false);
            }
        }

        void TimerUpdate()
        {
            CancelInvoke("TimerUpdate");

            remain--;
            if (remain > 0)
            {
                BtnTimer.text = SBFunc.TimeString((int)remain);
            }
            else
            {
                SupportBtn.interactable = false;
                BtnTimer.gameObject.SetActive(false);
            }

            Invoke("TimerUpdate", 1.0f);
        }

        public List<Asset> GetSurpportReward(ChampionInfo.ROUND_STATE round, ChampionSurpportInfo.eSurpportType type)
        {
            string defaultValue = "";
            switch (round)
            {
                case ChampionInfo.ROUND_STATE.ROUND_OF_16:
                    switch (type)
                    {
                        case ChampionSurpportInfo.eSurpportType.PHYS_DMG_RESIS:
                            defaultValue = "[[3,180000002,50],[3,30000005,10]]";
                            break;
                        case ChampionSurpportInfo.eSurpportType.ALL_ELEMENT_DMG_RESIS:
                            defaultValue = "[[3,180000002,50],[3,30000005,10]]";
                            break;
                        case ChampionSurpportInfo.eSurpportType.CRI_DMG_RESIS:
                            defaultValue = "[[3,180000002,50],[3,30000005,10]]";
                            break;
                    }
                    break;
                case ChampionInfo.ROUND_STATE.QUARTER_FINALS:
                    switch (type)
                    {
                        case ChampionSurpportInfo.eSurpportType.PHYS_DMG_RESIS:
                            defaultValue = "[[3,180000002,60],[3,30000005,10]]";
                            break;
                        case ChampionSurpportInfo.eSurpportType.ALL_ELEMENT_DMG_RESIS:
                            defaultValue = "[[3,180000002,60],[3,30000005,10]]";
                            break;
                        case ChampionSurpportInfo.eSurpportType.CRI_DMG_RESIS:
                            defaultValue = "[[3,180000002,60],[3,30000005,10]]";
                            break;
                    }
                    break;
                case ChampionInfo.ROUND_STATE.SEMI_FINALS:
                    switch (type)
                    {
                        case ChampionSurpportInfo.eSurpportType.PHYS_DMG_RESIS:
                            defaultValue = "[[3,180000002,80],[3,30000010,1]]";
                            break;
                        case ChampionSurpportInfo.eSurpportType.ALL_ELEMENT_DMG_RESIS:
                            defaultValue = "[[3,180000002,80],[3,30000010,1]]";
                            break;
                        case ChampionSurpportInfo.eSurpportType.CRI_DMG_RESIS:
                            defaultValue = "[[3,180000002,80],[3,30000010,1]]";
                            break;
                    }
                    break;
                case ChampionInfo.ROUND_STATE.FINAL:
                    switch (type)
                    {
                        case ChampionSurpportInfo.eSurpportType.PHYS_DMG_RESIS:
                            defaultValue = "[[3,180000002,100],[3,30000010,1]]";
                            break;
                        case ChampionSurpportInfo.eSurpportType.ALL_ELEMENT_DMG_RESIS:
                            defaultValue = "[[3,180000002,100],[3,30000010,1]]";
                            break;
                        case ChampionSurpportInfo.eSurpportType.CRI_DMG_RESIS:
                            defaultValue = "[[3,180000002,100],[3,30000010,1]]";
                            break;
                    }
                    break;
            }

            var ret = new List<Asset>();
            var value = GameConfigTable.GetConfigValue("CHAMP_SURPPORT_" + round + "_" + type, defaultValue);

            if (!string.IsNullOrEmpty(value))
            {
                JArray array = JArray.Parse(value);
                foreach (var a in array)
                {
                    if (a.Type == JTokenType.Array)
                    {
                        JArray asset = (JArray)a;
                        if (asset.Count == 3)
                        {
                            ret.Add(new Asset((eGoodType)asset[0].Value<int>(), asset[1].Value<int>(), asset[2].Value<int>()));
                        }
                    }
                }
            }
            return ret;
        }
        public void OnClickTab(int index)
        {
            switch (index)
            {
                case 0:
                    InitTab(ChampionSurpportInfo.eSurpportType.PHYS_DMG_RESIS);
                    break;
                case 1:
                    InitTab(ChampionSurpportInfo.eSurpportType.ALL_ELEMENT_DMG_RESIS);
                    break;
                case 2:
                    InitTab(ChampionSurpportInfo.eSurpportType.CRI_DMG_RESIS);
                    break;
            }
        }
        public override void InitUI()
        {

        }

        public void OnSurpport()
        {
            ChampionSurpportServerPopup.Open(currentInfo);
        }

        private void OnEnable()
        {
            EventManager.AddListener(this);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener(this);
            PopupManager.Instance.Top.SetMagniteUI(false);
        }

        public void OnEvent(ChampionSurpportUpdate eventType)
        {
            InitTab(currentInfo);
        }
    }
}

