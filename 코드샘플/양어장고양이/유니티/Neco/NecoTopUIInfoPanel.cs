using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public enum TOP_UI_PANEL_TYPE
{
    RESOURCE,
    GUIDE_QUEST,
    BOOSTER,

    ALL,
}

public enum TOP_UI_RESOURCE_TYPE
{
    GOLD,
    CAT_LEAF,
    CHEWRR,
    POINT,
    PASS_TICKET,
}

public class NecoTopUIInfoPanel : MonoBehaviour
{
    const int EVENT_ICON_INTERVAL = -80;

    [System.Serializable]
    public class TimeSaleIconClass {
        public uint id;
        public GameObject icon;        
    };
    [Header("[ResourceInfo Layer]")]
    public TopUIResourceInfoPanel resourceInfoPanel;

    [Header("[GuideQuestInfo Layer]")]
    public GuideQuest guideQuestPanel;

    [Header("[BoosterInfo Layer]")]
    public TopUIBoosterInfoPanel boosterInfoPanel;
    Dictionary<MaskableGraphic, Color> originColor = new Dictionary<MaskableGraphic, Color>();


    public TimeSaleIconClass[] TimeSaleIcon;
    
    public GameObject FirstBuyBenefitIcon;
    public GameObject SubscribeIcon;
    public GameObject RepairSubscribeIcon;
    public GameObject FishTraderIcon;
    public GameObject ChuseokIcon;
    public GameObject CatVisitCoinEffect;
    public GameObject HalloweenIcon;
    public GameObject ChristmasIcon;

    public Sprite[] CatIcon;

    List<GameObject> AttachedTimeSaleIcon = new List<GameObject>();
    List<GameObject> CoinActionEffects = new List<GameObject>();
    private void Awake()
    {
        foreach(MaskableGraphic graphic in GetComponentsInChildren<MaskableGraphic>())
        {
            originColor[graphic] = graphic.color;
        }
    }
    private void OnEnable()
    {
        if (NecoCanvas.GetUICanvas() != null)
        {
            Vector2 size = (NecoCanvas.GetUICanvas().transform as RectTransform).sizeDelta;

            float ratio = (size.x / size.y);
            float scale = Mathf.Min(1.0f, ratio * (20.0f / 9.0f));

            ResetBackgroundSize(scale);
        }

        resourceInfoPanel.Init();
        RefreshPanelData();

        CheckTimeSaleIcon();
    }

    public uint GetUserResource(TOP_UI_RESOURCE_TYPE resourceType)
    {
        RefreshPanelData(TOP_UI_PANEL_TYPE.RESOURCE);

        return resourceInfoPanel.GetUserResource(resourceType);
    }

    public void ResetBackgroundSize(float ratio)
    {
        resourceInfoPanel.transform.localScale = Vector3.one * ratio;
        guideQuestPanel.transform.localScale = Vector3.one * ratio;
        //boosterInfoPanel.transform.localScale = Vector3.one * ratio;

        guideQuestPanel.transform.localPosition = resourceInfoPanel.transform.localPosition + (new Vector3(0, -100, 0) * ratio);

        Vector3 pos = guideQuestPanel.transform.localPosition + (new Vector3(0, -100, 0) * ratio);
        foreach (GameObject icon in AttachedTimeSaleIcon)
        {
            if (icon == null) continue;

            icon.transform.localPosition = pos;
            pos.y += EVENT_ICON_INTERVAL * ratio;
        }
    }

    public void RefreshPanelData(TOP_UI_PANEL_TYPE refreshType = TOP_UI_PANEL_TYPE.ALL)
    {
        //OnShow();

        switch (refreshType)
        {
            case TOP_UI_PANEL_TYPE.RESOURCE:
                resourceInfoPanel.RefreshResourceData();
                break;
            case TOP_UI_PANEL_TYPE.GUIDE_QUEST:
                if(neco_data.GetPrologueSeq() >= neco_data.PrologueSeq.배틀패스보상받기)
                    guideQuestPanel.gameObject.SetActive(guideQuestPanel.SetGuideUI());
                break;
            case TOP_UI_PANEL_TYPE.BOOSTER:
                //boosterInfoPanel.RefreshBoosterData();
                break;
            case TOP_UI_PANEL_TYPE.ALL:
                resourceInfoPanel.RefreshResourceData();
                //boosterInfoPanel.RefreshBoosterData();
                if (neco_data.GetPrologueSeq() >= neco_data.PrologueSeq.배틀패스보상받기)
                    guideQuestPanel.gameObject.SetActive(guideQuestPanel.SetGuideUI());
                break;
        }
    }

