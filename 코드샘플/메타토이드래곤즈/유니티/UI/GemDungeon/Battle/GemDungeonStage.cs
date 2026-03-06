using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork
{
    public class GemDungeonStage : BattleStage
    {
        [SerializeField]
        protected GemDungeonBattleMap map = null;
        public override IBattleMap Map { get => map; }
        public override List<List<BattleSpine>> PrevSpines { get; protected set; } = null;

        private GemdungeonBuilding building = null;
        private GemdungeonBuilding Building { get {
                if (building == null)
                    building = GetComponent<GemdungeonBuilding>();
                return building;
            } }

        GemDungeonBattleData gemDungeonBattleData = null;


        public int CurFloor { get; private set; } = 0;
        List<int> dragonTags = new();
        List<int> monsterSpawnTags = new();
        bool isInit= false;


        protected override void Start() { }

        public void SetData(List<int> dragonTagList, List<int> monsterSpawnList, int floor)
        {
            CurFloor = floor;
            if (OffenseSpines == null || OffenseSpines.Count == 0)
            {
                Initialize();
                isInit = true;
            }

            if (dragonTagList == null)
                dragonTags = new List<int>();
            else
            {
                foreach(var tag in dragonTags)
                {
                    if(Town.TownDragonsDic.ContainsKey(tag))
                    {
                        if (!dragonTagList.Contains(tag))
                        {
                            Town.TownDragonsDic[tag].Data.RemoveDragonState(eDragonState.GemDungeon);
                            Town.TownDragonsDic[tag].OnEvent(new DragonShowEvent());
                        }
                    }
                }
                dragonTags = dragonTagList;                
            }

            foreach (var tag in dragonTags)
            {
                if (Town.TownDragonsDic.ContainsKey(tag))
                {
                    Town.TownDragonsDic[tag].Data.SetDragonState(eDragonState.GemDungeon);
                    Town.TownDragonsDic[tag].OnEvent(new DragonHideEvent());
                }
            }

            monsterSpawnTags = monsterSpawnList;
            if (gemDungeonBattleData == null)
            {
                gemDungeonBattleData = new GemDungeonBattleData();
                gemDungeonBattleData.Initialize();
            }
            gemDungeonBattleData.SetData(dragonTags, monsterSpawnList);
            if (StateMachine == null)
            {
                StateMachine = new GemDungeonMachine(this, gemDungeonBattleData, CurFloor);
                StateMachine.SetState();
            }
            Refresh();
        }

        public void AddMonsterData(List<int> AddTag) // 죽어있는 몬스터들 대체하는 새로운 몬스터! 세팅
        {
            gemDungeonBattleData.AddMonsterData(AddTag);
        }

        public void Refresh()
        {
            if (isInit == false)
                return;
            var data = Building.FloorData;
            switch (data.State)
            {
                case eGemDungeonState.IDLE:
                    StateMachine.ChangeState<GemDungeonStateIdle>();
                    break;
                case eGemDungeonState.BATTLE:
                    StateMachine.ChangeState<GemDungeonStateBattle>();
                    break;
                case eGemDungeonState.END:
                    StateMachine.ChangeState<GemDungeonStateEnd>();
                    break;
            }
        }

        public override BattleSpine GetDefenseSpine(IBattleCharacterData data)
        {
            var baseData = data.BaseData as MonsterBaseData;
            if (baseData == null)
                return null;

            //var monsterPrefab = ResourceManager.GetResource<GameObject>(eResourcePath.MonsterClonePath, baseData.IMAGE);//HEAD
            var monsterPrefab = baseData.GetPrefab();
            if (monsterPrefab == null)
                return null;

            var monsterObj = Instantiate(monsterPrefab, Map.DefenseBeacon.transform);
            var monsterSpine = monsterObj.GetComponent<GemDungeonMonsterSpine>();
            if (monsterSpine == null)
                monsterSpine = monsterObj.AddComponent<GemDungeonMonsterSpine>();

            monsterSpine.SetData(data);
            data.SetTransform(monsterSpine.transform);

            return monsterSpine;
        }

        public override BattleSpine GetOffenseSpine(IBattleCharacterData data)
        {
            var baseData = data.BaseData as CharBaseData;
            if (baseData == null)
                return null;

            var dragonPrefab = baseData.GetDefaultSpine();
            if (dragonPrefab == null)
                return null;

            //var dragonSpine = baseData.LoadGemDragonSpine(data, Map.OffenseBeacon.transform);
            var dragonObj = Instantiate(dragonPrefab, Map.OffenseBeacon.transform);
            dragonObj.SetActive(true);
            var dragonSpine = dragonObj.GetComponent<GemDungeonDragonSpine>();
            if (dragonSpine == null)
                dragonSpine = dragonObj.AddComponent<GemDungeonDragonSpine>();

            dragonSpine.SetData(data);
            data.SetTransform(dragonSpine.transform);

            return dragonSpine;
        }

        public void RemoveAllObject(bool DragonClear, bool MonsterClear)
        {
            if(MonsterClear)
                SBFunc.RemoveAllChildrens(Map.DefenseBeacon.transform);
            if(DragonClear)
                SBFunc.RemoveAllChildrens(Map.OffenseBeacon.transform);
        }

        public void Clear()
        {
            RemoveAllObject(true, true);
            foreach (var spineList in DefenseSpines)
            {
                spineList.Clear();
            }
            foreach (var spineList in OffenseSpines)
            {
                spineList.Clear();
            }
            OffenseSpines.Clear();
            DefenseSpines.Clear();
        }
        public override void SetGreenHpBar(IBattleCharacterData data)
        {
            return;
        }

        public override void SetRedHpBar(IBattleCharacterData data)
        {
            return;
        }

        protected override void BattleEnd()
        {
            
        }
        protected override void OnPausePopup()
        {
            return;
        }

        protected override IEnumerator UpdateCO()
        {
            while (true)
            {
                if (StateMachine == null)
                {
                    yield return null;
                    continue;
                }
                if (false == StateMachine.Update(SBGameManager.Instance.DTime * 1.5f))
                {
                    var data = Building.FloorData;
                    switch (data.ExpectedState)
                    {
                        case eGemDungeonState.IDLE:
                            StateMachine.ChangeState<GemDungeonStateIdle>();
                            break;
                        case eGemDungeonState.BATTLE:
                            StateMachine.ChangeState<GemDungeonStateBattle>();
                            break;
                        case eGemDungeonState.END:
                            StateMachine.ChangeState<GemDungeonStateEnd>();
                            break;
                    }
                }
                //if (!StateMachine.Update(SBGameManager.Instance.DTime))
                //{

                //}
                yield return null;
            }
        }
        protected override void LateUpdate()
        {
            //base.LateUpdate();
        }
    }
}

