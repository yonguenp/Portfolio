using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork {

    public class HolderPassPopup : Popup<PopupData>
    {



        [Header("Top")]
        [SerializeField]
        TimeObject passTimeObject = null;
        [SerializeField]
        Text remainTimeText = null;
        [SerializeField]
        Text passNameText = null;

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
        Sprite normalMissionNodeSprite = null;
        [SerializeField]
        GameObject missionGaugeIconObj = null;
        [Space(10)]
        [Header("Bot Mission Layer")]
        [SerializeField]
        Button missionRewardBtn = null;
        [SerializeField]
        TableView missionTableView = null;

        [SerializeField]
        TimeObject missionDailyTimeObj = null;
        [SerializeField]
        Text missionDailyTimeText = null;


        List<HolderPassObject> passObjList = new List<HolderPassObject>();

        List<ITableData> missionDatas = new List<ITableData>();
        bool isInit = false;

        PassInfoData curPassData = null;
        int rewardAbleCount = 0;

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
        }
        public void SetData()
        {
            Debug.LogError("holder req");
            //holder 패스 제거
            //NetworkManager.Send("pass/holder", null, (JObject jsonData) =>
            //{
            //    if (!SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
            //        return;
            //    if ((eApiResCode)(int)jsonData["rs"] == eApiResCode.OK)
            //    {
            //        if (SBFunc.IsJTokenType(jsonData["reward_list"], JTokenType.Array))
            //        {
            //            foreach (var data in (JArray)jsonData["reward_list"])
            //            {
            //                QuestManager.Instance.SetQuestComplete(data["qid"].ToObject<int>());
            //            }

            //        }
            //        if (SBFunc.IsJTokenType(jsonData["vip"], JTokenType.Integer))
            //        {
            //            ePassUserType type = jsonData["vip"].ToObject<ePassUserType>();
            //            if (type != ePassUserType.HOLDER)
            //            {
            //                UIObjectEvent.Event(UIObjectEvent.eEvent.REFRESH_BADGE);
            //                ClosePopup();
            //            }
            //        }
            //        BattlePassManager.Instance.SetHolderData(jsonData);
            //        if (jsonData.ContainsKey("pass_remain_time"))
            //        {
            //            if (jsonData["pass_remain_time"].Type == JTokenType.Null || jsonData["pass_remain_time"].ToObject<int>() < TimeManager.GetTime())
            //            {
            //                UIObjectEvent.Event(UIObjectEvent.eEvent.REFRESH_BADGE);
            //                ClosePopup();
            //            }
            //        }

            //        curPassData = BattlePassManager.Instance.HolderPassData;
            //        if (curPassData == null)
            //        {
            //            UIObjectEvent.Event(UIObjectEvent.eEvent.REFRESH_BADGE);
            //            ClosePopup();
            //        }
            //        if (BattlePassManager.Instance.HolderPassRemainTime > TimeManager.GetTime())
            //            SetTimeInfo(BattlePassManager.Instance.HolderPassRemainTime);
            //        else
            //        {
            //            ClosePopup();
            //            UIObjectEvent.Event(UIObjectEvent.eEvent.REFRESH_BADGE);

            //        }
            //        //SetDailyMissionTimeObj(BattlePassManager.Instance.HolderPassDailyMissionRefreshTime);
            //        SetPassObj();
            //        SetMissionInfo(false);
            //        passNameText.text = curPassData.Pass_Title;
            //        SetDailyMissionTimeObj();
            //    }
            //    else
            //    {
            //        UIObjectEvent.Event(UIObjectEvent.eEvent.REFRESH_BADGE);
            //        ClosePopup();

            //    }
            //},
            //(string result) =>
            //{
            //    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("배틀패스팝업문구")
            //    , ClosePopup
            //    , null
            //    , ClosePopup
            //    );
            //});

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

            //for (int i = 0, count = curPassData.PassItems.Count; i < count; ++i)
            //{
            //    passRewardState.Add(i > currentLv - 1 ? eBattlePassRewardState.REWARD_DISABLE : eBattlePassRewardState.REWARD_ABLE);
            //}
            //SetPassObj();
        }

        void SetDailyMissionTimeObj()
        {
            missionDailyTimeObj.Refresh = () =>
            {
                int time = TimeManager.GetContentResetTime();
                if (time <= 0)
                {
                    missionDailyTimeObj.Refresh = null;
                    InitUI();
                }
            };
        }

        public void SetPassObj()
        {
            var passDatas = curPassData.PassItems;
            rewardAbleCount = 0;
            var passRewardState = BattlePassManager.Instance.HolderPassRewardStates;
            if(curPassData.PassStartLv()==0)
                passRewardState.Insert(0, eBattlePassRewardState.REWARDED);
            int PassPoint = BattlePassManager.Instance.HolderPassPoint;
            bool isMaxLv = BattlePassManager.Instance.IsHolderPassMaxLv;
            int currentLv = BattlePassManager.Instance.HolderPassLevel;
            for (int i = 0, count = passDatas.Count; i < count; ++i)
            {
                if (passObjList.Count <= i)
                {
                    var pass = Instantiate(passObj, passParentTr).GetComponent<HolderPassObject>();
                    passObjList.Add(pass);
                }
                if (passRewardState[i] == eBattlePassRewardState.REWARD_ABLE)
                    ++rewardAbleCount;
                passObjList[i].gameObject.SetActive(true);
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
                passObjList[i].Init(passDatas[i], currentLv, gauge, passRewardState[i], i == count - 1, ChkRewardAbleCnt);
            }
            getAllBtn.SetButtonSpriteState(rewardAbleCount>0);
            getAllBtn.interactable = rewardAbleCount>0;
            //passScrollRect.horizontalNormalizedPosition = 0;
            if(passScrollRect.content.rect.size.x == 0)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(passScrollRect.content);
            }
            passScrollRect.FocusOnItem(passObjList[Mathf.Max(currentLv - 1,0)].GetComponent<RectTransform>(), 0.3f);
        }

        void ChkRewardAbleCnt(int lv)
        {
            --rewardAbleCount;
            getAllBtn.SetButtonSpriteState(rewardAbleCount > 0);
            getAllBtn.interactable = rewardAbleCount > 0;
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



        void IsFullBagAlert()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                () => {
                    //메인팝업 열기
                    PopupManager.OpenPopup<InventoryPopup>();
                    PopupManager.ClosePopup<PostListPopup>();
                }, () => { }, () => { });
        }

        public void OnClickGetAll()
        {
            List<Asset> AllItems = new List<Asset>();
            var passState = BattlePassManager.Instance.HolderPassRewardStates;
            for (int i = 0, count = curPassData.PassItems.Count; i < count; ++i)
            {
                if (passState[i] == eBattlePassRewardState.REWARD_ABLE && curPassData.PassItems[i].NormalReward !=null)
                    AllItems.Add(curPassData.PassItems[i].NormalReward);
            }

            if (User.Instance.CheckInventoryGetItem(AllItems))
            {
                IsFullBagAlert();
                return;
            }

            Debug.LogError("holder req");
            //holder 패스 제거
            //WWWForm param = new WWWForm();
            //param.AddField("season_id", curPassData.KEY);
            //NetworkManager.Send("pass/holderreward", param, (JObject jsonData) =>
            //{
            //    if (SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
            //    {

            //        if ((int)jsonData["rs"] == (int)eApiResCode.OK)
            //        {
            //            BattlePassManager.Instance.SetHolderData(jsonData); // 이 안에 주석에 있는 코드도 포함되어 있음
            //            //if (SBFunc.IsJArray(jsonData["rewarded"]))
            //            //{
            //            //    var rewardsInfo = (JArray)jsonData["rewarded"];
            //            //    BattlePassManager.Instance.SetHolderPassRewardState(rewardsInfo);
            //            //}
            //            if (SBFunc.IsJArray(jsonData["reward"]))
            //            {
            //                SystemRewardPopup.OpenPopup(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonData["reward"])),null,true);
            //            }
            //            getAllBtn.SetButtonSpriteState(false);
            //            getAllBtn.interactable = false;
            //            SetPassObj();
            //        }   
            //        else if ((int)jsonData["rs"] == (int)eApiResCode.INVENTORY_FULL)
            //        {
            //            IsFullBagAlert();
            //        }
            //    }
            //},(string res) =>
            //{
            //    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("배틀패스팝업문구"), true, false, true); 
            //});
        }
        public void OnClickClearAndReward()
        {
            int expectedAddExp = 0; // 클리어시 더해질 총 경험치량 계산
            var clearReadyQuest = curPassData.QuestProcessDone;
            int curPassPoint = BattlePassManager.Instance.HolderPassPoint;
            int curPassLv = BattlePassManager.Instance.HolderPassLevel;
            foreach (var quest in clearReadyQuest)
            {
                if (quest != null)
                {
                    var rewardID = quest.QuestTableData.REWARD_GROUP;
                    var itemGroup = ItemGroupData.Get(rewardID);
                    if (itemGroup != null && itemGroup.Count > 0)
                    {
                        expectedAddExp += itemGroup[0].Reward.Amount;
                    }
                }
            }
            int expectedLv = curPassData.GetCurrentLevel(curPassPoint + expectedAddExp);

            // 획득 예상 아이템에 관한 가방 체크
            if (expectedLv > curPassLv)
            {
                List<Asset> AllItems = new List<Asset>();
                var passState = BattlePassManager.Instance.HolderPassRewardStates;
                bool isHolderPass = BattlePassManager.Instance.UserType == ePassUserType.HOLDER;
                var spPassState = isHolderPass ? BattlePassManager.Instance.HolderPassRewardStates : BattlePassManager.Instance.SpecialRewardStates;
                for (int i = 0; i < expectedLv; ++i)
                {
                    if (passState[i] == eBattlePassRewardState.REWARD_DISABLE && curPassData.PassItems[i].NormalReward != null)
                        AllItems.Add(curPassData.PassItems[i].NormalReward);
                }
                if (User.Instance.CheckInventoryGetItem(AllItems))
                {
                    IsFullBagAlert();
                    return;
                }
            }
            Debug.LogError("holder req");
            //holder 패스 제거
            //NetworkManager.Send("pass/holdermissionreward", null, (JObject jsonData) =>
            //{
            //    if (SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
            //    {
            //        if ((int)jsonData["rs"] == (int)eApiResCode.OK)
            //        {
            //            if (SBFunc.IsJTokenType(jsonData["exp"], JTokenType.Integer))
            //            {
            //                int lastLv = BattlePassManager.Instance.HolderPassLevel;
            //                if (SBFunc.IsJTokenType(jsonData["reward_list"], JTokenType.Array))
            //                {
            //                    foreach (var data in (JArray)jsonData["reward_list"])
            //                    {
            //                        QuestManager.Instance.SetQuestComplete(data["qid"].ToObject<int>());
            //                    }
            //                }

            //                BattlePassManager.Instance.SetHolderData(jsonData);  // 이 안에 주석에 있는 코드도 포함되어 있음
            //                //BattlePassManager.Instance.SetHolderPassPoint(jsonData["exp"].ToObject<int>());

            //                int currentLv = BattlePassManager.Instance.HolderPassLevel;
            //                SetMissionInfo(true, lastLv != currentLv);
            //            }
            //            if (SBFunc.IsJArray(jsonData["reward"]))
            //            {
            //                SystemRewardPopup.OpenPopup(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonData["reward"])), null, true);
            //            }
            //            SetPassObj();
            //        }
            //        else if ((int)jsonData["rs"] == (int)eApiResCode.INVENTORY_FULL)
            //        {
            //            IsFullBagAlert();
            //        }
            //    }
            //}, (string res) =>
            //{
            //    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("배틀패스팝업문구"), true, false, true);
            //});

        }

        void SetMissionInfo(bool isExpChange, bool isLvChange = false)
        {

            if (BattlePassManager.Instance.IsHolderPassMaxLv)
            {
                curMissionPtSlider.value = curMissionPtSlider.maxValue = 1;
                curMissionPtText.text = "";
                curMissionNumText.text = curPassData.GetMaxLevel().ToString();
                missionGaugeIconObj.SetActive(false);
                //curMissionNodeImg.sprite = maxLvMissionNodeSprite;
            }
            else
            {
                int currentLv = BattlePassManager.Instance.HolderPassLevel;
                var before = curPassData.GetLvUpNeedPoint(currentLv - 1);
                var after = curPassData.GetLvUpNeedPoint(currentLv);
                int PassPoint = BattlePassManager.Instance.HolderPassPoint;
                curMissionNodeImg.sprite = normalMissionNodeSprite;
                curMissionPtText.text = string.Format("{0} / {1}", PassPoint, after);
                float newValue = (PassPoint - before) / (float)(after - before);
                if (isExpChange)
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
                curMissionNumText.text = string.Format("LEVEL {0}", currentLv);
            }
            SetMissionTableView();
        }
        void SetMissionTableView()
        {
            if (isInit==false)
            {
                missionTableView.OnStart();
                isInit = true;
            }
            List<Quest> missions = curPassData.QuestIncludeTerminate;            

            if (missions == null || missions.Count == 0) return;
            missions = missions.OrderByDescending(item => (item.State == eQuestState.PROCESS_DONE) == false).
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
    }
}
    
