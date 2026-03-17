using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class ShopPackageItem : MonoBehaviour
    {
        [SerializeField]
        Image bg = null;

        [SerializeField]
        Color defaultBgColor = new Color(0.2352941176470588f, 0.3215686274509804f, 0.4627450980392157f);
        [SerializeField]
        Color[] colors = new Color[6];

        [SerializeField]
        Image packItemImg = null;
        [SerializeField]
        Text itemNameText = null;
        [SerializeField]
        Text itemCountText = null;
        [SerializeField]
        Button detailBtn = null;

        [SerializeField]
        Text subscribeText = null;

        Asset itemAsset= null;

        public void SetData(Asset asset, string desc = "")
        {
            
            ItemBaseData itemData = null;

            if(subscribeText != null)
            {
                subscribeText.text = desc;
            }
            
            if(bg != null)
            {
                bg.color = defaultBgColor;
            }

            switch (asset.GoodType)
            {
                case eGoodType.GOLD:
                    itemData = ItemBaseData.Get(10000001);
                    break;
                case eGoodType.ENERGY:
                    itemData = ItemBaseData.Get(10000002);
                    break;
                case eGoodType.GEMSTONE:
                    itemData = ItemBaseData.Get(10000006);
                    break;
                case eGoodType.ARENA_TICKET:
                    itemData = ItemBaseData.Get(10000007);
                    break;
                case eGoodType.COIN:
                    itemData = ItemBaseData.Get(10000009);
                    break;
                case eGoodType.CHARACTER:
                    itemData = null;
                    int dragonNo = asset.ItemNo;
                    var dragonData = CharBaseData.Get(dragonNo);
                    itemNameText.text = StringData.GetStringByStrKey(dragonData._NAME);
                    packItemImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.CharIconPath, dragonData.THUMBNAIL);
                    itemCountText.text = StringData.GetStringByStrKey("한정캐릭터");

                    if (bg != null)
                    {
                        if(dragonData.GRADE < colors.Length)
                            bg.color = colors[dragonData.GRADE];
                    }
                    break;
                case eGoodType.PET:
                    itemData = null;
                    int petNo = asset.ItemNo;
                    var petData = PetBaseData.Get(petNo);
                    itemNameText.text = StringData.GetStringByStrKey(petData._NAME);

                    packItemImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.PetIconPath, petData.THUMBNAIL);
                    itemCountText.text = string.Empty;

                    if (bg != null)
                    {
                        if (petData.GRADE < colors.Length)
                            bg.color = colors[petData.GRADE];
                    }
                    break;
                default:
                    itemData = ItemBaseData.Get(asset.ItemNo);
                    if (itemData == null)
                        gameObject.SetActive(false);
                    break;
            }

            itemAsset = asset;

            if (itemData != null)
            {
                if (packItemImg != null)
                    packItemImg.sprite = itemData.ICON_SPRITE;
                if (itemNameText != null)
                    itemNameText.text = itemData.NAME;
                if (itemCountText != null)
                    itemCountText.text = asset.Amount.ToString();
            }
        }

        public void ShowItemToolTip()
        {
            if(itemAsset != null)
            {
                ToolTip.OnToolTip(itemAsset, packItemImg.gameObject);
            }
        }
    }
}

