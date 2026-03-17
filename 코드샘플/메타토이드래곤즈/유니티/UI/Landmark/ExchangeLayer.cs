using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{

    public enum eExchangeDailyBoxState
    {
        NONE = 0,
        RECEIVE_DISABLE =1,
        RECEIVE_ABLE =2,
        RECEIVED =3
    }
    public class ExchangeLayer : BuildingLayer
    {
        [SerializeField] private ExchangeObject[] exchangeObjects;

        [SerializeField] private GameObject dailyRewardPopup = null; // 이 팝업은 현재 어디에도 사용하지 않고 이 팝업 안에만 사용해서 이렇게 사용
                                                                     // 이 형태의 팝업을 다른 곳에서도 사용시 분리할 것
        [SerializeField] private TableView dailyRewardSubTableView = null;
        [SerializeField] private RectTransform dailyRewardContent = null;
        [SerializeField] private RectTransform dailyRewardViewPort = null;


        [Space(10)]
        [Header("daily reward ")]
        [SerializeField] private Button[] dailyRewardBoxBtns = null;
        [SerializeField] private GameObject[] dailyRewardCheckObjs = null;
        [SerializeField] private Text[] requireTexts = null;
        [SerializeField] private GameObject[] touchEffectObjs = null;


        [SerializeField] private Text dailyRewardTimeText = null;
        [SerializeField] private TimeObject dailyRewardTimeObj = null;

        public ExchangeManager Exchange { get { return User.Instance.Exchange; } }

        Tween[] boxTween = new Tween[3];
        bool isTableViewInit = false;

        private readonly int DailyRewardBoxCount = 3;

        public bool buttonLock { get { return PopupManager.GetPopup<LandMarkPopup>().buttonLock; } }
        public void ButtonLock()
        {
            PopupManager.GetPopup<LandMarkPopup>().ButtonLock();
        }
        public void ButtonUnlock()
        {
            PopupManager.GetPopup<LandMarkPopup>().ButtonUnlock();
        }
        public void InitData()
        {
            Exchange.Prepare(DataRefresh);
        }

        public void OnClickShowDailyRewardPopup()
        {
            int rewardIndex = -1;
            if (!Exchange.Rewarded[0])
            {
                if (Exchange.FIRST_EXCHANGE_DAILY_REWARD_COUNT <= Exchange.ClearCount)
                    rewardIndex = 1;
            }
            else if (!Exchange.Rewarded[1])
            {
                if (Exchange.SECOND_EXCHANGE_DAILY_REWARD_COUNT <= Exchange.ClearCount)
                    rewardIndex = 2;
            }
            else if (!Exchange.Rewarded[2])
            {
                if (Exchange.THIRD_EXCHANGE_DAILY_REWARD_COUNT <= Exchange.ClearCount)
                    rewardIndex = 3;
            }
            else
            {
                ToastManager.On(StringData.GetStringByIndex(100002673));
                return;
            }

            if (rewardIndex > 0)
            {
                var rewardItemList = ItemGroupData.Get(GameConfigTable.GetDailyRewardItemGroup(rewardIndex));
                List<Asset> rewards = new List<Asset>();
                if (rewardItemList != null && rewardItemList.Count > 0)
                {
                    foreach (var rewardItem in rewardItemList)
                    {
                        rewards.Add(rewardItem.Reward);
                    }
                }
                Exchange.OnReward(rewardIndex, () => { DataRefresh(); });
                return;
            }
            OnClickDailyRewardList(rewardIndex == -1 ? 1 : rewardIndex + 1);
        }
        void SetEffectInit()
        {
            foreach (var item in touchEffectObjs)
            {
                item.SetActive(false);
            }
        }
        public void OnClickDailyRewardList(int index) // 1,2,3 번째 상자 보상 확인 확인
        {
            if (index > 3 || index < 1) return;
            var rewardNeedCntArr = new int[3] { Exchange.FIRST_EXCHANGE_DAILY_REWARD_COUNT, Exchange.SECOND_EXCHANGE_DAILY_REWARD_COUNT, Exchange.THIRD_EXCHANGE_DAILY_REWARD_COUNT };
            if (!Exchange.Rewarded[index - 1])
            {
                /** 안 깨져있는 상자를 눌렀을 때 오동작 수정 */
                for (int i = 0, count = index - 1; i < count; ++i)
                {
                    if(false == Exchange.Rewarded[i])
                    {
                        rewardNeedCntArr[(i + 1)] += rewardNeedCntArr[i];
                    }
                } 
                //
                if (rewardNeedCntArr[index - 1] <= Exchange.ClearCount) // 보상을 받을 수 있는 경우
                {
                    touchEffectObjs[index - 1].SetActive(true);
                    Exchange.OnReward(index, () => { DataRefresh(); });
                    return;
                }

            }

            var rewardItemList = ItemGroupData.Get(GameConfigTable.GetDailyRewardItemGroup(index));
            List<Asset> rewards = new List<Asset>();
            foreach (var reward in rewardItemList)
            {
                rewards.Add(reward.Reward);
            }
            var popup =PopupManager.OpenPopup<RewardListShowPopup>(new RewardPopupData(rewards));
            popup.SetTitleText(StringData.GetStringByIndex(100002250));
            //dailyRewardPopup.SetActive(true);
            //if (dailyRewardSubTableView != null)
            //{
            //    if (isTableViewInit == false)
            //    {
            //        dailyRewardSubTableView.OnStart();
            //        isTableViewInit = true;
            //    }
            //    List<ITableData> tableViewItemList = new List<ITableData>();
            //    var rewardItemList = ItemGroupData.Get(GameConfigTable.GetDailyRewardItemGroup(index));
            //    if (rewardItemList != null && rewardItemList.Count > 0)
            //    {
            //        foreach (var rewardItem in rewardItemList)
            //        {
            //            tableViewItemList.Add(rewardItem.Reward);
            //        }
            //    }
            //    dailyRewardSubTableView.SetDelegate(
            //    new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) => {
            //        if (node == null || item == null)
            //        {
            //            return;
            //        }
            //        Asset reward = (Asset)item;
            //        node.GetComponent<ItemFrame>().SetFrameItem(reward.ItemNo, reward.Amount, (int)reward.GoodType);
            //    }));

            //    dailyRewardSubTableView.ReLoad();

            //    if (rewardItemList.Count < 5) //특정 갯수보다 적을땐 중앙 정렬
            //    {
            //        dailyRewardContent.sizeDelta = dailyRewardViewPort.sizeDelta;
            //    }

            //}
        }

        public void CloseDailyRewardPopup()
        {
            //dailyRewardPopup.SetActive(false);
        }

        // int refreshCnt = 0;
        public void DataRefresh()
        {
            // Debug.Log("refresh called : " + (++refreshCnt).ToString());
            for (int i = 0; i < 4; i++)
            {
                Exchange data = null;
                foreach (var d in Exchange.Exchange)
                {
                    if (d != null)
                    {
                        if (d.slot_id == i + 1)
                        {
                            data = d;
                            break;
                        }
                    }
                }

                var exchange = exchangeObjects[i];
                exchange.SetData(this, data);
            }
            SetDailyRewardUI();
            TutorialCheck();
        }

        void SetDailyRewardUI()
        {
            int lastRecieveRewardIndex = 0; // 현재 받은 보상 갯수
            eExchangeDailyBoxState[] boxStates = new eExchangeDailyBoxState[3];
            foreach (var isReward in Exchange.Rewarded)
            {
                if (isReward)
                {
                    ++lastRecieveRewardIndex;
                } 
            }
            var firstNeed = User.Instance.Exchange.FIRST_EXCHANGE_DAILY_REWARD_COUNT;
            var secondNeed = User.Instance.Exchange.SECOND_EXCHANGE_DAILY_REWARD_COUNT + firstNeed;
            var thirdNeed = User.Instance.Exchange.THIRD_EXCHANGE_DAILY_REWARD_COUNT + secondNeed;
            int[] needCnts = new int[3] { firstNeed , secondNeed, thirdNeed};
            var realCurCount = Exchange.ClearCount;
            switch (lastRecieveRewardIndex)
            {
                case 0:
                    break;
                case 1:
                    realCurCount += firstNeed;
                    break;
                case 2:
                    realCurCount += secondNeed;
                    break;
                case 3:
                    realCurCount += thirdNeed;
                    break;
            }
            for(int i =0; i < DailyRewardBoxCount; ++i)
            {
                if (i < lastRecieveRewardIndex)
                {
                    boxStates[i] = eExchangeDailyBoxState.RECEIVED;
                }
                else
                { 
                    boxStates[i] = realCurCount < needCnts[i] ? eExchangeDailyBoxState.RECEIVE_DISABLE: eExchangeDailyBoxState.RECEIVE_ABLE;
                }
                switch(boxStates[i]) {
                    case eExchangeDailyBoxState.RECEIVED:
                        requireTexts[i].text = "";
                        dailyRewardBoxBtns[i].SetButtonSpriteState(false);
                        dailyRewardCheckObjs[i].SetActive(true);
                        SetBoxTween(i, false);
                        break;
                    case eExchangeDailyBoxState.RECEIVE_DISABLE:
                        requireTexts[i].text = string.Format("<color=red>{0}</color>/{1}", realCurCount, needCnts[i]);
                        dailyRewardBoxBtns[i].SetButtonSpriteState(true);
                        dailyRewardCheckObjs[i].SetActive(false);
                        SetBoxTween(i, false);
                        break;
                    case eExchangeDailyBoxState.RECEIVE_ABLE:
                        requireTexts[i].text = string.Format("{0}/{1}", realCurCount, needCnts[i]);
                        dailyRewardBoxBtns[i].SetButtonSpriteState(true);
                        dailyRewardCheckObjs[i].SetActive(true);
                        SetBoxTween(i, true);
                        break;
                }
            }

            if (dailyRewardTimeObj.Refresh == null)
            {
                dailyRewardTimeObj.Refresh = () =>
                {
                    int time = TimeManager.GetContentResetTime();
                    dailyRewardTimeText.text = TimeText(time);
                    if (time <= 0)
                    {
                        dailyRewardTimeObj.Refresh = null;
                    }
                };
            }
        }
        private string TimeText(int time)
        {
            return SBFunc.TimeString(time);//StringData.GetStringFormatByIndex(100002626, SBFunc.TimeString(time));
        }

        void SetBoxTween(int index,bool state)
        {
            if (state)
            {
                if (boxTween[index] == null)
                {
                    boxTween[index] = dailyRewardBoxBtns[index].transform.DOScale(1.2f,0.5f).SetLoops(-1, LoopType.Yoyo);
                }
            }
            else
            {
                boxTween[index].Kill();
                boxTween[index] = null;
            }
        }



        protected override void Init()
        {
            InitData();
            //dailyRewardPopup.SetActive(false);
            SetEffectInit();

        }

        void TutorialCheck()
        {
            if (TutorialManager.tutorialManagement.IsFinishedTutorial(TutorialDefine.Exchange) == false)
            {
                foreach (var obj in exchangeObjects)
                {
                    if (obj.IsBeggingAble())
                        obj.SetBeggingLayer();
                }
                TutorialManager.tutorialManagement.StartTutorial((int)TutorialDefine.Exchange);
            }
        }


        protected override void Clear()
        {
           
        }

        protected override void SetLockState()
        {
            
        }

        protected override void SetNotBuiltState()
        {
            
        }

        protected override void SetConstructingState()
        {
            
        }

        protected override void SetNormalState()
        {
            
        }

        public override void OnClickConstruct()
        {
            
        }

        public override void OnClickConstructFinish()
        {
            
        }

        public override void OnClickUpgrade()
        {
           
        }

        protected override void Refresh()
        {
           
        }
    }

}
