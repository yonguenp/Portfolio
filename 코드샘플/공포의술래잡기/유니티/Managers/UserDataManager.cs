using Newtonsoft.Json.Linq;
using SBCommonLib;
using SBSocketSharedLib;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UserDataManager
{
    public string MyWebSessionID { get; private set; }
    public string MySocketSessionID { get; private set; }
    public long MyUserID { get; private set; } = 0;

    public string MyName { get; private set; }
    public int MyDefaultSurvivorCharacter { get; private set; }
    public int MyDefaultChaserCharacter { get; private set; }
    public int MyRankingSeasonID { get { return GetRankingSeason(); } }
    public string MyClanName { get; private set; }

    //public PlayerResult MyGameResult { get; private set; }
    public SCBcGameResult GameResult { get; private set; }
    public Dictionary<int, UserCharacterData> MyCharacters { get; private set; }
    private Dictionary<ItemGameData, int> MyItems = new Dictionary<ItemGameData, int>();
    private Dictionary<int, int> MyShopHistory = new Dictionary<int, int>();
    public Dictionary<int, UserEquipData> MyEquips = new Dictionary<int, UserEquipData>();

    public int MyGold { get; private set; }

    int MyFreeDia = 0;
    int MyCashDia = 0;
    int prevGold = 0;
    public int MyDia { get { return MyFreeDia + MyCashDia; } }

    public int MyMileage { get; private set; }

    public int MyPoint { get; private set; }
    public int MyHightPoint { get; private set; }
    public RankType MyRank { get { return RankType.GetRankFromPoint(MyPoint); } }
    public bool IsLobbyShown { get; private set; } = false;
    public bool ShownNotice { get; private set; } = false;

    public DateTime ADSeen_GACHA { get; private set; } = DateTime.MaxValue;
    public DateTime ADSeen_PACK1 { get; private set; } = DateTime.MaxValue;
    public DateTime ADSeen_PACK2 { get; private set; } = DateTime.MaxValue;
    public DateTime ADSeen_PACK3 { get; private set; } = DateTime.MaxValue;

    //유져가 보상받은 퀘스트 리스트
    public List<int> userRewardedQuest = new List<int>();
    //현재의 퀘스트 리스트
    public Dictionary<int, int> userQuestDic = new Dictionary<int, int>();
    public SeasonData seasonData { get; private set; } = new SeasonData();
    public JToken dayRankData { get; private set; } = null;
    public JToken dayClanRankData { get; private set; } = null;
    public JObject LimitedIAP { get; private set; } = null;
    public bool IsAttendanceChecked(int atten_id)
    {
        DateTime nowDate = SBCommonLib.SBUtil.KoreanTime.AddHours(-4);
        bool ret = CacheUserData.GetInt("attendance_" + atten_id.ToString(), 0) == nowDate.DayOfYear + 1;
        switch (atten_id)
        {
            case 1:
                break;
            default:
                if (GetAttendanceDay(atten_id) >= AttendanceGameData.GetAttendanceDayCount(atten_id))
                    return true;
                break;
        }

        return ret;
    }

    public int GetAttendanceDay(int atten_id)
    {
        return CacheUserData.GetInt("atten_count_" + atten_id.ToString(), 0);
    }

    public int RANK_SEANSON_ID { get { return PlayerPrefs.GetInt("rank_sseason_uid", 0); } set { PlayerPrefs.SetInt("rank_sseason_uid", value); } }

    public Dictionary<int, bool> ADEnables = new Dictionary<int, bool>();

    public int buff_hp { get; private set; } = 0;
    public int buff_atk { get; private set; } = 0;
    public int buff_gold { get; private set; } = 0;
    public int buff_item { get; private set; } = 0;

    public void AdvertisementInfo(JToken datas)
    {
        JObject data = (JObject)datas;
        if (data != null)
        {
            if (data.ContainsKey("gacha_last_seen"))
            {
                ADSeen_GACHA = DateTime.Parse(datas["gacha_last_seen"].Value<string>());
            }
            if (data.ContainsKey("ad_package1"))
            {
                ADSeen_PACK1 = DateTime.Parse(datas["ad_package1"].Value<string>());
            }
            if (data.ContainsKey("ad_package2"))
            {
                ADSeen_PACK2 = DateTime.Parse(datas["ad_package2"].Value<string>());
            }
            if (data.ContainsKey("ad_package3"))
            {
                ADSeen_PACK3 = DateTime.Parse(datas["ad_package3"].Value<string>());
            }
        }

        // 준형 :: 광고 시청시 로비에 있는 빨콩 표기
        DateTime pivot = System.DateTime.MaxValue;
        DateTime ableTime = pivot;

        if (ADSeen_PACK1 != null && ADSeen_PACK1 < System.DateTime.MaxValue)
        {
            ADEnables[1] = true;
            pivot = ADSeen_PACK1;

            ableTime = pivot.AddHours(4);
            ADEnables[1] = ADEnables[1] && pivot < System.DateTime.MaxValue && SBCommonLib.SBUtil.KoreanTime > ableTime;

        }
        if (ADSeen_PACK2 != null && ADSeen_PACK2 < System.DateTime.MaxValue)
        {
            ADEnables[2] = true;
            pivot = ADSeen_PACK2;

            ableTime = pivot.AddHours(4);//.AddDays(1);
            //ableTime = ableTime.AddHours((ableTime.Hour * -1) + 4).AddMinutes((ableTime.Minute * -1)).AddSeconds((ableTime.Second * -1));

            ADEnables[2] = ADEnables[2] && pivot < System.DateTime.MaxValue && SBCommonLib.SBUtil.KoreanTime > ableTime;
        }
        if (ADSeen_GACHA != null && ADSeen_GACHA < System.DateTime.MaxValue)
        {
            ADEnables[3] = 
                ADSeen_GACHA < System.DateTime.MaxValue && SBCommonLib.SBUtil.KoreanTime > Managers.UserData.ADSeen_GACHA.AddHours(GameConfig.Instance.GACHA_ADVERTISEMENT_TIME);

            var lobbyScene_gacha = Managers.Scene.CurrentScene as LobbyScene;
            if(lobbyScene_gacha != null)
                lobbyScene_gacha.OnRedDot(lobbyScene_gacha.lobbyBtns[2].transform, ADEnables[3]);
        }

        NotifyEvent.Trigger(NotifyEvent.NotifyEventMessage.ON_SHOP_INFO);

        var lobbyScene = Managers.Scene.CurrentScene as LobbyScene;
        if (lobbyScene != null)
            lobbyScene.CheckShopRedDot();
    }

    int GetRankingSeason()
    {
        int rank_uid = 0;
        var rankingTable = Managers.Data.GetData(GameDataManager.DATA_TYPE.ranking);
        foreach (RankingGameData item in rankingTable)
        {
            if (SBUtil.KoreanTime >= DateTime.Parse(item.start_time) && SBUtil.KoreanTime <= DateTime.Parse(item.end_time))
            {
                rank_uid = item.uid;
                break;
            }
        }

        if (rank_uid == 0)
        {
            TimeSpan minTime = TimeSpan.MaxValue;
            foreach (RankingGameData item in rankingTable)
            {
                var diffTime = SBUtil.KoreanTime - DateTime.Parse(item.end_time);
                if (diffTime.TotalSeconds > 0)
                {
                    if (minTime > diffTime)
                    {
                        minTime = diffTime;
                        rank_uid = item.uid;
                    }
                }
            }
        }
        return rank_uid;
    }
    public void PlayDataJToken(JToken datas)
    {
        foreach (var myChar in MyCharacters)
        {
            myChar.Value.playData.Clear();
        }

        if (datas["daily"].Type == JTokenType.Array)
        {
            foreach (JToken data in (JArray)datas["daily"])
            {
                if (data.Type == JTokenType.Null)
                    continue;

                PlayData pd = new PlayData(data);
                if (MyCharacters.ContainsKey(pd.charid))
                {
                    MyCharacters[pd.charid].AttachPlayData(pd);
                }
            }
        }

        if (datas["weekly"].Type == JTokenType.Array)
        {
            foreach (JToken data in (JArray)datas["weekly"])
            {
                if (data.Type == JTokenType.Null)
                    continue;
                PlayData pd = new PlayData(data);
                if (MyCharacters.ContainsKey(pd.charid))
                {
                    MyCharacters[pd.charid].AttachPlayData(pd);
                }
            }
        }

        if (datas["accum"].Type == JTokenType.Array)
        {
            foreach (JToken data in (JArray)datas["accum"])
            {
                if (data.Type == JTokenType.Null)
                    continue;
                PlayData pd = new PlayData(data);
                if (MyCharacters.ContainsKey(pd.charid))
                {
                    MyCharacters[pd.charid].AttachPlayData(pd);
                }
            }
        }

        SBDebug.Log("플레이 데이터 업데이트 완료");
    }


    public bool ShownTutorial
    {
        get { return CacheUserData.GetBoolean("shown_tutorial"); }
        set { CacheUserData.SetBoolean("shown_tutorial", value); }
    }
    public int RankPlayCount
    {
        get { return CacheUserData.GetInt("rank_game"); }
        set { CacheUserData.SetInt("rank_game", value); }
    }

    public int CustomPlayCount
    {
        get { return CacheUserData.GetInt("custom_game"); }
        set { CacheUserData.SetInt("custom_game", value); }
    }

    public int GachaCount
    {
        get { return CacheUserData.GetInt("gacha_game"); }
        set { CacheUserData.SetInt("gacha_game", value); }
    }

    public bool IsRankChanged()
    {
        int cacheRank = CacheUserData.GetInt("saved_rank", 0);
        if (0 == cacheRank)
        {
            cacheRank = MyRank.GetID();
            CacheUserData.SetInt("saved_rank", cacheRank);
        }

        return cacheRank != MyRank.GetID();
    }

    //UserInfo MyUserInfo = null;
    public void SetMySocketSessionID(byte[] id)
    {
        MySocketSessionID = new Guid(id).ToString();
    }
    public void SetWebSessionID(string websessionID)
    {
        MyWebSessionID = websessionID;
    }

    public void OnUserData(JObject userData)
    {
        if (userData.ContainsKey("user"))
            Managers.UserData.SetMyUserInfo(userData["user"]);
        if (userData.ContainsKey("characters"))
            Managers.UserData.SetMyCharacterInfo(userData["characters"]);
        if (userData.ContainsKey("session_id"))
            Managers.UserData.SetWebSessionID(userData["session_id"].Value<string>());

        if (userData.ContainsKey("items"))
            Managers.UserData.SetMyItemInfo(userData["items"]);
        if (userData.ContainsKey("buffinfo"))
            Managers.UserData.SetMyItemInfo(userData["buffinfo"]);

        Managers.UserData.ShownNotice = false;
    }

    public void SetMyUserInfo(JToken data)
    {
        if (data == null)
        {
            return;
        }

        bool bFirstInit = MyUserID == 0;

        MyUserID = data["user_no"].Value<long>();
        MyName = data["nick"].Value<string>();

        SetMyDefaultCharacter(data["cur_chaser"].Value<int>());
        SetMyDefaultCharacter(data["cur_survivor"].Value<int>());

        try
        {
            MyGold = data["gold"].Value<int>();
            MyFreeDia = data["dia"].Value<int>();
            MyCashDia = data["dia_cash"].Value<int>();
            MyMileage = data["mileage"].Value<int>();
        }
        catch (Exception)
        {
            MyGold = 0;
            MyFreeDia = 0;
            MyCashDia = 0;
            MyMileage = 0;
            PopupCanvas.Instance.ShowFadeText("재화정보오류");
        }

        var clanName = data["clan_name"];
        if (clanName != null)
        {
            if (MyClanName != clanName.Value<string>())
            {
                if(!bFirstInit)
                    Managers.Network.SendCSClanInfoUpdateNotify();

                Managers.ClanCaht.Clear();
                MyClanName = clanName.Value<string>();
            }
        }

        UpdatePoint(data["point"]);

        var recommend = data["recommend"];
        if (recommend != null)
            GameConfig.Instance.FRIEND_RECOMMEND = recommend.Value<int>() == 1;

        var friend_max = data["friend_max"];
        if (friend_max != null)
            Managers.FriendData.FRIEND_MAX_COUNT = friend_max.Value<int>();

        NotifyEvent.Trigger(NotifyEvent.NotifyEventMessage.ON_USER_INFO);
    }

    public void SetMyCharacterInfo(JToken characters)
    {
        if (MyCharacters == null)
            MyCharacters = new Dictionary<int, UserCharacterData>();

        MyCharacters.Clear();
        foreach (JToken data in (JArray)characters)
        {
            UserCharacterData tmp = new UserCharacterData(data);
            MyCharacters.Add(tmp.characterData.GetID(), tmp);
        }

        NotifyEvent.Trigger(NotifyEvent.NotifyEventMessage.ON_CHARACTERS_INFO);

        RefreshQuestCharacterData();
    }

    public void UpdateMyCharacter(JToken charData)
    {
        if (charData == null)
            return;

        var lobby = Managers.Scene.CurrentScene as LobbyScene;

        if (charData.Type == JTokenType.Array)
        {
            foreach (var data in (JArray)charData)
            {
                if (MyCharacters.ContainsKey(data["uid"].Value<int>()))
                {
                    UserCharacterData tmp = MyCharacters[data["uid"].Value<int>()];
                    tmp.SetData(data);
                }
                else
                {
                    UserCharacterData tmp = new UserCharacterData(data);
                    MyCharacters.Add(tmp.characterData.GetID(), tmp);
                    if(lobby)
                        lobby.OnRedDot(lobby.lobbyBtns[0].transform, true);
                }
            }
        }
        else
        {
            if (MyCharacters.ContainsKey(charData["uid"].Value<int>()))
            {
                UserCharacterData tmp = MyCharacters[charData["uid"].Value<int>()];
                tmp.SetData(charData);
            }
            else
            {
                UserCharacterData tmp = new UserCharacterData(charData);
                MyCharacters.Add(tmp.characterData.GetID(), tmp);
                if (lobby)
                    lobby.OnRedDot(lobby.lobbyBtns[0].transform, true);
            }
        }


        NotifyEvent.Trigger(NotifyEvent.NotifyEventMessage.ON_CHARACTER_UPDATE);

        RefreshQuestCharacterData();
    }

    public void SetMyItemInfo(JToken data)
    {
        if (data == null)
            return;

        Dictionary<ItemGameData, int> prevItems = null;
        if(MyItems.Count != 0)
            prevItems = new Dictionary<ItemGameData,int>(MyItems);

        MyItems.Clear();

        JArray items = null;
        if (data.Type != JTokenType.Array)
        {
            if (data.Type == JTokenType.Object)
            {
                JObject item_obj = (JObject)data;
                if (item_obj.ContainsKey("items") && item_obj["items"].Type == JTokenType.Array)
                {
                    items = (JArray)item_obj["items"];
                }

                if (item_obj.ContainsKey("equips") && item_obj["equips"].Type == JTokenType.Object)
                {
                    SetMyEquipItems(item_obj["equips"]);
                }

                if (item_obj.ContainsKey("buffinfo"))
                {
                    UpdateBuff(item_obj["buffinfo"]);
                }
            }
        }
        else
        {
            items = (JArray)data;
        }

        if (items == null)
            return;

        foreach (JToken item in (JArray)items)
        {
            UpdateMyItem(item, false);
        }

        NotifyEvent.Trigger(NotifyEvent.NotifyEventMessage.ON_ITEM_INFO);
                
        var curScene = Managers.Scene.CurrentScene as LobbyScene;
        if (curScene != null)
        {
            if (prevItems != null)
            {
                bool reddot = false;
                foreach (var item in MyItems)
                {
                    if (item.Key == null)
                        continue;

                    if (item.Key.type == ItemGameData.ITEM_TYPE.EMOTICON)
                        continue;

                    if (!prevItems.ContainsKey(item.Key))
                    {
                        reddot = true;
                        break;
                    }

                    if (prevItems[item.Key] < item.Value)
                    {
                        reddot = true;
                        break;
                    }
                }

                if (reddot)
                {
                    curScene.OnRedDot(curScene.lobbyBtns[7].transform, true);
                    CombineInventoryPopup inven = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.COMBINEINVENTORY_POPUP) as CombineInventoryPopup;
                    if (inven != null)
                        inven.inventory.CheckNewItems(prevItems);
                }                
            }
            else
            {
                curScene.OnRedDot(curScene.lobbyBtns[7].transform, false);
            }
        }
    }

    public void UpdateMyItem(JToken item, bool notifyEvent = true)
    {
        int itemNo = 0;
        int amount = 0;
        if (item.Type == JTokenType.Object)
        {
            itemNo = item["item_no"].Value<int>();
            amount = item["amount"].Value<int>();
        }
        else if (item.Type == JTokenType.Array)
        {
            itemNo = item[0].Value<int>();
            amount = item[1].Value<int>();
        }

        if (itemNo == 0)
            return;

        ItemGameData data = ItemGameData.GetItemData(itemNo);
        if (data == null)
            return;

        MyItems[data] = amount;

        if (notifyEvent)
            NotifyEvent.Trigger(NotifyEvent.NotifyEventMessage.ON_ITEM_UPDATE);
    }

    public void SetMyShopInfo(JObject data)
    {
        if (data == null)
            return;

        MyShopHistory.Clear();
        if (data.ContainsKey("cnt") && data["cnt"].Type == JTokenType.Object)
        {
            foreach (JToken item in data["cnt"])
            {
                JProperty ary = item.ToObject<JProperty>();
                MyShopHistory.Add(int.Parse(ary.Name.ToString()), int.Parse(ary.Value.ToString()));
            }
        }

        if (data.ContainsKey("limit"))
        {
            SetLimitedIAP(data["limit"]);
        }

        if (data.ContainsKey("subs") && data["subs"].Type == JTokenType.Array)
        {
            JArray items = (JArray)data["subs"];
            foreach (JObject item in items)
            {
                if (item.ContainsKey("prod"))
                {
                    MyShopHistory[item["prod"].Value<int>()] = 1;
                    SBWeb.RequestSubscribeReward(item["prod"].Value<int>());
                }
            }

        }
    }

    public void SetMyEquipItems(JToken datas)
    {
        if (datas == null)
            return;

        MyEquips.Clear();

        foreach (var data in (JObject)datas)
        {
            UserEquipData equip = new UserEquipData(data.Value);
            if (equip.id < 0)
                continue;

            MyEquips[equip.id] = equip;
        }   
    }

    public void UpdateMyShopInfo(int key, int cnt)
    {
        if (MyShopHistory.ContainsKey(key))
            MyShopHistory[key] += cnt;
        else
            MyShopHistory.Add(key, cnt);
    }

    public void SetLimitedIAP(JToken info)
    {
        if (info == null && info.Type != JTokenType.Object)
            return;

        LimitedIAP = (JObject)info;
    }

    public int GetMyItemCount(int itemNo)
    {
        ItemGameData data = ItemGameData.GetItemData(itemNo);
        if (data == null)
            return 0;

        if (MyItems.ContainsKey(data))
            return MyItems[data];
        return 0;
    }

    public Dictionary<ItemGameData, int> GetAllMyItemData()
    {
        return MyItems;
    }

    public int GetMyShopHistory(int goodsID)
    {
        if (MyShopHistory.ContainsKey(goodsID))
            return MyShopHistory[goodsID];

        return 0;
    }

    //public UserCharacterData GetMyCharacterInfo(CharacterGameData data)
    public UserCharacterData GetMyCharacterInfo(int id)
    {
        if (MyCharacters != null && MyCharacters.ContainsKey(id))
        {
            return MyCharacters[id];
        }

        return null;
    }

    public void ResetMyGameResult()
    {
        //MyGameResult = null;
    }

    public void ResetGameResult()
    {
        GameResult = null;
    }

    // public void SetMyGameResult(PlayerResult _result)
    // {
    //     MyGameResult = _result;
    // }

    public void SetGameResult(SCBcGameResult _result)
    {
        GameResult = _result;
    }

    public void SetMyDefaultCharacter(int type)
    {
        if (CharacterGameData.IsChaserCharacter(type))
        {
            MyDefaultChaserCharacter = type;
        }
        else
        {
            MyDefaultSurvivorCharacter = type;
        }
    }

    public void UpdatePoint(JToken point)
    {
        if (point != null)
            MyPoint = point.Value<int>();
    }
    public void UpdateHightPoint(int h_point)
    {
        MyHightPoint = h_point;
    }
    public void SetLobbyVisit()
    {
        IsLobbyShown = true;
    }

    public void ShowFirstNotice()
    {
        ShownNotice = true;
    }
    public void UpdateBuff(JToken bi)
    {
        JObject buffInfo = (JObject)bi;
        if (buffInfo == null)
            return;

        buff_hp = buffInfo["buff_hp"].Value<int>();
        buff_atk = buffInfo["buff_atk"].Value<int>();
        buff_gold = buffInfo["buff_gold"].Value<int>();
        buff_item = buffInfo["buff_item"].Value<int>();
    }

    public void SetMyQuestDBData(JToken datas)
    {
        userRewardedQuest.Clear();
        userQuestDic.Clear();

        if (datas["rewarded"].HasValues)
        {
            foreach (var item in datas["rewarded"])
            {
                userRewardedQuest.Add(item.Value<int>());
            }
        }

        if (datas["cur"].HasValues)
        {
            foreach (var item in datas["cur"])
            {
                JProperty obj = (JProperty)item;
                userQuestDic.Add(Convert.ToInt32(obj.Name), obj.Value.Value<int>());
            }
        }
        //SBDebug.Log("퀘스트 데이터 업데이트 완료");

        prevGold = MyGold;

        var LobbyScene = Managers.Scene.CurrentScene as LobbyScene;
        if (LobbyScene != null)
        {
            LobbyScene.CheckQuestRedDot();
        }
    }

    public void RefreshQuestGold()
    {
        int diff = Managers.UserData.MyGold - prevGold;
        prevGold = Managers.UserData.MyGold;

        if (diff >= 0)
            return;

        List<int> groupQuestList = new List<int>();
        List<QuestData> targetDatas = new List<QuestData>();
        foreach (var uq in userQuestDic)
        {
            QuestData data = QuestData.GetQuestData(uq.Key);
            if (data == null)
                continue;

            bool enableQuest = true;
            foreach (int prev in QuestData.GetPrevQuests(data.GetID()))
            {
                enableQuest = Managers.UserData.IsContainClearQuest(prev);

                if (!enableQuest)
                    break;
            }
            if (!enableQuest)
                continue;

            switch (data.quest_clear_type)
            {
                case 23://특정 그룹 퀘스트 모두 클리어
                    if (data.clear_count > uq.Value)
                        groupQuestList.Add(uq.Key);
                    break;
                case 6: //사용한 골드 수
                    targetDatas.Add(data);
                    break;
            }
        }

        foreach (QuestData data in targetDatas)
        {
            switch (data.quest_clear_type)
            {
                case 6:
                    userQuestDic[data.GetID()] += Mathf.Abs(diff);
                    break;
            }
        }

        foreach (var groupQuest in groupQuestList)
        {
            int clearQuestCount = 0;
            QuestData data = QuestData.GetQuestData(groupQuest);

            foreach (var uq in userQuestDic)
            {
                QuestData qd = QuestData.GetQuestData(uq.Key);
                if (qd == null)
                {
                    continue;
                }

                if (data.param == qd.group_uid)
                {
                    if (qd.clear_count <= uq.Value)
                    {
                        clearQuestCount += 1;
                    }
                }
            }

            foreach (var rq in userRewardedQuest)
            {
                QuestData qd = QuestData.GetQuestData(rq);
                if (qd == null)
                {
                    continue;
                }

                if (data.param == qd.group_uid)
                {
                    clearQuestCount += 1;
                }
            }

            userQuestDic[groupQuest] = clearQuestCount;
        }

        var LobbyScene = Managers.Scene.CurrentScene as LobbyScene;
        if (LobbyScene != null)
        {
            LobbyScene.CheckQuestRedDot();
        }
    }
    public void RefreshQuestCharacterData()
    {
        List<int> groupQuestList = new List<int>();
        List<QuestData> targetDatas = new List<QuestData>();
        foreach (var uq in userQuestDic)
        {
            QuestData data = QuestData.GetQuestData(uq.Key);
            if (data == null)
                continue;

            bool enableQuest = true;
            foreach (int prev in QuestData.GetPrevQuests(data.GetID()))
            {
                enableQuest = Managers.UserData.IsContainClearQuest(prev);

                if (!enableQuest)
                    break;
            }

            if (!enableQuest)
                continue;

            switch (data.quest_clear_type)
            {
                case 23://특정 그룹 퀘스트 모두 클리어
                    if (data.clear_count > uq.Value)
                        groupQuestList.Add(uq.Key);
                    break;
                case 24://특정 캐릭터 입수하기
                    targetDatas.Add(data);
                    break;
                case 25://특정 캐릭터 레벨업                    
                    targetDatas.Add(data);
                    break;
                case 26://특정 캐릭터 등급 업
                    targetDatas.Add(data);
                    break;
                case 27://특정 캐릭터 스킬 업 
                    targetDatas.Add(data);
                    break;
                case 28://캐릭터 콜랙션
                    targetDatas.Add(data);
                    break;
            }
        }

        foreach (QuestData data in targetDatas)
        {
            switch (data.quest_clear_type)
            {
                case 24://특정 캐릭터 입수하기
                    userQuestDic[data.GetID()] = Managers.UserData.GetMyCharacterInfo(data.param) != null ? 1 : 0;
                    break;
                case 25://특정 캐릭터 레벨업                    
                    {
                        var charInfo = Managers.UserData.GetMyCharacterInfo(data.param);
                        if (charInfo != null)
                        {
                            userQuestDic[data.GetID()] = charInfo.lv;
                        }
                    }
                    break;
                case 26://특정 캐릭터 등급 업
                    {
                        var charInfo = Managers.UserData.GetMyCharacterInfo(data.param);
                        if (charInfo != null)
                        {
                            userQuestDic[data.GetID()] = charInfo.enchant - 1;
                        }
                    }
                    break;
                case 27://특정 캐릭터 스킬 업 
                    {
                        var charInfo = Managers.UserData.GetMyCharacterInfo(data.param);
                        if (charInfo != null)
                        {
                            userQuestDic[data.GetID()] = charInfo.skillLv - 1;
                        }
                    }
                    break;
                case 28://캐릭터 콜랙션
                    {
                        int count = 0;
                        foreach (CollectionCharGroupData item in CollectionCharGroupData.GetGroupData(data.param))
                        {
                            var charInfo = Managers.UserData.GetMyCharacterInfo(item.char_uid);
                            if (charInfo != null)
                            {
                                if(charInfo.lv >= item.level_value && charInfo.skillLv >= item.skill_value)
                                    count++;
                            }
                        }

                        userQuestDic[data.GetID()] = count;
                    }
                    break;
            }
        }

        foreach (var groupQuest in groupQuestList)
        {
            int clearQuestCount = 0;
            QuestData data = QuestData.GetQuestData(groupQuest);

            foreach (var uq in userQuestDic)
            {
                QuestData qd = QuestData.GetQuestData(uq.Key);
                if (qd == null)
                {
                    continue;
                }

                if (data.param == qd.group_uid)
                {
                    if (qd.clear_count <= uq.Value)
                    {
                        clearQuestCount += 1;
                    }
                }
            }

            foreach (var rq in userRewardedQuest)
            {
                QuestData qd = QuestData.GetQuestData(rq);
                if (qd == null)
                {
                    continue;
                }

                if (data.param == qd.group_uid)
                {
                    clearQuestCount += 1;
                }
            }

            userQuestDic[groupQuest] = clearQuestCount;
        }

        var LobbyScene = Managers.Scene.CurrentScene as LobbyScene;
        if (LobbyScene != null)
        {
            LobbyScene.CheckQuestRedDot();
        }
    }

    public bool IsContainClearQuest(int id)
    {
        return userRewardedQuest.Contains(id);
    }

    public int GetFreeDia()
    {
        return MyFreeDia;
    }

    public int GetCashDia()
    {
        return MyCashDia;
    }

    public void SetPassData(JToken pass)
    {
        seasonData.Clear();

        if (pass != null)
        {
            seasonData.SetData(pass["season"].Value<int>(), pass["level"].Value<int>(), pass["exp"].Value<int>(), pass["vip"].Value<int>(), (JArray)pass["rewarded"], (JArray)pass["vip_rewarded"]);
        }
    }

    public void SetDayRank(JToken data)
    {
        dayRankData = data;
        if (dayRankData == null)
            dayRankData = "error";
    }

    public void SetClanRank(JToken data)
    {
        dayClanRankData = data;
    }

    public void ClearClanRank()
    {
        dayClanRankData = null;
    }
}

