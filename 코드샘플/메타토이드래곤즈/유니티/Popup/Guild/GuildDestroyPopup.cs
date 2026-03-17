using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork {
    public class GuildDestroyPopup : Popup<PopupData>
    {
        [SerializeField]
        Text infoText;
        public override void InitUI()
        {
            string guildName = GuildManager.Instance.MyGuildInfo.GetGuildName();
            infoText.text = StringData.GetStringFormatByStrKey("guild_desc:64",guildName);
        }

        public void OnClickDestroy()
        {
            var data = new WWWForm();
            data.AddField("gno",GuildManager.Instance.GuildID);
            GuildManager.Instance.NetworkSend("guild/close", data, () =>
            {
                PopupManager.ClosePopup<GuildManagePopup>();
                GuildEvent.RefreshGuildUI(GuildEvent.eGuildEventType.GuildRefresh);
                ClosePopup();
            });
         
        }
    }
}