using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Coffee.UIEffects;

namespace SandboxNetwork
{
    public class GachaMenuItem : MonoBehaviour
    {
        const float HEIGHT_SIZE = 700.0f;

        [Header("[Gacha Info Layer]")]
        [SerializeField] RectTransform imageRectTransform = null;
        [SerializeField] Image menuItemImage = null;
        [SerializeField] UIEffect uiEffect = null;
        [SerializeField] protected Text menuTitleText = null;
        [SerializeField] GameObject rateButton = null;

        [Header("[Item Amount Layer]")]
        [SerializeField] GameObject itemAmountObject = null;
        [SerializeField] Image itemAmountIconImage = null;
        [SerializeField] Text itemAmountText = null;

        [Header("[Time Info Layer]")]
        [SerializeField] GameObject timeInfoObject = null;
        [SerializeField] Text timeText = null;
        [SerializeField] TimeEnable remainTimeObject = null;

        [Header("[Block Layer]")]
        [SerializeField] GameObject blockLayerObject = null;
        [SerializeField] Text blockGuideText = null;

        [Header("[BG Sprites]")]
        [SerializeField] Image bgImage = null;

        [SerializeField]
        protected GameObject eventIcon = null;

        public GachaMenuData CurrentMenuData { get; protected set; } = null;
        public bool IsAvailGacha { get; protected set; } = false;
        public bool IsAvailCondition { get; protected set; } = false;
        public bool IsAvailPeriod { get; protected set; } = false;

        protected GachaUIController parentController = null;

        Vector2 originBGSize = Vector2.zero;

        public virtual void InitMenuItem(GachaMenuData menuData, GachaUIController parent)
        {
            if (menuData == null) return;

            CurrentMenuData = menuData;
            parentController = parent;

            // BG 이미지 관련
            bgImage.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.UISpritePath, CurrentMenuData.bg_type);

            // 메뉴 이미지 및 텍스트 관련
            menuItemImage.sprite = CurrentMenuData.sprite;
            if (!string.IsNullOrEmpty(CurrentMenuData.resource))
                CDNManager.SetBanner("gacha/" + CurrentMenuData.resource, menuItemImage, ()=> {
                    float spriteRate = HEIGHT_SIZE / CurrentMenuData.sprite.bounds.size.y;
                    imageRectTransform.sizeDelta = new Vector2(CurrentMenuData.sprite.bounds.size.x * spriteRate, imageRectTransform.sizeDelta.y);
                    originBGSize = imageRectTransform.sizeDelta;
                });
            else
            {
                menuItemImage.sprite = CurrentMenuData.sprite;
                float spriteRate = HEIGHT_SIZE / CurrentMenuData.sprite.bounds.size.y;
                imageRectTransform.sizeDelta = new Vector2(CurrentMenuData.sprite.bounds.size.x * spriteRate, imageRectTransform.sizeDelta.y);
                originBGSize = imageRectTransform.sizeDelta;
            }
            menuTitleText.text = CurrentMenuData.Name;

            // 필요 & 보유 아이템 관련 - 여러개에 대한 처리는 되어있지 않음
            IsAvailCondition = CheckGachaCondition();

            // 시간 관련
            IsAvailPeriod = CheckGachaPeriod();

            // 가챠 블락 레이어 처리
            IsAvailGacha = IsAvailCondition && IsAvailPeriod;
            blockLayerObject.SetActive(!IsAvailGacha);
            rateButton.SetActive(IsAvailPeriod);
            uiEffect.enabled = !IsAvailGacha;

            if(eventIcon != null)
            {
                eventIcon.SetActive(menuData.IsEvent());
            }

            RefreshAllLayout();
        }

        private void OnDisable()
        {
            ClearTween();
        }

        public void RefreshMenuItem()
        {
            IsAvailCondition = CheckGachaCondition();

            IsAvailGacha = IsAvailCondition && IsAvailPeriod;
            if(blockLayerObject != null)
                blockLayerObject.SetActive(!IsAvailGacha);
        }

