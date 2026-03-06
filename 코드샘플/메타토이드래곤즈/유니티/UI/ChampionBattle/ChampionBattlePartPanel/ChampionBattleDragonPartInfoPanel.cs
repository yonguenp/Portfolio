using Google.Impl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ChampionLeagueTable;


//장비 클릭 시 표시 되는 정보 패널
namespace SandboxNetwork
{
    public class ChampionBattleDragonPartInfoPanel : MonoBehaviour
    {
        const int MAX_SET_PART_EFFECT = 6;


        [SerializeField]
        GameObject dragonEquipLayer = null;//장비 레이어
        [SerializeField]
        PartSlotFrame partFrame = null;

        [SerializeField]
        Text partNameLabel = null;
        
        [SerializeField]
        Text statTypeLabel = null;
        [SerializeField]
        Text statAmountLabel = null;
        [SerializeField]
        Text reinforceStepLabel = null;

        [SerializeField]
        List<Sprite> partGradeTagList = new List<Sprite>();
        [SerializeField]
        Image gradeTagImageTarget = null;

        [SerializeField]
        GameObject equipButtonNode = null;
        [SerializeField]
        GameObject unequipButtonNode = null;

        [SerializeField]
        GameObject[] partOptionNodeList = null;

        [SerializeField]
        ChampionBattleDragonPartChangeEaterPanel partSlotEater = null;

        [SerializeField]
        GameObject leftParentNode = null;

        [SerializeField]
        GameObject rightParentNode = null;

        [SerializeField]
        List<GameObject> setOptionDescNode = new List<GameObject>();

        [SerializeField]
        Button equipButton = null;
        [SerializeField]
        Button unEquipButton = null;

        [SerializeField]
        Text[] settedOptions = null;
        [SerializeField]
        Text[] settedOptionValues = null;
        [SerializeField]
        Text[] noneSettedOptions = null;
        [SerializeField]
        Text fusionOption = null;

        private List<int> settedPartOptionList = null;
        private int settedFusionOption = 0;
        public ChampionDragon CurDragonData { get; private set; } = null;
        int dragonID = 0;//클릭 할 때의 드래곤 태그
        int partID = 0; //파츠 태그 정보
        int equipIndex = -1;

        bool isOpen = false;
        public bool IsOpen { get { return isOpen; } }

        public delegate void func();

        private func closeCallback;

        public func CloseCallback
        {
            set
            {
                if (value != null)
                {
                    closeCallback = value;
                }
            }
        }

        public void InitCurrentDragonData()
        {
            CurDragonData = null;
            if (PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().DragonTag != 0)//드래곤 태그값
            {
                var dragonTag = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().DragonTag;                
                CurDragonData = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;
                if (CurDragonData == null)
                {
                    Debug.Log("user Dragon is null");
                    return;
                }

                dragonID = dragonTag;
            }
        }

        /**
         * @param param //파츠 태그가 param으로 들어옴
         */

        public void ShowDetailInfo(int partID, int equipIndex, bool isRight = true, func CloseCallback = null)
        {
            CloseCallback = null;
            Transform parentTransform = null;
            parentTransform = isRight ? rightParentNode.transform : leftParentNode.transform;
            gameObject.transform.parent = parentTransform;
            gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

            
            if (partID < 0)
            {
                return;
            }

            if (gameObject.activeInHierarchy == false)
            {
                gameObject.SetActive(true);
            }

            settedPartOptionList = new List<int>();
            settedFusionOption = 0;

            InitCurrentDragonData();
            this.partID = partID;

            if(equipIndex < 0)
            {
                if (CurDragonData != null)
                {
                    equipIndex = CurDragonData.ChampionPart.Count;
                    for (int i = 0; i < 6; i++)
                    {
                        if (CurDragonData.GetPart(i) == null)
                        {
                            equipIndex = i;
                            break;
                        }
                    }

                    if (equipIndex >= CharExpData.GetSlotCountByDragonLevel((int)eDragonGrade.Legend, GameConfigTable.GetDragonLevelMax()))
                    {
                        //여기로 들어오면 안됨.
                        equipIndex = 0;
                    }
                }
            }
            this.equipIndex = equipIndex;

            RefreshUI();

            if (CloseCallback != null)
            {
                closeCallback = CloseCallback;
            }
            isOpen = true;
        }

