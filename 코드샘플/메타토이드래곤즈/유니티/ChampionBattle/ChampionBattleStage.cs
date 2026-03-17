using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ChampionBattleStage : BattleStage
    {
        [SerializeField]
        protected BattleMap map = null;
        [SerializeField]
        private Canvas canvas = null;
        public override IBattleMap Map { get => map; }
        public override List<List<BattleSpine>> PrevSpines { get; protected set; } = null;

        private Coroutine failCoroutine = null;
        private const int maxFailCount = 3;
        private bool isNetworkState = false;
        private int failCount = 0;

        protected override void Initialize()
        {
            UIManager.Instance.InitUI(eUIType.Battle_ChampionBattle);
            isNetworkState = false;

            base.Initialize();

            SBGameManager.Instance.SetFixedDeltaTime(true);

            InitStateMachine();
            
            failCount = 0;
        }

        protected virtual void InitStateMachine()
        {
            if (StateMachine == null)
            {
                StateMachine = new ChampionBattleColosseumMachine(this, ChampionManager.Instance.ChampionData);
                StateMachine.SetState();
            }

            StateMachine.ChangeState<ChampionBattleColosseumStart>();
        }

        protected virtual void UpdateState()
        {
            if (!StateMachine.Update(SBGameManager.Instance.DTime))
            {
                if (StateMachine.IsState<ChampionBattleColosseumStart>() && StateMachine.ChangeState<ChampionBattleColosseumBattle>())
                {
                    return;
                }
                else if (StateMachine.IsState<ChampionBattleColosseumBattle>() && StateMachine.ChangeState<ChampionBattleColosseumEnd>())
                {
                    RoundWinnerEffect();
                    return;
                }
                else if (StateMachine.IsState<ChampionBattleColosseumEnd>())
                {
                    BattleEnd();
                    StateMachine = null;
                }
            }
        }

        private void FixedUpdate()
        {
            if (StateMachine == null)
                return;

            UpdateState();
        }
        protected override IEnumerator UpdateCO()
        {
            yield return null;
            //while (true)
            //{
            //    if (StateMachine == null)
            //        yield return null;

            //    yield return new WaitForFixedUpdate();

            //    if (!StateMachine.Update(SBGameManager.Instance.DTimeFixed))
            //    {
            //        if (StateMachine.IsState<ChampionBattleColosseumStart>() && StateMachine.ChangeState<ChampionBattleColosseumBattle>())
            //        {
            //            yield return null;
            //            continue;
            //        }
            //        else if (StateMachine.IsState<ChampionBattleColosseumBattle>() && StateMachine.ChangeState<ChampionBattleColosseumEnd>())
            //        {
            //            yield return null;
            //            continue;
            //        }
            //        else if (StateMachine.IsState<ChampionBattleColosseumEnd>())
            //        {
            //            BattleEnd();
            //            yield break;
            //        }
            //    }
            //    yield return null;
            //}
        }
        protected virtual void BattleResultFail(string args)
        {
#if DEBUG
            Debug.LogError(SBFunc.StrBuilder("BattleResultFail => ", args));
#endif
            if (failCount >= maxFailCount)
            {
                var popup = SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByIndex(100002661), true, false, false);
                popup.SetCallBack(() =>
                {
                    failCount = maxFailCount - 1;

                    if (failCoroutine != null)
                    {
                        StopCoroutine(failCoroutine);
                        failCoroutine = null;
                    }
                    failCoroutine = StartCoroutine(BattleEndFail());
                });
                popup.SetBackBtn(false);
                popup.SetDimd(false);
                return;
            }

            if (failCoroutine != null)
            {
                StopCoroutine(failCoroutine);
                failCoroutine = null;
            }
            failCoroutine = StartCoroutine(BattleEndFail());
        }
        protected virtual IEnumerator BattleEndFail()
        {
            yield return SBDefine.GetWaitForSecondsRealtime(1f);
            isNetworkState = false;
            failCount++;
            BattleEnd();
            yield break;
        }
        protected override void BattleEnd()
        {
            if (PopupManager.IsPopupOpening(PopupManager.GetPopup<ChampionBattleStatisticPopup>()))
                return;

            PopupManager.OpenPopup<ChampionBattleStatisticPopup>(new ChampionBattleStatisticPopupData(ChampionManager.Instance.ChampionData, ()=> {
                ChampionManager.Instance.OnRoundEnd();
            }));
        }
        public override BattleSpine GetOffenseSpine(IBattleCharacterData data)
        {
            var spine = GetSpine(data, Map.OffenseBeacon.transform);

            var x = Mathf.FloorToInt((data.Position - (int)eChampionBattlePos.TEAM1) / 2);
            while(OffenseSpines.Count <= x)
            {
                OffenseSpines.Add(new());
            }
            OffenseSpines[x].Add(spine);

            return spine;
        }

        public override BattleSpine GetDefenseSpine(IBattleCharacterData data)
        {
            var spine = GetSpine(data, Map.DefenseBeacon.transform);

            var x = Mathf.FloorToInt((data.Position - (int)eChampionBattlePos.TEAM2) / 2);
            while (DefenseSpines.Count <= x)
            {
                DefenseSpines.Add(new());
            }
            DefenseSpines[x].Add(spine);

            return spine;
        }

        protected BattleSpine GetSpine(IBattleCharacterData data, Transform parent)
        {
            var baseData = data.BaseData as CharBaseData;
            if (baseData == null)
                return null;

            var dragonPrefab = baseData.GetDefaultSpine();
            if (dragonPrefab == null)
                return null;

            var dragonObj = Instantiate(dragonPrefab, parent);
            dragonObj.SetActive(true);
            var dragonSpine = dragonObj.GetComponent<ChampionBattleDragonSpine>();
            if (dragonSpine == null)
                dragonSpine = dragonObj.AddComponent<ChampionBattleDragonSpine>();

            var spineObj = dragonObj.transform.Find("spine");
            if (spineObj != null)
                spineObj.localPosition = new Vector3(0f, 12f, 0f);
            var shadowObj = dragonObj.transform.Find("shadow");
            if (shadowObj != null)
                shadowObj.localScale = Vector3.zero;

            dragonSpine.SetData(data);
            data.SetTransform(dragonSpine.transform);

            if (dragonSpine.Data.PetID > 0)
            {
                SetPet(PetBaseData.Get(dragonSpine.Data.PetID), dragonSpine);
            }

            return dragonSpine;
        }
        protected void SetPet(PetBaseData petData, BattleSpine dragonSpine)
        {
            if (petData == null || dragonSpine == null)
                return;

            var prefab = SBFunc.GetPetSpineByName(petData.IMAGE);
            if (prefab != null)
            {
                var petNode = Instantiate(prefab, Map.Beacon);
                var petSkin = petNode.GetComponent<PetSpine>();
                if (petSkin != null)
                    petSkin.SetData(petData);

                var follow = petNode.GetComponent<FollowSpine>();
                if (follow == null)
                    follow = petNode.AddComponent<FollowSpine>();

                follow.Set(dragonSpine.SpineTransform, dragonSpine.transform, dragonSpine.Data, new Vector3(-0.2f, -0.01f, 0f), true);
                dragonSpine.SetPet(follow);
            }
        }
        public override void SetGreenHpBar(IBattleCharacterData data)
        {
            SetHpBar(canvas, hpBarGreenPrefab, data);
        }
        public override void SetRedHpBar(IBattleCharacterData data)
        {
            SetHpBar(canvas, hpBarRedPrefab, data);
        }

        protected override void OnPausePopup()
        {
            LoadingManager.Instance.Clear();
            PausePopup.OpenArenaPopup();
        }
        protected override void Destroy()
        {
            base.Destroy();
            if (failCoroutine != null)
            {
                StopCoroutine(failCoroutine);
                failCoroutine = null;
            }
            failCount = 0;
        }

        protected virtual void RoundWinnerEffect()
        {
            UIBattleStateEndEvent.Send();
        }
    }
}