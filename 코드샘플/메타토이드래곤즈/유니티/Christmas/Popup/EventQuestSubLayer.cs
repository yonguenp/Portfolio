using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 어떤 이벤트 인지 간에 일일 이벤트 퀘스트로 이벤트 재화(주사위, 복주머니 등등)를 얻는 방식이 반복적으로 돌아서 최상단 구조 생성
    /// </summary>
    public class EventQuestSubLayer : SubLayer
    {
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
        [SerializeField]
        Text emptyLabelText = null;
        [SerializeField]
        GameObject botNode = null;//이벤트 만료 시점에는 하위 노드 (남은시간 + 한번에 받기 버튼) 꺼버림.

        protected List<Quest> eventQuestList = null;
        bool isFirstInit = false;
        int eventKey = -1;
        public virtual int GetEventKey()
        {
            return -1;
        }

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

        public override void ForceUpdate()
        {
            Init();
        }
        public override void Init()
        {
            if (missionTableView != null && !isFirstInit)
            {
                missionTableView.OnStart();
                isFirstInit = true;
            }

            SetAllButtonVisibleFalse();
            SetDailyMissionData();
            SetMissionTable();

            RefreshGetAllButton();
            RefreshRemainTime();
        }

        void SetAllButtonVisibleFalse()//모든 버튼(추가보상, 모두 받기, 각각의 레드닷) 끄기
        {
            btnGetAllReward.SetButtonSpriteState(false);
            getAllReddot.SetActive(false);
        }

        public virtual void OnClickGetAllMissionReward()//모두 받기 버튼
        {
            if (!isAvailableRewardQuest())//모두 받기를 할 수 있는 상황인지 선체크
            {
                ToastManager.On(100002519);//받을 보상이 없다는 토스트
                return;
            }

            var currentRewardAvailableList = GetAvailableRewardQuestList();
            QuestManager.Instance.RequestDailyEventAcceptableRewardQuest(currentRewardAvailableList, () =>
            {
                QuestManager.Instance.RequestQuestComplete(currentRewardAvailableList, () => {
                    ForceUpdate();
                });
            }
            , () =>
            {
                ForceUpdate();
            });
        }

        /// <summary>
        /// 정렬 규칙 - 보상 받을수 있는 퀘 / 미완료 / 완료
        /// </summary>
        protected virtual void SetDailyMissionData()//퀘스트 데이터 세팅(진행중 퀘스트 목록에서 보상 받으면 진행 데이터에서 빠지기 때문에,완료 목록 더해야함)
        {
            if (eventQuestList == null)
                eventQuestList = new List<Quest>();

            eventQuestList.Clear();

            eventQuestList = QuestManager.Instance.GetProceedQuestDataByType(eQuestType.EVENT, GetEventKey());
            var completeList = QuestManager.Instance.GetCompleteQuestDataByType(eQuestType.EVENT, GetEventKey());

            eventQuestList.AddRange(completeList);
            eventQuestList.Sort((d1, d2) => d1.ID - d2.ID);//quest_base Key 오름차순

            if (eventQuestList.Count <= 0)
                return;

            //상태 정렬
            var tempCompleteMission = new List<Quest>();//완료
            var tempInCompleteMission = new List<Quest>();//미완료
            var tempGetRewardMission = new List<Quest>();//보상획득 가능상태

            foreach (var quest in eventQuestList)
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

            eventQuestList = tempGetRewardMission.ToList();//정렬 리스트
        }

        void SetMissionTable()//퀘스트 데이터 기반 테이블 세팅
        {
            List<ITableData> tableViewMissionList = new List<ITableData>();
            if (eventQuestList != null && eventQuestList.Count > 0)
            {
                for (var i = 0; i < eventQuestList.Count; i++)
                {
                    if (eventQuestList[i] == null)
                    {
                        continue;
                    }

                    tableViewMissionList.Add(eventQuestList[i]);
                }
            }

            if (!IsEventPeriod())//이벤트 기간이 아니면 (end_time 이 지나버리면 강제로 퀘스트 데이터 삭제)
                tableViewMissionList.Clear();

            var isEmptyData = tableViewMissionList.Count <= 0;//이벤트 종료 시점에는 데이터 자체를 안줌. (완료든 진행중이든)

            if (emptyLabelText != null)
                emptyLabelText.gameObject.SetActive(isEmptyData);

            if (botNode != null)
                botNode.SetActive(!isEmptyData);

            missionTableView.SetDelegate(new TableViewDelegate(tableViewMissionList, (GameObject itemNode, ITableData item) =>
            {
                if (itemNode == null || item == null)
                    return;

                var frame = itemNode.GetComponent<EventMissionObj>();
                if (frame == null)
                    return;

                var questData = (Quest)item;
                if (questData == null)
                    return;
                frame.Init(questData, () => { ForceUpdate(); });
            }));
            missionTableView.ReLoad();
        }

        void RefreshGetAllButton()//모두 받기 버튼
        {
            var isAvailableReward = isAvailableRewardQuest();
            getAllReddot.SetActive(isAvailableReward);
            btnGetAllReward.SetButtonSpriteState(isAvailableReward);
        }

        bool isAvailableRewardQuest()//보상을 받을 수 있는 퀘스트가 있는지
        {
            if (eventQuestList == null || eventQuestList.Count <= 0)
                return false;

            foreach (var quest in eventQuestList)
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

            if (_quest.State == eQuestState.TERMINATE || _quest.State == eQuestState.PROCESS_DONE)//이미 완료한 퀘스트
                return false;

            if (_isSubQuest)//하위 퀘스트 조건만 체크
            {
                var isQuestTriggerSingle = _quest.IsSigleCondition;// 트리거 데이터 사이즈가 2이상이면 문제가 있음.
                if (!isQuestTriggerSingle)
                    return false;

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

            if (eventQuestList == null || eventQuestList.Count <= 0)
                return tempQuestIDList;

            foreach (var quest in eventQuestList)
            {
                if (IsGetRewardCondition(quest))
                    tempQuestIDList.Add(quest.ID);
            }
            return tempQuestIDList;
        }

        private void RefreshRemainTime()
        {
            if (remainTimeText == null)
                return;

            int time = TimeManager.GetEventContentResetTime();
            if (time > 0)
                remainTimeText.text = TimeText(time);
            else
                remainTimeText.text = TimeText(0);
        }
        private string TimeText(int time)
        {
            return StringData.GetStringFormatByIndex(100002626, SBFunc.TimeString(time));
        }

        /// <summary>
        /// 무조건 상속을 통한 구현 필수
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsEventPeriod() 
        {
            return true;
        }

        public override bool backBtnCall() { return base.backBtnCall(); } //백 버튼 콜백이 없으면 false 를 출력
    }
}

