using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 이벤트 도움말 팝업 - 단순 string
/// </summary>
namespace SandboxNetwork
{
    public class DiceEventHelpPopup : HelpDescriptionPopup
    {
        public override void InitUI()
        {
            string targetString;
            if(Data.TabIndex > 0 && Data.SubIndex > 0)
                targetString = StringData.GetStringByStrKey(string.Format("2023_EVENT_MENU{0}_{1}_HELP", Data.TabIndex, Data.SubIndex));
            else
                targetString = Data.Comment;

            contentText.text = targetString;
            
            RefreshContentFitter(contentRect);
        }
    }
}

