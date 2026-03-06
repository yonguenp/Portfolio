using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PackageBuyPopup : Popup
{
    public enum PACKAGE_POPUP_TYPE
    {
        NORMAL,
        DESC,
        BANNER,
        BANNER_AND_LIST,
        BANNER_AND_DESC,
    };

    [SerializeField]
    GameObject[] BackgroundType;
    [SerializeField]
    GameObject[] BotUI;

    [SerializeField]
    Text[] ShopItemName;
    [SerializeField]
    Text[] TermsText;
    [SerializeField]
    Text[] MailNotification;
    [SerializeField]
    Text[] NeedPrice;
    [SerializeField]
    Image[] NeedPriceIcon;

    [SerializeField]
    Text[] wonText;

    [SerializeField]
    UIBundleItem bundle;
    [SerializeField]
    GameObject botPanel;
    [SerializeField]
    Text countText;
    [SerializeField]
    Text timeText;

    [SerializeField]
    Text descText;

    [SerializeField]
    Image Banner;

    ShopItemGameData shopItem = null;
    protected BuyPopup.BuyCallback buyCallback = null;

    public void Init(PopupDefine sp, ShopItemGameData s, BuyPopup.BuyCallback cb, PACKAGE_POPUP_TYPE type)
    {
        shopItem = s;
        buyCallback = cb;
        sp.ApplySprite(Banner);
        Text priceText = null;
        wonText[0].text = string.Empty;
        wonText[1].text = string.Empty;
        Image priceIcon = null;
        switch (type)
        {
            case PACKAGE_POPUP_TYPE.BANNER:
                BackgroundType[0].SetActive(true);
                BackgroundType[1].SetActive(false);
                BotUI[0].SetActive(false);
                BotUI[1].SetActive(false);
                ShopItemName[0].text = s.GetName();
                TermsText[0].text = s.GetTerms();
                MailNotification[0].gameObject.SetActive(s.rewards.Count > 0);
                MailNotification[0].text = StringManager.GetString("shop_goods:mail:" + s.GetID());

                priceText = NeedPrice[0];
                priceIcon = NeedPriceIcon[0];
                break;
            case PACKAGE_POPUP_TYPE.BANNER_AND_LIST:
                BackgroundType[0].SetActive(false);
                BackgroundType[1].SetActive(true);
                BotUI[0].SetActive(true);
                BotUI[1].SetActive(false);
                ShopItemName[1].text = s.GetName();
                TermsText[1].text = s.GetTerms();

                MailNotification[2].gameObject.SetActive(s.rewards.Count > 0);
                MailNotification[2].text = StringManager.GetString("shop_goods:mail:" + s.GetID());
                
                foreach (Transform child in bundle.transform.parent)
                {
                    if (child != bundle.transform)
                        Destroy(child.gameObject);
                }
                bundle.gameObject.SetActive(true);
                foreach (ShopPackageGameData reward in shopItem.rewards)
                {
                    GameObject multiItemRow = Instantiate(bundle.gameObject);
                    multiItemRow.transform.SetParent(bundle.transform.parent);
                    multiItemRow.transform.localPosition = Vector3.zero;
                    multiItemRow.transform.localScale = Vector3.one;

                    UIBundleItem item = multiItemRow.GetComponent<UIBundleItem>();
                    item.SetReward(reward);
                }
                bundle.gameObject.SetActive(false);
                priceText = NeedPrice[1];
                priceIcon = NeedPriceIcon[1];
                break;
            case PACKAGE_POPUP_TYPE.BANNER_AND_DESC:
                BackgroundType[0].SetActive(false);
                BackgroundType[1].SetActive(true);
                BotUI[0].SetActive(false);
                BotUI[1].SetActive(true);
                ShopItemName[1].text = s.GetName();
                TermsText[1].text = s.GetTerms();
                descText.text = s.GetDesc();

                MailNotification[1].gameObject.SetActive(s.rewards.Count > 0);
                MailNotification[1].text = StringManager.GetString("shop_goods:mail:" + s.GetID());

                priceText = NeedPrice[1];
                priceIcon = NeedPriceIcon[1];
                break;
        }

        countText.text = StringManager.GetString("구매가능횟수") + (s.buyLimit - Managers.UserData.GetMyShopHistory(s.GetID())).ToString() + "/" + (s.buyLimit).ToString();
        countText.gameObject.SetActive(s.buyLimit > 0);

        priceIcon.gameObject.SetActive(shopItem.price.priceIcon != null);
        priceIcon.sprite = shopItem.price.priceIcon;
        if (shopItem.price.type == ASSET_TYPE.CASH)
        {
            priceText.text = shopItem.price.amount.ToString("N0");
            wonText[0].text = StringManager.GetString("ui_money_type_kr");
            wonText[1].text = StringManager.GetString("ui_money_type_kr");
        }
        else if (shopItem.price.type == ASSET_TYPE.ADVERTISEMENT)
        {
            bool enable = true;
            string remain = "";
            DateTime pivot = System.DateTime.MaxValue;
            DateTime ableTime = pivot;
            switch (shopItem.GetID())
            {
                //case 1001:
                //    pivot = Managers.UserData.ADSeen_PACK1;
                //    if (pivot < System.DateTime.MaxValue)
                //        ableTime = pivot.AddDays(1);

                //    enable = enable && pivot < System.DateTime.MaxValue && SBCommonLib.SBUtil.KoreanTime > ableTime;
                //    break;
                //case 1002:
                //    pivot = Managers.UserData.ADSeen_PACK2;
                //    if (pivot < System.DateTime.MaxValue)
                //        ableTime = pivot.AddDays(1);

                //    enable = enable && pivot < System.DateTime.MaxValue && SBCommonLib.SBUtil.KoreanTime > ableTime;
                //    break;
                //case 1003:
                //    pivot = Managers.UserData.ADSeen_PACK3;
                //    if (pivot < System.DateTime.MaxValue)
                //        ableTime = pivot.AddMonths(1);

                //    enable = enable && pivot < System.DateTime.MaxValue && SBCommonLib.SBUtil.KoreanTime > ableTime;
                //    break;
                default:
                    enable = true;
                    break;
            }

            if (enable)
            {
                remain = StringManager.GetString("광고시청");
            }
            else
            {
                if (pivot == System.DateTime.MaxValue)
                {
                    remain = StringManager.GetString("이용불가");
                }
                else
                {
                    TimeSpan diff = ableTime - SBCommonLib.SBUtil.KoreanTime;
                    if (diff.TotalDays >= 1.0f)
                    {
                        remain = StringManager.GetString("ui_day", (int)diff.TotalDays);
                    }
                    else if (diff.TotalHours >= 1.0f)
                    {
                        remain = StringManager.GetString("ui_hour", (int)diff.TotalHours);
                    }
                    else if (diff.TotalMinutes >= 1.0f)
                    {
                        remain = StringManager.GetString("ui_min", (int)diff.TotalMinutes);
                    }
                    else if (diff.TotalSeconds >= 1.0f)
                    {
                        remain = StringManager.GetString("ui_second", (int)diff.TotalSeconds);
                    }

                    remain = StringManager.GetString("ui_left_time", remain);
                }

                buyCallback = (res) =>
                {
                    PopupCanvas.Instance.ShowFadeText("이용불가");
                };
            }

            priceText.color = enable ? (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.SHOP_POPUP) as ShopPopup).GetAbleColor() : (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.SHOP_POPUP) as ShopPopup).GetDisableColor();
            priceText.text = remain;
        }
        else
        {
            priceText.text = shopItem.price.amount.ToString("N0");
        }

        string remainText = StringManager.GetString("구매불가");
        if (shopItem.endTime > SBCommonLib.SBUtil.KoreanTime)
        {
            if (shopItem.endTime == DateTime.MaxValue)
            {
                remainText = "";
            }
            else
            {
                var diff = shopItem.endTime - SBCommonLib.SBUtil.KoreanTime;
                if (diff.Days >= 1.0f)
                {
                    remainText = StringManager.GetString("ui_day", diff.Days.ToString()) + " " + StringManager.GetString("ui_hour", diff.Hours.ToString());
                }
                else
                {
                    remainText = StringManager.GetString("ui_hour", diff.Hours.ToString()) + " " + StringManager.GetString("ui_min", diff.Minutes.ToString());
                }
            }
        }

        botPanel.SetActive(s.buyLimit > 0 || shopItem.endTime != System.DateTime.MaxValue);

        timeText.text = remainText;
    }

    public override void Close()
    {
        base.Close();
    }

    public void OnBuy()
    {
        buyCallback?.Invoke(1);

        gameObject.SetActive(false);
        transform.SetAsFirstSibling();

        PopupCanvas.Instance.OnClosedPopup(GetPopupType());
        if (PopupCanvas.Instance.PopupEscList.Contains(this))
        {
            PopupCanvas.Instance.PopupEscList.Remove(this);
        }
    }
}

