using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class GuildJoinApplyListSubLayer : SubLayer
    {
        [SerializeField]
        TableView applyTableView = null;
        [SerializeField]
        GameObject noneApplyAlarmObj = null;

        public override void Init()
        {
            base.Init();
            DrawTableView();
        }

        void DrawTableView()
        {
            applyTableView.OnStart();
            var data = GuildManager.Instance.ReqGuildList;
            List<ITableData> tableDatas = new List<ITableData>();
            if(data != null )
            {
                foreach (var item in data.Values)
                {
                    tableDatas.Add(item);
                }
            }
            noneApplyAlarmObj.SetActive(tableDatas.Count == 0);
            applyTableView.SetDelegate(new TableViewDelegate(tableDatas, (GameObject node, ITableData item) => {
                if (node == null) return;
                var marketingObj = node.GetComponent<GuildMarketingObj>();
                if (marketingObj == null) return;
                GuildInfoData data = (GuildInfoData)item;
                marketingObj.Init(data);
                marketingObj.SetAppliedMode();
                marketingObj.SetCallBack(DrawTableView);                
            }));
            applyTableView.ReLoad();
        }
    }
}
