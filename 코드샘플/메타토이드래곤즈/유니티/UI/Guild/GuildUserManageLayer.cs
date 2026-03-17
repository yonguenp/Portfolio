using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class GuildUserManageLayer : TabLayer
    {
        public enum GUILDUSERMENU
        {
            SUM_TOTAL,
            SUM_ARENA,
            SUM_RAID,
            SUM_EXP,
            SUM_MAGNET,

            WEEK_TOTAL,
            WEEK_ARENA,
            WEEK_RAID,
            WEEK_EXP,
            WEEK_MAGNET,
            
            MONTH_TOTAL,
            MONTH_ARENA,
            MONTH_RAID,
            MONTH_EXP,
            MONTH_MAGNET,

            //default
            SUM = SUM_TOTAL,
            WEEK = WEEK_TOTAL,
            MONTH = MONTH_TOTAL
        }
        [Serializable]
        class TabMenu
        {
            [SerializeField] GameObject mainBtn;
            [SerializeField] GameObject subPanel;
            [SerializeField] Button[] subBtns;
            [SerializeField] GUILDUSERMENU menuType;
            
            public void Refresh(GUILDUSERMENU type)
            {
                if (type >= menuType && type < (menuType + 5))
                {
                    mainBtn.SetActive(false);
                    subPanel.SetActive(true);

                    for(int i = 0; i < subBtns.Length; i++)
                    {
                        subBtns[i].interactable = (type - (menuType + i)) != 0;
                    }
                }
                else
                {
                    mainBtn.SetActive(true);
                    subPanel.SetActive(false);
                }
            }
        }

        [SerializeField]
        TabMenu[] tabs;

        [SerializeField]
        TableView userTableView;

        [SerializeField]
        GameObject authorityBackLayer;

        [SerializeField]
        Button cancelBtn;

        [SerializeField]
        DropDownUIController dropdownController;

        [SerializeField]
        GameObject manageBtnObj;
        [SerializeField] Text pointText;
        bool isInit = false;
        bool isManageMode = false;
        List<GuildUserData> rankList = new List<GuildUserData>();

        GUILDUSERMENU curMenu = GUILDUSERMENU.SUM;
        public override void InitUI(TabTypePopupData datas = null)
        {
            base.InitUI(datas);
            SetData();
        }

        public override void RefreshUI()
        {
            base.RefreshUI();
            SetData();            
        }

        void SetData()
        {
            if (isInit == false)
            {
                userTableView.OnStart();
                isInit = true;
            }
            isManageMode = false;
            //dropdownController.RefreshAllFilterLabel();
            //dropdownController.InitDropDown();
            authorityBackLayer.SetActive(false);
            rankList.Clear();
            manageBtnObj.SetActive(GuildManager.Instance.MyData.GuildPosition == eGuildPosition.Operator || GuildManager.Instance.MyData.GuildPosition == eGuildPosition.Leader);
            WWWForm form = new WWWForm();
            form.AddField("gno", GuildManager.Instance.GuildID);
            form.AddField("page", (int)eGuildRankingPage.Member);
            GuildManager.Instance.NetworkSend("guild/ranking", form, (JObject jsonData) =>
            {
                if (SBFunc.IsJTokenCheck(jsonData["rs"]) && jsonData["rs"].ToObject<int>() == (int)eApiResCode.OK)
                {
                    if (SBFunc.IsJArray(jsonData["list"]))
                    {
                        GuildManager.Instance.UpdateGuildUserData(jsonData["list"].ToObject<JArray>());
                    }
                }
                DrawScrollView();
            });

            SetMenuUI();
        }

        public void OnClickManageBtn()
        {
            authorityBackLayer.SetActive(false);
            if (GuildManager.Instance.IsManageUserAble) // 권한체크
            {
                isManageMode = !isManageMode;
                //dropdownController.InitDropDown();
                DrawScrollView();
            }
            else
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("guild_desc:68"), true, false, false);
            }
        }
        
        void DrawScrollView()
        {
            var userList = GuildManager.Instance.MyGuildInfo.GuildUserList;
            eGuildRankType rankType = eGuildRankType.SumRanking;
            //var sort = dropdownController.GetDropdownIndex(eDropDownType.DEFAULT);
            switch (curMenu)
            {
                case GUILDUSERMENU.SUM_TOTAL:
                case GUILDUSERMENU.SUM_ARENA:
                case GUILDUSERMENU.SUM_RAID:
                case GUILDUSERMENU.SUM_EXP: 
                case GUILDUSERMENU.SUM_MAGNET :                     
                    rankType = eGuildRankType.SumRanking;
                    break;
                case GUILDUSERMENU.WEEK_TOTAL:
                case GUILDUSERMENU.WEEK_ARENA:
                case GUILDUSERMENU.WEEK_RAID:
                case GUILDUSERMENU.WEEK_EXP:
                case GUILDUSERMENU.WEEK_MAGNET:
                    rankType = eGuildRankType.WeeklyRanking;
                    break;
                case GUILDUSERMENU.MONTH_TOTAL:
                case GUILDUSERMENU.MONTH_ARENA:
                case GUILDUSERMENU.MONTH_RAID:
                case GUILDUSERMENU.MONTH_EXP:
                case GUILDUSERMENU.MONTH_MAGNET:
                    rankType = eGuildRankType.MonthlyRanking;
                    break;
            }

            if(pointText != null)
            {
                switch(rankType)
                {
                    case eGuildRankType.MonthlyRanking:
                        pointText.text = StringData.GetStringByStrKey("월간포인트");
                        break;
                    case eGuildRankType.WeeklyRanking:
                        pointText.text = StringData.GetStringByStrKey("주간포인트");
                        break;
                    default:
                        pointText.text = StringData.GetStringByStrKey("누적포인트");
                        break;

                }
                
            }

            switch (curMenu)
            {
                case GUILDUSERMENU.SUM_TOTAL:
                case GUILDUSERMENU.WEEK_TOTAL:
                case GUILDUSERMENU.MONTH_TOTAL:
                    userList = userList.OrderByDescending(user => {
                        if (user.TotalPoint[(int)rankType] > 0)
                            return user.TotalPoint[(int)rankType];
                        return int.MinValue;
                    }).ThenBy(user => user.Rank).ToList();
                    break;
                case GUILDUSERMENU.SUM_ARENA:
                case GUILDUSERMENU.WEEK_ARENA:
                case GUILDUSERMENU.MONTH_ARENA:                    
                    userList = userList.OrderByDescending(user => {
                        if (user.ArenaPoint[(int)rankType] > 0)
                            return user.ArenaPoint[(int)rankType];
                        return int.MinValue;
                    }).ThenBy(user => user.Rank).ToList();
                    break;
                case GUILDUSERMENU.SUM_RAID:
                case GUILDUSERMENU.WEEK_RAID:
                case GUILDUSERMENU.MONTH_RAID:
                    userList = userList.OrderByDescending(user => {
                        if (user.RaidPoint[(int)rankType] > 0)
                            return user.RaidPoint[(int)rankType];
                        return int.MinValue;
                    }).ThenBy(user => user.Rank).ToList();
                    break;
                case GUILDUSERMENU.WEEK_EXP:
                case GUILDUSERMENU.SUM_EXP:
                case GUILDUSERMENU.MONTH_EXP:
                    userList = userList.OrderByDescending(user => {
                        if (user.ExpPoint[(int)rankType] > 0)
                            return user.ExpPoint[(int)rankType];
                        return int.MinValue;
                    }).ThenBy(user => user.Rank).ToList();
                    break;
                case GUILDUSERMENU.SUM_MAGNET:
                case GUILDUSERMENU.WEEK_MAGNET:
                case GUILDUSERMENU.MONTH_MAGNET:
                    userList = userList.OrderByDescending(user => {
                        if (user.MagnetPoint[(int)rankType] > 0)
                            return user.MagnetPoint[(int)rankType];
                        return int.MinValue;
                    }).ThenBy(user => user.Rank).ToList();
                    break;
            }



            if (userTableView == null || userList.Count <= 0) return;
            List<ITableData> list = new List<ITableData>();
            for (var i = 0; i < userList.Count; i++)
            {
                if (userList[i] == null)
                {
                    continue;
                }
                list.Add(userList[i]);
            }
            userTableView.SetDelegate(
                new TableViewDelegate(list, (GameObject node, ITableData item) => {
                    if (node == null) return;
                    var manageObj = node.GetComponent<GuildUserManageObject>();
                    if (manageObj == null) return;
                    manageObj.Init(curMenu, (GuildUserData)item, rankType, DrawScrollView);
                    manageObj.SetManageMode(isManageMode ? eGuildManageMode.Manage : eGuildManageMode.Default);
                    node.SetActive(true);
                }));
            userTableView.ReLoad();

            SetMenuUI();
        }

        public void OnClickAuthorityOff()
        {
            cancelBtn.onClick.Invoke();
        }

        public void OnClickRankInfoBtn()
        {
            PopupManager.OpenPopup<GuildRankingRewardPopup>(new GuildRankRewardPopupData(eGuildRankRewardGroup.UserRank));
        }

        public void OnClickCustomSort(string customEventData)
        {
            var checker = int.Parse(customEventData);
            if (dropdownController.GetDropdownIndex(eDropDownType.DEFAULT) == checker)
            {
                dropdownController.SetDropDownVisible(eDropDownType.DEFAULT, false);
                return;
            }
            dropdownController.SetDropdownIndex(eDropDownType.DEFAULT, checker);
            dropdownController.RefreshAllFilterLabel();
            dropdownController.InitDropDown();
            DrawScrollView();
        }

        public void OnMakeManageModeOff()
        {
            isManageMode = false;
            DrawScrollView();
        }

        public void OnSelectMenu(int menu)
        {
            OnSelectMenu((GUILDUSERMENU)menu);
        }
        public void OnSelectMenu(GUILDUSERMENU menu)
        {
            curMenu = menu;
            DrawScrollView();
        }

        public void SetMenuUI()
        {
            foreach(var tab in tabs)
            {
                tab.Refresh(curMenu);
            }
        }
    }

}

