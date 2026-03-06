using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubscribeRecivePopup : MonoBehaviour
{
    [Serializable]
    public class SubcribeRewardData
    {
        public Image rewardIcon;
        public Text rewardCount;
        public Text rewardName;
    }
    public SubcribeRewardData[] rewardObjects;

    public Text curDayTitle;
    Action callback;

    neco_subscribe_data curSubscirbeData;

    public void SetDay(neco_subscribe_data subcribeData, Action cb)
    {
        curSubscirbeData = subcribeData;

        curDayTitle.text = string.Format(LocalizeData.GetText("N일차보상"), curSubscirbeData.cur_day);
        callback = cb;

        List<neco_subscribe> subList = neco_subscribe.GetNecoSubscribeListByID(curSubscirbeData.prod_id);
       
        if ((rewardObjects.Length > 0 && subList.Count > 0) && subList.Count == rewardObjects.Length)
        {
            for (int i = 0; i < rewardObjects.Length; ++i)
            {
                switch (subList[i].GetNecoSubscribeItemType())
                {
                    case "gold":
                        rewardObjects[i].rewardIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");
                        rewardObjects[i].rewardCount.text = subList[i].GetNecoSubcribeItemCount().ToString("n0");
                        rewardObjects[i].rewardName.text = LocalizeData.GetText("LOCALIZE_229");
                        break;
                    case "dia":
                        rewardObjects[i].rewardIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_catleaf");
                        rewardObjects[i].rewardCount.text = subList[i].GetNecoSubcribeItemCount().ToString("n0");
                        rewardObjects[i].rewardName.text = LocalizeData.GetText("LOCALIZE_348");
                        break;
                    case "item":
                        items itemData = items.GetItem(subList[i].GetNecoSubcribeItemID());
                        rewardObjects[i].rewardIcon.sprite = itemData.GetItemIcon();
                        rewardObjects[i].rewardCount.text = subList[i].GetNecoSubcribeItemCount().ToString("n0");
                        rewardObjects[i].rewardName.text = itemData.GetItemName();
                        break;
                }
            }
        }
    }

    public void OnClose()
    {
        callback?.Invoke();
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.SUBSCRIBE_POPUP);
    }

    public void OnClickCloseButton()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.SUBSCRIBE_POPUP);
    }
}
