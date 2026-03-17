using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static SBWeb;

public class BundleInfo
{
    public ASSET_TYPE goods_type;
    public int goods_param;
    public int goods_amount;

    public BundleInfo(ASSET_TYPE type, int param, int amount)
    {
        goods_type = type;
        goods_param = param;
        goods_amount = amount;
    }
}

public class UIBundleItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    class TooltipData
    {
        public string title;
        public string desc;

        public TooltipData(string t, string d)
        {
            title = t;
            desc = d;
        }
    }
    [SerializeField] Image Background;
    [SerializeField] Image character;
    [SerializeField] Image icon;
    [SerializeField] Text amount;
    [SerializeField] protected GameObject Checker;
    [SerializeField] protected GameObject multiItemDetailPopupButton;
    [SerializeField] Text name;
    [SerializeField] GameObject dim;
    [SerializeField] GameObject lockIcon;
    List<BundleInfo> bundleInfos = new List<BundleInfo>();
    string packageName = "";

    public bool toolTipOption = true; // 아이템 툴팁 기능 x

    TooltipData tooltipData = null;
    Action touchEvent = null;

    Dictionary<MaskableGraphic, Color> originColors = null;
    public void Clear()
    {
        SetChecker(false);
        if (character != null)
            character.gameObject.SetActive(false);
        icon.gameObject.SetActive(false);
        multiItemDetailPopupButton.SetActive(false);
        if (amount != null)
            amount.text = "";
        if (name != null)
            name.text = "";
        if (dim != null)
            dim.SetActive(false);
        if (lockIcon != null)
            lockIcon.SetActive(false);

        bundleInfos.Clear();

        RefreshBackground();

        touchEvent = null;
    }

    void RefreshBackground()
    {
        if (Background != null)
        {
            string resourcePath = string.Empty;
            int grade = 0;

            if (bundleInfos.Count > 0)
            {
                var target = bundleInfos[0];
                if (target != null)
                {
                    switch (target.goods_type)
                    {
                        case ASSET_TYPE.ITEM:
                        case ASSET_TYPE.EQUIPMENT:
                            var id = ItemGameData.GetItemData(target.goods_param);
                            if (id != null)
                                grade = id.grade;
                            break;
                        case ASSET_TYPE.CHARACTER:
                            var cd = CharacterGameData.GetCharacterData(target.goods_param);
                            if (cd != null)
                                grade = cd.char_grade;
                            break;
                    }
                }
            }

            resourcePath = "Texture/UI/quest/item_frame_01";
            if (bundleInfos.Count > 0)
            {
                var target = bundleInfos[0];
                //아이템의 경우 기본 번들 아이템
                if (target.goods_type == ASSET_TYPE.EQUIPMENT || target.goods_type == ASSET_TYPE.CHARACTER)
                {
                    switch (grade)
                    {
                        case 2:
                            resourcePath = "Texture/UI/equip/bg_equipment_b";
                            break;
                        case 3:
                            resourcePath = "Texture/UI/equip/bg_equipment_a";
                            break;
                        case 4:
                            resourcePath = "Texture/UI/equip/bg_equipment_s";
                            break;
                        case 1:
                            resourcePath = "Texture/UI/equip/bg_equipment_c";
                            break;
                        default:
                            resourcePath = "Texture/UI/quest/item_frame_01";
                            break;
                    }
                }
            }
            Background.sprite = Managers.Resource.Load<Sprite>(resourcePath);
        }
    }
    protected virtual void Init()
    {
        Clear();
    }

    public void SetRewards(List<ShopPackageGameData> rewards, bool check = false, Action cb = null)
    {
        Clear();

        if (rewards == null || rewards.Count == 0)
            return;

        if (rewards.Count == 1 && rewards[0].goods_type != (int)ASSET_TYPE.CHARACTER)
        {
            SetIcon(rewards[0].GetIcon(), rewards[0].goods_amount > 1 ? rewards[0].goods_amount.ToString() : "");
        }
        else if (rewards.Count == 1 && rewards[0].goods_type == (int)ASSET_TYPE.CHARACTER)
        {
            SetCharacterInfo(rewards[0].GetParam());
        }
        else
        {
            SetIcon(Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/RandomBox/icon_box_01"));
            multiItemDetailPopupButton.SetActive(true);
        }

        bundleInfos.Clear();

        foreach (ShopPackageGameData reward in rewards)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                packageName = StringManager.GetString(string.Format("package:name:{0}", reward.goods_id));
            }

            bundleInfos.Add(new BundleInfo((ASSET_TYPE)reward.goods_type, reward.GetParam(), reward.goods_amount));
        }

        SetChecker(check);

        touchEvent = cb;

        RefreshBackground();
    }
    public void SetReward(ShopPackageGameData reward)
    {
        if (reward == null)
            return;

        SetIcon(reward.GetIcon(), reward.goods_amount > 1 ? reward.goods_amount.ToString() : "");

        bundleInfos.Clear();
        bundleInfos.Add(new BundleInfo((ASSET_TYPE)reward.goods_type, reward.GetParam(), reward.goods_amount));

        RefreshBackground();
    }

    public void SetReward(BundleInfo reward)
    {
        if (reward == null)
            return;

        Sprite iconSprite = null;
        switch (reward.goods_type)
        {
            case ASSET_TYPE.GOLD:
                iconSprite = Managers.Resource.LoadAssetsBundle<Sprite>("Texture/UI/Lobby/Icon_gold");
                break;
            case ASSET_TYPE.DIA:
                iconSprite = Managers.Resource.LoadAssetsBundle<Sprite>("Texture/UI/Lobby/Icon_gem");
                break;
            case ASSET_TYPE.ITEM:
                ItemGameData targetItem = ItemGameData.GetItemData(reward.goods_param);
                iconSprite = targetItem.sprite;
                break;
            case ASSET_TYPE.CHARACTER:
                CharacterGameData targetCharacter = CharacterGameData.GetCharacterData(reward.goods_param);
                iconSprite = targetCharacter.sprite_ui_resource;
                break;
            default:
                break;
        }

        SetIcon(iconSprite, reward.goods_amount > 1 ? reward.goods_amount.ToString() : "");

        bundleInfos.Clear();
        bundleInfos.Add(new BundleInfo((ASSET_TYPE)reward.goods_type, reward.goods_param, reward.goods_amount));

        RefreshBackground();
    }

    public void SetReward(ResponseReward reward)
    {
        if (reward == null)
            return;

        CollectionBuff buff = null;

        Sprite iconSprite = null;
        switch (reward.Type)
        {
            case ResponseReward.RandomRewardType.GOLD:
                iconSprite = Managers.Resource.LoadAssetsBundle<Sprite>("Texture/UI/Lobby/Icon_gold");
                break;
            case ResponseReward.RandomRewardType.DIA:
                iconSprite = Managers.Resource.LoadAssetsBundle<Sprite>("Texture/UI/Lobby/Icon_gem");
                break;
            case ResponseReward.RandomRewardType.ITEM:
            case ResponseReward.RandomRewardType.EQUIP:
                ItemGameData targetItem = ItemGameData.GetItemData(reward.Id);
                iconSprite = targetItem.sprite;

                if (targetItem.type == ItemGameData.ITEM_TYPE.BUFF_ITEM)
                    buff = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.collection_buff, targetItem.GetID()) as CollectionBuff;
                break;
            case ResponseReward.RandomRewardType.CHARACTER:
                CharacterGameData targetCharacter = CharacterGameData.GetCharacterData(reward.Id);
                iconSprite = targetCharacter.sprite_ui_resource;
                break;
            default:
                break;
        }

        if (buff != null)
        {
            SetIcon(iconSprite, "+" + buff.GetValueString(reward.Amount));
        }
        else
        {
            SetIcon(iconSprite, reward.Amount > 1 ? reward.Amount.ToString() : "");
        }

        bundleInfos.Clear();
        bundleInfos.Add(new BundleInfo((ASSET_TYPE)reward.Type, reward.Id, reward.Amount));

        RefreshBackground();
    }

    public virtual void SetCharacterInfo(int char_id, bool check = false)
    {
        Init();

        if (character != null)
        {
            character.gameObject.SetActive(true);
            character.sprite = CharacterGameData.GetCharacterData(char_id).sprite_ui_resource;
        }

        bundleInfos.Clear();
        bundleInfos.Add(new BundleInfo(ASSET_TYPE.CHARACTER, char_id, 1));

        SetChecker(check);

        RefreshBackground();
    }

    public void SetItem(ItemGameData item, int amount)
    {
        if (item == null)
            return;

        SetIcon(item.sprite, amount > 1 ? amount.ToString() : "");
        if (name != null)
            name.text = item.GetName();

        bundleInfos.Clear();
        bundleInfos.Add(new BundleInfo(ASSET_TYPE.ITEM, item.GetID(), amount));

        RefreshBackground();
    }

    public void SetNeedItem(ItemGameData item, int mine, int need)
    {
        if (item == null)
            return;

        string amountText = mine < need ? $"<color=red>{mine.ToString()}</color>" + "/" + need
            : $"<color=white>{mine.ToString()}</color>" + "/" + need;

        SetIcon(item.sprite, amountText);

        if (dim != null)
        {
            if (mine < need)
                dim.SetActive(true);
            else
                dim.SetActive(false);
        }
        bundleInfos.Clear();
        bundleInfos.Add(new BundleInfo(ASSET_TYPE.ITEM, item.GetID(), need));

        RefreshBackground();
    }
    public void SetQuestIcon(QuestData data)
    {
        SetIcon(data.resource);

        tooltipData = new TooltipData(data.GetName(), data.GetDesc());
    }

    public void SetShopItem(ShopItemGameData item, Action cb = null)
    {
        if (item.rewards.Count == 1 && item.rewards[0].GetIcon() == item.resource && item.rewards[0].goods_amount > 1)
        {
            SetIcon(item.resource, item.rewards[0].goods_amount.ToString());
        }
        else
        {
            SetIcon(item.resource);
        }

        if(!string.IsNullOrEmpty(item.resource_path))
        {
            item.ApplySprite(icon);
        }

        bundleInfos.Clear();
        foreach (var rew in item.rewards)
        {
            bundleInfos.Add(new BundleInfo((ASSET_TYPE)rew.goods_type, rew.GetParam(), rew.goods_amount));
        }

        RefreshBackground();
        packageName = item.GetName();

        if (item.price.type == ASSET_TYPE.ADVERTISEMENT)
            toolTipOption = false;

        touchEvent = cb;
    }
    public void SetIcon(Sprite sprite, string _amount = "")
    {
        Init();
        icon.gameObject.SetActive(true);
        icon.sprite = sprite;
        amount.text = _amount;
        if (icon.sprite != null && (icon.sprite.name == "Icon_gold" || icon.sprite.name == "icon_gem" || icon.sprite.name == "icon_soul_stone"))
            icon.transform.localScale = Vector2.one * 0.7f;
        else
            icon.transform.localScale = Vector2.one;
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (originColors == null)
        {
            originColors = new Dictionary<MaskableGraphic, Color>();
            foreach (MaskableGraphic graphic in GetComponentsInChildren<MaskableGraphic>())
            {
                Color ogirin = graphic.color;
                originColors.Add(graphic, ogirin);
                ogirin = ogirin * 0.7843137f;
                ogirin.a = graphic.color.a;
                graphic.color = ogirin;
            }
        }
    }
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (!eventData.dragging && eventData.pointerCurrentRaycast.gameObject != null && CheckTarget(transform, eventData.pointerCurrentRaycast.gameObject.transform))
        {
            OnButton();
        }

        if (originColors != null)
        {
            foreach (MaskableGraphic graphic in originColors.Keys)
            {
                graphic.color = originColors[graphic];
            }

            originColors.Clear();
            originColors = null;
        }
    }
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.TOOLTIP_POPUP))
        {
            PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.TOOLTIP_POPUP);
            return;
        }

        if (tooltipData == null && bundleInfos.Count == 0)
            return;

        if (!toolTipOption)
            return;

        if (tooltipData != null)
        {
            PopupCanvas.Instance.ShowTooltip(tooltipData.title, tooltipData.desc, transform.position);
        }
        else
        {
            if (bundleInfos.Count == 1)
                PopupCanvas.Instance.ShowTooltip(bundleInfos[0], transform.position);
            else
                PopupCanvas.Instance.ShowTooltip(packageName, bundleInfos, transform.position);
        }
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.TOOLTIP_POPUP);
    }

    protected virtual void OnButton()
    {
        if (touchEvent != null)
            touchEvent.Invoke();
    }

    protected bool CheckTarget(Transform check, Transform target)
    {
        foreach (Transform child in check.transform)
        {
            if (child == target)
            {
                return true;
            }

            if (CheckTarget(child, target))
            {
                return true;
            }
        }

        return false;
    }

    public void OnDetailIcon()
    {
        PopupCanvas.Instance.ShowItemInfo(packageName, bundleInfos);
    }

    public virtual void SetScale(float scale)
    {
        if (character != null)
            character.transform.localScale = Vector3.one * scale;
        icon.transform.localScale = Vector3.one * scale;
        //amount.transform.localScale = Vector3.one * scale;
    }

    public void SetBundleInfos(List<BundleInfo> infos, string packageName = "")
    {
        bundleInfos.Clear();
        bundleInfos = infos;
        this.packageName = packageName;
    }

    public RectTransform GetcheckerRect()
    {
        return Checker.transform as RectTransform;
    }

    public void SetTouchCallback(Action cb)
    {
        touchEvent = cb;
    }

    public void SetChecker(bool check)
    {
        if (Checker != null)
            Checker.SetActive(check);
    }

    public List<BundleInfo> GetBundleInfos()
    {
        return bundleInfos;
    }

    public void SetLock(bool lockState)
    {
        if (lockIcon != null)
            lockIcon.SetActive(lockState);
    }

    public void SetDim(bool dimState)
    {
        if (dim != null)
            dim.SetActive(dimState);
    }
}
