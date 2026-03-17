using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

public enum CARD_SUB_PANEL
{
    CARD_SHOWCASE_LIST,
    CARD_LIST,
    COLLECTION_LIST,
    CARD_DETAIL,
    LIBRARY_LIST,
    LIBRARY_DETAIL,
};

public class CardListPopup : MonoBehaviour
{
    [Header("[CardShowcase Layers]")]
    public GameObject CardShowcaseContainer;
    public GameObject CardShowcaseItemPrefab;

    [Header("[CardList Slider]")]
    public Slider cardListSlider;
    public Text cardListSliderText;

    [Header("[CardList Layers]")]
    public GameObject CardScrollViewContainer;
    public GameObject CardItemPrefab;
    public GameObject PerfectCardItemPrefab;

    public GameObject CollectionScrollViewContainer;
    public GameObject CollectionItemPrefab;

    public GameObject emptyAlbumLayer;
    public GameObject emptyListLayer;

    public Image CardImage;
    public Text CardName;
    public Text CardDesc;
    public Text CardLevel;
    public Text CardExp;
    public Text CardCount;

    public Text PictureCount;
    public Text MovieCount;
    public Text StorageInfo;
    public Image StorageGauge;

    public CardDetailPanel CardDetailPopup;

    public RewardListUI RewardListUI;

    public GameObject[] CardSubPanel;
    public GameObject[] TrashCan;

    public Button MoreSpaceButton;

    CARD_SUB_PANEL curState = CARD_SUB_PANEL.CARD_LIST;
    CARD_SUB_PANEL preState = CARD_SUB_PANEL.CARD_LIST;
    card_define selectShowcaseCard = null;
    user_card selectCard = null;
    bool TrashMode;

    Coroutine cardListCoroutine = null;
    Coroutine cardShowcaseListCoroutine = null;

