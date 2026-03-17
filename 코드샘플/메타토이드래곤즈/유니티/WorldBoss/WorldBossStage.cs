using Com.LuisPedroFonseca.ProCamera2D;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    [Serializable]
    public class WorldBossSpace
    {
        [SerializeField]
        public BoxCollider2D dangerSpace = null;
        [SerializeField]
        public Transform weakSpace = null;
        [SerializeField]
        public int dangerEffectKey = -1;
        [SerializeField]
        public int weakEffectKey = -1;
    }

    public class WorldBossStage : BattleStage
    {
        public long SCORE { get { return Data.BossData.SCORE; } }
        private WorldBossBattleData Data { get; set; } = null;
        public override List<List<BattleSpine>> PrevSpines { get; protected set; } = new List<List<BattleSpine>>();
        public static WorldBossStage Instance { get; private set; } = null;

        Transform[,] DragonsPos = new Transform[WorldBossFormationData.MAX_PARTY_COUNT, WorldBossFormationData.MAX_DRAGON_COUNT];

        private Dictionary<IBattleCharacterData, IBattleCharacterData> SummonMonsters = new Dictionary<IBattleCharacterData, IBattleCharacterData>();
        List<SBRect> dangerRect = null;
        List<Transform> weakSpace = null;
        WorldBossSpace[] Space = new WorldBossSpace[4];
        
        void InitPositionsAndSpace()
        {
            var worldbossMap = Map as WorldBossBattleMap;
            if(worldbossMap == null)
            {
                Debug.LogError("WorldBossLog : AdventureMap Component Exist Failed");
                return;
            }

            var dragonPosArr = worldbossMap.GetDragonPos();
            for(int i = 0; i < WorldBossFormationData.MAX_PARTY_COUNT; i++)
            {
                for(int j = 0; j < WorldBossFormationData.MAX_DRAGON_COUNT; j++)
                    DragonsPos[i, j] = dragonPosArr[i].GetDragonPos(j);
            }

            Space = worldbossMap.GetSpace();
        }
        protected override void Initialize()
        {
            if (!SBGameManager.Instance.IsLoaded())
                return;

            if (Instance != this && Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;
            

            SummonMonsters.Clear();

            if (maps == null)
            {
                Debug.LogError("WorldBossLog : Non Map");
                return;
            }

            Data = WorldBossManager.Instance.Data;
            if (Data == null)
            {
                Debug.LogError("WorldBossLog : Non Data");
                return;
            }

            var stageData = StageBaseData.GetByWorldStage(Data.World, Data.Stage);
            if (stageData == null)
            {
                Debug.LogError("WorldBossLog : Non StageData");
                return;
            }

            var map = maps.Find((GameObject obj) =>
            {
                if (obj == null)
                    return false;

                return obj.name == stageData.IMAGE;
            });
            if (map == null)
            {
                Debug.LogError("WorldBossLog : Non FindMap");
                return;
            }

            var mapObj = Instantiate(map, transform);
            if (mapObj == null)
            {
                Debug.LogError("WorldBossLog : Map Instantiate Failed");
                return;
            }

            Map = mapObj.GetComponent<WorldBossBattleMap>();
            if (Map == null)
            {
                Debug.LogError("WorldBossLog : AdventureMap Component Exist Failed");
                return;
            }

            InitPositionsAndSpace();

            UIManager.Instance.InitUI(eUIType.Battle_WorldBoss);

            PlayBGMSound();

            base.Initialize();
        }

        public override BattleSpine GetOffenseSpine(IBattleCharacterData data)
        {
            var baseData = data.BaseData as CharBaseData;
            if (baseData == null)
                return null;

            var dragonPrefab = baseData.GetDefaultSpine();
            if (dragonPrefab == null)
                return null;

            var dragonObj = Instantiate(dragonPrefab, Map.OffenseBeacon.transform);
            dragonObj.SetActive(true);
            var dragonSpine = dragonObj.GetComponent<WorldBossDragonSpine>();
            if (dragonSpine == null)
                dragonSpine = dragonObj.AddComponent<WorldBossDragonSpine>();

            dragonSpine.SetData(data);
            data.SetTransform(dragonSpine.transform);

            return dragonSpine;
        }
        public override BattleSpine GetDefenseSpine(IBattleCharacterData data)
        {
            return GetDefenseSpine(data, Map.DefenseBeacon.transform);
        }

        public BattleSpine GetDefenseSpine(IBattleCharacterData data, Transform parent)
        {
            var baseData = data.BaseData as MonsterBaseData;
            if (baseData == null)
                return null;

            //var monsterPrefab = ResourceManager.GetResource<GameObject>(eResourcePath.MonsterClonePath, baseData.IMAGE);//HEAD
            var monsterPrefab = baseData.GetPrefab();
            if (monsterPrefab == null)
                return null;

            var monsterObj = Instantiate(monsterPrefab, parent);
            var monsterSpine = monsterObj.GetComponent<BattleMonsterSpine>();
            if (monsterSpine == null)
            {
                monsterSpine = monsterObj.AddComponent<BattleMonsterSpine>();
                monsterObj.transform.SetParent(Map.DefenseBeacon.transform);
                monsterObj.transform.localScale = Vector3.one * baseData.SIZE;

                SBController controller = monsterSpine.Controller;
                if (controller == null)
                {
                    controller = monsterSpine.GetComponent<SBController>();
                }

                if(controller != null)
                {
                    List<BattleSpine> targetPartyDragons = null;
                    WorldBossPartData partInfo = WorldBossPartData.Get(data.ID);
                    if (partInfo != null)
                    {
                        controller.IsRight = (partInfo.ATTACK_PRIORITY & eWorldBoss.PRIORITY_BOTTON_LEFT) > 0 || (partInfo.ATTACK_PRIORITY & eWorldBoss.PRIORITY_TOP_LEFT) > 0;
                        targetPartyDragons = partInfo.GetTargets(OffenseSpines);
                    }

                    ((BattleWorldBossSummonMonsterData)data).SetSummonInfo(!controller.IsRight, targetPartyDragons);
                }
            }

            monsterSpine.SetData(data);
            data.SetTransform(monsterSpine.transform);

            return monsterSpine;
        }

        public BattleSpine GetWorldBossSpine(IBattleCharacterData data)
        {
            var baseData = data.BaseData as MonsterBaseData;
            if (baseData == null)
                return null;

            //var monsterPrefab = ResourceManager.GetResource<GameObject>(eResourcePath.MonsterClonePath, baseData.IMAGE);//HEAD
            var monsterPrefab = baseData.GetPrefab();
            if (monsterPrefab == null)
                return null;

            var monsterObj = Instantiate(monsterPrefab, Map.DefenseBeacon.transform);
            var monsterSpine = monsterObj.GetComponent<BattleMonsterSpine>();
            //if (monsterSpine != null)
            //    Destroy(monsterSpine);

            //monsterSpine = monsterObj.GetComponent<BattleWorldBossSpine>();
            //if(monsterSpine == null)
            //{
            //    //monsterSpine = monsterObj.AddComponent<BattleWorldBossSpine>();
            //}

            monsterSpine.SetData(data);
            data.SetTransform(monsterSpine.transform);

            return monsterSpine;
        }

        protected override IEnumerator UpdateCO()
        {
            yield return null;

            StateMachine = new WorldBossMachine(this, Data);
            StateMachine.StateInit();
            StateMachine.StateDataClear();

			while (true)
            {
                if (ScenarioManager.Instance.IsPlaying)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                if (StateMachine == null)
                {
                    yield return null;
                    continue;
                }

                if (!StateMachine.Update(SBGameManager.Instance.DTime))
                {
                    if (StateMachine.IsState<WorldBossStateInit>())
					{
						switch (Data.State)
						{
							case eBattleState.Playing:
								StateMachine.ChangeState<WorldBossStateBattle>();
								continue;
							case eBattleState.Win:
							case eBattleState.Lose:
							case eBattleState.TimeOver:
								StateMachine.ChangeState<WorldBossStateEnd>();
								continue;
							case eBattleState.None:
							default:
                                break;
                        }
					}                    
                    else if (StateMachine.IsState<WorldBossStateBattle>())
                    {
                        switch (Data.State)
                        {
                            case eBattleState.Playing:
                            case eBattleState.TimeOver:
#if DEBUG
                                if (User.Instance.UserAccountData.UserNumber < 0)
                                    StateMachine.ChangeState<SimulatorStateResult>();
                                else
#endif
                                     StateMachine.ChangeState<WorldBossStateResult>();
                                continue;
                            case eBattleState.Win:
                            case eBattleState.Lose:
                                StateMachine.ChangeState<WorldBossStateEnd>();
                                continue;
                            case eBattleState.None:
                            default:
                                break;
                        }
                    }
                    else if (StateMachine.IsState<WorldBossStateResult>())
                    {
                        StateMachine.ChangeState<WorldBossStateEnd>();
                    }
                    else if (StateMachine.IsState<WorldBossStateEnd>())
                    {
                        BattleEnd();
                        yield break;
                    }
                }
                yield return null;
            }
        }
        protected override void BattleEnd()
        {
            UIDamageClearEvent.Send();
            //LoadingManager.Instance.EffectiveSceneLoad("WorldBossResult", eSceneEffectType.BlackBackground);
        }
        public override void SetGreenHpBar(IBattleCharacterData data)
        {
            SetHpBar(hpBarCanvas, hpBarGreenPrefab, data);
        }
        public override void SetRedHpBar(IBattleCharacterData data)
        {
            SetHpBar(hpBarCanvas, hpBarRedPrefab, data);
        }
        void PlayBGMSound()
        {
            if (Data == null) { return; }

            SoundManager.Instance.PushBGM("BGM_WORLD_BOSS_BATTLE", true);
        }
        
        protected override void OnPausePopup()
        {
            if (IsBattleState())
            {
                LoadingManager.Instance.Clear();
                PausePopup.OpenWorldBossPopup();
            }
        }
        protected override void Destroy()
        {
            base.Destroy();
            Data = null;
            if (Instance == this)
                Instance = null;
        }

        public eDirectionBit GetDirectionFromPartyIndex(int index)
        {
            switch(index)
            {
                case 0:
                case 2:
                    return eDirectionBit.Right;
                case 1:
                case 3:
                    return eDirectionBit.Left;
            }

            Debug.LogError("index is error");
            return eDirectionBit.None;            
        }
        public eDirectionBit GetPartyDirection(BattleSpine spine)
        {
            if (spine == null || spine.Data == null)
                return eDirectionBit.None;

            int dTag = spine.Data.ID;
            foreach (var posx in Data.OffensePos)
            {
                var x = posx.Key;
                for (int y = 0; y < posx.Value.Count; y++)
                {
                    if (dTag == posx.Value[y].DragonTag)
                    {
                        return GetDirectionFromPartyIndex(x);
                    }
                }
            }

            return eDirectionBit.None;
        }

        public override Vector3 GetDragonStartPosition(int x, int y, int maxY)
        {
            return DragonsPos[x, y].position;
        }
        public override Vector3 GetDragonEndPosition(int x, int y, int maxY)
        {
            return DragonsPos[x, y].position;
        }
        public Vector3 GetDragonEndPosition(BattleSpine spine)
        {
            if (spine == null || spine.Data == null)
                return Vector3.zero;

            int dTag = spine.Data.ID;
            foreach(var posx in Data.OffensePos)
            {
                var x = posx.Key;
                for (int y = 0; y < posx.Value.Count; y++)
                {
                    if(dTag == posx.Value[y].DragonTag)
                    {
                        return GetDragonEndPosition(x, y, 2);
                    }
                }
            }

            return Vector3.zero;
        }

        public override Vector3 GetOffenseStartFieldPosition(float offset)
        {
            return new Vector3(0f, 0f, 0f);
        }
        public override Vector3 GetOffenseEndFieldPosition(float offset)
        {
            return new Vector3(0f, 0f, 0f);
        }
        public override Vector3 GetMonsterStartPosition(int x, int y, int maxY, float xOffset, float extraOffset = 0)
        {
            return GetWorldBossPosition();
        }

        public override Vector3 GetMonsterStartFieldPosition(float xOffset, float extraOffset = 0)
        {
            return GetWorldBossPosition();
        }

        public override Vector3 GetMonsterEndFieldPosition(float offset)
        {
            return GetWorldBossPosition();
        }

        Vector3 GetWorldBossPosition()
        {
            var worldbossMap = Map as WorldBossBattleMap;
            if (worldbossMap == null)
            {
                Debug.LogError("WorldBossLog : AdventureMap Component Exist Failed");
                return  Vector3.zero;
            }

            return worldbossMap.GetWorldBossPosition();
        }

        public override BattleSpine MakeSummonMonsterData(IBattleCharacterData casterData, int id)
        {
            BattleSpine ret = null;
            if (casterData.IsEnemy)
            {
                if (SummonMonsters.ContainsKey(casterData))
                {
                    DestroySummonMonster(SummonMonsters[casterData]);
                }
            }

            if (StateMachine.BattleState != null)
            {
                var enemyData = MakeSummonMonsterData(id);
                var spine = GetDefenseSpine(enemyData, casterData.Transform);
                if (spine != null)
                {
                    DefenseSpines[0].Add(spine);
                    BattleStateLogic battleState = StateMachine.BattleState as BattleStateLogic;
                    if (battleState != null)
                    {
                        battleState.defenses.Add(spine);

                        SetRedHpBar(enemyData);
                        ret = spine;
                    }
                }
            }

            if (ret == null)
                return ret;

            SummonMonsters[casterData] = ret.Data;
            if(ret.Data is BattleWorldBossSummonMonsterData summonData)
            {
                summonData.SetDirection(casterData);
            }
            //AddCameraTarget(ret.transform);
            return ret;
        }

        public void DestroySummonMonster(IBattleCharacterData monster)
        {
            if (monster == null)
                return;

            SummonMonsters.Remove(monster);
            if (monster.Death)
                return;

            var spine = monster.GetSpine();
            if (spine != null)
            {
				//DelCameraTarget(spine.transform);
                DefenseSpines[0].Remove(spine);
                BattleStateLogic battleState = StateMachine.BattleState as BattleStateLogic;
                if (battleState != null)
                {
                    battleState.defenses.Remove(spine);
                    RemoveHpBar(monster);
                }

                monster.HP = 0;
                spine.Death();
                //Destroy(spine.gameObject);
                //monster.SetTransform(null);
            }
        }

        public override BattleMonsterData MakeSummonMonsterData(int id)
        {
            var enemyData = new BattleWorldBossSummonMonsterData(Data.BossData.Level);
            enemyData.SetData(0, id);
            return enemyData;
        }
        
        public IBattleCharacterData GetSummonMonsterByPart(IBattleCharacterData part)
        {
            if (part == null)
                return null;

            if (SummonMonsters.ContainsKey(part))
            {
                if (SummonMonsters[part].Death)
                    return null;

                return SummonMonsters[part];
            }

            return null;
        }
        public bool IsSummoningParts(IBattleCharacterData part)
        {
            return GetSummonMonsterByPart(part) != null;
        }

        public void AddCameraTarget(Transform transform)
        {
            if (ProCamera2D.Instance.GetCameraTarget(transform) != null)
                return;

            if (StateMachine.BattleState != null)
            {
                (StateMachine.BattleState as WorldBossState).targets.Add(ProCamera2D.Instance.AddCameraTarget(transform, 0, 0));
            }
        }
        public void DelCameraTarget(Transform transform)
        {
            if (StateMachine.BattleState != null)
            {
                (StateMachine.BattleState as WorldBossState).targets.Remove(ProCamera2D.Instance.GetCameraTarget(transform));
                ProCamera2D.Instance.RemoveCameraTarget(transform);
            }
        }

        public bool IsOnDangerSpace(BattleSpine spine)
        {
            foreach (var space in GetDangerRects())
            {
                if (space.IsContain(spine.Data.Transform.position))
                    return true;
            }
            return false;
        }

        public List<SBRect> GetDangerRects()
        {
            if (dangerRect == null)
            {
                dangerRect = new List<SBRect>();
                foreach (var space in Space)
                {
                    dangerRect.Add(new SBRect(space.dangerSpace));
                }
            }

            return dangerRect;
        }

        public List<Transform> GetWeakPosList()
        {
            if(weakSpace == null)
            {
                weakSpace = new List<Transform>();
                foreach (var space in Space)
                {
                    weakSpace.Add(space.weakSpace);
                }
            }

            return weakSpace;
        }

        public List<WorldBossSpace> GetSpaceInfoList()
        {
            return Space.ToList();
        }

        public bool IsBattleState()
        {
            if (StateMachine != null)
            {
                return StateMachine.IsState<WorldBossStateBattle>();
            }

            return false;
        }
    }
}