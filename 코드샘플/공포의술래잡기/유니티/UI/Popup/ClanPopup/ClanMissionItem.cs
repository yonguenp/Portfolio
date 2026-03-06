using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanMissionItem : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] Text missionName;
    [SerializeField] Slider slider;
    [SerializeField] Text gaugeValue;
    [SerializeField] GameObject clear;
    [SerializeField] Button RewardButton;
    int index = 0;
    public void Init(int idx, JObject data)
    {
        index = idx;
        int maxValue = 0;
        switch (idx)
        {
            //출석
            case 1:
                icon.sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/quest_login");
                maxValue = 1;
                missionName.text = StringManager.GetString("클랜퀘스트_출첵");

                SetGaugeValue(data["check"].Value<int>(), maxValue);
                break;
            //플레이
            case 2:
                icon.sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/quest_match");
                maxValue = 3;
                missionName.text = StringManager.GetString("클랜 퀘스트_듀오");
                SetGaugeValue(data["clan_match"].Value<int>(), maxValue);
                break;
            //승리 수
            case 3:
                icon.sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/quest_win");
                maxValue = 3;
                missionName.text = StringManager.GetString("클랜 퀘스트_듀오승리");
                SetGaugeValue(data["clan_win"].Value<int>(), maxValue);
                break;
        }

    }

    public void ClearBtn()
    {
        if (slider.value < slider.maxValue)
            return;

        var clanPopup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP) as ClanPopup;
        clanPopup.prelv = GetClanLv();
        clanPopup.ClanRequestMissionClear(index, (res) =>
        {
            SetGaugeValue(slider.maxValue, slider.maxValue);
        });
    }

    public int GetClanLv()
    {
        var clanPopup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP) as ClanPopup;
        int lv = clanPopup.ClanInfo["my"]["level"].Value<int>();
        return lv;
    }

    public void SetGaugeValue(float value, float maxValue)
    {
        if (index == 1)
            value = 1;

        slider.maxValue = maxValue;
        slider.minValue = 0;

        slider.value = value;

        gaugeValue.text = value.ToString() + " / " + slider.maxValue.ToString();

        clear.SetActive(slider.value == slider.maxValue);

        RewardButton.interactable = slider.value == maxValue;
    }
}
