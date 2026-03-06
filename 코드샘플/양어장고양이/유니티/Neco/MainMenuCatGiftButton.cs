using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuCatGiftButton : MonoBehaviour
{
    const int MAX_BOOST_TIME = 3600;

    public GameObject GiftSomeIcon;
    public GameObject GiftMaxIcon;

    public GameObject NotifyEffectObject;

    [Header("[FishFarmBooster Layer]")]
    public GameObject boosterObject;
    public GameObject boosterArrowObject;

    public Slider boosterSlider;
    public Text boosterRemainTimeText;

    uint maxBoostTime = 0;

    Coroutine coroutineMainMenuCatGiftBoost = null;

    private void OnEnable()
    {
        GiftMaxIcon.SetActive(false);
        GiftSomeIcon.SetActive(false);
        Refresh();

        RefreshBooster();
    }

    public void OnClickCatGiftButton()
    {
        if (CheckPrologueWithToastAlarm()) { return; }

        GetGiftItemAndPopupShow();
    }

    public void Refresh()
    {
        CancelInvoke("StateUpdate");

        neco_data.GAIN_STATE state = neco_data.Instance.CurGiftState();
        switch(state)
        {
            case neco_data.GAIN_STATE.NONE:
            case neco_data.GAIN_STATE.SOME:
                if (GiftSomeIcon.activeSelf == false)
                {
                    GiftSomeIcon.SetActive(true);
                    GiftSomeIcon.GetComponent<Animation>().Play("gift_idle");
                }
                GiftMaxIcon.SetActive(false);
                break;
            case neco_data.GAIN_STATE.FULL:
                if (GiftMaxIcon.activeSelf == false)
                {
                    GiftMaxIcon.SetActive(true);
                    GiftMaxIcon.GetComponent<Animation>().Play("gift_max");
                }
                GiftSomeIcon.SetActive(false);
                break;
        }

        uint next = neco_data.Instance.NextGiftUpdate();
        if (next > 0)
            Invoke("StateUpdate", next);

        // 프롤로그 강조 연출 처리
        NotifyEffectObject.SetActive(neco_data.GetPrologueSeq() <= neco_data.PrologueSeq.보은바구니UI등장);
    }

    void StateUpdate()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "plant");
        data.AddField("op", 3);
        data.AddField("id", (int)SUPPLY_UI_TYPE.CAT_GIFT);

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

    void GetGiftItemAndPopupShow()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "object");
        data.AddField("op", 3);

        NetworkManager.GetInstance().SendApiRequest("object", 3, data, (response) =>
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
                if (uri == "object")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            Invoke("ShowGiftItemPopup", 0.1f);
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_499"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_502"); break;
                                case 3: msg = LocalizeData.GetText("LOCALIZE_278"); break;
                                case 4: msg = LocalizeData.GetText("LOCALIZE_504"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_278"), msg);
                        }
                    }
                }
            }
        });
    }

    #region 부스터 관련 처리

    void RefreshBooster()
    {
        if (neco_data.Instance.GetGiftBoostTime() > NecoCanvas.GetCurTime())
        {
            // 부스트 타이머 처리
            if (gameObject.activeInHierarchy)
            {
                RefreshCatGiftBoostTimeData();
            }
        }

        boosterObject.SetActive(neco_data.Instance.GetGiftBoostTime() > NecoCanvas.GetCurTime());
        boosterArrowObject.SetActive(neco_data.Instance.GetGiftBoostTime() > NecoCanvas.GetCurTime());
    }

    void RefreshCatGiftBoostTimeData()
    {
        if (coroutineMainMenuCatGiftBoost != null)
        {
            StopCoroutine(coroutineMainMenuCatGiftBoost);
        }

        coroutineMainMenuCatGiftBoost = StartCoroutine(RefreshCatgiftBoostTime());
    }

    IEnumerator RefreshCatgiftBoostTime()
    {
        // 부스트 기존 최대 시간 계산
        if (neco_data.Instance.GetTrapBoostType() == neco_data.BOOST_TYPE.CATNIP_BOOST)
        {
            maxBoostTime = neco_booster.GetBoosterData(3).GetEffectTime();
        }
        else if (neco_data.Instance.GetTrapBoostType() == neco_data.BOOST_TYPE.AD_BOOST)
        {
            maxBoostTime = neco_booster.GetBoosterData(4).GetEffectTime();
        }

        // 남은 부스트 시간 계산
        uint remain = neco_data.Instance.GetGiftBoostTime();

        while (remain > NecoCanvas.GetCurTime())
        {
            SetBoostRemainTime(remain - NecoCanvas.GetCurTime());
            yield return new WaitForSeconds(1.0f);
        }

        // 부스트 시간 만료시 처리
        SetBoostRemainTime(0);
        neco_data.Instance.SetGiftBoostTime(0, neco_data.BOOST_TYPE.NONE);

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

    void ShowGiftItemPopup()
    {
        NecoCanvas.GetPopupCanvas().OnSupplyContentsPopupShow(neco_data.Instance.GetGiftList(), SUPPLY_UI_TYPE.CAT_GIFT);
    }

    bool CheckPrologueWithToastAlarm()
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();

        switch (seq)
        {
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