        public void HideDetailInfo()
        {
            DragonPartEvent.HideInfoPanel();

            if (closeCallback != null)
            {
                closeCallback();
            }

            if (gameObject.activeInHierarchy == true)
            {
                gameObject.SetActive(false);
            }

            OnHidePartSlotEaterNode();//장비 슬롯 등록 버튼UI 도 끄기   
            isOpen = false;
        }

        //장비 태그를 기준으로 링크 상태 판단, -1은 연결되지 않음
        void RefreshPartEquipButtonUI()
        {
            unequipButtonNode.SetActive(gameObject.transform.parent == rightParentNode.transform);
            equipButtonNode.SetActive(gameObject.transform.parent == leftParentNode.transform);
        }

        bool IsPartMaxLevel()
        {
            return true;
        }

        void RefreshUI()
        {
            if (partID <= 0)
            {
                return;
            }

            RefreshPartEquipButtonUI();//장착 / 해제 버튼 귀속 상태에 따른 refresh
            RefreshPartDetailUI();//아이콘 및 장비 이름 강화 레벨 같은 UI 연결
        }

        void RefreshPartDetailUI()
        {
            var partData = new ChampionPart(partID);//파츠 원본 데이터
            if(equipIndex >= 0)
            {
                if(CurDragonData != null)
                {
                    var equipPartData = CurDragonData.GetPart(equipIndex);
                    if (equipPartData != null)
                        partData = equipPartData;
                }
            }

            if (partData == null)
            {
                return;
            }
            var partItemDesignData = partData.GetItemDesignData();//파츠 아이템 테이블 데이터
            var partDesignData = partData.GetPartDesignData();//파츠 테이블 데이터

            if (partItemDesignData == null || partDesignData == null)
            {
                Debug.Log("designData is null");
                return;
            }

            var partName = partItemDesignData.NAME;//파츠(장비) 이름
            var partLevel = partData.Reinforce;//강화 레벨
            var partOptionList = partData.SubOptionList;//key와 value

            
            if (partData.SubOptionList != null && partData.SubOptionList.Count > 0)
            {
                for (int i = 0; i < partData.SubOptionList.Count; i++)
                {
                    settedPartOptionList.Add(partData.SubOptionList[i].Key);
                }
            }

            if(partData.FusionStatKey > 0)
                settedFusionOption = partData.FusionStatKey;

            var partMainoptionType_str = partDesignData.STAT_TYPE;//파츠 타입 string
            var partMainoption_Amount = partData.GetValue();
            var partMainType = partDesignData.VALUE_TYPE;//per 인지 value인지

            partNameLabel.text = partName;

            if (partFrame != null)
                partFrame.SetPartSlotFrame(partData);

            if (gradeTagImageTarget != null)
                gradeTagImageTarget.sprite = partGradeTagList[partData.Grade()];

            statTypeLabel.text = GetStringByType(partMainoptionType_str, partMainType == "PERCENT");

            SetLabelByMainType(partMainType, statAmountLabel, partMainoption_Amount);

            var maxReinforceCount = PartReinforceData.GetMaxReinforceStep(partDesignData.GRADE);//현재 최대 강화 단계
            if (reinforceStepLabel != null)
                reinforceStepLabel.text = SBFunc.StrBuilder(StringData.GetStringByIndex(100001128), " ", partLevel, "/", maxReinforceCount);


            //강화 단계별 리스트 데이터 갱신
            RefreshPartOptionUI(partLevel, partDesignData.KEY, settedPartOptionList, settedFusionOption);
            RefreshSetDescLabel();//부옵 표시 UI 갱신
        }