public class UserCharacterData
{
    public CharacterGameData characterData { get; private set; }
    public int lv { get; private set; } = 0;
    public int exp { get; private set; } = 0;
    public int enchant { get; private set; } = 0;
    public int skillLv { get; private set; } = 0;
    public int talent1 { get; private set; } = 0;
    public int talent2 { get; private set; } = 0;
    public int talent3 { get; private set; } = 0;
    private int equipment = 0;
    public UserEquipData curEquip
    {
        get
        {
            if (Managers.UserData.MyEquips.ContainsKey(equipment)) 
                return Managers.UserData.MyEquips[equipment]; 
            return null;
        }
    }


    public DateTime gainDate { get; private set; }

    public PlayData playData { get; private set; } = new PlayData();
    public UserCharacterData(JToken data)
    {
        SetData(data);
    }

    public void SetData(JToken data)
    {
        JObject d = (JObject)data;
        int uid = 0;
        if (d.ContainsKey("uid"))
        {
            uid = data["uid"].Value<int>();
        }
        else if (d.ContainsKey("character_uid"))
        {
            uid = data["character_uid"].Value<int>();
        }
        else
        {
            SBDebug.LogError("이상한 캐릭터 uid 접근");
            return;
        }

        characterData = CharacterGameData.GetCharacterData(uid);

        exp = data["exp"].Value<int>();
        lv = data["level"].Value<int>();
        enchant = data["enhance_step"].Value<int>();
        skillLv = data["skill"].Value<int>();
        talent1 = data["talent1"].Value<int>();
        talent2 = data["talent2"].Value<int>();
        talent3 = data["talent3"].Value<int>();
        
        if(data["equipment"] != null)
        {
            equipment = data["equipment"].Value<int>();
        }
        else
        {
            equipment = 0;
        }
        
        gainDate = DateTime.Parse(data["obtain_at"].Value<string>());

        playData.charid = uid;
    }

