using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CardShopPopup : MonoBehaviour
{
    [Header("[Shop Panel Control]")]
    public GameObject bottomUIPanelObject;

    [Header("[Display Photos]")]
    public Image displayPhoto_1;
    public Image displayPhoto_2;
    public Image displayPhoto_3;

    [Header("[Common]")]
    public GameObject ShopPanel;
    public GameObject RandomBoxItemPrefab;
    public GameObject RandomBoxContainer;
    public GameObject ResultPanel;
    public GameObject SkipButton;
    public GameObject OpenEffectObject;
    public GameObject PictureBackground;
    public RandomBoxResultList ResultListView;
    public Button OneTimeShot;
    public Button RepeatTypeShot;
    public Button RetryShot;
    public GameObject OneShotCloseButton;
    public Button Retry11Shot;

    //public Text CurUserCatnip;
    public ImageBlur ResultBlur;

    public Button CloseButton;
    public CardDetailPanel cardDetailPanel;

    private List<uint> resultList = new List<uint>();
    private List<uint> resultPointList = new List<uint>();
    private user_card curUserCard = null;
    private bool BackToPrevState = false;
    private void OnEnable()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "gacha");
        data.AddField("op", 2);

        NetworkManager.GetInstance().SendApiRequest("gacha", 2, data, (response) =>
        {
            Invoke("Init", 0.1f);
        }); 
    }

    public void Init()
    {
        ShopPanel.SetActive(true);
        BackToPrevState = false;
        RefreshUserGold();
        OnCloseResult();
        SetDisplayPhotos();
    }

    //public override void SetCanvasState(GameMain.HahahaState state)
    //{
    //    bool visible = state == GameMain.HahahaState.HAHAHA_PHOTO;
    //    if (visible == gameObject.activeSelf)
    //        return;

    //    gameObject.SetActive(true);
    //    if (visible)
    //    {
    //        foreach (DOTweenAnimation dotween in gameObject.GetComponentsInChildren<DOTweenAnimation>())
    //        {
    //            dotween.DOPlayForward();
    //        }
    //        CancelInvoke("OnCompleteTweenAnimation");
    //    }
    //    else
    //    {
    //        foreach (DOTweenAnimation dotween in gameObject.GetComponentsInChildren<DOTweenAnimation>())
    //        {
    //            dotween.DOPlayBackwards();
    //        }
    //        Invoke("OnCompleteTweenAnimation", 0.5f);
    //    }
    //}

    public void OnCompleteTweenAnimation()
    {
        gameObject.SetActive(false);
    }

    public void OnRandomBoxWithAD()
    {
#if UNITY_EDITOR
        NecoCanvas.GetPopupCanvas().OnToastPopupShow("유니티 에디터에서는 사용불가");
        return;
#endif

        if (neco_data.Instance.GetPhotoADCount() >= 5)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("광고제한"));
            return;
        }

        AdvertiseManager.GetInstance().TryADWithCallback(() => {
            OnRandomBoxSelect(1, true);
        }, () => {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_273"));
        });
    }

    public void OnRandomBoxSelect(int type)
    {
        OnRandomBoxSelect(type, false);
    }

    public void OnRandomBoxSelect(int type, bool useAD)
    {
        curUserCard = null;
        ResultBlur.GetComponent<Button>().interactable = false;

        Transform Effect = ResultListView.transform.Find("Allcard");
        if(Effect != null)
        {
            Effect.gameObject.SetActive(false);
        }

        WWWForm data = new WWWForm();
        data.AddField("api", "gacha");
        data.AddField("gtype", type.ToString());
        data.AddField("ad", useAD ? 1 : 0);

        NetworkManager.GetInstance().SendApiRequest("gacha", 1, data, (response) => {
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            string userID = SamandaLauncher.GetAccountNo();

            JArray income = null;
            JArray incomePoint = null;
            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "gacha")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            JToken opCode = row["op"];
                            if (opCode != null && opCode.Type == JTokenType.Integer)
                            {
                                int op = opCode.Value<int>();
                                if (op == 1)
                                {
                                    JToken rew = row.GetValue("res");
                                    if (rew != null)
                                    {
                                        if (rew.Type == JTokenType.Array)
                                            income = (JArray)rew;                                        
                                    }
                                    rew = row.GetValue("dupes");
                                    if (rew != null)
                                    {
                                        if (rew.Type == JTokenType.Array)
                                            incomePoint = (JArray)rew;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (rs == 1)
                                Invoke("ShowNeedMoneyPopup", 0.01f);
                            if (rs == 2)
                                Invoke("ShowMoreSpacePopup", 0.01f);
                        }
                    }
                }
                if (uri == "card")
                {
                    JToken opCode = row["op"];
                    if (opCode != null && opCode.Type == JTokenType.Integer)
                    {
                        int op = opCode.Value<int>();
                        if (op == 101)
                        {

                            int uniqueCardID = row["uid"].Value<int>();
                            int cardID = row["id"].Value<int>();

                            string newPhotoKey = string.Format("{0}_{1}_{2}", userID, cardID, uniqueCardID);
                            PlayerPrefs.SetInt(newPhotoKey, 0);
                        }
                    }
                }
            }

            if (income == null)
                return;

            resultList.Clear();
            resultPointList.Clear();
            ResultListView.Clear();

            if (income != null)
            {
                foreach (JToken val in income)
                {
                    uint id = val.Value<uint>();

                    resultList.Add(id);
                }
            }
            if(incomePoint != null)
            {
                foreach (JToken val in incomePoint)
                {
                    uint point = val.Value<uint>();

                    resultPointList.Add(point);
                }
            }

            Invoke("ResultActionStart", 0.01f);

            Invoke("RefreshUserGold", 0.01f);

            // 메인 레드닷 갱신
            NecoCanvas.GetUICanvas().SetAlbumAlarmState(true);
        });
    }

    public void OnRandomBoxSelectRetry(int type)
    {


        OnRandomBoxSelect(type);
    }

    public void ShowNeedMoneyPopup()
    {
        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CATNIP_BUY_POPUP);
    }

    public void ShowMoreSpacePopup()
    {
        ConfirmPopupData popupData = new ConfirmPopupData();

        popupData.titleText = LocalizeData.GetText("LOCALIZE_469");
        popupData.titleMessageText = LocalizeData.GetText("need_more_space");

        popupData.messageText_1 = LocalizeData.GetText("LOCALIZE_489");

        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, () => {
            NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.SHOP_LIST_POPUP);
            NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CARD_LIST_POPUP);
        });
    }

    public void ResultActionStart()
    {
        object obj;
        List<game_data> data_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);

        int i = 0;
        foreach (uint uid in resultList)
        {
            if (data_list != null)
            {
                foreach (game_data cd in data_list)
                {
                    if (uid == ((user_card)cd).GetCardUniqueID())
                    {
                        uint point = 0;
                        if (resultPointList.Count > i)
                            point = resultPointList[i];
                        i++;
                        ResultListView.AddResult((user_card)cd, point);
                        break;
                    }
                }
            }
        }

        RetryShot.gameObject.SetActive(false);
        OneShotCloseButton.SetActive(false);

        if (ResultListView.GetInsertCount() > 1)
            ShowResultList();
        else
        {
            ShowResult(0);
        }
    }

    public void SetDisplayPhotos()
    {
        //displayPhoto_1.sprite = 
        //displayPhoto_2.sprite =
        //displayPhoto_3.sprite =
    }

    public void OnCloseResult()
    {
        SkipButton.SetActive(false);
        ResultBlur.gameObject.SetActive(false);
        if(ResultBlur.resultEffect != null)
            ResultBlur.resultEffect.SetActive(false);
        OpenEffectObject.gameObject.SetActive(false);
        CloseButton.interactable = false;
        PictureBackground.SetActive(false);
        curUserCard = null;
        ResultBlur.GetComponent<Button>().interactable = false;

        if (ResultListView.gameObject.activeSelf == false)
        {
            ResultPanel.SetActive(false);
            bottomUIPanelObject.SetActive(true);
            NecoCanvas.GetPopupCanvas().OnTopUIInfoLayer();
        }

        //if(BackToPrevState)
        //{
        //    NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CARD_SHOP_POPUP);
        //}
    }

    public void ShowResultList()
    {
        Transform Effect = ResultListView.transform.Find("Allcard");
        if (Effect != null)
        {
            Effect.gameObject.SetActive(true);
        }

        PictureBackground.SetActive(false);
        ResultPanel.SetActive(true);
        bottomUIPanelObject.SetActive(false);
        NecoCanvas.GetPopupCanvas().OffTopUIInfoLayer();
        ResultListView.gameObject.SetActive(true);
        ResultBlur.gameObject.SetActive(false);
    }

    public void OnCloseResultList()
    {
        PictureBackground.SetActive(false);
        if (ResultBlur.resultEffect != null)
            ResultBlur.resultEffect.SetActive(false);
        ResultListView.gameObject.SetActive(false);
        ResultPanel.SetActive(false);
        bottomUIPanelObject.SetActive(true);
        NecoCanvas.GetPopupCanvas().OnTopUIInfoLayer();
    }

    public void ShowResult(int index)
    {
        StartCoroutine(ShowResultEffect(index));
    }

    public IEnumerator ShowResultEffect(int index)
    {
        ResultBlur.GetComponent<Button>().interactable = false;
        
        bool allClear = false;
        uint curID = resultList[index];
        if (resultList.Count > 1)
        {
            ResultListView.skipButton.SetActive(false);

            foreach (GameObject resultObj in ResultListView.resultList)
            {
                Button button = resultObj.GetComponent<Button>();
                if (button)
                    button.interactable = false;
            }

            ResultListView.OffEffect(index);

            yield return new WaitForSeconds(1.0f);

            foreach (GameObject resultObj in ResultListView.resultList)
            {
                Button button = resultObj.GetComponent<Button>();
                if (button)
                    button.interactable = true;
            }

            allClear = ResultListView.OffBlur(index);

            ResultListView.skipButton.SetActive(true);
        }

        PictureBackground.SetActive(true);
        ResultBlur.gameObject.SetActive(true);

        curUserCard = null;
        List<game_data> data_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);
        if (data_list != null)
        {
            foreach (game_data data in data_list)
            {
                if(((user_card)data).GetCardUniqueID() == curID)
                {
                    curUserCard = (user_card)data;
                }    
            }
        }

        if (curUserCard == null)
        {
            OnCloseResult();
        }
        else
        {
            SkipButton.SetActive(resultList.Count > 1);
            ResultPanel.gameObject.SetActive(true);
            bottomUIPanelObject.SetActive(false);
            NecoCanvas.GetPopupCanvas().OffTopUIInfoLayer();
            CloseButton.interactable = false;

            if (ResultBlur.OnResultEffect(curUserCard, OnResultEffectDone) == false)
            {
                ResultPanel.SetActive(false);
                bottomUIPanelObject.SetActive(true);
                NecoCanvas.GetPopupCanvas().OnTopUIInfoLayer();
                OnCloseResult();

                CloseButton.interactable = true;                
            }
        }

        if (allClear)
        {
            while (ResultBlur.gameObject.activeSelf)
            {
                yield return new WaitForSeconds(0.3f);
            }

            ResultListView.OnEffectFinished();
        }
    }

    public void OnResultEffectDone()
    {
        CloseButton.interactable = true;
        ResultBlur.GetComponent<Button>().interactable = true;

        if(resultList.Count == 1)
        {
            CloseButton.interactable = false;
            RetryShot.gameObject.SetActive(true);
            OneShotCloseButton.SetActive(true);

            uint total = 0;
            foreach(uint p in resultPointList)
            {
                total += p;
            }

            resultPointList.Clear();
            if(total > 0)
            {
                RewardData reward = new RewardData();
                reward.point = total;
                NecoCanvas.GetPopupCanvas().OnSingleRewardPopup(LocalizeData.GetText("LOCALIZE_242"), LocalizeData.GetText("LOCALIZE_243"), reward);
            }
        }
    }

    public void OnSkipButton()
    {
        OnCloseResult();
    }

    public void RefreshUserGold()
    {
        NecoCanvas.GetPopupCanvas()?.RefreshTopUILayer();

        Image oneShotTypeIcon = null;
        Text oneShotPriceText = null;
        Image repeatShotTypeIcon = null;
        Text repeatShotPriceText = null;
        Image retryShotTypeIcon = null;
        Text retryShotPriceText = null;
        Image retry11ShotTypeIcon = null;
        Text retry11ShotPriceText = null;

        Transform cash1 = OneTimeShot.transform.Find("cash");
        if (cash1 != null)
        {
            Transform icon = cash1.Find("Image");
            if (icon != null)
            {
                oneShotTypeIcon = icon.GetComponent<Image>();
                oneShotTypeIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_catleaf");
            }
            Transform text = cash1.Find("PriceText");
            if (text != null)
            {
                oneShotPriceText = text.GetComponent<Text>();
                oneShotPriceText.text = "50";
            }
        }
        Transform cash2 = RepeatTypeShot.transform.Find("cash");
        if (cash2 != null)
        {
            Transform icon = cash2.Find("Image");
            if (icon != null)
            {
                repeatShotTypeIcon = icon.GetComponent<Image>();
                repeatShotTypeIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_catleaf");
            }
            Transform text = cash2.Find("PriceText");
            if (text != null)
            {
                repeatShotPriceText = text.GetComponent<Text>();
                repeatShotPriceText.text = "500";
            }
        }
        Transform cash3 = Retry11Shot.transform.Find("cash");
        if (cash3 != null)
        {
            Transform icon = cash3.Find("Image");
            if (icon != null)
            {
                retry11ShotTypeIcon = icon.GetComponent<Image>();
                retry11ShotTypeIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_catleaf");
            }
            Transform text = cash3.Find("PriceText");
            if (text != null)
            {
                retry11ShotPriceText = text.GetComponent<Text>();
                retry11ShotPriceText.text = "500";
            }
        }
        Transform cash4 = RetryShot.transform.Find("cash");
        if (cash4 != null)
        {
            Transform icon = cash4.Find("Image");
            if (icon != null)
            {
                retryShotTypeIcon = icon.GetComponent<Image>();
                retryShotTypeIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_catleaf");
            }
            Transform text = cash4.Find("PriceText");
            if (text != null)
            {
                retryShotPriceText = text.GetComponent<Text>();
                retryShotPriceText.text = "50";
            }
        }

        uint totalTicket = user_items.GetUserItemAmount(137);
        bool enableOneShot = totalTicket > 0;
        bool enableRepeatShot = totalTicket >= 11;

        if(enableOneShot)
        {
            oneShotTypeIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_gachaticket");
            oneShotPriceText.text = totalTicket + " / 1";

            retryShotTypeIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_gachaticket");
            retryShotPriceText.text = totalTicket + " / 1";
        }

        if (enableRepeatShot)
        {
            repeatShotTypeIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_gachaticket");
            repeatShotPriceText.text = totalTicket + " / 11";

            retry11ShotTypeIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_gachaticket");
            retry11ShotPriceText.text = totalTicket + " / 11";
        }

        if (enableOneShot == false || enableRepeatShot == false)
        {
            object obj;
            uint catnip = 0;
            users user = GameDataManager.Instance.GetUserData();
            if (user != null)
            {
                if (user.data.TryGetValue("catnip", out obj))
                {
                    catnip = (uint)obj;
                }
            }

            if (enableOneShot == false)
            {
                enableOneShot = catnip >= 50;
            }

            if (enableRepeatShot == false)
            {
                enableRepeatShot = catnip >= 500;
            }
        }

        Color enableColor = new Color(0.5254902f, 0.8431373f, 0.9921569f, 1.0f);
        Color disableColor = new Color(0.6f, 0.6352941f, 0.6509804f, 1.0f);

        cash1.GetComponent<Image>().color = enableOneShot ? enableColor : disableColor;
        OneTimeShot.transform.Find("background").GetComponent<Image>().color = enableOneShot ? enableColor : disableColor;
        cash2.GetComponent<Image>().color = enableRepeatShot ? enableColor : disableColor;        
        RepeatTypeShot.transform.Find("background").GetComponent<Image>().color = enableRepeatShot ? enableColor : disableColor;
        cash3.GetComponent<Image>().color = enableRepeatShot ? enableColor : disableColor;
        Retry11Shot.transform.Find("background").GetComponent<Image>().color = enableRepeatShot ? enableColor : disableColor;
        cash4.GetComponent<Image>().color = enableOneShot ? enableColor : disableColor;
        RetryShot.transform.Find("background").GetComponent<Image>().color = enableOneShot ? enableColor : disableColor;

        enableColor = new Color(0.3607843f, 0.6235294f, 0.8f, 1.0f);
        disableColor = new Color(0.427451f, 0.4588235f, 0.4823529f, 1.0f);

        OneTimeShot.transform.Find("BuyInfoText").GetComponent<Outline>().effectColor = enableOneShot ? enableColor : disableColor;
        cash1.transform.Find("PriceText").GetComponent<Outline>().effectColor = enableOneShot ? enableColor : disableColor;
        RepeatTypeShot.transform.Find("BuyInfoText").GetComponent<Outline>().effectColor = enableRepeatShot ? enableColor : disableColor;
        cash2.transform.Find("PriceText").GetComponent<Outline>().effectColor = enableRepeatShot ? enableColor : disableColor;
        Retry11Shot.transform.Find("BuyInfoText").GetComponent<Outline>().effectColor = enableRepeatShot ? enableColor : disableColor;
        cash3.transform.Find("PriceText").GetComponent<Outline>().effectColor = enableRepeatShot ? enableColor : disableColor;
        RetryShot.transform.Find("BuyInfoText").GetComponent<Outline>().effectColor = enableOneShot ? enableColor : disableColor;
        cash4.transform.Find("PriceText").GetComponent<Outline>().effectColor = enableOneShot ? enableColor : disableColor;

        Transform AD1 = OneTimeShot.transform.Find("ADBubble2");
        Transform AD2 = RetryShot.transform.Find("ADBubble2");
        if (neco_data.Instance.GetPhotoADCount() >= 5)
        {
            AD1.gameObject.SetActive(false);
            AD2.gameObject.SetActive(false);
        }
        else
        {
            AD1.gameObject.SetActive(true);
            AD2.gameObject.SetActive(true);

            AD1.Find("Text").GetComponent<Text>().text = string.Format("{0}/{1}", neco_data.Instance.GetPhotoADCount(), 5);
            AD2.Find("Text").GetComponent<Text>().text = string.Format("{0}/{1}", neco_data.Instance.GetPhotoADCount(), 5);
        }
    }

    public void ShowCurCardDetail()
    {
        ShowCardDetail(curUserCard);
        OnCloseResult();
    }

    public void ShowCurCardDetailWithIndex(int index)
    {
        uint curID = resultList[index];
        object obj;
        List<game_data> data_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);
        if (data_list != null)
        {
            foreach (game_data data in data_list)
            {
                if (((user_card)data).GetCardUniqueID() == curID)
                {
                    ShowCardDetail((user_card)data);
                }
            }
        }
    }

    public void ShowCardDetail(user_card target)
    {
        List<game_data> list = new List<game_data>();
        list.Add(target);
        cardDetailPanel.OnShow(target, list, false);
    }

    public void ShowPhotoResult(user_card card)
    {
        gameObject.SetActive(true);
        ShopPanel.SetActive(false);

        resultList.Clear();
        ResultListView.Clear();

        resultList.Add(card.GetCardUniqueID());

        ResultActionStart();

        BackToPrevState = true;
    }

    //public void OnCloseCardShop()
    //{
    //    NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CARD_SHOP_POPUP);
    //}
    public void OnShowCardListPopup()
    {
        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CARD_LIST_POPUP);
        NecoCanvas.GetPopupCanvas().OnTopUIInfoLayer(TOP_UI_PANEL_TYPE.GUIDE_QUEST);
    }
}