        void SetLabelByMainType(string type, Text targetLabel, float text)
        {
            switch (type.ToUpper())
            {
                case "PERCENT":
                {
                    targetLabel.text = '+' + text.ToString() + '%';
                }
                break;
                case "VALUE":
                {
                    targetLabel.text = '+' + text.ToString();
                }
                break;
            }
        }

        void RefreshPartOptionUI(int partLevel, int ID, List<int> _settedOptionList, int fusion)
        {
            if (partOptionNodeList != null && partOptionNodeList.Length > 0)
            {
                var listCount = partOptionNodeList.Length;
                var availableSlot = PartBaseData.GetMaxReinforceSlotCount(ID);
                var checkType = "";
                for (var i = 0; i < listCount; i++)
                {
                    var CheckNode = partOptionNodeList[i];
                    int optionKey = 0;
                    if (_settedOptionList != null && _settedOptionList.Count > 0 && _settedOptionList.Count > i)
                    {
                        optionKey = _settedOptionList[i];

                        checkType = "open";
                    }
                    else
                    {
                        checkType = "none";
                    }
                    RefreshPartOptionUIByType(checkType, CheckNode, optionKey);
                }
            }

            if (fusion <= 0)
            {
                fusionOption.text = StringData.GetStringByStrKey("옵션선택팝업타이틀");
            }
            else
            {
                PartFusionData data = PartFusionData.Get(fusion);
                if (data != null)
                {
                    fusionOption.text = StringData.GetStringFormatByStrKey(data._DESC, data.VALUE_MAX + data.LEGEND_BONUS + (data.VALUE_REINFORCE * 3));
                }
                else
                {
                    fusionOption.text = StringData.GetStringByStrKey("옵션선택팝업타이틀");
                }
            }
        }

        void RefreshPartOptionUIByType(string checkType, GameObject targetNode, int optionKey = 0)
        {
            var childCount = targetNode.transform.childCount;
            if (childCount <= 0)
            {
                return;
            }

            var childNodeList = SBFunc.GetChildren(targetNode);

            GameObject visibleNode = null;
            for (var i = 0; i < childNodeList.Length; i++)
            {
                var node = childNodeList[i];
                var isRightNodeName = node.name == checkType;

                if (isRightNodeName)
                {
                    visibleNode = node;
                }

                node.SetActive(isRightNodeName);
            }

            if (visibleNode != null)//타입에 따라 세부 내용 세팅
            {
                switch (checkType)
                {
                    case "open":
                    {
                        if (optionKey <= 0)
                        {
                            return;
                        }
                        var data = SubOptionData.Get(optionKey);
                        var dataType = data.STAT_TYPE;
                        var valueType = data.VALUE_TYPE;
                        var typeStr = GetStringByType(dataType, valueType == "PERCENT");

                        float optionValue = data.VALUE_MAX;

                        var LabelNode = visibleNode.GetComponentInChildren<Text>();
                        if (LabelNode != null)
                        {
                            optionValue = (float)Math.Round((double)optionValue, 2);
                            var str = SBFunc.StrBuilder(typeStr, " +", optionValue);
                            if (valueType == "PERCENT")
                            {
                                str += "%";
                            }

                            LabelNode.text = str;
                        }
                    }
                    break;
                }
            }
        }

        string GetStringByType(string statTypeStr)
        {
            return StatTypeData.GetDescStringByStatType(statTypeStr);
        }

        string GetStringByType(string statTypeStr, bool isPer)
        {
            if (statTypeStr == "ATK_DMG_RESIS" && isPer == true)
            {
                return StringData.GetStringByStrKey("BASE_DMG_RESIS_PERCENT");
            }

            return StatTypeData.GetDescStringByStatType(statTypeStr, isPer);
        }

