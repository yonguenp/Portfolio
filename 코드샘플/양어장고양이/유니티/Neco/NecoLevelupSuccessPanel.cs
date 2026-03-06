using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NecoLevelupSuccessPanel : MonoBehaviour
{
    public delegate void Callback();

    [Header("[Levelup Info Layer]")]
    public GameObject levelupInfoContainer;
    public GameObject levelupInfoCloneObject;
    public Text levelupGuideText;
    public RectTransform layoutObject;

    SUPPLY_UI_TYPE curUIType = SUPPLY_UI_TYPE.UNKNOWN;

    Callback closeCallback = null;

    public void OnClickOkButton()
    {
        gameObject.SetActive(false);

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.LEVELUP_POPUP);

        closeCallback?.Invoke();

        MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
        if (mapController != null)
        {
            switch(neco_data.GetPrologueSeq())
            {
                case neco_data.PrologueSeq.조리대레벨업:
                    if (neco_data.Instance.GetCookRecipeLevel() >= 2)
                    {
                        mapController.SendMessage("조리대레벨업완료", SendMessageOptions.DontRequireReceiver);
                    }
                    break;
                case neco_data.PrologueSeq.낚시장난감오브젝트레벨업가이드:
                    if (neco_spot.GetNecoSpot(2).GetSpotLevel() >= 2)
                    {
                        mapController.SendMessage("길막이2레벨낚시장난감연출", SendMessageOptions.DontRequireReceiver);
                        //mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.제작대2레벨, SendMessageOptions.DontRequireReceiver);
                    }
                    break;

                case neco_data.PrologueSeq.캣닢급식소3레벨:
                    if (neco_spot.GetNecoSpotObjectByItemID(147).GetSpotLevel() >= 3)
                    {
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.캣닢급식소3레벨, SendMessageOptions.DontRequireReceiver);
                    }
                    break;
                case neco_data.PrologueSeq.캣닢급식소4레벨:
                    if (neco_spot.GetNecoSpotObjectByItemID(147).GetSpotLevel() >= 4)
                    {
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.캣닢급식소4레벨, SendMessageOptions.DontRequireReceiver);
                    }
                    break;
                case neco_data.PrologueSeq.캣닢급식소5레벨:
                    if (neco_spot.GetNecoSpotObjectByItemID(147).GetSpotLevel() >= 5)
                    {
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.캣닢급식소5레벨, SendMessageOptions.DontRequireReceiver);
                    }
                    break;
                case neco_data.PrologueSeq.화이트캣하우스5레벨:
                    if (neco_spot.GetNecoSpotObjectByItemID(109).GetSpotLevel() >= 5)
                    {
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.화이트캣하우스5레벨, SendMessageOptions.DontRequireReceiver);
                    }
                    break;
                case neco_data.PrologueSeq.화이트캣하우스6레벨:
                    if (neco_spot.GetNecoSpotObjectByItemID(109).GetSpotLevel() >= 6)
                    {
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.화이트캣하우스6레벨, SendMessageOptions.DontRequireReceiver);
                    }
                    break;
                case neco_data.PrologueSeq.화이트캣하우스7레벨:
                    if (neco_spot.GetNecoSpotObjectByItemID(109).GetSpotLevel() >= 7)
                    {
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.화이트캣하우스7레벨, SendMessageOptions.DontRequireReceiver);
                    }
                    break;
                case neco_data.PrologueSeq.화이트캣하우스8레벨:
                    if (neco_spot.GetNecoSpotObjectByItemID(109).GetSpotLevel() >= 8)
                    {
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.화이트캣하우스8레벨, SendMessageOptions.DontRequireReceiver);
                    }
                    break;
                case neco_data.PrologueSeq.화이트캣하우스9레벨:
                    if (neco_spot.GetNecoSpotObjectByItemID(109).GetSpotLevel() >= 9)
                    {
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.화이트캣하우스9레벨, SendMessageOptions.DontRequireReceiver);
                    }
                    break;
                case neco_data.PrologueSeq.화이트캣하우스10레벨:
                    if (neco_spot.GetNecoSpotObjectByItemID(109).GetSpotLevel() >= 10)
                    {
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.화이트캣하우스10레벨, SendMessageOptions.DontRequireReceiver);
                    }
                    break;
                case neco_data.PrologueSeq.플로랄캣하우스3레벨:
                    if (neco_spot.GetNecoSpotObjectByItemID(157).GetSpotLevel() >= 3)
                    {
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.플로랄캣하우스3레벨, SendMessageOptions.DontRequireReceiver);
                    }
                    break;
                case neco_data.PrologueSeq.플로랄캣하우스4레벨:
                    if (neco_spot.GetNecoSpotObjectByItemID(157).GetSpotLevel() >= 4)
                    {
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.플로랄캣하우스4레벨, SendMessageOptions.DontRequireReceiver);
                    }
                    break;
                case neco_data.PrologueSeq.플로랄캣하우스5레벨:
                    if (neco_spot.GetNecoSpotObjectByItemID(157).GetSpotLevel() >= 5)
                    {
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.플로랄캣하우스5레벨, SendMessageOptions.DontRequireReceiver);
                    }
                    break;
                case neco_data.PrologueSeq.알록달록이동식캣하우스3레벨:
                    if (neco_spot.GetNecoSpotObjectByItemID(154).GetSpotLevel() >= 3)
                    {
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.알록달록이동식캣하우스3레벨, SendMessageOptions.DontRequireReceiver);
                    }
                    break;
                case neco_data.PrologueSeq.알록달록이동식캣하우스4레벨:
                    if (neco_spot.GetNecoSpotObjectByItemID(154).GetSpotLevel() >= 4)
                    {
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.알록달록이동식캣하우스4레벨, SendMessageOptions.DontRequireReceiver);
                    }
                    break;
                case neco_data.PrologueSeq.알록달록이동식캣하우스5레벨:
                    if (neco_spot.GetNecoSpotObjectByItemID(154).GetSpotLevel() >= 5)
                    {
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.알록달록이동식캣하우스5레벨, SendMessageOptions.DontRequireReceiver);
                    }
                    break;

                default:
                    switch (neco_data.Instance.GetCookRecipeLevel())
                    {
                        case 3:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.조리대3레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 4:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.조리대4레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 5:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.조리대5레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 6:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.조리대6레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 7:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.조리대7레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 8:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.조리대8레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 9:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.조리대9레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 10:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.조리대10레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                    }
                    break;
            }

            if (curUIType == SUPPLY_UI_TYPE.CAT_GIFT && neco_data.Instance.GetGiftBasketLevel() >= 2)
            {
                if (neco_data.Instance.GetGiftBasketLevel() == 2)
                {
                    mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.바구니2레벨, SendMessageOptions.DontRequireReceiver);
                    mapController.SendMessage("OnNewCat", 4, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    switch (neco_data.Instance.GetGiftBasketLevel())
                    {
                        case 3:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.바구니3레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 4:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.바구니4레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 5:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.바구니5레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 6:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.바구니6레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 7:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.바구니7레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 8:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.바구니8레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 9:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.바구니9레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 10:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.바구니10레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                    }
                }
            }
            else if (curUIType == SUPPLY_UI_TYPE.FISH_FARM && neco_data.Instance.GetFishfarmLevel() >= 2)
            {
                if (neco_data.Instance.GetFishfarmLevel() == 2)
                {
                    mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.양어장2레벨, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    switch (neco_data.Instance.GetFishfarmLevel())
                    {
                        case 3:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.양어장3레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 4:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.양어장4레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 5:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.양어장5레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 6:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.양어장6레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 7:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.양어장7레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 8:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.양어장8레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 9:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.양어장9레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 10:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.양어장10레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                    }
                }
            }
            else if (curUIType == SUPPLY_UI_TYPE.FISH_TRAP && neco_data.Instance.GetFishtrapLevel() >= 2)
            {
                if (neco_data.Instance.GetFishtrapLevel() == 3)
                {
                    mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.통발3레벨, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    switch (neco_data.Instance.GetFishtrapLevel())
                    {
                        case 4:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.통발4레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 5:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.통발5레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 6:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.통발6레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 7:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.통발7레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 8:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.통발8레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 9:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.통발9레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 10:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.통발10레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                    }
                }
            }
            //else if (neco_data.Instance.GetCookRecipeLevel() > 2)
            //{
            //    if (neco_data.Instance.GetCookRecipeLevel() == 3)
            //    {
            //        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.제작대3레벨, SendMessageOptions.DontRequireReceiver);
            //    }
            //    else if (neco_data.Instance.GetCookRecipeLevel() == 4)
            //    {
            //        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.제작대4레벨, SendMessageOptions.DontRequireReceiver);
            //    }
            //}
            else if (curUIType == SUPPLY_UI_TYPE.WORKBENCH && neco_data.Instance.GetCraftRecipeLevel() >= 2)
            {
                if (neco_data.Instance.GetCraftRecipeLevel() == 2)
                {
                    mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.제작대2레벨, SendMessageOptions.DontRequireReceiver);
                    mapController.SendMessage("OnNewCat", 1, SendMessageOptions.DontRequireReceiver);
                }
                else if (neco_data.Instance.GetCraftRecipeLevel() == 3)
                {
                    mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.제작대3레벨, SendMessageOptions.DontRequireReceiver);
                    mapController.SendMessage("OnNewCat", 7, SendMessageOptions.DontRequireReceiver);
                }
                else if (neco_data.Instance.GetCraftRecipeLevel() == 4)
                {
                    mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.제작대4레벨, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    switch(neco_data.Instance.GetCraftRecipeLevel())
                    {
                        case 5:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.제작대5레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 6:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.제작대6레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 7:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.제작대7레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 8:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.제작대8레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 9:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.제작대9레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 10:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.제작대10레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 11:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.제작대11레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                        case 12:
                            mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.제작대12레벨, SendMessageOptions.DontRequireReceiver);
                            break;
                    }
                }
            }
        }
    }

    public void SetLevelupSuccessPopupMsg(SUPPLY_UI_TYPE uiType, List<LevelupTextData> textList, bool isFixed = false, Callback _closeCallback = null)
    {
        if (levelupInfoContainer == null || levelupInfoCloneObject == null) { return; }
        if (textList == null || textList.Count <= 0) { return; }

        curUIType = uiType;
        closeCallback = _closeCallback;

        levelupGuideText.text = isFixed ? LocalizeData.GetText("수리완료") : LocalizeData.GetText("LOCALIZE_94");

        foreach (Transform child in levelupInfoContainer.transform)
        {
            if (child.gameObject != levelupInfoCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        levelupInfoCloneObject.SetActive(true);

        foreach (LevelupTextData textData in textList)
        {
            if (textData.textStyle == LevelupTextData.LEVELUP_TEXT_STYLE.CUSTOM_STYLE) { continue; }

            GameObject InfoText = Instantiate(levelupInfoCloneObject);
            InfoText.transform.SetParent(levelupInfoContainer.transform);
            InfoText.transform.localScale = levelupInfoCloneObject.transform.localScale;
            InfoText.transform.localPosition = levelupInfoCloneObject.transform.localPosition;

            InfoText.GetComponent<LevelupTextInfo>().SetLevelTextData(textData);
        }

        levelupInfoCloneObject.SetActive(false);

        StartCoroutine(RefreshLayout());
    }

    IEnumerator RefreshLayout()
    {
        // 원인 불명.. 2프레임에 걸쳐 최소 2회 갱신해야 정상 작동함

        yield return new WaitForEndOfFrame();

        if (layoutObject != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutObject);
        }

        yield return new WaitForEndOfFrame();

        if (layoutObject != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutObject);
        }
    }
}