    public void AttachPlayData(PlayData data)
    {
        playData += data;
    }

    public UserCharacterData(CharacterGameData charData)
    {
        characterData = charData;
    }
    public void SetData(int cur_level, int cur_exp, int cur_enchant, int cur_skilllv, DateTime gain_Date)
    {
        lv = cur_level;
        exp = cur_exp;
        enchant = cur_enchant;
        skillLv = cur_skilllv;
        gainDate = gain_Date;
    }

    public CharacterLevelGameData GetLevelData()
    {
        return characterData.levelData[lv];
    }

    public SkillGameData GetSkillData()
    {
        return characterData.GetSkillData();
    }

    public CharacterSkillLevelGameData GetSkillNextLevelData()
    {
        return characterData.GetCharacterSkillLevelGameData(skillLv + 1);
    }
    public SkillBaseGameData GetSkillBaseData()
    {
        SkillGameData skillData = GetSkillData();
        if (skillData != null)
            return skillData.GetMajorSkill(skillLv);

        return null;
    }
    public SkillGameData GetAtkSkillData()
    {
        return characterData.GetAtkSkillData();
    }

};

public class UserEquipData
{
    public int id { get; private set; } = 0;
    public int lv { get; private set; } = 1;
    public int exp { get; private set; } = 0;
    public int item_no { get; private set; } = 0;