        void RefreshSetDescLabel()//세트 효과 표시
        {
            var partData = new ChampionPart(partID);//파츠 원본 데이터

            if (partData == null)
            {
                foreach(var obj in setOptionDescNode)
                {
                    if (obj == null)
                        continue;
                    obj.SetActive(false);
                }
                return;
            }

            var partSetList = PartSetData.GetAllOptionByGroup(partData.GetPartDesignData().SET_GROUP);
            bool isOnlySixPartSet = partSetList.Count == 1 && partSetList[0].NUM == ePartSetNum.SET_6;//6셋만 있을 때
            for(int i = 0; i< setOptionDescNode.Count; i++)
            {
                var obj = setOptionDescNode[i];
                if (obj == null)
                    continue;

                var descLabel = SBFunc.GetChildrensByName(obj.transform, "desc");
                var valueLabel = SBFunc.GetChildrensByName(obj.transform, "label");

                PartSetData data = null;

                bool isAvailable;
                if (partSetList == null || partSetList.Count < 1)
                {
                    isAvailable = false;
                }
                else if(partSetList.Count <= 1)
                {
                    if (isOnlySixPartSet)
                    {
                        isAvailable = !(i == 0);
                        data = i == 0 ? null : partSetList[0];
                    }
                    else
                    {
                        isAvailable = (i == 0);
                        data = i == 0 ? partSetList[0] : null;
                    }
                }
                else
                {
                    isAvailable = partSetList.Count > i;
                    data = partSetList[i];
                }

                obj.SetActive(true);
                descLabel.gameObject.SetActive(isAvailable);
                SetValueLabel(isAvailable, valueLabel.gameObject, data);
            }
        }

        void SetValueLabel(bool isAvailable,GameObject tartgetObj, PartSetData _setData)
        {
            if (!isAvailable)
                tartgetObj.GetComponent<Text>().text = "-";
            else
            {
                string preFix;
                string postFix;

                switch (_setData.VALUE_TYPE)
                {
                    case "PERCENT":
                        preFix = "";
                        postFix = "%";
                        break;
                    default:
                        preFix = "+";
                        postFix = "";
                        break;
                }

                tartgetObj.GetComponent<Text>().text = preFix + StatTypeData.GetDescStringByStatType(_setData.STAT_TYPE, _setData.VALUE_TYPE == "PERCENT") + " " + _setData.VALUE + postFix;
            }
        }

        /**
         * 1. 장착 비용 없음
         * 2. 해제 비용 있음 (장비 테이블 unequip_cost_num)
         * 3. 기존 장비에 또다른 장비를 씌울때 비용 없음(기존 장비 삭제 , 신규 장비 등록)
         * 4. 현재 드래곤에서(빈슬롯O) 다른 드래곤이 끼고 있는것 착용시 (다른 드래곤 장비 해제 비용 발생)
         * 5. 현재 드래곤에서 (풀슬롯) 다른 드래곤이 끼고 있는것 착용시 (2가지 중 뭐가 맞는지)
         * =>3번 조건 실행, 다른 드래곤 장비 해제 비용 발생)
         */
        public void OnClickEquipButton()
        {
            var dragonInfo = CurDragonData;
            if (dragonInfo == null)
            {
                Debug.Log("dragonData is null");
                return;
            }
            var inputSlot = dragonInfo.GetEmptySlotIndex();

            if (inputSlot < 0)//장착 슬롯이 가득 차 있을 경우 - 교체 팝업 띄우기
            {
                var slotOpenTotalCount = dragonInfo.GetCurrentSlotOpenCount();
                FullSlotState(slotOpenTotalCount);
            }
            else
            {
                RemainSlotState(inputSlot);
            }

        }

