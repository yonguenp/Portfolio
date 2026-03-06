
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class ArtBlockPopup : Popup<PopupData>
    {

        [SerializeField]
        Text bodyText; 
        public override void InitUI()
        {
            if (User.Instance.UserData.ExtraStatBuff.IsArtBlockAble)
            {
                bodyText.text = StringData.GetStringByStrKey("아트블록활성화");

            }
            else
            {
                bodyText.text = StringData.GetStringByStrKey("아트블록비활성화");
                   
            }
            
        }

        public void OnClickDapp()
        {
            DAppManager.Instance.OpenDAppArtBlock();
        }
    }

}
