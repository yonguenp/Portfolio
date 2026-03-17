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
    public class ChampionDragonDetailPartInfoPanel : MonoBehaviour
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
        GameObject[] partOptionNodeList = null;


        [SerializeField]
        GameObject leftParentNode = null;

        [SerializeField]
        GameObject rightParentNode = null;

        [SerializeField]
        List<GameObject> setOptionDescNode = new List<GameObject>();

        [SerializeField]
        Text[] settedOptions = null;
        [SerializeField]
        Text[] settedOptionValues = null;
        [SerializeField]
        Text[] noneSettedOptions = null;
        [SerializeField]
        Text fusionOption = null;

        ChampionDragonDetailPopup ParentPopup { get { return PopupManager.GetPopup<ChampionDragonDetailPopup>(); } }

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
            CurDragonData = ParentPopup.Dragon;
            if(CurDragonData != null)
                dragonID = CurDragonData.Tag;
        }

        /**
         * @param param //파츠 태그가 param으로 들어옴
         */

        public void ShowDetailInfo(int partID, int equipIndex, bool isRight = true, func CloseCallback = null)
        {
            CloseCallback = null;
            
            if (partID < 0)
            {
                return;
            }

            if (gameObject.activeInHierarchy == false)
            {
                gameObject.SetActive(true);
            }

            settedPartOptionList = new List<int>();

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

            isOpen = false;
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

        string GetStringByType(string statTypeStr, bool per)
        {
            if (statTypeStr == "ATK_DMG_RESIS" && per == true)
            {
                return StringData.GetStringByStrKey("BASE_DMG_RESIS_PERCENT");
            }

            return StatTypeData.GetDescStringByStatType(statTypeStr, per);
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


    }
}

