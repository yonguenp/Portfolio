using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class GuildMakePopup : Popup<PopupData>
    {
        [SerializeField]
        TableViewGrid flagTableView = null;
        [SerializeField]
        TableViewGrid markTableView = null;
        [Space()]

        [SerializeField]
        InputField guildNameInputField = null;
        [SerializeField]
        Button nameCheckButton = null;
        [SerializeField]
        GameObject nameBeforeCheckObject = null;
        [SerializeField]
        GameObject nameAfterCheckObject = null;
        [SerializeField]
        GuildMarkObject curMarkObj = null;
        [Space()]
        [SerializeField]
        InputField guildInfoInputField = null;
        [SerializeField]
        Text defaultInfoText;
        [Space()]
        [SerializeField]
        Button guildMakeBtn = null;
        [SerializeField]
        Text moneyText = null;
        [SerializeField]
        Image moneyIcon = null;
        bool checkGuildNameExistState = false;
        bool isInitFirst = false;

        const int NameByteLimit = 10;
        const int InfoByteLimit = 100;
        public bool CheckGuildNameExistState
        {
            get
            {
                return checkGuildNameExistState;
            }
            set
            {
                checkGuildNameExistState = value;

                nameBeforeCheckObject.SetActive(!checkGuildNameExistState);
                nameAfterCheckObject.SetActive(checkGuildNameExistState);
            }
        }

        bool isMoneyEnough = false;
        int needMoney { get { return GameConfigTable.GetConfigIntValue("GUILD_ESTABLISH_COST_NUM"); } }
        eGoodType needMoneyType
        {
            get
            {
                var value = GameConfigTable.GetConfigValue("GUILD_ESTABLISH_COST_TYPE", "GOLD");
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
        int curEmblem = 1001;
        int curMark = 2001;
        public override void InitUI()
        {
            guildMakeBtn.SetButtonSpriteState(false);
            SetTableView();
            SetMarkEmblem();
            SetInfoFieldDefault();
            SetNeedMoneyInfo();
        }
        void SetMarkEmblem()
        {
            curMarkObj.SetGuildMark(curEmblem, curMark);
        }
        void SetTableView()
        {
            if (flagTableView != null && markTableView != null && !isInitFirst)
            {
                flagTableView.OnStart();
                markTableView.OnStart();
                isInitFirst = true;
            }
            DrawEmblemScrollView();
            DrawMarkScrollView();
        }
        void SetInfoFieldDefault()
        {
            defaultInfoText.text = StringData.GetStringByStrKey(GameConfigTable.GetConfigValue("GUILD_INTRODUCE_DEFAULT_STRING"));
            guildNameInputField.text = "";
            guildNameInputField.characterLimit = 10;
            guildInfoInputField.text = "";
            guildInfoInputField.characterLimit = 100;
            OnChangNameField();
            OnChangeInfoField();
        }

        void SetNeedMoneyInfo()
        {
            moneyText.text = SBFunc.CommaFromNumber(needMoney);
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
        }


        public void DrawEmblemScrollView(bool _initPos = true)
        {
            List<GuildMarkIndexData> viewData = new List<GuildMarkIndexData>();
            List<ITableData> tableViewItemList = new List<ITableData>();
            foreach (var emblem in GuildResourceData.GetEmblems())
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

            flagTableView.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject itemNode, ITableData item) => {
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
                frame.SetClickCallBack((markIdx) =>
                {
                    curEmblem = markIdx;
                    SetMarkEmblem();
                    DrawEmblemScrollView(false);
                });
            }));

            flagTableView.ReLoad(_initPos);
        }

        public void DrawMarkScrollView(bool _initPos = true)
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
                    DrawMarkScrollView(false);
                });
            }));

            markTableView.ReLoad(_initPos);
        }

        public void OnClickExistNameCheck()
        {
            string name = guildNameInputField.text.Trim();            

            guildNameInputField.text = name;
            if (string.IsNullOrEmpty(name))
                return;
            //if (BWFManager.Instance.Contains(name))
            //{
            //    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002918), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
            //    return;
            //}
            var data = new WWWForm();
            data.AddField("guild_name", name);
            GuildManager.Instance.NetworkSend("guild/checkname", data, (JObject jsonData) => {
                switch (jsonData["reason"].Value<int>())
                {
                    case (int)eApiResCode.OK:
                        if (SBFunc.IsJTokenCheck(jsonData["is_duplicate"]) && jsonData["is_duplicate"].ToObject<bool>() == true)
                        {
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002832));
                        }
                        else
                        {
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002835), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
                            CheckGuildNameExistState = true;
                            nameCheckButton.SetInteractable(false);
                            nameCheckButton.SetButtonSpriteState(true);
                            guildMakeBtn.SetButtonSpriteState(isMoneyEnough);
                        };
                        return;
                    case (int)eApiResCode.NICKNAME_DUPLICATES:
                    case (int)eApiResCode.ACCOUNT_EXISTS:
                        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002832));
                        return;
                    case (int)eApiResCode.INVALID_NICK_CHAR:
                        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("길드이름오류"));
                        return;
                }
            });
        }
        public void OnChangNameField()
        {
            if (guildNameInputField == null || guildNameInputField.text == "")
            {
                if (nameCheckButton != null)
                {
                    nameCheckButton.SetButtonSpriteState(false);
                    CheckGuildNameExistState = false;
                    nameCheckButton.SetInteractable(false);
                    nameCheckButton.SetButtonSpriteState(false);
                }

                return;
            }
            if (guildMakeBtn != null)
            {
                guildMakeBtn.SetButtonSpriteState(false);
            }

            if (nameCheckButton != null)
            {
                CheckGuildNameExistState = false;
                nameCheckButton.SetInteractable(true);
                nameCheckButton.SetButtonSpriteState(true);
            }
            return;
            string inputText = guildNameInputField.text.Trim();
            guildNameInputField.text = GetLimitText(inputText, NameByteLimit);
        }

        public void OnChangeInfoField()
        {
            if (guildInfoInputField == null || guildInfoInputField.text == "")
            {
                return;
            }
            return;
            string inputText = guildInfoInputField.text.Trim();
            guildInfoInputField.text = GetLimitText(inputText, InfoByteLimit);
        }

        string GetLimitText(string text, int limitByte)
        {
            string ret = "";

            return ret;
        }

        public void OnClickMakeGuild()
        {
            if (guildNameInputField == null)
                return;
            if (isMoneyEnough == false)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("dragon_info_text_07"), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
                return;
            }
            string name = guildNameInputField.text.Trim();
            string info = guildInfoInputField.text.Trim();
            if (string.IsNullOrEmpty(name))
                return;
            if (string.IsNullOrEmpty(info))
                info = StringData.GetStringByStrKey(GameConfigTable.GetConfigValue("GUILD_INTRODUCE_DEFAULT_STRING"));
            if (CheckGuildNameExistState == false)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002836), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
                return;
            }

            //if (Crosstales.BWF.BWFManager.Instance.Contains(name) || Crosstales.BWF.BWFManager.Instance.Contains(info))
            //{
            //    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByIndex(100002664));
            //    return;
            //}

            if (curEmblem == 0 || curMark == 0)
            {
                return;
                ToastManager.On("임시 - 마크와 엠블럼을 선택해 주세요");
            }


            //if (guildMakeBtn != null)
            //{
            //    guildMakeBtn.interactable = false;            
            //    guildMakeBtn.SetButtonSpriteState(false);
            //}

            var data = new WWWForm();
            data.AddField("guild_name", name);
            data.AddField("mark_no", curMark);
            data.AddField("emblem_no", curEmblem);
            data.AddField("guild_desc", info);
            GuildManager.Instance.NetworkSend("guild/open", data, () =>
            {
                if (guildMakeBtn != null)
                {
                    guildMakeBtn.interactable = true;
                    guildMakeBtn.SetButtonSpriteState(true);
                }
                ClosePopup();
                PopupManager.OpenPopup<GuildInfoPopup>();
            });
        }
    }
}

