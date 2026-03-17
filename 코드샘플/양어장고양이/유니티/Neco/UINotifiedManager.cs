using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UINotifiedManager : MonoBehaviour
{
    static private UINotifiedManager instance = null;

    private List<NotificationUI> notificationUI = new List<NotificationUI>();
    private Dictionary<string, DateTime> notificationInfo = new Dictionary<string, DateTime>();
    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;        
    }

    static public UINotifiedManager GetInstance()
    {
        return instance;
    }


    static public void SetNotificationUI(NotificationUI ui)
    {
        GetInstance().notificationUI.Add(ui);
    }

    static public void RemoveNotificationUI(NotificationUI ui)
    {
        GetInstance().notificationUI.Remove(ui);
    }

    static public void RegistNotificationDate(string key)
    {
        GetInstance().notificationInfo[key] = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(NecoCanvas.GetCurTime());
        
        foreach (NotificationUI ui in GetInstance().notificationUI)
        {
            if (ui.MyNotificationKey == key)
            {
                CheckState(ui);
            }
        }
    }

    static public bool CheckState(NotificationUI target)
    {
        foreach (NotificationUI ui in GetInstance().notificationUI)
        {
            if(ui.ParentNotificationKey == target.MyNotificationKey)
            {
                if (CheckState(ui))
                    return true;
            }
        }

        if(GetInstance().notificationInfo.ContainsKey(target.MyNotificationKey))
        {
            return target.GetPivotTime() < GetInstance().notificationInfo[target.MyNotificationKey];
        }

        return false;
    }

    static public void RefershState(NotificationUI target)
    {
        foreach (NotificationUI ui in GetInstance().notificationUI)
        {
            if (ui.MyNotificationKey == target.ParentNotificationKey)
            {
                RefershState(ui);                
            }
        }

        target.CheckNotification();
    }

    static public void UpdateData(string type)
    {
        string[] param = type.Split('_');

        switch (param[0])
        {
            case "gold":
                CheckCookLevelup();
                CheckCraftLevelup();
                
                CheckGiftLevelup();
                CheckTrapLevelup();
                CheckFarmLevelup();
                break;

            case "item":
                CheckCookLevelup();
                CheckCraftLevelup();

                CheckGiftLevelup();
                CheckTrapLevelup();
                CheckFarmLevelup();

                CheckMapObject();
                break;

            case "upgradegift":
                CheckUpgradeGift();
                break;

            case "upgradefarm":
                CheckUpgradeFarm();
                break;

            case "upgradetrap":
                CheckUpgradeTrap();
                break;

            case "upgradecraft":
                CheckUpgradeCraft();
                break;

            case "upgradecook":
                CheckUpgradeCook();
                break;

            case "cat":
                CheckNewCat(param[1]);
                break;

            case "giftfull":
                CheckGiftFull();
                break;

            case "trapfull":
                CheckTrapFull();
                break;

            case "farmfull":
                CheckFarmFull(param[1]);
                break;
        }
    }

    private static void CheckTrapFull()
    {
        //throw new NotImplementedException();
    }

    private static void CheckFarmFull(string v)
    {
        //throw new NotImplementedException();
    }

    private static void CheckGiftFull()
    {
        //throw new NotImplementedException();
    }

    private static void CheckNewCat(string v)
    {
        //throw new NotImplementedException();
    }

    private static void CheckUpgradeCook()
    {
        //throw new NotImplementedException();
    }

    private static void CheckUpgradeCraft()
    {
        //throw new NotImplementedException();
    }

    private static void CheckUpgradeTrap()
    {
        //throw new NotImplementedException();
    }

    private static void CheckUpgradeFarm()
    {
        //throw new NotImplementedException();
    }

    private static void CheckUpgradeGift()
    {
        //throw new NotImplementedException();
    }

    private static void CheckMapObject()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_SPOT);
        if (necoData != null)
        {
            foreach (neco_spot data in necoData)
            {
                if (data != null)
                {
                    data.RefreshItem();
                }
            }
        }
    }

    private static void CheckFarmLevelup()
    {
        //throw new NotImplementedException();
    }

    private static void CheckTrapLevelup()
    {
        //throw new NotImplementedException();
    }

    private static void CheckGiftLevelup()
    {
        //throw new NotImplementedException();
    }

    private static void CheckCraftLevelup()
    {
        //throw new NotImplementedException();
    }

    private static void CheckCookLevelup()
    {
        //throw new NotImplementedException();
    }
}
