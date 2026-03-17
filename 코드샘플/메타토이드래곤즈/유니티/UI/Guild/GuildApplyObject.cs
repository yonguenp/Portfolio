using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork { 
    public class GuildApplyObject : MonoBehaviour
    {
        [SerializeField]
        UserPortraitFrame userFrame;
        [SerializeField]
        Text userLv;
        [SerializeField]
        Text userNick;

        public string UserNo { get; private set; }

        VoidDelegate agree_cb = null;
        VoidDelegate reject_cb = null;
        public void InitUI(string userNo, string nick, ThumbnailUserData thumnailData,  VoidDelegate agree_callBack, VoidDelegate reject_callBack)
        {
            userFrame.SetUserPortraitFrame(thumnailData);
            userNick.text = nick;
            UserNo = userNo;
            agree_cb = agree_callBack;
            reject_cb = reject_callBack;
        }

        public void InitUI(GuildApplyTableData data, VoidDelegate agree_callBack, VoidDelegate reject_callBack)
        {
            userFrame.SetUserPortraitFrame(data.ThumbnailUserData);
            userLv.text = StringData.GetStringFormatByStrKey("user_info_lv_02", data.UserLv);
            userNick.text = data.UserNick;
            UserNo = data.UserNo;
            agree_cb = agree_callBack;
            reject_cb = reject_callBack;
        }

        public void OnClickAgree()
        {
            var data = new WWWForm();
            data.AddField("tuno", UserNo);
            data.AddField("gno", GuildManager.Instance.GuildID);
            GuildManager.Instance.NetworkSend("guild/joinaccept", data, () =>
            {
                agree_cb?.Invoke();
            });
        }

        public void OnClickDisagree()
        {
            var data = new WWWForm();
            data.AddField("tuno", UserNo);
            data.AddField("gno", GuildManager.Instance.GuildID);
            GuildManager.Instance.NetworkSend("guild/joindeny", data, () =>
            {
                reject_cb?.Invoke();
            });
        }
    }
}