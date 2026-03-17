using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class DailyStage : BattleStage
    {
        private DailyBattleData Data { get; set; } = null;
        public override List<List<BattleSpine>> PrevSpines { get; protected set; } = new List<List<BattleSpine>>();
        public static DailyStage Instance { get; private set; } = null;
        public DailyMachine StateMachine { get; private set; } = null;

        protected override void Initialize()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;

            if (maps == null)
            {
                Debug.LogError("DailyLog : Non Map");
                return;
            }

            Data = DailyManager.Instance.Data;
            if (Data == null)
            {
                Debug.LogError("AdventureLog : Non Data");
                return;
            }

            var stageData = StageBaseData.GetByWorldStage(Data.World, Data.Stage);
            if (stageData == null)
            {
                Debug.LogError("AdventureLog : Non StageData");
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
                Debug.LogError("AdventureLog : Non FindMap");
                return;
            }

            var mapObj = Instantiate(map, transform);
            if (mapObj == null)
            {
                Debug.LogError("AdventureLog : Map Instantiate Failed");
                return;
            }

            Map = mapObj.GetComponent<BattleMap>();
            if (Map == null)
            {
                Debug.LogError("AdventureLog : AdventureMap Component Exist Failed");
                return;
            }

            UIManager.Instance.InitUI(eUIType.Battle_Daily);
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
            var dragonSpine = dragonObj.GetComponent<BattleDragonSpine>();
            if (dragonSpine == null)
                dragonSpine = dragonObj.AddComponent<BattleDragonSpine>();

            dragonSpine.SetData(data);
            data.SetTransform(dragonSpine.transform);

            return dragonSpine;
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
            var monsterSpine = monsterObj.GetComponent<BattleMonsterSpine>();
            if (monsterSpine == null)
                monsterSpine = monsterObj.AddComponent<BattleMonsterSpine>();

            monsterSpine.SetData(data);
            data.SetTransform(monsterSpine.transform);

            return monsterSpine;
        }
        protected override IEnumerator UpdateCO()
        {
            yield return null;

            StateMachine = new DailyMachine(this, Data);
            StateMachine.StateInit();
            StateMachine.StateDataClear();

            while (true)
            {
                if (StateMachine == null)
                    yield return null;

                if (!StateMachine.Update(SBGameManager.Instance.DTime))
                {
                    if (StateMachine.IsState<DailyStateStart>())
                    {
                        switch (Data.State)
                        {
                            case eBattleState.Playing:
                                StateMachine.ChangeState<DailyStateDragonMove>();
                                continue;
                            case eBattleState.Win:
                            case eBattleState.Lose:
                            case eBattleState.TimeOver:
                                StateMachine.ChangeState<DailyStateEnd>();
                                continue;
                            case eBattleState.None:
                            default:
                                break;
                        }
                    }
                    else if (StateMachine.IsState<DailyStateDragonMove>())
                    {
                        switch (Data.State)
                        {
                            case eBattleState.Playing:
                                StateMachine.ChangeState<DailyStateMonsterMove>();
                                continue;
                            case eBattleState.Win:
                            case eBattleState.Lose:
                            case eBattleState.TimeOver:
                                StateMachine.ChangeState<DailyStateEnd>();
                                continue;
                            case eBattleState.None:
                            default:
                                break;
                        }
                    }
                    else if (StateMachine.IsState<DailyStateMonsterMove>())
                    {
                        switch (Data.State)
                        {
                            case eBattleState.Playing:
                                StateMachine.ChangeState<DailyStateBattle>();
                                continue;
                            case eBattleState.Win:
                            case eBattleState.Lose:
                            case eBattleState.TimeOver:
                                StateMachine.ChangeState<DailyStateEnd>();
                                continue;
                            case eBattleState.None:
                            default:
                                break;
                        }
                    }
                    else if (StateMachine.IsState<DailyStateBattle>())
                    {
                        switch (Data.State)
                        {
                            case eBattleState.Playing:
                            case eBattleState.TimeOver:
                                StateMachine.ChangeState<DailyStateResult>();
                                continue;
                            case eBattleState.Win:
                            case eBattleState.Lose:
                                StateMachine.ChangeState<DailyStateEnd>();
                                continue;
                            case eBattleState.None:
                            default:
                                break;
                        }
                    }
                    else if (StateMachine.IsState<DailyStateResult>())
                    {
                        switch (Data.State)
                        {
                            case eBattleState.Playing:
                                StateMachine.ChangeState<DailyStateDragonMove>();
                                continue;
                            case eBattleState.Win:
                            case eBattleState.Lose:
                            case eBattleState.TimeOver:
                                StateMachine.ChangeState<DailyStateEnd>();
                                continue;
                            case eBattleState.None:
                            default:
                                break;
                        }
                    }
                    else if (StateMachine.IsState<DailyStateEnd>())
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
            LoadingManager.Instance.EffectiveSceneLoad("DailyReward", eSceneEffectType.CloudAnimation);
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

            switch (Data.World)
            {
                case 1:
                    SoundManager.Instance.PushBGM("BGM_WORLD_1_BATTLE", true);
                    break;
                case 2:
                    SoundManager.Instance.PushBGM("BGM_WORLD_2_BATTLE", true);
                    break;
                case 3:
                    SoundManager.Instance.PushBGM("BGM_WORLD_3_BATTLE", true);
                    break;
                case 4:
                    SoundManager.Instance.PushBGM("BGM_WORLD_4_BATTLE", true);
                    break;
                case 5:
                    SoundManager.Instance.PushBGM("BGM_WORLD_5_BATTLE", true);
                    break;
            }
        }
        protected override void OnPausePopup()
        {
            LoadingManager.Instance.Clear();
            PausePopup.OpenDailyPopup();
        }
        protected override void Destroy()
        {
            base.Destroy();
            if (Instance == this)
                Instance = null;
        }
    }
}