using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork {

    public class GuildApplyTableData : ITableData
    {
        public string UserNo { get; private set; }
        public string UserNick { get; private set; }
        public int UserLv { get; private set; }
        public ThumbnailUserData ThumbnailUserData { get; private set; }

        public GuildApplyTableData(string uno, string nick, ThumbnailUserData thumbnail)
        {
            UserNo = uno;
            UserNick = nick;
            ThumbnailUserData = thumbnail;
        }
        public GuildApplyTableData(JToken jsonData)
        {
            
            if (SBFunc.IsJTokenCheck(jsonData["user_no"]))
            {
                UserNo = jsonData["user_no"].ToString();
            }
            if (SBFunc.IsJTokenCheck(jsonData["nick"]))
            {
                UserNick = jsonData["nick"].ToString();
            }
            if (SBFunc.IsJTokenCheck(jsonData["exp"]))
            {
                UserLv = AccountData.GetLevelByExp(jsonData["exp"].Value<int>());
            }
            if (SBFunc.IsJTokenCheck(jsonData["level"]))
            {
                UserLv = jsonData["level"].Value<int>();
            }
            if (SBFunc.IsJTokenCheck(jsonData["icon"]))
            {
                ThumbnailUserData = new ThumbnailUserData(long.Parse(UserNo), UserNick, jsonData["icon"].ToString(), UserLv);
            }
            
                
        }

        public string GetKey()
        {
            return UserNo.ToString();
        }

        public void Init()
        {
            
        }
    }
    public class GuildApplyListPopup : Popup<PopupData>
    {
        [SerializeField]
        TableView tableView;
        
        List<ITableData> tableDatas = new List<ITableData>();
        public override void InitUI()
        {
            tableDatas.Clear();
            DrawTableView();
            RefreshData();
        }

        public void RefreshData()
        {
            NetworkManager.Send("guild/requestusers", null, (JObject jsonData) =>
            {
                tableDatas.Clear();
                if (jsonData.ContainsKey("req_users") && jsonData["req_users"].Type == JTokenType.Array)
                {
                    JArray array = (JArray)jsonData["req_users"];
                    foreach (var dat in array)
                    {
                        tableDatas.Add(new GuildApplyTableData(dat));
                    }
                }
                DrawTableView();

            });
        }

        public void DrawTableView()
        {
            tableView.OnStart();
            tableDatas.RemoveAll(data => GuildManager.Instance.MyGuildInfo.GuildUserDictionary.ContainsKey(long.Parse(((GuildApplyTableData)data).UserNo))); //목록에서 지워진 데이터 제거
            tableView.SetDelegate(new TableViewDelegate(tableDatas, (GameObject itemNode, ITableData item) => {
                if (itemNode == null || item == null)
                {
                    return;
                }
                var frame = itemNode.GetComponent<GuildApplyObject>();
                if (frame == null)
                {
                    return;
                }
                var data = (GuildApplyTableData)item;
                frame.InitUI(data, DrawTableView, () => RemoveRejectUser(long.Parse(data.UserNo)));
            }));
            tableView.ReLoad(true);
        }

        public void RemoveRejectUser(long rejectedUserNo)
        {
            tableDatas.RemoveAll(data => long.Parse(((GuildApplyTableData)data).UserNo) == rejectedUserNo);
            DrawTableView();
        }
    }

}