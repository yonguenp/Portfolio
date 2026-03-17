using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class PopupData
    {
    }
    public class TabTypePopupData : PopupData
    {
        public int TabIndex { get; private set; } = -1;
        public int SubIndex { get; private set; } = -1;
        public TabTypePopupData(int tab, int sub)
        {
            TabIndex = tab;
            SubIndex = sub;
        }
    }
    public class StagePopupData : PopupData
    {
        public int World { get; private set; } = -1;
        public int Stage { get; private set; } = -1;
        public int Diff { get; private set; } = -1;
        public StagePopupData(int world, int stage, int diff = -1)
        {
            World = world;
            Stage = stage;
            Diff = diff;
        }
    }
    public class BuildingPopupData : PopupData
    {
		public enum eBuilding
		{

		}

        public int BuildingTag { get; private set; } = -1;

        public BuildInfo BuildInfo { get { return buildInfo; } }
        public BuildingOpenData OpenData { get { return oepnData; } }
        public BuildingBaseData BaseData { get { return baseData; } }
        public BuildingLevelData LevelData { get { return levelData; } }

        public IProductFormData Product { get { return productData; } }
        public IProductFormData NextProduct { get { return nextProductData; } }//업그레이드 팝업에서씀
        public ProducesBuilding ProduceBuilding { get { return produceBuilding; } }

        public int Level { get { return BuildInfo == null ? 0 : BuildInfo.Level; } }
        public string BuildingKey { get { return BaseData == null ? "" : BaseData.KEY; } }
        public Vector2Int TargetCell { get; private set; } = Vector2Int.zero;

        private BuildInfo buildInfo = null;
        private BuildingOpenData oepnData = null;
        private BuildingBaseData baseData = null;
        private BuildingLevelData levelData = null;

        private IProductFormData productData = null;
        private IProductFormData nextProductData = null;

        private ProducesBuilding produceBuilding = null;
        
        public BuildingPopupData(int tag)
        {
            BuildingTag = tag;
            TargetCell = Vector2Int.zero;
            DataReload();
        }

        public BuildingPopupData(BuildInfo info)
        {
            BuildingTag = info.Tag;
            TargetCell = Vector2Int.zero;
            DataReload();
        }

        public BuildingPopupData(ConstructInfoData constructInfo)
        {
            TargetCell = Vector2Int.zero;
            baseData = constructInfo.buildingBase;
            oepnData = BuildingOpenData.GetAvailTotalBuilding(BaseData.KEY.ToString());
            levelData = BuildingLevelData.GetDataByGroupAndLevel(BaseData.KEY.ToString(), 0);
        }

        public BuildingPopupData(ConstructInfoData constructInfo, Vector2Int target)
        {
            TargetCell = target;
            baseData = constructInfo.buildingBase;
            oepnData = BuildingOpenData.GetAvailTotalBuilding(BaseData.KEY.ToString());
            levelData = BuildingLevelData.GetDataByGroupAndLevel(BaseData.KEY.ToString(), 0);
        }

        private void DataReload()
        {
            if (BuildingTag > 0)
            {
                buildInfo = User.Instance.GetUserBuildingInfoByTag(BuildingTag);
                oepnData = BuildingOpenData.GetWithTag(BuildingTag);
            }

            if (oepnData != null)
            {
                baseData = oepnData.BaseData;
            }

            if (!string.IsNullOrEmpty(BuildingKey))
            {
                levelData = BuildingLevelData.GetDataByGroupAndLevel(BuildingKey, Level);
                produceBuilding = User.Instance.GetProduces(BuildingTag);

                if (baseData != null)
                {
                    switch (baseData.TYPE)
                    {
                        case 2://battery
                        {
                            productData = ProductAutoData.GetProductDataByGropuAndLevel(BuildingKey, Level);
                            nextProductData = ProductAutoData.GetProductDataByGropuAndLevel(BuildingKey, Level + 1);
                        }
                        break;
                        default:
                        {
                            productData = ProductData.GetProductDataByGroupAndLevel(BuildingKey, Level);
                            nextProductData = ProductData.GetProductDataByGroupAndLevel(BuildingKey, Level + 1);
                        }
                        break;
                    }
                }
            }
        }
    }

    public class BuildingConstructListData : PopupData
    {
        public Vector2Int TargetCell { get; private set; } = Vector2Int.zero;
        public List<ConstructInfoData> ConstructList { get; private set; } = new List<ConstructInfoData>();

        public BuildingConstructListData(List<ConstructInfoData> _list)
        {
            ConstructList = _list;
        }

        public BuildingConstructListData()
        {
            TargetCell = Vector2Int.zero;
            SetCurrentBuildingList();
        }
        public BuildingConstructListData(Vector2Int target)
        {
            TargetCell = target;
            SetCurrentBuildingList();
        }

        void SetCurrentBuildingList()
        {
            if (ConstructList == null)
                ConstructList = new List<ConstructInfoData>();
            ConstructList.Clear();

            List<BuildingBaseData> BuildingList = BuildingBaseData.GetProductBuildingList();
            List<ConstructInfoData> constructInfoList = new List<ConstructInfoData>();
            constructInfoList.Clear();

            foreach (BuildingBaseData buildingData in BuildingList)
            {
                if (buildingData == null)
                {
                    continue;
                }

                constructInfoList.Add(new ConstructInfoData(buildingData.KEY));
            }
            
            ConstructList = constructInfoList.FindAll(element => element.openData != null && element.openData.OPEN_LEVEL <= User.Instance.TownInfo.AreaLevel).ToList();
        }

        public bool IsAvailableOpen()//건설 가능한 건물이 있는지
        {
            return ConstructList.Count > 0;
        }
    }

    public class RewardPopupData : PopupData
    {
        public List<Asset> Rewards { get; private set; } = new List<Asset>();
        public RewardPopupData(List<Asset> rewardItems)
        {
            Rewards = new List<Asset>(rewardItems);
        }

        public RewardPopupData(List<List<int>> rewards)
        {
            foreach (var value in rewards)
            {
                Rewards.Add(new Asset(value[1], value[2], value[0]));
            }
        }
    }
    public class PartPopupData : PopupData
    {
        public List<Asset> Rewards { get; private set; } = new List<Asset>();
        public PartPopupData(List<Asset> rewards)
        {
            foreach (var value in rewards)
            {
                Rewards.Add(value);
            }
        }
    }
    public class PetPopupData : PopupData
    {
        public bool IsNew { get; private set; } = false;
        public bool IsSuccess { get; private set; } = false;
        public int PetTag { get; private set; } = 0;
        public PetBaseData PetData { get; private set; } = null;

        public PetPopupData(bool isNew, bool isSuccess, int tag, PetBaseData data)
        {
            IsNew = isNew;
            IsSuccess = isSuccess;
            PetTag = tag;
            PetData = data;
        }
    }

    public class DragonPopupData : PopupData
    {
        public int DragonID { get; private set; } = -1;

        public CharBaseData DragonCharBaseData { get { return dragonCharBaseData; } }

        private CharBaseData dragonCharBaseData = null;

        public DragonPopupData(int dragonID)
        {
            DragonID = dragonID;
            DataReload();
        }

        private void DataReload()
        {
            dragonCharBaseData = CharBaseData.Get(DragonID);
        }
    }

    public class ArenaResultPopupData : PopupData
    {
        public List<ArenaResultDragonStat> MyDragonData { get; private set; } = new List<ArenaResultDragonStat>();
        public List<ArenaResultDragonStat> EnemyDragonData { get; private set; } = new List<ArenaResultDragonStat>();

        public int MyDragonCount { get { return MyDragonData.Count; } }
        public int EnemyDragonCount { get { return EnemyDragonData.Count; } }

        public int MyAtk { get; private set; } = 0;
        public int MyDef { get; private set; } = 0;
        public int EnemyAtk { get; private set; } = 0;
        public int EnemyDef { get; private set; } = 0;

        public ArenaResultPopupData(List<ArenaResultDragonStat> stats)
        {
            foreach (var charData in stats)
            {
                int dmg = charData.Damage;
                int givenDmg = charData.TakenDamage;
                if (charData.BTag < 'a')
                {
                    MyDragonData.Add(charData);
                    MyAtk += dmg;
                    MyDef += givenDmg;
                }
                else
                {
                    EnemyDragonData.Add(charData);
                    EnemyAtk += dmg;
                    EnemyDef += givenDmg;
                }
            }
        }
    }
    // AccelerationMainPopup
    public class AccelerationMainData : PopupData
    {
        public eAccelerationType accelerateType = eAccelerationType.NONE;
        public int accMainTag = 0;
        public int accMainTime = 0;
        public int accMainEndTime = 0;

        // 지하철 관련
        public int platform = 0;

        // 생산 관련
        public int frameIndex = -1;
        public bool isFull = false;

        public VoidDelegate timeCompleteAction = null;
        public VoidDelegate timeReductAction = null;

        public AccelerationMainData(eAccelerationType type, int tag, int time, int endTime, VoidDelegate _timeCompleteAction = null)
        {
            accelerateType = type;

            accMainTag = tag;
            accMainTime = time;
            accMainEndTime = endTime;

            timeCompleteAction = _timeCompleteAction;
            isFull = false;
        }

        public AccelerationMainData(eAccelerationType type, int tag, int time, int endTime, int frame = -1, VoidDelegate _timeCompleteAction = null, VoidDelegate _timeReduceAction = null, bool _isFull = false)
        {
            accelerateType = type;

            accMainTag = tag;
            accMainTime = time;
            accMainEndTime = endTime;

            frameIndex = frame;
            timeCompleteAction = _timeCompleteAction;
            timeReductAction = _timeReduceAction;
            isFull = _isFull;
        }

        public void Clear()
        {
            accelerateType = eAccelerationType.NONE;

            accMainTag = 0;
            accMainTime = 0;
            accMainEndTime = 0;

            timeCompleteAction = null;
            isFull = false;
        }
    }

    public class QuestPopupData : PopupData
    {
        public Quest questData;

        public QuestPopupData(int qid)
        {
            questData = QuestManager.Instance.GetQuest(qid);
        }
        public QuestPopupData(Quest qData)
        {
            questData = qData;
        }
    }

    public class BuildingUpGradeStateData : PopupData
    {
        public bool isUpGrade { get; private set; } = false;

        public BuildingUpGradeStateData(bool isUp)
        {
            isUpGrade = isUp;
        }
    }
    public class FilterPopupData : PopupData //여기 말고도 다른곳에서도 쓸것 같음
    {
        public eGradeFilter gradeFilter;
        public eTypeFilter typeFilter;
        public eReinforceLevelFilter levelFilter;
        public eElementFilter elementFilter;
        public eJobFilter jobFilter;
        public eJoinedContentFilter formationFilter;

        public void Init()
        {
            gradeFilter = eGradeFilter.ALL;
            typeFilter = eTypeFilter.ALL;
            levelFilter = eReinforceLevelFilter.ALL;
            elementFilter = eElementFilter.ALL;
            jobFilter = eJobFilter.ALL;
            formationFilter = eJoinedContentFilter.ALL;
        }

        public void SetFilter(FilterPopupData data)
        {
            gradeFilter = data.gradeFilter;
            typeFilter = data.typeFilter;
            levelFilter = data.levelFilter;
            elementFilter = data.elementFilter;
            jobFilter = data.jobFilter;
            formationFilter = data.formationFilter;
        }
        public void SetFilter(int grade, int type, int level)
        {
            gradeFilter = (eGradeFilter)grade;
            typeFilter = (eTypeFilter)type;
            levelFilter = (eReinforceLevelFilter)level;
        }
        public void SetFilter(eGradeFilter grade, eTypeFilter type, eReinforceLevelFilter level)
        {
            gradeFilter = grade;
            typeFilter = type;
            levelFilter = level;
        }
        public void SetFilter(eGradeFilter grade, eElementFilter element, eJobFilter job, eJoinedContentFilter formation)
        {
            gradeFilter = grade;
            elementFilter = element;
            jobFilter = job;
            formationFilter = formation;
        }
        public void SetFilter(eElementFilter element, eJobFilter job, eJoinedContentFilter formation)
        {
            elementFilter = element;
            jobFilter = job;
            formationFilter = formation;
        }
    }

    public class PetFilterData : PopupData
    {
        public eGradeFilter gradeFilter;
        public eElementFilter elementFilter;
        public ePetStatFilter statFilter;
        public ePetOptionFilter optionFilter;
        public bool isShowOnlyLockState;

        public void Init()
        {
            gradeFilter = eGradeFilter.ALL;
            elementFilter = eElementFilter.ALL;
            statFilter = ePetStatFilter.ALL;
            optionFilter = ePetOptionFilter.ALL;

            isShowOnlyLockState = false;
        }

        public void SetFilter(PetFilterData data)
        {
            gradeFilter = data.gradeFilter;
            elementFilter = data.elementFilter;
            statFilter = data.statFilter;
            optionFilter = data.optionFilter;

            isShowOnlyLockState = data.isShowOnlyLockState;
        }
        public void SetFilter(eGradeFilter grade, eElementFilter element, ePetStatFilter stat, ePetOptionFilter option, bool isShowOnlyLockState)
        {
            gradeFilter = grade;
            elementFilter = element;
            statFilter = stat;
            optionFilter = option;

            this.isShowOnlyLockState = isShowOnlyLockState;
        }
    }

    public class GuildReccomendFilterData : PopupData
    {
        public eGuildRecommendFilter filter { get; private set; }
        public void Init()
        {
            filter = eGuildRecommendFilter.All;
        }
        public void SetFilter(eGuildRecommendFilter data)
        {
            filter = data;
        }
        public GuildReccomendFilterData(eGuildRecommendFilter _filter)
        {
            filter = _filter;
        }
    }

    public class GuildRankRewardPopupData : PopupData
    {
        public eGuildRankRewardGroup group { get; private set; }

        public GuildRankRewardPopupData(eGuildRankRewardGroup _group)
        {
            group = _group;
        }
    }

    public class DragonLevelPopupData : PopupData
    {
        public int CurrentDragonLevel { get; private set; } = 0;
        public int NextDragonLevel { get; private set; } = 0;
        public CharacterStatus PrevDragonStat { get; private set; } = new CharacterStatus();

        public DragonLevelPopupData(int curLv, int nextLv, CharacterStatus stat)
        {
            CurrentDragonLevel = curLv;
            NextDragonLevel = nextLv;
            PrevDragonStat = stat;
        }
    }
    public class ItemInfoPopupData : PopupData
    {
        public ItemFrame Frame { get; private set; } = null;
        public ItemInfoPopupData(ItemFrame item)
        {
            Frame = item;
        }
    }

    public class PetLevelPopupData : PopupData
    {
        public int tag { get; private set; } = 0;
        public int currentLevel { get; private set; } = 0;
        public int nextLevel { get; private set; } = 0;

        public int reinforce { get; private set; } = 0;

        public PetLevelPopupData(int tag, int curLv, int nextLv, int Reinforce)
        {
            this.tag = tag;
            currentLevel = curLv;
            nextLevel = nextLv;
            reinforce = Reinforce;
        }
    }
    public class PricePopupData : PopupData
    {
        public string TitleStr { get; private set; } = "";
        public string SubTitleStr { get; private set; } = "";
        public string ContentStr { get; private set; } = "";
        public int Price { get; private set; } = -1;
        public ePriceDataFlag Flag { get; private set; } = ePriceDataFlag.None;
        public VoidDelegate BtnDelegate { get; private set; } = null;

        public PricePopupData(int titleStrNo, int subTitleStrNo, int contentStrNo, int price, ePriceDataFlag flag, VoidDelegate btnDelegate)
        {
            TitleStr = StringData.GetStringByIndex(titleStrNo);
            SubTitleStr = StringData.GetStringByIndex(subTitleStrNo);
            ContentStr = StringData.GetStringByIndex(contentStrNo);
            Price = price;
            Flag = flag;
            BtnDelegate = btnDelegate;
        }

        public PricePopupData(string titleStr, string subTitleStr, string contentStr, int price, ePriceDataFlag flag, VoidDelegate btnDelegate)
        {
            TitleStr = titleStr;
            SubTitleStr = subTitleStr;
            ContentStr = contentStr;
            Price = price;
            Flag = flag;
            BtnDelegate = btnDelegate;
        }
    }
    public class TooltipPopupData : PopupData
    {
        public ToolTipData TipData { get; private set; } = null;
        public TooltipPopupData(ToolTipData data)
        {
            TipData = data;
        }
    }
    public class SimulatorDragonPopupData : PopupData
    {
        public string SimulatorDragonTag { get; private set; } = "";
        public int SimulatorDragonIndex { get; private set; } = 0;

        public SimulatorDragonPopupData(string tag, int idx = 0)
        {
            SimulatorDragonTag = tag;
            SimulatorDragonIndex = idx;
        }
    }

    public class ChattingPopupData : PopupData
    {
        public ProfileUserData UserData { get; private set; } = null;
        public ChattingPopupData (ProfileUserData dataInfo)
        {
            UserData = dataInfo;
        }
    }

    public class ChattingMacroData : PopupData
    {
        public string macroString { get; private set; } = "";
        public int macroIndex { get; private set; } = 0;

        public ChattingMacroData(string contents, int idx = 0)
        {
            macroString = contents;
            macroIndex = idx;
        }
    }

    public class ProductManageProduceOptionData : PopupData
    {
        public ProductManagePopup parentPopup { get; private set; } = null;

        public ProductManageProduceOptionData(ProductManagePopup parent)
        {
            parentPopup = parent;
        }
    }
    public class ArenaRankChangePopupData : PopupData
    {
        public int prevGrade { get; private set; } = -1;
        public int currentGrade { get; private set; } = -1;

        public ArenaRankChangePopupData(int currentGrade, int prevGrade)//현재 등급 , 경기 이전 등급
        {
            this.currentGrade = currentGrade;
            this.prevGrade = prevGrade;
        }
    }

    public class StorePopupData : PopupData
    {
        public string storeTitleText { get; private set; } = string.Empty;

        public eStoreType storeType { get; private set; } = eStoreType.NONE;
        public eGoodType goodType { get; private set; } = eGoodType.NONE;

        public StorePopupData(string storeTitle ,eStoreType eStoreType, eGoodType eGoodType)
        {
            storeTitleText = storeTitle;
            goodType = eGoodType;
            storeType = eStoreType;
        }
    }

    public class MainShopPopupData : PopupData
    {
        public int menuNum { get; private set; } = -1;

        public MainShopPopupData()
        {

        }
        public MainShopPopupData(int menu)
        {            
            menuNum = menu;
        }
    }

    public class ShopPopupData : PopupData
    {
        
    }

    public class ShopBuyPopupData : PopupData
    {
        public ShopGoodsData ShopGoodsData { get; private set; } = null;
        public bool IsShowGoodsCount { get; private set; } =true; // 상품 묶음의 갯수를 보여줄지 안보여줄지에 관한 데이터
        public ShopBuyPopupData(ShopGoodsData data)
        {
            ShopGoodsData = data;
        }
        public ShopBuyPopupData(ShopGoodsData data, bool isShowGoodsCount)
        {
            ShopGoodsData = data;
            IsShowGoodsCount = isShowGoodsCount;
        }
        //public ShopBuyPopupData(GuildGoodsData data)
        //{
        //    GuildGoodsData = data;
        //}
    }
    public class ConditionBuyData : PopupData
    {
        public int Key = 0;
        public ConditionBuyData( int key)
        {
            Key = key;
        }
    }
    public class StatisticPopupData : PopupData
    {
        public bool IsArena;
        public StatisticPopupData(bool isArena)
        {
            IsArena = isArena;
        }
    }

    public class PetDetailInfoPopupData : PopupData
    {
        public UserPet leftPetData { get; private set; } = null; 
        public UserPet rightPetData { get; private set; } = null; 

        public PetDetailInfoPopupData(UserPet leftData, UserPet rightData)
        {
            leftPetData = leftData;
            rightPetData = rightData;
        }
    }

    public class GemDungeonBoosterPopupData : PopupData
    {
        public int Floor { get; private set; } = 0;

        public GemDungeonBoosterPopupData(int floor)
        {
            Floor = floor;
        }
    }

    public class GemDungeonHealPopupData : PopupData
    {
        public int dragonNo { get; private set; } = 0;
        public GemDungeonHealPopupData(int dragonNo)
        {
            this.dragonNo = dragonNo;
        }
    }

    public class ItemMakePopupData : PopupData
    {
        public string titleStr { get; private set; } = "";
        public int reciepeID { get; private set; } = 0;
        public bool oneByone { get; private set; } = false;
        public ItemMakePopupData(int reciepeID,string titleStr,bool onebyone = false)
        {
            this.titleStr = titleStr;
            this.reciepeID = reciepeID;
            this.oneByone = onebyone;
        }
    }

    public class PassiveSkillPopupData : PopupData
    {
        public int dragonNo { get; private set; } = 0;
        public int maxSkillSlot { get; private set; } = 0;

        public PassiveSkillPopupData(int dragonNo, int maxSkillSlot)
        {
            this.dragonNo = dragonNo;
            this.maxSkillSlot = maxSkillSlot;
        }
    }

    public class PassiveSkillResultPopupData : PopupData
    {
        public int dragonNo { get; private set; } = 0;
        public int PassiveKey { get; private set; } = 0;
        public ePassiveRefreshType ePassiveRefreshType { get; private set; } = ePassiveRefreshType.NONE;
        public PassiveSkillResultPopupData(int dragonNo, int passiveKey, ePassiveRefreshType type)
        {
            this.dragonNo = dragonNo;
            this.PassiveKey = passiveKey;
            ePassiveRefreshType = type;
        }
    }
    public class TranscendenceResultPopupData : PopupData
    {
        public bool Success { get; private set; } = false;
        public UserDragon Dragon { get; private set; } = null;
        public TranscendenceResultPopupData(bool success, UserDragon dragon)
        {
            Success = success;
            Dragon = dragon;
        }
    }

    public class ArenaStatisticPopupData : PopupData
    {
        public bool IsMyWin { get; private set; } = false;
        public int TotalBattleTime { get; private set; } = 0;

        public ArenaStatisticPopupData(bool isMyWin, int totalBattleTime)
        {
            IsMyWin = isMyWin;
            TotalBattleTime = totalBattleTime;
        }
    }
    public class AdventureStatisticPopupData : PopupData
    {
        public bool IsMyWin { get; private set; } = false;
        public float TotalBattleTime { get; private set; } = 0;

        public string Stage { get; private set; } = "";

        public AdventureStatisticPopupData(bool isMyWin, float totalBattleTime, string stage)
        {
            IsMyWin = isMyWin;
            TotalBattleTime = totalBattleTime;
            Stage = stage;
        }
    }

    public class ChampionBattleStatisticPopupData : PopupData
    {
        public ChampionBattleBattleData BattleData { get; private set; } = null;
        public Action CloseCB { get; private set; } = null;
        public ChampionBattleStatisticPopupData(ChampionBattleBattleData battleData, Action cb)
        {
            BattleData = battleData;
            CloseCB = cb;
        }
    }

    public class WebViewPopupData : PopupData
    {
        public string Url { get; private set; } = string.Empty;

        public WebViewPopupData(string url)
        {
            Url = url;
        }
    }

    #region Mining & Miner Popup

    public class MiningMainPopupData : PopupData
    {

    }

    public class MineBoostItemUsePopupData : PopupData
    {
        public MineBoosterItem boostItem { get; private set; } = null;
        public MineBoostItemUsePopupData(MineBoosterItem item)
        {
            boostItem = item;
        }
    }

    #endregion

}