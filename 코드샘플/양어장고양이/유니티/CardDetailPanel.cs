using DG.Tweening;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using static UnityEngine.UI.Button;

public class CardDetailPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public RawImage CardDetailImage;
    
    public VideoPlayer videoPlayer;
    public GameObject EditButton;
    public Text CardTitle;
    
    public Text CardGainDate;

    user_card userCardData = null;
    card_define defineCardData = null;

    private Vector2 prevPoint;
    private Vector2 newPoint;
    private Vector2 screenTravel;
    private int currentMainFinger = -1;
    private int currentSecondFinger = -1;
    private Vector2 posA;
    private Vector2 posB;
    private float previousDistance = -1f;
    private float distance;
    private float pinchDelta = 0f;

    float   minRatio = 0.0f;
    float   maxRatio = 0.0f;
    Vector2 MAX_SIZE = Vector2.zero;
    Vector2 CONTENT_SIZE = Vector2.zero;
    List<game_data> viewList = null;
    bool bOnCardCanvas = true;

    public GameObject EditMemoPopup;
    public GameObject PerfectIcon;
    public GameObject MovieIcon;
    public HahahaChat hahahaChat;
    public GameObject[] SubButtons;
    public CardListPopup CardListPopup;

    [Header("[Bragging Button Layer]")]
    public GameObject SendChatButton;
    public Image priceIcon;
    public Text priceAmountText;

    private void OnEnable()
    {
        RenderTexture target = videoPlayer.targetTexture;
        if (target)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }
    }

    private void OnDisable()
    {
        RenderTexture target = videoPlayer.targetTexture;
        if (target)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }
    }
    
    public void OnShow(user_card card, List<game_data> detaillist = null, bool onCardCanvas = true)
    {
        bOnCardCanvas = onCardCanvas;
        if (onCardCanvas == false)
        {
            if (NecoCanvas.GetPopupCanvas().IsPopupOpen(NecoPopupCanvas.POPUP_TYPE.SHOP_LIST_POPUP))
            {
                NecoShopPanel panel = NecoCanvas.GetPopupCanvas().PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.SHOP_LIST_POPUP].GetComponent<NecoShopPanel>();
                if (panel != null)
                {
                    if (panel.cardShopPanel.ResultPanel.activeSelf)
                    {
                        foreach (GameObject subPanel in CardListPopup.CardSubPanel)
                        {
                            subPanel.SetActive(false);
                        }
                    }
                }
            }

            CardListPopup.CardDetailPopup.transform.parent.gameObject.SetActive(true);
            CardListPopup.CardDetailPopup.gameObject.SetActive(true);
            CardListPopup.gameObject.SetActive(true);            
        }

        foreach (GameObject sub in SubButtons)
        { 
            if(sub != null)
                sub.SetActive(!NecoCanvas.GetPopupCanvas().IsPopupOpen(NecoPopupCanvas.POPUP_TYPE.SHOP_LIST_POPUP));
        }

        minRatio = 0.0f;
        maxRatio = 0.0f;
        MAX_SIZE = Vector2.zero;
        CONTENT_SIZE = Vector2.zero;

        userCardData = card;
        defineCardData = card.GetCardData();

        viewList = detaillist == null ? GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD) : detaillist;

        if (defineCardData == null)
            return;

        videoPlayer.Stop();
        VideoClip preClip = videoPlayer.clip;
        Resources.UnloadAsset(preClip);

        RenderTexture target = videoPlayer.targetTexture;
        if (target)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        object obj;

        if (defineCardData.data.TryGetValue("resource_path", out obj))
        {
            string path = (string)obj;
            if (defineCardData.data.TryGetValue("resource_type", out obj))
            {
                uint type = (uint)obj;
                switch (type)
                {
                    case 1:
                        VideoClip clip = Resources.Load<VideoClip>(path);
                        if (clip)
                        {
                            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                            videoPlayer.clip = clip;
                            videoPlayer.isLooping = true;
                            CardDetailImage.texture = videoPlayer.targetTexture;
                            CardDetailImage.uvRect = new Rect(Vector2.zero, Vector2.one);
                            videoPlayer.enabled = true;

                            videoPlayer.prepareCompleted += OnVideoPrepared;
                            videoPlayer.Prepare();
                        }
                        break;
                    case 0:
                    default:
                        videoPlayer.Stop();
                        videoPlayer.enabled = false;
                        Sprite sprite = Resources.Load<Sprite>(path);
                        CardDetailImage.texture = sprite.texture;
                        CardDetailImage.uvRect = userCardData.GetUVRect();
                        break;
                }
            }
            Canvas canvas = transform.GetComponentInParent<Canvas>();
            RectTransform canvasRT = canvas.transform as RectTransform;
            Vector2 canvasSize = canvasRT.rect.size;

            RectTransform containerRT = CardDetailImage.transform.parent.GetComponent<RectTransform>();
            canvasSize.y = canvasSize.y- containerRT.offsetMax.y - containerRT.offsetMin.y;
            
            MAX_SIZE = canvasSize;
            CONTENT_SIZE = userCardData.GetRect().size;

            minRatio = MAX_SIZE.x / CONTENT_SIZE.x;
            maxRatio = MAX_SIZE.y / CONTENT_SIZE.y;

            RectTransform rt = CardDetailImage.GetComponent<RectTransform>();
            rt.sizeDelta = CONTENT_SIZE * minRatio;
            rt.localPosition = Vector2.zero;
        }

        string memo = userCardData.GetCardMemo();
        EditButton.SetActive(string.IsNullOrEmpty(memo));
        CardTitle.text = string.IsNullOrEmpty(memo) ? LocalizeData.GetText("default_pic_name") : memo;

        CardGainDate.text = "";
        if (userCardData.data.TryGetValue("get_time", out obj))
        {
            DateTime time = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(((uint)obj));
            CardGainDate.text = time.ToString(string.Format("yyyy.MM.dd"));
        }

        //if (ManageMenuTab.activeSelf)
        //{
        //    ManageMenuTab.GetComponent<DOTweenAnimation>().DOPlayBackwards();
        //}
        gameObject.SetActive(true);

        PictureFixPosCheck(false);

        PerfectIcon.SetActive(false);
        MovieIcon.SetActive(false);
        switch (userCardData.GetCardType())
        {
            case user_card.CARD_TYPE.PERFECT:
                PerfectIcon.SetActive(true);
                break;
            case user_card.CARD_TYPE.MOVIE:
                MovieIcon.SetActive(true);
                break;
            case user_card.CARD_TYPE.PIECE:
            default:
                break;
        }

        if (bOnCardCanvas)
        {
            string newPhotoKey = string.Format("{0}_{1}_{2}", SamandaLauncher.GetAccountNo(), card.GetCardID(), card.GetCardUniqueID());
            PlayerPrefs.SetInt(newPhotoKey, 1);
        }

        // 자랑하기 버튼 UI 세팅
        uint braggingTiecketCount = user_items.GetUserItemAmount(141);
        if (braggingTiecketCount > 0)
        {
            priceIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_good");
            priceAmountText.text = string.Format("{0}/{1}", braggingTiecketCount.ToString("n0"), 1);
        }
        else
        {
            priceIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_catleaf");
            priceAmountText.text = "30";
        }
    }

    public void OnShow(JObject cardData)
    {
        bOnCardCanvas = false;
        
        foreach (GameObject subPanel in CardListPopup.CardSubPanel)
        {
            subPanel.SetActive(false);
        }

        CardListPopup.CardDetailPopup.gameObject.SetActive(true);
        CardListPopup.gameObject.SetActive(true);

        foreach (GameObject sub in SubButtons)
        {
            sub.SetActive(false);
        }

        minRatio = 0.0f;
        maxRatio = 0.0f;
        MAX_SIZE = Vector2.zero;
        CONTENT_SIZE = Vector2.zero;

        userCardData = null;

        JToken jtk;
        uint cardNo = 0;
        float x = 0.0f;
        float y = 0.0f;
        float w = 0.0f;
        float h = 0.0f;
        string memo = "";
        string owner = "";
        uint date = 0;

        if (cardData.TryGetValue("CardNo", out jtk))
        {
            cardNo = jtk.Value<uint>();
        }
        if (cardData.TryGetValue("x", out jtk))
        {
            x = jtk.Value<float>();
        }
        if (cardData.TryGetValue("y", out jtk))
        {
            y = jtk.Value<float>();
        }
        if (cardData.TryGetValue("w", out jtk))
        {
            w = jtk.Value<float>();
        }
        if (cardData.TryGetValue("h", out jtk))
        {
            h = jtk.Value<float>();
        }
        if (cardData.TryGetValue("memo", out jtk))
        {
            memo = jtk.Value<string>();
        }
        if (cardData.TryGetValue("date", out jtk))
        {
            date = jtk.Value<uint>();
        }
        if (cardData.TryGetValue("owner", out jtk))
        {
            owner = jtk.Value<string>();
        }

        object obj;
        List<game_data> card_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.CARD_DEFINE);
        foreach (game_data data in card_list)
        {
            if (data.data.TryGetValue("card_id", out obj))
            {
                if ((uint)obj == cardNo)
                {
                    defineCardData = (card_define)data;
                }
            }
        }

        viewList = new List<game_data>();

        if (defineCardData == null)
            return;

        videoPlayer.Stop();
        VideoClip preClip = videoPlayer.clip;
        Resources.UnloadAsset(preClip);

        RenderTexture target = videoPlayer.targetTexture;
        if (target)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        user_card.CARD_TYPE cardType = user_card.CARD_TYPE.UNKNOWN;
        if (defineCardData.data.TryGetValue("resource_path", out obj))
        {
            string path = (string)obj;
            if (defineCardData.data.TryGetValue("resource_type", out obj))
            {
                uint type = (uint)obj;
                switch (type)
                {
                    case 1:
                        VideoClip clip = Resources.Load<VideoClip>(path);
                        if (clip)
                        {
                            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                            videoPlayer.clip = clip;
                            videoPlayer.isLooping = true;
                            CardDetailImage.texture = videoPlayer.targetTexture;
                            CardDetailImage.uvRect = new Rect(Vector2.zero, Vector2.one);
                            videoPlayer.enabled = true;

                            videoPlayer.prepareCompleted += OnVideoPrepared;
                            videoPlayer.Prepare();
                        }
                        cardType = user_card.CARD_TYPE.MOVIE;
                        break;
                    case 0:
                    default:
                        videoPlayer.Stop();
                        videoPlayer.enabled = false;
                        Sprite sprite = Resources.Load<Sprite>(path);
                        CardDetailImage.texture = sprite.texture;
                        CardDetailImage.uvRect = new Rect(x, y, w, h);
                        cardType = user_card.CARD_TYPE.PIECE;
                        if(CardDetailImage.uvRect.position == Vector2.zero && CardDetailImage.uvRect.size == Vector2.one)
                        {
                            cardType = user_card.CARD_TYPE.PERFECT;
                        }
                        break;
                }
            }

            RectTransform canvasRT = NecoCanvas.GetPopupCanvas().transform as RectTransform;
            Vector2 canvasSize = canvasRT.rect.size;

            RectTransform containerRT = CardDetailImage.transform.parent.GetComponent<RectTransform>();
            canvasSize.y = canvasSize.y - containerRT.offsetMax.y - containerRT.offsetMin.y;

            MAX_SIZE = canvasSize;
            CONTENT_SIZE = new Vector2(CardDetailImage.texture.width * w, CardDetailImage.texture.height * h);

            minRatio = MAX_SIZE.x / CONTENT_SIZE.x;
            maxRatio = MAX_SIZE.y / CONTENT_SIZE.y;

            RectTransform rt = CardDetailImage.GetComponent<RectTransform>();
            rt.sizeDelta = CONTENT_SIZE * minRatio;
            rt.localPosition = Vector2.zero;
        }

        EditButton.SetActive(false);
        CardTitle.text = "\'" + owner + "\'의 " + (string.IsNullOrEmpty(memo) ? "사진" : memo);

        DateTime time = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(date);
        CardGainDate.text = time.ToString(string.Format("yyyy.MM.dd"));

        //if (ManageMenuTab.activeSelf)
        //{
        //    ManageMenuTab.GetComponent<DOTweenAnimation>().DOPlayBackwards();
        //}
        gameObject.SetActive(true);

        PictureFixPosCheck(false);

        PerfectIcon.SetActive(false);
        MovieIcon.SetActive(false);
        switch (cardType)
        {
            case user_card.CARD_TYPE.PERFECT:
                PerfectIcon.SetActive(true);
                break;
            case user_card.CARD_TYPE.MOVIE:
                MovieIcon.SetActive(true);
                break;
            case user_card.CARD_TYPE.PIECE:
            default:
                break;
        }
    }

    public void OnVideoPrepared(VideoPlayer player)
    {
        player.Play();
    }

    public void OnClose()
    {
        //if (ManageMenuTab.activeSelf)
        //{
        //    ManageMenuTab.GetComponent<DOTweenAnimation>().DOPlayForward();
        //}
        if(bOnCardCanvas)
        {
            CardListPopup.RefreshRedDot();
        }
        gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (currentMainFinger == -1)
        {
            currentMainFinger = data.pointerId;
            prevPoint = data.position;

            posA = data.position;

            return;
        }

        if (currentSecondFinger == -1)
        {
            currentSecondFinger = data.pointerId;
            posB = data.position;

            figureDelta();
            previousDistance = distance;

            return;
        }

        Debug.Log("third+ finger! (ignore)");
    }

    public void OnDrag(PointerEventData data)
    {
        if (currentMainFinger == data.pointerId)
        {
            newPoint = data.position;
            screenTravel = newPoint - prevPoint;
            prevPoint = newPoint;

            if (currentSecondFinger == -1)
            {
                _processSwipe(); 
            }
            else
            {
                
            }

            posA = data.position;
        }

        if (currentSecondFinger == -1) return;

        if (currentMainFinger == data.pointerId) posA = data.position;
        if (currentSecondFinger == data.pointerId) posB = data.position;

        figureDelta();
        pinchDelta = distance - previousDistance;
        previousDistance = distance;

        _processPinch(); 
    }

    private void figureDelta()
    {
        // when/if two touches, keep track of the distance between them
        distance = Vector2.Distance(posA, posB);
    }

    public void OnPointerUp(PointerEventData data)
    {
        bool EnableSwipe = false;
        if (currentMainFinger == data.pointerId)
        {
            currentMainFinger = -1;
            EnableSwipe = true;
        }
        if (currentSecondFinger == data.pointerId)
        {
            currentSecondFinger = -1;
        }

        PictureFixPosCheck(EnableSwipe);
    }

    private void PictureFixPosCheck(bool EnableSwipe)
    {
        RectTransform rt = CardDetailImage.GetComponent<RectTransform>();

        float curRatio = rt.sizeDelta.x / CONTENT_SIZE.x;
        if (curRatio < minRatio)
            rt.sizeDelta = CONTENT_SIZE * minRatio;
        if (curRatio > maxRatio)
            rt.sizeDelta = CONTENT_SIZE * maxRatio;

        Vector2 pos = rt.localPosition;
        float limit = Mathf.Abs((rt.sizeDelta.x / 2) - (MAX_SIZE.x / 2));
        if (pos.x >= limit)
        {
            if (EnableSwipe && Mathf.Abs(pos.x - limit) > (MAX_SIZE.x / 4))
            {
                //Debug.LogError("OnLeftSwipe");
                if (OnLeftSwipe())
                {
                    currentMainFinger = -1;
                    return;
                }
            }
            pos.x = limit;
        }
        if (pos.x <= limit * -1.0f)
        {
            if (EnableSwipe && Mathf.Abs(pos.x - limit) > (MAX_SIZE.x / 4))
            {
                //Debug.LogError("OnRightSwipe");
                if (OnRightSwipe())
                {
                    currentMainFinger = -1;
                    return;
                }
            }
            pos.x = limit * -1.0f;
        }

        if (curRatio <= minRatio)
        {
            pos.y = 0;
        }
        else
        {
            limit = Mathf.Abs((rt.sizeDelta.y / 2) - (MAX_SIZE.y / 2));
            if (pos.y > limit)
            {
                pos.y = limit;
            }
            if (pos.y < limit * -1.0f)
            {
                pos.y = limit * -1.0f;
            }
        }

        rt.localPosition = pos;
    }

    private void _processSwipe()
    {
        RectTransform rt = CardDetailImage.GetComponent<RectTransform>();
        Vector2 pos = (Vector2)rt.localPosition + screenTravel;

        float limit = Mathf.Abs((rt.sizeDelta.x / 2) - (MAX_SIZE.x / 2));
        float curRatio = rt.sizeDelta.x / CONTENT_SIZE.x;

        rt.localPosition = pos;
        if (curRatio <= minRatio)
        {
            pos.y = 0;
        }
        else
        {
            limit = Mathf.Abs((rt.sizeDelta.y / 2) - (MAX_SIZE.y / 2));
            if (pos.y > limit)
            {
                pos.y = limit;
            }
            if (pos.y < limit * -1.0f)
            {
                pos.y = limit * -1.0f;
            }
        }

        rt.localPosition = pos;
    }

    private void _processPinch()
    {
        RectTransform rt = CardDetailImage.GetComponent<RectTransform>();
        float curRatio = Mathf.Max(((rt.sizeDelta.x / CONTENT_SIZE.x) + (pinchDelta / CONTENT_SIZE.x)), minRatio);
        curRatio = Mathf.Min(curRatio, maxRatio);
        rt.sizeDelta = CONTENT_SIZE * curRatio;
    }

    public void OnSetBackgroundCard()
    {
        NecoCanvas.GetPopupCanvas().OnToastPopupShow("삭제 예정인 컨텐츠입니다.");
    }

    bool OnRightSwipe()
    {
        if (viewList != null)
        {
            for (int i = 0; i < viewList.Count; i++)
            {
                if (viewList[i] == userCardData)
                {
                    int targetIndex = i + 1;
                    if (targetIndex >= viewList.Count)
                        targetIndex = 0;

                    if ((user_card)viewList[targetIndex] == userCardData)
                    {
                        PictureFixPosCheck(false);
                        return false;
                    }

                    OnShow((user_card)viewList[targetIndex], viewList);
                    AudioSource source = GetComponent<AudioSource>();
                    if(source)
                    {
                        source.Play();
                    }
                    return true;
                }
            }
        }

        return false;
    }

    bool OnLeftSwipe()
    {        
        if (viewList != null)
        {
            for (int i = 0; i < viewList.Count; i++)
            {
                if (viewList[i] == userCardData)
                {
                    int targetIndex = i - 1;
                    if (targetIndex < 0)
                        targetIndex = viewList.Count - 1;

                    if ((user_card)viewList[targetIndex] == userCardData)
                    {
                        PictureFixPosCheck(false);
                        return false;
                    }

                    OnShow((user_card)viewList[targetIndex], viewList);
                    AudioSource source = GetComponent<AudioSource>();
                    if (source)
                    {
                        source.Play();
                    }

                    return true;
                }
            }
        }

        return false;
    }

    public void OnCardTitleEdit()
    {
        EditMemoPopup.SetActive(true);
    }

    public void OnCardTitleEditExit()
    {
        EditMemoPopup.SetActive(false);
    }

    public void TryEditMemo()
    {
        InputField input = EditMemoPopup.GetComponentInChildren<InputField>();
        if (input == null)
            return;

        string memo = input.text;
        input.text = "";
        user_card card = userCardData;
        if (card == null)
            return;

        WWWForm data = new WWWForm();
        data.AddField("api", "card");
        data.AddField("uid", card.GetCardUniqueID().ToString());
        data.AddField("memo", memo);

        NetworkManager.GetInstance().SendApiRequest("card", 1, data, (response) => {
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
                            JToken opCode = row["op"];
                            if (opCode != null && opCode.Type == JTokenType.Integer)
                            {
                                int op = opCode.Value<int>();
                                if (op == 1)
                                {
                                    JToken mem = row["memo"];
                                    if (mem != null)
                                    {
                                        CardTitle.text = mem.Value<string>();
                                    }
                                }
                            }

                            EditMemoPopup.SetActive(false);
                            EditButton.SetActive(false);
                        }
                        else
                        {
                            if (rs == 1)
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_332"), LocalizeData.GetText("name_edit_error_nocard"));
                            else if (rs == 2)
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_332"), LocalizeData.GetText("name_edit_error_alreadynamed"));
                            else if (rs == 3)
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_332"), LocalizeData.GetText("name_edit_error_rudeword"));
                        }
                    }
                }
            }
        });
    }

    public void OnAbandonCard()
    {
        ConfirmPopupData param = new ConfirmPopupData();

        param.titleText = LocalizeData.GetText("LOCALIZE_250");
        param.titleMessageText = LocalizeData.GetText("LOCALIZE_474");

        param.messageText_1 = LocalizeData.GetText("LOCALIZE_476");

        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(param, CONFIRM_POPUP_TYPE.COMMON, ()=> {
            user_card card = userCardData;
            if (card == null)
                return;

            WWWForm data = new WWWForm();
            data.AddField("api", "card");
            data.AddField("uid", card.GetCardUniqueID().ToString());

            NetworkManager.GetInstance().SendApiRequest("card", 2, data, (response) => {
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
                                CardListPopup.Invoke("OnLoadCardListUI", 0.01f);

                                //CardLibraryDetail libDetail = cardCanvas.GetComponentInChildren<CardLibraryDetail>(true);
                                //libDetail.Invoke("SetLibraryDetailUI", 0.01f);

                                CardListPopup.Invoke("OnManageStateBack", 0.02f);

                                if (row.ContainsKey("rew"))
                                {
                                    NecoCanvas.GetPopupCanvas().OnRewardPopupShow(LocalizeData.GetText("LOCALIZE_200"), LocalizeData.GetText("LOCALIZE_201"), "card", apiArr);
                                }
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
    }

    public void OnMoveLibrary()
    {
        CardLibraryDetail libDetail = CardListPopup.CardSubPanel[4].transform.parent.GetComponentInChildren<CardLibraryDetail>(true);
        libDetail.ShowLibraryDetail(defineCardData);
        libDetail.CancelInvoke("InitCusor");
        Invoke("SetLibraryCusor", 0.5f);
    }

    public void SetLibraryCusor()
    {
        CardLibraryDetail libDetail = CardListPopup.CardSubPanel[4].transform.parent.GetComponentInChildren<CardLibraryDetail>(true);
        libDetail.SetCurosr(userCardData);        
    }

    public void OnSendCardInfoToSamanda()
    {
        if (bOnCardCanvas == false)
        {
            if (NecoCanvas.GetPopupCanvas().IsPopupOpen(NecoPopupCanvas.POPUP_TYPE.SHOP_LIST_POPUP))
            {
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_493"));
                return;
            }
        }

        if (userCardData == null)
            return;

        ConfirmPopupData popupData = new ConfirmPopupData();
        popupData.titleText = LocalizeData.GetText("LOCALIZE_85");

        popupData.titleMessageText = LocalizeData.GetText("사진자랑하기");

        if (user_items.GetUserItemAmount(141) > 0)
        {
            popupData.messageText_1 = LocalizeData.GetText("사진자랑하기쿠폰");
            popupData.amountIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_good");
            popupData.amountText = "1"; // todo bt - 추후 데이터 연동 필요
        }
        else
        {
            popupData.messageText_1 = LocalizeData.GetText("사진자랑하기금액");
            popupData.amountIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_catleaf");
            popupData.amountText = "30"; // todo bt - 추후 데이터 연동 필요
        }
        

        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, ()=> {
            WWWForm param = new WWWForm();
            param.AddField("api", "chore");
            param.AddField("op", 1);
            
            if (user_items.GetUserItemAmount(141) > 0)
            {
                param.AddField("item", 141);
                param.AddField("cnt", -1);
            }
            else
            {
                uint catnip = 0;
                users user = GameDataManager.Instance.GetUserData();
                if (user != null)
                {
                    object c;
                    if (user.data.TryGetValue("catnip", out c))
                    {
                        catnip = (uint)c;
                    }
                }

                if (catnip < 30)
                {
                    NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CATNIP_BUY_POPUP);
                    return;
                }
                param.AddField("catnip", -30);
            }

            NetworkManager.GetInstance().SendApiRequest("chore", 1, param, (response) => {

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
                    if (uri == "chore")
                    {
                        JToken resultCode = row["rs"];
                        if (resultCode != null && resultCode.Type == JTokenType.Integer)
                        {
                            int rs = resultCode.Value<int>();
                            if (rs == 0)
                            {
                                if (userCardData.GetCardData().GetResourceType() == 1)
                                {
                                    NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("Show_Fail"));
                                }
                                else
                                {
                                    Rect uv = userCardData.GetUVRect();
                                    JObject card = new JObject();
                                    card.Add("CardNo", userCardData.GetCardID());
                                    card.Add("x", uv.x);
                                    card.Add("y", uv.y);
                                    card.Add("w", uv.width);
                                    card.Add("h", uv.height);

                                    string memo = Crosstales.BWF.Manager.BadWordManager.Instance.ReplaceAll(userCardData.GetCardMemo());
                                    memo = System.Text.RegularExpressions.Regex.Replace(memo, "/\xF0[\x90-\xBF][\x80-\xBF]{2}|[\xF1 -\xF3][\x80 -\xBF]{ 3}|\xF4[\x80 -\x8F][\x80 -\xBF]{ 2}/ ", "");
                                    card.Add("memo", memo);
                                    object obj;
                                    if (userCardData.data.TryGetValue("get_time", out obj))
                                    {
                                        card.Add("date", (uint)obj);
                                    }

                                    NecoCanvas.GetUICanvas().ChatUI.OnSendCardInfo(card.ToString(Newtonsoft.Json.Formatting.None));
                                    NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("Show_Seccess"));
                                    NecoCanvas.GetPopupCanvas().OnPopupClose();

                                    NecoCanvas.GetUICanvas().OnUIShow(NecoUICanvas.UI_TYPE.MAIN_UI);
                                    NecoCanvas.GetUICanvas().OnUIShow(NecoUICanvas.UI_TYPE.TOP_INFO_UI);

                                    if (!NecoCanvas.GetUICanvas().ChatUI.IsChatUIActive())
                                        NecoCanvas.GetUICanvas().ChatUI.OnToggleChat();
                                    else
                                        NecoCanvas.GetUICanvas().ChatUI.OnShowChatUI();
                                }

                                Invoke("TopUIRefresh", 0.1f);

                                OnClose();
                                return;
                            }
                        }
                    }
                }
            });
        });
    }

    void TopUIRefresh()
    {
        NecoCanvas.GetPopupCanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
    }

    public void OnCloseButton()
    {
        if (bOnCardCanvas)
        {
            CardListPopup.OnManageStateBack();
        }
        else
        {
            OnClose();
            CardListPopup.CardDetailPopup.gameObject.SetActive(false);
            CardListPopup.gameObject.SetActive(false);
        }
    }
}
