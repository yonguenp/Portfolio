using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;



public class PracticeDragon : ChampionDragon
{
    public ChampionPracticeBattleLine Parent { get; private set; } = null;
    public PracticeDragon(int tag, ChampionPracticeBattleLine line)
        : base(tag, null)
    {
        Parent = line;
        var TeamSide = Parent.TeamSIde;
        var prevData = JObject.Parse(CacheUserData.GetString("ChampionPracticeDeck_" + TeamSide, "{}"));
        if (prevData.ContainsKey("Dragon"))
        {
            JObject dragons = (JObject)prevData["Dragon"];
            if (dragons.ContainsKey(Tag.ToString()))
            {
                InitData(Tag, (JObject)dragons[Tag.ToString()]);
            }
        }
    }

    public JObject MakeJsonObject(bool random = false)
    {
        JObject param = new JObject();

        System.Random rnd = null;
        if (random)
            rnd = new System.Random();

        param.Add("dragon_id", Tag);

        JObject pet = new JObject();
        int petID = 0;
        if (ChampionPet != null)
        {
            petID = ChampionPet.ID;
        }
        else if (rnd != null)
        {
            var pets = ChampionManager.GetSelectablePets();
            petID = pets[rnd.Next(pets.Count)].KEY;
        }

        pet.Add("pet_id", petID);

        JArray stats = new JArray();
        for (int i = 0; i < 4; i++)
        {
            stats.Add(0);
            if (ChampionPet != null && ChampionPet.Stats.Count > i)
            {
                stats[i] = ChampionPet.Stats[i].Key;
            }
            else if (rnd != null)
            {
                var petstats = ChampionManager.GetSelectablePetStatsList();
                stats[i] = petstats[rnd.Next(petstats.Count)].KEY;
            }
        }
        pet.Add("pet_stats", stats);


        JArray subs = new JArray();
        for (int i = 0; i < 4; i++)
        {
            subs.Add(0);
            if (ChampionPet != null && ChampionPet.SubOptionList.Count > i)
            {
                subs[i] = ChampionPet.SubOptionList[i].Key;
            }
            else if (rnd != null)
            {
                var petoptions = ChampionManager.GetSelectableSubOptions(PetBaseData.Get(petID), i);
                subs[i] = petoptions[rnd.Next(petoptions.Count)].KEY;
            }
        }
        pet.Add("pet_subs", subs);
        param.Add("pet", pet);

        JArray parts = new JArray();
        for (int i = 0; i < 6; i++)
        {
            var val = new JObject();
            JArray fusion = new JArray();
            if (ChampionPart != null && ChampionPart.ContainsKey(i) && ChampionPart[i] != null)
            {
                val.Add("equip_id", ChampionPart[i].ID);

                JArray sub = new JArray();
                for (int j = 0; j < 4; j++)
                {
                    sub.Add(0);
                    if (ChampionPart[i].SubOptionList.Count > j)
                    {
                        sub[j] = ChampionPart[i].SubOptionList[j].Key;
                    }
                    else if (rnd != null)
                    {
                        var petoptions = ChampionManager.GetSelectableSubOptions(ChampionPart[i].GetPartDesignData(), j);
                        sub[j] = petoptions[rnd.Next(petoptions.Count)].KEY;
                    }
                }
                val.Add("equip_subs", sub);
                fusion.Add(ChampionPart[i].FusionStatKey);
            }
            else if (rnd != null)
            {
                var partlist = ChampionManager.GetSelectableParts();
                var part = partlist[rnd.Next(partlist.Count)];
                val.Add("equip_id", part.KEY);

                JArray sub = new JArray();
                for (int j = 0; j < 4; j++)
                {
                    sub.Add(0);
                    var petoptions = ChampionManager.GetSelectableSubOptions(part, j);
                    sub[j] = petoptions[rnd.Next(petoptions.Count)].KEY;
                }

                val.Add("equip_subs", sub);

                
                var fusionList = ChampionManager.GetSelectableFusionOptions();
                fusion.Add(fusionList[rnd.Next(fusionList.Count)].KEY);
            }

            val.Add("equip_fusions", fusion);
            parts.Add(val);
        }


        param.Add("equips", parts);


        JArray passive = new JArray();
        for (int i = 0; i < 2; i++)
        {
            passive.Add(0);
            if (PassiveSkills.Count > i && PassiveSkills[i] > 0)
            {
                passive[i] = PassiveSkills[i];
            }
            else if (rnd != null)
            {
                var passivelist = ChampionManager.GetSelectablePassive(BaseData.JOB, i);
                passive[i] = passivelist[rnd.Next(passivelist.Count)].KEY;
            }
        }

        param.Add("passive_skill", passive);

        return param;
    }

