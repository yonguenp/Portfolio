using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SandboxNetwork;
using Newtonsoft.Json.Linq;
using TMPro;

public class ArenaTeamInfoPopup : Popup<PopupData>
{
    [Header("Team Info")]
    [SerializeField]
    private Text offenceBattlePointLabel = null;
    [SerializeField]
    private Text defenceBattlePointLabel = null;
    public void init()
    {
        RefreshData();
    }
    public void RefreshData()
    {
        RefreshOffenceBattlePoint();
        RefreshDefenceBattlePoint();
    }

    public void OnClickOffenceSettingButton()
    {
        ArenaManager.Instance.SetArenaTeamModeData(true);
        RequestSceneChange("ArenaTeamSetting");
    }

    public void OnClickDefenceSettingButton()
    {
        ArenaManager.Instance.SetArenaTeamModeData(false);
        RequestSceneChange("ArenaTeamSetting");
    }
    private void RequestSceneChange(string targetSceneName)
    {
        //SoundManager.Instance.AllStopSound();
        //LoadingManager.ImmediatelySceneLoad(targetSceneName);
        LoadingManager.Instance.EffectiveSceneLoad(targetSceneName, eSceneEffectType.CloudAnimation);
    }

    public void RefreshOffenceBattlePoint()
    {
        var currentBattleLine = User.Instance.PrefData.ArenaFormationData;
        var atkBattleLine = User.Instance.PrefData.ArenaFormationData.TeamFormationATK[CacheUserData.GetInt("presetArenaAtkDeck", 0)];

        if (atkBattleLine==null || atkBattleLine.Count <= 0)
        {
            offenceBattlePointLabel.text = "0";
            return;
        }
        offenceBattlePointLabel.text = CalcTotalBattlePoint(atkBattleLine).ToString();
    }
    public void RefreshDefenceBattlePoint()
    {
        //int currentTeamPresetNo = CacheUserData.GetInt("presetArenaDefDeck", 0);
        var currentBattleLine = User.Instance.PrefData.ArenaFormationData;
        var defBattleLine = currentBattleLine.TeamFormationDEF[0];

        if (defBattleLine == null || defBattleLine.Count <= 0)
        {
            defenceBattlePointLabel.text = "0";
            return;
        }
        defenceBattlePointLabel.text = CalcTotalBattlePoint(defBattleLine).ToString();
    }

    public override void InitUI()
    {

    }

    public int CalcTotalBattlePoint(List<int> dragonTagList)
    {
        float totalPoint = 0;
        if (dragonTagList == null || dragonTagList.Count <= 0) return 0;

        foreach(int tag in dragonTagList)
        {
            if (tag <= 0) continue;

            var dragonData = User.Instance.DragonData.GetDragon(tag);
            if (dragonData == null) 
                continue;

            if (dragonData.Status == null)
                continue;

            totalPoint += dragonData.Status.GetTotalINF();
        }

        return Mathf.FloorToInt(totalPoint);
    }

    public void onClickExpectGameAlphaUpdate()
    {

    }
}
