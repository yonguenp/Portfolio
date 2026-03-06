using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CONFIRM_POPUP_TYPE
{
    COMMON,
    COMMON_WITHDRAWAL,
    CONFIRM_SHOP,
    CONFIRM_FISHTRUCK,
}

public class ConfirmPopupData
{
    // MainIcon
    public Sprite contentsSprite = null;
    public string contentsCountText = "";
    public string contentsNameText = "";

    // DescText
    public string titleText = "";
    public string titleMessageText = "";
    public string messageText_1 = "";
    public string messageText_2 = "";

    public string messageText_3 = "";
    public string messageText_4 = "";

    // Icon + Amount
    public Sprite amountIcon = null;
    public string amountText = "";
}

public class SystemConfirmPanel : MonoBehaviour
{
    public delegate void Callback();
    public delegate void CountCallback(uint count);

    [Header("[Main Icon Info Layers]")]
    public GameObject contentsLayer;
    public Image contentsImage;
    public Text contentsCountText;
    public Text contentsNameText;

    [Header("[DescText Info Layers]")]
    public Text titleText;
    public Text titleMessageText;
    public Text messageText_1;
    public Text messageText_2;
    public Text messageText_3;
    public Text messageText_4;

    [Header("[Icon+Amount Info Layers]")]
    public GameObject amountInfoLayer;
    public Image amountImage;
    public Text amountCountText;

    [Header("[Withdrawal Layers]")]
    public GameObject withdrawalLayer;
    public GameObject withdrawalPopupObject;
    public GameObject withdrawalScrollObject;

    [Header("[Button Layers]")]
    public GameObject shopButton;

    [Header("[Layout List]")]
    public RectTransform layoutRect;
    public RectTransform topLayoutRect;

    [Header("[Div Line]")]
    public GameObject DivideLine_1;
    public GameObject DivideLine_2;

    ConfirmPopupData curConfirmData;
    CONFIRM_POPUP_TYPE curConfirmType;

    Callback okCallback = null;
    CountCallback countCallback = null;

    string returnCountValue = "";

    Vector2 originAmountImageSize = new Vector2(28, 28);
    Vector2 largeAmountImageSize = new Vector2(50,50);

    public void OnClickOkButton()
    {
        // 확인시 별도 처리
        // ..


        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.SYSTEM_CONFIRM_POPUP);
        okCallback?.Invoke();