    public override void ReqSaveDragon(NetworkManager.SuccessCallback cb = null, bool random = false)
    {
        InitData(Tag, MakeJsonObject(random));

        Parent.SaveDeck(cb);        
    }

    public void Clone(ChampionDragon championDragon)
    {
        ChampionPet = championDragon.ChampionPet;
        ChampionPart = championDragon.ChampionPart;
        TranscendenceData.SetPassiveData(championDragon.PassiveSkills);
        
        RefreshALLStatus();
    }
}
public class ChampionPracticeBattleLine : BattleLine
{
    public enum TeamType
    {
        LEFT = 0,
        RIGHT = 1,
    }

    protected override int MaxDeckCount => 5;
    protected override int HiddenCount => 1;
    protected override int XSize => 3;
    protected override int YSize => 2;

    public TeamType TeamSIde { get; private set; } = TeamType.LEFT;

    Dictionary<int, PracticeDragon> DragonInfo = new Dictionary<int, PracticeDragon>();

    public PracticeDragon GetPracticeDragon(int tag)
    {
        if(!DragonInfo.ContainsKey(tag))
            DragonInfo.Add(tag, new PracticeDragon(tag, this));

        return DragonInfo[tag];
    }

    public void SaveDeck(NetworkManager.SuccessCallback cb = null)
    {
        var saveData = new JObject();
        JArray deck = JArray.FromObject(Dragons);
        saveData.Add("Deck", deck);
        JObject dragons = new JObject();
        foreach(var dragon in DragonInfo)
        {
            dragons.Add(dragon.Key.ToString(), dragon.Value.MakeJsonObject());
        }
        saveData.Add("Dragon", dragons);

        CacheUserData.SetString("ChampionPracticeDeck_" + TeamSIde, saveData.ToString());

        cb?.Invoke(new JObject());
    }
    public override bool LoadBattleLine(int index = 0)
    {
        TeamSIde = (TeamType)index;
        var prevData = JObject.Parse(CacheUserData.GetString("ChampionPracticeDeck_" + TeamSIde, "{}"));
        if(prevData != null)
        {
            if (prevData.ContainsKey("Deck"))
            {
                List<int> deck = new List<int>();
                foreach(var val in (JArray)prevData["Deck"])
                {
                    deck.Add(val.Value<int>());
                }

                SetLine(deck);
            }

            if (prevData.ContainsKey("Dragon"))
            {
                JObject dragons = (JObject)prevData["Dragon"];
                foreach (var val in dragons.Properties())
                {
                    int no = int.Parse(val.Name);
                    var dragon = new PracticeDragon(no, this);
                    if (!DragonInfo.ContainsKey(no))
                        DragonInfo.Add(no, dragon);
                    else
                        DragonInfo[no] = dragon;
                }
            }
        }

        return false;
    }
    public int GetTotalINF()
    {
        int ret = 0;

        for (int i = 0, count = Dragons.Length; i < count; i++)
        {
            var id = Dragons[i];
            if (id < 1)
                continue;

            if (!DragonInfo.ContainsKey(id))
                continue;

            var dragon = DragonInfo[id];
            if (dragon == null)
                continue;

            dragon.RefreshALLStatus();
            ret += dragon.GetTotalINF();
        }

        return ret;
    }


    public override string GetJsonString()
    {
        JArray deck = new JArray();

        for (int x = 0; x < XSize; x++)
        {
            for (int y = 0; y < YSize; y++)
            {
                int index = x * YSize + y;
                JObject dragonInfo = null;
                if(Dragons != null && Dragons.Length > index)
                {
                    int tag = Dragons[index];
                    if (DragonInfo.ContainsKey(tag))
                    {
                        dragonInfo = DragonInfo[tag].MakeJsonObject();
                    }
                }

                deck.Add(dragonInfo);
            }
        }

        return deck.ToString();
    }
}

