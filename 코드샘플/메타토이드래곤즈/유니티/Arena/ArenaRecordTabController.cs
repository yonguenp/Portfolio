using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace SandboxNetwork { 
    public class ArenaRecordInfoData : ITableData
    {
        public int battleListIndex = 0;
        public string userName = "";
        public int userBattlePoint = 0;
        public List<DragonInfo> userATKTeamList = new List<DragonInfo>();
        public eMatchListState userBattleState = eMatchListState.OPEN;

        public int resultArenaPoint = 0;
        public int recordTime = 0;
        public int recordDay = 0;
        public int recordHour = 0;
        public int recordMinutes = 0;

        public ArenaUserData UserData { get; private set; } = null;
        public ArenaRecordInfoData(int index, ArenaTeamData defenceData)
        {
            battleListIndex = index;
            userName = defenceData.Nick;
            userBattlePoint = defenceData.AtkBattlePoint;
            userATKTeamList = defenceData.AtkDeck;
            userBattleState = (eMatchListState)int.Parse(defenceData.Status);
            recordDay = defenceData.day;
            recordHour = defenceData.hour;
            recordMinutes = defenceData.minutes;
            resultArenaPoint = defenceData.ChangePoint;

            if (UserData == null)
                UserData = new ArenaUserData(defenceData.UID, defenceData.Nick, defenceData.PortraitIcon, defenceData.Level, defenceData.Point, defenceData.RankGrade, defenceData.EtcInfo,defenceData.GuildNo, defenceData.GuildName, defenceData.GuildMarkNo, defenceData.GuildEmblemNo);
        }
        public virtual void Init() { }
        public string GetKey() { return battleListIndex.ToString(); }

    }
    public class ArenaRecordTabController : MonoBehaviour
    {
        [SerializeField]
        private TableView recordInfoTableView = null;

        bool changeIndexFlag = false;
        public bool ChangeIndexFlag{
            get
            {
                return changeIndexFlag;
            }
            }
        public int tempPageIndex = 0;

        List<ITableData> ArenaOpponentList = new List<ITableData>();

        [Header("page info")]
        [SerializeField]
        private Text currentPageLabel = null;
        [SerializeField]
        private Button leftButton = null;
        [SerializeField]
        private Button rightButton = null;

        [SerializeField]
        private GameObject noneRecordAlarmObj = null;




        private bool isFirst = false;
        public void Init()
        {

            ClearData();
            var currentList = ArenaManager.Instance.DefenceList;
            noneRecordAlarmObj.SetActive(currentList.Count == 0);
            if(currentList !=null && currentList.Count > 0)
            {
                for(int i=0; i < currentList.Count; ++i)
                {
                    var defenceData = currentList[i];
                    if (defenceData == null) continue;

                    var tempData = new ArenaRecordInfoData(i, defenceData);
                    
                    ArenaOpponentList.Add(tempData);
                }
                if (isFirst == false)
                {
                    recordInfoTableView.OnStart();
                    isFirst = true;
                }
                SetRecordInfoTabData();
                RefreshPageButton();
            }
        }
        
        void SetRecordInfoTabData()
        {
            if (recordInfoTableView == null||  ArenaOpponentList.Count<=0) return;
            recordInfoTableView.SetDelegate(
                new TableViewDelegate(ArenaOpponentList, ( GameObject node, ITableData item) => {
                    if (node == null) return;
                    var slotInfo = node.GetComponent<ArenaRecordInfoSlot>();
                    if (slotInfo == null) return;
                    slotInfo.Init((ArenaRecordInfoData)item);
                    node.SetActive(true);
            }));
            recordInfoTableView.ReLoad();
        }
        public void onClickLeftButton()
        {
            changeIndexFlag = true;
            tempPageIndex = ArenaManager.Instance.CurrentDefencePage - 1;
            ArenaManager.Instance.RefreshUI();
        }
        public void onClickRightButton()
        {
            changeIndexFlag = true;
            tempPageIndex = ArenaManager.Instance.CurrentDefencePage + 1;
            ArenaManager.Instance.RefreshUI();
        }

        public void RefreshPageButton()
        {
            var totalPage = ArenaManager.Instance.TotalDefencePage;
            var currentPage = ArenaManager.Instance.CurrentDefencePage;

            rightButton.SetInteractable(!(currentPage + 1 >= totalPage));
            leftButton.SetInteractable(!(currentPage <=0));
            if (currentPageLabel != null)
            {
                currentPageLabel.text = (currentPage + 1).ToString();
            }
        }

        void ClearData()
        {
            ArenaOpponentList.Clear();
            tempPageIndex = 0;
            changeIndexFlag = false;

        }
    }
}