    public void TogglePanelState(TOP_UI_PANEL_TYPE toggleType = TOP_UI_PANEL_TYPE.ALL, bool isOn = true)
    {
        //OnShow();

        switch (toggleType)
        {
            case TOP_UI_PANEL_TYPE.RESOURCE:
                resourceInfoPanel.gameObject.SetActive(isOn);
                break;
            case TOP_UI_PANEL_TYPE.GUIDE_QUEST:
                guideQuestPanel.gameObject.SetActive(isOn);
                break;
            case TOP_UI_PANEL_TYPE.BOOSTER:
                //boosterInfoPanel.gameObject.SetActive(isOn);
                break;
            case TOP_UI_PANEL_TYPE.ALL:
                resourceInfoPanel.gameObject.SetActive(isOn);
                guideQuestPanel.gameObject.SetActive(isOn);
                //boosterInfoPanel.gameObject.SetActive(isOn);
                break;
        }
    }

    public bool IsUIOpen(TOP_UI_PANEL_TYPE type)
    {
        switch (type)
        {
            case TOP_UI_PANEL_TYPE.RESOURCE:
                return resourceInfoPanel.gameObject.activeSelf;
            case TOP_UI_PANEL_TYPE.GUIDE_QUEST:
                return guideQuestPanel.gameObject.activeSelf;
            case TOP_UI_PANEL_TYPE.BOOSTER:
                return boosterInfoPanel.gameObject.activeSelf;
            case TOP_UI_PANEL_TYPE.ALL:
                return resourceInfoPanel.gameObject.activeSelf && guideQuestPanel.gameObject.activeSelf && boosterInfoPanel.gameObject.activeSelf;
            default:
                return false;
        }
    }

    public void OnHide()
    {
        foreach(KeyValuePair<MaskableGraphic, Color> child in originColor)
        {
            Color color = child.Value;
            color.a = 0.0f;
            child.Key.DOColor(color, 0.3f);
        }
    }

    public void OnShow()
    {
        foreach (KeyValuePair<MaskableGraphic, Color> child in originColor)
        {
            child.Key.color = child.Value;
        }
    }

    public void CheckTimeSaleIcon()
    {
        float ratio = guideQuestPanel.transform.localScale.y;
        Vector3 pos = guideQuestPanel.transform.localPosition + (new Vector3(0, -100, 0) * ratio);

        foreach (GameObject ic in AttachedTimeSaleIcon)
        {
            Destroy(ic);
        }
        AttachedTimeSaleIcon.Clear();

        if (neco_data.PrologueSeq.프리플레이 > neco_data.GetPrologueSeq())
            return;

        if (ChuseokIcon != null)
        {
            chuseok_event eventData = null;
            foreach (neco_event evt in neco_data.Instance.GetEvents())
            {
                if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.CHUSEOK)
                    eventData = (chuseok_event)evt;
            }

            if (eventData != null)
            {
                if (eventData.IsEventTime())
                {
                    GameObject icon = Instantiate(ChuseokIcon);
                    icon.transform.SetParent(transform);
                    icon.transform.localPosition = pos;
                    pos.y += EVENT_ICON_INTERVAL * ratio;
                    icon.transform.localScale = guideQuestPanel.transform.localScale;
                    AttachedTimeSaleIcon.Add(icon);
                }
            }
        }

        if (HalloweenIcon != null)
        {
            halloween_event eventData = null;
            foreach (neco_event evt in neco_data.Instance.GetEvents())
            {
                if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.HALLOWEEN)
                    eventData = (halloween_event)evt;
            }

