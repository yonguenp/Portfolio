using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class neco_data
{
    public enum PrologueSeq
    {
        //chapter1
        챕터1시작 = 0,
        길막이꼬리얼쩡댐,
        길막이등장영상,
        길막이뒷쪽에표시,
        길막이획득프로필팝업확인,
        길막이배고파연출,
        배고픈길막이대사,
        배고픈길막이가이드퀘스트,
        통발UI등장,
        통발UI닫힘,
        통발수급대사,
        조리대UI등장,
        조리대UI닫힘,
        조리대UI닫히고대사,
        첫밥그릇등장,
        첫밥주기완료,
        첫밥주기완료대사,
        길막배고픔연출사라짐,
        길막밥그릇돌진,
        길막밥먹는거대사,
        고양이10번터치가이드퀘스트,
        고양이10번터치가이드퀘스트완료,
        챕터1끝,

        //chapter2
        챕터2시작,
        챕터2시작대사,
        양어장UI등장,
        양어장골드연출,
        양어장닫고대사,
        철판제작가이드퀘스트,
        철판제작가이드퀘스트완료,
        철판제작가이드퀘스트완료대사,
        조리대레벨업,
        조리대레벨업완료,
        조리대레벨업완료후대사,
        상점배스구매가이드퀘스트,
        상점배스구매완료,
        상점배스구매완료대사,
        배스구이강조,
        배스구이완료후밥그릇강조,
        배스구이밥그릇에담음,
        뒷길막이선물생성,        
        뒷길막이선물획득대사,
        길막이배스구이먹음대사,
        보은바구니UI등장,
        첫보은받기성공,
        챕터2끝,

        배틀패스강조및대사,
        배틀패스보상획득,
        배틀패스종료대사,

        //chapter3
        챕터3시작,
        길막이퇴장,
        길막이심심해대사,
        낚시장난감만들기,
        낚시장난감만들기완료,
        길막이낚시장난감배치,
        길막이낚시돌발등장,
        길막이노잼대사,
        낚시장난감오브젝트레벨업가이드,
        길막이2레벨낚시장난감연출,
        길막이만지기돌발대사,
        길막이만지기돌발발생,
        길막이낚시돌발완료,
        사진찍기돌발대사,
        사진찍기완료대사,

        //길막이사진돌발발생,
        //길막이낚시돌발대사,

        //chapter4
        //챕터4시작,
        //낚시장난감오브젝트레벨업가이드,
        //길막이2레벨낚시장난감연출,
        //사진찍기돌발대사,

        //chapter5
        챕터5시작,
        스와이프가이드,
        삼색이등장,
        챕터5끝,

        프리플레이,

        배틀패스보상받기,
        제작대2레벨,
        방문고양이터치,
        말풍선아이템받기,
        우드락제작,
        화이트캣하우스제작,
        하트30회,
        양어장2레벨,
        바구니2레벨,
        골드받기,
        돌발아이콘터치,
        제작대3레벨,
        바구니3레벨,
        화장실제작,
        통발2레벨물고기10개획득,
        제작대4레벨,
        보온캣하우스제작,

        양어장3레벨,
        통발3레벨,
        조리대3레벨,
        빙어튀김요리,
        통발4레벨,
        조리대4레벨,
        잉어찜요리,
        바구니4레벨,
        양어장4레벨,
        통발5레벨,
        제작대5레벨,
        무스크래쳐제작,
        양어장5레벨,
        조리대5레벨,
        민물고기찜요리,
        바구니5레벨,
        통발6레벨,
        제작대6레벨,
        원목캣타워제작,
        양어장6레벨,
        조리대6레벨,
        바다고기회요리,
        바구니6레벨,
        통발7레벨,
        제작대7레벨,
        반자동화장실제작,
        양어장7레벨,
        조리대7레벨,
        장어구이요리,
        바구니7레벨,
        통발8레벨,
        제작대8레벨,
        파이프캣타워제작,
        양어장8레벨,
        조리대8레벨,
        참치구이요리,
        바구니8레벨,
        통발9레벨,
        제작대9레벨,
        나무위캣하우스제작,
        양어장9레벨,
        조리대9레벨,
        고급바다고기회요리,
        바구니9레벨,
        통발10레벨,
        제작대10레벨,
        크리스마스캣타워제작,
        양어장10레벨,
        조리대10레벨,
        무지개배스찜요리,
        바구니10레벨,
        제작대11레벨,
        캣닢급식소제작,
        캣닢급식소3레벨,
        캣닢급식소4레벨,
        캣닢급식소5레벨,
        화이트캣하우스5레벨,
        화이트캣하우스6레벨,
        화이트캣하우스7레벨,
        화이트캣하우스8레벨,
        화이트캣하우스9레벨,
        화이트캣하우스10레벨,
        제작대12레벨,
        플로랄캣하우스제작,
        플로랄캣하우스3레벨,
        플로랄캣하우스4레벨,
        플로랄캣하우스5레벨,
        알록달록이동식캣하우스제작,
        알록달록이동식캣하우스3레벨,
        알록달록이동식캣하우스4레벨,
        알록달록이동식캣하우스5레벨,

        PROLOGUE_SEQ_DONE,
    };

    public static uint DebugSeq = 0;

    private static PrologueSeq prologueSeq = PrologueSeq.챕터1시작;

    // debug
    private static UnityEvent seqUpdateEvent = null;

    public static PrologueSeq GetPrologueSeq() {
//#if UNITY_EDITOR
//        Debug.Log("NOW PrologueSEQ =====> " + prologueSeq);
//#endif
        return prologueSeq; }
    public static void SetPrologueSeq(PrologueSeq seq, bool saveToServer = false)
    {
        prologueSeq = seq;
        GameDataManager.Instance.GetUserData().data["contents"] = (uint)seq;
        PlayerPrefs.SetInt("contents", (int)seq);

        if(saveToServer)
        {
            WWWForm data = new WWWForm();
            data.AddField("api", "user");
            data.AddField("val", (int)prologueSeq);
            //SetPrologueSeq((PrologueSeq)((int)prologueSeq));

            NetworkManager.GetInstance().SendApiRequest("user", 4, data, (response) => {

            });

            OnSeqUpdate();
        }
    }
    public static void PrologueNextSeq()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "user");
        data.AddField("val", (int)prologueSeq + 1);
        SetPrologueSeq((PrologueSeq)((int)prologueSeq + 1));

        NetworkManager.GetInstance().SendApiRequest("user", 4, data, (response) => {
                        
        });

        OnSeqUpdate();
    }

    public static void AddListener(UnityAction cb)
    {
        if (null == seqUpdateEvent)
        {
            seqUpdateEvent = new UnityEvent();
        }
        seqUpdateEvent.AddListener(cb);
    }

    private static void OnSeqUpdate()
    {
        if (null != seqUpdateEvent)
        {
            seqUpdateEvent.Invoke();
        }

        TapjoyUnity.Tapjoy.SetUserLevel((int)prologueSeq);
    }

    public static bool CheckAndNextPrologueSeq(PrologueSeq seq)
    {
        if (prologueSeq == seq)
        {
            PrologueNextSeq();
            return true;
        }

        return false;
    }

    public static List<uint> ShownFlag = new List<uint>();

    public struct neco_gift_info
    {
        public neco_cat cat;
        public items item;
        public uint gold;
        public uint count;
    }

    public enum GAIN_STATE
    {
        NONE,
        SOME,
        FULL,
    };

    public enum BOOST_TYPE
    {
        NONE,
        CATNIP_BOOST,
        AD_BOOST,
    };

    // MAX 레벨 정의
    public static uint GIFT_MAX_LEVEL = 10;
    public static uint FISH_FARM_MAX_LEVEL = 10;
    public static uint FISH_TRAP_MAX_LEVEL = 10;
    public static uint COOK_MAX_LEVEL = 10;
    public static uint CRAFT_MAX_LEVEL = 12;
    public static uint OBJECT_MAX_LEVEL = 10;
    //

    private List<neco_gift_info> gift_list = new List<neco_gift_info>();
     
    private GAIN_STATE curFishState = GAIN_STATE.NONE;
    private static neco_data instance = null;
    private MainMenuPanel ui = null;
    private uint nextUpdate = 0;
    private uint goldFullUpdate = 0;
    private uint fishNextUpdate = 0;
    private uint giftFullUpdate = 0;

    private uint basketLevel = 10;
    private uint fishfarmLevel = 10;
    private uint fishtrapLevel = 10;
    private uint cookLevel = 10;
    private uint craftLevel = 10;

    private BOOST_TYPE giftBoostType = BOOST_TYPE.NONE;
    private BOOST_TYPE farmBoostType = BOOST_TYPE.NONE;
    private BOOST_TYPE trapBoostType = BOOST_TYPE.NONE;

    private uint giftBooster = 0;
    private uint farmBooster = 0;
    private uint trapBooster = 0;

    private List<uint> readyCatList = new List<uint>();
    private neco_pass_data passData = new neco_pass_data();
    private neco_market_data marketData = new neco_market_data();
    private Dictionary<uint, uint> purchaseHistory = new Dictionary<uint, uint>();
    private bool isFirstBuyBenefitEnable = false;
    private Dictionary<uint, uint> timeSaleRemain = new Dictionary<uint, uint>();
    private List<neco_subscribe_data> subscribes = new List<neco_subscribe_data>();
    private List<neco_event> events = new List<neco_event>();

    private uint newPostTimestamp = 0;
    private uint openPostTimestamp = 0;
    private uint PhotoAD = 0;
    private uint enableCatObjectCoin = 0;

    private fishing_data fishingData = new fishing_data();


    public static neco_data Instance
    {
        get
        {
            if (null == instance)
            {
                instance = new neco_data();
            }
            return instance;
        }
    }

    public void SetGiftStatusUI(MainMenuPanel mainUI)
    {
        ui = mainUI;

        if (ui != null)
        {
            ui.RefreshGiftButton();
        }
    }

    public void Clear()
    {
        gift_list.Clear();
    }

    public void Reset()
    {
        instance = new neco_data();
    }

    public List<neco_gift_info> GetGiftList()
    {
        return gift_list;
    }

    public void AddGift(neco_gift_info gift)
    {
        gift_list.Add(gift);
    }

    public void SetGoldFullTick(uint ft)
    {
        goldFullUpdate = ft;

        if (ui != null)
        {
            ui.RefreshGoldButton();
        }
    }

    public uint GetGoldFullTick()
    {
        return goldFullUpdate;
    }

    public GAIN_STATE CurGoldState()
    {
        if (goldFullUpdate == 0)
            return GAIN_STATE.FULL;

        if (NecoCanvas.GetCurTime() > goldFullUpdate)
            return GAIN_STATE.FULL;

        neco_level data = neco_level.GetNecoLevelData("GOLD", GetFishfarmLevel());
        uint FullCount = (data.GetNecoLevelValue2() / data.GetNecoLevelValue1());
        uint tick = data.GetNecoLevelTick();
        if (NecoCanvas.GetCurTime() > goldFullUpdate - (FullCount * tick))
            return GAIN_STATE.SOME;

        return GAIN_STATE.NONE;
    }

    public uint NextGoldUpdate()
    {
        GAIN_STATE curState = CurGoldState();
        switch (curState)
        {
            case GAIN_STATE.FULL:
                return 0;
            case GAIN_STATE.NONE:
            {
                if (goldFullUpdate == 0)
                    return 0;

                neco_level data = neco_level.GetNecoLevelData("GOLD", GetFishfarmLevel());
                uint FullCount = (data.GetNecoLevelValue2() / data.GetNecoLevelValue1());
                uint tick = data.GetNecoLevelTick();
                return (goldFullUpdate - ((FullCount - 1) * tick)) - (uint)NecoCanvas.GetCurTime();
            }
            case GAIN_STATE.SOME:
            {
                return goldFullUpdate - (uint)NecoCanvas.GetCurTime();                    
            }
        }
        
        return 0;
    }

    public void SetGiftFullTick(uint ft)
    {
        giftFullUpdate = ft;

        if (ui != null)
        {
            ui.RefreshGiftButton();
        }
    }

    public GAIN_STATE CurGiftState()
    {
        if (giftFullUpdate == 0)
            return GAIN_STATE.FULL;

        if (NecoCanvas.GetCurTime() > giftFullUpdate)
            return GAIN_STATE.FULL;

        neco_level data = neco_level.GetNecoLevelData("GIFT", neco_data.Instance.GetGiftBasketLevel());
        uint FullCount = (data.GetNecoLevelValue2() / data.GetNecoLevelValue1());
        uint tick = data.GetNecoLevelTick();
        if (NecoCanvas.GetCurTime() > giftFullUpdate - (FullCount * tick))
            return GAIN_STATE.SOME;

        return GAIN_STATE.NONE;
    }

    public uint NextGiftUpdate()
    {
        GAIN_STATE curState = CurGiftState();
        switch (curState)
        {
            case GAIN_STATE.FULL:
                return 0;
            case GAIN_STATE.NONE:
                {
                    if (giftFullUpdate == 0)
                        return 0;

                    neco_level data = neco_level.GetNecoLevelData("GIFT", neco_data.Instance.GetGiftBasketLevel());
                    uint FullCount = (data.GetNecoLevelValue2() / data.GetNecoLevelValue1());
                    uint tick = data.GetNecoLevelTick();
                    return (giftFullUpdate - ((FullCount - 1) * tick)) - (uint)NecoCanvas.GetCurTime();
                }
            case GAIN_STATE.SOME:
                {
                    return giftFullUpdate - (uint)NecoCanvas.GetCurTime();
                }
        }

        return 0;
    }

    public GAIN_STATE CurFishState()
    {
        return curFishState;
    }

    public void SetFishNextUpdate(uint t)
    {
        fishNextUpdate = t;
    }

    public bool IsFishTrapNeedUpdate()
    {
        return CurFishState() != GAIN_STATE.FULL && fishNextUpdate < NecoCanvas.GetCurTime();
    }

    public uint GetFishNextUpdateRemain()
    {
        if (fishNextUpdate > 0)
        {
            if (fishNextUpdate < NecoCanvas.GetCurTime())
            {
                neco_level info = neco_level.GetNecoLevelData("FISH", GetFishtrapLevel());
                while (fishNextUpdate < NecoCanvas.GetCurTime())
                {
                    fishNextUpdate += info.GetNecoLevelTick();
                }
            }

            if (fishNextUpdate > NecoCanvas.GetCurTime())
                return fishNextUpdate - (uint)NecoCanvas.GetCurTime();
        }

        return 0;
    }

    public void SetFishState(uint state)
    {
        curFishState = (GAIN_STATE)state;

        if (ui != null)
        {
            ui.RefreshFishButton();
        }

        if(curFishState == GAIN_STATE.FULL)
            UINotifiedManager.UpdateData("trapfull");
    }

    public void SetNextObjectUpdate(uint tick)
    {
        nextUpdate = tick;
    }

    public uint GetNextObjectUpdate()
    {
        return nextUpdate;
    }

    public uint GetCookRecipeLevel()
    {
        return cookLevel;
    }

    public void SetCookRecipeLevel(uint level)
    {
        cookLevel = level;
    }

    public uint GetCraftRecipeLevel()
    {
        return craftLevel;
    }

    public void SetCraftRecipeLevel(uint level)
    {
        craftLevel = level;
    }

    public uint GetGiftBasketLevel()
    {
        return basketLevel;
    }

    public void SetGiftBasketLevel(uint level)
    {
        basketLevel = level;
    }
    
    public uint GetFishfarmLevel()
    {
        return fishfarmLevel;
    }

    public void SetFishfarmLevel(uint level)
    {
        fishfarmLevel = level;
    }

    public uint GetFishtrapLevel()
    {
        return fishtrapLevel;
    }

    public void SetFishtrapLevel(uint level)
    {
        fishtrapLevel = level;
    }

    public void UpdatePlant(JObject plant)
    {
        JToken value;
        if(plant.TryGetValue("id", out value))
        {
            uint id = value.Value<uint>();
            uint level = plant["lvl"].Value<uint>();
            switch ((SUPPLY_UI_TYPE)id)
            {
                case SUPPLY_UI_TYPE.FISH_FARM:
                    fishfarmLevel = level;
                    SetGoldFullTick(plant["full"].Value<uint>());
                    UINotifiedManager.UpdateData("farmfull" + plant["full"].Value<uint>().ToString());
                    
                    if (plant.ContainsKey("boost"))
                    {
                        BOOST_TYPE type = BOOST_TYPE.NONE;
                        if (plant.ContainsKey("btype"))
                        {
                            if (plant["btype"].Value<uint>() == 1)
                                type = BOOST_TYPE.CATNIP_BOOST;
                            else
                                type = BOOST_TYPE.AD_BOOST;
                        }
                        SetFarmBoostTime(plant["boost"].Value<uint>(), type);
                    }
                    else
                        SetFarmBoostTime(0, BOOST_TYPE.NONE);

                    break;
                case SUPPLY_UI_TYPE.FISH_TRAP:
                    fishtrapLevel = level;
                    SetFishState(plant["state"].Value<uint>());
                    SetFishNextUpdate(plant["next"].Value<uint>());
                    
                    if (plant.ContainsKey("boost"))
                    {
                        BOOST_TYPE type = BOOST_TYPE.NONE;
                        if (plant.ContainsKey("btype"))
                        {
                            if (plant["btype"].Value<uint>() == 1)
                                type = BOOST_TYPE.CATNIP_BOOST;
                            else
                                type = BOOST_TYPE.AD_BOOST;
                        }
                        SetTrapBoostTime(plant["boost"].Value<uint>(), type);
                    }
                    else
                        SetTrapBoostTime(0, BOOST_TYPE.NONE);
                    break;

                case SUPPLY_UI_TYPE.CAT_GIFT:
                    basketLevel = level;
                    SetGiftFullTick(plant["full"].Value<uint>());
                    UINotifiedManager.UpdateData("giftfull" + plant["full"].Value<uint>().ToString());

                    if (plant.ContainsKey("boost"))
                    {
                        BOOST_TYPE type = BOOST_TYPE.NONE;
                        if(plant.ContainsKey("btype"))
                        {
                            if (plant["btype"].Value<uint>() == 1)
                                type = BOOST_TYPE.CATNIP_BOOST;
                            else
                                type = BOOST_TYPE.AD_BOOST;
                        }
                        SetGiftBoostTime(plant["boost"].Value<uint>(), type);
                    }
                    else
                        SetGiftBoostTime(0, BOOST_TYPE.NONE);
                    break;

                //case SUPPLY_UI_TYPE.WORKBENCH:
                //case SUPPLY_UI_TYPE.COUNTERTOP:
            }

        }
    }

    public void SetNewPostTimestamp(uint timestamp)
    {
        newPostTimestamp = timestamp;
    }

    public uint GetNewPostTimestamp()
    {
        return newPostTimestamp;
    }

    public void SetOpenPostTimestamp(uint timestamp)
    {
        openPostTimestamp = timestamp;
    }

    public uint GetOpenPostTimestamp()
    {
        return openPostTimestamp;
    }

    public void InitReadyCat(uint readyCats)
    {
        readyCatList.Clear();
        if (readyCats > 0)
        {
            for (uint i = 1; i < 13; i++)
            {
                int bitCat = 1 << (int)i;
                if ((readyCats & bitCat) > 0)
                {
                    if (neco_user_cat.GetUserCatInfo(i) == null)
                        readyCatList.Add(i);
                }
            }
        }
    }

    public List<uint> GetReadyCatList()
    {
        List<uint> ret = new List<uint>();
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_USER_CAT);
        if (necoData != null)
        {            
            foreach (neco_user_cat uc in necoData)
            {
                if(uc != null)
                {
                    if(uc.GetState() == 1)
                    {
                        ret.Add(uc.GetCatID());
                    }
                }
            }
        }

        return ret;
        //return readyCatList;
    }

    public List<uint> GetNewCatList()
    {
        List<uint> ret = new List<uint>();
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_USER_CAT);
        if (necoData != null)
        {
            foreach (neco_user_cat uc in necoData)
            {
                if (uc != null)
                {
                    if (uc.GetState() == 2)
                    {
                        ret.Add(uc.GetCatID());
                    }
                }
            }
        }

        return ret;
        //return readyCatList;
    }

    public bool IsCatReady(uint catID)
    {
        return readyCatList.Contains(catID);
    }

    public void ReadyCat(uint catID)
    {
        if (!readyCatList.Contains(catID))
        {
            readyCatList.Add(catID);
        }

        uint readyCats = 0;
        foreach(uint cat in readyCatList)
        {
            int bitCat = 1 << (int)cat;
            readyCats |= (uint)bitCat;
        }

        WWWForm data = new WWWForm();
        data.AddField("api", "user");
        data.AddField("val", readyCats.ToString());

        NetworkManager.GetInstance().SendApiRequest("user", 6, data, (response) => {

        });
    }

    public uint FirstAppearCatMapID(uint catID)
    {
        switch (catID) 
        {
            case 1:
                // 어미냥
                return 1;
            case 2:
                // 길막이
                return 1;
            case 3:
                // 삼색
                return 2;
            case 4:
                // 나옹
                return 10;
            case 5:
                // 야통
                return 3;
            case 6:
                // 연님
                return 4;
            case 7:
                // 카사노바
                return 10;
            case 8:
                // 뚱땅
                return 10;
            case 9:
                // 마를린
                return 1;
            case 10:
                // 도도
                return 5;
            case 11:
                // 조
                return 4;
            case 12:
                // 무
                return 4;
            case 13:
                // 래기
                return 6;
            case 14:
                // 빈집
                return 3;
            case 15:
                // 얼렁
                return 10;
            case 16:
                //작통
                return 9;
            case 17:
                //카오스
                return 4;
            case 18:
                //흰둥이들
                return 3;
            case 19:
                //얼룩이들
                return 5;

            default:
                return 0;
        }
    }

    public void CatAppear(uint catID)
    {
        readyCatList.Remove(catID);
    }

    public neco_pass_data GetPassData()
    {
        return passData;
    }

    public neco_market_data GetMarketData()
    {
        return marketData;
    }

    public void SetGiftBoostTime(uint time, BOOST_TYPE type) { giftBooster = time; giftBoostType = type; }
    public uint GetGiftBoostTime() { return giftBooster; }
    public BOOST_TYPE GetGiftBoostType() { return giftBoostType; }
    public void SetFarmBoostTime(uint time, BOOST_TYPE type) { farmBooster = time; farmBoostType = type; }
    public uint GetFarmBoostTime() { return farmBooster; }
    public BOOST_TYPE GetFarmBoostType() { return farmBoostType; }
    public void SetTrapBoostTime(uint time, BOOST_TYPE type) { trapBooster = time; trapBoostType = type; }
    public uint GetTrapBoostTime() { return trapBooster; }
    public BOOST_TYPE GetTrapBoostType() { return trapBoostType; }

    public void SetPurchaseHistory(JObject data)
    {
        purchaseHistory.Clear();

        foreach (JProperty property in data.Properties())
        {
            uint id = uint.Parse(property.Name);
            uint count = property.Value.Value<uint>();
            purchaseHistory[id] = count;
        }
    }

    public uint GetPurchaseCount(uint id)
    {
        if(purchaseHistory.ContainsKey(id))
            return purchaseHistory[id];
        
        return 0;
    }

    public void SetTimeSale(JObject data)
    {
        timeSaleRemain.Clear();

        foreach (JProperty property in data.Properties())
        {
            uint id = uint.Parse(property.Name);
            uint time = property.Value.Value<uint>();
            timeSaleRemain[id] = time;
        }
    }

    public void UpdateTimeSale(uint id, uint time)
    {
        timeSaleRemain[id] = time;
    }

    public uint GetTimeSaleProductRemain(uint id)
    {
        if(timeSaleRemain.ContainsKey(id))
            return timeSaleRemain[id];

        return 0;
    }

    public uint GetEventSaleProductRemain(uint id)
    {
        switch(id)
        {
            case 40:
            {
                string val = game_config.GetConfig("chuseok_package_time");
                if (string.IsNullOrEmpty(val))
                    return 0;

                JObject jobject = JObject.Parse(val);
                if(jobject.ContainsKey("start") && jobject.ContainsKey("end"))
                {
                    string start = jobject["start"].Value<string>();
                    DateTime startTime = Convert.ToDateTime(start);
                    string end = jobject["end"].Value<string>();
                    DateTime endTime = Convert.ToDateTime(end);

                    DateTime curTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    curTime = curTime.AddSeconds(NecoCanvas.GetCurTime()).ToLocalTime();
                        
                    if (startTime <= curTime && endTime >= curTime)
                    {
                        TimeSpan diff = (endTime - curTime);
                        return (uint)diff.TotalSeconds;
                    }
                }
                break;
            }

            case 53:
            {
                neco_event eventData = null;

                foreach(neco_event evt in neco_data.instance.GetEvents())
                {
                    if(evt.GetEventID() == (uint)neco_event.EVENT_TYPE.HALLOWEEN)
                    {
                        eventData = evt;
                    }
                }

                if(eventData != null)
                {
                    DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(eventData.GetEventStartTime()).ToLocalTime();
                    DateTime endTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(eventData.GetEventEndTime()).ToLocalTime();

                    DateTime curTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    curTime = curTime.AddSeconds(NecoCanvas.GetCurTime()).ToLocalTime();

                    if (startTime <= curTime && endTime >= curTime)
                    {
                        TimeSpan diff = (endTime - curTime);
                        return (uint)diff.TotalSeconds;
                    }
                }
                break;
            }

            default: break;
        }

        return 0;
    }

    public void SetBenefit(bool enable)
    {
        isFirstBuyBenefitEnable = enable;
    }

    public bool IsBenefitEnable()
    {
        return isFirstBuyBenefitEnable;
    }

    public DateTime GetFishtruckDateTime(bool isStartTime)
    {
        DateTime curTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(NecoCanvas.GetCurTime()).ToLocalTime();
        DateTime fishtruckStartTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        DateTime fishtruckEndTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        string value = game_config.GetConfig("fishtruck_time");
        if (!string.IsNullOrEmpty(value))
        {
            JObject json = JObject.Parse(value);
            if (json != null)
            {
                if (json.ContainsKey("start"))
                {
                    if (json["start"].Type == JTokenType.Array)
                    {
                        JArray array = (JArray)json["start"];
                        foreach (JToken val in array)
                        {
                            string startTime = val.Value<string>();
                            DateTime configStartTime = DateTime.Parse(startTime);
                            fishtruckStartTime = new DateTime(curTime.Year, curTime.Month, curTime.Day, configStartTime.Hour, configStartTime.Minute, configStartTime.Second);
                        }
                    }

                    if (json["end"].Type == JTokenType.Array)
                    {
                        JArray array = (JArray)json["end"];
                        foreach (JToken val in array)
                        {
                            string endTime = val.Value<string>();
                            DateTime configEndTime = DateTime.Parse(endTime);
                            fishtruckEndTime = new DateTime(curTime.Year, curTime.Month, curTime.Day, configEndTime.Hour, configEndTime.Minute, configEndTime.Second);
                        }
                    }
                }
            }
        }

        return isStartTime ? fishtruckStartTime : fishtruckEndTime;
    }

    public void SetPhotoADCount(uint count) { PhotoAD = count; }
    public uint GetPhotoADCount() { return PhotoAD; }
    public uint GetEnalbeCoinTime()
    {
        return enableCatObjectCoin;
    }

    public void SetEnableCoinTime(uint time)
    {
        enableCatObjectCoin = time;
    }

    public void ClearSubscribe()
    {
        subscribes.Clear();
    }

    public void SetSubscribe(JObject subsInfo)
    {
        neco_subscribe_data sub = new neco_subscribe_data();
        sub.prod_id = subsInfo["prod"].Value<uint>();
        sub.cur_day = subsInfo["idx"].Value<uint>();
        sub.max_day = subsInfo["tot"].Value<uint>();
        sub.enable_recive = subsInfo["today"].Value<uint>() > 0;
        sub.next_day_time = subsInfo["exp"].Value<uint>();

        subscribes.Add(sub);
    }

    public void UpdateSubscribe(JObject subsInfo)
    {
        uint id = subsInfo["prod"].Value<uint>();
        foreach(neco_subscribe_data sub in subscribes)
        {
            if(sub.prod_id == id)
            {
                sub.prod_id = subsInfo["prod"].Value<uint>();
                sub.cur_day = subsInfo["idx"].Value<uint>();
                sub.max_day = subsInfo["tot"].Value<uint>();
                sub.enable_recive = subsInfo["today"].Value<uint>() > 0;
                sub.next_day_time = subsInfo["exp"].Value<uint>();
                return;
            }
        }

        SetSubscribe(subsInfo);
    }

    public List<neco_subscribe_data> GetSubscribe()
    {
        bool needRefresh = false;
        List<neco_subscribe_data> del = new List<neco_subscribe_data>();
            
        foreach (neco_subscribe_data sub in subscribes)
        {
            if(sub.next_day_time <= NecoCanvas.GetCurTime())
            {
                if (sub.cur_day < sub.max_day)
                {
                    needRefresh = true;
                }
                else
                {
                    del.Add(sub);
                }
            }
        }
        
        foreach (neco_subscribe_data sub in del)
        {
            subscribes.Remove(sub);
        }

        if(needRefresh)
        {
            WWWForm data = new WWWForm();
            data.AddField("api", "iap");
            data.AddField("op", 6);

            NetworkManager.GetInstance().SendApiRequest("iap", 6, data);
        }

        return subscribes;
    }

    public List<neco_event> GetEvents()
    {
        return events;
    }

    public void SetEventData(JArray eventArray)
    {
        events.Clear();
        foreach (JObject val in eventArray)
        {
            neco_event evt = neco_event.CreateEvent(val);
            if (evt == null)
                continue;

            events.Add(evt);
        }
    }

    public fishing_data FishingData
    {
        get { return fishingData; }
    }
};


