using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipPopup : Popup
{
    [Header("ToolTip")]
    [SerializeField] GameObject toolTip;
    [SerializeField] Text itemName;
    [SerializeField] Text itemDes;
    [SerializeField] RectTransform rect;
    [SerializeField] GameObject multiItemDesc;

    public bool IsShow { get; private set; } = false;
    //툴팁 설명
    public void ClearDesc()
    {
        multiItemDesc.SetActive(false);

        foreach(Transform child in multiItemDesc.transform.parent)
        {
            if (child == multiItemDesc.transform)
                continue;

            Destroy(child.gameObject);
        }

        itemName.text = "";
        itemDes.text = "";
    }

    public void SetGoldDesc()
    {
        itemName.text = StringManager.GetString("gold_name");
        itemDes.text = StringManager.GetString("gold_info");
    }

    public void SetDiaDesc()
    {
        itemName.text = StringManager.GetString("dia_name");
        itemDes.text = StringManager.GetString("dia_info");
    }

    public void SetMileageDesc()
    {
        itemName.text = StringManager.GetString("mileage_name");
        itemDes.text = StringManager.GetString("mileage_info");
    }
    public void SetItemDes(ItemGameData d)
    {
        itemName.text = d.GetName();
        itemDes.text = d.GetDesc();
    }

    public void SetCharacterDesc(CharacterGameData d)
    {
        itemName.text = d.GetName();
        itemDes.text = d.GetDesc();
    }

    public void SetItemDesc(BundleInfo d)
    {
        switch (d.goods_type)
        {
            case ASSET_TYPE.GOLD:
            case ASSET_TYPE.DIA:
            case ASSET_TYPE.MILEAGE:
                SetItemDesc(d.goods_type);
                break;
            case ASSET_TYPE.ITEM:
            case ASSET_TYPE.EQUIPMENT:
            case ASSET_TYPE.BUFF_ITEM:
                SetItemDes(ItemGameData.GetItemData(d.goods_param));
                break;
            case ASSET_TYPE.CHARACTER:
                SetCharacterDesc(CharacterGameData.GetCharacterData(d.goods_param));
                break;
            default:
                ClearDesc();
                break;
        }
    }

    public void SetItemDesc(ASSET_TYPE type)
    {
        switch (type)
        {
            case ASSET_TYPE.GOLD:
                SetGoldDesc();
                break;
            case ASSET_TYPE.DIA:
                SetDiaDesc();
                break;
            case ASSET_TYPE.MILEAGE:
                SetMileageDesc();
                break;
            default:
                ClearDesc();
                break;
        }
    }

    public void Init(ASSET_TYPE type, Vector2 position)
    {
        ClearDesc();

        SetItemDesc(type);

        SetPosition(position);
    }

    public void Init(string name, string desc, Vector2 position)
    {
        ClearDesc();

        itemName.text = name;
        itemDes.text = desc;

        SetPosition(position);
    }

    public void Init(BundleInfo info, Vector2 position)
    {
        ClearDesc();
        if (info == null)
        {
            return;
        }

        SetItemDesc(info);

        SetPosition(position);
    }

    public void Init(string name, List<BundleInfo> infos, Vector2 position)
    {
        ClearDesc();
        if (infos == null || infos.Count == 0)
        {
            return;
        }
        else if (infos.Count == 1)
        {
            SetItemDesc(infos[0]);
        }
        else
        {
            foreach (BundleInfo info in infos)
            {
                AddMultipleReward(info);                
            }

            itemName.text = name;
            itemDes.text = "";
        }

        SetPosition(position);
    }

    public void SetPosition(Vector2 position)
    {
        toolTip.transform.position = position;
        Vector3 Pos = toolTip.transform.localPosition;
        Pos.y += GameConfig.Instance.TOOLTIP_OFFSET;
        Pos.z = 0.0f;
        toolTip.transform.localPosition = Pos;

        foreach(MaskableGraphic graphic in GetComponentsInChildren<MaskableGraphic>())
        {
            Color color = graphic.color;
            color.a = 0.0f;
            graphic.color = color;
        }

        IsShow = false;
        Invoke("SetTooltipLayout", 0.05f);
    }

    private void AddMultipleReward(BundleInfo info)
    {
        Sprite icon = null;
        string name = "";
        float scale = 1.0f;

        switch (info.goods_type)
        {
            case ASSET_TYPE.GOLD:
                icon = Managers.Resource.LoadAssetsBundle<Sprite>("Texture/UI/Lobby/Icon_gold");
                name = StringManager.GetString("ui_gold_count", info.goods_amount);
                break;
            case ASSET_TYPE.DIA:
                icon = Managers.Resource.LoadAssetsBundle<Sprite>("Texture/UI/Lobby/Icon_gem");
                name = StringManager.GetString("ui_dia_count", info.goods_amount);
                break;
            case ASSET_TYPE.ITEM:
                scale = 1.428571428571429f;
                ItemGameData item = ItemGameData.GetItemData(info.goods_param);
                if (item != null)
                {
                    icon = item.sprite;
                    name = string.Format("{0} {1}개", item.GetName(), info.goods_amount);
                }
                break;
            case ASSET_TYPE.CHARACTER:
                CharacterGameData charData = CharacterGameData.GetCharacterData(info.goods_param);
                if (charData != null)
                {
                    icon = charData.sprite_ui_resource;
                    name = string.Format("{0} {1}개", charData.GetName(), info.goods_amount);
                }
                break;
            default:
                break;
        }

        if (icon == null || string.IsNullOrEmpty(name))
            return;

        multiItemDesc.SetActive(true);

        GameObject multiItemRow = Instantiate(multiItemDesc);
        multiItemRow.transform.SetParent(multiItemDesc.transform.parent);
        multiItemRow.transform.localPosition = Vector3.zero;
        multiItemRow.transform.localScale = Vector3.one;

        multiItemRow.GetComponentInChildren<Text>().text = name;
        Image image = multiItemRow.GetComponentInChildren<Image>();
        image.sprite = icon;
        image.transform.localScale = new Vector3(scale, scale, 1.0f);

        multiItemDesc.SetActive(false);
    }

    void SetTooltipLayout()
    {
        CancelInvoke("SetTooltipLayout");
        rect.sizeDelta = new Vector2(
            Mathf.Max((30 + (itemName.transform as RectTransform).sizeDelta.x), (itemDes.transform as RectTransform).sizeDelta.x) + 20,
            (itemName.transform as RectTransform).sizeDelta.y + (itemDes.transform as RectTransform).sizeDelta.y + 17
        );

        Canvas curCanvas = PopupCanvas.Instance.GetComponent<Canvas>();
        if (curCanvas != null)
        {
            Vector2 canvasSize = (curCanvas.transform as RectTransform).sizeDelta;
            Vector2 minpos = (canvasSize * -0.5f) + new Vector2(rect.sizeDelta.x * 0.5f, 0);
            Vector2 maxpos = (canvasSize * 0.5f) - new Vector2(rect.sizeDelta.x * 0.5f, rect.sizeDelta.y);
            bool over_pos = false;
            if (minpos.x > toolTip.transform.localPosition.x) over_pos = true;
            else if (maxpos.x < toolTip.transform.localPosition.x) over_pos = true;
            else if (minpos.y > toolTip.transform.localPosition.y) over_pos = true;
            else if (maxpos.y < toolTip.transform.localPosition.y) over_pos = true;

            if (over_pos)
            {
                Vector3 pos = Vector3.zero;
                pos.y -= rect.sizeDelta.y * 0.5f;
                toolTip.transform.localPosition = pos;
            }
        }

        foreach (MaskableGraphic graphic in GetComponentsInChildren<MaskableGraphic>())
        {
            Color color = graphic.color;
            color.a = 1.0f;
            graphic.DOColor(color, 0.05f);
        }

        Invoke("ShowFlag", 0.3f);
    }

    public void ShowFlag()
    {
        CancelInvoke("ShowFlag");
        IsShow = true;
    }
}
