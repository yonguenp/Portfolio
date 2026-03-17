using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RestrictedAreaLayer : MonoBehaviour, EventListener<DragonChangedEvent>
{
    const int MAX_SELECT_COUNT = 5;
    [SerializeField] Text Title;
    [SerializeField] TravelLayer parent;

    [SerializeField] GameObject NormalLayer;
    [SerializeField] GameObject LogLayer;

    [SerializeField] Sprite[] WorldBGSprite;
    [SerializeField] Image WorldBGImage;
    [SerializeField] UserPortraitFrame OwnerPortrait;
    [SerializeField] GuildBaseInfoObject OwnerGuild;
    [SerializeField] Text OwnerNick;
    [SerializeField] Image[] OwnerDragons;
    [SerializeField] GameObject ShieldObject;
    [SerializeField] Text ShieldTimer;

    [SerializeField] GameObject conquestLayer;
    [SerializeField] GameObject neutralityLayer;

    [SerializeField] Text WorldName;
    [SerializeField] Text NeedBattlePoint;

    [SerializeField] ItemFrame ItemPrefab;


    [SerializeField] RestrictedAreaSlot[] Slots;
    [SerializeField] Text TravelCount;

    [SerializeField] GameObject[] ingNode;
    [SerializeField] Text RemainTimer;

    [SerializeField] Button BattleBtn;
    [SerializeField] Button RunBtn;
    [SerializeField] Button RunFreeBtn;
    [SerializeField] Button DoneBtn;

    [SerializeField] Text NeedEnergy;
    [SerializeField] Text NeedMagnet;
    [SerializeField] Text[] NeedTime;
    [SerializeField] Text BattleWaitTime;

    [SerializeField] GameObject DeckLayer;
    [SerializeField] GameObject IncomeLayer;

    [SerializeField] Text MyIncome;

    [SerializeField] GameObject dragonPortraitFrame = null;
    [SerializeField] GameObject dragonListSlotNode;
    [SerializeField] Text MyBP;

    [SerializeField] RestrictedLogSlot LogClone = null;
    [SerializeField] Text TaxTotal;

    RestrictedAreaSlot SelectedSlot = null;
    int curSelectedIndex = -1;
    int battleWaitTime = -1;
    JObject Data = null;

    List<Dictionary<string, GameObject>> characterSlots = null;
    bool TeamSetting = false;

    List<int> usingDragons = new List<int>();
    List<int> currentDragons = new List<int>();
    StageDifficult curDifficult = StageDifficult.NONE;
    int travelWorldCount = 0;
    int curBattlePoint = 0;
    public void Show(StageDifficult diff, int world = 1)
    {
        if(diff != StageDifficult.NONE)
            curDifficult = diff;

        if (curDifficult == StageDifficult.NONE)
            return;

        switch (curDifficult)
        {
            case StageDifficult.HARD:
                Title.text = StringData.GetStringByStrKey("제한구역_어려움2");
                break;
            case StageDifficult.HELL:
                Title.text = StringData.GetStringByStrKey("제한구역_지옥2");
                break;
            default:
                return;
        }

        gameObject.SetActive(true);

        Clear();

        WWWForm param = new WWWForm();
        param.AddField("diff", (int)curDifficult);
        NetworkManager.Send("travelinst/info", param, (data) =>
        {
            if(data.ContainsKey("travel_inst") && data["travel_inst"].Type == JTokenType.Object)
                Data = (JObject)data["travel_inst"];

            OnAreaSelect(world);
        });

        PopupManager.Instance.Top.SetStaminaUI(true);
        PopupManager.Instance.Top.SetMagnetUI(true);

        EventManager.AddListener<DragonChangedEvent>(this);
    }

    public void Hide()
    {
        PopupManager.Instance.Top.SetStaminaUI(false);
        PopupManager.Instance.Top.SetMagnetUI(false);

        EventManager.RemoveListener<DragonChangedEvent>(this);
        gameObject.SetActive(false);        
    }

    void Clear()
    {
        curSelectedIndex = -1;
        battleWaitTime = -1;
        curBattlePoint = 0;
        usingDragons.Clear();

        foreach (Transform child in ItemPrefab.transform.parent)
        {
            if (child == ItemPrefab.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }

        foreach(var slot in Slots)
        {
            slot.Clear();
        }

        ItemPrefab.gameObject.SetActive(false);
        BattleBtn.gameObject.SetActive(false);
        RunBtn.gameObject.SetActive(false);
        RunFreeBtn.gameObject.SetActive(false);
        DoneBtn.gameObject.SetActive(false);

        foreach (var lockObj in ingNode)
        {
            lockObj.SetActive(false);
        }

        NormalLayer.SetActive(true);
        LogLayer.SetActive(false);

        foreach (var dimg in OwnerDragons)
        {
            dimg.sprite = null;
        }

        if (characterSlots == null)
        {
            if (characterSlots == null)
            {
                characterSlots = new List<Dictionary<string, GameObject>>();
            }

            if (dragonListSlotNode != null)
            {                
                for (var i = 1; i <= 5; i++)
                {
                    var curSlot = SBFunc.GetChildrensByName(dragonListSlotNode.transform, new string[] { i.ToString() }).gameObject;
                    if (curSlot != null)
                    {
                        var select = SBFunc.GetChildrensByName(curSlot.transform, new string[] { "Select" }).gameObject;
                        var dimd = SBFunc.GetChildrensByName(curSlot.transform, new string[] { "dimd" }).gameObject;
                        var plus = SBFunc.GetChildrensByName(curSlot.transform, new string[] { "plus" }).gameObject;

                        if (select != null)
                        {
                            select.SetActive(false);
                            var container = SBFunc.GetChildrensByName(select.transform, new string[] { "character" }).gameObject;

                            if (container != null && dimd != null)
                            {
                                Dictionary<string, GameObject> tempDic = new Dictionary<string, GameObject>
                                {
                                    { "slot", curSlot },
                                    { "target", select },
                                    { "container", container },
                                    { "dimd", dimd },
                                    { "plus", plus }
                                };

                                characterSlots.Add(tempDic);
                            }
                        }
                    }
                }
            }
        }
    }

    void Refresh()
    {
        OnAreaSelect(curSelectedIndex);
    }
    public void OnAreaSelect(int index)
    {
        Clear();

        curSelectedIndex = index;

        travelWorldCount = 0;

        for(int diff = 2; diff <= 3; diff++)
        {
            JObject diff_data = null;
            if (Data != null && Data.ContainsKey(diff.ToString()))
            {
                diff_data = (JObject)Data[diff.ToString()];
            }
            
            for (int world = 1; world <= 8; world++)
            {
                JObject data = null;
                if (diff_data != null && diff_data.ContainsKey(world.ToString()))
                {
                    data = (JObject)diff_data[world.ToString()];
                }

                if (diff == (int)curDifficult)
                {
                    var slot = Slots[world - 1];
                    slot.OnSelect(curSelectedIndex, data, curDifficult);

                    if (slot.Index == curSelectedIndex)
                        SelectedSlot = slot;

                    if (slot.IntVal("travel_tag") > 0)
                    {
                        if (slot.Value("travel_deck").Type == JTokenType.Array)
                        {
                            JArray deck = (JArray)slot.Value("travel_deck");
                            foreach (int no in deck)
                            {
                                if (no > 0)
                                    usingDragons.Add(no);
                            }
                        }

                    }
                    if (slot.IntVal("ctrl_user_no") == User.Instance.UserAccountData.UserNumber)
                    {
                        var dom_deck = slot.Value("dom_deck");
                        if (dom_deck != null && dom_deck.Type == JTokenType.Array)
                        {
                            var deckarray = (JArray)dom_deck;
                            foreach (int d in deckarray)
                            {
                                if (d > 0)
                                {
                                    usingDragons.Add(d);
                                }
                            }
                        }
                    }

                    if (slot.IntVal("travel_tag") > 0)
                        travelWorldCount++;
                }
                else
                {
                    if (data != null)
                    {
                        if (data.ContainsKey("travel_tag"))
                        {
                            if (data["travel_tag"].Value<int>() > 0)
                            {
                                if (data.ContainsKey("travel_deck") && data["travel_deck"].Type == JTokenType.Array)
                                {
                                    JArray deck = (JArray)data["travel_deck"];
                                    foreach (int no in deck)
                                    {
                                        if (no > 0)
                                            usingDragons.Add(no);
                                    }
                                }
                            }
                        }
                        if (data.ContainsKey("ctrl_user_no") && data["ctrl_user_no"].Value<int>() == User.Instance.UserAccountData.UserNumber && data.ContainsKey("dom_deck"))
                        {
                            var dom_deck = data["dom_deck"];
                            if (dom_deck != null && dom_deck.Type == JTokenType.Array)
                            {
                                var deckarray = (JArray)dom_deck;
                                foreach (int d in deckarray)
                                {
                                    if (d > 0)
                                    {
                                        usingDragons.Add(d);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        switch (curDifficult)
        {
            case StageDifficult.HARD:
                TravelCount.text = travelWorldCount.ToString() + "/" + GameConfigTable.GetConfigIntValue("MAX_HARD_RESTRICT_AREA_TRAVEL", 3).ToString();
                break;
            case StageDifficult.HELL:
            default:
                TravelCount.text = travelWorldCount.ToString() + "/" + GameConfigTable.GetConfigIntValue("MAX_HELL_RESTRICT_AREA_TRAVEL", 5).ToString();
                break;
        }


        RefreshRightUI();
    }

    void RefreshRightUI()
    {
        battleWaitTime = -1;

        RestrictedAreaData curData = RestrictedAreaData.GetByWorldDiff(curSelectedIndex, curDifficult);
        if(curData == null)
        {
            Debug.LogError("데이터 못찾음");
            return;
        }

        WorldBGImage.sprite = WorldBGSprite[curSelectedIndex];
        WorldName.text = StringData.GetStringByStrKey(curData._NAME);
        NeedEnergy.text = curData.COST_STAMINA.ToString();
        NeedMagnet.text = curData.COST_FEE.ToString();
        NeedBattlePoint.text = StringData.GetStringFormatByStrKey("필요전투력_" + (curDifficult == StageDifficult.HELL ? "헬" : "어려움"), SBFunc.CommaFromNumber(curData.NEED_BP).ToString());

        foreach (var time in NeedTime)
        {
            time.text = SBFunc.TimeString(curData.TIME);
        }

        if (Data == null)
            return;

        foreach(Transform child in ItemPrefab.transform.parent)
        {
            if (child == ItemPrefab.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }

        ItemPrefab.gameObject.SetActive(true);

        var rewards = ItemGroupData.Get(curData.REWARD_ITEM);
        Dictionary<eGoodType, Dictionary<int, List<int>>> attached = new Dictionary<eGoodType, Dictionary<int, List<int>>>();

        var specials = new List<Asset>();
        specials.Add(new Asset(eGoodType.MAGNET, 0, curData.REWARD_MAGNET_MIN));
        specials.Add(new Asset(eGoodType.MAGNET, 0, curData.REWARD_MAGNET_MAX));
        specials.Add(new Asset(eGoodType.ITEM, 110000004, curData.REWARD_CHIPSET_MIN));
        specials.Add(new Asset(eGoodType.ITEM, 110000004, curData.REWARD_CHIPSET_MAX));
        specials.Add(new Asset(eGoodType.ITEM, 50000007, curData.REWARD_GOLDBLOCK_MIN));
        specials.Add(new Asset(eGoodType.ITEM, 50000007, curData.REWARD_GOLDBLOCK_MAX));
        specials.Add(new Asset(eGoodType.ITEM, 50000006, curData.REWARD_LEADBLOCK_MIN));
        specials.Add(new Asset(eGoodType.ITEM, 50000006, curData.REWARD_LEADBLOCK_MAX));
        specials.Add(new Asset(eGoodType.ITEM, 140000004, curData.REWARD_ELEMENTAL_MIN));
        specials.Add(new Asset(eGoodType.ITEM, 140000004, curData.REWARD_ELEMENTAL_MAX));
        specials.Add(new Asset(eGoodType.ITEM, 150000001, 0));
        specials.Add(new Asset(eGoodType.ITEM, 150000002, 0));
        specials.Add(new Asset(eGoodType.ITEM, 150000003, 0));

        foreach (var reward in specials)
        {
            if (!attached.ContainsKey(reward.GoodType))
            {
                var list = new List<int>();
                list.Add(reward.Amount);

                var dic = new Dictionary<int, List<int>>();
                dic.Add(reward.ItemNo, list);

                attached.Add(reward.GoodType, dic);
            }
            else
            {
                if (!attached[reward.GoodType].ContainsKey(reward.ItemNo))
                {
                    var list = new List<int>();
                    list.Add(reward.Amount);

                    attached[reward.GoodType].Add(reward.ItemNo, list);
                }
                else
                {
                    attached[reward.GoodType][reward.ItemNo].Add(reward.Amount);
                }
            }
        }

        foreach (var group in rewards)
        {
            var reward_group = new List<Asset>();
            if(group.Reward.GoodType != eGoodType.DICE_GROUP)
            {
                reward_group.Add(group.Reward);
            }
            else
            {
                foreach (var r1 in group.Child)
                {
                    if (r1.Reward.GoodType != eGoodType.DICE_GROUP)
                    {
                        if(group.ITEM_RATE < 1000000)
                        {
                            reward_group.Add(new Asset(r1.Reward.GoodType, r1.Reward.ItemNo, 0));
                        }
                        reward_group.Add(r1.Reward);
                    }
                    else
                    {
                        foreach (var r2 in r1.Child)
                        {
                            if (r1.Reward.GoodType != eGoodType.DICE_GROUP)
                            {
                                if (group.ITEM_RATE < 1000000)
                                {
                                    reward_group.Add(new Asset(r2.Reward.GoodType, r2.Reward.ItemNo, 0));
                                }
                                else if (r1.ITEM_RATE < 1000000)
                                {
                                    reward_group.Add(new Asset(r2.Reward.GoodType, r2.Reward.ItemNo, 0));
                                }
                                reward_group.Add(r2.Reward);
                            }
                            else
                            {
                                Debug.LogError("주사위를 몇개나 굴리는거야");
                            }
                        }
                    }
                }
            }

            foreach (var reward in reward_group)
            {
                if (!attached.ContainsKey(reward.GoodType))
                {
                    var list = new List<int>();
                    list.Add(reward.Amount);

                    var dic = new Dictionary<int, List<int>>();
                    dic.Add(reward.ItemNo, list);

                    attached.Add(reward.GoodType, dic);
                }
                else
                {
                    if (!attached[reward.GoodType].ContainsKey(reward.ItemNo))
                    {
                        var list = new List<int>();
                        list.Add(reward.Amount);

                        attached[reward.GoodType].Add(reward.ItemNo, list);
                    }
                    else
                    {
                        attached[reward.GoodType][reward.ItemNo].Add(reward.Amount);
                    }
                }
            }            
        }

        foreach(var key in attached.Keys)
        {
            foreach(var no in attached[key].Keys)
            {
                int min = attached[key][no].Min();
                int max = attached[key][no].Max();
                
                if (min == 0 && max == 0)
                    continue;

                var r = new Asset(key, no, max);                
                var newItem = Instantiate(ItemPrefab, ItemPrefab.transform.parent);
                if (newItem != null)
                {
                    var frame = newItem.GetComponent<ItemFrame>();
                    if (frame != null)
                    {
                        frame.SetFrameItem(r);
                        if (min != max)                        
                        {
                            frame.SetMinMaxText(min, max, true);
                        }
                    }
                }
            }
        }
        
        parent.SetRestrictedRewardPopup(attached);

        ItemPrefab.gameObject.SetActive(false);

        if (SelectedSlot.IntVal("ctrl_guild_no") > 0)
        {
            OwnerNick.text = SelectedSlot.StrVal("ctrl_user_nick");

            OwnerPortrait.SetUserPortraitFrame(SelectedSlot.StrVal("ctrl_user_icon"), SelectedSlot.IntVal("ctrl_user_level"), false, new PortraitEtcInfoData(SelectedSlot.Value("ctrl_user_portrait")));
            OwnerGuild.Init(new GuildBaseData(SelectedSlot.IntVal("ctrl_guild_no"), SelectedSlot.StrVal("ctrl_guild_name"), SelectedSlot.IntVal("ctrl_emblem_no"), SelectedSlot.IntVal("ctrl_mark_no")));
            
            if (SelectedSlot.RemainShield > 0)
            {
                ShieldObject.SetActive(true);
                ShieldTimer.text = SBFunc.TimeStringMinute(SelectedSlot.RemainShield);
            }
            else
            {
                ShieldObject.SetActive(false);
            }
        }
        else
        {
            OwnerNick.text = "";
            OwnerGuild.Init(null);
            OwnerPortrait.InitPortrait();
            ShieldObject.SetActive(false);
        }

        var dom_deck = SelectedSlot.Value("dom_deck");
        if (dom_deck != null && dom_deck.Type == JTokenType.Array)
        {
            int index = 0;
            var deckarray = (JArray)dom_deck;
            foreach (var d in deckarray)
            {
                CharBaseData cd = CharBaseData.Get(d.Value<int>());
                if (cd != null)
                {
                    OwnerDragons[index++].sprite = cd.GetThumbnail();
                }
            }
        }

        conquestLayer.SetActive(curData.CONQUEST != 0);
        neutralityLayer.SetActive(curData.CONQUEST == 0);

        currentDragons.Clear();
        DelAllSlot();
        if (SelectedSlot.IntVal("ctrl_user_no") == User.Instance.UserAccountData.UserNumber)
        {
            DeckLayer.SetActive(false);
            IncomeLayer.SetActive(true);

            MyIncome.text = SBFunc.CommaFromNumber(SelectedSlot.IntVal("tax_income"));
            int bp = SelectedSlot.IntVal("dom_deck_bp");
            if (bp <= 0)
            {
                if (dom_deck != null && dom_deck.Type == JTokenType.Array)
                {
                    int index = 0;
                    var deckarray = (JArray)dom_deck;
                    foreach (int d in deckarray)
                    {
                        if (d > 0)
                        {
                            CharBaseData cd = CharBaseData.Get(d);
                            if (cd != null)
                            {
                                OwnerDragons[index++].sprite = cd.GetThumbnail();
                                currentDragons.Add(d);

                                var dragon = User.Instance.DragonData.GetDragon(d);
                                if (dragon != null)
                                    bp += dragon.GetTotalINF();
                            }
                        }
                    }
                }
            }

            if (curDifficult == StageDifficult.HELL)
            {
                RunFreeBtn.interactable = (curData.NEED_BP <= bp);
            }
            else if (curDifficult == StageDifficult.HARD)
            {
                RunFreeBtn.interactable = (0 < bp && curData.NEED_BP >= bp);
            }

            curBattlePoint = bp;
        }
        else
        {
            DeckLayer.SetActive(true);
            IncomeLayer.SetActive(false);

            int bp = 0;
            if (SelectedSlot.IntVal("travel_tag") > 0)
            {
                int slotIdx = 0;
                if (SelectedSlot.Value("travel_deck").Type == JTokenType.Array)
                {
                    JArray deck = (JArray)SelectedSlot.Value("travel_deck");
                    foreach (int no in deck)
                    {
                        if (no > 0)
                        {
                            var dragon = User.Instance.DragonData.GetDragon(no);
                            SetSlot(slotIdx++, dragon);
                            currentDragons.Add(no);
                            bp += dragon.GetTotalINF();
                        }
                    }
                }
            }
            else
            {
                int slotIdx = 0;
                for (int i = 0; i < 6; i++)
                {
                    int dno = CacheUserData.GetInt("restricted_deck_" + (int)curDifficult + "_" + curSelectedIndex + "_" + i, 0);

                    if (usingDragons.Contains(dno))
                    {
                        CacheUserData.SetInt("restricted_deck_" + (int)curDifficult + "_" + curSelectedIndex + "_" + i, 0);
                        dno = 0;
                    }

                    if (dno > 0)
                    {
                        var dragon = User.Instance.DragonData.GetDragon(dno);
                        SetSlot(slotIdx++, dragon);                        
                        bp += dragon.GetTotalINF();
                    }

                    currentDragons.Add(dno);
                }
            }

            MyBP.text = SBFunc.CommaFromNumber(bp);
            if (curDifficult == StageDifficult.HELL)
            {
                MyBP.color = curData.NEED_BP <= bp ? Color.green : Color.red;
                RunBtn.interactable = (curData.NEED_BP <= bp);
                RunFreeBtn.interactable = (curData.NEED_BP <= bp);
            }
            else if (curDifficult == StageDifficult.HARD)
            {
                MyBP.color = 0 < bp && curData.NEED_BP >= bp ? Color.green : Color.red;
                RunBtn.interactable = (0 < bp && curData.NEED_BP >= bp);
                RunFreeBtn.interactable = (0 < bp && curData.NEED_BP >= bp);
            }

            curBattlePoint = bp;
        }

        if (SelectedSlot.IntVal("travel_tag") > 0)
        {
            if (SelectedSlot.RemainTravel <= 0)
            {
                RemainTimer.text = StringData.GetStringFormatByStrKey("여행완료");

                //여행 완료
                foreach (var lockObj in ingNode)
                {
                    lockObj.SetActive(false);
                }

                BattleBtn.gameObject.SetActive(false);
                RunBtn.gameObject.SetActive(false);
                RunFreeBtn.gameObject.SetActive(false);
                DoneBtn.gameObject.SetActive(true);
            }
            else
            {
                RemainTimer.text = SBFunc.TimeStringMinute(SelectedSlot.RemainTravel);

                //여행 중
                foreach (var lockObj in ingNode)
                {
                    lockObj.SetActive(true);
                }

                BattleBtn.gameObject.SetActive(false);
                RunBtn.gameObject.SetActive(false);
                RunFreeBtn.gameObject.SetActive(false);
                DoneBtn.gameObject.SetActive(false);
            }
        }
        else
        {
            //여행 대기
            foreach (var lockObj in ingNode)
            {
                lockObj.SetActive(false);
            }

            if (curData.CONQUEST == 0 || (SelectedSlot.IntVal("ctrl_guild_no") == GuildManager.Instance.GuildID && !GuildManager.Instance.IsNoneGuild))
            {
                //우리 길드 소유이거나 침략불가인경우
                BattleBtn.gameObject.SetActive(false);
                RunBtn.gameObject.SetActive(false);
                RunFreeBtn.gameObject.SetActive(true);
                DoneBtn.gameObject.SetActive(false);
            }
            else
            {
                BattleBtn.gameObject.SetActive(true);
                RunBtn.gameObject.SetActive(true);
                RunFreeBtn.gameObject.SetActive(false);
                DoneBtn.gameObject.SetActive(false);

                battleWaitTime = SelectedSlot.IntVal("dom_fail_at_ts") + curData.LOSE_TIME - TimeManager.GetTime();
                if (battleWaitTime < 0 && !GuildManager.Instance.IsNoneGuild && SelectedSlot.RemainShield <= 0)
                {
                    //배틀 가능
                    if (curDifficult == StageDifficult.HELL)
                    {
                        BattleBtn.interactable = (curData.NEED_BP <= curBattlePoint);
                    }
                    else if (curDifficult == StageDifficult.HARD)
                    {
                        BattleBtn.interactable = (0 < curBattlePoint && curData.NEED_BP >= curBattlePoint);
                    }

                    BattleWaitTime.transform.parent.gameObject.SetActive(false);
                }
                else
                {
                    //배틀 불가
                    BattleBtn.interactable = SelectedSlot.RemainShield <= 0 && !GuildManager.Instance.IsNoneGuild;
                    if (battleWaitTime > 0)
                    {
                        BattleWaitTime.transform.parent.gameObject.SetActive(true);
                        BattleWaitTime.text = SBFunc.TimeStringMinute(battleWaitTime);
                    }
                }
            }
        }

        if (SelectedSlot.RemainShield > 0 || SelectedSlot.RemainTravel > 0 || battleWaitTime > 0)
            Invoke("TimeRefresh", 1.0f);

    }


    public void OnShowLog()
    {
        NormalLayer.SetActive(false);
        LogLayer.SetActive(true);

        RefreshLogUI();
    }

    public void OnCloseLog()
    {
        NormalLayer.SetActive(true);
        LogLayer.SetActive(false);
    }

    void ClearLog()
    {

    }
    void RefreshLogUI()
    {
        foreach(Transform child in LogClone.transform.parent)
        {
            if (child == LogClone.transform)
                continue;

            Destroy(child.gameObject);
        }
        LogClone.gameObject.SetActive(false);

        TaxTotal.text = "";
        WWWForm param = new WWWForm();
        param.AddField("world", curSelectedIndex);
        param.AddField("diff", (int)curDifficult);

        NetworkManager.Send("travelinst/log", param, (data) =>
        {
            bool hasData = false;
            if(data.ContainsKey("log") && data["log"].Type == JTokenType.Object)
            {
                JObject res = (JObject)data["log"];
                
                if(res.ContainsKey("list") && res["list"].Type == JTokenType.Array)
                {
                    int tax = 0;
                    RestrictedAreaData curData = RestrictedAreaData.GetByWorldDiff(curSelectedIndex, curDifficult);
                    if (curData != null)
                    {
                        tax = curData.FEE_TAX;
                    }

                    JArray datas = (JArray)res["list"];
                    LogClone.gameObject.SetActive(true);
                    foreach (var d in datas)
                    {
                        var newItem = Instantiate(LogClone.gameObject, LogClone.transform.parent);
                        if (newItem != null)
                        {
                            var frame = newItem.GetComponent<RestrictedLogSlot>();
                            if (frame != null)
                            {
                                frame.SetData((JObject)d, tax);
                                hasData = true;
                            }
                        }
                    }
                    LogClone.gameObject.SetActive(false);
                }
                if(res.ContainsKey("tax_income"))
                {
                    TaxTotal.text = SBFunc.CommaFromNumber(res["tax_income"].Value<int>());
                }
            }

            if(!hasData)
            {
                ToastManager.On(StringData.GetStringByStrKey("제한구역수익없음"));
                OnCloseLog();
            }
        });
    }

    void TimeRefresh()
    {
        CancelInvoke("TimeRefresh");
        if (SelectedSlot.RemainShield > 0)
        {
            ShieldObject.SetActive(true);
            ShieldTimer.text = SBFunc.TimeStringMinute(SelectedSlot.RemainShield);
        }
        else
        {
            ShieldObject.SetActive(false);
        }

        if (SelectedSlot.RemainTravel > 0)
        {
            RemainTimer.text = SBFunc.TimeStringMinute(SelectedSlot.RemainTravel);
        }
        else
        {
            RemainTimer.text = StringData.GetStringFormatByStrKey("여행완료");
        }

        if (battleWaitTime < 0)
        {
            //배틀 가능            
            BattleWaitTime.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            battleWaitTime--;
            //배틀 불가
            BattleWaitTime.text = SBFunc.TimeStringMinute(battleWaitTime);
        }

        if (SelectedSlot.RemainShield >= 0 || SelectedSlot.RemainTravel >= 0 || battleWaitTime > 0)
            Invoke("TimeRefresh", 1.0f);
    }

    public void RunFree()
    {
        if (currentDragons.Count <= 0 && SelectedSlot.IntVal("ctrl_user_no") != User.Instance.UserAccountData.UserNumber)
        {
            ToastManager.On(StringData.GetStringByStrKey("제한구역여행전투력제한"));
            return;
        }

        RestrictedAreaData curData = RestrictedAreaData.GetByWorldDiff(curSelectedIndex, curDifficult);
        if (curData == null)
        {
            return;
        }

        switch (curDifficult)
        {
            case StageDifficult.HARD:
                if (curBattlePoint > curData.NEED_BP)
                {
                    ToastManager.On(StringData.GetStringByStrKey("제한구역여행전투력제한"));
                    return;
                }
                if (travelWorldCount >= GameConfigTable.GetConfigIntValue("MAX_HARD_RESTRICT_AREA_TRAVEL", 3))
                {
                    ToastManager.On(StringData.GetStringByStrKey("제한구역여행동시제한안내"));
                    return;
                }
                break;
            case StageDifficult.HELL:
            default:
                if (curBattlePoint < curData.NEED_BP)
                {
                    ToastManager.On(StringData.GetStringByStrKey("제한구역여행전투력제한"));
                    return;
                }
                if (travelWorldCount >= GameConfigTable.GetConfigIntValue("MAX_HELL_RESTRICT_AREA_TRAVEL", 5))
                {
                    ToastManager.On(StringData.GetStringByStrKey("제한구역여행동시제한안내"));
                    return;
                }
                break;
        }

        if (User.Instance.ENERGY < curData.COST_STAMINA)
        {
            ToastManager.On(100000134);
            return;
        }

        SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("제한구역여행확인_무료"), StringData.GetStringByStrKey("확인"), StringData.GetStringByStrKey("취소"),
            () =>
            {
                if (SelectedSlot.IntVal("ctrl_user_no") == User.Instance.UserAccountData.UserNumber)
                {
                    currentDragons.Clear();
                    var dom_deck = SelectedSlot.Value("dom_deck");
                    if (dom_deck != null && dom_deck.Type == JTokenType.Array)
                    {
                        var deckarray = (JArray)dom_deck;
                        foreach (int d in deckarray)
                        {
                            if (d > 0)
                            {
                                currentDragons.Add(d);
                            }
                        }
                    }
                }

                WWWForm param = new WWWForm();
                param.AddField("world", curSelectedIndex);
                param.AddField("deck", JsonConvert.SerializeObject(currentDragons.ToArray()));
                param.AddField("type", 1);
                param.AddField("diff", (int)curDifficult);
                NetworkManager.Send("travelinst/start", param, (data) =>
                {
                    Data[((int)curDifficult).ToString()][curSelectedIndex.ToString()] = data;

                    Refresh();
                });
            },
            () => { });
    }

    public void Run()
    {
        if (currentDragons.Count <= 0)
        {
            ToastManager.On(StringData.GetStringByStrKey("제한구역여행전투력제한"));
            return;
        }

        RestrictedAreaData curData = RestrictedAreaData.GetByWorldDiff(curSelectedIndex, curDifficult);
        if (curData == null)
            return;
        switch (curDifficult)
        {
            case StageDifficult.HARD:
                if (curBattlePoint > curData.NEED_BP)
                {
                    ToastManager.On(StringData.GetStringByStrKey("제한구역여행전투력제한"));
                    return;
                }
                if (travelWorldCount >= GameConfigTable.GetConfigIntValue("MAX_HARD_RESTRICT_AREA_TRAVEL", 3))
                {
                    ToastManager.On(StringData.GetStringByStrKey("제한구역여행동시제한안내"));
                    return;
                }
                break;
            case StageDifficult.HELL:
            default:
                if (curBattlePoint < curData.NEED_BP)
                {
                    ToastManager.On(StringData.GetStringByStrKey("제한구역여행전투력제한"));
                    return;
                }
                if (travelWorldCount >= GameConfigTable.GetConfigIntValue("MAX_HELL_RESTRICT_AREA_TRAVEL", 5))
                {
                    ToastManager.On(StringData.GetStringByStrKey("제한구역여행동시제한안내"));
                    return;
                }
                break;
        }

        if (User.Instance.UserData.Magnet < curData.COST_FEE)
        {
            ToastManager.On(100007843);
            return;
        }

        SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("제한구역여행확인_유료"), StringData.GetStringByStrKey("확인"), StringData.GetStringByStrKey("취소"),
            () =>
            {
                WWWForm param = new WWWForm();
                param.AddField("world", curSelectedIndex);
                param.AddField("deck", JsonConvert.SerializeObject(currentDragons.ToArray()));
                param.AddField("type", 2);
                param.AddField("diff", (int)curDifficult);
                NetworkManager.Send("travelinst/start", param, (data) =>
                {
                    Data[((int)curDifficult).ToString()][curSelectedIndex.ToString()] = data;

                    Refresh();
                });
            },
            ()=> { });
    }

    public void Battle()
    {
        if(currentDragons.Count <= 0)
        {
            ToastManager.On(StringData.GetStringByStrKey("제한구역여행전투력제한"));
            return;
        }

        RestrictedAreaData curData = RestrictedAreaData.GetByWorldDiff(curSelectedIndex, curDifficult);
        if (curData == null)
            return;
        switch (curDifficult)
        {
            case StageDifficult.HARD:
                if (curBattlePoint > curData.NEED_BP)
                {
                    ToastManager.On(StringData.GetStringByStrKey("제한구역여행전투력제한"));
                    return;
                }
                if (travelWorldCount >= GameConfigTable.GetConfigIntValue("MAX_HARD_RESTRICT_AREA_TRAVEL", 3))
                {
                    ToastManager.On(StringData.GetStringByStrKey("제한구역여행동시제한안내"));
                    return;
                }
                break;
            case StageDifficult.HELL:
            default:
                if (curBattlePoint < curData.NEED_BP)
                {
                    ToastManager.On(StringData.GetStringByStrKey("제한구역여행전투력제한"));
                    return;
                }
                if (travelWorldCount >= GameConfigTable.GetConfigIntValue("MAX_HELL_RESTRICT_AREA_TRAVEL", 5))
                {
                    ToastManager.On(StringData.GetStringByStrKey("제한구역여행동시제한안내"));
                    return;
                }
                break;
        }

        if (battleWaitTime > 0)
        {
            AccelerationImmediatelyPopup.OpenPopup(SelectedSlot.IntVal("dom_fail_at_ts") + curData.LOSE_TIME, curData.LOSE_TIME, OnBattleWithItem);
            return;
        }

        OnBattleWithItem(null);
    }

    public void OnBattleWithItem(Asset asset)
    {
        RestrictedAreaTeamData restrictAreaTeamData = new RestrictedAreaTeamData();
        restrictAreaTeamData.InitData();
        foreach (var no in currentDragons)
        {
            DragonInfo tempDataSet = new DragonInfo(no, 50, 0);
            restrictAreaTeamData.DefDeck.Add(tempDataSet);
        }
        restrictAreaTeamData.SlotID = curSelectedIndex;
        restrictAreaTeamData.UID = SelectedSlot.IntVal("ctrl_user_no");
        //friendArenaData.Level = data.Level;
        restrictAreaTeamData.Nick = SelectedSlot.StrVal("ctrl_user_nick");
        //friendArenaData.EtcInfo = data.EtcInfo;
        restrictAreaTeamData.SetDiffficult(curDifficult);

        ArenaManager.Instance.SetRestrictedTeamDataSet(restrictAreaTeamData);
        ArenaManager.Instance.SendRestrictedAreaFight(curSelectedIndex, currentDragons, curDifficult, asset);

        UICanvas.Instance.EndBackgroundBlurEffect();
    }

    public void Done()
    {
        WWWForm param = new WWWForm();
        param.AddField("world", curSelectedIndex);
        //param.AddField("deck", JsonConvert.SerializeObject(currentDragons.ToArray()));
        param.AddField("travel_tag", SelectedSlot.IntVal("travel_tag"));
        param.AddField("diff", (int)curDifficult);
        NetworkManager.Send("travelinst/finish", param, (data) =>
        {
            //Data[curSelectedIndex.ToString()] = data;
            Data[((int)curDifficult).ToString()][curSelectedIndex.ToString()]["travel_tag"] = 0;

            List<Asset> totalRewards = new List<Asset>();
            if (data.ContainsKey("rewards"))
            {
                totalRewards.AddRange(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(data["rewards"])));
                if (totalRewards.Count > 0)
                {
                    SystemRewardPopup.OpenPopup(totalRewards.ToList());
                }
            }
            Refresh();
        });
    }


    public void OnTeamSetting()
    {
        RestrictedAreaData curData = RestrictedAreaData.GetByWorldDiff(curSelectedIndex, curDifficult);
        if (curData == null)
        {
            Debug.LogError("데이터 못찾음");
            return;

        }

        int cap = curDifficult == StageDifficult.HARD ? curData.NEED_BP : -1;
        TeamSetting = true;
        RestrictedInfoPopupData newPopupData = new RestrictedInfoPopupData(curData.NEED_WORLD_CLEAR, curDifficult, usingDragons, cap);
        PopupManager.OpenPopup<RestrictedAreaReadyPopup>(newPopupData);
        TeamSetting = false;
    }

    public bool IsCloseAble()
    {
        if (TeamSetting)
            return false;

        if (PopupManager.IsPopupOpening(PopupManager.GetPopup<RestrictedAreaReadyPopup>()))
            return false;

        return true;
    }

    void SetSlot(int slot, UserDragon data)
    {
        if (characterSlots == null)
        {
            return;
        }
        var slotCount = characterSlots.Count;
        var curSlot = characterSlots[slot];

        if (curSlot.ContainsKey("dimd") && curSlot["dimd"] != null)
        {
            curSlot["dimd"].SetActive(slot >= MAX_SELECT_COUNT);

            if (curSlot.ContainsKey("plus") && curSlot["plus"] != null)
            {
                curSlot["plus"].SetActive(slot < MAX_SELECT_COUNT);
            }
        }

        if (slot < 0 || slot >= slotCount)
        {
            return;
        }

        if (curSlot == null)
        {
            return;
        }

        if (data == null)
        {
            if (curSlot.ContainsKey("target"))
            {
                var targetGo = curSlot["target"];
                targetGo.SetActive(false);
            }

            if (curSlot.ContainsKey("container"))
            {
                var containerGo = curSlot["container"];
                SBFunc.RemoveAllChildrens(containerGo.transform);
            }
            return;
        }

        if (curSlot.ContainsKey("target"))
        {
            var targetGo = curSlot["target"];
            targetGo.SetActive(true);
        }

        if (curSlot.ContainsKey("container"))
        {
            var containerGo = curSlot["container"];
            SBFunc.RemoveAllChildrens(containerGo.transform);

            //var clone = Instantiate(User.Instance.DragonData.GetNameDragonSpine(data.Image(), eSpineType.UI), containerGo.transform);
            //var dragonSpine = clone.GetComponent<UIDragonSpine>();
            //if (dragonSpine != null)
            //{
            //    dragonSpine.Data = data.BaseData;
            //}
            var clone = Instantiate(dragonPortraitFrame, containerGo.transform);//드래곤 스파인이 아닌, 드래곤 포트레이트로 기획 변경
            clone.transform.localPosition = Vector3.zero;
            clone.gameObject.SetActive(true);
            var frame = clone.GetComponent<DragonPortraitFrame>();
            if (frame == null)
            {
                return;
            }
            frame.SetDragonPortraitFrame(data, false, false);
        }
    }

    void DelAllSlot()
    {
        for (var i = 0; i < 5; ++i)
        {
            SetSlot(i, null);
        }
    }

    public void OnEvent(DragonChangedEvent eventType)
    {
        RefreshRightUI();
    }
}