public class neco_pass_data
{
    bool passAlarm = false;    
    bool dailyAlarm = false;
    bool seasonAlarm = false;

    public bool IsPassAlarm() { return passAlarm; }
    public bool IsDailyAlarm() { return dailyAlarm; }
    public bool IsSeasonAlarm() { return seasonAlarm; }
    public void SetPassAlarm(bool alarm) { 
        passAlarm = alarm;
        CheckAlarm();
    }
    public void SetDailyAlarm(bool alarm) { 
        dailyAlarm = alarm;
        CheckAlarm();
    }
    public void SetSeasonAlarm(bool alarm) 
    { 
        seasonAlarm = alarm;
        CheckAlarm();
    }

    void CheckAlarm()
    {
        NecoCanvas.GetUICanvas()?.OnPassAlram();
    }

    uint curMission = 0;
    uint curDonation = 0;
    uint curPassStep = 0;
    uint totalExp = 0;
    uint curLevel = 0;

    List<uint> rewarded = new List<uint>();
    List<neco_mission_data> missionData = new List<neco_mission_data>();

    public void UpdatePassData(JObject data)
    {
        // 패스 단계가 오를 경우 갱신
        if (curPassStep > 0 && data["step"].Value<uint>() > curPassStep)
        {
            SetPassAlarm(true);
        }

        curMission = data["season"].Value<uint>();
        curDonation = data["donation"].Value<uint>();
        curPassStep = data["step"].Value<uint>();
        totalExp = data["exp"].Value<uint>();
        curLevel = data["level"].Value<uint>();

        rewarded.Clear();
        JArray rewardArray = (JArray)data["rewarded"];        
        foreach (JToken val in rewardArray)
        {
            rewarded.Add(val.Value<uint>());
        }
    }