        void RemainSlotState(int inputSlot)
        {
            //드래곤에 장비 채우기
            var dragonInfo = CurDragonData;
            if (dragonInfo == null)
            {
                Debug.Log("dragonData is null");
                return;
            }

            List<SubOptionData> subs = new List<SubOptionData>();
            for (int i = 0; i < settedPartOptionList.Count; i++)
            {
                var id = settedPartOptionList[i];
                subs.Add(SubOptionData.Get(id));
            }

            PartFusionData fusion = PartFusionData.Get(settedFusionOption);

            var partData = new ChampionPart(partID, subs, fusion);

            dragonInfo.AddPart(equipIndex, partData, (data) =>
            {
                RefreshUI();
                HideDetailInfo();

                var equipLayer = dragonEquipLayer.GetComponent<SubLayer>();
                if (equipLayer != null)
                {
                    equipLayer.ForceUpdate();

                    DragonPartEvent.PlayEquipPartAnim(inputSlot);
                }
            });

        }

        //장착 슬롯이 가득 찼을 경우 유저에게 슬롯 갈아 끼울거냐고 제공
        void FullSlotState(int totalSlotCount)
        {
            OnShowPartSlotEaterNode(totalSlotCount);//파츠 교체시 표시 UI
        }

        //파츠 교체시 표시 UI
        void OnShowPartSlotEaterNode(int totalSlotCount)
        {
            DragonPartEvent.HideInfoPanel();

            if (partSlotEater == null)
            {
                return;
            }

            partSlotEater.OnShowPartSlotEaterNode(totalSlotCount, partID);

            if (gameObject.activeInHierarchy == true)//파츠 교체 UI 팝업 시 상세창 닫음
            {
                gameObject.SetActive(false);
            }
        }

        void OnHidePartSlotEaterNode()
        {
            if (partSlotEater == null)
            {
                return;
            }

            partSlotEater.OnHidePartSlotEaterNode();
        }

        void InventoryFullAlert()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077),
                () => {
                    PopupManager.OpenPopup<InventoryPopup>();
                },
                () => {   //나가기

                },
                () => {  //나가기

                }
            );
        }

        /**
         * //업데이트 기대해달라는 문구
         */
        public void OnClickExpectGameAlphaUpdate()
        {
            ToastManager.On(100000326);
        }


        
        public void OnClickUnEquipButton()
        {
            var dragonInfo = CurDragonData;
            if (dragonInfo == null)
            {
                Debug.Log("dragonData is null");
                return;
            }

            var partData = dragonInfo.GetPart(equipIndex);
            if(partData == null)
            {
                return;
            }

            dragonInfo.RemovePart(dragonID, equipIndex, (data) =>
            {
                RefreshUI();
                HideDetailInfo();

                var equipLayer = dragonEquipLayer.GetComponent<SubLayer>();
                if (equipLayer != null)
                {
                    equipLayer.ForceUpdate();
                }
            });
        }

        public void OnClickSetOption()
        {
            var popup = PopupManager.OpenPopup<OptionSelectPopup>();
            popup.SetData(OptionSelectPopup.OptionType.PartOption, dragonID, partID, equipIndex);
            popup.SetCallback(SelectComplete);
        }

        public void OnClickSetFusionOption()
        {
            var popup = PopupManager.OpenPopup<OptionSelectPopup>();
            popup.SetData(OptionSelectPopup.OptionType.PartFusion, dragonID, partID, equipIndex);
            popup.SetCallback(SelectCompleteFusion);
        }

        public void SelectComplete(List<int> _options)
        {
            settedPartOptionList = new List<int>(_options);

            if (gameObject.transform.parent == rightParentNode.transform)
            {
                //기존 젬 옵션 변경 
                //바로 addpart
                RemainSlotState(equipIndex);
            }

            RefreshUI();
        }

        public void SelectCompleteFusion(List<int> _options)
        {
            settedFusionOption = _options[0];

            if (gameObject.transform.parent == rightParentNode.transform)
            {
                //기존 젬 옵션 변경 
                //바로 addpart
                RemainSlotState(equipIndex);
            }

            RefreshUI();
        }
    }
}

