using System;
using System.Collections;
using System.Collections.Generic;
using EasyMobile;
using GooglePlayGames;
using UnityEngine;

public class GameServiceManager : MonoBehaviour
{
    static private GameServiceManager _Instance;
    public static GameServiceManager GetInstance()
    {
        return _Instance;
    }

    private void OnEnable()
    {
        _Instance = this;

        GameServices.UserLoginSucceeded += OnUserLoginSucceeded;
        GameServices.UserLoginFailed += OnUserLoginFailed;
    }

    private void OnDisable()
    {
        if(_Instance == this)
        {
            _Instance = null;
        }

        GameServices.UserLoginSucceeded -= OnUserLoginSucceeded;
        GameServices.UserLoginFailed -= OnUserLoginFailed;
    }

    public void OnClickAchievementIcon()
    {
        GameServices.ShowAchievementsUI();

        // 업적을 확인했으면 레드닷 off
        NecoCanvas.GetUICanvas().SetAchievementRedDotState(false);
        NecoCanvas.GetUICanvas().RefreshTopMenuRedDot();
    }

    public float GetAchievementValue(string achievementName)
    {
        float progressResult = 0f;

        return progressResult;
    }

    public void TryAchievementProgress(string achievementName, double progress, Action<bool> successCallback)
    {
        GameServices.ReportAchievementProgress(achievementName, progress, successCallback);

        // 현재는 1단계 완료형 업적만 있으므로 해당 부분에서 업적 레드닷 갱신
        NecoCanvas.GetUICanvas().SetAchievementRedDotState(true);
        NecoCanvas.GetUICanvas().RefreshTopMenuRedDot();
    }

    void OnUserLoginSucceeded()
    {
        Debug.Log("User logged in successfully.");
    }

    void OnUserLoginFailed()
    {
        Debug.Log("User login failed.");
    }
}
