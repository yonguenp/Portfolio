using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationUI : MonoBehaviour
{
    public string MyNotificationKey;
    public string ParentNotificationKey;

    public GameObject NotificationObject;

    public void Awake()
    {
        if (NotificationObject != null)
        {
            if (NotificationObject == this.gameObject)
            {
                Debug.LogError("NotificationObject는 자신의 오브젝트를 사용할수 없습니다.");
            }
            else
            {
                UINotifiedManager.SetNotificationUI(this);
            }
        }
    }
        
    public void OnDestroy()
    {
        UINotifiedManager.RemoveNotificationUI(this);
    }

    public void OnEnable()
    {
        CheckNotification();
        RefershPivotTime();
    }

    public void OnDisable()
    {        
        UINotifiedManager.RefershState(this);
    }

    public void RefershPivotTime()
    {
        PlayerPrefs.SetInt(MyNotificationKey, (int)NecoCanvas.GetCurTime());
    }

    public DateTime GetPivotTime()
    {
        int sec = PlayerPrefs.GetInt(MyNotificationKey, 0);
        return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(sec);         
    }

    public void CheckNotification()
    {
        OnNotification(UINotifiedManager.CheckState(this));
    }

    public void OnNotification(bool enable)
    {
        NotificationObject.SetActive(enable);
    }
}
