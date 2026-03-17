using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardShowcaseItem : MonoBehaviour
{
    public GameObject dimmedLayer;
    public GameObject perfectIconLayer;
    public GameObject redDotIcon;

    public Image cardImage;
    public Image cardGaugeImage;

    List<user_card> userCardList = new List<user_card>();

    card_define curCardData;

    CardListPopup cardCanvas = null;

    public void SetCardShowcaseData(card_define cardDefineData, CardListPopup cardListCanvas, int row, List<user_card> cardList)
    {
        if (cardDefineData == null) { return; }

        userCardList = cardList;

        curCardData = cardDefineData;
        cardCanvas = cardListCanvas;

        cardImage.sprite = curCardData.GetIcon();

        dimmedLayer.SetActive(userCardList.Count <= 0);

        // 퍼펙트 사진 체크
        bool perfectState = (userCardList.Count > 0 && userCardList.Exists(x => x.GetCardType() == user_card.CARD_TYPE.PERFECT));
        perfectIconLayer.SetActive(perfectState);

        // 카드 모음 달성 게이지 세팅
        List<card_define_sub> defineSubList = card_define_sub.GetCardDefineSubList(curCardData.GetCardID());
        int total = defineSubList.Count;
        int current = userCardList.Count;

        cardGaugeImage.fillAmount = (float)current / total;

        // 레드닷 세팅
        UpdateRedDotState();
    }
    
    public void OnClickShowcaseCard()
    {
        if (userCardList.Count <= 0)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("album_showcase_dimmed"));
            return;
        }
        
        cardCanvas.SwitchCurrentPanel(CARD_SUB_PANEL.CARD_LIST, curCardData);
    }

    void UpdateRedDotState()
    {
        if (userCardList != null)
        {
            redDotIcon?.SetActive(false);

            foreach (user_card userCardData in userCardList)
            {
                string newPhotoKey = string.Format("{0}_{1}_{2}", SamandaLauncher.GetAccountNo(), userCardData.GetCardID(), userCardData.GetCardUniqueID());
                if (PlayerPrefs.GetInt(newPhotoKey, 0) == 0)
                {
                    redDotIcon?.SetActive(true);
                    break;
                }
            }
        }
    }
}