        public void ClearTween()
        {
            imageRectTransform?.DOKill();
            if(menuItemImage != null)
                menuItemImage.transform?.DOKill();
        }
        // 선택된 메뉴에 대한 처리
        public virtual void SetSelectedState(bool isSelected)
        {
            //transform.localScale = isSelected ? new Vector3(1.1f, 1.1f, 1.1f) : Vector3.one;
            ClearTween();
            if (isSelected)
            {
                float scaleFactor = 0.05f;
                if (imageRectTransform != null)
                {
                    imageRectTransform.DOPunchScale(new Vector3(scaleFactor, scaleFactor, scaleFactor), 0.3f, 5);
                    imageRectTransform.sizeDelta *= 1.1f;
                }

                if (menuItemImage != null)
                {
                    menuItemImage.transform.DOKill();
                    menuItemImage.transform.localScale = Vector3.one;
                    if (IsAvailGacha)
                    {
                        menuItemImage.transform.DOScale(1.1f, 1.0f).SetLoops(-1, LoopType.Yoyo);
                    }
                }
            }
            else
            {
                if (menuItemImage != null)
                {
                    menuItemImage.transform.DOKill();
                    menuItemImage.transform.localScale = Vector3.one;
                }

                if (imageRectTransform != null)
                    imageRectTransform.sizeDelta = originBGSize;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        public void OnClickMenuItem()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

            parentController?.UpdateGachaMenuItem(CurrentMenuData);
        }

        public void OnClickGachaRateInfoButton()
        {
            GachaTablePopup.OpenPopup(CurrentMenuData.key);
        }

        protected void RefreshAllLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            parentController?.RefreshLayout();
        }

        protected bool CheckGachaCondition()
        {
            bool result = false;

            if (CurrentMenuData.typeDatas != null && CurrentMenuData.typeDatas.Count > 0)
            {
                itemAmountIconImage.sprite = SBFunc.GetGoodTypeIcon(CurrentMenuData.typeDatas[0].price_type, CurrentMenuData.typeDatas[0].price_uid);

                // 재료 부족 관련처리
                switch (CurrentMenuData.typeDatas[0].price_type)
                {
                    case eGoodType.ITEM:
                        string itemAmountString = SBFunc.GetGoodTypeAmountText(CurrentMenuData.typeDatas[0].price_type, CurrentMenuData.typeDatas[0].price_uid);
                        itemAmountText.text = StringData.GetStringFormatByStrKey("장", itemAmountString);

                        if (blockGuideText != null)
                        {
                            result = User.Instance.GetItemCount(CurrentMenuData.typeDatas[0].price_uid) > 0;
                            if (result == false)
                            {
                                blockGuideText.text = StringData.GetStringByStrKey("gacha_ticket_shortage");
                            }
                        }
                        break;
                    case eGoodType.ADVERTISEMENT:
                        var data = AdvertisementData.Get(CurrentMenuData.typeDatas[0].price_uid);
                        int cur = 0;
                        int max = 0;

                        if (data != null)
                        {
                            cur = data.CUR;
                            max = data.LIMIT;
                        }

                        itemAmountText.text = string.Format("{0}/{1}", max - cur, max);
                        result = cur < max;
                        if (result == false)
                        {
                            blockGuideText.text = StringData.GetStringByStrKey("ad_empty_alert");
                        }
                        break;
                    case eGoodType.GEMSTONE:
                        string gemstoneAmountString = SBFunc.GetGoodTypeAmountText(CurrentMenuData.typeDatas[0].price_type, CurrentMenuData.typeDatas[0].price_uid);
                        itemAmountText.text = StringData.GetStringFormatByStrKey("production_quantity", gemstoneAmountString);

                        result = User.Instance.GEMSTONE >= CurrentMenuData.typeDatas[0].price_value;
                        if (result == false)
                        {
                            blockGuideText.text = StringData.GetStringByStrKey("다이아부족");
                        }
                        break;
                }
            }

            return result;
        }

        protected bool CheckGachaPeriod()
        {
            bool result = false;

            if (timeInfoObject != null)
                timeInfoObject.SetActive(CurrentMenuData.isLimit);

            if (CurrentMenuData.isLimit)
            {
                if (CurrentMenuData.IsValid())  // 현재 기간 진행중
                {
                    result = true;
                    UpdateRemainTime(result);
                }
                else if (CurrentMenuData.IsShowUI())    // 추후 오픈 예정
                {
                    UpdateRemainTime(result);
                    if(blockGuideText != null)
                        blockGuideText.text = StringData.GetStringByStrKey("gacha_time_alarm");
                }
            }
            else
            {
                result = true;
            }

            return result;
        }

        void UpdateRemainTime(bool isPeriod)
        {
            if (remainTimeObject != null)
            {
                DateTime targetTime = isPeriod ? CurrentMenuData.end : CurrentMenuData.start;
                int textFormatIndex = isPeriod ? 100002626 : 100005149;

                remainTimeObject.Refresh = () => {
                    int time = TimeManager.GetTimeCompare(TimeManager.GetTimeStamp(targetTime));
                    if (time >= 0)
                    {
                        timeText.text = StringData.GetStringFormatByIndex(textFormatIndex, SBFunc.TimeString(time));
                    }
                    else
                    {
                        timeText.text = StringData.GetStringFormatByIndex(textFormatIndex, SBFunc.TimeString(0));
                        remainTimeObject.Refresh = null;
                        parentController?.ReloadAll();
                    }
                };
            }
        }
    }
}
