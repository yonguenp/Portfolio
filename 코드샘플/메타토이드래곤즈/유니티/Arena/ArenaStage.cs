using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ArenaStage : BattleStage
    {
        [SerializeField]
        protected BattleMap map = null;
        [SerializeField]
        private Canvas canvas = null;
        public override List<List<BattleSpine>> PrevSpines { get; protected set; } = null;

        private Coroutine failCoroutine = null;
        private const int maxFailCount = 3;
        private bool isNetworkState = false;
        private int failCount = 0;

        protected override void Initialize()
        {
            UIManager.Instance.InitUI(eUIType.Battle_Arena);
            isNetworkState = false;
            
            Map = map;

            if (ArenaManager.Instance.IsRestrictedFightDataFlag)
            {
                if (maps == null)
                {
                    Debug.LogError("RestrictedAreaLog : Non Map");
                    return;
                }

                var stageData = StageBaseData.GetByWorldStage(ArenaManager.Instance.RestrictedDataSet.SlotID, 1, ArenaManager.Instance.RestrictedDataSet.Difficult);
                if (stageData == null)
                {
                    Debug.LogError("RestrictedAreaLog : Non StageData");
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
                    Debug.LogError("RestrictedAreaLog : Non FindMap");
                    return;
                }

                var mapObj = Instantiate(map, transform);
                if (mapObj == null)
                {
                    Debug.LogError("RestrictedAreaLog : Map Instantiate Failed");
                    return;
                }

                Map = mapObj.GetComponent<BattleMap>();
                if (Map == null)
                {
                    Debug.LogError("RestrictedAreaLog : AdventureMap Component Exist Failed");
                    return;
                }

            }
            base.Initialize();

            if (StateMachine == null)
            {
                StateMachine = new ArenaColosseumMachine(this, ArenaManager.Instance.ColosseumData);
                StateMachine.SetState();
            }
            StateMachine.ChangeState<ArenaColosseumStart>();
            failCount = 0;
        }


        protected override IEnumerator UpdateCO()
        {
            while (true)
            {
                if (StateMachine == null)
                    yield return null;

                if (!StateMachine.Update(SBGameManager.Instance.DTime))
                {
                    if (StateMachine.IsState<ArenaColosseumStart>() && StateMachine.ChangeState<ArenaColosseumBattle>())
                    {
                        yield return null;
                        continue;
                    }
                    else if (StateMachine.IsState<ArenaColosseumBattle>() && StateMachine.ChangeState<ArenaColosseumEnd>())
                    {
                        yield return null;
                        continue;
                    }
                    else if (StateMachine.IsState<ArenaColosseumEnd>())
                    {
                        BattleEnd();
                        yield break;
                    }
                }
                yield return null;
            }
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
            if (ArenaManager.Instance.ColosseumData.IsMock)
            {
                LoadingManager.Instance.EffectiveSceneLoad("ArenaResult", eSceneEffectType.CloudAnimation);
                return;
            }

            WWWForm param = new WWWForm();
            param.AddField("abort", 0);
            param.AddField("win", (int)ArenaManager.Instance.ColosseumData.WinType);

            var myDat = StatisticsMananger.Instance.GetDamageInfoString(true);
            param.AddField("statistic", myDat);
            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;

            NetworkManager.Send("arena/result", param, (JObject jsonData) =>
            {
                if (jsonData.ContainsKey("rs") && (int)jsonData["rs"]== (int)eApiResCode.OK)
                {
                    isNetworkState = false;
                    ArenaManager.Instance.ColosseumData.SetResultData(jsonData);
                    LoadingManager.Instance.EffectiveSceneLoad("ArenaResult", eSceneEffectType.CloudAnimation);
                }
                else
                {
                    BattleResultFail(jsonData.ToString());
                }
                //LoadingManager.ImmediatelySceneLoad("ArenaResult");

            }, BattleResultFail);
        }
        public override BattleSpine GetOffenseSpine(IBattleCharacterData data)
        {
            var spine = GetSpine(data, Map.OffenseBeacon.transform);

            var x = Mathf.FloorToInt((data.Position - (int)eArenaPos.TEAM1) / 2);
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

            var x = Mathf.FloorToInt((data.Position - (int)eArenaPos.TEAM2) / 2);
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
            var dragonSpine = dragonObj.GetComponent<ArenaDragonSpine>();
            if (dragonSpine == null)
                dragonSpine = dragonObj.AddComponent<ArenaDragonSpine>();

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
                var pet = User.Instance.PetData.GetPet(dragonSpine.Data.PetID);
                if (pet != null)
                    SetPet(pet.GetPetDesignData(), dragonSpine);
                else
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
    }
}