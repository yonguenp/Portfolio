using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RankNextResetInfoClone : MonoBehaviour
{

    [SerializeField]
    TableViewLayout tableview;
    [SerializeField]
    RankIconClone nextRank;

    bool isInit = false;
    public void Init(int resetRank, List<ArenaRankData> beforeRank)
    {
        if(isInit ==false)
        {
            tableview.OnStart();
            isInit = true;
        }
        List<ITableData> tableViewItemList = new List<ITableData>();
        if (beforeRank != null && beforeRank.Count > 0)
        {
            for (var i = 0; i < beforeRank.Count; i++)
            {
                var data = beforeRank[i];
                if (data == null)
                    continue;

                tableViewItemList.Add(data);
            }
        }
        tableViewItemList.Reverse();
        tableview.SetDelegate(
        new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) => {
            if (node == null) return;
            var slotInfo = node.GetComponent<RankIconClone>();
            if (slotInfo == null) return;
            slotInfo.Init((ArenaRankData)item);
        }));
        tableview.ReLoad();
        nextRank.Init(resetRank);
    }

}
