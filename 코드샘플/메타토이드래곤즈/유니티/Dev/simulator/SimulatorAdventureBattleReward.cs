using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if DEBUG
namespace SandboxNetwork
{
    public class SimulatorAdventureBattleReward : AdventureBattleReward
    {
        [SerializeField]
        private SimulatorDamageLogPanel logPanel = null;

        JObject totalData = new JObject();

        protected override void Start()
        {
            base.Start();

            nodeBtnNext.gameObject.SetActive(false);

            if (logPanel != null)
                logPanel.SetDamageLog();

            initAutoButton();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        void initAutoButton()
        {
            if (SimulatorLoger.AutoPlay)
            {
                var totalCount = SimulatorLoger.TotalRound;
                var currentCount = SimulatorLoger.CurRound;

                if (totalCount <= currentCount)//반복전투 종료
                {
                    HideAutoButton();
                }
                else
                {
                    ShowAutoButton();
                    StartCoroutine(AutoScheduler());
                }
            }
            else
            {
                HideAutoButton();
            }
        }

        public override void OpenAutoAdventurePopup()
        {
            isAuto = false;
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000781),
            () => {
                HideAutoButton();
                StageManager.ClearAccumData();
            },
            () => {
                //카운터 코루틴 재시작
                StartCoroutine(AutoScheduler());
            },
            () => {
                //카운터 코루틴 재시작
                StartCoroutine(AutoScheduler());
            });
        }
        void HideAutoButton()
        {
            if (autoButtonNode.activeInHierarchy)
            {
                autoButtonNode.SetActive(false);
            }
        }
        void ShowAutoButton()
        {
            if (autoButtonNode.activeInHierarchy == false)
            {
                autoButtonNode.SetActive(true);
            }

            int remainCnt = SimulatorLoger.CurRound;
            int maxCnt = SimulatorLoger.TotalRound;
            autoCountCompLabel.text = string.Format("{0}/{1}", maxCnt - remainCnt, maxCnt);
            LayoutRebuilder.ForceRebuildLayoutImmediate(autoCountCompLabel.transform.parent.GetComponent<RectTransform>());
        }
        protected override IEnumerator AutoScheduler()
        {
            isAuto = true;
            while (AutoPlayingTimer > 0 && isAuto)
            {
                AutoPlayingTimer -= Time.deltaTime;
                autoButtonLabel.text = ((int)AutoPlayingTimer).ToString();
                yield return null;
            }
            if (AutoPlayingTimer <= 0 && isAuto)
            {
                onClickSimulatorRetry();
                isAuto = false;
                autoButtonLabel.text = "0";
            }
        }
        public void onClickSimulatorRetry()
        {
            if(SimulatorLoger.AutoPlay)
            {
                SimulatorLoger.CurRound = SimulatorLoger.CurRound + 1;
            }

            var selectWorld = SimulatorLoger.World;
            var selectStage = SimulatorLoger.Stage;
            /*
             * onClickBattleStart : 244
             * 서버에서 받아오는 구조 분석해서 주작데이터 세팅법 제작하기
             * 유저 데이터(드래곤, 펫, 장비) 어떻게 세팅하고 있는지 확인
             */

            totalData = new JObject();
            totalData.Add("world", selectWorld);
            totalData.Add("stage", selectStage);
            totalData.Add("state", 1);//Playing
            totalData.Add("maxTime", 180);//total Time
            totalData.Add("wave", 1);//wave
            totalData.Add("tag", 1);//서버 기록용 태그-> 확인 할 것

            SetPlayerData();//현재 드래곤 데이터 유지되는지 확인
            SetMonsterData(selectWorld, selectStage);//몬스터 데이터 세팅

            SimulatorLoger.Wave = 1;

            AdventureManager.Instance.SetSimulatorStartData(selectWorld, selectStage, totalData);
            var temp = LoadingManager.Instance;
            LoadingManager.Instance.EffectiveSceneLoad("AdventureBattle", eSceneEffectType.CloudAnimation);
        }

        void SetPlayerData()
        {
            JArray rawData = new JArray();
            for (int i = 0; i < 3; i++)
            {
                JArray colData = new JArray();
                JObject tagData = null;
                for (int k = 0; k < 2; k++)
                {
                    tagData = new JObject();

                    var index = i * 2 + k;
                    var currentDtag = SimulatorLoger.Dragons[index];

                    if (currentDtag > 0)
                    {
                        tagData.Add("dtag", currentDtag);
                        tagData.Add("btag", index + 1);

                        colData.Add(tagData);
                    }
                }

                rawData.Add(colData);
            }

            totalData.Add("player", rawData);
        }
        void SetMonsterData(int selectWorld, int selectStage)
        {
            if (totalData == null)
                return;

            var enemyData = MakeMonsterJobject(selectWorld, selectStage, 1);//첫 웨이브는 1로

            if (enemyData != null)
                totalData.Add("enemy", enemyData);
        }

        JToken MakeMonsterJobject(int worldIndex, int stageIndex, int waveIndex)
        {
            var stageInfo = StageBaseData.GetByWorldStage(worldIndex, stageIndex);
            var stageSpawnKey = stageInfo.SPAWN;//몬스터 스폰키

            var monsterSpawnData = MonsterSpawnData.GetBySpawnGroup(stageSpawnKey);//현재 스폰 데이터

            List<MonsterSpawnData> waveList = monsterSpawnData.FindAll(data => (data.WAVE == waveIndex));//같은 웨이브 데이터만 뽑아냄
            if (waveList == null || waveList.Count <= 0)
                return null;

            JArray rawData = new JArray();
            for (int i = 0; i < 3; i++)
            {
                JArray colData = new JArray();
                JObject tagData = null;

                List<MonsterSpawnData> rawDataList = waveList.FindAll(data => (data.POSITION == i + 1));//같은 포지션 데이터만 뽑아냄

                if (rawDataList == null || rawDataList.Count <= 0)
                {
                    rawData.Add(colData);
                    continue;
                }

                for (int k = 0; k < rawDataList.Count; k++)
                {
                    tagData = new JObject();

                    var rawDataInfo = rawDataList[k];

                    var position = rawDataInfo.POSITION;
                    var grp = rawDataInfo.GROUP;
                    var id = int.Parse(rawDataInfo.KEY.ToString());
                    var btag = grp + 50;
                    var type = 15;

                    tagData.Add("btag", btag);
                    tagData.Add("type", type);
                    tagData.Add("id", id);
                    tagData.Add("grp", grp);

                    colData.Add(tagData);
                }

                rawData.Add(colData);
            }

            return rawData;
        }

        public void onClickChangeTeamSetting()
        {
            SimulatorLoger.ClearLog();

            LoadingManager.Instance.EffectiveSceneLoad("pve_simulator", eSceneEffectType.CloudAnimation);
        }

        public void onClickChangeStage()
        {
            SimulatorLoger.ClearLog();

            SimulatorLoger.WorldSelect = true;
            LoadingManager.Instance.EffectiveSceneLoad("pve_simulator", eSceneEffectType.CloudAnimation);
        }
    }
}
#endif