public class PopupDefine : GameData
{
    public int type { get; private set; }
    public string resource_path { get; private set; }
    public Sprite resource { get; private set; }

    private Coroutine resourceSyncCoroutine = null;
    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        type = Int(data["type"]);
        resource_path = data["resource"];

        if (resource == null)
        {
            resource = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/icon_loading");
        }
    }

    static public PopupDefine GetData(int uid)
    {
        foreach (PopupDefine d in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.popup_define))
        {
            if (d.GetID() == uid)
                return d;
        }

        return null;
    }

    public void ApplySprite(Image target)
    {
        if (!string.IsNullOrEmpty(resource_path))
        {
            if (resourceSyncCoroutine == null)
            {
                resourceSyncCoroutine = Managers.Instance.StartCoroutine(ResourceSync(target));
            }
            return;
        }

        target.sprite = resource;
    }

    System.Collections.IEnumerator ResourceSync(Image target)
    {
        if (!target.IsDestroyed())
        {
            target.sprite = null;
            Color color = Color.white;
            color.a = 0.0f;
            target.color = color;
        }

        if (File.Exists(ClientConstants.AssetBundleDownloadPath + "popups/" + resource_path + ".png"))
        {
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(File.ReadAllBytes(ClientConstants.AssetBundleDownloadPath + "popups/" + resource_path + ".png"));

            Rect rect = new Rect(0, 0, texture.width, texture.height);
            resource = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));

            resource_path = "";
        }
        else
        {
            UnityEngine.Networking.UnityWebRequest wr = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(SBWeb.CDN_URL + "popups/" + resource_path + ".png");

            UnityEngine.Networking.DownloadHandlerTexture texDI = new UnityEngine.Networking.DownloadHandlerTexture(true);
            wr.downloadHandler = texDI;

            yield return wr.SendWebRequest();

            if (wr.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                SBDebug.LogError("팝업 이미지 다운로드 오류!!!!!!!!!!!!!!! : " + "popups/" + resource_path + ".png");
            }
            else
            {
                if (!Directory.Exists(ClientConstants.AssetBundleDownloadPath))
                {
                    Directory.CreateDirectory(ClientConstants.AssetBundleDownloadPath);
                }

                if (!Directory.Exists(ClientConstants.AssetBundleDownloadPath + "popups/"))
                {
                    Directory.CreateDirectory(ClientConstants.AssetBundleDownloadPath + "popups/");
                }

                string[] folders = resource_path.Split('/');
                string folder = "";
                for (int i = 0; i < folders.Length - 1; i++)
                {
                    folder += folders[i] + "/";
                    if (!Directory.Exists(ClientConstants.AssetBundleDownloadPath + "popups/" + folder))
                    {
                        Directory.CreateDirectory(ClientConstants.AssetBundleDownloadPath + "popups/" + folder);
                    }
                }

                File.WriteAllBytes(ClientConstants.AssetBundleDownloadPath + "popups/" + resource_path + ".png", texDI.data);

                Rect rect = new Rect(0, 0, texDI.texture.width, texDI.texture.height);
                resource = Sprite.Create(texDI.texture, rect, new Vector2(0.5f, 0.5f));
            }
        }

        if (!target.IsDestroyed())
        {
            target.sprite = resource;
            target.color = Color.white;
        }

        resourceSyncCoroutine = null;
    }
}