    public void OnClickEmptyAlbumLayer()
    {
        if (!NecoCanvas.GetPopupCanvas().IsPopupOpen(NecoPopupCanvas.POPUP_TYPE.SHOP_LIST_POPUP))
        {
            ConfirmPopupData popupData = new ConfirmPopupData();

            popupData.titleText = LocalizeData.GetText("LOCALIZE_478");
            popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_479");

            popupData.messageText_1 = LocalizeData.GetText("LOCALIZE_480");

            NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, () => {
                NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CARD_LIST_POPUP);
                NecoCanvas.GetPopupCanvas().OnShopListPopupShow(NecoShopPanel.SHOP_CATEGORY.CARD);
            });
        }
    }

    public void OnCompleteTweenAnimation()
    {
        //ManageMenuTab.SetActive(false);
        gameObject.SetActive(false);
    }

    public void OnCompleteTweenAnimationSubPanel()
    {
        for(int i = 0; i < CardSubPanel.Length; i++)
        {
            CardSubPanel[i].SetActive(i == (int)curState);
        }

        switch (curState)
        {
            case CARD_SUB_PANEL.CARD_SHOWCASE_LIST:
                OnLoadCardShowcaseListUI();
                break;
            case CARD_SUB_PANEL.CARD_LIST:
                OnLoadCardListUI();
                break;
            case CARD_SUB_PANEL.COLLECTION_LIST:
                OnCollectionList();
                break;
        }
    }

    public void OnEnable()
    {
        if (NecoCanvas.GetPopupCanvas().IsPopupOpen(NecoPopupCanvas.POPUP_TYPE.SHOP_LIST_POPUP))
        {
            bool isGachaPlaying = false;
            NecoShopPanel panel = NecoCanvas.GetPopupCanvas().PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.SHOP_LIST_POPUP].GetComponent<NecoShopPanel>();
            if (panel != null)
            {
                if (panel.cardShopPanel.ResultPanel.activeSelf)
                {
                    isGachaPlaying = true;
                }
            }

            if(isGachaPlaying)
            {
                curState = CARD_SUB_PANEL.CARD_DETAIL;
            }
            else
            {
                //curState = CARD_SUB_PANEL.CARD_LIST;
                curState = CARD_SUB_PANEL.CARD_SHOWCASE_LIST;
            }
        }
        else
        {
            //curState = CARD_SUB_PANEL.CARD_LIST;
            curState = CARD_SUB_PANEL.CARD_SHOWCASE_LIST;

        }

        OnCloseDetailInfo();

        GameObject target = CardSubPanel[(int)curState];
        target.SetActive(true);

        OnCompleteTweenAnimationSubPanel();

        CardShowcaseContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
    }

    public void OnDisable()
    {
        selectCard = null;

        CardSubPanel[(int)CARD_SUB_PANEL.LIBRARY_DETAIL].GetComponent<CardLibraryDetail>().SetPanelOffData();
    }

    void ClearCardShowcaseListUI()
    {
        foreach (Transform child in CardShowcaseContainer.transform)
        {
            Destroy(child.gameObject);
        }

        CardShowcaseContainer.transform.DetachChildren();
    }

    void ClearCardListUI()
    {
        foreach (Transform child in CardScrollViewContainer.transform)
        {
            Destroy(child.gameObject);
        }

        CardScrollViewContainer.transform.DetachChildren();

        CardScrollViewContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        PerfectCardItemPrefab.GetComponent<CardList_item>().ClearData();
    }

    public void UpdateCardListSelectState ()
    {
        foreach (Transform child in CardScrollViewContainer.transform)
        {
            child.GetComponent<CardList_item>().UpdateSelectedState();
        }

        PerfectCardItemPrefab.GetComponent<CardList_item>().UpdateSelectedState();
    }

    public void RefreshRedDot()
    {
        foreach (CardList_item cardItem in CardScrollViewContainer.transform.GetComponentsInChildren<CardList_item>())
        {
            if (cardItem != null && cardItem.userCardData != null)
            {
                string newPhotoKey = string.Format("{0}_{1}_{2}", SamandaLauncher.GetAccountNo(), cardItem.userCardData.GetCardID(), cardItem.userCardData.GetCardUniqueID());
                cardItem.redDotIcon.SetActive(PlayerPrefs.GetInt(newPhotoKey, 0) == 0);
            }
        }
    }

    public void SwitchCurrentPanel(CARD_SUB_PANEL state, card_define defineData = null)
    {
        if (state == CARD_SUB_PANEL.CARD_LIST && defineData != null)
        {
            selectCard = null;
            selectShowcaseCard = defineData;
        }

        curState = state;

        OnCloseDetailInfo();

        GameObject target = CardSubPanel[(int)curState];
        target.SetActive(true);

        OnCompleteTweenAnimationSubPanel();
    }

    public void OnLoadCardShowcaseListUI()
    {
        if (cardShowcaseListCoroutine != null)
            StopCoroutine(cardShowcaseListCoroutine);

        StartCoroutine(LoadCardShowcaseList());
    }

    IEnumerator LoadCardShowcaseList()
    {
        ClearCardShowcaseListUI();

        List<game_data> cardData_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CARD_DEFINE);
        if (cardData_list != null)
        {
            int row = 0;
            int col = 0;

            //NecoCanvas.GetPopupCanvas().RefreshTopUILayer();

            foreach (game_data data in cardData_list)
            {
                card_define defineCardData = (card_define)data;

                GameObject showcaseListItem = Instantiate(CardShowcaseItemPrefab);
                showcaseListItem.transform.SetParent(CardShowcaseContainer.transform);
                RectTransform rt = showcaseListItem.GetComponent<RectTransform>();
                rt.localPosition = Vector3.zero;
                rt.localScale = Vector3.one;

                if (col >= 4)
                {
                    col = 0;
                    row++;
                    if (row > 30)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                }

                List<user_card> userCardDataList = user_card.GetUserCardList(defineCardData.GetCardID());

                CardShowcaseItem showcaseItem = showcaseListItem.GetComponent<CardShowcaseItem>();
                showcaseItem.SetCardShowcaseData(defineCardData, this, row, userCardDataList);

                col++;
            }
        }
    }

    public void OnLoadCardListUI()
    {
        if (cardListCoroutine != null)
            StopCoroutine(cardListCoroutine);

        StartCoroutine(LoadList());
    }

    IEnumerator LoadList()
    {
        ClearCardListUI();

        if (selectShowcaseCard != null)
        {
            int photoCount = 0;

            List<card_define_sub> cardSubDataList = card_define_sub.GetCardDefineSubList(selectShowcaseCard.GetCardID());
            if (cardSubDataList != null)
            {
                int myphoto = 0;
                int mymovie = 0;
                object obj;
                int row = 0;
                int col = 0;

                //PictureCount.text = LocalizeData.GetText("13") + " " + myphoto.ToString() + LocalizeData.GetText("amount");
                //MovieCount.text = LocalizeData.GetText("14") + " " + mymovie.ToString() + LocalizeData.GetText("amount");

                StorageInfo.text = (myphoto + mymovie).ToString() + "/" + GameDataManager.Instance.GetUserData().GetMaxCardCount() + LocalizeData.GetText("33");
                StorageGauge.fillAmount = Mathf.Min(((float)(myphoto + mymovie)) / GameDataManager.Instance.GetUserData().GetMaxCardCount(), 1.0f);
                TrashModeChange(false);

                //// 보유 카드가 1장도 없는 경우 처리
                //emptyAlbumLayer.SetActive(userData_list?.Count <= 0);
                //emptyListLayer.SetActive(userData_list?.Count <= 0);

                MoreSpaceButton.interactable = GameDataManager.Instance.GetUserData().GetMaxCardCount() < 300;
                //NecoCanvas.GetPopupCanvas().RefreshTopUILayer();

                foreach (card_define_sub data in cardSubDataList)
                {
                    CardList_item item = null;

                    if (data.GetIsPefectValue() == 1)
                    {
                        item = PerfectCardItemPrefab.GetComponent<CardList_item>();
                        item.SetCardListUI(data, this, row);
                    }
                    else
                    {
                        GameObject listItem = Instantiate(CardItemPrefab);
                        listItem.transform.SetParent(CardScrollViewContainer.transform);
                        RectTransform rt = listItem.GetComponent<RectTransform>();
                        rt.localPosition = Vector3.zero;
                        rt.localScale = Vector3.one;

                        if (col >= 4)
                        {
                            col = 0;
                            row++;
                            if (row > 30)
                            {
                                yield return new WaitForSeconds(0.1f);
                            }
                        }

                        item = listItem.GetComponent<CardList_item>();
                        item.SetCardListUI(data, this, row);
                    }

                    user_card userCardData = user_card.GetUserCard(data.GetCardID(), data.GetParentCardID());
                    if (userCardData != null)
                    {
                        col++;
                        photoCount++;

                        if (selectCard == null)
                        {
                            selectCard = userCardData;
                            //item.Invoke("OnSelectThisCard", 0.1f);
                            item.OnSelectThisCard();

                            string newPhotoKey = string.Format("{0}_{1}_{2}", SamandaLauncher.GetAccountNo(), selectCard.GetCardID(), selectCard.GetCardUniqueID());
                            PlayerPrefs.SetInt(newPhotoKey, 1);
                        }

                        if (userCardData.GetCardData().GetResourceType() == 1)
                            mymovie++;
                        else
                            myphoto++;
                    }
                }
            }

            // 슬라이더 처리
            cardListSlider.value = (float)photoCount / cardSubDataList.Count;
            cardListSliderText.text = string.Format("{0}/{1}", photoCount, cardSubDataList.Count);
        }


        //List<game_data> userData_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);
        //List<uint> cards = new List<uint>();
        //if (userData_list != null && selectShowcaseCard != null)
        //{
        //    int myphoto = 0;
        //    int mymovie = 0;
        //    object obj;
        //    int row = 0;
        //    int col = 0;

        //    //PictureCount.text = LocalizeData.GetText("13") + " " + myphoto.ToString() + LocalizeData.GetText("amount");
        //    //MovieCount.text = LocalizeData.GetText("14") + " " + mymovie.ToString() + LocalizeData.GetText("amount");

        //    StorageInfo.text = (myphoto + mymovie).ToString() + "/" + GameDataManager.Instance.GetUserData().GetMaxCardCount() + LocalizeData.GetText("33");
        //    StorageGauge.fillAmount = Mathf.Min(((float)(myphoto + mymovie)) / GameDataManager.Instance.GetUserData().GetMaxCardCount(), 1.0f);
        //    TrashModeChange(false);

        //    // 보유 카드가 1장도 없는 경우 처리
        //    emptyAlbumLayer.SetActive(userData_list?.Count <= 0);
        //    emptyListLayer.SetActive(userData_list?.Count <= 0);

        //    MoreSpaceButton.interactable = GameDataManager.Instance.GetUserData().GetMaxCardCount() < 300;
        //    //NecoCanvas.GetPopupCanvas().RefreshTopUILayer();

        //    foreach (game_data data in userData_list)
        //    {
        //        user_card userCardData = (user_card)data;

        //        if (selectShowcaseCard.GetCardID() != userCardData.GetCardID()) { continue; }

        //        GameObject listItem = Instantiate(CardItemPrefab);
        //        listItem.transform.SetParent(CardScrollViewContainer.transform);
        //        RectTransform rt = listItem.GetComponent<RectTransform>();
        //        rt.localPosition = Vector3.zero;
        //        rt.localScale = Vector3.one;

        //        if (col >= 4)
        //        {
        //            col = 0;
        //            row++;
        //            if(row > 30)
        //            {
        //                yield return new WaitForSeconds(0.1f);
        //            }
        //        }

        //        CardList_item item = listItem.GetComponent<CardList_item>();
        //        item.SetCardListUI(userCardData, this, row);

        //        col++;

        //        if (selectCard == null)
        //        {
        //            selectCard = userCardData;
        //            //item.Invoke("OnSelectThisCard", 0.1f);
        //            item.OnSelectThisCard();

        //            string newPhotoKey = string.Format("{0}_{1}_{2}", SamandaLauncher.GetAccountNo(), selectCard.GetCardID(), selectCard.GetCardUniqueID());
        //            PlayerPrefs.SetInt(newPhotoKey, 1);
        //        }

        //        if (data.data.TryGetValue("card_id", out obj))
        //        {
        //            cards.Add((uint)obj);
        //        }

        //        if (userCardData.GetCardData().GetResourceType() == 1)
        //            mymovie++;
        //        else
        //            myphoto++;
        //    }
        //}
        //else
        //{
        //    //CardCount.text = "-/-";
        //}
    }

    public void OnCardItemSelect(user_card usercard)
    {
        if(TrashMode)//multi select mode
        {
            foreach (CardList_item cardItem in CardScrollViewContainer.transform.GetComponentsInChildren<CardList_item>())
            {
                if(cardItem.userCardData == usercard)
                {
                    cardItem.SelectedCursor.SetActive(!cardItem.SelectedCursor.activeSelf);
                }
            }

            return;
        }

        CardDetailPopup.OnShow(usercard);
        OnManageStateChange(CARD_SUB_PANEL.CARD_DETAIL);
    }   

    public void TrashModeChange(bool bOn)
    {
        TrashMode = bOn;
        foreach (CardList_item cardItem in CardScrollViewContainer.transform.GetComponentsInChildren<CardList_item>())
        {
            cardItem.SelectedCursor.SetActive(false);
        }

        TrashCan[0].SetActive(!TrashMode);
        TrashCan[1].SetActive(TrashMode);
    }

    public void OnTrashButton()
    {
        if(TrashMode)
        {
            List<uint> delCard = new List<uint>();
            foreach (CardList_item cardItem in CardScrollViewContainer.transform.GetComponentsInChildren<CardList_item>())
            {
                if(cardItem.SelectedCursor.activeSelf)
                {
                    delCard.Add(cardItem.userCardData.GetCardUniqueID());

                    // 삭제하는 카드 중 현재 선택한 카드가 포함되어 있을 경우
                    if (cardItem.userCardData == selectCard)
                    {
                        selectCard = null;
                    }
                }
            }

            if(delCard.Count > 0)
            {
                ConfirmPopupData param = new ConfirmPopupData();

                param.titleText = LocalizeData.GetText("LOCALIZE_250");
                param.titleMessageText = LocalizeData.GetText("LOCALIZE_474");

                param.messageText_1 = string.Format(LocalizeData.GetText("LOCALIZE_475"), delCard.Count);

                NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(param, CONFIRM_POPUP_TYPE.COMMON, () =>
                {
                    WWWForm data = new WWWForm();
                    data.AddField("api", "card");
                    data.AddField("uid", string.Join(",", delCard));

                    NetworkManager.GetInstance().SendApiRequest("card", 2, data, (response) =>
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
                            if (uri == "card")
                            {
                                JToken resultCode = row["rs"];
                                if (resultCode != null && resultCode.Type == JTokenType.Integer)
                                {
                                    int rs = resultCode.Value<int>();
                                    if (rs == 0)
                                    {
                                        CancelInvoke("OnLoadCardListUI");
                                        Invoke("OnLoadCardListUI", 0.1f);

                                        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_85"), LocalizeData.GetText("LOCALIZE_473"), ()=> {
                                            if (row.ContainsKey("rew"))
                                            {
                                                NecoCanvas.GetPopupCanvas().OnRewardPopupShow(LocalizeData.GetText("LOCALIZE_200"), LocalizeData.GetText("LOCALIZE_201"), "card", apiArr);
                                            }
                                        });
                                    }
                                    else
                                    {
                                        if (rs == 1)
                                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_332"), LocalizeData.GetText("name_edit_error_nocard"));
                                    }
                                }
                            }
                        }
                    });
                });
                return;
            }            
        }

        TrashModeChange(!TrashMode);
    }

    public bool GetTrashModeState()
    {
        return TrashMode;
    }

    public void OnManageStateChange(int state)
    {
        OnManageStateChange((CARD_SUB_PANEL)state);
    }

    void OnManageStateChange(CARD_SUB_PANEL state)
    {
        if (curState == state)
            return;

        preState = curState;
        
        GameObject target = CardSubPanel[(int)curState];
        target.SetActive(false);

        curState = state;
        target = CardSubPanel[(int)curState];
        target.SetActive(true);

        OnManageStateChangeDone();
    }

    public void OnManageStateBack()
    {
        OnManageStateChange(preState);

        if (curState == CARD_SUB_PANEL.LIBRARY_DETAIL)
        {
            CardLibraryDetail libDetail = CardSubPanel[(int)CARD_SUB_PANEL.LIBRARY_DETAIL].transform.GetComponent<CardLibraryDetail>();
            libDetail.SetLibraryDetailUI();
        }
    }

    public void OnManageStateChangeDone()
    {
        for(int i = 0; i < CardSubPanel.Length; i++)
        {
            CardSubPanel[i].SetActive((int)curState == i);
        }
    }

    void ClearCollectionList()
    {
        foreach (Transform child in CollectionScrollViewContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void OnCollectionList()
    {
        ClearCollectionList();

        List<game_data> card_collection = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CARD_COLLECTION);
        if (card_collection != null)
        {
            foreach (game_data data in card_collection)
            {
                GameObject listItem = Instantiate(CollectionItemPrefab);
                listItem.transform.SetParent(CollectionScrollViewContainer.transform);
                RectTransform rt = listItem.GetComponent<RectTransform>();
                rt.localScale = Vector3.one;
                rt.localPosition = Vector3.zero;
                CollectionListItem compo = listItem.GetComponent<CollectionListItem>();
                if (compo)
                {
                    compo.SetCollectionListItem((card_collection)data, this);
                }
            }
        }
    }

    public void OnShowCardDetailInfo()
    {
        return;
    }

    public void OnCloseDetailInfo()
    {
        CardDetailPopup.OnClose();
    }

    public void OnCollectionReward(card_collection data)
    {
        OnCollectionList();

        //GameManager.PopupControl.OnShowCollectionPopup(data, ()=> {
        //    RewardListUI.ShowCollectionRewardList(data.GetCollectionID());
        //});
    }

    public void OnCloseCardShowcaseList()
    {
        
    }

    public void OnCloseCardList()
    {
        if (CardSubPanel[(int)CARD_SUB_PANEL.CARD_LIST].activeInHierarchy)
        {
            SwitchCurrentPanel(CARD_SUB_PANEL.CARD_SHOWCASE_LIST);
        }
        else
        {
            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CARD_LIST_POPUP);

            NecoCanvas.GetUICanvas().UpdateAlbumAlarm();
            NecoCanvas.GetUICanvas().UpdateMainMenuRedDot();
        }
    }

    public void OnCloseCardDetailByState()
    {
        OnCloseCardList();
    }

    public void OnMoreSpace()
    {
        ConfirmPopupData param = new ConfirmPopupData();

        param.titleText = LocalizeData.GetText("LOCALIZE_469");
        param.titleMessageText = LocalizeData.GetText("LOCALIZE_470");

        param.messageText_1 = string.Format(LocalizeData.GetText("LOCALIZE_471"), 100);

        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(param, CONFIRM_POPUP_TYPE.COMMON, ()=> {

            WWWForm data = new WWWForm();
            data.AddField("api", "card");

            NetworkManager.GetInstance().SendApiRequest("card", 3, data, (response) => {
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
                    if (uri == "card")
                    {
                        JToken resultCode = row["rs"];
                        if (resultCode != null && resultCode.Type == JTokenType.Integer)
                        {
                            int rs = resultCode.Value<int>();
                            if (rs == 0)
                            {
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_85"), LocalizeData.GetText("LOCALIZE_472"));
                                Invoke("OnLoadCardListUI", 0.1f);
                            }
                            else
                            {
                                switch(rs)
                                {
                                    case 4:
                                        ConfirmPopupData cpd = new ConfirmPopupData();

                                        cpd.titleText = LocalizeData.GetText("LOCALIZE_490");
                                        cpd.titleMessageText = LocalizeData.GetText("LOCALIZE_491");

                                        cpd.messageText_1 = LocalizeData.GetText("LOCALIZE_492");

                                        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(cpd, CONFIRM_POPUP_TYPE.COMMON, () => {
                                            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CARD_LIST_POPUP);
                                            NecoCanvas.GetPopupCanvas().OnShopListPopupShow(NecoShopPanel.SHOP_CATEGORY.CAT_LEAF);
                                        });
                                        break;
                                    case 5:
                                        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("error"), LocalizeData.GetText("LOCALIZE_488"));
                                        break;
                                    default:
                                        NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("error"), LocalizeData.GetText("LOCALIZE_316"));
                                        break;
                                }
                                
                            }
                        }
                    }
                }
            }); 
            
        });        
    }
}
