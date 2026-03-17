using Crosstales.BWF;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork {
    public class GuildNameChangePopup : Popup<PopupData>
    {
        [SerializeField]
        Text curGuildNameText = null;
        [SerializeField]
        InputField newGuildNameField = null;
        [SerializeField]
        Button nameCheckButton = null;
        [SerializeField]
        GameObject nameBeforeCheckObject = null;
        [SerializeField]
        GameObject nameAfterCheckObject = null;
        
        [Space()]
        [SerializeField]
        Button NameChangeBtn = null;
        [SerializeField]
        Text moneyText = null;
        [SerializeField]
        Image moneyIcon = null;

        bool checkGuildNameExistState = false;

        bool isMoneyEnough = false;

        int needMoney { get { return GameConfigTable.GetConfigIntValue("GUILD_NAME_CHANGE_COST_NUM"); } }
        eGoodType needMoneyType
        {
            get
            {
                var value = GameConfigTable.GetConfigValue("GUILD_NAME_CHANGE_COST_TYEP", "GOLD");
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
        public override void InitUI()
        {
            curGuildNameText.text = GuildManager.Instance.MyGuildInfo.GetGuildName();
            newGuildNameField.text = string.Empty;
            CheckGuildNameExistState = false;
            SetNeedMoneyInfo();
            OnChangNameField();
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
            NameChangeBtn.SetButtonSpriteState(isMoneyEnough && CheckGuildNameExistState);
            //moneyText.text = "";
            //moneyIcon.sprite = 
            //isMoneyEnough
        }
        public void OnChangNameField()
        {
            if (newGuildNameField == null || newGuildNameField.text == "")
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
            if (NameChangeBtn != null)
            {
                NameChangeBtn.SetButtonSpriteState(false);
            }

            if (nameCheckButton != null)
            {
                CheckGuildNameExistState = false;
                nameCheckButton.SetInteractable(true);
                nameCheckButton.SetButtonSpriteState(true);
            }
            return;
            //string inputText = newGuildNameField.text.Trim();
            //newGuildNameField.text = GetLimitText(inputText, NameByteLimit);
        }
        public void OnClickChangeNick()
        {
            if (GuildManager.Instance.MyData.GuildPosition == eGuildPosition.Leader)
            {
                var date = GuildManager.Instance.NextNameChangeTimeStamp;
                if (TimeManager.GetTime() >= date)
                {
                    if (isMoneyEnough == false)
                    {
                        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("dragon_info_text_07"), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
                        return;
                    }
                    string name = newGuildNameField.text.Trim();
                    if (string.IsNullOrEmpty(name))
                        return;
                    if (BWFManager.Instance.Contains(name))
                    {
                        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002918), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
                        return;
                    }
                    if (CheckGuildNameExistState == false)
                    {
                        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002836), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
                        return;
                    }

                    if (NameChangeBtn != null)
                    {
                        NameChangeBtn.SetButtonSpriteState(false);
                    }

                    var data = new WWWForm();
                    data.AddField("gno", GuildManager.Instance.GuildID);
                    data.AddField("guild_name", name);
                    GuildManager.Instance.NetworkSend("guild/changeguildname", data, () =>
                    {
                        ClosePopup();
                        if (NameChangeBtn != null)
                        {
                            NameChangeBtn.SetButtonSpriteState(true);
                        }
                        SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("guild_desc:117"),true,false,true);
                    });
                }
                else
                {
                    var remainTime = TimeManager.GetTimeCompare((int)date);
                    SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringFormatByStrKey("guild_desc:101", SBFunc.TimeCustomString(remainTime, true, true, true, false, true)), true, false, true);
                }

            }
            else
            {
                ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
            }
            
        }

        public void OnClickExistNameCheck()
        {
            if (GuildManager.Instance.MyData.GuildPosition == eGuildPosition.Leader)
            {
                string name = newGuildNameField.text.Trim();
                newGuildNameField.text = name;
                if (string.IsNullOrEmpty(name))
                    return;
                var data = new WWWForm();
                data.AddField("gno", GuildManager.Instance.GuildID);
                data.AddField("guild_name", name);
                GuildManager.Instance.NetworkSend("guild/checkname", data, (JObject jsonData) =>
                {
                    switch (jsonData["reason"].Value<int>())
                    {
                        case (int)eApiResCode.OK:
                            if (SBFunc.IsJTokenCheck(jsonData["is_duplicate"]) && jsonData["is_duplicate"].ToObject<bool>() == true)
                            {
                                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002832), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
                            }
                            else
                            {
                                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002835), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
                                CheckGuildNameExistState = true;
                                nameCheckButton.SetInteractable(false);
                                nameCheckButton.SetButtonSpriteState(true);
                                SetNeedMoneyInfo();
                            }                            
                            return;
                        case (int)eApiResCode.NICKNAME_DUPLICATES:
                        case (int)eApiResCode.ACCOUNT_EXISTS:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002832), StringData.GetStringByIndex(100000199), StringData.GetStringByIndex(100000200));
                            return;
                        case (int)eApiResCode.INVALID_NICK_CHAR:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByStrKey("길드이름오류"));
                            return;
                    }
                });
            }
            else
            {
                ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
            }
        }
    }
}