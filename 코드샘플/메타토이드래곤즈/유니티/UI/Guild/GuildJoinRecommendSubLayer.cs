using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class GuildJoinRecommendSubLayer : SubLayer
    {

        [SerializeField]
        TableView recommendListView = null;
        [SerializeField]
        InputField searchInputField = null;
        [SerializeField]
        DropDownUIController dropdownController = null;
        [SerializeField]
        Button refreshBtn = null;

        eGuildRecommendFilter guildRecommendFilter = eGuildRecommendFilter.ImmediateJoin | eGuildRecommendFilter.ApplyJoin;
        string inputText = string.Empty;
        int curSortingIndex = 0;
        bool isInit = false;
        public override void Init()
        {
            base.Init();
            if (isInit == false)
            {
                recommendListView.OnStart();
                isInit = true;
            }
            inputText = searchInputField.text = string.Empty;
            refreshBtn.interactable = true;
            InitCustomSort();
        }
        public void OnInputSearchEnter()
        {
            if(searchInputField.text ==  string.Empty)
                return;
            if (inputText != searchInputField.text)
            {
                inputText = searchInputField.text;
                WWWForm form = new WWWForm();
                
                form.AddField("guild_name", inputText);
                GuildManager.Instance.NetworkSend("guild/search", form, (JObject jsonData) =>
                {
                    // to do.. 검색 결과 데이터 세팅
                    if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                    {
                        switch (jsonData["rs"].ToObject<int>())
                        {
                            case (int)eApiResCode.OK:
                            {
                                if (SBFunc.IsJTokenCheck(jsonData["search_guild_info"]))
                                {
                                    var data = new GuildInfoData((JObject)jsonData["search_guild_info"]);
                                    SetTableView(new List<ITableData> { data });
                                }
                                else
                                {
                                    ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_24"));
                                }
                            }
                            break;
                            case (int)eApiResCode.GUILD_DATA_ERROR:
                            {
                                ToastManager.On(StringData.GetStringByStrKey("guild_errorcode_24"));
                                SetTableView(new List<ITableData>());
                            }
                            break;
                        }
                        
                    }
                    //var data = GuildManager.Instance.RecommandGuildList;
                    //List<ITableData> tableDatas = new List<ITableData>();
                    //foreach (var item in data.Values)
                    //{
                    //    tableDatas.Add(item);
                    //}
                    //SetTableView(tableDatas);
                });
            }
        }



        public void InitCustomSort()
        {
            dropdownController.RefreshAllFilterLabel();
            dropdownController.InitDropDown();
            OnClickCustomSort(dropdownController.GetDropdownIndex(eDropDownType.DEFAULT));
        }

        public void OnClickCustomSort(int index)
        {
            dropdownController.SetDropdownIndex(eDropDownType.DEFAULT, index);
            dropdownController.RefreshAllFilterLabel();
            dropdownController.InitDropDown();
            curSortingIndex = index;
            SetRecommendData();
        }

        public void OnClickRefresh()
        {
            inputText = searchInputField.text = string.Empty;
            GetRecommendData();
        }

        public void OnClickFilter()
        {
            var popup = PopupManager.OpenPopup<GuildRecommendFilterPopup>(new GuildReccomendFilterData(guildRecommendFilter));
            popup.ApplyCallback = SetFilter;
        }

        void SetFilter(GuildReccomendFilterData data)
        {
            if (data == null)
            {
                Debug.Log("필터 데이터 생성 누락");
                return;
            }
            guildRecommendFilter = data.filter;
            SetRecommendData();
        }

        void SetRecommendData()
        {
            var data = GuildManager.Instance.RecommandGuildList;
            List<ITableData> tableDatas = new List<ITableData>();
            foreach (var item in data.Values)
            {
                tableDatas.Add(item);
            }
            SetTableView(tableDatas);
        }

        void SetTableView(List<ITableData> tableDatas)
        {
            if (curSortingIndex == 0)
            {
                tableDatas= tableDatas.OrderByDescending(a => {
                    int rank = ((GuildInfoData)a).GuildRank;
                    if (rank == 0)
                        return int.MinValue;
                    return -rank;
                    }).ToList();

            }
            else
            {
                tableDatas= tableDatas.OrderByDescending(a => {
                    int rank = ((GuildInfoData)a).GuildRank;
                    if (rank == 0)
                        return int.MaxValue;
                    return rank;
                }).ToList();
            }
            tableDatas.RemoveAll(x => {
                var type = ((GuildInfoData)x).GuildJoinType;
                if (guildRecommendFilter.HasFlag(eGuildRecommendFilter.ImmediateJoin) == false && type == eGuildJoinType.JoinRightNow)
                {
                    return true;
                }
                if (guildRecommendFilter.HasFlag(eGuildRecommendFilter.ApplyJoin) == false && type == eGuildJoinType.JoinWait)
                {
                    return true;
                }
                return false;
            });
            //var applyList = GuildManager.Instance.ApplyGuildList;
            //tableDatas.RemoveAll(x => applyList.ContainsKey(((GuildInfoData)x).GuildID));

            recommendListView.SetDelegate(new TableViewDelegate(tableDatas, (GameObject node, ITableData item) => {
                if (node == null) return;
                var marketingObj = node.GetComponent<GuildMarketingObj>();
                if (marketingObj == null) return;
                GuildInfoData data = (GuildInfoData)item;
                marketingObj.Init(data);
                marketingObj.SetCallBack(SetRecommendData);
            }));
            recommendListView.ReLoad();
        }

        void GetRecommendData()
        {
            WWWForm form = new WWWForm();
            GuildManager.Instance.NetworkSend("guild/refreshrecommnd", form, () =>
            {
                SetRecommendData();
                refreshBtn.interactable = false;
                CancelInvoke(nameof(SetRefreshBtnOn));
                Invoke(nameof(SetRefreshBtnOn), 3f);
            });
        }
        void SetRefreshBtnOn()
        {
            refreshBtn.interactable = true;
        }
    }
    
}