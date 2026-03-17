using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork {
    public class GuildEmblemChangePopup : Popup<PopupData>
    {
        [Header("change result Layer")]
        [SerializeField]
        GuildMarkObject beforeMark;
        [SerializeField]
        GuildMarkObject afterMark;
        [Space()]
        [SerializeField]
        TableViewGrid emblemTableView;
        [Space()]
        [SerializeField]
        TableViewGrid markTableView;
        [Header("bot")]
        [SerializeField]
        Button changeBtn;
        [SerializeField]
        Text moneyText;
        [SerializeField]
        Image moneyIcon;

        int curMark = 0;
        int curEmblem = 0;
        bool isMoneyEnough;

        bool isInit = false;
        int needMoney { get { return GameConfigTable.GetConfigIntValue("GUILD_MARK_CHANGE_COST_NUM"); } }
        eGoodType needMoneyType
        {
            get
            {
                var value = GameConfigTable.GetConfigValue("GUILD_MARK_CHANGE_COST_TYPE", "GOLD");
                switch (value)
                {
                    case "GEMSTONE":
                        return eGoodType.GEMSTONE;
                    case "GOLD":
                    default:
                        return eGoodType.GOLD;

                }
            }
        }
        GuildDetailData guildInfo = null;
        public override void InitUI()
        {
            guildInfo = GuildManager.Instance.MyGuildInfo;
            curMark = guildInfo.GetGuildMark();
            curEmblem = guildInfo.GetGuildEmblem();
            beforeMark.SetGuildMark(curEmblem, curMark);
            afterMark.SetGuildMark(curEmblem, curMark);

            PopupTopUIRefreshEvent.Hide();
            if (isInit == false)
            {
                isInit = true;
                emblemTableView.OnStart();
                markTableView.OnStart();
            }
            SetNeedMoneyInfo();
            SetEmblemTableView();
            SetMarkTableView();
        }
        void SetMarkEmblem()
        {
            afterMark.SetGuildMark(curEmblem, curMark);
            RefreshButton();
        }
        void SetNeedMoneyInfo()
        {
            moneyText.text = SBFunc.CommaFromMoney(needMoney);
            isMoneyEnough = false;
            switch (needMoneyType)
            {
                case eGoodType.GOLD:
                    moneyIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gold");
                    isMoneyEnough = User.Instance.GOLD >= needMoney;
                    break;
                case eGoodType.GEMSTONE:
                    moneyIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gemstone");
                    isMoneyEnough = User.Instance.GEMSTONE >= needMoney;
                    break;
                case eGoodType.ENERGY:
                    moneyIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "energy");
                    isMoneyEnough = User.Instance.ENERGY >= needMoney;
                    break;
            }
            moneyText.color = isMoneyEnough ? Color.white : Color.red;

            RefreshButton();
        }

        void RefreshButton()
        {
            bool changed = curMark != guildInfo.GetGuildMark() || curMark != guildInfo.GetGuildMark();            
            changeBtn.SetButtonSpriteState(isMoneyEnough && changed);
        }

        void SetEmblemTableView(bool _initPos = true)
        {
            List<GuildMarkIndexData> emeblemViewData = new List<GuildMarkIndexData>();
            List<ITableData> emblemTableViewItemList = new List<ITableData>();
            emblemTableViewItemList.Clear();
            
            foreach (var emblem in GuildResourceData.GetEmblems())
            {
                emeblemViewData.Add(new GuildMarkIndexData(emblem.KEY));
            }

            if (emeblemViewData != null && emeblemViewData.Count > 0)
            {
                for (var i = 0; i < emeblemViewData.Count; ++i)
                {
                    var data = emeblemViewData[i];
                    if (data == null)
                    {
                        continue;
                    }
                    emblemTableViewItemList.Add(data);
                }
            }

            emblemTableView.SetDelegate(new TableViewDelegate(emblemTableViewItemList, (GameObject itemNode, ITableData item) => {
                if (itemNode == null || item == null)
                {
                    return;
                }
                var frame = itemNode.GetComponent<GuildEmblemSelectObj>();
                
                if (frame == null)
                {
                    return;
                }
                int markIdx = ((GuildMarkIndexData)item).Idx;
                frame.SetIconImg(markIdx, true, curEmblem == markIdx);
                frame.SetClickCallBack((emblemNo) =>
                {
                    curEmblem = markIdx;
                    SetMarkEmblem();
                    SetEmblemTableView(false);
                });

            }));
            emblemTableView.ReLoad(_initPos);
        }

        void SetMarkTableView(bool _initPos = true)
        {
            List<GuildMarkIndexData> viewData = new List<GuildMarkIndexData>();
            List<ITableData> tableViewItemList = new List<ITableData>();
            foreach (var emblem in GuildResourceData.GetMarks())
            {
                viewData.Add(new GuildMarkIndexData(emblem.KEY));
            }
            tableViewItemList.Clear();
            if (viewData != null && viewData.Count > 0)
            {
                for (var i = 0; i < viewData.Count; ++i)
                {
                    var data = viewData[i];
                    if (data == null)
                    {
                        continue;
                    }
                    tableViewItemList.Add(data);
                }
            }
            markTableView.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject itemNode, ITableData item) => {
                if (itemNode == null || item == null)
                {
                    return;
                }
                var frame = itemNode.GetComponent<GuildEmblemSelectObj>();
                //frame.SetIconImg("", false,);
                if (frame == null)
                {
                    return;
                }
                int markIdx = ((GuildMarkIndexData)item).Idx;
                frame.SetIconImg(markIdx, false, markIdx == curMark);
                frame.SetClickCallBack((markIdx) =>
                {
                    curMark = markIdx;
                    SetMarkEmblem();
                    SetMarkTableView(false);
                });
            }));
            markTableView.ReLoad(_initPos);
        }

        public override void ClosePopup()
        {
            base.ClosePopup();
            
        }
        public void OnClickChangeBtn()
        {
            bool changed = curMark != guildInfo.GetGuildMark() || curMark != guildInfo.GetGuildMark();
            if (!changed)
                return;

            if (GuildManager.Instance.MyData.GuildPosition == eGuildPosition.Leader)
            {
                var date = GuildManager.Instance.NextEmblemChangeTimeStamp;
                if (TimeManager.GetTime() >= date)
                {
                    if (isMoneyEnough == false)
                    {
                        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("dragon_info_text_07"), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
                        return;
                    }
                    if (changeBtn != null)
                    {
                        changeBtn.SetButtonSpriteState(false);
                    }
                    var data = new WWWForm();
                    data.AddField("gno", guildInfo.GetGuildID());
                    data.AddField("mark_no", curMark);
                    data.AddField("emblem_no", curEmblem);

                    GuildManager.Instance.NetworkSend("guild/changeemblem", data, (JObject jsonData) =>
                    {
                        switch (jsonData["rs"].Value<int>())
                        {
                            case (int)eApiResCode.OK:
                                GuildManager.Instance.UpdateGuildData(jsonData);
                                ClosePopup();
                                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("guild_desc:124"),true,false,true);
                                return;
                        }
                        if (changeBtn != null)
                        {
                            SetNeedMoneyInfo();
                        }

                    });
                }
                else
                {
                    var remainTime = TimeManager.GetTimeCompare((int)date);
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringFormatByStrKey("guild_desc:100",SBFunc.TimeCustomString(remainTime,true,true,true,false,true)), true, false, true);
                }

            }
            else
            {
                ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
            }


            
        }
    }
}