            if (eventData != null)
            {
                if (eventData.IsEventTime())
                {
                    GameObject icon = Instantiate(HalloweenIcon);
                    icon.transform.SetParent(transform);
                    icon.transform.localPosition = pos;
                    pos.y += EVENT_ICON_INTERVAL * ratio;
                    icon.transform.localScale = guideQuestPanel.transform.localScale;
                    AttachedTimeSaleIcon.Add(icon);
                }
            }
        }

        if (ChristmasIcon != null)
        {
            christmas_event eventData = null;
            foreach (neco_event evt in neco_data.Instance.GetEvents())
            {
                if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.CHRISTMAS)
                    eventData = (christmas_event)evt;
            }

            if (eventData != null)
            {
                if (eventData.IsEventTime())
                {
                    GameObject icon = Instantiate(ChristmasIcon);
                    icon.transform.SetParent(transform);
                    icon.transform.localPosition = pos;
                    pos.y += EVENT_ICON_INTERVAL * ratio;
                    icon.transform.localScale = guideQuestPanel.transform.localScale;
                    AttachedTimeSaleIcon.Add(icon);
                    }
                }
            }

        if (FishTraderIcon != null)
        {
            DateTime curTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(NecoCanvas.GetCurTime()).ToLocalTime();
            DateTime startTime = neco_data.Instance.GetFishtruckDateTime(true);
            DateTime endTime = neco_data.Instance.GetFishtruckDateTime(false);

            if (curTime > startTime && curTime < endTime)
            {
                GameObject icon = Instantiate(FishTraderIcon);
                icon.transform.SetParent(transform);
                icon.transform.localPosition = pos;
                pos.y += EVENT_ICON_INTERVAL * ratio;
                icon.transform.localScale = guideQuestPanel.transform.localScale;
                AttachedTimeSaleIcon.Add(icon);
            }
        }

        if (TimeSaleIcon != null)
        {
            List<neco_shop> timesaleList = neco_shop.GetNecoShopListByType("timesale");
            foreach (neco_shop shopData in timesaleList)
            {
                if (neco_data.Instance.GetTimeSaleProductRemain(shopData.GetNecoShopID()) < NecoCanvas.GetCurTime())
                {
                    continue;
                }

                TimeSaleIconClass targetTimeSale = null;
                foreach (TimeSaleIconClass ts in TimeSaleIcon)
                {
                    if(ts.id == shopData.GetNecoShopID())
                    {
                        targetTimeSale = ts;
                    }
                }

                if (targetTimeSale == null)
                    continue;

                GameObject icon = Instantiate(targetTimeSale.icon);
                icon.transform.SetParent(transform);
                icon.transform.localPosition = pos;
                pos.y += EVENT_ICON_INTERVAL * ratio;
                icon.transform.localScale = guideQuestPanel.transform.localScale;
                icon.GetComponent<TimeSaleIcon>().SetTimeOver(shopData.GetNecoShopID());
                AttachedTimeSaleIcon.Add(icon);
            }            
        }

        if (FirstBuyBenefitIcon != null)
        {
            if (neco_data.Instance.IsBenefitEnable())
            {
                GameObject icon = Instantiate(FirstBuyBenefitIcon);
                icon.transform.SetParent(transform);
                icon.transform.localPosition = pos;
                pos.y += EVENT_ICON_INTERVAL * ratio;
                icon.transform.localScale = guideQuestPanel.transform.localScale;
                AttachedTimeSaleIcon.Add(icon);
            }
        }

        foreach (neco_subscribe_data subs in neco_data.Instance.GetSubscribe())
        {
            if (subs.enable_recive)
            {
                GameObject target = null;

                switch (subs.prod_id)
                {
                    case 38:
                        target = SubscribeIcon;
                        break;
                    case 46:
                        target = RepairSubscribeIcon;
                        break;
                    default:
                        continue;
                }

                if (target == null)
                    continue;

                GameObject icon = Instantiate(target);
                icon.transform.SetParent(transform);
                icon.transform.localPosition = pos;
                pos.y += EVENT_ICON_INTERVAL * ratio;
                icon.transform.localScale = guideQuestPanel.transform.localScale;
                icon.GetComponent<SubscribeIcon>().SetIcon(subs);
                AttachedTimeSaleIcon.Add(icon);
            }
        }
    }

    [ContextMenu("OnCatVisitCoin")]
    public void OnCatVisitCoin(uint catid, uint amount)
    {
        if (amount <= 0)
            return;

        if (CatVisitCoinEffect == null)
            return;

        if (resourceInfoPanel.goldLayerObject.activeInHierarchy == false)
            return;
        if (CatIcon.Length <= catid || CatIcon[catid] == null)
            return;


        GameObject coin = Instantiate(CatVisitCoinEffect);
        coin.transform.SetParent(resourceInfoPanel.goldLayerObject.transform);
        
        coin.transform.Find("Text").GetComponent<Text>().text = "+" + amount.ToString();
        coin.transform.Find("Image").GetComponent<Image>().sprite = CatIcon[catid];
        float delaytime = 0.0f;
        if (CoinActionEffects.Count > 0)
        {
            Vector2 bound = (resourceInfoPanel.goldLayerObject.transform as RectTransform).sizeDelta;
            bound /= 2;

            coin.transform.localPosition = new Vector3(UnityEngine.Random.Range(-bound.x, bound.x), UnityEngine.Random.Range(-bound.y, bound.y), 0);
            delaytime = CoinActionEffects.Count * 0.1f;
        }
        else
        {
            coin.transform.localPosition = CatVisitCoinEffect.transform.localPosition;
        }

        CoinActionEffects.Add(coin);
        coin.transform.localScale = new Vector3(1.0f, 0.0f, 1.0f);
        coin.SetActive(true);

        coin.transform.DOScaleY(1.0f, 0.25f).SetDelay(delaytime);
        coin.transform.DOLocalMoveY(19, 0.25f).SetDelay(0.75f + delaytime).OnComplete(() => {
            CoinActionEffects.Remove(coin);
            Destroy(coin);            
        });

        coin.GetComponent<Image>().DOColor(new Color(1, 1, 1, 0), 1.0f).SetDelay(delaytime);
    }
}
