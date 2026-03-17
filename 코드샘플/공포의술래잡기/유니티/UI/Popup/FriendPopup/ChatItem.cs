using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class ChatItem : FriendItem
{
    public const int CHAT_LINE_CHARACTER_LIMIT = 20;
    [SerializeField]
    Text msgUI;

    [SerializeField]
    Text txtDate;

    public void SetChatItem(string msg)
    {
        msg = Crosstales.BWF.BWFManager.Instance.ReplaceAll(msg);

        var len = msg.Length / CHAT_LINE_CHARACTER_LIMIT;
        var mod = msg.Length % CHAT_LINE_CHARACTER_LIMIT;
        if(len == 0)
            msgUI.text = msg;
        else
        {
            var sb = new StringBuilder();
            bool isMod = mod != 0;
            for(int i = 0 ; i < len; ++i)
            {
                sb.Append(msg.Substring(CHAT_LINE_CHARACTER_LIMIT * i, CHAT_LINE_CHARACTER_LIMIT));
                if( i != len - 1)
                {
                      sb.Append("\n");
                }
                else
                {
                    if(isMod) sb.Append("\n");
                }
            }

            sb.Append(msg.Substring(CHAT_LINE_CHARACTER_LIMIT * len, mod));
            msgUI.text = sb.ToString();
        }
        
    }

    public void SetDate(long ms)        //밀리세컨드 타임
    {
        var time = SBCommonLib.SBUtil.ConvertFromUnixMilliSecTimestamp(ms);
        txtDate.text = string.Format("{0:00}:{1:00}", time.Hour, time.Minute);
    }
}
