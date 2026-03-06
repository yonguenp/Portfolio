using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using static SandboxNetwork.SBFunc;
using System.Linq;
using System.IO;
using SQLite4Unity3d;

namespace SandboxNetwork
{
    public class User
    {
        //EnterPlayModeOptions에서 static class 는 직접 초기화를 시켜야함.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitPlayMode()
        {
            if (instance != null)
            {
                instance = null;
            }
        }

        private static User instance = null;
        public static User Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new User();
                }
                return instance;
            }
        }

        public UserAccountData UserAccountData { get; private set; } = new UserAccountData();
        public UserData UserData { get; private set; } = new UserData();
        public int GOLD { get { return UserData.Gold; } }
        public int ENERGY { get { return UserData.Energy; } }
        public int GEMSTONE { get { return UserData.Gemstone + UserData.CashGemstone; } }
        public int ORACLE { get { return (GameConfigTable.WEB3_MENU_OPEN_ON_KOREAN || ENABLE_P2E) ? UserData.Oracle : 0; } }
        public PrefData PrefData { get; private set; } = new PrefData();
        public TownExteriorData ExteriorData { get; private set; } = new TownExteriorData();
        public UserDragonData DragonData { get; private set; } = new UserDragonData();
        public UserPartData PartData { get; private set; } = new UserPartData();
        public UserPetData PetData { get; private set; } = new UserPetData();
        public UserDragonCardList DragonCards { get; protected set; } = new UserDragonCardList();
        public TownInfo TownInfo { get; private set; } = new TownInfo();
        public ExchangeManager Exchange { get; private set; } = new ExchangeManager();
        public AttendanceData Attendance { get; private set; } = new AttendanceData();

        public EventDataManager EventData { get; private set; } = new EventDataManager();




        protected Dictionary<int, ProducesBuilding> Produces { get; private set; } = new Dictionary<int, ProducesBuilding>();

        public Inventory Inventory { get; protected set; } = new Inventory();
        protected Dictionary<int, BuildInfo> buildings = new Dictionary<int, BuildInfo>();

        private Dictionary<Type, Landmark> landmark_data = new Dictionary<Type, Landmark>();

        public bool ENABLE_P2E
        {
            get
            {
                if (GameConfigTable.IsRegistedVersion())
                    return (UserData.State & 1) > 0;
                else
                    return false;
            }
        }
        public bool REGISTED_WALLET { get { return ENABLE_P2E && (UserData.State & 2) > 0; } }
        public bool ENABLE_MINING { get { return REGISTED_WALLET && (UserData.State & 4) > 0; } }
        public bool IS_HOLDER { get { return REGISTED_WALLET && (UserData.State & 8) > 0; } }
        public bool ADVERTISEMENT_PASS { get { return (UserData.State & 16) > 0; } }
        public bool BATTLE_SPPED_BOOST { get { return (UserData.State & 32) > 0; } }


        public Lock Lock { get; private set; } = new Lock();

        public void RefreshDataToInfo()

        {
            TownInfo.RefreshData();
        }

        // 로그아웃등에 사용

        public void ClearUserData(bool accountClear = false)
        {
            UserData?.Clear();
            Inventory?.Clear();
            buildings?.Clear();
            ExteriorData?.Clear();
            Produces?.Clear();
            landmark_data?.Clear();
            PartData?.ClearData();
            PetData?.ClearData();
            DragonData?.ClearData();
            PrefData?.Clear();
            DragonCards?.Clear();
            Exchange?.Clear();
            Attendance?.Clear();
            EventData?.Clear();
            Lock.Clear();
            ShopManager.Instance.Clear();
            BattlePassManager.Instance.ClearLevelPass();

            if (accountClear)
            {
                UserAccountData?.Clear(); // 우편api 확인 필요
            }
        }

        public void SetBase(JObject jsonData)
        {
            // 데이터 클리어
            ClearUserData();

            // 계정 관련 정보
            UserAccountData.Set(jsonData);

            // 유저 관련 정보
            if (IsJTokenCheck(jsonData["user_base"]))
            {
                UserData.Set((JObject)jsonData["user_base"]);
            }

            // 유저의 cypto 관련 상태 정보
            if (IsJTokenCheck(jsonData["user_state"]))
            {
                UserData.UpdateUserState(jsonData["user_state"].Value<int>());
            }

            //inventory
            if (IsJTokenCheck(jsonData["inven_step"]))
            {
                Inventory.SetStep(jsonData["inven_step"].Value<int>());
            }

            if (IsJTokenCheck(jsonData["items"]))
            {
                var jObjItems = (JArray)jsonData["items"];

                foreach (var element in jObjItems)
                {
                    var element_item = (JArray)element;
                    if (element_item.Count == 2)
                    {
                        UpdateItem(element_item[0].Value<int>(), element_item[1].Value<int>());
                    }
                }
            }

            if (IsJTokenCheck(jsonData["mine_items"]))//광산 전용 부스터 아이템 초기 구성
            {
                var jObjItems = (JArray)jsonData["mine_items"];
                foreach (var element in jObjItems)
                {
                    var element_item = (JArray)element;
                    if (element_item.Count == 3)
                    {
                        MiningManager.Instance.UpdateItem(element_item[0].Value<int>(), element_item[1].Value<int>(), element_item[2].Value<int>());
                    }
                    else
                        Debug.LogError("item size not 3");
                }
            }

            // 유저의 magnet 갯수 관련 상태 정보
            if (IsJTokenCheck(jsonData["my_magnet"]))
            {
                UserData.UpdateMagnet(jsonData["my_magnet"].Value<int>());
            }

            if (IsJTokenCheck(jsonData["my_magnite"]))
            {
                UserData.UpdateMagnite(jsonData["my_magnite"].Value<int>());
            }

            if (IsJTokenCheck(jsonData["my_champ_oracle"]))
            {
                UserData.UpdateOracle(jsonData["my_champ_oracle"].Value<int>());
            }

            if (IsJTokenCheck(jsonData["buildings"]))
            {
                JArray arrays = (JArray)jsonData["buildings"];

                var arrayCount = arrays.Count;
                if (arrayCount > 0)
                {
                    for (var i = 0; i < arrayCount; i++)
                    {
                        JObject jobjData = (JObject)arrays[i];

                        var tag = IsJTokenType(jobjData["tag"], JTokenType.Integer) ? jobjData["tag"].Value<int>() : -1;
                        var state = IsJTokenType(jobjData["state"], JTokenType.Integer) ? jobjData["state"].Value<int>() : -1;
                        var level = IsJTokenType(jobjData["level"], JTokenType.Integer) ? jobjData["level"].Value<int>() : 0;
                        var construct_exp = IsJTokenType(jobjData["construct_exp"], JTokenType.Integer) ? jobjData["construct_exp"].Value<int>() : -1;

                        if (tag > 0 && state > 0)
                        {
                            buildings.Add(tag, new BuildInfo(tag, level, (eBuildingState)state, construct_exp));
                            //Debug.Log(">>>>>>SetBase.buildings // Field tag :" + tag + "  state : " + state + " level : " + level + " construct_exp : " + construct_exp);
                        }
                    }
                }
            }

            if (ExteriorData != null)
            {
                ExteriorData.Set(jsonData);
                //SBLog(exterior_data);
            }

            if (jsonData.ContainsKey("produces"))
            {
                var jObjectList = (JArray)jsonData["produces"];

                foreach (var element in jObjectList)
                {
                    UpdateProduces((JObject)element);
                }
            }

            if (landmark_data == null)
                landmark_data = new();
            else
                landmark_data.Clear();

            landmark_data.Add(SBDefine.TYPE_DOZER, new LandmarkDozer());
            landmark_data.Add(SBDefine.TYPE_TRAVEL, new LandmarkTravel());
            landmark_data.Add(SBDefine.TYPE_SUBWAY, new LandmarkSubway());
            landmark_data.Add(SBDefine.TYPE_EXCHANGE, new LandmarkExchange());
            landmark_data.Add(SBDefine.TYPE_GEMDUNGEON, new LandmarkGemDungeon());
            landmark_data.Add(SBDefine.TYPE_MINE, new LandmarkMine());
            var travel = GetLandmarkData<LandmarkTravel>();
            if (travel != null)
                travel.Init();

            if (jsonData.ContainsKey("parts"))
            {
                var jObjectList = (JArray)jsonData["parts"];

                foreach (var element in jObjectList)
                {
                    PartData.AddUserPart((JObject)element);
                }
            }

            //서버와 데이터 구조 맞추기
            if (jsonData.ContainsKey("pets"))
            {
                var jObjectList = jsonData["pets"];

                foreach (var element in jObjectList)
                {
                    PetData.RefreshUserPet(element);
                }
            }

            if (jsonData.ContainsKey("dragons"))
            {
                var jObjectList = (JArray)jsonData["dragons"];

                foreach (var element in jObjectList)
                {
                    var data = (JObject)element;
                    if (data == null)
                        continue;

                    var tag = IsJTokenType(data["dragon_id"], JTokenType.Integer) ? data["dragon_id"].Value<int>() : -1;
                    if (tag > 0)
                    {
                        UserDragon newDragon = new UserDragon();
                        newDragon.SetJsonData(tag, data);
                        if (newDragon.BaseData == null)
                            continue;

                        DragonData.AddUserDragon(newDragon.Tag, newDragon);
                    }
                }
            }

            if (jsonData.ContainsKey("landmarks"))
            {
                UpdateLandmark((JArray)jsonData["landmarks"]);
            }

            //유저 기본 데이터
            if (jsonData.ContainsKey("pref"))
            {
                if (jsonData["pref"].HasValues)
                {
                    PrefData.Set((JObject)jsonData["pref"]);
                }
            }

            //월드 진행 데이터 - 일단 prefs에 진행 내역 넣어둠
            if (jsonData.ContainsKey("adventure"))
            {
                if (jsonData["adventure"].HasValues)
                {
                    StageManager.Instance.SetAdventureWorldProgress((JObject)jsonData["adventure"]);
                }
            }

            if (jsonData.ContainsKey("daily"))
            {
                var dailyData = jsonData["daily"];
                if (IsJObject(dailyData))
                {
                    var infoData = dailyData["daily_info"];
                    if (infoData != null)
                    {
                        StageManager.Instance.DailyDungeonProgressData.SetDailyInfoData((JObject)infoData);
                    }

                    var logData = dailyData["daily_log"];
                    if (IsJArray(logData))
                        StageManager.Instance.SetDailyDungeonProgress((JArray)logData);
                }
            }
            if (jsonData.ContainsKey("raid"))
            {
                var worldBossData = jsonData["raid"];
                if (IsJObject(worldBossData))//raid_info , raid_log
                {
                    WorldBossManager.Instance.WorldBossProgressData.SetRaidBossInfoData((JObject)worldBossData);
                    WorldBossManager.Instance.WorldBossProgressData.SetWorldBossLogData((JObject)worldBossData);
                }
            }

            if (jsonData.ContainsKey("tutorial"))
            {
                if (SBFunc.IsJObject(jsonData["tutorial"]))
                    TutorialManagement.SetTutorialData((JObject)jsonData["tutorial"]);
            }

            if (jsonData.ContainsKey("quest"))
            {
                //questEvent 확인
                QuestManager.Instance.UserDataSync((JObject)jsonData["quest"]);
            }

            if (IsJArray(jsonData["attendance"]))
            {
                Attendance?.SetData((JArray)jsonData["attendance"]);
            }
            //if (IsJArray(jsonData["event_attendance"]))
            //{
            //    EventAttendance?.SetData((JArray)jsonData["attendance"]);
            //}

            if (DragonCards != null)
            {
                DragonCards.Set(jsonData);
            }

            ShopManager.Instance.Clear();

            if (IsJTokenCheck(jsonData["purchase_history"]))
            {
                SetPurchased((JObject)jsonData["purchase_history"]);
            }

            if (IsJTokenCheck(jsonData["advertisement"]))
            {
                SetAdvertisement((JObject)jsonData["advertisement"]);
            }

            if (IsJTokenCheck(jsonData["personal_limit_active"]))
            {
                SetPrivateGoods((JObject)jsonData["personal_limit_active"]);
            }

            if (IsJTokenCheck(jsonData["subscribe"]))
            {
                SetSubscribeGoods((JToken)jsonData["subscribe"]);
            }

            if (jsonData.ContainsKey("collection"))
            {
                CollectionAchievementManager.Instance.UserDataSync((JObject)jsonData["collection"], eCollectionAchievementType.COLLECTION);
            }

            if (jsonData.ContainsKey("achievement"))
            {
                CollectionAchievementManager.Instance.UserDataSync((JObject)jsonData["achievement"], eCollectionAchievementType.ACHIEVEMENT);
            }

            if (jsonData.ContainsKey("magicshowcase"))
            {
                MagicShowcaseManager.Instance.UserDataSync((JObject)jsonData["magicshowcase"]);
            }

            //레벨 패스 데이터 세팅
            if (jsonData.ContainsKey("level_pass") && SBFunc.IsJTokenType(jsonData["level_pass"], JTokenType.Object))
            {
                BattlePassManager.Instance.SetLevelPassData((JObject)jsonData["level_pass"]);
            }

            //아레나 티켓 데이터 세팅
            if (jsonData.ContainsKey("arena_ticket") && SBFunc.IsJTokenType(jsonData["arena_ticket"], JTokenType.Integer))
            {
                ArenaManager.Instance.UserArenaData.SetArenaTicketCount(jsonData["arena_ticket"].Value<int>());
            }
            Exchange.Init();
        }

        public void UpdateItem(int itemNo, int itemCount)
        {
            Inventory.UpdateItem(itemNo, itemCount);
        }

        public void UpdateProduces(JObject obj)
        {
            if (obj.ContainsKey("tag") && obj.ContainsKey("slots"))
            {
                var tag = obj["tag"].Value<int>();
                var slot = obj["slots"].Value<int>();
                List<object> items = new List<object>();
                if (obj.ContainsKey("items"))
                {
                    var jobjItems = (JArray)obj["items"];
                    items = jobjItems.ToObject<List<object>>();
                }
                ProducesBuilding building = null;
                var recipes = new List<ProducesRecipe>();

                if (!Produces.ContainsKey(tag))
                {
                    building = new ProducesBuilding(tag, slot, recipes);
                    Produces[tag] = building;
                }
                else
                {
                    building = Produces[tag];
                    building.SetData(tag, slot, recipes);
                }

                if (1001 <= tag && tag <= 1010)
                {
                    if (items != null && items.Count > 0)
                    {
                        int completeSlotIndex = Convert.ToInt32(items[0]);
                        int productingSlotExp = Convert.ToInt32(items[1]);

                        if (completeSlotIndex > 0)
                        {
                            for (var i = 0; i < completeSlotIndex; i++)
                            {
                                var Item = new ProducesRecipe(completeSlotIndex, 0, eProducesState.Complete);
                                building.Items.Add(Item);
                            }
                        }

                        if (productingSlotExp > 0)
                        {
                            var Item = new ProducesRecipe(0, productingSlotExp, eProducesState.Ing);
                            building.Items.Add(Item);
                        }
                    }
                }
                else
                {
                    if (items != null)
                    {
                        var itemsCount = items.Count;

                        for (var i = 0; i < itemsCount; i++)
                        {
                            var data = items[i];
                            var parseData = (JArray)data;

                            int arrayCount = parseData.Count;
                            if (arrayCount < 2)
                            {
                                continue;
                            }
                            var itemNo = parseData[0].Value<int>();
                            var itemCount = parseData[1].Value<int>();
                            var itemTime = arrayCount > 2 ? parseData[2].Value<int>() : 0;

                            var Item = new ProducesRecipe(itemNo, itemTime, (eProducesState)itemCount);
                            building.Items.Add(Item);
                        }
                    }
                }
            }
        }

        public void UpdateBuilding(int tag, int state, int level, int? construct_exp = null)
        {
            var building = GetUserBuildingInfoByTag(tag);
            if (building != null)
                building.SetData(tag, level, (eBuildingState)state, construct_exp != null ? construct_exp.Value : -1);
            else
                buildings.Add(tag, new BuildInfo(tag, level, (eBuildingState)state, construct_exp != null ? construct_exp.Value : -1));

            if (buildings != null && buildings.Count > 3)//기본 랜드마크 제외
                UIManager.Instance.RefreshUI(eUIType.Town);//생산 버튼 갱신
        }

        public void UpdateGrid(JObject jsonData)
        {
            if (ExteriorData != null)
            {
                ExteriorData.Set(jsonData);

                //popupmanager 생성
                //PopupManager.ForceUpdate();
            }
        }

        public void PushUpdateLandmark(JObject jsonData)
        {
            if (jsonData.ContainsKey("data"))
            {
                UpdateLandmark((JArray)jsonData["data"]);
            }
        }
        private void UpdateLandmark(JArray arrays)
        {
            if (arrays == null)
                return;

            foreach (var element in arrays)
            {
                if (false == IsJObject(element) || false == IsJTokenType(element["tag"], JTokenType.Integer))
                    continue;

                var tag = (eLandmarkType)element["tag"].Value<int>();
                if (landmark_data.TryGetValue(GetLandmarkType(tag), out var data))
                {
                    data.SetData(element);
                }
            }
        }

        // 해당 재화가 충분한지 체크
        public bool IsSufficientCost(eGoodType goodsType, int targetAmount)
        {
            switch (goodsType)
            {
                case eGoodType.GOLD:
                    return GOLD >= targetAmount;
                case eGoodType.GEMSTONE:
                    return GEMSTONE >= targetAmount;
                case eGoodType.MAGNET:
                    return UserData.Magnet >= targetAmount;
                default:
                    return false;
            }
        }

        public InventoryItem GetItem(int itemID)
        {
            return Inventory.GetItem(itemID);
        }

        // 특정 아이템의 아이템 갯수만 반환
        public int GetItemCount(string itemID)
        {
            return GetItemCount(int.Parse(itemID));
        }

        public int GetItemCount(int itemID)
        {
            var resultItem = GetItem(itemID);

            return resultItem != null ? resultItem.Amount : 0;
        }

        // 각 컨텐츠별 인벤체크 데이터매니저 param을 기준으로 던전 입장 가능 여부 판단
        public bool CheckInventoryForContentsEnter(eInvenSlotCheckContentType contentType, int worldIndex, int stageIndex)
        {
            var currentSlotCount = Inventory.GetEmptySlotCount();
            var slotCheckCount = 0;

            switch (contentType)
            {
                case eInvenSlotCheckContentType.Adventure:
                {
                    var stageData = StageBaseData.GetByAdventureWorldStage(worldIndex, stageIndex);
                    if (stageData != null)
                    {
                        slotCheckCount = stageData.REWARD_ITEM_COUNT;
                    }
                }
                break;
                case eInvenSlotCheckContentType.Travel:
                {
                    var travelData = TravelData.GetByWorldID(worldIndex);
                    if (travelData != null)
                    {
                        slotCheckCount = travelData.REWARD_BONUS_NUM;
                    }
                }
                break;
                case eInvenSlotCheckContentType.DailyDungeon:
                {
                    var stageData = StageBaseData.GetByWorldStage(worldIndex, stageIndex);
                    if (stageData != null)
                    {
                        slotCheckCount = stageData.REWARD_ITEM_COUNT;
                    }
                }
                break;
                default: return false;
            }

            ///미사용으로 확인되어 제거.
            //int prevSlotCount = Inventory.GetPrevInvenSlotCount(contentType);
            //// 값이 있을 경우 인벤 상태 체크
            //if (prevSlotCount > 0)
            //{
            //    return (currentSlotCount > prevSlotCount) && (currentSlotCount >= slotCheckCount);
            //}
            // 값이 없으면 우편을 보낸적이 없으므로 인벤 슬롯 자체만 체크

            return currentSlotCount >= slotCheckCount;
        }

        /// <summary>
        /// 해당 아이템을 인벤토리로 획득할 수 있는지 여부 판단
        /// </summary>
        /// <returns> true 획득 불가, false 획득 가능 </returns>
        public bool CheckInventoryGetItem(List<Asset> itemArr) // [ [아이템 타입, 아이템 번호, 아이템 수] ] //itemArr : readonly[] | [] | any[]
        {
            if (Inventory == null)
                return true;

            return false == Inventory.CanItems(itemArr);
        }

        public ProducesBuilding GetProduces(int buildingTag)
        {
            if (Produces == null)
            {
                return null;
            }

            if (!Produces.ContainsKey(buildingTag))
            {
                return null;
            }

            return Produces[buildingTag];
        }

        public Dictionary<int, ProducesBuilding> GetAllProduces()
        {
            if (Produces == null)
            {
                return null;
            }

            return Produces;
        }

        public List<ProducesBuilding> GetAllProducesList(bool exceptAutoProduct = false)
        {
            if (Produces == null)
            {
                return null;
            }

            if (exceptAutoProduct)
            {
                List<ProducesBuilding> resultList = new List<ProducesBuilding>();

                foreach (ProducesBuilding building in Produces.Values)
                {
                    if (ProductData.IsProductBuilding(building.OpenData.BUILDING))
                    {
                        resultList.Add(building);
                    }
                }

                return resultList;
            }
            else
            {
                return new List<ProducesBuilding>(Produces.Values);
            }
        }

        //curData["building"] = new Dictionary<string, List<BuildingOpenData>>();
        public int GetBuildingCount(string buildingName)
        {
            return TownInfo.GetBuildingCount(buildingName);
        }

        public Vector4 GetMapData()
        {
            return new Vector4(0, TownInfo.FloorMin, TownInfo.Width, TownInfo.FloorMax);
        }

        public Vector2 GetOpenFloorData()
        {
            var floorData = new Vector2(TownInfo.AreaOpenMinLevel, TownInfo.AreaOpenMaxLevel);

            return floorData;
        }

        public Dictionary<int, Dictionary<int, int>> GetActiveGridData()
        {
            if (ExteriorData == null)
            {
                Dictionary<int, Dictionary<int, int>> tempDic = new Dictionary<int, Dictionary<int, int>>();

                Dictionary<int, int> tempObj = new Dictionary<int, int>();
                tempObj.Add(0, 0);
                tempObj.Add(1, 0);
                tempObj.Add(2, 0);



                tempDic.Add(-1, tempObj);
                tempDic.Add(0, tempObj);
                tempDic.Add(1, tempObj);

                return tempDic;
            }
            return ExteriorData.ActiveGrid;
        }

        public Dictionary<int, Dictionary<int, int>> GetGridData()
        {
            return ExteriorData.ExteriorGrid;
        }

        public List<int> GetCurProductBuildingTagList()
        {
            List<int> tempList = new List<int>();
            if (ExteriorData != null)
            {
                foreach (var floorGrid in ExteriorData.ActiveGrid.Values)
                {
                    foreach (var tag in floorGrid.Values)
                    {
                        if (tag > 1000)
                        {
                            BuildInfo info = GetUserBuildingInfoByTag(tag);
                            if (info != null)
                            {
                                if (info.State == eBuildingState.NORMAL)
                                    tempList.Add(tag);
                            }
                        }
                    }
                }
            }
            return tempList;
        }

        public int GetBuildingTagInGridData(int floor, int cell)
        {
            int result = 0;

            if (ExteriorData.ExteriorGrid.ContainsKey(floor))
            {
                //그리드 데이터가 있으면 그 값을 주고, 없으면 -1로 리턴(원래는 0으로 주고있던게 - 못가는 곳과 빈곳을 동일처리한게 잘못된것같음)
                result = ExteriorData.ExteriorGrid[floor].ContainsKey(cell) ? ExteriorData.ExteriorGrid[floor][cell] : -1;
            }

            return result;
        }

        public int GetAreaLevel()
        {
            return TownInfo.AreaLevel;
        }

        public BuildInfo GetUserBuildingInfoByTag(int buildingTag)
        {
            if (buildings.ContainsKey(buildingTag))
                return buildings[buildingTag];

            return null;
        }

        public List<BuildInfo> GetUserBuildingList()
        {
            return buildings.Values.ToList();
        }

        public List<int> GetUserBuildingTagList()
        {
            return buildings.Keys.ToList();
        }

        public class EnergyExpireInfo
        {
            public int exp;
            public int energy;

            public void init()
            {
                exp = 0;
                energy = 0;
            }
        }

        public EnergyExpireInfo GetNextEnergyExpire()
        {
            var exp = UserData.Energy_Exp;
            var maxStamina = AccountData.GetLevel(User.instance.UserData.Level).MAX_STAMINA;//TableManager.GetTable<AccountTable>().GetByLevel(User.Instance.UserData.Level).MAX_STAMINA;
            var totalTick = exp + ((maxStamina - UserData.Energy) * 300);

            EnergyExpireInfo energyInfo = new EnergyExpireInfo();

            if (UserData.Energy >= maxStamina)
            {
                energyInfo.init();
                energyInfo.exp = -1;
                energyInfo.energy = UserData.Energy;
            }
            else if (exp < 0 || totalTick <= TimeManager.GetTime())
            {
                UserData.UpdateEnergy(maxStamina);

                energyInfo = new EnergyExpireInfo();
                energyInfo.init();
                energyInfo.exp = -1;
                energyInfo.energy = UserData.Energy;
            }
            else if (exp > TimeManager.GetTime())
            {
                energyInfo = new EnergyExpireInfo();
                energyInfo.init();
                energyInfo.exp = exp;
                energyInfo.energy = UserData.Energy;
            }
            else if (totalTick > TimeManager.GetTime())
            {
                for (var i = UserData.Energy + 1; i < maxStamina; i++)
                {
                    UserData.UpdateEnergyExp(UserData.Energy_Exp + 20);
                    UserData.UpdateEnergy(i);
                    if (UserData.Energy_Exp > TimeManager.GetTime())
                    {
                        break;
                    }
                }

                energyInfo = new EnergyExpireInfo();
                energyInfo.init();
                energyInfo.exp = UserData.Energy_Exp;
                energyInfo.energy = UserData.Energy;
            }
            return energyInfo;
        }
        public T GetLandmarkData<T>() where T : Landmark // : LandmarkCoindozer | LandmarkSubway | LandmarkWorldtrip | LandmarkRequestCenter
        {
            if (landmark_data.TryGetValue(typeof(T), out var val))
            {
                return val as T;
            }
            return null;
        }
        private Type GetLandmarkType(eLandmarkType type)
        {
            return type switch
            {
                eLandmarkType.Dozer => SBDefine.TYPE_DOZER,
                eLandmarkType.Travel => SBDefine.TYPE_TRAVEL,
                eLandmarkType.SUBWAY => SBDefine.TYPE_SUBWAY,
                eLandmarkType.EXCHANGE => SBDefine.TYPE_EXCHANGE,
                eLandmarkType.GEMDUNGEON => SBDefine.TYPE_GEMDUNGEON,
                eLandmarkType.MINE => SBDefine.TYPE_MINE,
                eLandmarkType.GUILD => null,
                eLandmarkType.UNKNOWN => null,
                _ => null
            };
        }

        public void SetPurchased(JObject info)
        {
            ShopManager.Instance.SetPurchased(info);
        }
        public void UpdatePurchased(int index, int count)
        {
            if (index < 0)
                return;

            ShopManager.Instance.UpdatePurchased(index, count);
        }

        public void SetPrivateGoods(JObject info)
        {
            if (info == null)
                return;

            ShopManager.Instance.SetPrivateGoods(info);
        }

        public void UpdatePrivateGoods(int index, long stamp)
        {
            ShopManager.Instance.UpdatePrivateGoods(index, stamp);
            UIObjectEvent.Event(UIObjectEvent.eEvent.REFRESH_BADGE);
            PopupManager.GetPopup<ShopPopup>().RefreshCurrentMenu();
        }

        public void SetSubscribeGoods(JToken info)
        {
            ShopManager.Instance.SetSubscribeGoods(info);
        }

        public void SetAdvertisement(JObject info)
        {
            if (info == null)
                return;
            JObject ad_list = (JObject)info["ad_list"];
            if (ad_list == null)
                return;
            var properties = ad_list.Properties();

            foreach (var obj in properties.Select((value, i) => (value, i)))
            {
                string key = obj.value.Name;
                int index = int.Parse(key);

                var adver = ShopManager.Instance.GetAdvertiseState(index);
                if (adver != null)
                {
                    adver.UpdateAdvertise(ad_list[key]["c"].Value<int>(), ad_list[key]["u"].Value<int>());
                }
            }
        }
        public void UpdateAdvertisement(int index, int view)
        {
            if (index < 0)
                return;

            var adver = ShopManager.Instance.GetAdvertiseState(index);
            if (adver != null)
            {
                adver.UpdateAdvertise(view, TimeManager.GetDateTime());
            }
        }


        public void UpdateSequence(int seq, Action cb = null)
        {
            seq = Math.Max(UserData.Sequence, seq);
            CacheUserData.SetInt("Sequence", seq);
            WWWForm param = new WWWForm();
            param.AddField("seq", seq); //메뉴번호 
            NetworkManager.Send("user/sequence", param, (res) =>
            {
                Debug.Log("update seq : " + seq.ToString());

                cb?.Invoke();
            });
        }
    }


    static public class CacheUserData
    {
        private static readonly string DBName = "{0}_cache.db";
        private static readonly int DBVersion = 1;
#if UNITY_EDITOR
        private static readonly string PATH = Application.dataPath + "/";
#else
        private static readonly string PATH = Application.persistentDataPath + "/";
#endif
        static Dictionary<string, DBUser_cache> cached = new Dictionary<string, DBUser_cache>();
        private static string GetDBName()
        {
            return Path.Combine(PATH, string.Format(DBName, UserKey));
        }
        private static SQLiteConnection DB(SQLiteOpenFlags flag = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create)
        {
            var dbName = GetDBName();
            if(!File.Exists(dbName) && flag.HasFlag(SQLiteOpenFlags.Create))
            {
                var ret = new SQLiteConnection(dbName, flag);
                ret.DropCreate<DBUser_cache>();

                string userData = PlayerPrefs.GetString(UserKey, "");

                if (!string.IsNullOrEmpty(userData))
                {
                    var prev = JObject.Parse(userData);
                    foreach(var pc in prev)
                    {
                        DBUser_cache d = new DBUser_cache();
                        d.UNIQUE_KEY = pc.Key;
                        d.VALUE = pc.Value.ToString();
                        ret.Insert(d);
                    }

                    PlayerPrefs.DeleteKey(UserKey);
                }

                return ret;
            }

            if (File.Exists(dbName))
            {
                return new SQLiteConnection(dbName, flag);
            }

            return null;
        }

        static public bool GetBoolean(string key, bool unset_default = false)
        {
            DBUser_cache value = GetValue(key);
            if (value != null)
            {
                bool ret = unset_default;
                if (bool.TryParse(value.VALUE, out ret))
                    return ret;
            }

            return unset_default;
        }


        static public void SetBoolean(string key, bool value)
        {
            SetValue(key, value);
        }
        static public int GetInt(string key, int unset_default = 0)
        {
            DBUser_cache value = GetValue(key);
            if (value != null)
            {
                int ret = unset_default;
                if (int.TryParse(value.VALUE, out ret))
                    return ret;
            }

            return unset_default;
        }

        static public void SetInt(string key, int value)
        {
            SetValue(key, value);
        }

        static public string GetString(string key, string unset_default = "")
        {
            DBUser_cache value = GetValue(key);
            if (value != null)
            {
                return value.VALUE;
            }

            return unset_default;
        }

        static public void SetString(string key, string value)
        {
            SetValue(key, value);
        }

        private static DBUser_cache GetValue(string key)
        {
            if (cached.ContainsKey(key))
                return cached[key];

            DBUser_cache data = DB().Get<DBUser_cache>(key);
            if (data != null)
            {
                cached.Add(key, data);
                return data;
            }

            return null;
        }

        public static void DeleteKey(string key)
        {
            DB().Delete<DBUser_cache>(key);
        }

        private static void SetValue(string key, JToken value)
        {
            DBUser_cache d = new DBUser_cache();
            d.UNIQUE_KEY = key;
            d.VALUE = value.ToString();
            DB().InsertOrReplace(d);

            if(cached.ContainsKey(key))
                cached[key] = d;
            else
                cached.Add(key, d);
        }

        static string UserKey { get { return SBGameManager.GetPrefStringByServer(User.Instance.UserAccountData.UserNumber.ToString()); } }
    };

    public class Lock
    {
        private List<int> lockPet = null;
        private List<int> lockPart = null;
        public List<int> LockPart { get { return lockPart; } }
        void InitLockPetData()
        {
            lockPet = new List<int>();

            string raw = User.Instance.PrefData.PetLock;
            if (string.IsNullOrEmpty(raw))
                raw = CacheUserData.GetString("PetLock", "");

            if (string.IsNullOrEmpty(raw))
                return;

            JArray array = JArray.Parse(raw);
            if (array.Count == 0)
                return;

            foreach (var info in array)
            {
                lockPet.Add(info.Value<int>());
            }
        }

        void SaveLockPetData(Action ok, Action fail)
        {
            JArray array = new JArray();

            foreach (var tag in lockPet)
            {
                array.Add(tag);
            }

            string data = array.ToString(Newtonsoft.Json.Formatting.None);
            //CacheUserData.SetString("PetLock", data);

            var param = new WWWForm();
            param.AddField("data", data);
            NetworkManager.Send("pet/lock", param, (jsonData) =>
            {
                if (jsonData.ContainsKey("rs") && (eApiResCode)jsonData["rs"].Value<int>() == eApiResCode.OK)
                {
                    ok?.Invoke();
                }
                else
                {
                    fail?.Invoke();
                }
            },
            (failRes) =>
            {
                fail?.Invoke();
            });
        }

        void InitLockPartData()
        {
            lockPart = new List<int>();

            string raw = User.Instance.PrefData.PartLock;
            if (string.IsNullOrEmpty(raw))
                raw = CacheUserData.GetString("PartLock", "");
            if (string.IsNullOrEmpty(raw))
                return;

            JArray array = JArray.Parse(raw);
            if (array.Count == 0)
                return;

            foreach (var info in array)
            {
                lockPart.Add(info.Value<int>());
            }
        }

        void SaveLockPartData(Action ok, Action fail)
        {
            JArray array = new JArray();

            foreach (var tag in lockPart)
            {
                array.Add(tag);
            }

            string data = array.ToString(Newtonsoft.Json.Formatting.None);
            //CacheUserData.SetString("PartLock", data);

            var param = new WWWForm();
            param.AddField("data", data);
            NetworkManager.Send("part/lock", param, (jsonData) =>
            {
                if (jsonData.ContainsKey("rs") && (eApiResCode)jsonData["rs"].Value<int>() == eApiResCode.OK)
                {
                    ok.Invoke();
                }
                else
                {
                    fail?.Invoke();
                }
            },
            (failRes) =>
            {
                fail?.Invoke();
            });
        }

        public void Clear()
        {
            if (lockPet != null)
                lockPet.Clear();
            lockPet = null;

            if (lockPart != null)
                lockPart.Clear();
            lockPart = null;
        }

        public bool IsLockPet(int tag)
        {
            if (lockPet == null)
            {
                InitLockPetData();
            }

            return lockPet.Contains(tag);
        }

        public void SetPetLock(int tag, Action ok)
        {
            if (lockPet == null)
            {
                InitLockPetData();
            }

            if (!lockPet.Contains(tag))
            {
                lockPet.Add(tag);
                SaveLockPetData(() =>
                {
                    ok.Invoke();
                    ToastManager.On(StringData.GetStringByStrKey("잠금"));
                }, () =>
                {
                    lockPet.Remove(tag);
                });
            }

            PetDataEvent.Send(PetDataEvent.PetEvent.LOCK_STATE, tag);
        }

        public void SetPetUnlock(int tag, Action ok)
        {
            if (lockPet == null)
            {
                InitLockPetData();
            }

            if (lockPet.Contains(tag))
            {
                lockPet.Remove(tag);
                SaveLockPetData(() =>
                {
                    ok.Invoke();
                    ToastManager.On(StringData.GetStringByStrKey("잠금해제"));
                }, () =>
                {
                    lockPet.Add(tag);
                });
            }

            PetDataEvent.Send(PetDataEvent.PetEvent.LOCK_STATE, tag);
        }

        public bool IsLockPart(int tag)
        {
            if (lockPart == null)
            {
                InitLockPartData();
            }

            return lockPart.Contains(tag);
        }

        public void SetPartLock(int tag, Action ok)
        {
            if (lockPart == null)
            {
                InitLockPartData();
            }

            if (!lockPart.Contains(tag))
            {
                lockPart.Add(tag);
                SaveLockPartData(() =>
                {
                    ok.Invoke();
                    ToastManager.On(StringData.GetStringByStrKey("잠금"));
                }, () =>
                {
                    lockPart.Remove(tag);
                });
            }
            PartDataEvent.Send(PartDataEvent.PartEvent.LOCK_STATE, tag);
        }

        public void SetPartUnlock(int tag, Action ok)
        {
            if (lockPart == null)
            {
                InitLockPartData();
            }

            if (lockPart.Contains(tag))
            {
                lockPart.Remove(tag);
                SaveLockPartData(() =>
                {
                    ok.Invoke();
                    ToastManager.On(StringData.GetStringByStrKey("잠금해제"));
                }, () =>
                {
                    lockPart.Add(tag);
                });
            }
            PartDataEvent.Send(PartDataEvent.PartEvent.LOCK_STATE, tag);
        }
    }
}
