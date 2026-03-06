using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SandboxNetwork { 
    public class ArenaSeasonRewardController : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField]
        private TableView rewardInfoTableView = null;
        [SerializeField]
        private ArenaSeasonRewardInfoSlot myRewardInfo = null;

        private bool isFirstInit = false;
        private int seasonID = 0;
        public void Init()
        {
            var targetSeasonID = ArenaManager.Instance.UserArenaData.GetRewardSeasonID();
            if (isFirstInit == false || seasonID != targetSeasonID)
            {
                seasonID = targetSeasonID;
                SetSeasonRewardInfoTabData();
                isFirstInit = true;
            }
            int seasonGrade = (int)ArenaManager.Instance.UserArenaData.SeasonGrade;
            myRewardInfo.Init(ArenaRankData.GetFirstInGroup(seasonGrade));
        }

        void SetSeasonRewardInfoTabData()
        {
            if (rewardInfoTableView == null) return;
            
            List<ITableData> seasonRewardList = new List<ITableData>(ArenaRankData.GetAll());
            seasonRewardList.Reverse();

            rewardInfoTableView.OnStart();
            rewardInfoTableView.SetDelegate(
                 new TableViewDelegate(seasonRewardList, (GameObject node, ITableData item) => {
                    if (node == null) return;
                    var slotInfo = node.GetComponent<ArenaSeasonRewardInfoSlot>();
                    if (slotInfo == null) return;
                    slotInfo.Init((ArenaRankData)item);
                    node.SetActive(true);
            }));
            rewardInfoTableView.ReLoad();
        }
    }
}