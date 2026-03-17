using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CatnipFarmObjectSpot : MapObjectSpot
{
    public RectTransform timerObj;
    public Image gauge;
    public Text rewardText;

    uint remainTime;
    uint rewardCount;
    uint catnip_FarmLevel;
    uint moveTowerLevel = 0;
    neco_spot moveTower = null;
    uint towerType = 0;

    bool timeSet;


    IEnumerator timer;

    override protected void Awake()
    {
        base.Awake();

        remainTime = 41400;
        timeSet = false;
        timerObj.gameObject.SetActive(false);
        timer = null;              
    }

    void Init()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "plant");
        data.AddField("op", 2);
        data.AddField("id", 106);

        //To do
        //리퀘스트 부분을 SimpleSend로 변경할 것.
        //후에 onFail 콜백에 
        NetworkManager.GetInstance().SendApiRequest("plant", 2, data, (response) =>
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
                if (uri == "plant")
                {
                    JToken opCode = row["op"];
                    if (opCode != null && opCode.Type == JTokenType.Integer)
                    {
                        int op = opCode.Value<int>();
                        switch (op)
                        {
                            case 2: //OpPlant::REWARD_INFO:  
                                {
                                    JToken resultCode = row["rs"];
                                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                                    {
                                        int rs = resultCode.Value<int>();
                                        if (rs == 0)
                                        {
                                            JToken full = row["full"];

                                            if(((JToken)row["state"]).Value<uint>() == 2)
                                            {
                                                remainTime = 0;
                                            }
                                            else
                                            {
                                                DateTime curTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                                                curTime = curTime.AddSeconds(NecoCanvas.GetCurTime()).ToLocalTime();

                                                DateTime fullTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                                                fullTime = fullTime.AddSeconds(full.Value<uint>()).ToLocalTime();

                                                TimeSpan diff = (fullTime - curTime);
                                                remainTime = (uint)diff.TotalSeconds;
                                            }
                                            Invoke("Refresh", 0.1f);
                                        }
                                        else
                                        {
                                            //서버 통신 오류
                                            string msg = rs.ToString();
                                            switch (rs)
                                            {
                                                case 1: msg = LocalizeData.GetText("LOCALIZE_267"); break;
                                                case 2: msg = LocalizeData.GetText("LOCALIZE_268"); break;
                                            }

                                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_278"), msg);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        });

        uint catnip_FarmLevel = neco_spot.GetNecoSpotObjectByItemID(147).GetSpotLevel();
    }

    public void onTouch()
    {
        if(remainTime > 0)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("캣닢급식소보상준비중"));
            return;
        }

        GetReward();
    }

    void Refresh()
    {
        if (timer != null)
            StopCoroutine(timer);

        timer = TimeClock();
        StartCoroutine(timer);
    }

    void RewardRefesh()
    {
        uint farmLevel = neco_spot.GetNecoSpotObjectByItemID(147).GetSpotLevel();
        uint towerType = 0;
        uint towerLevel = 0;

        if (user_items.GetUserItemAmount(124) > 0 || user_items.GetUserItemAmount(154) > 0)
        {
            towerType = (uint)(user_items.GetUserItemAmount(124) > 0 ? 124 : 154);
            moveTower = neco_spot.GetNecoSpotObjectByItemID(124);
        }
        
        towerLevel = moveTower != null ? moveTower.GetSpotLevel() : 0;

        if (farmLevel != catnip_FarmLevel || towerType != this.towerType || towerLevel != moveTowerLevel)
        {
            catnip_FarmLevel = farmLevel;
            this.towerType = towerType;
            moveTowerLevel = towerLevel;

            catnip_farm farmdata = catnip_farm.GetCatnipFarmData(catnip_FarmLevel, towerType, moveTowerLevel);
            if(farmdata == null)
            {
                farmdata = catnip_farm.GetCatnipFarmData(catnip_FarmLevel, 0, 0);
            }

            rewardCount = farmdata.GetFarmValue();
            rewardText.text = "+" + rewardCount.ToString();
        }
    }

    public void TimeRefresh()
    {
        RewardRefesh();

        if (remainTime > 0)
        {
            timerObj.gameObject.SetActive(true);
            timerObj.Find("Normal").gameObject.SetActive(true);
            timerObj.Find("Max").gameObject.SetActive(false);

            if (gauge != null)
            {
                neco_level curLevel = neco_level.GetNecoLevelDataByObjectID(SpotID, neco_spot.GetNecoSpotObjectByItemID(147).GetSpotLevel());
                float ratio = 0.0f;
                if(curLevel != null)
                {
                    ratio = 1.0f - ((float)remainTime / curLevel.GetNecoLevelTick());
                }

                gauge.fillAmount = ratio;
            }

            uint hour = remainTime / (60 * 60);
            uint minute = (remainTime - (hour * 60 * 60)) / 60;
            uint second = remainTime % 60;

            timerObj.Find("Normal").Find("Time").Find("Text").GetComponent<Text>().text = hour.ToString("00") + ":" + minute.ToString("00") + ":" + second.ToString("00");
        }
        else
        {
            if (gauge != null)
                gauge.fillAmount = 0.0f;

            timerObj.gameObject.SetActive(true);
            timerObj.Find("Normal").gameObject.SetActive(false);
            timerObj.Find("Max").gameObject.SetActive(true);
        }
    }

    IEnumerator TimeClock()
    {
        //1분과 1초마다 시간을 갱신시킴, 로컬 시간이 00:00:00 일 경우 서버와 동기화
        while(remainTime > 0)
        {
            TimeRefresh();
            remainTime--;
            yield return new WaitForSeconds(1f);
        }

        TimeRefresh();
        yield return null;
    }

    public void OnIAPObjectHelpPopup()
    {
        NecoCanvas.GetPopupCanvas().PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.IAP_OBJECT_HELP_POPUP].GetComponent<IAPObjectHelpPopup>().SetUIType(1);
        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.IAP_OBJECT_HELP_POPUP);
    }

    public override void RefreshSpot()
    {
        base.RefreshSpot();

        timerObj.gameObject.SetActive(curSpotData.GetSpotState() >= neco_spot.SPOT_STATE.OBJECT_SET);

        if (curSpotData.GetSpotState() >= neco_spot.SPOT_STATE.OBJECT_SET && !timeSet)
        {
            Init();
        }
    }

    void GetReward()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "plant");
        data.AddField("op", 1);
        data.AddField("id", 106);

        NetworkManager.GetInstance().SendApiRequest("plant", 1, data, Response);
    }

    void Response(string response)
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
            if (uri == "plant")
            {
                JToken opCode = row["op"];
                if (opCode != null && opCode.Type == JTokenType.Integer)
                {
                    int op = opCode.Value<int>();
                    switch (op)
                    {
                        case 1: //OpPlant::REWARD_INFO:  
                            {
                                JToken resultCode = row["rs"];
                                if (resultCode != null && resultCode.Type == JTokenType.Integer)
                                {
                                    int rs = resultCode.Value<int>();
                                    if (rs == 0)
                                    {
                                        //먹음
                                        NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("캣닢급식소보상받기"));
                                    }
                                    else
                                    {
                                        //서버 통신 오류
                                        string msg = rs.ToString();
                                        switch (rs)
                                        {
                                            case 1: msg = LocalizeData.GetText("LOCALIZE_267"); break;
                                            case 2: msg = LocalizeData.GetText("LOCALIZE_268"); break;
                                        }

                                        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_278"), msg);
                                        break;
                                    }
                                }
                            }
                            continue;

                        case 101:
                            {
                                JToken full = row["full"];

                                DateTime curTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                                curTime = curTime.AddSeconds(NecoCanvas.GetCurTime()).ToLocalTime();

                                DateTime fullTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                                fullTime = fullTime.AddSeconds(full.Value<uint>()).ToLocalTime();

                                TimeSpan diff = (fullTime - curTime);
                                remainTime = (uint)diff.TotalSeconds;

                                Invoke("Refresh", 0.1f);
                            }
                            break;
                    }
                }
            }
        }
    }
}