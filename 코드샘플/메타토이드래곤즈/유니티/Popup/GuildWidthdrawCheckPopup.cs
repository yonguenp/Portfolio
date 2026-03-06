using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class GuildWidthdrawCheckPopup : Popup<PopupBase>
    {
        [SerializeField] Text Title = null;
        [SerializeField] Text Target = null;
        [SerializeField] Text Value = null;
        [SerializeField] GameObject Magnite = null;
        [SerializeField] GameObject Magnet = null;

        [SerializeField] Toggle[] Toggles;
        [SerializeField] Button OKButton;

        GuildAssetType type;
        GuildUserData data;
        int value;
        public static void Open(GuildAssetType type, GuildUserData data, int value)
        {
            var popup = PopupManager.OpenPopup<GuildWidthdrawCheckPopup>();
            popup.SetData(type, data, value);
        }

        public void SetData(GuildAssetType t, GuildUserData d, int v)
        {
            type = t;
            data = d;
            value = v;
            switch (type)
            {
                case GuildAssetType.MAGNET_DEPOSIT:
                    Target.text = GuildManager.Instance.MyBaseData.GuildName;
                    Title.text = StringData.GetStringByStrKey("마그넷입금");
                    Value.text = SBFunc.CommaFromNumber(value);
                    Magnite.SetActive(false);
                    Magnet.SetActive(true);
                    break;
                case GuildAssetType.MAGNET_WITHDRAW:
                    Target.text = data.Nick;
                    Title.text = StringData.GetStringByStrKey("마그넷출금");
                    Value.text = SBFunc.CommaFromNumber(value);
                    Magnite.SetActive(false);
                    Magnet.SetActive(true);
                    break;
                case GuildAssetType.MAGNITE_DEPOSIT:
                    Target.text = GuildManager.Instance.MyBaseData.GuildName;
                    Title.text = StringData.GetStringByStrKey("마그나이트입금");
                    Value.text = SBFunc.CommaFromNumber(value);
                    Magnite.SetActive(true);
                    Magnet.SetActive(false);
                    break;
                case GuildAssetType.MAGNITE_WITHDRAW:
                    Target.text = data.Nick;
                    Title.text = StringData.GetStringByStrKey("마그나이트출금");
                    Value.text = SBFunc.CommaFromNumber(value);
                    Magnite.SetActive(true);
                    Magnet.SetActive(false);
                    break;
            }

            foreach(var toggle in Toggles)
            {
                toggle.isOn = false;
            }

            OKButton.interactable = false;
        }

        public void OnCheckBox()
        {
            foreach (var toggle in Toggles)
            {
                if(!toggle.isOn)
                {
                    OKButton.interactable = false;
                    return;
                }
            }

            OKButton.interactable = true;
        }

        public override void InitUI()
        {

        }

        public void OK()
        {
            foreach (var toggle in Toggles)
            {
                if (!toggle.isOn)
                {
                    return;
                }
            }

            var param = new WWWForm();
            param.AddField("amount", value);

            switch (type)
            {
                case GuildAssetType.MAGNET_DEPOSIT:
                    
                    GuildManager.Instance.NetworkSend("guild/magnetdeposit", param, (jObject) => {
                        PopupManager.ClosePopup<GuildWidthdrawPopup>();
                        PopupManager.ClosePopup<GuildWidthdrawCheckPopup>();
                    });
                    break;
                case GuildAssetType.MAGNET_WITHDRAW:
                    param.AddField("to_uno", data.UID.ToString());
                    GuildManager.Instance.NetworkSend("guild/magnetwithdraw", param, (jObject) => {
                        PopupManager.ClosePopup<GuildWidthdrawPopup>();
                        PopupManager.ClosePopup<GuildWidthdrawCheckPopup>();
                    });
                    break;
                case GuildAssetType.MAGNITE_DEPOSIT:
                    GuildManager.Instance.NetworkSend("guild/magnitedeposit", param, (jObject) => {
                        PopupManager.ClosePopup<GuildWidthdrawPopup>();
                        PopupManager.ClosePopup<GuildWidthdrawCheckPopup>();
                    });
                    break;
                case GuildAssetType.MAGNITE_WITHDRAW:
                    param.AddField("to_uno", data.UID.ToString());
                    GuildManager.Instance.NetworkSend("guild/magnitewithdraw", param, (jObject) => {
                        PopupManager.ClosePopup<GuildWidthdrawPopup>();
                        PopupManager.ClosePopup<GuildWidthdrawCheckPopup>();
                    });
                    break;
            }
        }
    }
}