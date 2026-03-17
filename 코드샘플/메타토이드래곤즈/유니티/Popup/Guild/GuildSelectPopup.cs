using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class GuildSelectPopup : Popup<PopupData>
    {
        Action make_cb = null;

        public static void Show(Action action)
        {
            var popup = PopupManager.OpenPopup<GuildSelectPopup>();
            popup.make_cb = action;
        }

        public override void InitUI()
        {
            
        }

        public void OnMake()
        {
            ClosePopup();
            make_cb?.Invoke();
        }

        public void OnJoin()
        {
            ClosePopup();
            PopupManager.OpenPopup<GuildJoinPopup>(); 
        }
    }
}
