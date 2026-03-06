using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class GuildLvRewardInfoPopup : Popup<PopupData>
    {
        [SerializeField]
        TableView RewardTableView = null;
        [SerializeField]
        GuildLvRewardClone CurrReward = null;
        bool isInit = false;

        public override void InitUI()
        {
            if (isInit == false)
            {
                RewardTableView.OnStart();
                isInit = true;
            }
            DrawScrollView();
        }

        void DrawScrollView()
        {
            var datas = GuildExpData.GetAll();
            if (datas == null || datas.Count == 0)
            {
                Debug.Log("no datas");
                return;
            }
                
            datas.Sort((a, b) => a.LEVEL - b.LEVEL);
            List<ITableData> tableViewItemList = new List<ITableData>();
            tableViewItemList.Clear();
            int dataCnt = datas.Count;
            if (datas != null && dataCnt > 0)
            {
                for (var i = 0; i < dataCnt; ++i)
                {
                    if (datas[i] == null)
                        continue;
                    tableViewItemList.Add(datas[i]);
                }
            }
            int guildLv = GuildManager.Instance.MyGuildInfo.GetGuildLevel();

            if(CurrReward != null)
            {
                List<string> buff = new List<string>();
                foreach (var data in GuildExpData.GetStatsByLv(guildLv))
                {
                    buff.Add(string.Format("{0} +{1}", data.Key, SBFunc.CommaFromNumber((int)data.Value)));
                }

                CurrReward.Init(guildLv, string.Join("\n", buff), guildLv);
            }

            RewardTableView.SetDelegate(
            new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) => {
                if (node == null) 
                    return;
                var slotInfo = node.GetComponent<GuildLvRewardClone>();
                if (slotInfo == null) 
                    return;
                var data = (GuildExpData)item;
                string str = string.Empty;
                if (data.VALUE_TYPE == 1) // 퍼센트 표기
                    str = string.Format("{0} +{1}%", StatTypeData.GetDescStringByStatType(data.REWARD_STAT_TYPE, true), data.REWARD_STAT_VALUE);
                else // 일반 증가 표기
                    str = string.Format("{0} +{1}", StatTypeData.GetDescStringByStatType(data.REWARD_STAT_TYPE, false), data.REWARD_STAT_VALUE);
                
                slotInfo.Init(data.LEVEL,str, guildLv);
                node.SetActive(true);
            }));
            
            RewardTableView.ReLoad(true);
        }
    }

}
