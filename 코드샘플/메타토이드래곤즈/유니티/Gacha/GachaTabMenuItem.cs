using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    [System.Serializable]
    public struct GachaTabButtonColor
    {
        public Color innerColor;
        public Color outerColor;
    }

    public class GachaTabMenuItem : MonoBehaviour
    {
        //const float WIDTH_SIZE = 480.0f;

        [SerializeField] Image tabOuterBGImage = null;
        [SerializeField] Image tabInnerBGImage = null;
        [SerializeField] Image tabIconImage = null;
        [SerializeField] Text tabMainText = null;
        [SerializeField] Text tabSubText = null;

        [Header("[BG Colors]")]
        [SerializeField] GachaTabButtonColor selectedTabColor;
        [SerializeField] GachaTabButtonColor dragonTabColor;
        [SerializeField] GachaTabButtonColor petTabColor;
        [SerializeField] GachaTabButtonColor luckyBoxTabColor;

        [Space(10)]
        [SerializeField] Color textDisableColor;

        GachaUIController parentController = null;

        public GachaGroupData CurrentGroupData { get; private set; } = null;

        RectTransform imageRectTransform = null;
        [SerializeField] GameObject Reddot = null;

        public void InitTabMenu(GachaGroupData groupData, GachaUIController parent)
        {
            imageRectTransform = tabIconImage.gameObject.GetComponent<RectTransform>();

            CurrentGroupData = groupData;
            parentController = parent;

            if(!string.IsNullOrEmpty(CurrentGroupData.resource))
                CDNManager.SetBanner(CurrentGroupData.resource, tabIconImage);
            else
                tabIconImage.sprite = CurrentGroupData.sprite;
            //float spriteRate = WIDTH_SIZE / currentGroupData.sprite.bounds.size.x;
            //imageRectTransform.sizeDelta = new Vector2(WIDTH_SIZE, currentGroupData.sprite.bounds.size.y * spriteRate);

            //tabMainText.text = currentGroupData.Name;
            tabSubText.text = CurrentGroupData.Name;

            SetSelectedState();

            CheckReddot();
        }

        public void CheckReddot()
        {
            bool hasPossible = false;
            foreach (var menu in CurrentGroupData.GetGachaMenus())
            {
                foreach (var type in menu.typeDatas)
                {
                    switch (type.price_type)
                    {
                        case eGoodType.ITEM:
                            if (User.Instance.GetItemCount(type.price_uid) >= type.price_value)
                                hasPossible = true;
                            break;
                        case eGoodType.ADVERTISEMENT:
                            if (ShopManager.Instance.GetAdvertiseState(type.price_uid).IS_VALIDE)
                                hasPossible = true;
                            break;
                        default://다이아 골드등 재화 소모형은 빨콩 제외?
                            break;
                    }

                    if (hasPossible)
                        break;
                }

                if (hasPossible)
                    break;
            }

            Reddot.SetActive(hasPossible);
        }

        public void OnClickTabButton()
        {
            parentController?.UpdateGachaGroupTab(CurrentGroupData);
        }

        public void SetSelectedState(GachaGroupData selectedGroupData = null)
        {
            bool isSelected = false;
            if (selectedGroupData != null && selectedGroupData.key == CurrentGroupData.key) // 선택된 탭에 대한 처리
            {
                tabOuterBGImage.color = selectedTabColor.outerColor;
                tabInnerBGImage.color = selectedTabColor.innerColor;

                //tabMainText.color = Color.white;
                //tabSubText.color = Color.white;

                isSelected = true;
            }
            else // 나머지 선택되지 않은탭에 대한 처리
            {
                switch ((eGachaGroupMenu)CurrentGroupData.key)
                {
                    case eGachaGroupMenu.PICKUP_DRAGON:
                    case eGachaGroupMenu.CLASS or eGachaGroupMenu.DRAGON_PREMIUM or eGachaGroupMenu.DRAGON_FREE:
                        tabOuterBGImage.color = dragonTabColor.outerColor;
                        tabInnerBGImage.color = dragonTabColor.innerColor;
                        break;
                    case eGachaGroupMenu.PET_PREMIUM or eGachaGroupMenu.PET_FREE:
                        tabOuterBGImage.color = petTabColor.outerColor;
                        tabInnerBGImage.color = petTabColor.innerColor;
                        break;
                    case eGachaGroupMenu.LUCKYBOX:
                        tabOuterBGImage.color = luckyBoxTabColor.outerColor;
                        tabInnerBGImage.color = luckyBoxTabColor.innerColor;
                        break;
                }

                //tabMainText.color = textDisableColor;
                //tabSubText.color = textDisableColor;
            }

            transform.localScale = isSelected ? new Vector3(1.05f, 1.05f, 1.05f) : Vector3.one;
        }
    }
}