    public void UpdatePassRewarded(JObject data)
    {
        rewarded.Clear();
        JArray rewardArray = (JArray)data["rewarded"];
        foreach (JToken val in rewardArray)
        {
            rewarded.Add(val.Value<uint>());
        }

        // 레드닷 관련 갱신
        if (rewarded.Count == GetCurLevel())    // 현재 레벨 단계 보상까지 수령한 경우
        {
            SetPassAlarm(rewarded.Contains(0));
        }
        else if (rewarded.Count == GetCurLevel() - 1)   // 현재 레벨 단계 보상은 미수령 상태인 경우
        {
            SetPassAlarm(true);
        }
    }

    public void UpdatePassExp(JObject data)
    {
        totalExp = data["exp"].Value<uint>();
        if (data.ContainsKey("level"))
        {
            curLevel = data["level"].Value<uint>();
            SetPassAlarm(true); // 패스 레벨이 오를 경우 갱신
        }
    }

    public uint GetCurMissionID()
    {
        return curMission;
    }

    public uint GetCurDonation()
    {
        return curDonation;
    }

    public uint GetCurPassStep()
    {
        return curPassStep;
    }

    public uint GetCurExp()
    {
        uint curExp = totalExp;
        if (curLevel > 1)
        {
            neco_pass_reward_forever curRewardData = neco_pass_reward_forever.GetNecoPassReward(curLevel - 1);
            curExp -= curRewardData.GetNecoPassRewardExp();
        }

        return curExp;
    }

