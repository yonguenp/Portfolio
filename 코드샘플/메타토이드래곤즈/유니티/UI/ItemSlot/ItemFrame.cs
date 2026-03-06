using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{

    public class ItemFrame : MonoBehaviour, EventListener<ItemFrameEvent>
    {
        const int PART_ITEM_KIND = 10;

        [SerializeField]
        Image itemFrame = null;

        [SerializeField]
        Image gemstoneItemFrame = null;

        [SerializeField]
        DragonPortraitFrame dragonFrame = null;

        [SerializeField]
        PetPortraitFrame petFrame = null;

        [SerializeField]
        GameObject itemFrameObj = null;

        [SerializeField]
        GameObject ItemHighLight = null;

        [SerializeField]
        Image itemIcon = null;

        [SerializeField]
        Text itemAmountText = null;

        [SerializeField]
        GameObject expNode = null;

        [SerializeField]
        Sprite spriteNormal = null;

        [SerializeField]
        Sprite gemstoneSpriteNormal = null;

        [SerializeField]
        Sprite spriteSelect = null;

        [SerializeField]
        Sprite spriteDisable = null;

        [SerializeField]
        Sprite emptyIconSprite = null;

        [SerializeField]
        GameObject checkNode = null;

        [SerializeField]
        GameObject[] starObjects;

        [SerializeField]
        GameObject extendNode = null;

        [SerializeField]
        Text extendAmountText = null;

        [SerializeField]
        Image partGradeBoard = null;

        [Header("item")]
        [SerializeField]
        GameObject frameObj = null;

        [SerializeField]
        GameObject lockIconObj = null;

        [SerializeField]
        GameObject nftCheckNode = null;
        [SerializeField]
        GameObject nftTextNode = null;


        bool isInit = false;
        int currentItemID = 0;
        eFrameFunctioal eFuncType = eFrameFunctioal.NONE;
        eItemFrameType itemtype = eItemFrameType.NONE;
        eGoodType assetType = eGoodType.NONE;

        bool isSufficientAmount = false;
        public bool IsSufficientAmount { get { return isSufficientAmount; } }

        private Action eventUpdateCallback = null;

        public int GetItemID()
        {
            return currentItemID;
        }

        public string GetItemAmountString()
        {
            return itemAmountText.text;
        }

        public delegate void Func(string itemID);

        private Func clickItemIDCallback = null;

        void Start()
        {
            if (!isInit)
            {
                setFrameBlank();
            }
            EventManager.AddListener(this);
        }

        void OnDisable()
        {
            EventManager.RemoveListener(this);
        }

        void init(bool isEmpty = false)
        {
            isInit = true;

            SetVisibleNFTNode(null);

            if (itemFrame != null)
            {
                if (isEmpty)
                {
                    itemFrame.sprite = spriteDisable;
                    return;
                }
                itemFrame.sprite = spriteNormal;
            }
            
            itemIcon.gameObject.SetActive(true);

            if (partGradeBoard != null)
                partGradeBoard.gameObject.SetActive(false);
        }

        /**
         * 비어있는 아이템 슬릇으로 설정
         */
        public void setFrameBlank(string _amount = "")
        {
            init(true);
            eFuncType = eFrameFunctioal.NONE;
            if (itemIcon != null)
            {
                itemIcon.sprite = emptyIconSprite;
                if (emptyIconSprite == null)
                {
                    itemIcon.gameObject.SetActive(false);
                }

            }
            if (itemAmountText != null)
            {
                itemAmountText.text = string.IsNullOrEmpty(_amount) ? "" : _amount;
            }
        }
        public void SetFrameItem(Asset itemInfo, string desc = "")
        {
            SetFrameItem(itemInfo.ItemNo, itemInfo.Amount, (int)itemInfo.GoodType);
            
            if(!string.IsNullOrEmpty(desc))
            {
                if (expNode != null)
                {
                    expNode.gameObject.SetActive(true);
                    var expLabelNode = SBFunc.GetChildrensByName(expNode.transform, new string[] { "expLabel" });
                    if (expLabelNode != null)
                    {
                        expLabelNode.GetComponent<Text>().text = desc;
                    }
                }
            }
        }
        public void SetFrameItem(int itemID, int itemAmount, int itemType = 0)
        {
            if(petFrame != null)
                petFrame.gameObject.SetActive(false);
            if(dragonFrame != null)
                dragonFrame.gameObject.SetActive(false);
            if(itemFrameObj != null)
                itemFrameObj.SetActive(true);

            assetType = (eGoodType)itemType;
            switch (assetType)
            {
                case eGoodType.GOLD:
                case eGoodType.ARENA_TICKET:
                case eGoodType.GEMSTONE:
                case eGoodType.ACCOUNT_EXP:
                case eGoodType.MAGNET:
                case eGoodType.ARENA_POINT:
                case eGoodType.FRIENDLY_POINT:
                case eGoodType.GUILD_EXP:
                case eGoodType.GUILD_POINT:
                case eGoodType.MAGNITE:
                    setFrameCashInfo(itemType, itemAmount, false, false, false, itemID);
                    return;

                case eGoodType.ENERGY:
                    setFrameEnergyInfo(itemAmount);
                    return;
                case eGoodType.CARD:
                case eGoodType.CHARACTER:
                    if(itemFrameObj != null)
                        itemFrameObj.SetActive(false);
                    if (dragonFrame != null)
                    {
                        dragonFrame.gameObject.SetActive(true);
                        dragonFrame.SetCustomPotraitFrame(itemID, 0);

                        if (clickItemIDCallback != null)
                        {
                            dragonFrame.setCallback((itemID) =>
                            {
                                if (clickItemIDCallback != null)
                                    clickItemIDCallback.Invoke(itemID);
                            });
                        }
                        else
                        {
                            dragonFrame.setCallback((itemID) =>
                            {
                                ToolTip.OnToolTip(new Asset(eGoodType.CHARACTER, int.Parse(itemID), 1), gameObject);
                            });
                        }
                    }
                    break;
                case eGoodType.PET:
                    if (petFrame != null)
                    {
                        petFrame.gameObject.SetActive(true);
                        petFrame.SetCustomPotraitFrame(itemID, itemAmount);
                    }
                    if (itemFrameObj != null)
                        itemFrameObj.SetActive(false);
                    break;

                case eGoodType.ITEM:
                case eGoodType.EQUIPMENT:
                case eGoodType.ITEMGROUP:
                case eGoodType.CASH:
                case eGoodType.MILEAGE:
                case eGoodType.COIN:
                case eGoodType.NONE:
                default:
                    break;
            }
            itemIcon.gameObject.SetActive(true);
            ItemBaseData itemInfo = ItemBaseData.Get(itemID);
            if (itemInfo == null)
            {
                SetFrameItemInfo(0, 0);
                return;
            }

            switch (itemInfo.ASSET_TYPE)
            {
                case eGoodType.GOLD:
                {
                    setFrameCashInfo((int)eGoodType.GOLD, itemAmount);
                }
                break;
                case eGoodType.ENERGY:
                {
                    setFrameEnergyInfo(itemAmount);
                }
                break;
                case eGoodType.ACCOUNT_EXP:
                {
                    SetFrameItemExpInfo(itemID, itemAmount, 0);
                }
                break;
                case eGoodType.GEMSTONE:
                {
                    setFrameCashInfo((int)eGoodType.GEMSTONE, itemAmount);
                }
                break;
                case eGoodType.MAGNET:
                {
                    setFrameCashInfo((int)eGoodType.MAGNET, itemAmount);
                }
                break;
                case eGoodType.ARENA_POINT:
                {
                    setFrameCashInfo((int)eGoodType.ARENA_POINT, itemAmount);
                }
                break;
                case eGoodType.FRIENDLY_POINT:
                {
                    setFrameCashInfo((int)eGoodType.FRIENDLY_POINT, itemAmount);
                }
                break;
                default:
                {
                    SetFrameItemInfo(itemID, itemAmount);
                }
                break;
            }
        }


        /**
         * 일반 아이템 슬릇으로 설정
         * @param itemID 아이템 번호
         * @param itemAmount 아이템 수량 - 기본값 = 유저 소지 수량
         */
        public void SetFrameItemInfo(string itemID, int itemAmount)
        {
            SetFrameItemInfo(int.Parse(itemID), itemAmount);
        }

        public void SetFrameItemInfo(int itemID, int itemAmount, int visibleAmountCount = 0)
        {
            init();
            eFuncType = eFrameFunctioal.TOOLTIP;
            //itemID으로 Icon 가져오기
            //
            if (itemID == 0)
            {
                setFrameBlank();
                return;
            }

            ItemBaseData itemInfo = ItemBaseData.Get(itemID);
            if (itemInfo == null)
                return;

            bool isPartType = (int)itemInfo.KIND == PART_ITEM_KIND;
            if (partGradeBoard != null)
                partGradeBoard.gameObject.SetActive(isPartType);

            if (isPartType)
            {
                itemIcon.sprite = itemInfo.ICON_SPRITE;
                if (partGradeBoard != null)
                    partGradeBoard.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.PartsIconPath, SBFunc.StrBuilder("bggrade_board_", itemInfo.GRADE));
                visibleAmountCount = 1;
            }
            else
            {
                itemIcon.sprite = itemInfo.ICON_SPRITE;
            }
            currentItemID = itemID;
            itemAmountText.text = itemAmount.ToString();
            itemtype = eItemFrameType.ITEM;
            assetType = eGoodType.ITEM;

            itemAmountText.gameObject.SetActive(itemAmount > visibleAmountCount);

            // 프레임 설정
            if(itemFrame != null)
                itemFrame.gameObject.SetActive(true);
            if(gemstoneItemFrame != null)
                gemstoneItemFrame.gameObject.SetActive(false);
        }
        /**
         * 
         * @param itemID 아이템 번호
         * @param itemAmount 아이템 수량
         * @param expAmount 해당 아이템 경험치 수량
         */
        public void SetFrameItemExpInfo(int itemID, int itemAmount, int expAmount)
        {
            init();
            eFuncType = eFrameFunctioal.TOOLTIP;
            itemtype = eItemFrameType.ITEM;
            //itemID으로 Icon 가져오기
            //
            if (itemID == 0)
            {
                setFrameBlank();
                return;
            }


            ItemBaseData itemInfo = ItemBaseData.Get(itemID);
            itemIcon.sprite = itemInfo.ICON_SPRITE;
            currentItemID = itemID;

            itemAmountText.text = itemAmount.ToString();

            if (expAmount > 0)
            {
                if (expNode != null)
                {
                    expNode.gameObject.SetActive(true);
                    var expLabelNode = SBFunc.GetChildrensByName(expNode.transform, new string[] { "expLabel" });
                    if (expLabelNode != null)
                    {
                        expLabelNode.GetComponent<Text>().text = SBFunc.StrBuilder("EXP ", expAmount.ToString());
                    }
                }
            }
        }

        /**
        * 생산 아이템 슬릇으로 설정
        * @param ItemID 툴팁에 표시를 위한 아이템 번호
        * @param IconName 레시피에 표기되는 아이템 아이콘
        * @param itemAmount 아이템 수량
        */
        public void SetFrameProductInfo(int ItemID, Sprite Icon, int itemAmount, bool isAmountLabelVisible = true)
        {
            init();
            eFuncType = eFrameFunctioal.TOOLTIP;

            var item = User.Instance.GetItem(ItemID);
            ItemBaseData itemInfo = item.BaseData;
            ItemHighLight.SetActive(false);
            itemFrame.gameObject.SetActive(false);
            itemIcon.sprite = Icon;
            itemAmountText.text = string.Format("{0}", itemAmount);
            itemAmountText.gameObject.SetActive(isAmountLabelVisible);
            currentItemID = ItemID;
            itemtype = eItemFrameType.ITEM;
        }
        /**
         * 재료 아이템 슬릇으로 설정
         * 요구 수량 / 소지 수량 으로 변경 됨, 요구 수량 불충족시, ItemFrame을 Invaild이미지로 변경
         * @param requireItemID 요구 아이템 번호
         * @param requireAmount 요구 아이템 수량
         */
        public void setFrameRecipeInfo(int requireItemID, int requireAmount, bool extendMode = false)
        {
            init();
            eFuncType = eFrameFunctioal.TOOLTIP;

            var item = User.Instance.GetItem(requireItemID);
            ItemBaseData itemInfo = item.BaseData;
            var userItem = 0;
            if (item != null)
            {
                userItem = item.Amount;

                if(itemInfo != null)
                {
                    if (itemInfo.ASSET_TYPE == eGoodType.MAGNET)
                    {
                        userItem = User.Instance.UserData.Magnet;
                    }
                }
            }
            itemIcon.sprite = itemInfo.ICON_SPRITE;
            currentItemID = requireItemID;
           
            itemAmountText.text = string.Format("{0}/{1}", userItem, requireAmount);
            isSufficientAmount = !(userItem < requireAmount);
            itemAmountText.color = isSufficientAmount ? new Color(255, 255, 255, 255) : new Color(255, 0, 0, 255);
            itemtype = eItemFrameType.ITEM;

            eventUpdateCallback = () =>
            {
                var userItem = User.Instance.GetItem(currentItemID).Amount;
                isSufficientAmount = !(userItem < requireAmount);
                itemAmountText.color = isSufficientAmount ? new Color(255, 255, 255, 255) : new Color(255, 0, 0, 255);
            };

            if (extendMode)
                SetExtendLabel();
        }

        /**
         * 재화 슬릇으로 설정
         * 재화는 아이템이 아니므로 재화전용 슬릇으로 변경
         * 수량을 명시할 경우 요구 수량 / 소지 수량으로 변경 됨
         * @param cashType 재화 타입
         * @param cashAmount 재화 수량 - 기본값 = 유저 소지 수량
         * @param extendMode 재화 수량 튜닝
         * @param isDiffLabel 인벤 재화와 비교 라벨표시
         */
        public void setFrameCashInfo(int cashType, int cashAmount, bool checkAmount = false, bool extendMode = false, bool isDiffLabel = false, int itemno = 0)
        {
            init();
            eFuncType = eFrameFunctioal.TOOLTIP;
            currentItemID = itemno;
            var userCash = 0;
            switch ((eGoodType)cashType)
            {
                // 골드
                case eGoodType.GOLD:
                    userCash = User.Instance.GOLD;
                    assetType = eGoodType.GOLD;
                    break;
                case eGoodType.ARENA_TICKET:
                    userCash = ArenaManager.Instance.UserArenaData.Arena_Ticket;
                    assetType = eGoodType.ARENA_TICKET;
                    break;
                // 젬스톤
                case eGoodType.GEMSTONE:
                    userCash = User.Instance.GEMSTONE;
                    assetType = eGoodType.GEMSTONE;
                    break;
                case eGoodType.ACCOUNT_EXP:
                    userCash = User.Instance.UserData.Exp;
                    break;
                case eGoodType.MAGNET:
                    userCash = User.Instance.UserData.Magnet;
                    assetType = eGoodType.MAGNET;
                    break;
                case eGoodType.MAGNITE:
                    userCash = User.Instance.UserData.Magnite;
                    assetType = eGoodType.MAGNITE;
                    break;
                case eGoodType.ARENA_POINT:
                    userCash = User.Instance.UserData.Arena_Point;
                    break;
                case eGoodType.FRIENDLY_POINT:
                    userCash = User.Instance.UserData.Friendly_Point;
                    break;
                case eGoodType.GUILD_POINT:
                    userCash = 0;
                    break;
                case eGoodType.GUILD_EXP:
                    userCash = 0;
                    break;
            }

            itemtype = SBFunc.GetFrameTypeByGoodType((eGoodType)cashType);

            if (itemIcon != null)
                itemIcon.sprite = SBFunc.GetGoodTypeIcon((eGoodType)cashType);

            // 프레임 설정 (젬스톤만 on)
            if(itemFrame != null)
                itemFrame.gameObject.SetActive((eGoodType)cashType != eGoodType.GEMSTONE);
            if(gemstoneItemFrame != null)
                gemstoneItemFrame.gameObject.SetActive((eGoodType)cashType == eGoodType.GEMSTONE);
            if(itemAmountText != null)
            {
                itemAmountText.gameObject.SetActive(cashAmount > 0);
                itemAmountText.text = SBFunc.CommaFromNumber(cashAmount);

                if (checkAmount)
                    itemAmountText.color = cashAmount > userCash ? new Color(255, 0, 0, 255) : new Color(255, 255, 255, 255);

                if (isDiffLabel)
                    itemAmountText.text = string.Format("{0}/{1}", userCash, cashAmount);

            }

            if (extendMode)
            {
                isSufficientAmount = cashAmount <= userCash;
                SetExtendLabel();
            }
        }

        void SetExtendLabel()
        {
            if (itemAmountText != null)
                itemAmountText.gameObject.SetActive(false);
            if (extendNode != null)
                extendNode.SetActive(true);
            if (extendAmountText != null)
            {
                extendAmountText.color = itemAmountText.color;
                extendAmountText.text = itemAmountText.text;
            }
        }

        public void setFrameEnergyInfo(int Amount)
        {
            init();
            currentItemID = 0;
            eFuncType = eFrameFunctioal.TOOLTIP;
            itemIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "energy");

            itemAmountText.text = Amount.ToString();
            itemtype = eItemFrameType.STEMINA;
        }

        public void setFrameArenaTicketInfo(int Amount)
        {
            init();
            eFuncType = eFrameFunctioal.TOOLTIP;
            itemIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "item_pvp_ticket_1");

            itemAmountText.text = Amount.ToString();
            itemtype = eItemFrameType.ARENA_TICKET;
        }

        public void SetItemBgOff(bool isFrameOn = true)
        {
            itemFrame.gameObject.SetActive(false);
            gemstoneItemFrame.gameObject.SetActive(false);
            ItemHighLight.SetActive(false);
            if (frameObj != null)
            {
                frameObj.SetActive(isFrameOn);
            }
        }

        /**
         * 인벤토리 내부 기능 이용하기
         */
        public void setInventoryFunc()
        {
            eFuncType = eFrameFunctioal.POPUP;
        }
        public void setCallback(Func ok_cb)
        {
            eFuncType = eFrameFunctioal.CallBack;

            if (ok_cb != null)
            {
                clickItemIDCallback = ok_cb;
            }
        }

        public void SetTooltipShowAble()
        {
            eFuncType = eFrameFunctioal.TOOLTIP;
        }

        public void setFrameSelect()
        {
            if (itemFrame != null)
            {
                itemFrame.sprite = spriteSelect;
            }

            if (gemstoneItemFrame != null)
            {
                gemstoneItemFrame.sprite = spriteSelect;
            }
        }

        public void setFrameNormal()
        {
            if (itemFrame != null)
            {
                itemFrame.sprite = spriteNormal;
            }

            if (gemstoneItemFrame != null)
            {
                gemstoneItemFrame.sprite = gemstoneSpriteNormal;
            }
        }
        public void SetFrameFuncNone()
        {
            eFuncType = eFrameFunctioal.NONE;
        }

        public void setFrameCheck(bool active)
        {
            if (checkNode != null)
            {
                checkNode.gameObject.SetActive(active);
            }
        }

        public void SetInventoryAnimationItem(int itemID, int itemType = 0)
        {
            SetFrameItemInfo(itemID, 1);
            itemAmountText.gameObject.SetActive(false);
            itemFrame.gameObject.SetActive(false);
        }

        static public bool IsInventoryItem(int itemID)
        {
            ItemBaseData itemInfo = ItemBaseData.Get(itemID);
            if (itemInfo == null)
                return false;

            switch (itemInfo.ASSET_TYPE)
            {
                case eGoodType.GOLD:
                case eGoodType.ENERGY:
                case eGoodType.ACCOUNT_EXP:
                case eGoodType.GEMSTONE:
                case eGoodType.MAGNET:
                    return false;
                default:
                    return true;
            }
        }

        public void onClick()
        {
            switch (eFuncType)
            {
                case eFrameFunctioal.NONE:
                    return;

                case eFrameFunctioal.POPUP:
                {
                    setFrameSelect();
                    PopupManager.OpenPopup<ItemInfoPopup>(new ItemInfoPopupData(this));
                } break;

                case eFrameFunctioal.TOOLTIP:
                {
                    if (currentItemID < 0)
                        return;
                    setFrameSelect();
                    ToolTip.OnToolTip(new Asset(currentItemID, 1, (int)assetType), gameObject);
                } break;
                case eFrameFunctioal.CallBack:
                    clickItemIDCallback?.Invoke(currentItemID.ToString());
                    break;
            }
        }
        /**
         * ItemFrame을 사용할지는 아직 모르므로
         * @param starNumb 
         */
        public void setRewardStar(int starNumb)
        {
            const int starMaxCount = 3;
            if (starNumb > starMaxCount) return;
            for (int i = 0; i < starNumb; ++i)
            {
                starObjects[i].SetActive(true);
            }
            for (int i = starNumb; i < starMaxCount; ++i)
            {
                starObjects[i].SetActive(false);
            }

        }
        public void OnEvent(ItemFrameEvent eventType)
        {
            switch (eventType.Event)
            {
                case ItemFrameEvent.ItemFrameEventEnum.ITEM_UPDATE:
                {
                    if (eventUpdateCallback != null)
                    {
                        eventUpdateCallback();
                    }
                }
                break;
            }
        }

        public void SetAmountTextSizeMultiful(float multifulValue){
            itemAmountText.GetComponent<RectTransform>().localScale *= multifulValue;
        }
        public void SetTextAlignment(TextAnchor anchor)
        {
            itemAmountText.alignment = anchor;
        }

        public Image GetIconImage()
        {
            return itemIcon;
        }

        public void SetLockIcon(bool state)
        {
            if(lockIconObj != null)
                lockIconObj.SetActive(state);
        }
        public void SetFrameColor(Color color)
        {
            if(frameObj != null)
            {
                frameObj.GetComponent<Image>().color = color;
            }
        }

        public void SetTextColor(Color color)
        {
            itemAmountText.color = color;
        }

        public void SetMinMaxText(Asset min, Asset max)
        {
            if (min.ItemNo != max.ItemNo)
                return;
            if (min.GoodType != max.GoodType)
                return;

            SetMinMaxText(min.Amount, max.Amount);
        }
        public void SetMinMaxText(int min, int max, bool forceActive = false)
        {
            if (itemAmountText == null)
                return;

            if (forceActive)
                itemAmountText.gameObject.SetActive(true);

            itemAmountText.text = SBFunc.StrBuilder(min, "~", max);
        }

        public void SetCustomText(string _text)
        {
            if (itemAmountText == null)
                return;

            if (!itemAmountText.gameObject.activeInHierarchy)
                itemAmountText.gameObject.SetActive(true);

            itemAmountText.text = _text;
        }

        public void SetMediumSizeArenaPointIcon()
        {
            if(itemtype== eItemFrameType.ARENA_POINT)
                itemIcon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "pvp_gold_point");
        }

        public void SetDragonPortraitSpine(int _dragonTag)
        {
            if (dragonFrame == null)
                return;

            if (_dragonTag < 0)
                return;

            var dragonData = CharBaseData.Get(_dragonTag);
            if (dragonData == null)
                return;

            dragonFrame.SetSpine(dragonData);
        }

        public void SetVisibleNFTNode(ItemBaseData _itemData = null)
        {
            bool isP2EUser = User.Instance.ENABLE_P2E;
            if (nftCheckNode != null)
                nftCheckNode.SetActive(_itemData == null ? false : isP2EUser ? _itemData.ENABLE_NFT : false);
            if (nftTextNode != null)
                nftTextNode.SetActive(_itemData == null ? false : isP2EUser ? _itemData.ENABLE_NFT : false);
        }
    }
}

