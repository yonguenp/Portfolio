using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;

namespace SandboxNetwork
{
    public class ArenaBattleInfoData : ITableData
    {
        public int BattleListIndex { get; private set; } = 0;
        public string userName { get; private set; } = "";
        public int userBattlePoint { get; private set; } = 0;

        public List<DragonInfo> userDefenceTeamList = new List<DragonInfo>();
        public eMatchListState userBattleState = eMatchListState.OPEN;

        //public ArenaUserThumbnailInfoData userInfoData = null;
        public ArenaUserData UserInfoData { get; private set; } = null;
        public ArenaBattleInfoData(int index, ArenaTeamData matchData)
        {
            BattleListIndex = index;
            userName = matchData.Nick;
            userBattlePoint = matchData.DefBattlePoint < 0 ? 0 : matchData.DefBattlePoint;
            userDefenceTeamList = matchData.DefDeck;
            userBattleState = (eMatchListState)int.Parse(matchData.Status);

            if (UserInfoData == null)
                UserInfoData = new ArenaUserData(matchData.UID, matchData.Nick, matchData.PortraitIcon, matchData.Level, matchData.Point, matchData.RankGrade, matchData.EtcInfo, matchData.GuildNo, matchData.GuildName, matchData.GuildMarkNo, matchData.GuildEmblemNo);
        }
        public virtual void Init() { }
        public string GetKey() { return BattleListIndex.ToString(); }
    }
    public class ArenaBattleTabController : MonoBehaviour
    {
        [SerializeField]
        private TableView battleInfoTableView = null;
        [SerializeField]
        private GameObject ArenaBattleInfoSlotPrefab = null;

        [SerializeField]
        private Text refreshButtonLabel = null;
        [SerializeField]
        private TimeObject RefreshTimeObject = null;

        [SerializeField]
        private GameObject tempBattleBtnObj = null; // cbt에선 제거할 것

        private List<ITableData> ArenaOpponentList = new List<ITableData>();
        

        bool isAvailRefresh = false; // 공짜 리프레쉬 가능 여부

        bool isFirstTableInit = false;

        bool isTutorialing = false;
        public void Init()
        {
            if(!isFirstTableInit)
            {
                battleInfoTableView.OnStart();
                isTutorialing = TutorialManager.tutorialManagement.IsPlayingTutorialByGroup(TutorialDefine.Arena);
                isFirstTableInit = true;
            }
            
            ClearData();
            var currentList = ArenaManager.Instance.MatchList;
            if(currentList !=null && currentList.Count > 0)
            {
                int index = 0;
                for(int i=0; i<currentList.Count;++i)
                {
                    ArenaTeamData matchData = currentList[i];
                    if (matchData == null||matchData.Nick == "") continue;

                    ArenaBattleInfoData tempData = new ArenaBattleInfoData(index++, matchData);
                    ArenaOpponentList.Add(tempData);
                }
            }
            if (AutoRefreshBattleTab())
            {
                ArenaManager.Instance.RequestNewMatchList(RefreshBattleTab);
            }

            
            SetBattleInfoTabData();

#if UNITY_EDITOR
            if(tempBattleBtnObj != null) { 
                tempBattleBtnObj.gameObject.SetActive(true);
            }
#else
            if(tempBattleBtnObj != null) { 
                tempBattleBtnObj.gameObject.SetActive(false);
            }
#endif
        }

        void SetBattleInfoTabData()
        {
            if (battleInfoTableView == null || ArenaBattleInfoSlotPrefab == null) return;
            battleInfoTableView.SetDelegate(
                new TableViewDelegate(ArenaOpponentList, (GameObject node, ITableData item) => {
                    if (node == null) return;
                    var slotInfo = node.GetComponent<ArenaBattleInfoSlot>();
                    if (slotInfo == null) return;
                    ArenaBattleInfoData data = (ArenaBattleInfoData)item;
                    slotInfo.Init(data);
                    if (isTutorialing)
                    {
                        if (data.BattleListIndex == 0)
                            TutorialManager.Instance.SetRecordObject(1100101, node.GetComponent<RectTransform>());
                    }
                }));
            battleInfoTableView.ReLoad();
            RefreshButtonState();
        }
        public void RefreshBattleTab()
        {
            Init();
        }

        void RefreshButtonState()
        {
            var expireTime = ArenaManager.Instance.GetRefreshMatchListTime();
            if(expireTime> TimeManager.GetTime())
            {
                // 타임 오브젝트 if (RefreshTimeObject.Refresh == null) 안하는 이유 ㅣ Refresh가 null이 아닐때 서버에서 받은 시간을 적용할 수 없음 
                RefreshTimeObject.Refresh = () => {    //ex) 25분 상태에서 서버에서 시간을 다시 받아 30분이 되는 경우 처리 불가
                    float remain = TimeManager.GetTimeCompare(expireTime);
                    refreshButtonLabel.gameObject.SetActive(true);
                    refreshButtonLabel.text = SBFunc.TimeStringMinute(remain);
                    isAvailRefresh = false;
                    if (remain <= 0)
                    {
                        isAvailRefresh = true;
                        RefreshTimeObject.Refresh = null;
                        refreshButtonLabel.gameObject.SetActive(false);

                    }
                };
                
            }
            else
            {
                isAvailRefresh = true;
                refreshButtonLabel.gameObject.SetActive(false);
            }
        }

        void ClearData()
        {
            ArenaOpponentList.Clear();
        }
        public void OnClickRefreshButton()
        {
            // 무료 갱신이 가능할
            if (isAvailRefresh)
            {
                ArenaManager.Instance.RequestNewMatchList(RefreshBattleTab);
            }
            else
            {
               PopupManager.OpenPopup<ArenaMatchListRefreshPopup>().SetCallBack(RefreshBattleTab);
                // PopupManager.OpenPopup('PvPMatchListRefreshPopup', false, { });
            }
        }

        


        bool AutoRefreshBattleTab()//모든 대전 상대가 승부가 지어지면 자동 갱신 요청
        {
            if (ArenaOpponentList == null || ArenaOpponentList.Count <= 0) return false;
            int totalMatchCount = ArenaOpponentList.Count;
            int checkCount = 0;
            for(int i = 0; i < totalMatchCount; ++i)
            {
                var data = (ArenaBattleInfoData)ArenaOpponentList[i];
                if (data == null) continue;

                if(data.userBattleState ==eMatchListState.DEFENSE || data.userBattleState == eMatchListState.OFFENSE)
                {
                    ++checkCount;
                }
            }
            if (checkCount == totalMatchCount)
            {
                return true;
            }

            return false;
        }
    }

}