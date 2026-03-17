using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 기본 미션 UI 오브젝트 
    /// </summary>
    /// 
    public class MissionUIObject : MonoBehaviour
    {
        [SerializeField]
        protected Text missionText = null;
        [SerializeField]
        protected Slider missionProgressSlider = null;
        [SerializeField]
        protected Text missionProgressText = null;
        [SerializeField]
        protected Button btnGetReward = null;
        
        [SerializeField]
        protected Button btnShortCut = null;
        [SerializeField]
        protected Button btnComplete = null;
        [SerializeField]
        protected GameObject completeDimmedObject = null;

        [SerializeField]
        protected GameObject shortCutAbleTextObj = null;  // 바로 가기 텍스트
        [SerializeField]
        protected GameObject shortCutLockIcon = null;  // 자물쇠 아이콘

        [SerializeField]
        protected List<ItemFrame> rewardItemList = new List<ItemFrame>();

        protected Quest currentQuest = null;
        protected VoidDelegate getRewardDelegate = null;
        protected List<Asset> rewardList = null;
        
        public virtual void Init(Quest _data, VoidDelegate _rewardDelegate = null) // 세팅해야 할 것 미션 내용, 미션 진행 정도, 미션 보상, 바로가기 관한 것
        {
            if (_data == null)
            {
                Debug.LogError("퀘스트 데이터 누락");
                return;
            }

            if (_rewardDelegate != null)
                getRewardDelegate = _rewardDelegate;

            currentQuest = _data;
            SetMissionInfo();
            SetRewardItem();
            SetButtonState();
        }

        protected void SetMissionInfo()
        {
            var conditionData = currentQuest.GetSingleConditionData();
            if (conditionData == null)
                return;

            var subject = currentQuest.GetQuestSubjectNotie();
            var max = conditionData.CompleteValue;
            var current = conditionData.CurrentValue;
            if (currentQuest.IsAlreadyGetRewards())//이미 보상 수령 (완료처리)
                current = max;

            missionText.text = subject;
            missionProgressSlider.maxValue = max;
            missionProgressSlider.value = current;
            missionProgressText.text = string.Format("{0}/{1}", current, max);
        }

        protected void SetButtonState()
        {
            completeDimmedObject.gameObject.SetActive(false);
            InitRewardButton();//버튼 일단 다끔
            RefreshRewardButton();//보상 버튼 갱신
            RefreshShortcutButton();//바로가기 버튼 갱신
        }

        protected virtual void RefreshRewardButton()
        {
            var isGetRewards = currentQuest.IsAlreadyGetRewards();//이미 보상 수령
            var isClear = currentQuest.IsQuestClear();

            if (isGetRewards)//이미 보상 받았으면
            {
                btnGetReward.gameObject.SetActive(false);
                btnShortCut.gameObject.SetActive(false);
                btnComplete.gameObject.SetActive(true);
                completeDimmedObject.SetActive(true);
            }
            else
            {
                btnGetReward.gameObject.SetActive(isClear);
                btnGetReward.SetButtonSpriteState(isClear);
                SBFunc.GetChildrensByName(btnGetReward.transform, "reddot").gameObject.SetActive(isClear);
                btnShortCut.gameObject.SetActive(!isClear);
            }
        }

        protected virtual void RefreshShortcutButton()
        {
            var triggerData = currentQuest.GetSingleTriggerData();
            var isPossibleShortCut = QuestManager.Instance.IsAvailableDirect(triggerData);
            eQuestCompleteCondType completeState = (eQuestCompleteCondType)QuestCondition.strCondition.IndexOf(triggerData.TYPE);
            btnShortCut.SetButtonSpriteState(isPossibleShortCut);
            shortCutAbleTextObj.SetActive(isPossibleShortCut);//바로가기 라벨
            shortCutLockIcon.SetActive(!isPossibleShortCut);//바로가기 잠금 라벨

            btnShortCut.onClick.RemoveAllListeners();
            if (isPossibleShortCut) // 바로 가기 가능
            {
                btnShortCut.onClick.AddListener(() => {
                    var triggerData = currentQuest.GetSingleTriggerData();
                    if (triggerData != null)
                    {
                        QuestManager.Instance.DirectGotoTarget(triggerData);
                    }
                });
            }
            else // 바로가기 - 컨텐츠 뚫어야 이용가능
            {
                btnShortCut.onClick.AddListener(() =>
                {
                    switch (completeState)
                    {
                        case eQuestCompleteCondType.GAIN_GEMDUNGEON:
                            ToastManager.On(StringData.GetStringByStrKey("guild_desc:121"));
                            break;
                        default:
                            ToastManager.On(StringData.GetStringByStrKey("퀘스트바로가기오류"));
                            break;
                    }
                    
                });
            }
        }

        /// <summary>
        /// 보상 세팅
        /// </summary>
        protected virtual void SetRewardItem()
        {
            rewardList = currentQuest.GetReward();
            if (rewardList == null || rewardList.Count <= 0)
                return;

            if(rewardItemList != null && rewardItemList.Count > 0)
            {
                var uiItemCount = rewardItemList.Count;
                foreach (var itemUI in rewardItemList)
                    if (itemUI != null)
                        itemUI.gameObject.SetActive(false);

                for(int i = 0; i< rewardList.Count; i++)
                {
                    var asset = rewardList[i];
                    if (uiItemCount > i)
                    {
                        rewardItemList[i].gameObject.SetActive(true);
                        rewardItemList[i].SetFrameItem(asset);
                    }
                }
            }
        }
        /// <summary>
        /// 일단 디폴트는 하루기준
        /// </summary>
        public virtual void OnClickGetReward()
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

        protected virtual void InitRewardButton()//일단 다끔
        {
            if (btnGetReward != null)
                btnGetReward.gameObject.SetActive(false);
            if (btnShortCut != null)
                btnShortCut.gameObject.SetActive(false);
            if (btnComplete != null)
                btnComplete.gameObject.SetActive(false);
        }
    }
}

