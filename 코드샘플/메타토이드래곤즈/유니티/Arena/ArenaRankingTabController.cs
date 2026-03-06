using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;

namespace SandboxNetwork
{
    public class ArenaRankingInfoData : ITableData
    {
        public string userName = "";

        public int userBattlePoint = 0;
        public List<DragonInfo> userDefenceTeamList = new List<DragonInfo>();
        public int userRank = 0;
        public string userRankStr = "";

        public ArenaUserData UserData { get; set; } = null;
        public ArenaRankingInfoData(ArenaBaseData userArenatData, int point, List<DragonInfo> dragons)
        {
            userName = User.Instance.UserData.UserNick;
            userBattlePoint = point;
            userDefenceTeamList = dragons;
            userRank = userArenatData.season_rank;

            if (UserData == null)
                UserData = new ArenaUserData(User.Instance, userArenatData.season_point, userArenatData.SeasonGrade);
        }

        public ArenaRankingInfoData(ArenaTeamData rankInfo)
        {
            userName = rankInfo.Nick;
            userBattlePoint = rankInfo.DefBattlePoint;
            userDefenceTeamList = rankInfo.DefDeck;
            userRank = rankInfo.Rank;

            if (UserData == null)
                UserData = new ArenaUserData(rankInfo.UID, rankInfo.Nick, rankInfo.PortraitIcon, rankInfo.Level, rankInfo.Point, rankInfo.RankGrade, rankInfo.EtcInfo, rankInfo.GuildNo, rankInfo.GuildName, rankInfo.GuildMarkNo, rankInfo.GuildEmblemNo);
        }

        public virtual void Init() { }
        public string GetKey() {
            if (UserData == null)
                return "";

            return UserData.PortraitIcon; 
        }
    }
    
    public class ArenaRankingTabController : MonoBehaviour
    {
        [SerializeField]
        private TableView rankingInfoTableView = null;

        [SerializeField]
        private ArenaRankingInfoSlot myArenaRankingInfoSlot = null;

        ArenaRankingInfoData myRankingData = null;
        List<ITableData> ArenaOpponentList = new List<ITableData>();

        const int MaxDragonDeckSeatCount = 6;//드래곤 자리 카운트
        private bool isFirst = false;
        public void Init()
        {
            if (isFirst == false)
            {
                rankingInfoTableView.OnStart();
                isFirst = true;
            }
            ClearData();
            var rankList = ArenaManager.Instance.RankingList;

            if(rankList !=null && rankList.Count > 0)
            {
                for(int i = 0; i < rankList.Count; ++i)
                {
                    var rankInfo = rankList[i];
                    if (rankInfo == null) continue;
                    var tempData = new ArenaRankingInfoData(rankInfo);
                    
                    ArenaOpponentList.Add(tempData);
                }
            }
            var myArenaData = ArenaManager.Instance.UserArenaData;
            var myData = new ArenaRankingInfoData(myArenaData, RefreshDefenceBattlePoint(), GetMyDefTeamData());            
            myRankingData = myData;

            SetRankingInfoTabData();

        }
        int RefreshDefenceBattlePoint()
        {
            //int currentTeamPresetNo = CacheUserData.GetInt("presetArenaDefDeck", 0);
            var defBattleLine = User.Instance.PrefData.ArenaFormationData.TeamFormationDEF[0];
            return CalcTotalBattlePoint(defBattleLine);
        }
        int CalcTotalBattlePoint(List<int> dragonTagList)
        {
            var totalPoint = 0;
            if(dragonTagList==null || dragonTagList.Count <= 0)
            {
                return totalPoint;
            }
            foreach(int tag in dragonTagList)
            {
                var dragonData = User.Instance.DragonData.GetDragon(tag);
                if (dragonData == null) continue;

                totalPoint += (int)dragonData.Status.GetTotalINF();//레벨 스탯, 장비, 등등 포함 총 값
            }
            return totalPoint;
        }

        List<DragonInfo> GetMyDefTeamData()
        {
            List<DragonInfo> tempList = new List<DragonInfo>();
            //int currentTeamPresetNo = CacheUserData.GetInt("presetArenaDefDeck", 0);
            var currentBattleLine = User.Instance.PrefData.ArenaFormationData;
            var defBattleLine = currentBattleLine.TeamFormationDEF[0];
            if (defBattleLine == null || defBattleLine.Count <= 0)
            {
                for (int i = 0; i < MaxDragonDeckSeatCount; ++i)
                {
                    tempList.Add(new DragonInfo(0, 0));
                }
                return tempList;
            }
            foreach (int tag in defBattleLine)
            {

                if(tag <= 0)
                {
                    tempList.Add(new DragonInfo(0, 0));
                    continue;
                }
                var dragonData = User.Instance.DragonData.GetDragon(tag);
                if (dragonData == null) continue;

                tempList.Add(new DragonInfo(tag, dragonData.Level, dragonData.TranscendenceStep));
            }
            return tempList;
        }
        void SetRankingInfoTabData()
        {
            if (rankingInfoTableView == null || ArenaOpponentList.Count <= 0) return;
            
            rankingInfoTableView.SetDelegate(
                new TableViewDelegate(ArenaOpponentList,(GameObject node, ITableData item) => {
                    if (node == null) return;
                    var slotInfo = node.GetComponent<ArenaRankingInfoSlot>();
                    if (slotInfo == null) return;
                    slotInfo.Init((ArenaRankingInfoData)item);
                    node.SetActive(true);

                }));
            rankingInfoTableView.ReLoad();
            myArenaRankingInfoSlot.Init(myRankingData);
            myArenaRankingInfoSlot.gameObject.SetActive(true);
        }

        void ClearData()
        {
            //  SBFunc.RemoveAllChildrens(contents);
            
            myRankingData = null;
            ArenaOpponentList.Clear();

        }
    }
}