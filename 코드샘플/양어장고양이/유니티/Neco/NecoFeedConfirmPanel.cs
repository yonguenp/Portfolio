using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NecoFeedConfirmPanel : MonoBehaviour
{
    public Image foodIcon;
    public Text foodNameText;
    public Text foodDescText;
    public Text foodDurationText;

    [Header("[Button Layers]")]
    public GameObject okButtonObject;

    neco_spot curSpotData = null;
    FoodData curFoodData = null;

    private void OnEnable()
    {
        
        GetComponent<AudioSource>().enabled = PlayerPrefs.GetInt("Setting_SFX", 1) == 1;
    }

    public void OnClickFeedConfirmButton()
    {
        if (curSpotData == null || curFoodData == null) { return; }

        neco_map map = neco_map.GetNecoMap(NecoCanvas.GetGameCanvas().curMapID);
        if(map != null)
        {
            neco_spot foodspot = map.GetFoodSpot();
            if (foodspot != null)
            {
                if(foodspot.GetCurItem() != null && foodspot.GetItemRemain() > 0)
                {
                    ConfirmPopupData param = new ConfirmPopupData();

                    param.titleText = LocalizeData.GetText("LOCALIZE_250");
                    param.titleMessageText = LocalizeData.GetText("LOCALIZE_251");

                    
                    param.messageText_1 = LocalizeData.GetText("LOCALIZE_252") + foodspot.GetCurItem().GetItemName() + LocalizeData.GetText("LOCALIZE_253");

                    NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(param, CONFIRM_POPUP_TYPE.COMMON, OnFeedItem);
                    return;
                }
                else
                {
                    OnFeedItem();
                }
            }
        }
    }

    public void OnFeedItem()
    {
        NecoGameCanvas.GetGameCanvas().OnSetFood(curSpotData, curFoodData.itemData);
        NecoCanvas.GetPopupCanvas().OnPopupClose();

        if (CheckPrologue())
        {
            MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
            if (mapController != null)
            {
                mapController.SendMessage("첫밥주기완료", SendMessageOptions.DontRequireReceiver);
            }
        }
        if (neco_data.GetPrologueSeq() >= neco_data.PrologueSeq.배스구이완료후밥그릇강조 && curFoodData.itemData.GetItemID() == 80)
        {
            MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
            if (mapController != null)
            {
                mapController.SendMessage("배스구이밥그릇에담음", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public void OnClickCloseButton()
    {
        // 프롤로그 체크
        if (CheckPrologue())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_254"));
            return;
        }

        if (CheckPrologueWithToastAlarm()) { return; }

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_FEED_CONFIRM_POPUP);
    }

    public void SetFeedConfirmDataUI(FoodData data, neco_spot spotData)
    {
        curFoodData = data;
        curSpotData = spotData;
        if (curFoodData == null) { return; }

        foodIcon.sprite = curFoodData.foodIcon;
        foodNameText.text = curFoodData.foodName;
        foodDescText.text = curFoodData.foodDesc;
        foodDurationText.text = curFoodData.foodDuration;

        okButtonObject.GetComponent<RectTransform>().localScale = Vector3.one;

        // 프롤로그 체크
        if (CheckPrologue() && curFoodData.itemData.GetItemID() == 78)
        {
            okButtonObject.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배스구이완료후밥그릇강조 && curFoodData.itemData.GetItemID() == 80)
        {
            okButtonObject.GetComponent<RectTransform>().DOScale(1.1f, 0.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }
    }

    bool CheckPrologue()
    {
        return neco_data.GetPrologueSeq() <= neco_data.PrologueSeq.첫밥그릇등장;
    }

    void ClearTween()
    {
        okButtonObject.GetComponent<RectTransform>().DORewind();
        okButtonObject.GetComponent<RectTransform>().DOKill();
    }

    bool CheckPrologueWithToastAlarm()
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();

        switch (seq)
        {
            case neco_data.PrologueSeq.배스구이완료후밥그릇강조:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_400"));
                return true;
            case neco_data.PrologueSeq.배틀패스강조및대사:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("패틀패스강조"));
                return true;
        }

        return false;
    }
}
