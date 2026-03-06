using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    [System.Serializable]
    public class PartAutoSlot 
    {
        [SerializeField]
        List<Sprite> gradeBgList = new List<Sprite>();
        [SerializeField]
        List<Sprite> gradeTagList = new List<Sprite>();
        [SerializeField]
        List<Sprite> partIconList = new List<Sprite>();

        public Image tagIcon = null;

        public Image Bg = null;

        public Image partIcon = null;

        public Text percentText = null;

        public SlotFrameController frame = null;
        
        public void SetData(int grade)
        {
            if (tagIcon != null)
                tagIcon.sprite = gradeTagList[grade];
            if (Bg != null)
                Bg.sprite = gradeBgList[grade];
            if (partIcon != null)
                partIcon.sprite = partIconList[grade];
            if (frame != null)
                frame.SetColor(grade);
        }

        public void SetPercent(float amount)
        {
            if (percentText != null)
                percentText.text = amount.ToString() + "%";
        }
    }

    public class DragonPartAutoDescLayer : TabLayer
    {
        [SerializeField]
        List<PartAutoSlot> slotList = new List<PartAutoSlot>();

        [SerializeField]
        Button compoundButton = null;

        [SerializeField]
        Text costLabel = null;

        [SerializeField]
        Text compoundCount = null;
        [SerializeField]
        Text obtainCount = null;

        int grade = -1;
        int count = -1;

        bool isSufficientGold = false;

        DragonPartCompoundPanel compoundPanel = null;

        bool isCompoundRequestClick = false;
        int MergeLimit { get { return GameConfigTable.GetPartMergeLimitCount(); } }

        public override void InitUI(TabTypePopupData datas = null)//데이터가 있는 갱신
        {
            base.InitUI(datas);

            var popupData = (DragonPartAutoTabTypePopupData)datas;

            grade = popupData.SubIndex;//현재 들어온 subindex를 grade로 삼음
            
            count = Mathf.Min(popupData.CompoundCount, MergeLimit);

            if (compoundPanel == null && popupData.CompoundPanel != null)
                compoundPanel = popupData.CompoundPanel;

            SetNode();
            SetCostLabel();
            SetObtainLabel();

            isCompoundRequestClick = false;
        }

        public override void RefreshUI()//데이터 유지 갱신
        {

        }

        void SetNode()
        {
            slotList[0].SetData(grade);
            slotList[1].SetData(grade);

            var currentPercent = GetMergeBaseSuccessPercent();//다음 등급 확률
            var remainPercent = (float)Math.Round(((float)100 - currentPercent), 2);//현재 획득 확률
            slotList[1].SetPercent(remainPercent);

            slotList[2].SetData(grade + 1);
            slotList[2].SetPercent(currentPercent);
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
            var baseNum = partBaseData.BASE_NUM;//기본 갯수
            var addAmountBonus = PartMergeEquipAmountBonusData.GetRateByGradeAndBonusAmountNum(grade, DragonPartAutoCompoundPopup.PART_MERGE_MATERIAL_MAX_COUNT - baseNum);

            if (addAmountBonus > 0)
                success_rate += addAmountBonus;
            return (float)Math.Round((success_rate / (float)SBDefine.MILLION * 100), 2);
        }
        PartMergeBaseData GetMergeBaseData()
        {
            var mergeDefaultData = PartMergeBaseData.GetDataByGrade(grade);
            if (mergeDefaultData == null || mergeDefaultData.Count <= 0)
            {
                return null;
            }
            return mergeDefaultData[0];
        }

        void SetCostLabel()
        {
            var mergeData = GetMergeBaseData();
            if (mergeData == null)//버그
            {
                if(costLabel != null)
                    costLabel.text = "-";
                return;
            }

            var expectCost = mergeData.COST_NUM * count;//개당 가격 * 횟수
            
            if (costLabel != null)
                costLabel.text = SBFunc.CommaFromNumber(expectCost);

            isSufficientGold = expectCost <= User.Instance.GOLD;
            costLabel.color = isSufficientGold ? Color.white : Color.red;

            if (compoundButton != null)
                compoundButton.SetButtonSpriteState(isSufficientGold);
        }
        void SetObtainLabel()
        {
            if (compoundCount != null)
                compoundCount.text = (count * DragonPartAutoCompoundPopup.PART_MERGE_MATERIAL_MAX_COUNT).ToString();
            if (obtainCount != null)
                obtainCount.text = count.ToString();
        }
        public void OnClickCompoundButton()
        {
            if(!isSufficientGold)
            {
                ToastManager.On(StringData.GetStringByStrKey("town_upgrade_text_05"));//골드가 부족합니다.
                return;
            }

            if(isCompoundRequestClick)
            {
                return;
            }

            isCompoundRequestClick = true;

            var ignoreString = GetLockListByGrade(grade);
            //합성 요청
            var param = new WWWForm();
            param.AddField("grade", grade);
            param.AddField("ignore", ignoreString);//,로 이뤄진 string

            NetworkManager.Send("part/mergeall", param, (jsonObj) =>
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
                            List<Asset> rewards = new List<Asset>();

                            var tagList = (JArray)data["tags"];
                            foreach(var tag in tagList)
                            {
                                var partTag = tag.Value<int>();
                                var partData = User.Instance.PartData.GetPart(partTag);
                                if (partData == null)
                                    continue;

                                rewards.Add(new Asset(partData.GetItemDesignData().KEY));

                                if (partData.Grade() == 5)
                                {
                                    eAchieveSystemMessageType messageType = eAchieveSystemMessageType.GET_EQUIPMENT;
                                    ChatManager.Instance.SendAchieveSystemMessage(messageType, User.Instance.UserData.UserNick, partData.GetItemDesignData().KEY);
                                }
                            }
                            
                            ShowCompoundResultPopup(rewards);
                            SoundManager.Instance.PlaySFX("FX_PART_MERGE_RESULT");
                            compoundPanel.ForceUpdate();//합성 패널 갱신 요청
                            DragonPartEvent.RefreshList();//리스트 갱신 요청
                        }
                    }
                    break;
                    case eApiResCode.PART_NOT_EXISTS:
                    {
                        isCompoundRequestClick = false;//응답 받으면 초기화
                        ToastManager.On(100002550);
                    }
                    break;
                    case eApiResCode.PART_INVALID_MATERIAL_TO_MERGE:
                    {
                        isCompoundRequestClick = false;//응답 받으면 초기화
                        ToastManager.On(100002551);
                    }
                    break;
                    case eApiResCode.PART_INVALID_GRADE_MATERIAL_TO_MERGE:
                    {
                        isCompoundRequestClick = false;//응답 받으면 초기화
                        ToastManager.On(100002552);
                    }
                    break;
                    case eApiResCode.PART_INVALID_MATERIAL_COUNT_TO_MERGE:
                    {
                        isCompoundRequestClick = false;//응답 받으면 초기화
                        ToastManager.On(100002553);
                    }
                    break;
                }
            });
        }

        //int[] GetLockListByGrade(int _grade)//현재 grade로 이뤄진 Lock 상태의 part Tag List 가져오기
        //{
        //    List<int> lockList = new List<int>();
        //    if (_grade <= 0)
        //        return lockList.ToArray();

        //    var partLockList = User.Instance.Lock.LockPart;
        //    if (partLockList == null || partLockList.Count <= 0)
        //        return lockList.ToArray();
        //    return partLockList.FindAll(Element => {
        //        var partData = User.Instance.PartData.GetPart(Element);
        //        if (partData == null)
        //            return false;
        //        return partData.Grade() == _grade;
        //    }).ToArray();
        //}

        string GetLockListByGrade(int _grade)
        {
            string totalStr = "";
            var partLockList = User.Instance.Lock.LockPart;
            if (partLockList == null || partLockList.Count <= 0)
                return "";

            int checkIndex = 0;
            partLockList.ForEach((element) => {
                var partData = User.Instance.PartData.GetPart(element);
                if (partData != null)
                {
                    if(_grade == partData.Grade())
                    {
                        string prefix = checkIndex > 0 ? "," : "";
                        totalStr += (prefix + element.ToString());
                        checkIndex++;
                    }
                }
            });

            return totalStr;
        }
        void ShowCompoundResultPopup(List<Asset> items)//결과 팝업 노출(tag를 ID로 래핑해서 던짐)
        {
            SystemRewardPopup.OpenPopup(items, ()=> {
                isCompoundRequestClick = false;//응답 받으면 초기화
                //보상 팝업 열려서 받으면 첫화면으로 보냄
                PopupManager.GetPopup<DragonPartAutoCompoundPopup>().moveTab(new DragonPartAutoTabTypePopupData(0, 0, 0));

            });//다중 보상은 일단 mainUI쪽에서는 필요없어서 보상 콜백 제거
        }
    }
}
