using Crosstales;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SandboxNetwork
{
    public struct GemDungeonUpdateEvent
    {
        private static GemDungeonUpdateEvent obj; // 

        public int floor;
        public int needMonster; // 새로 생성이 필요한 몬스터 수
        public List<int> removeMonsters; // 제거된 몬스터 태그
        public static void Send(int floor, int needMonster, List<int> removeMonsters)
        {
            obj.needMonster = needMonster;
            obj.removeMonsters = removeMonsters;
            obj.floor = floor;
            EventManager.TriggerEvent(obj);
        }
    }
    public class GemdungeonBuilding : LandmarkBuilding, EventListener<LandmarkUpdateEvent>, EventListener<GemDungeonUpdateEvent>
    {
        [SerializeField] Transform dragonTr;
        [SerializeField] GemDungeonStage dungeonStage;

        public LandmarkGemDungeon GemdungeonData { get => LandmarkGemDungeon.Get(); }
        public LandmarkGemDungeonFloor FloorData { get => GemdungeonData?.GetFloorData(Floor); }
        public GemDungeonStage DungeonStage { get { return dungeonStage; } }
        const float REFRESH_TIME = 6f;

        List<int> lastDragons = new List<int>();
        List<int> lastFatigueOutDragons = new List<int>();
        eGemDungeonState lastState = eGemDungeonState.NONE;
        int lastWorld = 0;
        int lastStage = 0;
        float refresh = 0f;
        int maxMonsterCount {get { return GameConfigTable.GetConfigIntValue("GEMDUNGEON_MONSTER_COUNT_MAX"); } }
    
        int currentWorld = 0;
        int currentStage = 0;

        List<int> monsterSpawnKeys = new List<int>();

        List<Animator> gemBuildingAnimations = new List<Animator>();
        bool isAnim = false;
        bool isOpenAnim = false;
        bool isInit = false;
        int lastRewardCount = -1;
        protected override void Start()
        {
            EventManager.AddListener<LandmarkUpdateEvent>(this);
            EventManager.AddListener<GemDungeonUpdateEvent>(this);
            lastWorld = StageManager.Instance.AdventureProgressData.GetLatestWorld();
            lastStage = StageManager.Instance.AdventureProgressData.GetWorldInfoData(lastWorld).GetLastestClearStage();
            if (lastStage == 0) // 새로운 스테이지 입성했지만 하나도 클리어하지 않은 경우 이전 스테이지로
            {
                lastWorld = math.max(lastWorld - 1, 1);
                lastStage = math.max(StageManager.Instance.AdventureProgressData.GetWorldInfoData(lastWorld).GetLastestClearStage(), 1);
            }
            base.Start();
            //if (isInit == false)
            //{
            //    if (FloorData != null) // 건설완료 후에도 이 과정이 호출되어야 함
            //    {
            //        SetWorldRand();
            //        SetSpawnData();
            //        isInit = true;
            //    }
            //}
            refresh = REFRESH_TIME;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventManager.RemoveListener<LandmarkUpdateEvent>(this);
            EventManager.RemoveListener<GemDungeonUpdateEvent>(this);
            
        }

        void SetWorldRand()
        {
            currentWorld = SBFunc.Random(0, lastWorld) + 1;
            if (currentWorld == lastWorld)
            {
                currentStage = SBFunc.Random(0, lastStage) + 1;
            }
            else
            {
                int max = math.max(StageBaseData.GetByWorld(currentStage).Count, 1);
                currentStage = SBFunc.Random(0, max) + 1;
            }
        }

        void SetSpawnData()
        {

            var spawn = MonsterSpawnData.GetBySpawnGroup(StageBaseData.GetByWorldStage(currentWorld,currentStage).SPAWN);
            

            List<int> allSpawnList = new List<int>();
            foreach (var item in spawn)
            {
                allSpawnList.Add(item.KEY);
            }
            allSpawnList.CTShuffle(0);
            monsterSpawnKeys = allSpawnList.Take(maxMonsterCount).ToList();
            dungeonStage.SetData(FloorData.Dragons, monsterSpawnKeys, Floor);
        }
        void SetNextStage()
        {
            if(currentWorld == lastWorld && currentStage == lastStage)
            {
                currentWorld = 1;
                currentStage = 1;
                return;
            }
            int maxStage = math.max(StageBaseData.GetByWorld(currentWorld).Count, 1);
            if(currentStage == maxStage)
            {
                currentWorld += 1;
                currentStage = 1;
                return;
            }
            currentStage += 1;
        }
        List<int> GetNewMonster(int needMonsterCount)
        {
            SetNextStage();
            List<int> ret = new List<int>();
            var stageData = StageBaseData.GetByWorldStage(currentWorld, currentStage);
            if(stageData == null)
            {
                Debug.Log("err info : "+currentWorld.ToString()+"-"+currentStage.ToString());
                SetWorldRand();
                stageData = StageBaseData.GetByWorldStage(currentWorld, currentStage);
            }
            
            var spawn = MonsterSpawnData.GetBySpawnGroup(stageData.SPAWN);
            foreach (var item in spawn)
            {
                ret.Add(item.KEY);
            }
            ret.CTShuffle(0);
            ret.RemoveRange(needMonsterCount, ret.Count - needMonsterCount);
            return ret;
        }

        public void OnEvent(LandmarkUpdateEvent eventData)
        {
            if (eventData.eLandmark != eLandmarkType.GEMDUNGEON)
                return;

            if (FloorData != null && IsDataChange())
            {
                dungeonStage.SetData(FloorData.Dragons, monsterSpawnKeys, Floor);
            }
            lastDragons.Clear();
            foreach (var item in FloorData.Dragons)
            {
                lastDragons.Add(item);
            }
            lastFatigueOutDragons.Clear();
            foreach (var item in GemdungeonData.DragonDatas.Values)
            {
                if (item.ExpectedFatigue == 0 && lastDragons.Contains(item.DragonNo))
                    lastFatigueOutDragons.Add(item.DragonNo);
            }
            lastState = FloorData.State;
            refresh = 0;
            CheckProductAlarm();
        }

        bool IsDataChange()
        {
            if (lastState != FloorData.State)
                return true;
            var curDragon = FloorData.Dragons;
            if (curDragon.Count != lastDragons.Count)
                return true;
            if(curDragon.SequenceEqual(lastDragons)==false)
                return true;
            List<int> curBurnOut = new List<int>();
            foreach ( var item in GemdungeonData.DragonDatas.Values)
            {
                if (item.ExpectedFatigue == 0)
                    curBurnOut.Add(item.DragonNo);
            }
            if (lastFatigueOutDragons.Count != curBurnOut.Count)
                return true;
            return !curBurnOut.SequenceEqual(lastFatigueOutDragons);
        }

        public void OnEvent(GemDungeonUpdateEvent eventData)
        {
            if(eventData.floor == Floor)
            {
                //monsterSpawnKeys.RemoveAll(x=> eventData.removeMonsters.Contains(x));
                // 진행한 탐험이 적을경우 monsterSpawnKeys가 중복의 키를 들고 있을 수 있음, 그렇게 되면 1마리 죽은 걸 [중복 수]마리 만큼 죽었다고 인지됨
                // 따라서 removeAll로 처리할 경우 문제가 발생할 가능성이 있음

                foreach(var key in eventData.removeMonsters)
                {
                    if(monsterSpawnKeys.Contains(key))
                        monsterSpawnKeys.Remove(key);
                }
                
                if(monsterSpawnKeys.Count + eventData.needMonster == maxMonsterCount)  
                {
                    var addMonsterList = GetNewMonster(eventData.needMonster);
                    //Debug.Log("monster alive : " + addMonsterList.Count.ToString());
                    monsterSpawnKeys.AddRange(addMonsterList);
                    dungeonStage.AddMonsterData(addMonsterList);
                }
                else if (monsterSpawnKeys.Count + eventData.needMonster < maxMonsterCount) // 예외처리
                {
                    var addMonsterList = GetNewMonster(maxMonsterCount - monsterSpawnKeys.Count);
                    //Debug.Log("monster alive : " + addMonsterList.Count.ToString());
                    monsterSpawnKeys.AddRange(addMonsterList);
                    dungeonStage.AddMonsterData(addMonsterList);
                }
            }
        }

        public void SetGemBuildingAnimation(List<Animator> animations)
        {
            gemBuildingAnimations = animations;
        }

        public override bool ActiveAction()
        {
            bool result = false;
            if (FloorData != null)
            {
                SetLockIcon(FloorData.State);
                npcSpine?.gameObject.SetActive(true);
                switch (FloorData.State)
                {
                    case eGemDungeonState.IDLE:
                    case eGemDungeonState.BATTLE:
                    case eGemDungeonState.END:
                        result = true;
                        break;
                    default:
                        result = false;
                        break;
                }
            }
            if (Data != null)
            {
                var state = Data.State;
                switch (state)
                {
                    case eBuildingState.NORMAL:
                        if (isOpenAnim == false)
                        {
                            foreach (var anim in gemBuildingAnimations)
                            {
                                anim.Play("open");
                            }
                            isOpenAnim = true;
                        }
                        if(isInit == false)
                        {
                            if (FloorData != null) // 건설완료 후에도 이 과정이 호출되어야 함
                            {
                                SetWorldRand();
                                SetSpawnData();
                                isInit = true;
                            }
                        }
                        break;
                    case eBuildingState.CONSTRUCTING:
                        int time = TimeManager.GetTimeCompare(Data.ActiveTime);
                        if (time <= 0)
                        {
                            time = 0;
                            if (shutterSpine != null)
                            {
                                shutterSpine.gameObject.SetActive(true);
                                shutterSpine.AnimationState.SetAnimation(0, "closed", false);
                            }
                        }
                        if (timeText != null)
                        {
                            if (time <= 0)
                            {
                                timeText.text = StringData.GetStringByStrKey("건설완료");
                                if (Data.Level > 0)
                                {
                                    foreach (var anim in gemBuildingAnimations)
                                    {
                                        anim.Play("closed");
                                    }
                                    isOpenAnim = false;
                                    SetLockIcon(state);
                                    dungeonStage.Clear();
                                }
                            }
                            else
                                timeText.text = SBFunc.TimeString(time);
                        }
                        curBuildingState = state;
                        break;
                    case eBuildingState.CONSTRUCT_FINISHED:
                        foreach (var anim in gemBuildingAnimations)
                        {
                            anim.Play("closed");
                        }
                        isOpenAnim = false;
                        SetLockIcon(state);
                        break;
                    case eBuildingState.LOCKED:
                    case eBuildingState.NOT_BUILT:
                    case eBuildingState.NONE:
                        if (isAnim == false)
                        {
                            foreach (var anim in gemBuildingAnimations)
                            {
                                foreach (var render in anim.transform.GetComponentsInChildren<SpriteRenderer>())
                                {
                                    if (render != null)
                                    {
                                        if (render.name.Contains("dimmed") || render.name.Contains("locked"))
                                        {
                                            render.gameObject.SetActive(true);
                                        }
                                    }
                                }
                            }
                            isAnim = true;
                        }
                        
                        SetLockIcon(state);
                        break;
                }


                finishLayerObject.SetActive(false);
                timeText.gameObject.SetActive(state == eBuildingState.CONSTRUCTING);
                
                if (state != eBuildingState.CONSTRUCT_FINISHED)
                {
                    if (shutterEffectObj != null && shutterEffectObj.activeSelf)
                        shutterEffectObj.SetActive(false);
                }
            }
            else
                SetLockIcon(eBuildingState.LOCKED);
            
            return result;
        }
        protected override void BuildingAction()
        {
            if (FloorData == null)
            {
                SetProductState(ProductState.UNKNOWN);
                return;
            }

            switch (FloorData.State)
            {
                case eGemDungeonState.IDLE:
                case eGemDungeonState.BATTLE:
                    SetProductState(ProductState.RUNNING);
                    break;
                case eGemDungeonState.END:
                    SetProductState(ProductState.COMPLETED_ALL);
                    break;
                default:
                    SetProductState(ProductState.UNKNOWN);
                    break;
            }

            refresh += waitSec;
            if (refresh > REFRESH_TIME)
            {
                refresh -= REFRESH_TIME;
                CheckProductAlarm();
            }
        }
        protected override void SetProductState(ProductState state)
        {
            if (curState == state)
                return;

            curState = state;
            if (BuildingProductUI != null)
            {
                BuildingProductUI.SetState(curState);
            }
        }

        protected void SetLockIcon(eGemDungeonState state)
        {
            base.SetLockIcon(state switch
            {
                eGemDungeonState.NONE => eBuildingState.NONE,
                eGemDungeonState.BATTLE => eBuildingState.NORMAL,
                eGemDungeonState.IDLE => eBuildingState.NORMAL,
                eGemDungeonState.END => eBuildingState.NORMAL,
                eGemDungeonState.NOT_BUILT => eBuildingState.NOT_BUILT,
                _ => eBuildingState.LOCKED
            });   
        }
        public override void OnClickLandmark()
        {
            if (null == GemdungeonData)
                return;

            switch (Data.State)
            {
                case eBuildingState.NONE:
                    break;
                case eBuildingState.LOCKED:
                    int TownLvToSubwayOpen = 0;
                    var buildingData = BuildingOpenData.GetByInstallTag((int)eLandmarkType.GEMDUNGEON);
                    if (buildingData != null)
                        TownLvToSubwayOpen = buildingData.OPEN_LEVEL;
                    ToastManager.On(string.Format(StringData.GetStringByIndex(100000059), TownLvToSubwayOpen));
                    break;
                case eBuildingState.NOT_BUILT:
                    PopupManager.OpenPopup<BuildingConstructPopup>(new BuildingPopupData(BTag));
                    break;
                case eBuildingState.CONSTRUCTING:
                    if(GemdungeonData.Level > 0)
                        BuildCompleteEvent.Send(this, eBuildingState.CONSTRUCT_FINISHED);
                    break;
                case eBuildingState.CONSTRUCT_FINISHED:
                    BuildCompleteEvent.Send(this, Data.State);
                    break;
                case eBuildingState.NORMAL:
                    //BuildingProductUI 클릭 시 획득제거
                    //if(BuildingProductUI != null)
                    //{
                    //    var items = BuildingProductUI.GetBubbles();
                    //    var count = items != null ? items.Count : 0;
                    //    if (count > 0 && FloorData.IsReward)
                    //    {
                    //        FloorData.OnHarvest();
                    //        return;
                    //    }
                    //}
                    PopupManager.OpenPopup<GemDungeonPopup>();
                    break;
                default:
                    break;
            }
        }
        public override void OnHarvest(eHarvestType harvestType)
        {
            //BuildingProductUI 클릭 시 획득제거
            //FloorData.OnHarvest();
            PopupManager.OpenPopup<GemDungeonPopup>();
        }
        public override void CheckProductAlarm()
        {
            if (BuildingProductUI != null)
            {
                BuildingProductUI.SetState(curState);
            }

            if (FloorData == null)
                return;

            var product = new List<ProductReward>();
            var rewards = FloorData.Rewards;
            for(int i = 0, count = rewards.Count; i < count; ++i)
            {
                if ((rewards[i] + FloorData.GetClientReward(i)) < SBDefine.MILLION)
                    continue;

                int itemNo = i switch {
                    1 => 10000017,
                    2 => 10000018,
                    3 => 10000019,
                    4 => 10000020,
                    _ => 110000004
                };

                product.Add(new ProductReward(eGoodType.ITEM, itemNo, 1));
            }

            if (lastRewardCount != product.Count)
            {
                lastRewardCount = product.Count;
                BuildingProductUI.SetProducts(product);
            }
        }
    }
}