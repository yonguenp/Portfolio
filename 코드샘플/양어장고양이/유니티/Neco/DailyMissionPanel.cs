using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DailyMissionPanel : MonoBehaviour
{
    [Header("[Daily Mission Info List]")]
    public GameObject dailyListScrollContainer;
    public GameObject dailyListCloneObject;

    SeasonPassPanel rootParentPanel;

    public void InitDailyMissionUI(SeasonPassPanel parentPanel)
    {
        rootParentPanel = parentPanel;

        bool reciveAllEnable = false;
        List<neco_mission> dailyMissionList = neco_mission.GetNecoMissionDataListByRepeatType(neco_mission.MISSION_TYPE.DAILY_MISSION);
        if (dailyMissionList != null)
        {
            foreach (neco_mission mission in dailyMissionList)
            {
                neco_mission_data d = neco_data.Instance.GetPassData().GetMissionData(mission.GetNecoMissionID());
                if (d != null)
                {
                    if (d.GetState() == 2)
                    {
                        reciveAllEnable = true;
                        break;
                    }
                }
            }
        }
        rootParentPanel.SetActiveReciveAllButton(reciveAllEnable);

        ClearData();

        StopAllCoroutines();
        StartCoroutine(SetDailyMissionList(dailyMissionList));

        dailyListScrollContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        //neco_data.Instance.GetPassData().SetDailyAlarm(false);
    }

    public void DoReceiveAllReward()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "mission");
        data.AddField("op", 5);
        data.AddField("type", 1);

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
                                    neco_data.Instance.GetPassData().SetSeasonAlarm(false);
                                    rootParentPanel.UpdateRedDotState();
                                    rootParentPanel.SetActiveReciveAllButton(neco_data.Instance.GetPassData().IsSeasonAlarm());
                                    rootParentPanel.RefreshLayer();
                                });
                            }
                            else
                            {
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_85"), LocalizeData.GetText("MISSION_COMPLETE"), () => {
                                    neco_data.Instance.GetPassData().SetSeasonAlarm(false);
                                    rootParentPanel.UpdateRedDotState();
                                    rootParentPanel.SetActiveReciveAllButton(neco_data.Instance.GetPassData().IsSeasonAlarm());
                                    rootParentPanel.RefreshLayer();
                                });
                            }
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_509"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_499"); break;
                                case 3: msg = LocalizeData.GetText("LOCALIZE_333"); break;
                                case 4: msg = LocalizeData.GetText("LOCALIZE_500"); break;
                                case 5: msg = LocalizeData.GetText("LOCALIZE_501"); break;
                                case 6: msg = LocalizeData.GetText("LOCALIZE_502"); break;
                                case 7: msg = LocalizeData.GetText("LOCALIZE_278"); break;
                                case 8: msg = LocalizeData.GetText("LOCALIZE_500"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_278"), msg);
                        }
                    }
                }
            }
        });
    }

    IEnumerator SetDailyMissionList(List<neco_mission> list)
    {
        dailyListCloneObject.SetActive(false);

        if (dailyListScrollContainer != null && dailyListCloneObject != null)
        {

            foreach (Transform child in dailyListScrollContainer.transform)
            {
                if (child.gameObject != dailyListCloneObject)
                {
                    Destroy(child.gameObject);
                }
            }

            // 데이터 리스트 정렬
            var dailyMissionList = list.OrderBy(x => OrderState(neco_data.Instance.GetPassData().GetMissionData(x.GetNecoMissionID()).GetState())).ThenBy(x => x.GetNecoMissionID());
            List<string> attachedTrigerTypes = new List<string>();

            if (dailyMissionList != null)
            {
                int FirstDisplayCount = 6;
                foreach (neco_mission missionData in dailyMissionList)
                {
                    neco_mission_data md = neco_data.Instance.GetPassData().GetMissionData(missionData.GetNecoMissionID());
                    if (0 == md.GetState())
                    {
                        continue;
                    }

                    GameObject missionInfoUI = Instantiate(dailyListCloneObject);
                    missionInfoUI.transform.SetParent(dailyListScrollContainer.transform);
                    missionInfoUI.transform.localScale = dailyListCloneObject.transform.localScale;
                    missionInfoUI.transform.localPosition = dailyListCloneObject.transform.localPosition;
                    missionInfoUI.SetActive(true);

                    MissionListInfo missionInfoComponent = missionInfoUI.GetComponent<MissionListInfo>();
                    missionInfoComponent.InitMissionInfoData(missionData, rootParentPanel);

                    if (FirstDisplayCount > 0)
                        FirstDisplayCount--;
                    else
                        yield return new WaitForSeconds(0.05f);
                }
            }
        }
    }

    public uint OrderState(uint parm)
    {
        switch (parm)
        {
            case 1:
                return 2;
            case 2:
                return 1;
            default:
                return 3;
        }
    }

    void RefreshData()
    {
        ClearData();

        SetDailyMissionList(neco_mission.GetNecoMissionDataListByRepeatType(neco_mission.MISSION_TYPE.DAILY_MISSION));
    }

    void ClearData()
    {

    }
}
