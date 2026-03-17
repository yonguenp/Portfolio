using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace SandboxNetwork
{
    public class DragonPartCompoundPanel : MonoBehaviour
    {
        const int materialMaxCount = 5;//재물 등록 최대 갯수

        [Space(10)]
        [Header("compound Info")]
        [SerializeField]
        Button[] materialButtonList = null;//onClickMaterialButton 연결하기
        [SerializeField]
        Button compoundButton = null;

        [SerializeField]
        GameObject compoundDisableObject = null;

        [SerializeField]
        List<GameObject> compoundButtonVisibleList = new List<GameObject>();

        [SerializeField]
        Text successPercentLabel = null;
        [SerializeField]
        Text costLabel = null;

        [SerializeField]
        GameObject percentNode = null;
        [SerializeField]
        GameObject countLessNode = null;

        [Header("result Node")]
        [SerializeField]
        List<Sprite> gradeTagList = new List<Sprite>();
        [SerializeField]
        Image gradeTagImageTarget = null;
        [SerializeField]
        GameObject hiddenTag = null;

        [SerializeField]
        List<Sprite> expectBgList = new List<Sprite>();
        [SerializeField]
        Image expectBgTarget = null;
        [SerializeField]
        GameObject hiddenSlot = null;

        [SerializeField]
        SlotFrameController expectSlotController = null;

        


        [SerializeField]
        Button backBtn = null;




        List<int> compoundTagList = new List<int>();//합성 모드 선택 시 다중 체크 담기
        public List<int> CompoundTagList { get { return compoundTagList; }}

        int[] materialList = new int[5] { 0, 0, 0, 0, 0 };//재물 4칸 - 일단 0으로 채움 (선택된 재물의 태그값으로 세팅)
        bool isSufficientMaterial = false;
        float successPercent = 0;

        bool isOpen = false;
        public bool IsOpen { get { return isOpen; } }

        void OnDisable()
        {
            isOpen = false;
        }
        public void ShowCompoundPanel()
        {
            gameObject.SetActive(true);
            isOpen = true;
            Init();
        }

        public void HideCompoundPanel()
        {
            gameObject.SetActive(false);
        }

        public void Init()
        {
            InitMaterialList();//재료 리스트 초기화
            InitCompoundUI();
        }

        public void ForceUpdate()
        {
            Init();
        }

        public void PushCompoundList(int tag)
        {
            compoundTagList.Add(tag);
            PushMaterialTag(tag);
        }

        public void PopCompoundList(int tag)
        {
            var index = compoundTagList.IndexOf(tag);
            if (index > -1)
            {
                compoundTagList.RemoveAt(index);
            }
            PopMaterialTag(tag);
        }

        public void OnClickAutoSetPart()//일괄 넣기 버튼(합성) - 일괄 합성 기능으로 변경
        {
            PopupManager.OpenPopup<DragonPartAutoCompoundPopup>(new DragonPartAutoTabTypePopupData(0, 0, 0, this));

            //var isFullCheck = IsFullMaterialList();//재료칸 전부 찼는 지
            //if (isFullCheck)
            //{
            //    ToastManager.On(100001132);
            //    return;
            //}

            //DragonPartEvent.CompoundAutoSet();
        }

        void InitCompoundUI()
        {
            successPercent = 0;

            RefreshCompoundTotalUI();//일단 데이터 기반 ui 갱신
        }

        void InitMaterialList()
        {
            if (compoundTagList == null)
            {
                compoundTagList = new List<int>();
            }
            compoundTagList.Clear();

            for (var i = 0; i < materialList.Length; i++)
            {
                materialList[i] = 0;
            }
        }

        public int GetFrontTagByMaterialList()//재료 리스트 중에서 0이 아닌 가장 앞 태그를 가져옴 - 0번 인덱스가 빠질 수 있음
        {
            if (materialList == null || materialList.Length <= 0)
            {
                return -1;
            }

            var tempTag = 0;
            for (var i = 0; i < materialList.Length; i++)
            {
                var tag = materialList[i];

                if (tag > 0)
                {
                    tempTag = tag;
                    return tempTag;
                }
            }

            return tempTag;
        }

        PartMergeBaseData GetMergeBaseData()
        {
            var partData = User.Instance.PartData.GetPart(GetFrontTagByMaterialList());
            if (partData == null)
            {
                return null;
            }

            var partGrade = partData.Grade();

            var mergeDefaultData = PartMergeBaseData.GetDataByGrade(partGrade);
            if (mergeDefaultData == null || mergeDefaultData.Count <= 0)
            {
                return null;
            }
            return mergeDefaultData[0];
        }

        public void OnClickMaterialButton(string customEventData)//리스트에서 제거 및 해제
        {
            var clickIndex = int.Parse(customEventData);

            //리스트에서 제거
            var tempTag = materialList[clickIndex];
            if (tempTag <= 0)
            {
                return;
            }

            materialList[clickIndex] = 0;
            ReleaseCheckPartSlotByTag(tempTag, compoundTagList);

            RefreshCompoundTotalUI();
        }

        void ReleaseCheckPartSlotByTag(int tag, List<int> targetList)
        {
            //데이터 상에서도 지우기
            var index = targetList.IndexOf(tag);
            if (index > -1)
            {
                targetList.RemoveAt(index);
            }
        }

        public bool IsFullMaterialList()//재료 아이템 넣을 수 있는 최대 갯수 인지
        {
            var maxIndex = GetAvailablePushIndex();//전부 차있으면 -1 리턴
            return maxIndex < 0;
        }

        void PushMaterialTag(int tag)
        {
            var availableIndex = GetAvailablePushIndex();
            if (availableIndex < 0)
            {
                return;
            }

            materialList[availableIndex] = tag;

            RefreshCompoundTotalUI();
        }

        void PopMaterialTag(int tag)
        {
            var availableIndex = GetIndexByTag(tag);
            if (availableIndex < 0)
            {
                return;//태그를 찾지 못함 (버그)
            }

            materialList[availableIndex] = 0;

            RefreshCompoundTotalUI();
        }

        int GetIndexByTag(int tag)
        {
            var firstIndex = -1;
            if (materialList == null)
            {
                return firstIndex;
            }

            for (var i = 0; i < materialList.Length; i++)
            {
                var listTag = materialList[i];
                if (listTag == tag)
                {
                    firstIndex = i;
                    return firstIndex;
                }
            }

            return firstIndex;
        }

        int GetAvailablePushIndex()//materialList에서 index중 0이 가장 처음 오는 값
        {
            var firstIndex = -1;
            if (materialList == null)
            {
                return firstIndex;
            }

            for (var i = 0; i < materialList.Length; i++)
            {
                var listTag = materialList[i];
                if (listTag <= 0)
                {
                    firstIndex = i;
                    return firstIndex;
                }
            }

            return firstIndex;
        }

        public int GetRemainCount()//materialList 기준으로 추가로 등록 가능한 갯수
        {
            var remainCount = 0;

            if (materialList == null || materialList.Length <= 0)
            {
                return remainCount;
            }

            for (var i = 0; i < materialList.Length; i++)
            {
                var tag = materialList[i];
                var CheckTag = tag <= 0;
                if (CheckTag)
                {
                    remainCount++;
                }
            }

            return remainCount;
        }

        void RefreshCompoundTotalUI()//materialList 기준으로 다시 그리기
        {
            CalcCompoundSuccessPercent();//합성 성공 확률 계산
            RefreshExpectResultItemIcon();//합성 가능 결과 아이콘 갱신
            RefreshCompoundMaterial();//합성 재료 계산
            RefreshMaterialButtonUI();//합성 버튼 재료상태에 따라 갱신
            RefreshCompoundButtonUI();//현재 재료 플래그 & 재물 등록 요구치 계산해서 버튼 플래그 세팅
            RefreshVariableUI();//합성 확률 라벨 표시

            DragonPartEvent.RefreshList();//리스트 다시 그리기
        }

        void CalcCompoundSuccessPercent()
        {
            var remainCount = GetRemainCount();
            if (remainCount == materialMaxCount)
            {
                successPercent = 0;//초기 등록값은 0퍼
                return;
            }

            var basePercent = GetMergeBaseSuccessPercent();
            var reinforceBonusPercent = GetMergeReinforceBonusPercent();
            var equipAmountBonusPercent = GetMergeEquipAmountBonusPercent();

            successPercent = (float)Math.Round(basePercent + reinforceBonusPercent + equipAmountBonusPercent, 2);
        }

        float GetMergeBaseSuccessPercent()//현재 등급 베이스 확률
        {
            var partBaseData = GetMergeBaseData();
            float success_rate = 0;
            if (partBaseData == null)
            {
                return success_rate;
            }

            success_rate = partBaseData.RATE;
            return (float)Math.Round((success_rate / (float)SBDefine.MILLION * 100), 2);
        }

        float GetMergeReinforceBonusPercent()//강화 수치에 따른 추가 확률 리스트 전체 합산 데이터
        {
            var partData = User.Instance.PartData.GetPart(GetFrontTagByMaterialList());
            var partGrade = partData.Grade();
            var totalPartNum = 0;

            for (var i = 0; i < materialList.Length; i++)
            {
                var tag = materialList[i];
                if (tag <= 0)
                {
                    continue;
                }

                partData = User.Instance.PartData.GetPart(tag);
                if (partData == null)
                {
                    continue;
                }
                var tempPartLevel = partData.Reinforce;
                totalPartNum += tempPartLevel;
            }

            float rate = PartMergeReinforceBonusData.GetRateByGradeAndReinforceNum(partGrade, totalPartNum);

            if (rate < 0)
            {
                //console.log('합성 확률 데이터를 못찾음! 버그!');
                return 0;
            }
            else
            {
                return (float)Math.Round((rate / (float)SBDefine.MILLION * 100), 2);
            }
        }

        float GetMergeEquipAmountBonusPercent()//재료 장비 수에 따른 확률(최소 요구치 이후 추가 장비 갯수에 따라 확률 가산)
        {
            var partBaseData = GetMergeBaseData();
            if (partBaseData == null)
            {
                return 0;
            }
            var grade = partBaseData.GRADE;
            var baseNumber = partBaseData.BASE_NUM;//합성 최소 요구 갯수

            var RemainSlotCount = GetRemainCount();//현재 남아있는 슬롯 갯수
            var currentRegistCount = materialMaxCount - RemainSlotCount;//현재 슬롯 등록 갯수

            if (currentRegistCount >= baseNumber)
            {
                var extra_num = currentRegistCount - baseNumber;
                if (extra_num > 0)
                {
                    var rate = PartMergeEquipAmountBonusData.GetRateByGradeAndBonusAmountNum(grade, extra_num);
                    if (rate < 0)
                    {
                        //console.log('합성 확률 데이터를 못찾음! 버그!');
                        return 0;
                    }
                    else
                    {
                        return (float)Math.Round((rate / (float)SBDefine.MILLION * 100), 2);
                    }
                }
            }
            return 0;
        }

        void RefreshMaterialButtonUI()//현재 등록된 재물을 materialList 기준으로 재물 버튼에 씌우는 작업
        {
            if (materialButtonList == null || materialButtonList.Length <= 0)
            {
                return;
            }

            if (materialButtonList.Length != materialList.Length)
            {
                Debug.Log("기반 데이터의 길이가 서로 다름!");
                return;
            }

            for (var i = 0; i < materialButtonList.Length; i++)
            {
                var buttonNode = materialButtonList[i];
                if (buttonNode == null)
                {
                    continue;
                }

                var buttonTag = materialList[i];

                var isEmpty = (buttonTag <= 0);

                var slotComp = buttonNode.GetComponent<DragonPartCompoundSlot>();
                if (slotComp == null)
                    continue;

                slotComp.SetMaterialButtonUI(buttonTag, buttonNode.gameObject, isEmpty);
            }
        }

        void RefreshCompoundButtonUI()//장비 강화 재물 갯수 최소 조건 && 재료 충족 조건 만족시 활성화
        {
            if (compoundButton == null)
            {
                return;
            }

            var compoundCondition = IsCompoundCondition();
            foreach(var obj in compoundButtonVisibleList)
            {
                if (obj == null)
                    continue;
                obj.SetActive(compoundCondition);
            }

            compoundButton.SetButtonSpriteState(compoundCondition);
        }

        bool IsCompoundCondition()
        {
            var remainCount = GetRemainCount();//재물 등록 가능 잔여 갯수(4개의 슬롯에 다 차있으면 0 리턴)
            if (remainCount == materialMaxCount)
            {
                compoundButton.SetButtonSpriteState(false);
                return false;
            }
            var currentRegistCount = materialMaxCount - remainCount;//현재 등록된 재물 갯수
            var mergeBaseData = GetMergeBaseData();
            if (mergeBaseData == null)
            {
                return false;
            }
            var mergeMaterialMinCount = mergeBaseData.BASE_NUM;//합성을 하기 위한 재료 최소 요구치(메인 갯수 포함데이터)

            var isMaterialCondition = (currentRegistCount >= mergeMaterialMinCount);
            var isCompoundButtonInteractable = isMaterialCondition && isSufficientMaterial;

            return isCompoundButtonInteractable;
        }

        void RefreshVariableUI()//update 할때마다 변경되는 UI들 (성공확률 라벨 같은)
        {
            var defaultString = StringData.GetStringByIndex(100001175);

            if (successPercentLabel != null)
            {
                successPercentLabel.text = SBFunc.StrBuilder(Math.Round(successPercent, 2), "%");
            }
        }

        void RefreshExpectResultItemIcon()//현재 등급 상승 가능한 아이콘 호출
        {
            var partBaseData = GetMergeBaseData();
            if (partBaseData == null)
            {
                hiddenSlot.SetActive(true);
                hiddenTag.SetActive(true);
                compoundDisableObject.SetActive(true);
                return;
            }

            hiddenSlot.SetActive(false);
            hiddenTag.SetActive(false);

            var grade = partBaseData.GRADE;//현재 등급
            var modifyIndex = grade - 1;

            if(expectBgList.Count > modifyIndex && modifyIndex >= 0)
                expectBgTarget.sprite = expectBgList[grade - 1];
            if (gradeTagList.Count > grade && grade >= 0)
                gradeTagImageTarget.sprite = gradeTagList[grade];

            if (expectSlotController != null)
                expectSlotController.SetColor(grade + 1);

            bool isFullFillCount = partBaseData.BASE_NUM <= compoundTagList.Count;
            percentNode.SetActive(isFullFillCount);
            countLessNode.SetActive(!isFullFillCount);
            compoundDisableObject.SetActive(!isFullFillCount);
        }

        void RefreshCompoundMaterial()//합성에 필요한 재료 슬롯 갱신 부분
        {
            var currentMergeBaseData = GetMergeBaseData();
            if (currentMergeBaseData == null)
            {
                costLabel.text = "-";
                costLabel.color = Color.white;
                return;
            }

            var cost_type = currentMergeBaseData.COST_TYPE;
            var cost_num = currentMergeBaseData.COST_NUM;

            var goldSufficient = true;
            var itemSufficient = true;

            if (cost_num > 0)
                goldSufficient = User.Instance.GOLD >= cost_num;

            isSufficientMaterial = (goldSufficient && itemSufficient);
            costLabel.text = SBFunc.CommaFromNumber(cost_num);
            costLabel.color = goldSufficient ? Color.white : Color.red;
        }

        public void OnClickCompoundButton()//합성 요청
        {
            if (!IsCompoundCondition())
            {
                ToastManager.On(100001130);
                return;
            }

            //0인 재료는 제외처리
            List<int> currentSelectedMaterialList = new List<int>();
            currentSelectedMaterialList.Clear();

            for (var i = 0; i < materialList.Length; i++)
            {
                var tagValue = materialList[i];
                if (tagValue <= 0)
                {
                    continue;
                }

                currentSelectedMaterialList.Add(tagValue);
            }

            var matArr = currentSelectedMaterialList.ToArray();

            var param = new WWWForm();
            param.AddField("materials", JsonConvert.SerializeObject(matArr));

            NetworkManager.Send("part/merge", param, (jsonObj) =>
            {
                var data = jsonObj;
                var isSuccess = (data["err"].Value<int>() == 0);
                var rs = (eApiResCode)data["rs"].Value<int>();

                switch (rs)
                {
                    case eApiResCode.OK:
                    {
                        if (isSuccess)
                        {
                            var partTag = data["tag"].Value<int>();

                            List<Asset> rewards = new List<Asset>();
                            Asset reward = new Asset(partTag);
                            rewards.Add(reward);
                            ShowCompoundResultPopup(rewards);
                            SoundManager.Instance.PlaySFX("FX_PART_MERGE_RESULT");
                            ForceUpdate();
                            
                            DragonPartEvent.RefreshList();//리스트 갱신 요청

                            var partData = User.Instance.PartData.GetPart(partTag);
                            if (partData != null && partData.Grade() == 5)
                            {
                                eAchieveSystemMessageType messageType = eAchieveSystemMessageType.GET_EQUIPMENT;
                                ChatManager.Instance.SendAchieveSystemMessage(messageType, User.Instance.UserData.UserNick, partData.GetItemDesignData().KEY);
                            }
                        }
                    }
                    break;
                    case eApiResCode.PART_NOT_EXISTS:
                    {
                        InitMaterialList();
                        Init();
                        ToastManager.On(100002550);
                    }
                    break;
                    case eApiResCode.PART_INVALID_MATERIAL_TO_MERGE:
                    {
                        InitMaterialList();
                        Init();
                        ToastManager.On(100002551);
                    }
                    break;
                    case eApiResCode.PART_INVALID_GRADE_MATERIAL_TO_MERGE:
                    {
                        InitMaterialList();
                        Init();
                        ToastManager.On(100002552);
                    }
                    break;
                    case eApiResCode.PART_INVALID_MATERIAL_COUNT_TO_MERGE:
                    {
                        InitMaterialList();
                        Init();
                        ToastManager.On(100002553);
                    }
                    break;
                }
            });
        }

        void ShowCompoundResultPopup(List<Asset> items)//결과 팝업 노출 (새로 생성된 장비 tag로 주는 지 확인)
        {
            PopupManager.OpenPopup<PartCompoundResultPopup>(new PartPopupData(items));
        }
    }
}