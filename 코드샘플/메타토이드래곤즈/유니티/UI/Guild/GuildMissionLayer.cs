using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class GuildMissionLayer : TabLayer
    {
        [SerializeField]
        Button[] tabs;
        [SerializeField]
        TableView missionTableView;
        [SerializeField]
        Text remainTimeText;
        [SerializeField]
        Button getAllBtn;
        [SerializeField]
        TimeEnable timeObject = null;
        [SerializeField]
        Text NoneLeftQeustAlertText = null;
        int curTabIndex = 0;
        bool isInit = false;
        List<int> currentRewardAvailableList = new List<int>();
        public override void InitUI(TabTypePopupData datas = null)
        {
            base.InitUI(datas);
            
            if (isInit == false)
            {
                isInit = true;
                missionTableView.OnStart();
            }
            CheckQuest();
            OnClickTab(curTabIndex);
        }
        
        void CheckQuest()
        {
            var quests = QuestManager.Instance.GetTotalQuestDataByGroup(eQuestGroup.Guild);
            if(quests.Count == 0) // 서버로부터 받은 퀘스트 정보가 없음 -  길드 최초 가입 케이스
            { 
                QuestManager.Instance.RequestSyncronizing(()=> {
                    OnClickTab(curTabIndex);
                });
            }
        }

        public void OnClickTab(int index)
        {
            curTabIndex = index;
            foreach (var tab in tabs)
            {
                if (tab != null)
                {
                    tab.interactable = true;
                }
            }
            tabs[index].interactable = false;
            DrawScrollView();
            SetRemainTimeText((eGuildMissionType)index);
        }
        void SetRemainTimeText(eGuildMissionType type)
        {
            switch (type)
            {
                case eGuildMissionType.Daily:
                    if (timeObject != null)
                    {
                        timeObject.Refresh = RefreshRemainDailyTime;
                    }
                    break;
                case eGuildMissionType.Weekly:
                    if (timeObject != null)
                    {
                        timeObject.Refresh = RefreshRemainWeeklyTime;
                    }
                    break;
                case eGuildMissionType.Left:
                    if (timeObject != null)
                    {
                        timeObject.Refresh = RefreshRemainWeeklyTime;
                        remainTimeText.text = string.Empty;
                    }
                    break;
            }
        }
        void DrawScrollView()
        {
            var CheckQuestType = GetQuestTypeByIndex((eGuildMissionType)curTabIndex);
            var guildProceedQuest = QuestManager.Instance.GetProceedUIData(CheckQuestType, eQuestGroup.Guild);//현재 진행중 퀘
            var guildCompleteQuest = QuestManager.Instance.GetCompleteUIData(CheckQuestType, eQuestGroup.Guild);//완료한 길퀘
            
            RefreshClearAbleQuest(guildProceedQuest, guildCompleteQuest, (eGuildMissionType)curTabIndex);

            List<Quest> totalQuestList = new List<Quest>();
            totalQuestList.AddRange(guildProceedQuest);
            totalQuestList = totalQuestList.OrderBy(quest =>
            {
                bool isRewardAble = IsGetRewardCondition(quest);//진행 중이지만 보상 안받음
                if (isRewardAble)
                    return -1;
                else
                    return 1;
            }).ToList();
            totalQuestList.AddRange(guildCompleteQuest);


            List<ITableData> tableDatas = new List<ITableData>();
            tableDatas = totalQuestList.ToList<ITableData>();
            if(tableDatas.Count == 0)
            {
                NoneLeftQeustAlertText.gameObject.SetActive(true);
                NoneLeftQeustAlertText.text = StringData.GetStringByStrKey("guild_desc:114");
            }
            else
            {
                NoneLeftQeustAlertText.gameObject.SetActive(false);
                NoneLeftQeustAlertText.text = string.Empty;
            }

            missionTableView.SetDelegate(new TableViewDelegate(tableDatas, (GameObject itemNode, ITableData item) =>
            {
                if (itemNode == null) return;
                var slotInfo = itemNode.GetComponent<GuildMissionObject>();
                if (slotInfo == null) return;
                Quest data = (Quest)item;
                eGuildMissionType curType = (eGuildMissionType)curTabIndex;
                slotInfo.Init(data,()=> DrawScrollView());
            }));
            missionTableView.ReLoad(true);
        }

        void RefreshClearAbleQuest(List<Quest> _proceedQuestList , List<Quest> _completeQuestList, eGuildMissionType _type)
        {
            currentRewardAvailableList.Clear();
            currentRewardAvailableList = _proceedQuestList.Where(element => IsGetRewardCondition(element)).Select(item => item.ID).ToList();
            getAllBtn.SetButtonSpriteState(currentRewardAvailableList.Count > 0);

            if(_type == eGuildMissionType.Left)
            {
                var visibleCheck = _proceedQuestList.Count <= 0 && _completeQuestList.Count <= 0;
                getAllBtn.gameObject.SetActive(!visibleCheck);
            }
            else
                getAllBtn.gameObject.SetActive(true);
        }

        eQuestType GetQuestTypeByIndex(eGuildMissionType _type)
        {
            return _type switch
            {
                eGuildMissionType.Daily => eQuestType.DAILY,
                eGuildMissionType.Weekly => eQuestType.WEEKLY,
                eGuildMissionType.Left => eQuestType.CHAIN,
                _ => eQuestType.DAILY,
            };
        }

        public void OnClickGetAll()
        {
            if (currentRewardAvailableList.Count > 0) { 
                QuestManager.Instance.RequestQuestComplete(currentRewardAvailableList, () => {
                    DrawScrollView();
                });
            }
        }
        bool IsGetRewardCondition(Quest _quest)//보상을 받을 수 있는 상태인가?
        {
            if (_quest == null)
                return false;

            if (_quest.IsAlreadyGetRewards())//이미 완료한 퀘스트
                return false;

            if (_quest.IsQuestClear())//진행 중인 퀘스트만 체크
                return true;

            return false;
        }

        private void RefreshRemainDailyTime()
        {
            if (remainTimeText == null)
                return;

            int time = TimeManager.GetContentResetTime();
            if (time > 0)
                remainTimeText.text = TimeText(time);
            else
                remainTimeText.text = TimeText(0);
        }

        private void RefreshRemainWeeklyTime()
        {
            if (remainTimeText == null)
                return;

            var specifictimeStamp = TimeManager.GetTimeCompare(TimeManager.GetSpecificNextDay());
            int time = specifictimeStamp;
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
