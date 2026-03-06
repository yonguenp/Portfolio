using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuFishTrapButton : MonoBehaviour
{
    public GameObject FishtrapNoneIcon;
    public GameObject FishtrapSomeIcon;
    public GameObject FishtrapMaxIcon;

    public GameObject NotifyEffectObject;

    [Header("[FishTrapBooster Layer]")]
    public GameObject boosterObject;
    public GameObject boosterArrowObject;

    public Slider boosterSlider;
    public Text boosterRemainTimeText;

    uint maxBoostTime = 0;

    Coroutine coroutineMainMenuFishtrapBoost = null;

    private void OnEnable()
    {
        FishtrapMaxIcon.SetActive(false);
        FishtrapSomeIcon.SetActive(false);
        Refresh();

        RefreshBooster();
    }

    public void OnClickFishtrapButton()
    {
        if (CheckPrologueWithToastAlarm()) { return; }

        WWWForm data = new WWWForm();
        data.AddField("api", "plant");
        data.AddField("op", 3);
        data.AddField("id", (int)SUPPLY_UI_TYPE.FISH_TRAP);

        NetworkManager.GetInstance().SendApiRequest("plant", 3, data, (response) =>
        {
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            List<RewardData> rewards = new List<RewardData>();

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
                            case 3: //OpPlant::HARVEST  
                            {
                                JToken resultCode = row["rs"];
                                if (resultCode != null && resultCode.Type == JTokenType.Integer)
                                {
                                    int rs = resultCode.Value<int>();
                                    if (rs == 0)
                                    {
                                        if (row.ContainsKey("rew"))
                                        {
                                            if (row["rew"].HasValues)
                                            {
                                                JObject income = (JObject)row["rew"];
                                                if (income.ContainsKey("gold"))
                                                {
                                                    RewardData reward = new RewardData();
                                                    reward.gold = income["gold"].Value<uint>();
                                                    rewards.Add(reward);
                                                }


                                                if (income.ContainsKey("item"))
                                                {
                                                    JArray item = (JArray)income["item"];
                                                    foreach (JObject rw in item)
                                                    {
                                                        RewardData reward = new RewardData();
                                                        reward.itemData = items.GetItem(rw["id"].Value<uint>());
                                                        reward.count = rw["amount"].Value<uint>();
                                                        rewards.Add(reward);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        string msg = rs.ToString();
                                        switch (rs)
                                        {
                                            case 1: msg = LocalizeData.GetText("LOCALIZE_499"); break;
                                            case 2: msg = LocalizeData.GetText("LOCALIZE_199"); break;
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

            ShowFishtrapRewardPopup(rewards);
        });
    }

    public void Refresh()
    {
        CancelInvoke("StateUpdate");

        neco_data.GAIN_STATE state = neco_data.Instance.CurFishState();
        switch (state)
        {
            case neco_data.GAIN_STATE.NONE:
                if (FishtrapNoneIcon.activeSelf == false)
                {
                    FishtrapNoneIcon.SetActive(true);
                    FishtrapNoneIcon.GetComponentInChildren<Animator>().Play("Fishtrap_idle");
                }
                FishtrapSomeIcon.SetActive(false);
                FishtrapMaxIcon.SetActive(false);
                break;
            case neco_data.GAIN_STATE.SOME:
                if (FishtrapSomeIcon.activeSelf == false)
                {
                    FishtrapSomeIcon.SetActive(true);
                    FishtrapSomeIcon.GetComponentInChildren<Animator>().Play("Fishtrap_Max");
                }
                FishtrapNoneIcon.SetActive(false);
                FishtrapMaxIcon.SetActive(false);
                break;
            case neco_data.GAIN_STATE.FULL:
                if (FishtrapMaxIcon.activeSelf == false)
                {
                    FishtrapMaxIcon.SetActive(true);
                    FishtrapMaxIcon.GetComponentInChildren<Animation>().Play("fishtrap_Full");
                }
                FishtrapNoneIcon.SetActive(false);
                FishtrapSomeIcon.SetActive(false);
                break;
        }

        if (gameObject.activeInHierarchy)
        {
            uint next = neco_data.Instance.GetFishNextUpdateRemain();
            if (next > 0)
            {
                if (neco_data.Instance.IsFishTrapNeedUpdate())
                {
                    StateUpdate();
                }
                else
                    Invoke("StateUpdate", next);
            }
        }

        // 프롤로그 강조 연출 처리
        NotifyEffectObject.SetActive(neco_data.GetPrologueSeq() <= neco_data.PrologueSeq.통발UI등장);
    }

    void StateUpdate()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "plant");
        data.AddField("op", 3);
        data.AddField("id", (int)SUPPLY_UI_TYPE.FISH_TRAP);

        NetworkManager.GetInstance().SendApiRequest("plant", 3, data, (response) =>
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
                            case 3: //OpPlant::HARVEST  
                                {
                                    JToken resultCode = row["rs"];
                                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                                    {
                                        int rs = resultCode.Value<int>();
                                        if (rs == 0)
                                        {
                                            Invoke("Refresh", 0.1f);
                                        }
                                        else
                                        {
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
    }

    #region 부스터 관련 처리

    void RefreshBooster()
    {
        if (neco_data.Instance.GetTrapBoostTime() > NecoCanvas.GetCurTime())
        {
            // 부스트 타이머 처리
            if (gameObject.activeInHierarchy)
            {
                RefreshFishtrapBoostTimeData();
            }
        }

        boosterObject.SetActive(neco_data.Instance.GetTrapBoostTime() > NecoCanvas.GetCurTime());
        boosterArrowObject.SetActive(neco_data.Instance.GetTrapBoostTime() > NecoCanvas.GetCurTime());
    }

    void RefreshFishtrapBoostTimeData()
    {
        if (coroutineMainMenuFishtrapBoost != null)
        {
            StopCoroutine(coroutineMainMenuFishtrapBoost);
        }

        coroutineMainMenuFishtrapBoost = StartCoroutine(RefreshFishtrapBoostTime());
    }

    IEnumerator RefreshFishtrapBoostTime()
    {
        // 부스트 기존 최대 시간 계산
        if (neco_data.Instance.GetTrapBoostType() == neco_data.BOOST_TYPE.CATNIP_BOOST)
        {
            maxBoostTime = neco_booster.GetBoosterData(2).GetEffectTime();
        }
        else if (neco_data.Instance.GetTrapBoostType() == neco_data.BOOST_TYPE.AD_BOOST)
        {
            maxBoostTime = neco_booster.GetBoosterData(4).GetEffectTime();
        }

        // 남은 부스트 시간 계산
        uint remain = neco_data.Instance.GetTrapBoostTime();

        while (remain > NecoCanvas.GetCurTime())
        {
            SetBoostRemainTime(remain - NecoCanvas.GetCurTime());
            yield return new WaitForSeconds(1.0f);
        }

        // 부스트 시간 만료시 처리
        SetBoostRemainTime(0);
        neco_data.Instance.SetTrapBoostTime(0, neco_data.BOOST_TYPE.NONE);

        boosterObject.SetActive(false);
        boosterArrowObject.SetActive(false);
    }

    void SetBoostRemainTime(uint remainTime)
    {
        uint minute = remainTime / 60;
        uint second = remainTime % 60;

        boosterRemainTimeText.text = string.Format("{0:D2}:{1:D2}", minute, second);

        boosterSlider.value = (float)remainTime / maxBoostTime;
    }

    #endregion

    void ShowFishtrapRewardPopup(List<RewardData> rewardList)
    {
        NecoCanvas.GetPopupCanvas().OnFishTrapPopupShow(rewardList, SUPPLY_UI_TYPE.FISH_TRAP);
    }

    bool CheckPrologueWithToastAlarm()
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();

        switch (seq)
        {
            case neco_data.PrologueSeq.양어장UI등장:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_262"));
                return true;
            case neco_data.PrologueSeq.철판제작가이드퀘스트:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ13"));
                return true;
            case neco_data.PrologueSeq.조리대레벨업:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ17"));
                return true;
            case neco_data.PrologueSeq.상점배스구매가이드퀘스트:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ19"));
                return true;
            case neco_data.PrologueSeq.배스구이강조:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ23"));
                return true;
            case neco_data.PrologueSeq.배스구이완료후밥그릇강조:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_400"));
                return true;
            case neco_data.PrologueSeq.뒷길막이선물생성:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("뒷길막이선물생성"));
                return true;
            case neco_data.PrologueSeq.보은바구니UI등장:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ25"));
                return true;
            case neco_data.PrologueSeq.낚시장난감만들기:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ27"));
                return true;
            case neco_data.PrologueSeq.길막이낚시장난감배치:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("tutorial_block"));
                return true;
            case neco_data.PrologueSeq.낚시장난감오브젝트레벨업가이드:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_401"));
                return true;
            case neco_data.PrologueSeq.길막이만지기돌발발생:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("tutorial_block"));
                return true;
            case neco_data.PrologueSeq.사진찍기돌발대사:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("tutorial_block"));
                return true;
            case neco_data.PrologueSeq.스와이프가이드:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_402"));
                return true;
            case neco_data.PrologueSeq.배틀패스강조및대사:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("패틀패스강조"));
                return true;
        }

        return false;
    }
}
