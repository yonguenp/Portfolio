using SandboxNetwork;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

public class ContentLock : MonoBehaviour
{
    [SerializeField] Button[] LockButtons = null;
    [SerializeField] int TownLevel = 0;
    [SerializeField] GameObject LockIcon = null;

    Dictionary<Button, ButtonClickedEvent> OriginEvent = new Dictionary<Button, ButtonClickedEvent>();

    bool EnableContent 
    { 
        get {
            if (TownLevel <= 0) return true;
            
            return User.Instance.ExteriorData.ExteriorLevel >= TownLevel;
        } 
    }
    void OnEnable()
    {
        //RefreshState();    
    }

    public void RefreshState()
    {
        if (LockIcon != null)
        {
            LockIcon.SetActive(!EnableContent);
        }

        foreach (var btn in LockButtons)
        {
            if (!OriginEvent.ContainsKey(btn))
                OriginEvent.Add(btn, btn.onClick);

            if (EnableContent)
            {
                btn.onClick = OriginEvent[btn];
            }
            else
            {
                btn.onClick = new ButtonClickedEvent();
                btn.onClick.AddListener(() => {
                    ToastManager.On(StringData.GetStringFormatByStrKey("컨텐츠잠금", TownLevel));
                });                
            }
        }
    }
}