    public uint GetTotalExp()
    {
        return totalExp;
    }

    public uint GetCurLevel()
    {
        return curLevel;
    }

    public uint GetRewardStatus(uint level)
    {
        if(rewarded.Count > level - 1)
            return rewarded[(int)level - 1];

        return 0;
    }



    public void UpdateMissionData(JObject data)
    {
        JArray missions = (JArray)data["missions"];
        foreach(JObject val in missions)
        {
            uint id = val["id"].Value<uint>();
            uint state = val["state"].Value<uint>();
            uint cur = 0;
            if(val.ContainsKey("cur"))
                cur = val["cur"].Value<uint>();

            while (missionData.Count <= id)
            {
                neco_mission_data nd = new neco_mission_data((uint)(missionData.Count - 1));
                missionData.Add(nd);
            }

            missionData[(int)id].UpdateData(id, state, cur);
        }

        uint dailyMissionCount = 0;
        uint seasonMissionCount = 0;
        foreach (neco_mission_data mission in missionData)
        {
            if (mission == null) { continue; }

            neco_mission missionInfo = neco_mission.GetNecoMissionData(mission.GetID());

            if (missionInfo == null) { continue; }

            if (missionInfo.GetMissionType() == neco_mission.MISSION_TYPE.DAILY_MISSION)
            {
                if (mission.GetState() == 2)
                {
                    dailyMissionCount++;
                }
            }
            else if (missionInfo.GetMissionType() == neco_mission.MISSION_TYPE.SEASON_MISSON)
            {
                if (mission.GetState() == 2)
                {
                    seasonMissionCount++;
                }
            }
        }

        SetDailyAlarm(dailyMissionCount > 0);
        SetSeasonAlarm(seasonMissionCount > 0);
    }

