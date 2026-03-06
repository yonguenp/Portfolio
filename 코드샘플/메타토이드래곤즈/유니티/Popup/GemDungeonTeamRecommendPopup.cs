using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork
{
    public class GemDungeonTeamRecommendPopup : Popup<PopupData>
    {
        VoidDelegate CallBack;
        public override void InitUI() { }

        public void SetCallBack(VoidDelegate cb)
        {
            if(cb != null)
            {
                CallBack = cb;
            }
        }

        public void OnClickOk()
        {
            CallBack?.Invoke();
        }
    }
}