        countCallback?.Invoke(uint.Parse(returnCountValue, System.Globalization.NumberStyles.AllowThousands));
    }

    public void OnClickCancelButton()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.SYSTEM_CONFIRM_POPUP);
    }

    public void OnClickShopButton()
    {
        // 상점 이동 처리
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.SYSTEM_CONFIRM_POPUP);
        NecoCanvas.GetPopupCanvas().OnShopListPopupShow();
    }

    public void OnClickWithdrawalButton()
    {
        if (withdrawalPopupObject == null || withdrawalScrollObject == null) { return; }

        withdrawalScrollObject.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        withdrawalPopupObject.SetActive(!withdrawalPopupObject.activeInHierarchy);

        LayoutRebuilder.ForceRebuildLayoutImmediate(withdrawalScrollObject.GetComponent<RectTransform>());
    }

    public void SetSystemConfirmPopup(ConfirmPopupData confirmData, CONFIRM_POPUP_TYPE confirmType = CONFIRM_POPUP_TYPE.COMMON, Callback _okCallback = null)
    {
        if (confirmData == null) { return; }

        ClearData();

        curConfirmData = confirmData;
        curConfirmType = confirmType;
        okCallback = _okCallback;

        amountImage.rectTransform.sizeDelta = originAmountImageSize;

        switch (curConfirmType)
        {
            case CONFIRM_POPUP_TYPE.COMMON:
                InitConfirmUIState();
                break;
            case CONFIRM_POPUP_TYPE.COMMON_WITHDRAWAL:
                withdrawalLayer.SetActive(true);
                DivideLine_1.SetActive(true);
                DivideLine_2.SetActive(true);
                if (confirmData.messageText_4.Length > 0)
                {
                    messageText_3.gameObject.SetActive(true);
                    messageText_3.text = confirmData.messageText_3;
                }
                    
                if(confirmData.messageText_4.Length > 0)
                {
                    messageText_4.gameObject.SetActive(true);
                    messageText_4.text = confirmData.messageText_4;
                }
                InitConfirmUIState();
                break;
            case CONFIRM_POPUP_TYPE.CONFIRM_SHOP:
                shopButton.SetActive(true);
                InitConfirmUIState();
                break;
            case CONFIRM_POPUP_TYPE.CONFIRM_FISHTRUCK:
                amountImage.rectTransform.sizeDelta = largeAmountImageSize;
                InitConfirmUIState();
                break;
        }

        StartCoroutine(RefreshLayout());
    }

    public void SetSystemCountConfirmPopup(ConfirmPopupData confirmData, string returnValue, CONFIRM_POPUP_TYPE confirmType = CONFIRM_POPUP_TYPE.COMMON, CountCallback _okCallback = null)
    {
        if (confirmData == null) { return; }

        ClearData();

        curConfirmData = confirmData;
        curConfirmType = confirmType;
        countCallback = _okCallback;

        returnCountValue = returnValue;

        amountImage.rectTransform.sizeDelta = originAmountImageSize;

        switch (curConfirmType)
        {
            case CONFIRM_POPUP_TYPE.COMMON:
                InitConfirmUIState();
                break;
            case CONFIRM_POPUP_TYPE.COMMON_WITHDRAWAL:
                withdrawalLayer.SetActive(true);
                InitConfirmUIState();
                break;
            case CONFIRM_POPUP_TYPE.CONFIRM_SHOP:
                shopButton.SetActive(true);
                InitConfirmUIState();
                break;
            case CONFIRM_POPUP_TYPE.CONFIRM_FISHTRUCK:
                amountImage.rectTransform.sizeDelta = largeAmountImageSize;
                InitConfirmUIState();
                break;
        }

        StartCoroutine(RefreshLayout());
    }

    void InitConfirmUIState()
    {
        // 설정된 데이터 Setting
        // MainIcon
        contentsImage.sprite = curConfirmData.contentsSprite;
        contentsCountText.text = curConfirmData.contentsCountText;
        contentsNameText.text = curConfirmData.contentsNameText;

        // DescText
        titleText.text = curConfirmData.titleText;
        titleMessageText.text = curConfirmData.titleMessageText;
        messageText_1.text = curConfirmData.messageText_1;

        foreach(Transform child in messageText_2.transform)
        {
            Destroy(child.gameObject);
        }

        if (curConfirmData.messageText_2 != "") 
        {
            string[] text2 = curConfirmData.messageText_2.Split('\n');
            messageText_2.text = "";

            List<GameObject> clones = new List<GameObject>();
            for (int i = 0; i < text2.Length; i++)
            {
                messageText_2.text += "\n";
                GameObject c = Instantiate(messageText_2.gameObject);
                clones.Add(c);
            }

            for (int i = 0; i < clones.Count; i++)
            {
                GameObject clone = clones[i];

                clone.transform.parent = messageText_2.transform;
                (clone.transform as RectTransform).anchorMin = new Vector2(0.5f, 1.0f);
                (clone.transform as RectTransform).anchorMax = new Vector2(0.5f, 1.0f);
                (clone.transform as RectTransform).pivot = new Vector2(0.5f, 1.0f);
                clone.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                clone.transform.localScale = Vector3.one;

                if (text2[i].Contains("<"))
                {
                    string[] spliter = text2[i].Split('<');
                    if (spliter[1].Contains(">"))
                    {
                        text2[i] = spliter[0];
                        spliter = spliter[1].Split('>');
                        uint itemID = uint.Parse(spliter[0]);
                        if (itemID > 0)
                        {
                            GameObject icon = Instantiate(Resources.Load<GameObject>("Prefabs/Neco/UI/notice_item_chance"), clone.transform);
                            icon.transform.localScale = Vector3.one;
                            icon.GetComponent<RectTransform>().anchoredPosition = new Vector3(18, 0, 0);
                            icon.GetComponent<Button>().onClick.AddListener(() => {
                                NecoCanvas.GetPopupCanvas().ShowRandomBoxInfo(itemID);
                            });
                        }
                    }
                }

                clone.GetComponent<Text>().text = text2[i];
            }
        }
        

        // Icon + Amount
        amountImage.sprite = curConfirmData.amountIcon;
        amountCountText.text = curConfirmData.amountText;



        // 설정되지 않은 레이어 off
        // MainIcon
        if (contentsImage.sprite == null) { contentsImage.gameObject.SetActive(false); }
        if (contentsCountText.text == "") { contentsCountText.gameObject.SetActive(false); }
        if (contentsNameText.text == "") { contentsNameText.gameObject.SetActive(false); }
        contentsLayer.SetActive(contentsImage.sprite != null);

        // DescText
        if (titleText.text == "") { titleText.gameObject.SetActive(false); }
        if (titleMessageText.text == "") { titleMessageText.gameObject.SetActive(false); }
        if (messageText_1.text == "") { messageText_1.gameObject.SetActive(false); }
        if (curConfirmData.messageText_2 == "") { messageText_2.gameObject.SetActive(false); }

        // Icon + Amount
        if (amountImage.sprite == null) { amountImage.gameObject.SetActive(false); }
        if (amountCountText.text == "") { amountCountText.gameObject.SetActive(false); }
        amountInfoLayer.SetActive(amountImage.sprite != null || amountCountText.text != "");
    }

    void ClearData()
    {
        shopButton.SetActive(false);
        withdrawalLayer.SetActive(false);

        // MainIcon
        contentsLayer.SetActive(true);
        contentsImage.gameObject.SetActive(true);
        contentsCountText.gameObject.SetActive(true);
        contentsNameText.gameObject.SetActive(true);

        // DescText
        titleText.gameObject.SetActive(true);
        titleMessageText.gameObject.SetActive(true);
        messageText_1.gameObject.SetActive(true);
        messageText_2.gameObject.SetActive(true);

        // Icon + Amount
        amountInfoLayer.SetActive(true);
        amountImage.gameObject.SetActive(true);
        amountCountText.gameObject.SetActive(true);

        okCallback = null;
        countCallback = null;

        withdrawalPopupObject.SetActive(false);

        DivideLine_1.SetActive(false);
        DivideLine_2.SetActive(false);
        messageText_3.gameObject.SetActive(false);
        messageText_4.gameObject.SetActive(false);

        amountImage.rectTransform.sizeDelta = originAmountImageSize;
    }

    IEnumerator RefreshLayout()
    {
        // 원인 불명.. 2프레임에 걸쳐 최소 2회 갱신해야 정상 작동함

        yield return new WaitForEndOfFrame();

        if (layoutRect != null && topLayoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
            LayoutRebuilder.ForceRebuildLayoutImmediate(topLayoutRect);
        }

        yield return new WaitForEndOfFrame();

        if (layoutRect != null && topLayoutRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
            LayoutRebuilder.ForceRebuildLayoutImmediate(topLayoutRect);
        }

        Vector2 size = (messageText_2.transform as RectTransform).sizeDelta;
        size.y += 20;   // spacing
        float perHeight = size.y / messageText_2.transform.childCount;
        for (int i = 0; i < messageText_2.transform.childCount; i++)
        {
            messageText_2.transform.GetChild(i).localPosition = new Vector3(0.0f, (size.y * 0.5f) + (perHeight * i * -1.0f), 0.0f);
        }
    }

    private void OnDisable()
    {
        foreach (Transform child in messageText_2.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
