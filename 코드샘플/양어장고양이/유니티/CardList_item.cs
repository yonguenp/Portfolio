using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardList_item : MonoBehaviour
{
    public Image CardImage;
    public Image NoneImage;
    public GameObject SelectedCursor;
    public GameObject curSelectedCursor;
    public Button Button;

    public user_card userCardData = null;
    public card_define defineCardData = null;
    public card_define_sub defineCardSubData = null;

    public GameObject PerfectIcon;
    public GameObject MovieIcon;
    public GameObject redDotIcon;
    public GameObject DimmedLayer;

    private CardListPopup CardCanvas = null;
    private CardLibraryDetail cardLibaryPanel = null;

    string newPhotoKey = "";
    Coroutine iconLoadCoroutine = null;
    
    public void SetCardListUI(card_define_sub defineSubData, CardListPopup cardCanvas, int row)
    {
        CardCanvas = cardCanvas;
        cardLibaryPanel = CardCanvas.CardSubPanel[(int)CARD_SUB_PANEL.LIBRARY_DETAIL].transform.GetComponent<CardLibraryDetail>();

        defineCardSubData = defineSubData;

        CardImage.gameObject.SetActive(false);

        userCardData = user_card.GetUserCard(defineCardSubData.GetCardID(), defineCardSubData.GetParentCardID());
        defineCardData = card_define.GetCardDefine(defineCardSubData.GetParentCardID());

        if (iconLoadCoroutine != null)
            StopCoroutine(iconLoadCoroutine);

        iconLoadCoroutine = StartCoroutine(defineCardSubData.SetIconSpriteAsync(CardImage, row * 0.1f));

        SelectedCursor.SetActive(false);
        UpdateSelectedState();
        NoneImage.gameObject.SetActive(true);
        CardImage.gameObject.SetActive(CardImage.sprite != null);

        Button.enabled = false;

        SetUserCardListUI();

        PerfectIcon.SetActive(false);
        MovieIcon.SetActive(false);
        //switch (userCard.GetCardType())
        //{
        //    case user_card.CARD_TYPE.PERFECT:
        //        PerfectIcon.SetActive(true);
        //        break;
        //    case user_card.CARD_TYPE.MOVIE:
        //        MovieIcon.SetActive(true);
        //        break;
        //    case user_card.CARD_TYPE.PIECE:
        //    default:
        //        break;
        //}

        PerfectIcon.SetActive(defineCardSubData.GetIsPefectValue() == 1);
        DimmedLayer.SetActive(userCardData == null);

        // 레드닷 세팅
        if (userCardData != null)
        {
            newPhotoKey = string.Format("{0}_{1}_{2}", SamandaLauncher.GetAccountNo(), userCardData.GetCardID(), userCardData.GetCardUniqueID());
            redDotIcon.SetActive(PlayerPrefs.GetInt(newPhotoKey, 0) == 0);
        }
    }

    public void SetUserCardListUI()
    {
        NoneImage.gameObject.SetActive(false);

        //object obj;
        //if (data.data.TryGetValue("today_run_count", out obj))
        //{
        //    isEnable = (uint)obj <= 0;
        //}
        Button.enabled = true;
        Button.onClick.AddListener(OnSelectThisCard);
    }

    public void OnSelectThisCard()
    {
        //SelectedCursor.SetActive(true);
        //CardCanvas.OnCardItemSelect(userCardData);
        //CardCanvas.OnShowCardDetailInfo();

        if (userCardData == null)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("album_showcase_dimmed"));
            return;
        }

        if (CardCanvas.GetTrashModeState())
        {
            CardCanvas.OnCardItemSelect(userCardData);
            return;
        }

        cardLibaryPanel.SetPanelOnData();
        cardLibaryPanel.SetLibraryDetailUI(defineCardData);
        cardLibaryPanel.SetCurosr(userCardData);

        CardCanvas.UpdateCardListSelectState();

        PlayerPrefs.SetInt(newPhotoKey, 1);
        redDotIcon.SetActive(PlayerPrefs.GetInt(newPhotoKey, 0) == 0);
    }

    public void UpdateSelectedState()
    {
        curSelectedCursor.SetActive(cardLibaryPanel ? cardLibaryPanel.GetCursor() == userCardData : false);
    }

    public void ClearData()
    {
        userCardData = null;
        defineCardData = null;
        defineCardSubData = null;

        CardCanvas = null;
        cardLibaryPanel = null;

        iconLoadCoroutine = null;
    }
}
