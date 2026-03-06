using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NecoCatDetailPanel : MonoBehaviour
{
    public CatDetailInfo catDetailInfo;

    [Header("[Move Button]")]
    public Image leftMoveButtonImage;
    public Image rightMoveButtonImage;
    public Color buttonEnableColor;
    public Color buttonDisableColor;

    [Header("[Layout List]")]
    public RectTransform[] layoutRectList;

    public GameObject gaugeObject;

    neco_cat curSelectedCatData = null;
    List<neco_cat> catDataList = new List<neco_cat>();
    int selectedListIndex = 0;
    bool returnCatList = true;

    private void OnEnable()
    {
        NecoCanvas.GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().resourceInfoPanel.SetTopUIMode(TopUIResourceInfoPanel.UI_MODE.CATSTICK);
    }
    private void OnDisable()
    {
        catDataList.Clear();
        selectedListIndex = 0;

        leftMoveButtonImage.color = buttonEnableColor;
        rightMoveButtonImage.color = buttonEnableColor;

        NecoCanvas.GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().resourceInfoPanel.SetTopUIMode(TopUIResourceInfoPanel.UI_MODE.NORMAL);

        NecoCanvas.GetUICanvas()?.UpdateCatListAlarm();
        NecoCanvas.GetUICanvas()?.UpdateMainMenuRedDot();
    }

    public void SetCatDetailInfo(neco_cat catData, List<neco_cat> dataList, bool calledCatList = true)
    {
        returnCatList = calledCatList;

        curSelectedCatData = catData;
        catDataList = new List<neco_cat>(dataList);
        selectedListIndex = catDataList.IndexOf(curSelectedCatData);

        catDetailInfo.SetCatDetailInfoData(curSelectedCatData);

        ButtonChecker();

        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CAT_DETAIL_POPUP);

        RebuildLayout();

        SetGuage();
    }

    void SetGuage()
    {
        Transform Max = gaugeObject.transform.Find("Max");
        if (Max != null)
        {
            Max.gameObject.SetActive(false);
        }

        if (curSelectedCatData.GetCatState() >= 3)
        {
            gaugeObject.SetActive(true);
            Transform GaugeFilled = gaugeObject.transform.Find("GaugeFilled");
            if (GaugeFilled != null)
            {
                int total = neco_cat_memory.GetNecoMemoryCount(curSelectedCatData.GetCatID());
                int cur = (int)curSelectedCatData.GetMemoryCount();
                float rate = (float)cur / total;

                GaugeFilled.GetComponent<Image>().fillAmount = rate;

                if (Max != null)
                {
                    Max.gameObject.SetActive(cur == total);
                }
            }
        }
        else
        {
            gaugeObject.SetActive(false);
        }
    }

    public void OnClickCloseButton()
    {
        // 이전 캣리스트 UI로 돌아감
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_DETAIL_POPUP);
        if(returnCatList && neco_data.PrologueSeq.사진찍기돌발대사 != neco_data.GetPrologueSeq())
            NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CAT_LIST_POPUP);

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.사진찍기돌발대사)
        {
            MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
            if (mapController != null)
                mapController.SendMessage("사진찍기완료", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void OnClickBackPaenlCloseButton()
    {
        // 고양이 첫등장 프롤로그 예외처리
        if (neco_data.GetPrologueSeq() <= neco_data.PrologueSeq.길막이획득프로필팝업확인)
        {
            return;
        }

        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.사진찍기돌발대사)
        {
            return;
        }

        // 이전 캣리스트 UI로 돌아감
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_DETAIL_POPUP);
        if (returnCatList)
            NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CAT_LIST_POPUP);
    }


    public void OnClickMoveLeftButton()
    {
        if (selectedListIndex <= 0) { return; }

        selectedListIndex--;

        ButtonChecker();

        curSelectedCatData = catDataList[selectedListIndex];

        catDetailInfo.SetCatDetailInfoData(curSelectedCatData);

        rightMoveButtonImage.color = buttonEnableColor;

        SetGuage();
    }

    public void OnClickMoveRightButton()
    {
        if (selectedListIndex >= catDataList.Count - 1) { return; }

        selectedListIndex++;

        ButtonChecker();

        curSelectedCatData = catDataList[selectedListIndex];

        catDetailInfo.SetCatDetailInfoData(curSelectedCatData);

        leftMoveButtonImage.color = buttonEnableColor;

        SetGuage();
    }

    void ButtonChecker()
    {
        if (catDataList.Count == 1)
        {
            leftMoveButtonImage.color = buttonDisableColor;
            rightMoveButtonImage.color = buttonDisableColor;
        }
        else if (selectedListIndex <= 0)
        {
            leftMoveButtonImage.color = buttonDisableColor;

            selectedListIndex = 0;
        }
        else if (selectedListIndex >= catDataList.Count - 1)
        {
            rightMoveButtonImage.color = buttonDisableColor;

            selectedListIndex = catDataList.Count - 1;
        }
    }

    void RebuildLayout()
    {
        if (layoutRectList != null && layoutRectList.Length > 0)
        {
            foreach(RectTransform rect in layoutRectList)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }
        }
    }
}
