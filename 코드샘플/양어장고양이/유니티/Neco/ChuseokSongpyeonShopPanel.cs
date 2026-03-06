using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ChuseokSongpyeonShopPanel : MonoBehaviour
{
    [Header("[SongPyeon Shop Data]")]
    public GameObject songpeyonPurchaseCountObject;
    public GameObject chuseokShopListContainer;
    public GameObject chuseokShopCloneObject;

    public Text songpyeonAmountText;

    public RectTransform layoutRect;

    ChuseokUI rootParentPanel;

    public void InitSongpyeonShopPanel(ChuseokUI rootPanel)
    {
        rootParentPanel = rootPanel;

        RefreshData();

        chuseokShopListContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
    }

    void SetSongpyeonShopData() 
    {
        ClearData();

        if (chuseokShopListContainer == null || chuseokShopCloneObject == null) { return; }

        foreach (Transform child in chuseokShopListContainer.transform)
        {
            if (child.gameObject != chuseokShopCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        chuseokShopCloneObject.SetActive(true);

        List<game_data> gameDataList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_EVENT_THANKS_SHOP);
        List<neco_event_thanks_shop> eventShopList = gameDataList.Cast<neco_event_thanks_shop>().ToList();

        if (eventShopList != null && eventShopList.Count > 0)
        {
            foreach (neco_event_thanks_shop eventShopData in eventShopList)
            {
                GameObject eventShopInfo = Instantiate(chuseokShopCloneObject);
                eventShopInfo.transform.SetParent(chuseokShopListContainer.transform);
                eventShopInfo.transform.localScale = chuseokShopCloneObject.transform.localScale;
                eventShopInfo.transform.localPosition = chuseokShopCloneObject.transform.localPosition;

                eventShopInfo.GetComponent<SongpyeonShopItemInfo>().SetPurchaseLimitItemData(eventShopData, this);
            }
        }

        chuseokShopCloneObject.SetActive(false);

        // 재화 갱신
        songpyeonAmountText.text = user_items.GetUserItemAmount(144).ToString("n0");

        // 레이아웃 갱신
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RefreshLayout());
        }
    }

    public void RefreshData()
    {
        WWWForm data = new WWWForm();
        data.AddField("uri", "event");
        data.AddField("eid", (int)neco_event.EVENT_TYPE.CHUSEOK);
        data.AddField("op", 30);

        NetworkManager.GetInstance().SendApiRequest("event", 30, data, (response) =>
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
                            Invoke("SetSongpyeonShopData", 0.1f);
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("Event_Res_1"); break;
                                case 2: msg = LocalizeData.GetText("Event_Res_2"); break;
                                case 11: msg = LocalizeData.GetText("Event_Res_11"); break;
                                case 12: msg = LocalizeData.GetText("Event_Res_12"); break;
                                case 13: msg = LocalizeData.GetText("Event_Res_13"); break;
                                case 14: msg = LocalizeData.GetText("Event_Res_14"); break;
                                case 31: msg = LocalizeData.GetText("Event_Res_31"); break;
                                case 32: msg = LocalizeData.GetText("Event_Res_32"); break;
                                case 41: msg = LocalizeData.GetText("Event_Res_41"); break;
                                case 42: msg = LocalizeData.GetText("Event_Res_42"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_316"), msg);
                        }
                    }
                }
            }
        });
    }

    public void OnShowPurchasePopup(neco_event_thanks_shop eventShopData, int maxBuyCount, SystemConfirmPanel.CountCallback _okCallback = null)
    {
        if (songpeyonPurchaseCountObject == null) { return; }

        songpeyonPurchaseCountObject.SetActive(true);
        songpeyonPurchaseCountObject.GetComponent<SongpyeonShopPurchaseCountPanel>().InitShopPurchaseCountPanel(eventShopData, maxBuyCount, _okCallback);
    }

    public void OnClosePurchasePopup()
    {
        if (songpeyonPurchaseCountObject == null) { return; }

        songpeyonPurchaseCountObject.SetActive(false);
    }

    void ClearData()
    {
        songpeyonPurchaseCountObject.SetActive(false);
    }

    IEnumerator RefreshLayout()
    {
        // 원인 불명.. 2프레임에 걸쳐 최소 2회 갱신해야 정상 작동함

        yield return new WaitForEndOfFrame();

        if (layoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
        }

        yield return new WaitForEndOfFrame();

        if (layoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
        }
    }
}
