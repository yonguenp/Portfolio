using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if DEBUG
namespace SandboxNetwork
{
    public class SimulatorWorldStageController : MonoBehaviour
    {
        //전투 결과 -> 스테이지 변경 시에 값 들고있어야되나?
        int selectWorld = 1;
        int selectStage = 1;

        [SerializeField] TMPro.TMP_Dropdown worldDrop = null;
        [SerializeField] TMPro.TMP_Dropdown stageDrop = null;

        [SerializeField] SBPveSimulator parent = null;

        [SerializeField] Toggle gohomeModeToggle = null;

        [SerializeField] TMP_InputField inputfield = null;

        StageTable stageTable = null;

        int currentMaxWorldIndex;

        Dictionary<int, int> worldDic = new Dictionary<int, int>();//worldindex(1~4), stageCount(1~8)

        bool isInit = false;

        int gohomeRound = 0;
        bool gohomeFlag = false;

        JObject BattleInfo = new JObject();
        public void init()
        {
            if (isInit)
            {
                return;
            }

            isInit = true;

            if(stageTable == null)
            {
                stageTable = TableManager.GetTable<StageTable>();
            }

            if (inputfield != null)
                inputfield.onEndEdit.AddListener(onEndEdit);

            var stageInfoList = stageTable.GetNormalStage();
            stageInfoList.Sort((d1,d2) => d2.WORLD - d1.WORLD);//world 인덱스 내림차순

            currentMaxWorldIndex = stageInfoList[0].WORLD;

            if (worldDic == null)
            {
                worldDic = new Dictionary<int, int>();
            }

            worldDic.Clear();

            for(var i = 1; i<= currentMaxWorldIndex; i++ )
            {
                var stageCount = StageTable.GetWorldStageCount(i);

                worldDic.Add(i, stageCount);
            }

            gohomeRound = 0;

            SetWorldDropData();
            RefreshWorldDrop();

            SetStageDropData();
            RefreshStageDrop();

            RefreshToggleValue();//토글값 갱신
        }

        void SetWorldDropData()
        {
            if(worldDrop == null)
            {
                return;
            }

            worldDrop.ClearOptions();

            var keys = worldDic.Keys.ToList();
            List<string> keyStr = keys.Select(i => i.ToString()).ToList();

            List<string> strOptions = new List<string>();

            for(var i = 0; i< keyStr.Count; i++)
            {
                var key = keyStr[i];

                strOptions.Add(key);
            }

            worldDrop.AddOptions(strOptions);
        }

        void RefreshWorldDrop()
        {
            var options = worldDrop.options;
            if(options == null || options.Count <= 0)
            {
                return;
            }

            string dropText = "";
            int dropValue = 0;

            if (SimulatorLoger.World > 0)
            {
                selectWorld = SimulatorLoger.World;
                SimulatorLoger.World = -1;
            }

            for (var i = 0; i < options.Count; i++)
            {
                var text = options[i].text;

                if(int.Parse(text) == selectWorld)
                {
                    dropValue = i;
                    dropText = text;
                    break;
                }
            }

            worldDrop.captionText.text = dropText;
            worldDrop.value = dropValue;
        }

        public void onClickWorldDrop()
        {
            var selectedText = worldDrop.captionText.text;
            selectWorld = int.Parse(selectedText);

            //Debug.Log("selectWorld : " + selectWorld);
        }

        void SetStageDropData()
        {
            if (stageDrop == null)
            {
                return;
            }

            stageDrop.ClearOptions();

            int stageCount = 8;
            if (worldDic.ContainsKey(selectWorld))
            {
                stageCount = worldDic[selectWorld];
            }

            List<string> strOptions = new List<string>();

            for (var i = 0; i < stageCount; i++)
            {
                var index = (i + 1).ToString();
                strOptions.Add(index);
            }

            stageDrop.AddOptions(strOptions);
        }

        void RefreshStageDrop()
        {
            var options = stageDrop.options;
            if (options == null || options.Count <= 0)
            {
                return;
            }

            string dropText = "";
            int dropValue = 0;

            if (SimulatorLoger.Stage > 0)
            {
                selectStage = SimulatorLoger.Stage;
                SimulatorLoger.Stage = -1;
            }

            for (var i = 0; i < options.Count; i++)
            {
                var text = options[i].text;
                if (int.Parse(text) == selectStage)
                {
                    dropValue = i;
                    dropText = text;
                    break;
                }
            }

            stageDrop.captionText.text = dropText;
            stageDrop.value = dropValue;
        }

        public void onClickStageDrop()
        {
            var selectedText = stageDrop.captionText.text;
            selectStage = int.Parse(selectedText);

            //Debug.Log("selectStage : " + selectStage);
        }