public struct PracticeDragonChangedEvent
{
    private static PracticeDragonChangedEvent e = default;

    public enum TYPE
    {
        REFRESH,
    }
    public TYPE type;

    public static void Refresh()
    {
        e.type = TYPE.REFRESH;
        EventManager.TriggerEvent(e);
    }
}
public class ChampionPracticeMode : MonoBehaviour, EventListener<PracticeDragonChangedEvent>
{

    [SerializeField]
    Text PracticePriceText = null;

    [Serializable]
    public class DeckUI
    {
        [SerializeField]
        GameObject Root;
        [SerializeField]
        public ChampionPracticeDragonListView ListScroll;
        [SerializeField]
        public Text TeamPoint;
        [SerializeField]
        public ChampionBattleCharacterSlotFrame[] Slots;

        public ChampionPracticeBattleLine BattleLine { get; private set; } = null;
        public ChampionPracticeBattleLine.TeamType TeamType { get; private set; } = ChampionPracticeBattleLine.TeamType.LEFT;

        int CurSelectedDragonTag = -1;

        public bool IsListShow()
        {
            if(ListScroll != null)
                return ListScroll.IsShowList();

            return false;
        }

        public void ShowList(ChampionPracticeMode p, ChampionPracticeBattleLine b, ChampionBattleDragonListView.ClickCallBack regist_cb, ChampionBattleDragonListView.ClickCallBack release_cb)
        {
            ListScroll.OnShow(p, b, regist_cb, release_cb);

            foreach(var slot in Slots)
            {
                slot.EnableDrag = true;
            }
        }

        public void HideList()
        {
            ListScroll.OnClose();

            foreach (var slot in Slots)
            {
                slot.EnableDrag = false;
            }
        }
        
        public void Init(ChampionPracticeBattleLine.TeamType type)
        {
            TeamType = type;
            BattleLine = new ChampionPracticeBattleLine();
            BattleLine.LoadBattleLine((int)type);

            ListScroll.RefreshDragonCountLabel(BattleLine.DeckCount, 5);

            HideList();

            SetActive(true);
        }
        public void SetActive(bool enable)
        {
            Root.SetActive(enable);
        }
        
        public void RefreshUI()
        {
            for (int i = 0; i < 6; i++)
            {
                var slot = Slots[i];
                int dragonTag = BattleLine.GetDragon(i);
                ChampionDragon element = dragonTag > 0 ? BattleLine.GetPracticeDragon(dragonTag) : null;
                if (element != null)
                {
                    if(!BattleLine.IsContainDragon(element.Tag))
                    {
                        element = null;
                    }
                }

                if (element != null)
                {
                    element.RefreshALLStatus();

                    slot.SetDragonData(element, BattleLine);
                    slot.setCallback((param) =>
                    {
                        if (CurSelectedDragonTag < 0) return;
                        if (CurSelectedDragonTag > 0 && int.Parse(param) > 0)
                        {
                            OnClickChangeTeam(int.Parse(param), CurSelectedDragonTag);
                            HideAllArrowAnimation(); 
                            return;
                        }
                        bool isHideArrow = slot.isHideArrow();
                        if (!isHideArrow) return;
                        bool isDeckFull = BattleLine.IsDeckFull();
                        if (isDeckFull)
                        {
                            OnClickReleaseTeam(int.Parse(param));
                            return;
                        }
                        OnClickRegistTeam(CurSelectedDragonTag);
                        CurSelectedDragonTag = -1;
                    });
                }
                else
                {
                    slot.setEmptyData(i / 2, i % 2);
                    slot.setCallback((param) => {
                        OnClickRegistPosition(slot.Line, slot.Index);
                        CurSelectedDragonTag = -1;
                    });
                }
            }

            TeamPoint.text = BattleLine.GetTotalINF().ToString();
        }

