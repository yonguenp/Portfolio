using DG.Tweening;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DailyMissionLayer : TabLayer
    {
        [Header("Left")]
        [SerializeField]
        Text missionProgressText = null;
        [SerializeField]
        Button btnGetAddtionalReward = null;
        [SerializeField]
        Text addRewardCountText = null;
        [SerializeField]
        GameObject GetAdditionalRewardReddot = null;
        [SerializeField]
        GameObject advIconObj = null;

        [Space(10)]
        [Header("right")]
        [SerializeField]
        TableView missionTableView = null;
        [SerializeField]
        Text remainTimeText = null;
        [SerializeField]
        TimeEnable timeObject = null;
        [SerializeField]
        Button btnGetAllReward = null;
        [SerializeField]
        GameObject getAllReddot = null;

        private List<Quest> dailyMissionList = null;//퀘스트 클리어와는 별개로 보상을 수령했는지는 어떻게 할지 논의
        private Quest dailyHeadQuest = null;//해당 데이터는 무조건 0번 인덱스퀘스트 데이터 :: (오늘의 퀘스트 0/6) 일퀘 전체를 아우르는 퀘스트

        bool isFirstInit = false;

        private void Start()
        {
            InitTimeEnable();
        }
        void InitTimeEnable()
        {
            if (timeObject != null)
            {
                timeObject.Refresh = RefreshRemainTime;
            }
        }

        public override void RefreshUI()
        {
            base.RefreshUI();

            if(missionTableView != null && !isFirstInit)
            {
                missionTableView.OnStart();
                isFirstInit = true;
            }

            SetAllButtonVisibleFalse();
            SetDailyMissionData();
            SetMissionTable();
            SetAdditionalItem();

            RefreshMissionProgress();
            RefreshGetAllButton();
            RefreshAddRewardButton();
            RefreshRemainTime();
        }

        void SetAllButtonVisibleFalse()//모든 버튼(추가보상, 모두 받기, 각각의 레드닷) 끄기
        {
            btnGetAddtionalReward.SetButtonSpriteState(false);
            btnGetAllReward.SetButtonSpriteState(false);
            GetAdditionalRewardReddot.SetActive(false);
            getAllReddot.SetActive(false);
        }

        void SetAdditionalItem()//추가 보상 아이템 데이터 세팅
        {
            if(dailyHeadQuest == null)
            {
                return;
            }

            var rewardItems = dailyHeadQuest.GetReward();
            if (rewardItems == null || rewardItems.Count <= 0)
                return;

            if(rewardItems.Count == 1)//보상은 다이아만으로 고정한다고 함.
            {
                var reward = rewardItems[0];
                addRewardCountText.text = SBFunc.StrBuilder(reward.Amount,StringData.GetStringByIndex(100000082));//"개"
            }
        }

        public void OnClickGetAdditionalReward()//추가 보상 받기 버튼
        {
            var isAvailable = IsGetRewardCondition(dailyHeadQuest, false);
            if (!isAvailable)
            {
                ToastManager.On(100002519);
                return;
            }

            if (IsHeadQuestTypeAD() && !User.Instance.ADVERTISEMENT_PASS)
                RequestRewardByAD();
            else
                RequestAddReward();
        }
        bool IsHeadQuestTypeAD()//헤드 퀘스트 (추가 보상 관리 퀘스트의 타입이 광고 타입인가?
        {
            if (dailyHeadQuest == null)
                return false;

            return QuestCondition.IsAdvertiseQuestType(dailyHeadQuest.GetSingleConditionData().TriggerData.TYPE);
        }
        void RequestRewardByAD()//광고 보고 보상 요청
        {
            QuestManager.Instance.RequestAcceptableRewardQuest(dailyHeadQuest, () =>
            {
                AdvertiseManager.Instance.TryADWithPopup((log) => {
                    //광고 시청 이후
                    QuestManager.Instance.RequestQuestComplete(dailyHeadQuest.ID, () => {
                        RefreshUI();
                    }, log);
                }, () => { ToastManager.On(100007692); });//더이상 광고를 불러올 수 없습니다.
            }
            , () =>
            {
                RefreshUI();
            });
        }

        void RequestAddReward()//일반 보상 요청
        {
            QuestManager.Instance.RequestAcceptableRewardQuest(dailyHeadQuest, () =>
            {
                QuestManager.Instance.RequestQuestComplete(dailyHeadQuest.ID, () =>
                {
                    RefreshUI();
                });
            }
            , () =>
            {
                RefreshUI();
            });
        }  

        public void OnClickGetAllMissionReward()//모두 받기 버튼
        {
            if(!isAvailableRewardQuest())//모두 받기를 할 수 있는 상황인지 선체크
            {
                ToastManager.On(100002519);//받을 보상이 없다는 토스트
                return;
            }

            var currentRewardAvailableList = GetAvailableRewardQuestList();
            QuestManager.Instance.RequestAcceptableRewardQuest(dailyHeadQuest, () =>
            {
                QuestManager.Instance.RequestQuestComplete(currentRewardAvailableList, () => {
                    RefreshUI();
                });
            }
            , () =>
            {
                RefreshUI();
            });
        }

        /// <summary>
        /// 정렬 규칙 - 보상 받을수 있는 퀘 / 미완료 / 완료
        /// </summary>
        void SetDailyMissionData()//퀘스트 데이터 세팅(진행중 퀘스트 목록에서 보상 받으면 진행 데이터에서 빠지기 때문에,완료 목록 더해야함)
        {
            if (dailyMissionList == null)
                dailyMissionList = new List<Quest>();

            dailyMissionList.Clear();

            dailyMissionList = QuestManager.Instance.GetProceedUIData(eQuestType.DAILY, eQuestGroup.Normal);
            var completeList = QuestManager.Instance.GetCompleteUIData(eQuestType.DAILY, eQuestGroup.Normal);

            dailyMissionList.AddRange(completeList);
            dailyMissionList.Sort((d1, d2) => d1.ID - d2.ID);//quest_base Key 오름차순

            if (dailyMissionList.Count <= 0)
                return;

            dailyHeadQuest = dailyMissionList[0];//가장 첫번째 퀘스트는 전체 관리 퀘스트
            dailyMissionList = dailyMissionList.GetRange(1, dailyMissionList.Count - 1);//첫번째는 일퀘 전체를 체크하는 퀘스트라 제거

            //상태 정렬
            var tempCompleteMission = new List<Quest>();//완료
            var tempInCompleteMission = new List<Quest>();//미완료
            var tempGetRewardMission = new List<Quest>();//보상획득 가능상태

            foreach (var quest in dailyMissionList)
            {
                if (quest == null)
                    continue;

                if (completeList.Contains(quest))
                    tempCompleteMission.Add(quest);
                else
                {
                    var isGetReward = IsGetRewardCondition(quest);
                    if (isGetReward)
                        tempGetRewardMission.Add(quest);
                    else
                        tempInCompleteMission.Add(quest);
                }
            }

            tempGetRewardMission.AddRange(tempInCompleteMission);
            tempGetRewardMission.AddRange(tempCompleteMission);

            dailyMissionList = tempGetRewardMission.ToList();//정렬 리스트
        }

        void SetMissionTable()//퀘스트 데이터 기반 테이블 세팅
        {
            List<ITableData> tableViewMissionList = new List<ITableData>();
            if (dailyMissionList != null && dailyMissionList.Count > 0)
            {
                for (var i = 0; i < dailyMissionList.Count; i++)
                {
                    if (dailyMissionList[i] == null)
                    {
                        continue;
                    }
                    tableViewMissionList.Add(dailyMissionList[i]);
                }
            }
            missionTableView.SetDelegate(new TableViewDelegate(tableViewMissionList, (GameObject itemNode, ITableData item) =>
            {
                if (itemNode == null || item == null)
                    return;

                var frame = itemNode.GetComponent<DailyMissionObj>();
                if (frame == null)
                    return;

                var questData = (Quest)item;
                if (questData == null)
                    return;
                frame.Init(questData, ()=> { RefreshUI(); });
            }));
            missionTableView.ReLoad();
        }

        void RefreshGetAllButton()//모두 받기 버튼
        {
            //모두 받기 버튼
            var isAvailableReward = isAvailableRewardQuest();
            getAllReddot.SetActive(isAvailableReward);
            btnGetAllReward.SetButtonSpriteState(isAvailableReward);
        }

        void RefreshAddRewardButton()//추가 보상 받기 버튼
        {
            if (dailyHeadQuest != null)
            {
                var isADType = QuestCondition.IsAdvertiseQuestType(dailyHeadQuest.GetSingleTriggerData().TYPE) && !User.Instance.ADVERTISEMENT_PASS;//광고타입 & 광고제거 유저
                advIconObj.SetActive(isADType);//광고 아이콘

                var isGetReward = IsGetRewardCondition(dailyHeadQuest, false);
                btnGetAddtionalReward.SetButtonSpriteState(isGetReward);
                GetAdditionalRewardReddot.SetActive(isGetReward);
            }
        }

        bool isAvailableRewardQuest()//보상을 받을 수 있는 퀘스트가 있는지
        {
            if (dailyMissionList == null || dailyMissionList.Count <= 0)
                return false;

            foreach(var quest in dailyMissionList)
            {
                if (IsGetRewardCondition(quest))
                    return true;
            }
            return false;
        }

        bool IsGetRewardCondition(Quest _quest, bool _isSubQuest = true)//보상을 받을 수 있는 상태인가?
        {
            if (_quest == null)
                return false;

            if (_quest.IsAlreadyGetRewards())//이미 완료한 퀘스트
                return false;

            if(_isSubQuest)//하위 퀘스트 조건만 체크 - 대장 퀘스트는 앞단에서 선체크 해줌.
            {
                var isShowAD = QuestCondition.IsAdvertiseQuestType(_quest.GetSingleTriggerData().TYPE) && !User.Instance.ADVERTISEMENT_PASS;
                if (isShowAD)//questTrigger가 광고 타입 && 광고제거 없으면
                    return false;
            }

            if (_quest.IsQuestClear())//진행 중인 퀘스트만 체크
                return true;

            return false;
        }

        List<int> GetAvailableRewardQuestList()//보상 받을 수 있는 퀘스트의 인덱스 가져오기
        {
            List<int> tempQuestIDList = new List<int>();

            if (dailyMissionList == null || dailyMissionList.Count <= 0)
                return tempQuestIDList;

            foreach (var quest in dailyMissionList)
            {
                if (IsGetRewardCondition(quest))
                    tempQuestIDList.Add(quest.ID);
            }
            return tempQuestIDList;
        }

        void RefreshMissionProgress()//현재 완료한 퀘스트 갯수 기반 프로그레스 갱신
        {
            if (dailyHeadQuest == null)
                return;

            if (dailyHeadQuest.IsSigleCondition)
            {
                var conditionData = dailyHeadQuest.GetSingleConditionData();
                var totalCount = conditionData.CompleteValue;

                int clearCount;
                if (dailyHeadQuest.IsAlreadyGetRewards())//이미 보상 수령
                    clearCount = totalCount;
                else
                    clearCount = conditionData.CurrentValue;

                if (missionProgressText != null)
                    missionProgressText.text = string.Format("{0}/{1}", clearCount, totalCount);
            }
        }

        private void RefreshRemainTime()
        {
            if (remainTimeText == null)
                return;

            int time = TimeManager.GetContentResetTime();
            if (time > 0)
                remainTimeText.text = TimeText(time);
            else
                remainTimeText.text = TimeText(0);
        }
        private string TimeText(int time)
        {
            return StringData.GetStringFormatByIndex(100002626, SBFunc.TimeString(time));
        }
    }
}

