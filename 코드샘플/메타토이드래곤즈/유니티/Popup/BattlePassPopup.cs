using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

namespace SandboxNetwork {

    public class BattlePassPopup : Popup<PopupData>
    {

        [SerializeField]
        GameObject spPassBuyBtn = null;

        [Header("Top")]
        [SerializeField]
        Text passNameText = null;
        [SerializeField]
        TimeObject passTimeObject = null;
        [SerializeField]
        Text remainTimeText = null;
        [SerializeField]
        GameObject HolderAlertObj = null;

        [Space(10)]
        [Header("PassLayer")]
        [SerializeField]
        GameObject passObj = null;
        [SerializeField]
        ScrollRect passScrollRect = null;
        [SerializeField]
        Transform passParentTr = null;

        [SerializeField]
        Button getAllBtn = null;


        [Space(10)]
        [Header("Last Special Reward")]
        [SerializeField]
        ItemFrame LastSpecialRewardItem = null;
        //[SerializeField]
        //GameObject specialRewardEffectObj = null;


        [Space(10)]
        [Header("Top Mission Layer")]
        [SerializeField]
        Slider curMissionPtSlider = null;
        [SerializeField]
        Text curMissionPtText = null;
        [SerializeField]
        Text curMissionNumText = null;
        [SerializeField]
        Image curMissionNodeImg = null;
        [SerializeField]
        Sprite maxLvMissionNodeSprite = null;
        [SerializeField]
        Sprite normalMissionNodeSprite =null;
        [SerializeField]
        GameObject missionGaugeIconObj = null;
        [Space(10)]
        [Header("Bot Mission Layer")]
        [SerializeField]
        Button missionRewardBtn = null;
        [SerializeField]
        TableView missionTableView = null;


        List<BattlePassObject> passObjList = new List<BattlePassObject>();


        List<ITableData> missionDatas = new List<ITableData>();
        bool isInit = false;
        int rewardAbleCount = 0;

        PassInfoData curPassData = null;

        public override void InitUI()
        {
            UICanvas.Instance.StartBackgroundBlurEffect();

            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);
            foreach (var obj in passObjList)
            {
                obj.gameObject.SetActive(false);
            }
            SetData();
        }

        public override void ClosePopup()
        {
            base.ClosePopup();
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
            UICanvas.Instance.EndBackgroundBlurEffect();
            passTimeObject.Refresh = null;
        }

        void SetData()
        {
            WWWForm param = new WWWForm();
            param.AddField("imholder", User.Instance.IS_HOLDER ? 1 : 0);
            NetworkManager.Send("pass/pass", param, (JObject jsonData) =>
            {
                if (!SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                    return;
                if ((eApiResCode)(int)jsonData["rs"] == eApiResCode.OK)
                {
                    BattlePassManager.Instance.RefreshBattlePass();
                    BattlePassManager.Instance.SetBattlePassData(jsonData);;
                    HolderAlertObj.SetActive(BattlePassManager.Instance.UserType == ePassUserType.HOLDER);
                    curPassData = BattlePassManager.Instance.BattlePassData;
                    SetPassObj();
                    SetMissionInfo(false);
                    if (curPassData.END_TIME > TimeManager.GetDateTime())
                        SetTimeInfo(TimeManager.GetTimeStamp(curPassData.END_TIME));
                    else
                    {
                        ClosePopup();
                        UIObjectEvent.Event(UIObjectEvent.eEvent.REFRESH_BADGE);
                    }

                    passNameText.text = curPassData.Pass_Title;
                }
                else
                {
                    UIObjectEvent.Event(UIObjectEvent.eEvent.REFRESH_BADGE);
                    ClosePopup();
                }
            }, (string result) =>
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("배틀패스팝업문구")
                    , ClosePopup
                    , null
                    , ClosePopup
                    );
            });




