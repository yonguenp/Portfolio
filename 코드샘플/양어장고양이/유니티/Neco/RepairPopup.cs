using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RepairPopup : MonoBehaviour
{
    const int REQ_REPAIR_COUNT = 1; // 수리에 필요한 오브젝트 갯수

    neco_spot curSpotData = null;

    public Image objectImage;
    public Text objectNameText;
    public Text objectCountText;

    public Color originCountColor;
    public Color notEnoughCountColor;

    public GameObject[] RepairDisableUI;
    public GameObject[] RepairEnableUI;


    public void SetSpotData(neco_spot spot)
    {
        curSpotData = spot;

        SetUI();
    }

    public void SetUI()
    {
        //if(curSpotData.GetSpotItemDurability() <= 0)
        if (curSpotData == null)
            return;

        items targetItem = curSpotData.GetCurItem();
        if (targetItem == null)
            return;

        uint curUserItemCount = user_items.GetUserItemAmount(targetItem.GetItemID());
        bool repairEnable = curUserItemCount > 1;//1개는 이미설치됨
        foreach(GameObject ui in RepairDisableUI)
        {
            ui.SetActive(!repairEnable);
        }

        foreach (GameObject ui in RepairEnableUI)
        {
            ui.SetActive(repairEnable);
        }

        // UI 세팅
        objectImage.sprite = targetItem.GetItemIcon();
        objectNameText.text = targetItem.GetItemName();

        uint objectCountResult = curUserItemCount >= 1 ? curUserItemCount - 1 : curUserItemCount;
        objectCountText.text = string.Format("{0}/{1}", objectCountResult.ToString("n0"), REQ_REPAIR_COUNT.ToString("n0"));

        objectCountText.color = objectCountResult >= REQ_REPAIR_COUNT ? originCountColor : notEnoughCountColor;
    }

    public void OnClose()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.REPAIR_POPUP);
    }

    public void OnRepair()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "upgrade");
        data.AddField("op", 1);

        data.AddField("what", curSpotData.GetSpotID().ToString());

        NetworkManager.GetInstance().SendApiRequest("upgrade", 1, data, (response) => {


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
                if (uri == "upgrade")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.REPAIR_POPUP);
                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("수리"), LocalizeData.GetText("수리완료"));
                        }
                        else
                        {
                            string msg = LocalizeData.GetText("LOCALIZE_294");
                            switch (rs)
                            {
                                case 2: msg = LocalizeData.GetText("매력도최대상태"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnToastPopupShow(msg);
                        }
                    }
                }
            }
        });
    }
}