    public neco_mission_data GetMissionData(uint i)
    {
        while (missionData.Count <= i)
        {
            neco_mission_data nd = new neco_mission_data((uint)(missionData.Count - 1));
            missionData.Add(nd);
        }

        return missionData[(int)i];
    }
    
};

public class neco_mission_data
{
    uint id = 0;
    uint state = 0;
    uint cur = 0;

    public neco_mission_data(uint i)
    {
        id = i;
    }

    public void UpdateData(uint i, uint s, uint c = 0)
    {
        if (/*state != 2 && */s == 2)   // 현재는 보상 받을게 있을 경우 처리
        {
            List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_MISSION);
            
            object obj;
            foreach (neco_mission missionData in necoData)
            {
                if(missionData.GetNecoMissionID() == i)
                {
                    switch(missionData.GetMissionType())
                    {
                        case neco_mission.MISSION_TYPE.DAILY_MISSION:
                            neco_data.Instance.GetPassData().SetDailyAlarm(true);
                            break;
                        case neco_mission.MISSION_TYPE.SEASON_MISSON:
                            neco_data.Instance.GetPassData().SetSeasonAlarm(true);
                            break;
                    }
                }
            }
        }

        id = i;
        state = s;
        cur = c;
    }

    public uint GetID() { return id; }
    public uint GetState() { return state; }
    public uint GetCurValue() { return cur; }
}

public class neco_market_data
{
    public Dictionary<uint, uint> saleFish = new Dictionary<uint, uint>();
    public uint fishRefresh;
    public Dictionary<uint, uint> saleHardware = new Dictionary<uint, uint>();
    public uint hardwareRefresh;

    public uint fishADCount;
    public uint hardwareADCount;
}

public class neco_subscribe_data
{
    public uint prod_id;
    public uint cur_day;
    public uint max_day;
    public bool enable_recive;
    public uint next_day_time;
}

public class fishing_data
{
    public uint Baits = 0;

    public bool IsNewBaits()
    {
        return Baits > 0;
    }
}