            // 임시 용
            //curPassData = PassInfoData.GetCurPass(eBattlePassType.BATTLE);
            //if (curPassData == null)
            //{
            //    ClosePopup();
            //    ToastManager.On("현재 시즌이 아닙니다");
            //}
            //PassPoint = SBFunc.Random(0, 3000);
            //currentLv = curPassData.GetCurrentLevel(PassPoint);
            //isMaxLv = currentLv == curPassData.GetMaxLevel();
            //LastRoadObj.GetComponent<Image>().sprite = isMaxLv ? roadOnImg : roadOffImg;

            //for (int i = 0, count = curPassData.PassItems.Count; i < count; ++i)
            //{
            //    var state = eBattlePassRewardState.REWARD_ABLE;
            //    if(i > currentLv-1)
            //    {
            //        state = eBattlePassRewardState.REWARD_DISABLE;
            //    }
            //    passRewardState.Add(state);
            //    specialRewardState.Add(userType == ePassUserType.DEFAULT ? eBattlePassRewardState.LOCK : state);
            //}
            //SetPassObj();
            //  여기 까지 임시용


        }





        public void SetPassObj()
        {
           
            var passDatas = curPassData.PassItems;
            List<eBattlePassRewardState> passRewardState = BattlePassManager.Instance.PassRewardStates;
            List<eBattlePassRewardState> specialRewardState = BattlePassManager.Instance.SpecialRewardStates;
            
            
            //if (curPassData.PassStartLv() == 0)
            //{
            //    passRewardState.Insert(0, eBattlePassRewardState.REWARDED);
            //    specialRewardState.Insert(0, eBattlePassRewardState.REWARDED);
            //}
                
            bool isMaxLv = BattlePassManager.Instance.IsPassMaxLv;
            int currentLv = BattlePassManager.Instance.PassLevel;
            int PassPoint = BattlePassManager.Instance.PassPoint;
            ePassUserType userType = BattlePassManager.Instance.UserType;
            int lastRewardIndex = passDatas.Count - 1;
            rewardAbleCount = 0;
            for (int i = 0, count = lastRewardIndex; i < count; ++i)
            {
                if (passObjList.Count <= i)
                {
                    var pass = Instantiate(passObj, passParentTr).GetComponent<BattlePassObject>();
                    passObjList.Add(pass);
                }
                passObjList[i].gameObject.SetActive(true);
                if (passRewardState[i] == eBattlePassRewardState.REWARD_ABLE)
                    ++rewardAbleCount;
                if (specialRewardState[i] == eBattlePassRewardState.REWARD_ABLE)
                    ++rewardAbleCount;

                float gauge = 0f;
                if (isMaxLv)
                    gauge = 1f;
                else
                {
                    if (passDatas[i].LEVEL < currentLv)
                        gauge = 1f;
                    else if (passDatas[i].LEVEL == currentLv)
                    {
                        var before = i == 0 ? 0 : passDatas[i - 1].NEXT_POINT;
                        var after = passDatas[i].NEXT_POINT;
                        gauge = (PassPoint - before) / (float)(after - before);
                    }
                }
                passObjList[i].Init(passDatas[i], currentLv, gauge, passRewardState[i], specialRewardState[i], i == count - 1, ChkRewardAbleCount);
            }

            if (specialRewardState[lastRewardIndex] == eBattlePassRewardState.REWARD_ABLE)
                ++rewardAbleCount;

            SetSpecialReward(passRewardState[lastRewardIndex], specialRewardState[lastRewardIndex], lastRewardIndex, passDatas[lastRewardIndex].LEVEL == currentLv);



            spPassBuyBtn.SetActive(userType == ePassUserType.DEFAULT);
            if (passScrollRect.content.rect.size.x == 0)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(passScrollRect.content);
            }
            
            if (currentLv - 1 < lastRewardIndex)
            {
                passScrollRect.FocusOnItem(passObjList[currentLv>0?currentLv - 1:0].GetComponent<RectTransform>(), 0.3f);
            }
            
        }

        void ChkRewardAbleCount(int passLv, int isSpecial)
        {
            --rewardAbleCount;
            //getAllBtn.SetButtonSpriteState(rewardAbleCount > 0);
            //getAllBtn.interactable = rewardAbleCount > 0;
        }



        void SetSpecialReward(eBattlePassRewardState lastNormalState, eBattlePassRewardState lastSpecialState,int lastRewardIndex, bool isRewardAble)
        {
            //specialRewardEffectObj.SetActive(false);
            bool isGetSpecialReward = false;
            ePassUserType userType = BattlePassManager.Instance.UserType;
            if (lastNormalState == eBattlePassRewardState.REWARDED && curPassData.PassItems[lastRewardIndex].NormalReward !=null)
            {
                LastSpecialRewardItem.SetFrameItem(curPassData.PassItems[lastRewardIndex].NormalReward);
                LastSpecialRewardItem.setFrameCheck(true);
                getAllBtn.SetButtonSpriteState(false);
                getAllBtn.interactable = false;
                //specialRewardEffectObj.SetActive(false);
                return;
            }
            if (lastSpecialState == eBattlePassRewardState.REWARDED && curPassData.PassItems[lastRewardIndex].SpecialReward != null)
            {
                LastSpecialRewardItem.SetFrameItem(curPassData.PassItems[lastRewardIndex].SpecialReward);
                LastSpecialRewardItem.setFrameCheck(true);
                getAllBtn.SetButtonSpriteState(false);
                getAllBtn.interactable = false;
                //specialRewardEffectObj.SetActive(false);
            }
            else if (lastSpecialState == eBattlePassRewardState.REWARDED_HOLDER && curPassData.PassItems[lastRewardIndex].HolderReward != null)
            {
                LastSpecialRewardItem.SetFrameItem(curPassData.PassItems[lastRewardIndex].HolderReward);
                LastSpecialRewardItem.setFrameCheck(true);
                getAllBtn.SetButtonSpriteState(false);
                getAllBtn.interactable = false;
                //specialRewardEffectObj.SetActive(false);
            }
            else
            {
                if (curPassData.PassItems[lastRewardIndex].NormalReward == null) // 마지막 보상 Normal Reward 테이블 값이 없는 경우
                {
                    LastSpecialRewardItem.SetFrameItem(userType == ePassUserType.HOLDER ? curPassData.PassItems[lastRewardIndex].HolderReward : curPassData.PassItems[lastRewardIndex].SpecialReward);
                    LastSpecialRewardItem.SetLockIcon(userType == ePassUserType.DEFAULT);
                    isGetSpecialReward = isRewardAble && userType != ePassUserType.DEFAULT;
                    //specialRewardEffectObj.SetActive(isRewardAble && userType != ePassUserType.DEFAULT);
                }
                else
                {
                    LastSpecialRewardItem.SetLockIcon(false);
                    isGetSpecialReward = isRewardAble;
                    //specialRewardEffectObj.SetActive(isRewardAble);
                    Asset curAsset = null;
                    switch (userType)
                    {
                        case ePassUserType.DEFAULT:
                            curAsset = curPassData.PassItems[lastRewardIndex].NormalReward;
                            break;
                        case ePassUserType.HOLDER:
                            curAsset = curPassData.PassItems[lastRewardIndex].HolderReward;
                            break;
                        case ePassUserType.PASS_BUY:
                            curAsset = curPassData.PassItems[lastRewardIndex].SpecialReward;
                            break;
                    }
                    LastSpecialRewardItem.SetFrameItem(curAsset);
                    LastRewardCallBackSet(lastRewardIndex, userType != ePassUserType.DEFAULT, curAsset);

                }
            }
            getAllBtn.SetButtonSpriteState(isGetSpecialReward);
            getAllBtn.interactable = isGetSpecialReward;
            LastSpecialRewardItem.SetItemBgOff(false);
        }

        void LastRewardCallBackSet(int index, bool isSpecial,Asset asset)
        {
            LastSpecialRewardItem.setCallback((itemID) =>
            {
                GetRewardItemOnce(index, isSpecial, () =>
                {
                   // specialRewardEffectObj.SetActive(false);
                    LastSpecialRewardItem.SetTooltipShowAble();
                    LastSpecialRewardItem.setFrameCheck(true);
                    //ChkRewardAbleCount(index, isSpecial? 1:0);
                    SystemRewardPopup.OpenPopup(new List<Asset>() { asset });
                });

            });
        }


        void GetRewardItemOnce(int passIndex, bool isSpecial, VoidDelegate cb = null)
        {
            return;
            //List<Asset> AllItems = new List<Asset>();
            //var passState = BattlePassManager.Instance.PassRewardStates;
            //var spPassState = BattlePassManager.Instance.HolderPassRewardStates;
            //if (passState[passIndex] == eBattlePassRewardState.REWARD_ABLE)
            //    AllItems.Add(curPassData.PassItems[passIndex].NormalReward);
            //if (passState[passIndex] == eBattlePassRewardState.REWARD_ABLE)
            //    AllItems.Add(curPassData.PassItems[passIndex].HolderReward);
            //if (User.Instance.CheckInventoryGetItem(AllItems))
            //{
            //    IsFullBagAlert();
            //    return;
            //}
            //WWWForm param = new WWWForm();
            //param.AddField("level", passIndex);
            //param.AddField("type", isSpecial ? 2 : 1);
            //NetworkManager.Send("pass/???", param, (JObject jsonData) =>
            //{
            //    if (jsonData.ContainsKey("rs") && (int)jsonData["rs"] == (int)eApiResCode.OK)
            //    {
            //        cb?.Invoke();
            //    }
            //});
        }



        void SetTimeInfo(int remainTime)
        {
            passTimeObject.Refresh = () =>
            {
                remainTimeText.text = TimeManager.GetTimeCompareString(remainTime);
                float remain = TimeManager.GetTimeCompare(remainTime);
                if(remain <= 0)
                {
                    passTimeObject.Refresh = null;
                    InitUI();
                }

            };
        }

        public void OnClickGetAll()
        {
            List<Asset> AllItems = new List<Asset>();
            var passState = BattlePassManager.Instance.PassRewardStates;
            bool isHolderPass = BattlePassManager.Instance.UserType == ePassUserType.HOLDER;
            var spPassState = isHolderPass ? BattlePassManager.Instance.HolderPassRewardStates : BattlePassManager.Instance.SpecialRewardStates;
            for (int i =0, count = curPassData.PassItems.Count; i <count ; ++i)
            {
                if (passState[i] == eBattlePassRewardState.REWARD_ABLE && curPassData.PassItems[i].NormalReward != null)
                    AllItems.Add(curPassData.PassItems[i].NormalReward);
                if (spPassState[i] == eBattlePassRewardState.REWARD_ABLE)
                {
                    if (isHolderPass && curPassData.PassItems[i].HolderReward != null)
                        AllItems.Add(curPassData.PassItems[i].HolderReward);
                    else if( isHolderPass ==false && curPassData.PassItems[i].SpecialReward != null)
                        AllItems.Add(curPassData.PassItems[i].SpecialReward);
                }
            }

        
            if (User.Instance.CheckInventoryGetItem(AllItems))
            {
                IsFullBagAlert();
                return;
            }

            WWWForm param = new WWWForm();
            param.AddField("season_id", curPassData.KEY);
            NetworkManager.Send("pass/reward", param, (JObject jsonData) =>
            {
                if (SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                {
                    if ((int)jsonData["rs"] == (int)eApiResCode.OK)
                    {
                        if (SBFunc.IsJArray(jsonData["rewarded"]))
                        {
                            var rewardsInfo = (JArray)jsonData["rewarded"];
                            BattlePassManager.Instance.SetPassRewardState(rewardsInfo);
                        }
                        if (SBFunc.IsJArray(jsonData["reward"]))
                        {
                            SystemRewardPopup.OpenPopup(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonData["reward"])), null, true);
                        }
                        //getAllBtn.SetButtonSpriteState(false);
                        //getAllBtn.interactable = false;
                        SetPassObj();
                    }
                    // 시스템 리워드 팝업 띄워 줘야 함
                }
                else if ((int)jsonData["rs"] == (int)eApiResCode.INVENTORY_FULL)
                {
                    IsFullBagAlert();
                }
            }, (string res) =>
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("배틀패스팝업문구"), true, false, true);
            });
        }


        public void OnClickGetSpecial() // 보상 모두 받기에서  마지막 스페셜이나 홀더 보상 만 받기 버튼으로 바뀜
        {
            List<Asset> AllItems = new List<Asset>();
            var passState = BattlePassManager.Instance.PassRewardStates;
            bool isHolderPass = BattlePassManager.Instance.UserType == ePassUserType.HOLDER;
            var spPassState = isHolderPass ?  BattlePassManager.Instance.HolderPassRewardStates : BattlePassManager.Instance.SpecialRewardStates;
            //for (int i =0, count = curPassData.PassItems.Count; i <count ; ++i)
            //{
            //    if (passState[i] == eBattlePassRewardState.REWARD_ABLE)
            //        AllItems.Add(curPassData.PassItems[i].NormalReward);
            //    if (spPassState[i] == eBattlePassRewardState.REWARD_ABLE)
            //    {
            //        if (isHolderPass && curPassData.PassItems[i].HolderReward != null)
            //            AllItems.Add(curPassData.PassItems[i].HolderReward);
            //        else if( isHolderPass ==false && curPassData.PassItems[i].SpecialReward != null)
            //            AllItems.Add(curPassData.PassItems[i].SpecialReward);
            //    }
            //}

            int lastIndex = curPassData.PassItems.Count - 1;
            if (spPassState[lastIndex] == eBattlePassRewardState.REWARD_ABLE)
            {
                if (isHolderPass && curPassData.PassItems[lastIndex].HolderReward != null)
                    AllItems.Add(curPassData.PassItems[lastIndex].HolderReward);
                else if (isHolderPass == false && curPassData.PassItems[lastIndex].SpecialReward != null)
                    AllItems.Add(curPassData.PassItems[lastIndex].SpecialReward);
            }

            if (User.Instance.CheckInventoryGetItem(AllItems))
            {
                IsFullBagAlert();
                return;
            }

            WWWForm param = new WWWForm();
            param.AddField("season_id", curPassData.KEY);
            NetworkManager.Send("pass/reward", param, (JObject jsonData) =>
            {
                if (SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                {
                    if ((int)jsonData["rs"] == (int)eApiResCode.OK)
                    {
                        if (SBFunc.IsJArray(jsonData["rewarded"])) { 
                            var rewardsInfo = (JArray)jsonData["rewarded"];
                            BattlePassManager.Instance.SetPassRewardState(rewardsInfo);
                        }
                        if (SBFunc.IsJArray(jsonData["reward"]))
                        {
                            SystemRewardPopup.OpenPopup(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonData["reward"])),null,true);
                        }
                        //getAllBtn.SetButtonSpriteState(false);
                        //getAllBtn.interactable = false;
                        SetPassObj();
                    }
                    // 시스템 리워드 팝업 띄워 줘야 함
                }
                else if ((int)jsonData["rs"] == (int)eApiResCode.INVENTORY_FULL)
                {
                    IsFullBagAlert();
                }
            },(string res) =>
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("배틀패스팝업문구"), true, false, true);
            });
        }

        public void OnClickClearAndReward()
        {
            JArray jArr = new JArray();
            int curPassPoint = BattlePassManager.Instance.PassPoint;
            int curPassLv = BattlePassManager.Instance.PassLevel;
            int expectedAddExp = 0; // 클리어시 더해질 총 경험치량 계산
            foreach (var quest in curPassData.QuestIncludeTerminate) 
            {
                if (quest.IsQuestClear() && quest.State == eQuestState.PROCEEDING)
                {
                    jArr.Add(quest.ID);
                    var rewardID = quest.QuestTableData.REWARD_GROUP;
                    var itemGroup = ItemGroupData.Get(rewardID);
                    if (itemGroup != null && itemGroup.Count > 0)
                    {
                        expectedAddExp += itemGroup[0].Reward.Amount;
                    }
                }
            }
            // 예상 획득 아이템을 통한 가방 체크
            int expectedLv = curPassData.GetCurrentLevel(curPassPoint + expectedAddExp);
            if (expectedLv > curPassLv)
            {
                List<Asset> AllItems = new List<Asset>();
                var passState = BattlePassManager.Instance.PassRewardStates;
                bool isHolderPass = BattlePassManager.Instance.UserType == ePassUserType.HOLDER;
                var spPassState = isHolderPass ? BattlePassManager.Instance.HolderPassRewardStates : BattlePassManager.Instance.SpecialRewardStates;
                for (int i = 0; i < expectedLv; ++i)
                {
                    if (passState[i] == eBattlePassRewardState.REWARD_DISABLE && curPassData.PassItems[i].NormalReward != null)
                        AllItems.Add(curPassData.PassItems[i].NormalReward);
                    if (spPassState[i] == eBattlePassRewardState.REWARD_DISABLE)
                    {
                        if (isHolderPass && curPassData.PassItems[i].HolderReward != null)
                            AllItems.Add(curPassData.PassItems[i].HolderReward);
                        else if (isHolderPass == false && curPassData.PassItems[i].SpecialReward != null)
                            AllItems.Add(curPassData.PassItems[i].SpecialReward);
                    }
                }
                if (User.Instance.CheckInventoryGetItem(AllItems))
                {
                    IsFullBagAlert();
                    return;
                }
            }
            

            WWWForm param = new WWWForm();
            param.AddField("season_id", curPassData.KEY);

            param.AddField("quest",jArr.ToString(Formatting.None));
            NetworkManager.Send("pass/missionreward", param, (JObject jsonData) =>
            {
                if (SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                {
                    if ((int)jsonData["rs"] == (int)eApiResCode.OK)
                    {
                        if (SBFunc.IsJTokenType(jsonData["reward_list"], JTokenType.Array))
                        {
                            foreach (var data in (JArray)jsonData["reward_list"])
                            {
                                QuestManager.Instance.SetQuestComplete(data["qid"].ToObject<int>());
                            }

                            QuestEvent.Event(QuestEvent.eEvent.QUEST_UPDATE);
                        }
                        
                        if (SBFunc.IsJTokenType(jsonData["exp"], JTokenType.Integer))
                        {
                            int lastLv = BattlePassManager.Instance.PassLevel;
                            BattlePassManager.Instance.SetPassPoint(jsonData["exp"].ToObject<int>());
                            int currentLv = BattlePassManager.Instance.PassLevel;
                            SetMissionInfo(true, lastLv != currentLv);
                        }
                        if (SBFunc.IsJArray(jsonData["rewarded"]))
                        {
                            var rewardsInfo = (JArray)jsonData["rewarded"];
                            BattlePassManager.Instance.SetPassRewardState(rewardsInfo);
                        }
                        if (SBFunc.IsJArray(jsonData["reward"]))
                        {
                            SystemRewardPopup.OpenPopup(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonData["reward"])), null, true);
                        }
                        SetPassObj();
                    }
                    else if ((int)jsonData["rs"] == (int)eApiResCode.INVENTORY_FULL)
                    {
                        IsFullBagAlert();
                    }
                }
            });

        }

        public void OnClickBuySpecial()
        {

            var data = ShopGoodsData.Get(curPassData.PASS_GOODS_ID);
            if(data != null)
            {
                var popup = PopupManager.OpenPopup<ShopBannerPopup>(new ShopBuyPopupData(data));
                popup.SetBattlePassUI();
                popup.SetBuyCallBack(() =>
                {
                    BattlePassManager.Instance.SetUserType(ePassUserType.PASS_BUY);
                    SetPassObj();
                    LastSpecialRewardItem.SetLockIcon(false);
                    spPassBuyBtn.SetActive(false);

                });
            }

        }


        void SetMissionInfo(bool isExpChange, bool isLvChange=false)
        {
            if (BattlePassManager.Instance.IsPassMaxLv)
            {
                curMissionPtSlider.value = 1;
                curMissionPtText.text = "";
                curMissionNumText.text = "SPECIAL";
                missionGaugeIconObj.SetActive(false);
                //curMissionNodeImg.sprite = maxLvMissionNodeSprite;
            }
            else
            {
                int currentLv = BattlePassManager.Instance.PassLevel;
                int PassPoint = BattlePassManager.Instance.PassPoint;
                var before = curPassData.GetLvUpNeedPoint(currentLv - 1);
                var after = curPassData.GetLvUpNeedPoint(currentLv);
                curMissionNodeImg.sprite = normalMissionNodeSprite;
                curMissionPtText.text = string.Format("{0} / {1}", PassPoint, after);
                missionGaugeIconObj.SetActive(true);
                float newValue = (PassPoint - before) / (float)(after - before);
                if(isExpChange)
                {
                    Sequence sequence = DOTween.Sequence();
                    if (isLvChange)
                    {
                        sequence.Append(curMissionPtSlider.DOValue(1f, 0.2f));
                        sequence.Append(curMissionPtSlider.DOValue(0f, 0.1f));
                    }
                    sequence.Append(curMissionPtSlider.DOValue(newValue, 0.2f));
                    sequence.AppendCallback(() =>
                    {
                        sequence.Kill();
                    });
                }
                else
                {
                    curMissionPtSlider.value = newValue;
                }
                if (curPassData.GetMaxLevel() > currentLv)
                    curMissionNumText.text =  string.Format("LEVEL {0}", currentLv);
            }
            SetMissionTableView();
        }
        void SetMissionTableView()
        {
            if(!isInit)
            {
                missionTableView.OnStart();
                isInit = true;
            }
            
            List<Quest> missions = curPassData.QuestTypeUnique(true);
            if (missions == null || missions.Count == 0) return;
            missions = missions.OrderByDescending(item => ( item.State == eQuestState.PROCESS_DONE) == false).
                ThenBy(item => item.IsQuestClear() == false).ToList();

            missionDatas.Clear();
            bool isClearMissionExist = false;
            foreach (Quest quest in missions)
            {
                missionDatas.Add(quest);
                if (quest.State != eQuestState.TERMINATE && quest.State != eQuestState.PROCESS_DONE && quest.IsQuestClear())
                    isClearMissionExist = true;
            }
            missionRewardBtn.SetButtonSpriteState(isClearMissionExist || rewardAbleCount > 0);
            missionRewardBtn.interactable = (isClearMissionExist || rewardAbleCount > 0);
            missionTableView.SetDelegate(new TableViewDelegate(missionDatas, (GameObject node, ITableData item) => {
                if (node == null) return;
                var passObj = node.GetComponent<BattlePassMisionObj>();
                if (passObj == null) return;
                passObj.Init((Quest)item);
                node.SetActive(true);
            }));
            missionTableView.ReLoad();
        }

        void IsFullBagAlert()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                () => {
                    //메인팝업 열기
                    PopupManager.OpenPopup<InventoryPopup>();
                    PopupManager.ClosePopup<PostListPopup>();
                }, () => { }, () => { });
        }
    }
    
}