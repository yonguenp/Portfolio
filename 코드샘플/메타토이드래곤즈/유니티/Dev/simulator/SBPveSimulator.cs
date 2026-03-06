using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

#if DEBUG

namespace SandboxNetwork
{
    /*
     * DataManager 용처
     * simulator_log : 통상 시뮬 로그 (1판만 세팅)
     * simulator_global_log : 반복 모드용 전체 로그 데이터
     * simulator_wave : 현재 웨이브
     * simulator_world : 현재 입장 월드 인덱스
     * simulator_stage : 현재 입장 스테이지 인덱스
     * simulator_world_select : 결과 창에서 월드 선택했는지 플래그 용도
     * simulator_dragon_preset : 팀 세팅 창 드래곤 프리셋 사전 저장 데이터
     * simulator_dragon : 현재 탐험 드래곤 배열 인덱스
     * simulator_adventure_start_data : 탐험 시작 전 jobject 통 데이터
     * simulator_auto_flag : 반복 모드 켰는지 체크용
     * simulator_total_round : 반복 모드 시 전체 판 수
     * simulator_current_round : 반복 모드 현재 판 수
     */

    public class SBPveSimulator : MonoBehaviour, EventListener<SBSimulatorEvent>, EventListener<DragonChangedEvent>
    {
        [SerializeField] private SimulatorWorldStageController worldDropController = null;
        [SerializeField] private SBDragonEditDropdownController dropdownController = null;
        [SerializeField] private GameObject[] arrDragonParent = null;
        [SerializeField] private GameObject prefDragonSlot = null;
        [SerializeField] private Text labelMyBattlePoint = null;

        [SerializeField] GameObject editRoot = null;
        [SerializeField] GameObject stageRoot = null;

        [SerializeField] Button editbtn = null;
        [SerializeField] Button stagebtn = null;

        [SerializeField] List<PresetDragonSlot> presetSlotList = new List<PresetDragonSlot>();
        public List<PresetDragonSlot> PresetSlotList { get { return presetSlotList; } }

        private SimulatorBattleLine battleLine = new SimulatorBattleLine();
        public SimulatorBattleLine BattleLine { get { return battleLine; } }

        private void OnEnable()
        {
            EventManager.AddListener<SBSimulatorEvent>(this);
            EventManager.AddListener<DragonChangedEvent>(this);
        }
        private void OnDisable()
        {
            EventManager.AddListener<SBSimulatorEvent>(this);
            EventManager.RemoveListener<DragonChangedEvent>(this);
        }

        public void OnEvent(SBSimulatorEvent eventData)
        {
            switch(eventData.Event)
            {
                case SBSimulatorEvent.eSimulatorEventEnum.deleteDragonUI:
                {
                    var deleteIndex = eventData.battleLineIndex;
                    if(battleLine != null)
                    {
                        if (battleLine.MaxCount > deleteIndex)
                        {
                            var currentDragonTag = battleLine.GetDragon(deleteIndex);
                            if (currentDragonTag > 0)
                            {
                                User.Instance.DragonData.DeleteUserDragon(currentDragonTag);
                            }
                        }

                        battleLine.DeleteDragonByIndex(deleteIndex);

                        DrawTeamDragon();

                        dropdownController.RefreshDragonDrops(deleteIndex,0);
                    }
                }
                break;
                case SBSimulatorEvent.eSimulatorEventEnum.drawDragonUI:
                {
                    var dragonData = eventData.dragonData;
                    var battleIndex = eventData.battleLineIndex;

                    if(battleLine.MaxCount > battleIndex)
                    {
                        var currentDragonTag = battleLine.GetDragon(battleIndex);
                        if(currentDragonTag > 0)
                        {
                            User.Instance.DragonData.DeleteUserDragon(currentDragonTag);
                        }
                    }

                    var line = battleIndex / 2;
                    var pos = battleIndex % 2;

                    User.Instance.DragonData.AddUserDragon(dragonData.Tag, dragonData);

                    if(battleLine != null)
                    {
                        battleLine.AddDragonPosition(line,pos, dragonData.Tag);

                        DrawTeamDragon();

                        dropdownController.RefreshDragonDrops(battleIndex, dragonData.Tag);
                    }
                }
                break;
                case SBSimulatorEvent.eSimulatorEventEnum.refreshDragonData:
                {
                    DrawTeamDragon();//일단 드래곤UI 갱신만
                }
                break;
                case SBSimulatorEvent.eSimulatorEventEnum.refreshPartData:
                {

                }
                break;
                case SBSimulatorEvent.eSimulatorEventEnum.refreshPetData:
                {

                }
                break;
                case SBSimulatorEvent.eSimulatorEventEnum.dragonDataLoadStart:
                {
                    dropdownController.IsLoaded = true;
                }
                break;
                case SBSimulatorEvent.eSimulatorEventEnum.dragonDataLoadEnd:
                {
                    dropdownController.IsLoaded = false;
                }
                break;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            SimulatorLoger.MakeInstance();
            if (SimulatorLoger.FirstInit)
            {
                SimulatorLoger.FirstInit = false;
            }
            else
            {
                StopCoroutine("sceneReload");
                StartCoroutine("sceneReload");
            }
        }

        IEnumerator sceneReload()
        {
            while (true)
            {
                yield return SBDefine.GetWaitForSeconds(0.1f);

                SceneReload();
                yield break;
            }
        }

        void SceneReload()
        {
            DrawDragon();//드래곤 그리기 초기화
            initPresetDragonSlot();//프리셋 슬롯 battleLine 넣기
            initDropDownController();//드롭다운 초기화 - battleLine 넘겨야되서 draw 다음에 실행
            RefreshDragonPrevData();//simulator_dragon 타입 int[] 받아다가 있으면 갱신

            if (SimulatorLoger.WorldSelect)
            {
                onClickSelectTabButton(2);
                SimulatorLoger.WorldSelect = false;
            }
            else
                onClickSelectTabButton(1);//기본적으로 편집 모드 활성화
        }

        void RefreshDragonPrevData()
        {
            List<PresetDragon> dragonPresetList = SimulatorLoger.PresetDragons;

            dropdownController.IsLoaded = true;
            for (int i = 0; i < SimulatorLoger.Dragons.Length; i++)
            {
                var line = i / 2;
                var pos = i % 2;
                var dragonTag = SimulatorLoger.Dragons[i];

                if (dragonTag <= 0)
                    continue;

                if (battleLine != null)
                {
                    battleLine.AddDragonPosition(line, pos, dragonTag);
                    dropdownController.RefreshDragonDrops(i, dragonTag);

                    if (dragonPresetList != null && dragonPresetList.Count > i && dragonPresetList[i] != null)
                        PresetSlotList[i].RefreshPresetData(dragonPresetList[i], dragonTag);
                }
            }
            DrawTeamDragon();
            dropdownController.IsLoaded = false;
        }

        void InitSimulator()
        {
            DrawDragon();//드래곤 그리기 초기화

			DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory());
			Debug.Log(directory.FullName);

            initPresetDragonSlot();//프리셋 슬롯 battleLine 넣기

            initDropDownController();//드롭다운 초기화 - battleLine 넘겨야되서 draw 다음에 실행

            //기본적으로 편집 모드 활성화
            onClickSelectTabButton(1);
        }

