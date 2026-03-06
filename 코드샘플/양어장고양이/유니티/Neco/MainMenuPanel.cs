using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [Serializable]
    // 트윈 애니메이션에 사용되는 Objects
    public class MainMenuIconObject
    {
        public GameObject IconObject;
        public Image IconBgImage;
        public Image IconImage;
    }

    const float TOPMENU_TWEEN_TIME = 0.5f;
    const float MAINMENU_TWEEN_TIME = 0.6f;

    [Header("MainMenuToggle")]
    public Toggle mainMenuToggle;
    public GameObject ToggleOnObject;
    public GameObject ToggleOffObject;
    public MainMenuGroup mainMenuGroup;
    public GameObject TopUIPanel;

    public MainMenuCatGiftButton giftButton;
    public MainMenuFishFarmButton farmButton;
    public MainMenuFishTrapButton trapButton;
    public Button passButton;
    public Button shopButton;
    public Button fishingButton;

    public GameObject NotifyEffectObject;

    [Header("TopMenuToggle")]
    public Toggle topMenuToggle;
    public GameObject topToggleOnObject;
    public GameObject topToggleOffObject;
    public MainMenuGroup topMenuGroup;

    [Header("TopUITweenImages")]
    public MainMenuIconObject[] topMenuObjects;
    Vector2[] topMenuOriginPos;

    [Header("MainMenuUITweenImages")]
    public Image menuIconBG;
    public MainMenuIconObject[] mainmenuObjects;
    Vector2[] mainMenuOriginPos;

    [Header("Common")]
    public GameObject MidUIPanel;

    public GameObject[] ScaleContorlObjects;
    public GameObject[] ScaleReverseControlObjects;
    public GameObject ScaleYContorlObject;
    private void Awake()
    {
        // 메인 메뉴 관련
        if (mainmenuObjects != null && mainmenuObjects.Length > 0)
        {
            mainMenuOriginPos = new Vector2[mainmenuObjects.Length];

            for (int i = 0; i < mainmenuObjects.Length; ++i)
            {
                mainMenuOriginPos[i] = mainmenuObjects[i].IconObject.GetComponent<RectTransform>().anchoredPosition;
            }
        }

        // 탑 메뉴 관련
        if (topMenuObjects != null && topMenuObjects.Length > 0)
        {
#if UNITY_IOS
            //const int AchivementIconIndex = 2;
            int AchivementIconIndex = Array.FindIndex(topMenuObjects, x => x.IconObject.name == "Achieve_Icon");
            topMenuObjects[AchivementIconIndex].IconObject.SetActive(false);

            for (int i = 0; i < topMenuObjects.Length; ++i)
            {
                if (i > AchivementIconIndex)
                {
                    Vector2 originPos = topMenuObjects[i].IconObject.GetComponent<RectTransform>().anchoredPosition;
                    topMenuObjects[i].IconObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(originPos.x, originPos.y += 52);
                }
            }
#endif

            topMenuOriginPos = new Vector2[topMenuObjects.Length];

            for (int i = 0; i < topMenuObjects.Length; ++i)
            {
                topMenuOriginPos[i] = topMenuObjects[i].IconObject.GetComponent<RectTransform>().anchoredPosition;
            }
        }
    }

    private void OnEnable()
    {
        mainMenuToggle.isOn = false;
        topMenuToggle.isOn = false;

        UpdateButtonState();

        UpdatePrologueNotifyEffect();

        neco_data.Instance.SetGiftStatusUI(this);
        RefreshGoldButton();
        RefreshFishButton();
    }

    private void OnDestroy()
    {
        neco_data.Instance.SetGiftStatusUI(null);
    }

    public void OnClickMainMenuButton()
    {
        if (mainMenuToggle == null)
        {
            Debug.LogError("mainMenuToggle is null");
            return;
        }

        // 프롤로그 예외처리
        if (CheckPrologueWithToastAlarm()) { return; }

        // tween 초기화
        ResetMainMenuTween();

        // 애니메이션 적용
        //SetTopUITween();
        SetMainMenuUITween();

        //MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
        //if (mapController != null)
        //{
        //    mapController.SendMessage("OnCookButtonOpen", SendMessageOptions.DontRequireReceiver);
        //}

        //if (mapController != null)
        //{
        //    mapController.SendMessage("OnCraftingButtonOpenIcon", SendMessageOptions.DontRequireReceiver);
        //}

        UpdatePrologueNotifyEffect();
    }

    public void OnClickTopMenuButton()
    {
        if (topMenuToggle == null)
        {
            Debug.LogError("topMenuToggle is null");
            return;
        }

        // tween 초기화
        ResetTopMenuTween();

        // 애니메이션 적용
        SetTopUITween();

        //UpdatePrologueNotifyEffect();
    }

    public void RefreshGiftButton()
    {
        giftButton.Refresh();
    }

    public void RefreshGoldButton()
    {
        farmButton.Refresh();
    }

    public void RefreshFishButton()
    {
        trapButton.Refresh();
    }

    public void RefreshBySeq()
    {
        //neco_data.ClientDEBUG_Seq seq = neco_data.GetDebugSeq();
        //trapButton.gameObject.SetActive(neco_data.ClientDEBUG_Seq.FISH_TRAP_BUTTON_OPEN >= seq);

        //if (neco_data.ClientDEBUG_Seq.FISH_TRAP_BUTTON_OPEN == seq)
        //{
        //    trapButton.transform.DOScale(1.3f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        //}
        //else
        //{
        //    trapButton.transform.DOKill();
        //    trapButton.transform.localScale = Vector3.one;
        //}

        //if (neco_data.ClientDEBUG_Seq.COOK_BUTTON_OPEN == seq || neco_data.ClientDEBUG_Seq.CRAFT_BUTTON_OPEN == seq)
        //{
        //    ToggleOffObject.transform.DOScale(1.3f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        //}
        //else
        //{
        //    ToggleOffObject.transform.DOKill();
        //    ToggleOffObject.transform.localScale = Vector3.one;
        //}
    }

    public void UpdatePrologueNotifyEffect()
    {
        if (NotifyEffectObject == null) { return; }

        NotifyEffectObject.SetActive(CheckPrologue());
    }

    void UpdateButtonState()
    {
        // 메인 메뉴 관련
        ToggleOnObject.SetActive(mainMenuToggle.isOn);
        ToggleOffObject.SetActive(!mainMenuToggle.isOn);

        mainMenuGroup.SetActive(mainMenuToggle.isOn);

        // 탑 메뉴 관련
        topToggleOnObject.SetActive(topMenuToggle.isOn);
        topToggleOffObject.SetActive(!topMenuToggle.isOn);

        topMenuGroup.SetActive(topMenuToggle.isOn);
    }

    void UpdateButtonInteractable()
    {
        mainMenuGroup.ToggleButtonInteractable(mainMenuToggle.isOn);
        topMenuGroup.ToggleButtonInteractable(topMenuToggle.isOn);
    }

    void ResetMainMenuTween()
    {
        for (int i = 0; i < mainmenuObjects.Length; ++i)
        {
            menuIconBG.DOKill();
            menuIconBG.DORewind();

            mainmenuObjects[i].IconObject.GetComponent<RectTransform>().DOKill();
            mainmenuObjects[i].IconObject.GetComponent<RectTransform>().DORewind();

            mainmenuObjects[i].IconBgImage.DOKill();
            mainmenuObjects[i].IconBgImage.DORewind();

            mainmenuObjects[i].IconImage.DOKill();
            mainmenuObjects[i].IconImage.DORewind();

            mainmenuObjects[i].IconObject.GetComponent<RectTransform>().anchoredPosition = mainMenuOriginPos[i];
        }
    }

    void ResetTopMenuTween()
    {
        for (int i = 0; i < topMenuObjects.Length; ++i)
        {
            topMenuObjects[i].IconObject.GetComponent<RectTransform>().DOKill();
            topMenuObjects[i].IconObject.GetComponent<RectTransform>().DORewind();

            //topMenuObjects[i].IconBgImage.DOKill();
            //topMenuObjects[i].IconBgImage.DORewind();

            topMenuObjects[i].IconImage.DOKill();
            topMenuObjects[i].IconImage.DORewind();

            topMenuObjects[i].IconObject.GetComponent<RectTransform>().anchoredPosition = topMenuOriginPos[i];
        }
    }

    void SetTopUITween()
    {
        if (topMenuToggle.isOn)
        {
            UpdateButtonState();

            Sequence topMenuOpenAnim = DOTween.Sequence();

            float delay = TOPMENU_TWEEN_TIME / topMenuObjects.Length;
            float totalDelay = delay;
            for (int i = 0; i < topMenuObjects.Length; ++i)
            {
                if (topMenuObjects[i].IconObject.activeSelf)
                {
                    topMenuOpenAnim.Join(topMenuObjects[i].IconObject.GetComponent<RectTransform>().DOAnchorPosY(-45, totalDelay).SetRelative().SetEase(Ease.InOutSine));
                    //topMenuOpenAnim.Join(mainmenuObjects[i].IconBgImage.DOFade(0.745f, totalDelay));
                    topMenuOpenAnim.Join(topMenuObjects[i].IconImage.DOFade(1.0f, delay));

                    totalDelay += delay;
                }
            }

            topMenuOpenAnim.OnComplete(UpdateButtonInteractable);

            topMenuOpenAnim.Restart();
        }
        else
        {
            UpdateButtonInteractable();

            Sequence topMenuUICloseAnim = DOTween.Sequence();

            float delay = (TOPMENU_TWEEN_TIME - 0.4f) / topMenuObjects.Length;
            float totalDelay = delay;
            for (int i = 0; i < topMenuObjects.Length; ++i)
            {
                if (topMenuObjects[i].IconObject.activeSelf)
                {
                    topMenuUICloseAnim.Join(topMenuObjects[i].IconObject.GetComponent<RectTransform>().DOAnchorPosY(45, totalDelay)/*.SetRelative()*/.SetEase(Ease.InOutSine));
                    //topMenuUICloseAnim.Join(mainmenuObjects[i].IconBgImage.DOFade(0, totalDelay));
                    topMenuUICloseAnim.Join(topMenuObjects[i].IconImage.DOFade(0, totalDelay));

                    totalDelay += delay;
                }
            }

            topMenuUICloseAnim.OnComplete(UpdateButtonState);

            topMenuUICloseAnim.Restart();
        }
    }

    void SetMainMenuUITween()
    {
        if (mainMenuToggle.isOn)
        {
            UpdateButtonState();

            Sequence MainmenuOpenAnim = DOTween.Sequence();
            
            float delay = MAINMENU_TWEEN_TIME / mainmenuObjects.Length;
            float totalDelay = delay;
            for (int i = 0; i < mainmenuObjects.Length; ++i)
            {
                MainmenuOpenAnim.Join(mainmenuObjects[i].IconObject.GetComponent<RectTransform>().DOAnchorPosY(45, totalDelay).SetRelative().SetEase(Ease.InOutSine));
                MainmenuOpenAnim.Join(mainmenuObjects[i].IconBgImage.DOFade(0.745f, totalDelay));
                MainmenuOpenAnim.Join(mainmenuObjects[i].IconImage.DOFade(1.0f, totalDelay));
                MainmenuOpenAnim.Join(menuIconBG.DOFade(1.0f, MAINMENU_TWEEN_TIME));

                totalDelay += delay;
            }

            MainmenuOpenAnim.OnComplete(UpdateButtonInteractable);

            MainmenuOpenAnim.Restart();
        }
        else
        {
            UpdateButtonInteractable();

            Sequence MainmenuUICloseAnim = DOTween.Sequence();

            float delay = (MAINMENU_TWEEN_TIME - 0.2f) / mainmenuObjects.Length;
            float totalDelay = delay;
            for (int i = 0; i < mainmenuObjects.Length; ++i)
            {
                MainmenuUICloseAnim.Join(mainmenuObjects[i].IconObject.GetComponent<RectTransform>().DOAnchorPosY(-45, totalDelay).SetRelative().SetEase(Ease.InOutSine));
                MainmenuUICloseAnim.Join(mainmenuObjects[i].IconBgImage.DOFade(0, totalDelay));
                MainmenuUICloseAnim.Join(mainmenuObjects[i].IconImage.DOFade(0, totalDelay));
                MainmenuUICloseAnim.Join(menuIconBG.DOFade(0, MAINMENU_TWEEN_TIME - 0.2f));

                totalDelay += delay;
            }

            MainmenuUICloseAnim.OnComplete(UpdateButtonState);

            MainmenuUICloseAnim.Restart();
        }
    }

    bool CheckPrologue()
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();
        bool check = seq == neco_data.PrologueSeq.조리대UI등장 ||
            seq == neco_data.PrologueSeq.조리대레벨업 ||
            seq == neco_data.PrologueSeq.낚시장난감만들기 ||
            seq == neco_data.PrologueSeq.낚시장난감오브젝트레벨업가이드 ||
            seq == neco_data.PrologueSeq.철판제작가이드퀘스트 ||
            seq == neco_data.PrologueSeq.배스구이강조;

        //bool check = (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.통발회수 ||
        //                neco_data.GetPrologueSeq() == neco_data.PrologueSeq.조리대등장_메뉴버튼 ||
        //                neco_data.GetPrologueSeq() == neco_data.PrologueSeq.조리대등장_세부버튼강조부터제작까지 ||
        //                neco_data.GetPrologueSeq() == neco_data.PrologueSeq.보은바구니대사 ||
        //                neco_data.GetPrologueSeq() == neco_data.PrologueSeq.제작대등장_메뉴버튼 ||
        //                neco_data.GetPrologueSeq() == neco_data.PrologueSeq.제작대등장_세부버튼강조부터제작까지 ||
        //                neco_data.GetPrologueSeq() == neco_data.PrologueSeq.전갱이구이제작);

        return check;
    }

    bool CheckPrologueWithToastAlarm()
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();

        switch (seq)
        {
            case neco_data.PrologueSeq.통발UI등장:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_274"));
                return true;
            case neco_data.PrologueSeq.첫밥그릇등장:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_254"));
                return true;
            case neco_data.PrologueSeq.고양이10번터치가이드퀘스트:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_214"));
                return true;
            case neco_data.PrologueSeq.양어장UI등장:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_262"));
                return true;
            case neco_data.PrologueSeq.상점배스구매가이드퀘스트:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ19"));
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
            case neco_data.PrologueSeq.길막이낚시장난감배치:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("tutorial_block"));
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

    public void ResetBackgroundSize(float ratio)
    {
        Vector3 scale = Vector3.one * ratio;
        foreach (GameObject obj in ScaleContorlObjects)
        {
            obj.transform.localScale = scale;
        }

        scale = Vector3.one * (1.0f / ratio);
        foreach (GameObject obj in ScaleReverseControlObjects)
        {
            obj.transform.localScale = scale;
        }
    }

    //public void OnCardShop()
    //{
    //    NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CARD_SHOP_POPUP);
    //}
    public void OnCardList()
    {
        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CARD_LIST_POPUP);
        NecoCanvas.GetPopupCanvas().OnTopUIInfoLayer(TOP_UI_PANEL_TYPE.GUIDE_QUEST);
    }

    public void OnPhotoStudioButton()
    {
        if (!NecoCanvas.GetPopupCanvas().IsPopupOpen(NecoPopupCanvas.POPUP_TYPE.SHOP_LIST_POPUP))
        {
            NecoCanvas.GetPopupCanvas().OnPopupClose();
            NecoCanvas.GetPopupCanvas().OnShopListPopupShow(NecoShopPanel.SHOP_CATEGORY.CARD);
        }
    }
}
