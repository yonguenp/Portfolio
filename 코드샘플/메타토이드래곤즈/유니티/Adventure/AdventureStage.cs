using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class AdventureStage : BattleStage
    {   
        private AdventureBattleData Data { get; set; } = null;
        public override List<List<BattleSpine>> PrevSpines { get; protected set; } = new List<List<BattleSpine>>();
        public static AdventureStage Instance { get; private set; } = null;

        protected override void Initialize()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;

            if (maps == null)
            {
                Debug.LogError("AdventureLog : Non Map");
                return;
            }

            Data = AdventureManager.Instance.Data;
            if (Data == null)
            {
                Debug.LogError("AdventureLog : Non Data");
                return;
            }

            var stageData = StageBaseData.GetByAdventureWorldStage(Data.World, Data.Stage);
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

            if (User.Instance.UserAccountData.UserNumber > 0)
                UIManager.Instance.InitUI(eUIType.Battle_Adventure);
            else
                UIManager.Instance.InitUI(eUIType.Battle_Simulator);

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
            var dragonSpine = dragonObj.GetComponent<AdventureDragonSpine>();
            if (dragonSpine == null)
                dragonSpine = dragonObj.AddComponent<AdventureDragonSpine>();

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

            StateMachine = new AdventureMachine(this, Data);
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
                    if (StateMachine.IsState<AdventureStateStart>())
					{
						switch (Data.State)
						{
							case eBattleState.Playing:
								StateMachine.ChangeState<AdventureStateDragonMove>();
								continue;
							case eBattleState.Win:
							case eBattleState.Lose:
							case eBattleState.TimeOver:
								StateMachine.ChangeState<AdventureStateEnd>();
								continue;
							case eBattleState.None:
							default:
                                break;
                        }
					}
                    else if (StateMachine.IsState<AdventureStateDragonMove>())
                    {
                        switch (Data.State)
                        {
                            case eBattleState.Playing:
                                StateMachine.ChangeState<AdventureStateMonsterMove>();
								continue;
                            case eBattleState.Win:
                            case eBattleState.Lose:
                            case eBattleState.TimeOver:
                                StateMachine.ChangeState<AdventureStateEnd>();
                                continue;
                            case eBattleState.None:
                            default:
                                break;
                        }
                    }
                    else if (StateMachine.IsState<AdventureStateMonsterMove>())
                    {
                        switch (Data.State)
                        {
                            case eBattleState.Playing:
                                StateMachine.ChangeState<AdventureStateBattle>();
                                continue;
                            case eBattleState.Win:
                            case eBattleState.Lose:
                            case eBattleState.TimeOver:
                                StateMachine.ChangeState<AdventureStateEnd>();
                                continue;
                            case eBattleState.None:
                            default:
                                break;
                        }
                    }
                    else if (StateMachine.IsState<AdventureStateBattle>())
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
                                     StateMachine.ChangeState<AdventureStateResult>();
                                continue;
                            case eBattleState.Win:
                            case eBattleState.Lose:
                                StateMachine.ChangeState<AdventureStateEnd>();
                                continue;
                            case eBattleState.None:
                            default:
                                break;
                        }
                    }
                    else if (StateMachine.IsState<AdventureStateResult>())
                    {
                        switch (Data.State)
                        {
                            case eBattleState.Playing:
								StateMachine.ChangeState<AdventureStateDragonMove>();
                                continue;
                            case eBattleState.Win:
                            case eBattleState.Lose:
                            case eBattleState.TimeOver:
                                StateMachine.ChangeState<AdventureStateEnd>();
                                continue;
                            case eBattleState.None:
                            default:
                                break;
                        }
                    }
                    else if (StateMachine.IsState<AdventureStateEnd>())
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
            if (AdventureManager.Instance.Data.World >= (int)eDailyDungeonWorldIndex.Mon) 
            { 
                //임시로 연결
                LoadingManager.Instance.EffectiveSceneLoad("DailyReward", eSceneEffectType.CloudAnimation);
            }
            else 
            { 
                if(User.Instance.UserAccountData.UserNumber < 0)
                    LoadingManager.Instance.EffectiveSceneLoad("AdventureSimulatorReward", eSceneEffectType.CloudAnimation);
                else
                    LoadingManager.Instance.EffectiveSceneLoad("AdventureReward", eSceneEffectType.CloudAnimation);
            }
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
            
            SoundManager.Instance.PushBGM("BGM_WORLD_" + Data.World + "_BATTLE", true);            
        }
        
        protected override void OnPausePopup()
        {
            LoadingManager.Instance.Clear();
            PausePopup.OpenAdventurePopup();
        }
        public bool IsState<T>() where T : BattleState
        {
            if (StateMachine == null)
                return false;

            return StateMachine.IsState<T>();
        }
        protected override void Destroy()
        {
            base.Destroy();
            Data = null;
            if (Instance == this)
                Instance = null;
        }
    }
}