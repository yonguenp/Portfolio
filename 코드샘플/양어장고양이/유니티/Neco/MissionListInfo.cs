using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionListInfo : MonoBehaviour
{
    [Header("[Mission Info Layer]")]
    public Image missionImage;
    public Text missionNameText;
    public Text missionExpText;
    public Slider missionExpSlider;
    public Text missionSliderExpText;

    [Header("[Button Layer]")]
    public GameObject enableButton;
    public GameObject disableButton;
    public GameObject completeButton;

    [Header("[Dimmed Layer]")]
    public GameObject dimmedLayer;

    neco_mission curMissionData;
    SeasonPassPanel rootParentPanel;

    public void OnClickReceiveButton()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "mission");
        data.AddField("op", 5);
        data.AddField("mid", curMissionData.GetNecoMissionID().ToString());

        NetworkManager.GetInstance().SendApiRequest("mission", 5, data, (response) =>
        {
            //패스 레벨 만렙시 별도보상을줌
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "mission")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            if (row.ContainsKey("rew"))
                            {
                                NecoCanvas.GetPopupCanvas().OnRewardPopupShow(LocalizeData.GetText("LOCALIZE_200"), LocalizeData.GetText("LOCALIZE_201"), "mission", apiArr, () => {
                                    rootParentPanel.UpdateRedDotState();
                                    if (curMissionData.GetMissionType() == neco_mission.MISSION_TYPE.DAILY_MISSION)
                                    {
                                        rootParentPanel.SetActiveReciveAllButton(neco_data.Instance.GetPassData().IsDailyAlarm());
                                    }
                                    else if (curMissionData.GetMissionType() == neco_mission.MISSION_TYPE.SEASON_MISSON)
                                    {
                                        rootParentPanel.SetActiveReciveAllButton(neco_data.Instance.GetPassData().IsSeasonAlarm());
                                    }
                                    rootParentPanel.RefreshLayer();
                                });
                            }
                            else
                            {
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_85"), LocalizeData.GetText("MISSION_COMPLETE"), rootParentPanel.RefreshLayer);
                            }
                        }
                    }
                }
            }                   
        });
    }

    public void InitMissionInfoData(neco_mission missionData, SeasonPassPanel parentPanel)
    {
        curMissionData = missionData;
        rootParentPanel = parentPanel;

        SetMissionInfoData();

        UpdateButtonState();
    }

    void SetMissionInfoData()
    {
        if (curMissionData == null) { return; }

        // 레이어 정보 세팅
        missionImage.sprite = Resources.Load<Sprite>(curMissionData.GetNecoMissionIcon());
        missionNameText.text = curMissionData.GetNecoMissionNameKr();
        missionExpText.text = string.Format(LocalizeData.GetText("LOCALIZE_231"), curMissionData.GetNecoMissionExp());

        // 슬라이더 정보 세팅
        neco_mission_data md = neco_data.Instance.GetPassData().GetMissionData(curMissionData.GetNecoMissionID());
        uint nowCount = md.GetCurValue();        
        uint maxCount = curMissionData.GetMissionMaxCount();

        if (md.GetState() != 1)
            nowCount = maxCount;
        
        missionExpSlider.value = (float)nowCount / maxCount;
        missionSliderExpText.text = string.Format("{0} / {1}", nowCount, maxCount);
    }

    void UpdateButtonState() 
    {
        // 버튼 상태 리셋
        enableButton.SetActive(false);
        disableButton.SetActive(false);
        completeButton.SetActive(false);

        neco_mission_data md = neco_data.Instance.GetPassData().GetMissionData(curMissionData.GetNecoMissionID());
        switch(md.GetState())
        {
            case 2:
                enableButton.SetActive(true);
                break;
            case 3:
                completeButton.SetActive(true);
                break;
            case 1:
            default:
                disableButton.SetActive(true);
                break;
        }

        dimmedLayer.SetActive(md.GetState() == 3);
    }
}
