using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChristmasMissionInfo : MonoBehaviour
{
    public enum MISSION_STATE { NONE, PROGRESS, FULL, COMPLETE }

    public ChristmasMissionLayer masterLayer;

    [Header("RewardItem")]
    public Image itemIcon;
    public Text itemAmount;

    [Header("Mission Label")]
    public TextLocalize missionText;

    [Header("Button Layer")]
    public GameObject buttonComplete;
    public GameObject buttonIncomplete;
    public GameObject buttonDone;
    public GameObject CompDimd;

    public int missionIndex = 0;
    
    public void Init(int index, string key, neco_event_xmas_missions reward)
    {
        missionIndex = index;
        missionText.TextKey = key;
        itemIcon.sprite = items.GetItem(reward.GetNecoEventRewardID()).GetItemIcon();
        itemAmount.text = reward.GetNecoEventRewardCount().ToString();
        SetMissionInfo(MISSION_STATE.PROGRESS);
    }

    public void SetMissionInfo(MISSION_STATE state)
    {
        buttonComplete.SetActive(false);
        buttonIncomplete.SetActive(false);
        buttonDone.SetActive(false);
        CompDimd.SetActive(false);

        switch (state)
        {
            case MISSION_STATE.PROGRESS:
                buttonIncomplete.SetActive(true);
                break;
            case MISSION_STATE.FULL:
                buttonDone.SetActive(true);
                break;
            case MISSION_STATE.COMPLETE:
                buttonComplete.SetActive(true);
                CompDimd.SetActive(true);
                //Dimd ON
                break;
        }
    }

    public void OnClickProgressDone()
    {
        //missionIndex가 미션 번호
        //WWW 통신에 미션 번호를 담아 쏘기

        //정상일 때 SetMissionInfo(MISSION_STATE.COMPLETE);
        WWWForm data = new WWWForm();
        data.AddField("uri", "event");
        data.AddField("eid", (int)neco_event.EVENT_TYPE.CHRISTMAS);
        data.AddField("op", 21);
        data.AddField("mid", missionIndex);

        NetworkManager.GetInstance().SendApiRequest("event", 21, data, (response) =>
        {
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
                if (uri == "event")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            JArray missions = (JArray)row["mission"];

                            SetMissionInfo((MISSION_STATE)((JObject)missions[0]).GetValue("state").ToObject<int>());

                            List<RewardData> ret = new List<RewardData>();
                            if (row.ContainsKey("rew"))
                            {
                                JArray incomeArr = (JArray)((JObject)row["rew"])["item"];
                                foreach (JObject income in incomeArr)
                                {
                                    RewardData reward = new RewardData();
                                    reward.itemData = items.GetItem(income["id"].Value<uint>());
                                    reward.count = income["amount"].Value<uint>();
                                    ret.Add(reward);
                                }
                            }

                            NecoCanvas.GetPopupCanvas().OnRewardListPopup(LocalizeData.GetText("LOCALIZE_233"), LocalizeData.GetText("LOCALIZE_234"), ret, masterLayer.Init);
                        }
                        else
                        {
                            ChristmasEventPopup.ChristmasEventWWWErrorCode(rs, true);
                        }
                    }
                }
            }
        });
    }
}