        void initPresetDragonSlot()
        {
            if(presetSlotList == null || presetSlotList.Count <= 0)
            {
                return;
            }

            for(var i = 0; i< presetSlotList.Count;i++)
            {
                var slot = presetSlotList[i];
                if(slot == null)
                {
                    continue;
                }
                slot.SetBattleLine(battleLine);
            }
        }


        void initDropDownController()
        {
            if(dropdownController != null)
            {
                dropdownController.init(battleLine);
            }
        }

        void RemoveAllDragonPrefab()
        {
            var keyarr = arrDragonParent;
            if (keyarr == null || keyarr.Length <= 0) return;

            for (int i = 0; i < keyarr.Length; ++i)
            {
                if (keyarr[i] == null) return;
                SBFunc.RemoveAllChildrens(keyarr[i].transform);
            }
        }
        void DrawDragon()
        {
            battleLine.LoadBattleLine();
            DrawTeamDragon();
        }
        void DrawTeamDragon()
        {
            int myTotalBp = 0;
            RemoveAllDragonPrefab();
            int i = 0, l = 0, lineLimit = 2;
            while (l < 3)
            {
                int val = battleLine.GetDragon(l, i);
                UserDragon element = User.Instance.DragonData.GetDragon(val);


                var dragonSlot = Instantiate(prefDragonSlot, arrDragonParent[l].transform);
                dragonSlot.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                dragonSlot.transform.localEulerAngles = new Vector3(0, 0, 75);

                var characterSlotComp = dragonSlot.GetComponent<CharacterSlotFrame>();
                if (element != null)
                {
                    element.RefreshALLStatus();
                    myTotalBp += element.Status.GetTotalINF();
                    if (characterSlotComp != null)
                    {
                        characterSlotComp.SetDragonData(element.Tag, true, true, battleLine);
                        characterSlotComp.name = element.Tag.ToString();
                    }
                }
                else
                {
                    characterSlotComp.setEmptyData(l, i);
                }
                ++i;
                if (i >= lineLimit)
                {
                    i = 0;
                    ++l;
                }
            }
            labelMyBattlePoint.text = myTotalBp.ToString();
        }

		public void OnClickDirectory()
		{
			PopupManager.OpenPopup<SimulatorPresetPopup>();
		}

        public void onClickSelectTabButton(int _index)
        {
            bool isEdit = false;
            switch(_index)
            {
                case 1:
                    isEdit = true;
                    break;
                case 2:
                    isEdit = false;
                    break;
            }
            editRoot.SetActive(isEdit);
            editbtn.SetInteractable(!isEdit);

            stageRoot.SetActive(!isEdit);
            stagebtn.SetInteractable(isEdit);
            if (!isEdit) { worldDropController.init(); }
        }

        public void onClickSimulatorStartScene()
        {
            LoadingManager.Instance.EffectiveSceneLoad("Simulator_battle_start", eSceneEffectType.CloudAnimation);
        }

        public void OnEvent(DragonChangedEvent eventType)
        {
            DrawTeamDragon();
        }
    }
}

#endif