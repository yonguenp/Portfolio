using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork {
    public class GuildMarketingChangePopup : Popup<PopupData>
    {
        [SerializeField]
        GameObject[] checkObjs = null;

        int curIndex = -1;
        public override void InitUI()
        {
            curIndex = ((int) GuildManager.Instance.MyGuildInfo.GetGuildJoinType());
            foreach (var obj in checkObjs)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
            if (curIndex > 0)
            {
                checkObjs[curIndex - 1].SetActive(true);
            }
            else
            {
                checkObjs[0].SetActive(true);
            }
        }

        public void OnClickBtn(int index)
        {
            if(curIndex != index)
            {
                curIndex = index;
                foreach(var obj in checkObjs)
                {
                    if(obj != null)
                        obj.SetActive(false);
                }
                if(curIndex > 0)
                {
                    checkObjs[curIndex - 1].SetActive(true);
                }
                else
                {
                    checkObjs[0].SetActive(true);
                }
                
            }
        }

        public void OnClickChange()
        {
            if (GuildManager.Instance.IsChangeJoinTypeAble)
            {
                var data = new WWWForm();
                data.AddField("gno", GuildManager.Instance.GuildID);
                data.AddField("join_type", curIndex);
                GuildManager.Instance.NetworkSend("guild/changejointype", data, () =>
                {
                    ClosePopup();
                });
            }
            else
            {
                // to do . 불가 알림
                ToastManager.On(StringData.GetStringByStrKey("guild_desc:104"));
            }
            
        }
    }
}