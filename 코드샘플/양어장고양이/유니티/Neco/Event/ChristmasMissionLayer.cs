using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChristmasMissionLayer : MonoBehaviour
{
    /*
    LOCALIZE_108		달성완료
    LOCALIZE_109		미달성
    LOCALIZE_110		획득완료
    LOCALIZE_111		보상받기
    */

    [Header("Clone")]
    public GameObject missionObjClone;

    [Header("Objects")]
    public RectTransform missionContentRT;

    public Button clearAllBtn;

    List<game_data> missionDatas;
    List<GameObject> clones;

    int canReward = 0;

    private void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        //미션 갯수대로 미션Obj Clone 복사 후 개별 이니셜 메서드 실행
        if (missionDatas == null)
            missionDatas = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_EVENT_XMAS_MISSIONS);

        if (clones == null)
            clones = new List<GameObject>();

        if(clones.Count == 0)
        {
            int index = 0;
            foreach (neco_event_xmas_missions mission in missionDatas)
            {
                GameObject obj = Instantiate(missionObjClone, missionContentRT);
                ChristmasMissionInfo missionScript = obj.GetComponent<ChristmasMissionInfo>();

                missionScript.Init(++index, string.Format("CHRISTMAS_MISSION_{0}", mission.GetNecoEventID()), mission);
                clones.Add(obj);
                obj.SetActive(true);
            }
        }

        //미션 정보 WWW통신
        canReward = 0;
        clearAllBtn.interactable = false;
        WWWForm data = new WWWForm();
        data.AddField("uri", "event");
        data.AddField("eid", (int)neco_event.EVENT_TYPE.CHRISTMAS);
        data.AddField("op", 20);

        NetworkManager.GetInstance().SendApiRequest("event", 20, data, (response) =>
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
                            JArray missions = (JArray)row["missions"];

                            foreach(JObject mission in missions)
                            {
                                clones[mission.GetValue("id").ToObject<int>()-1].GetComponent<ChristmasMissionInfo>().SetMissionInfo((ChristmasMissionInfo.MISSION_STATE)mission.GetValue("state").ToObject<int>());
                                if ((ChristmasMissionInfo.MISSION_STATE)mission.GetValue("state").ToObject<int>() == ChristmasMissionInfo.MISSION_STATE.FULL)
                                    canReward++;
                            }

                            if (canReward > 0)
                                clearAllBtn.interactable = true;
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

    public void ClearAllMission()
    {
        if (canReward == 0)
            return;

        clearAllBtn.interactable = false;

        WWWForm data = new WWWForm();
        data.AddField("uri", "event");
        data.AddField("eid", (int)neco_event.EVENT_TYPE.CHRISTMAS);
        data.AddField("op", 21);

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

                            foreach (JObject mission in missions)
                            {
                                clones[mission.GetValue("id").ToObject<int>() - 1].GetComponent<ChristmasMissionInfo>().SetMissionInfo((ChristmasMissionInfo.MISSION_STATE)mission.GetValue("state").ToObject<int>());
                            }

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

                            NecoCanvas.GetPopupCanvas().OnRewardListPopup(LocalizeData.GetText("LOCALIZE_233"), LocalizeData.GetText("Xmasbox_openReward"), ret, Init);
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