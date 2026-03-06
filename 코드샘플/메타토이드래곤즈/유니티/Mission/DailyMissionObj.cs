using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork { 
    public class DailyMissionObj : MissionUIObject
    {
        [SerializeField]
        protected Button btnGetADReward = null;

        protected override void InitRewardButton()//일단 다끔
        {
            base.InitRewardButton();
            if (btnGetADReward != null)
                btnGetADReward.gameObject.SetActive(false);
        }
        protected override void RefreshRewardButton()
        {
            var isGetRewards = currentQuest.IsAlreadyGetRewards();//이미 보상 수령
            var rewardButton = GetRewardButtonByType();
            var isClear = currentQuest.IsQuestClear();

            if (isGetRewards)//이미 보상 받았으면
            {
                rewardButton.gameObject.SetActive(false);
                btnShortCut.gameObject.SetActive(false);
                btnComplete.gameObject.SetActive(true);
                completeDimmedObject.SetActive(true);
            }
            else
            {
                rewardButton.gameObject.SetActive(isClear);
                rewardButton.SetButtonSpriteState(isClear);
                SBFunc.GetChildrensByName(rewardButton.transform, "reddot").gameObject.SetActive(isClear);
                btnShortCut.gameObject.SetActive(!isClear);
            }
        }

        Button GetRewardButtonByType()//광고 타입 & 광고제거 유저인지 아닌지를 제어
        {
            if (IsADRewardType() && !User.Instance.ADVERTISEMENT_PASS)
                return btnGetADReward;
            else
                return btnGetReward;
        }

        protected bool IsADRewardType()//광고를 봐야지 보상을 받는 타입인가
        {
            if (currentQuest == null)
                return false;

            var triggerData = currentQuest.GetSingleTriggerData();

            return QuestCondition.IsAdvertiseQuestType(triggerData.TYPE);
        }
        protected override void SetRewardItem()
        {
            rewardList = currentQuest.GetReward();
            if (rewardList == null || rewardList.Count <= 0)
                return;

            if(rewardList.Count == 1)//보상은 무조건 다이아 1종이라고 픽스.
            {
                var assetData = rewardList[0];
                rewardItemList[0].SetFrameItem(assetData.ItemNo, assetData.Amount, (int)assetData.GoodType);
            }
        }

        public void OnClickAdReward()//광고 보고 보상 받기
        {
            if (currentQuest.IsAlreadyGetRewards())
            {
                ToastManager.On(StringData.GetStringByStrKey("일일보상오류"));
                return;
            }

            QuestManager.Instance.RequestAcceptableRewardQuest(currentQuest, () =>
            {
                AdvertiseManager.Instance.TryADWithPopup((log) => {
                    //광고 시청 이후
                    QuestManager.Instance.RequestQuestComplete(currentQuest.ID, () => {
                        if (getRewardDelegate != null)
                            getRewardDelegate();
                    }, log);
                }, () => { ToastManager.On(100007692); });//더이상 광고를 불러올 수 없습니다.
            }
            , () =>
            {
                if (getRewardDelegate != null)//UI 갱신
                    getRewardDelegate();
            });
        }
        public override void OnClickGetReward()
        {
            if (currentQuest.IsAlreadyGetRewards())
            {
                ToastManager.On(StringData.GetStringByStrKey("일일보상오류"));
                return;
            }

            QuestManager.Instance.RequestAcceptableRewardQuest(currentQuest, () =>
            {
                QuestManager.Instance.RequestQuestComplete(currentQuest.ID, () => {
                    if (getRewardDelegate != null)
                        getRewardDelegate();
                });
            }
            , () =>
            {
                if (getRewardDelegate != null)//UI 갱신
                    getRewardDelegate();
            });
        }
    }
}