        public void onClickGotoBattle()
        {
            if(parent != null && parent.BattleLine.IsDeckEmpty())
            {
                ToastManager.On("덱을 먼저 세팅 해주세요!");
                return;
            }

            if(stageTable.GetByWorldStage(selectWorld, selectStage) == null)
            {
                ToastManager.On("유효한 월드 및 스테이지가 아닙니다" + "선택 월드 : " + selectWorld + " 선택 스테이지 : " + selectStage);
                return;
            }

            if (selectWorld <= 0)
                selectWorld = 1;
            if (selectStage <= 0)
                selectStage = 1;

            if(isCheckGoHomeMode())
            {
                SystemPopup.OnSystemPopup("퇴근 모드", gohomeRound + "판 만큼 반복 전투를 수행할까요?",
                    () => {
                        SimulatorLoger.TotalRound = gohomeRound;
                        SimulatorLoger.CurRound = 1;//최초 입장 시 1판 세팅
                        SimulatorLoger.AutoPlay = true;

                        JustOnceProcess();
                    }, 
                    () => {
                    },
                    () => {
                    }
                );
            }
            else
            {
                SimulatorLoger.AutoPlay = false;
                JustOnceProcess();
            }
        }

        bool isCheckGoHomeMode()
        {
            if (gohomeFlag && gohomeRound > 0)
                return true;
            return false;
        }

        void JustOnceProcess()
        {
            BattleInfo = new JObject();
            BattleInfo.Add("world", selectWorld);
            BattleInfo.Add("stage", selectStage);
            BattleInfo.Add("state", 1);//Playing
            BattleInfo.Add("maxTime", 180);//total Time
            BattleInfo.Add("wave", 1);//wave
            BattleInfo.Add("tag", 1);//서버 기록용 태그-> 확인 할 것

            SetPlayerData();
            SetMonsterData();//몬스터 데이터 세팅
            SaveDragonPreset();//드래곤 프리셋 상태 저장

            SimulatorLoger.World = selectWorld;
            SimulatorLoger.Stage = selectStage;
            SimulatorLoger.Wave = 1;
            SimulatorLoger.BattleInfo = BattleInfo;

            AdventureManager.Instance.SetSimulatorStartData(selectWorld, selectStage, BattleInfo);
            var temp = LoadingManager.Instance;
            LoadingManager.Instance.EffectiveSceneLoad("AdventureBattle", eSceneEffectType.CloudAnimation);
        }

        void SetPlayerData()
        {
            if(parent == null || parent.BattleLine == null)
            {
                return;
            }

            int[] convertArray = new int[6];
            var length = parent.BattleLine.MaxCount;

            JArray rawData = new JArray();
            for (int i = 0; i < 3; i++)
            {
                JArray colData = new JArray();
                JObject tagData = null;
                for (int k = 0; k < 2; k++)
                {
                    tagData = new JObject();

                    var convertIndex = i * 2 + k;
                    var index = i * 2 + 1 - k;
                    var currentDtag = parent.BattleLine.GetDragon(index);

                    if(currentDtag > 0)
                    {
                        tagData.Add("dtag", currentDtag);
                        tagData.Add("btag", index + 1);

                        colData.Add(tagData);
                    }
                    convertArray[convertIndex] = currentDtag;
                }

                rawData.Add(colData);
            }

            BattleInfo.Add("player", rawData);

            SimulatorLoger.Dragons = convertArray;
        }

        //wave 데이터 (몬스터 스폰 데이터 생성 규칙)
        /*
         * MonsterSpawnTable 을 기반으로 세팅
         * btag = 50 (monsterKey) + group 인덱스
         * type = 15 //몬스터 타입
         * grp = group 인덱스
         * id = monster_Spawn 테이블의 key값
         */

        void SetMonsterData()
        {
            if (BattleInfo == null)
                return;

            var enemyData = MakeMonsterJobject(selectWorld, selectStage, 1);//첫 웨이브는 1로

            if (enemyData != null)
                BattleInfo.Add("enemy", enemyData);
        }

        JToken MakeMonsterJobject(int worldIndex, int stageIndex, int waveIndex)
        {
            var stageInfo = TableManager.GetTable<StageTable>().GetByWorldStage(worldIndex, stageIndex);
            var stageSpawnKey = stageInfo.SPAWN;//몬스터 스폰키

            var monsterSpawnData = TableManager.GetTable<MonsterSpawnTable>().GetBySpawnGroup(stageSpawnKey);//현재 스폰 데이터

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

        void SaveDragonPreset()
        {
            if (parent == null)
                return;

            var presetList = parent.PresetSlotList;
            if (presetList == null || presetList.Count <= 0)
                return;

            List<PresetDragon> tempPresetList = new List<PresetDragon>();
            tempPresetList.Clear();

            for(int i = 0; i < presetList.Count; i++)
            {
                var presetComp = presetList[i];
                if (presetComp == null)
                    continue;

                var presetData = presetComp.Preset;
                tempPresetList.Add(presetData);
            }

            SimulatorLoger.PresetDragons = tempPresetList;
        }
        public void onClickChangeToggleValue()
        {
            if (gohomeModeToggle == null)
                return;

            gohomeFlag = gohomeModeToggle.isOn;

            Debug.Log(gohomeFlag);
        }

        void RefreshToggleValue()
        {
            if (gohomeModeToggle == null)
                return;

            if(SimulatorLoger.AutoPlay)
            {
                gohomeModeToggle.isOn = true;
            }
            else
            {
                gohomeModeToggle.isOn = false;
            }
        }

        public void onEndEdit(string _endText)
        {
            if(int.TryParse(_endText, out int result))
            {
                gohomeRound = result;
            }
            else
            {
                gohomeRound = 0;
                ToastManager.On("숫자(정수)가 아닙니다!");
                return;
            }
        }
    }
}
#endif