    public EquipInfo equipData { get; private set; } = null;
    public DateTime update_time { get; private set; }

    public UserEquipData(JToken data)
    {
        SetData(data);
    }

    public void SetData(JToken data)
    {
        JObject d = (JObject)data;
        
        if (d.ContainsKey("id"))
        {
            item_no = data["item_no"].Value<int>();
        }
        else
        {
            SBDebug.LogError("이상한 item_no 접근");
            return;
        }

        equipData = EquipInfo.GetEquipData(item_no);

        exp = data["exp"].Value<int>();
        lv = data["level"].Value<int>();
        id = data["id"].Value<int>();

        update_time = DateTime.Parse(data["updates"].Value<string>());
    }
}

static public class CacheUserData
{
    static public bool GetBoolean(string key, bool unset_defulat = false)
    {
        JToken value = GetValue(key);
        if (value != null)
        {
            if (value.Type != JTokenType.Boolean)
                return unset_defulat;

            return value.Value<bool>();
        }

        return unset_defulat;
    }


    static public void SetBoolean(string key, bool value)
    {
        SetValue(key, value);
    }
    static public int GetInt(string key, int unset_defulat = 0)
    {
        JToken value = GetValue(key);
        if (value != null)
        {
            if (value.Type != JTokenType.Integer)
                return unset_defulat;

            return value.Value<int>();
        }

        return unset_defulat;
    }

    static public void SetInt(string key, int value)
    {
        SetValue(key, value);
    }

    private static JToken GetValue(string key, string unset_defulat = null)
    {
        JObject dataArray = GetUserData();
        if (dataArray.ContainsKey(key))
        {
            return dataArray[key];
        }

        return unset_defulat;
    }

    private static void SetValue(string key, JToken value)
    {
        JObject userData = GetUserData();

        if (userData.ContainsKey(key))
        {
            userData[key] = value;
        }
        else
        {
            userData.Add(key, value);
        }

        PlayerPrefs.SetString(Managers.UserData.MyUserID.ToString(), userData.ToString(Newtonsoft.Json.Formatting.None));
    }

    private static JObject GetUserData()
    {
        string userData = PlayerPrefs.GetString(Managers.UserData.MyUserID.ToString(), "");

        if (string.IsNullOrEmpty(userData))
        {
            return new JObject();
        }
        else
        {
            try
            {
                return JObject.Parse(userData);
            }
            catch
            {
                return new JObject();
            }
        }
    }
};