        public void OnClickRegistTeam(int tag)
        {
            if (CurSelectedDragonTag > 0)
            {
                HideAllArrowAnimation();
            }

            CurSelectedDragonTag = tag;
            if (BattleLine.IsDeckFull())
            {
                var line = BattleLine.GetList();
                for (int i = 0; i < Slots.Length; ++i)
                {
                    if (line[i] > 0)
                    {
                        Slots[i].ShowAnimArrowNode(false);
                    }
                }
            }
            else
            {
                for (int i = 0; i < Slots.Length; ++i)
                {
                    Slots[i].ShowAnimArrowNode(false);
                }
            }
        }

        void OnClickRegistPosition(int line, int index)
        {
            if (BattleLine.GetDragon(line, index) <= 0 && CurSelectedDragonTag <= 0)
            {
                return;
            }
            BattleLine.AddDragonPosition(line, index, CurSelectedDragonTag);
            RefreshUI();
            ListScroll.RefreshList(GetPracticeDragons().Select(data => data.Tag).ToArray());
            ListScroll.RefreshDragonCountLabel(BattleLine.DeckCount, 5);

            HideAllArrowAnimation();
            
            BattleLine.SaveDeck();
        }

        public void OnClickReleaseTeam(int dragonTag)
        {
            bool isVisible = ListScroll.IsShowList();
            if (!isVisible) 
                return;
            BattleLine.DeleteDragon(dragonTag);
            RefreshUI();
            ListScroll.RefreshList(GetPracticeDragons().Select(data => data.Tag).ToArray());

            HideAllArrowAnimation();

            BattleLine.SaveDeck();
        }

        void OnClickChangeTeam(int BeforeDTag, int AfterDTag)
        {
            BattleLine.ChangeDragon(BeforeDTag, AfterDTag);
            RefreshUI();
            HideAllArrowAnimation();
            ListScroll.RefreshList(GetPracticeDragons().Select(data => data.Tag).ToArray());

            BattleLine.SaveDeck();
        }

        public void HideAllArrowAnimation()
        {
            CurSelectedDragonTag = -1;

            foreach (ChampionBattleCharacterSlotFrame elem in Slots)
            {
                if (elem == null) return;
                elem.HideAnimArrowNode();
            }
        }

        public List<ChampionDragon> GetPracticeDragons()
        {
            List<ChampionDragon> ret = new List<ChampionDragon>();
            foreach (var dragon in ChampionManager.GetSelectableDragons())
            {
                ret.Add(BattleLine.GetPracticeDragon(dragon.KEY));
            }

            return ret;
        }
    }

    [Header("드래곤 좌")]
    [SerializeField]
    DeckUI LeftDeckLine;
    [Header("드래곤 우")]
    [SerializeField]
    DeckUI RightDeckLine;

    bool count_dirty = true;

    private void OnEnable()
    {
        EventManager.AddListener<PracticeDragonChangedEvent>(this);
        Init();
    }
    private void OnDIsable()
    {
        EventManager.RemoveListener<PracticeDragonChangedEvent>(this);
    }
    public void Init()
    {
        LeftDeckLine.Init(ChampionPracticeBattleLine.TeamType.LEFT);
        RightDeckLine.Init(ChampionPracticeBattleLine.TeamType.RIGHT);

        OnClickListClose();

        Refresh();
    }
    public void Refresh()
    {
        LeftDeckLine.RefreshUI();
        RightDeckLine.RefreshUI();

        PracticePriceText.text = ((ChampionManager.Instance.MyInfo.PracticeCount * GameConfigTable.PRACTICE_PRICE_VALUE) + GameConfigTable.PRACTICE_PRICE_VALUE).ToString();

        if (count_dirty || ChampionManager.Instance.MyInfo.PracticeCount < 0)
        {
            WWWForm param = new WWWForm();
            param.AddField("season_id", ChampionManager.Instance.CurChampionInfo.CurSeason);
            param.AddField("round", (int)ChampionManager.Instance.CurChampionInfo.CurState);

            NetworkManager.Send("unifiedtournament/practicecount", param, (jsonData) =>
            {
                if (!SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                    return;

                switch ((eApiResCode)jsonData["rs"].Value<int>())
                {
                    case eApiResCode.OK:
                    {
                        if (jsonData.Type == JTokenType.Object)
                        {
                            ChampionManager.Instance.MyInfo.SetPracticeCount(jsonData["count"].Value<int>());
                            PracticePriceText.text = ((ChampionManager.Instance.MyInfo.PracticeCount * GameConfigTable.PRACTICE_PRICE_VALUE) + GameConfigTable.PRACTICE_PRICE_VALUE).ToString();
                        }
                    }
                    break;
                }

                count_dirty = false;
            });
        }
    }

    public void OnClickLeft()
    {
        if (LeftDeckLine.ListScroll.IsShowList())
            return;
        if (RightDeckLine.ListScroll.IsShowList())
            return;

        LeftDeckLine.ShowList(this, LeftDeckLine.BattleLine, OnClickRegistTeam, LeftDeckLine.OnClickReleaseTeam);
    }

    public void OnClickRight()
    {
        if (LeftDeckLine.ListScroll.IsShowList())
            return;
        if (RightDeckLine.ListScroll.IsShowList())
            return;

        RightDeckLine.ShowList(this, RightDeckLine.BattleLine, OnClickRegistTeam, RightDeckLine.OnClickReleaseTeam);
    }

    public void OnClickListClose()
    {
        LeftDeckLine.HideAllArrowAnimation();
        LeftDeckLine.HideList();

        RightDeckLine.HideAllArrowAnimation();
        RightDeckLine.HideList();
    }
    void OnClickRegistTeam(int dragonTag)
    {
        DeckUI deck = null;
        if (LeftDeckLine.ListScroll.IsShowList())
            deck = LeftDeckLine;
        if (RightDeckLine.ListScroll.IsShowList())
            deck = RightDeckLine;
        if (deck == null) 
            return;

        deck.OnClickRegistTeam(dragonTag);
    }

    public void OnClickGameStart()
    {
        PricePopup.OpenPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("연습모드안내문구1"), StringData.GetStringByStrKey("연습모드안내문구2"), ((ChampionManager.Instance.MyInfo.PracticeCount * GameConfigTable.PRACTICE_PRICE_VALUE) + GameConfigTable.PRACTICE_PRICE_VALUE), ePriceDataFlag.GemStone | ePriceDataFlag.ContentBG | ePriceDataFlag.SubTitleLayer | ePriceDataFlag.CancelBtn, () =>
        {
            var data = new WWWForm();
            var json1 = LeftDeckLine.BattleLine.GetJsonString();
            data.AddField("off", json1);
            var json2 = RightDeckLine.BattleLine.GetJsonString();
            data.AddField("def", json2);

            data.AddField("season_id", ChampionManager.Instance.CurChampionInfo.CurSeason);
            data.AddField("round", (int)ChampionManager.Instance.CurChampionInfo.CurState);

            NetworkManager.Send("unifiedtournament/practicefight", data, (JObject jsonData) =>
            {
                if (SBFunc.IsJTokenType(jsonData["err"], JTokenType.Integer) && (eApiResCode)jsonData["err"].Value<int>() == eApiResCode.OK && (eApiResCode)jsonData["rs"].Value<int>() == eApiResCode.OK)
                {
                    ChampionManager.Instance.OnPracticeStart((JArray)jsonData["off"], (JArray)jsonData["def"]);
                    LoadingManager.Instance.EffectiveSceneLoad("ChampionPracticeColosseum", eSceneEffectType.CloudAnimation);
                }
                else
                {
                    Debug.LogError(jsonData.ToString());
                }
            });

            count_dirty = true;
        });
    }

    public List<ChampionDragon> GetPracticeDragons(ChampionPracticeBattleLine.TeamType type)
    {
        DeckUI deck = null;
        if (type == ChampionPracticeBattleLine.TeamType.LEFT)
            deck = LeftDeckLine;
        else
            deck = RightDeckLine;

        if(deck != null)
        {
            return deck.GetPracticeDragons();
        }

        return null;
    }

    public void OnEvent(PracticeDragonChangedEvent eventType)
    {

        if (LeftDeckLine.IsListShow()) LeftDeckLine.RefreshUI();

        if (RightDeckLine.IsListShow()) RightDeckLine.RefreshUI();
        
    }
}
