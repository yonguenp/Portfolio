using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class GiftBoxOpenPopupData : PopupData
    {
        public int slotIndex = -1;//선물상자 인덱스
        public Asset item = null;//현재 선택한 선물 상자
        public List<Asset> rewardList = new List<Asset>();
        public GiftBoxOpenPopupData(int _slotIndex,Asset _item, List<Asset> _rewardList)
        {
            slotIndex = _slotIndex;
            rewardList = _rewardList.ToList();
            item = _item;
        }
    }
    
    public class DiceEventGiftOpenPopup : Popup<GiftBoxOpenPopupData>
    {
        //normal
        //const string BOX_SKIN_NAME_PREFIX = "box";
        //fall box version
        const string BOX_SKIN_NAME_PREFIX = "fall_box_";

        [SerializeField] GameObject scrollViewNode = null;

        [SerializeField] Image[] itemIcon = null;
        [SerializeField] Text[] itemCount = null;

        [SerializeField] Text remainCountText = null;

        [SerializeField] GameObject buttonBundleNode = null;

        [SerializeField] Button[] keepOpenButton = null;
        [SerializeField] Button buyButton = null;

        [SerializeField] SkeletonGraphic spine = null;//스파인 제어
        [SerializeField] GameObject spineEffectNode = null;


        [Header("ScrollView")]
        [SerializeField]
        protected ScrollRect scrollView = null;
        [SerializeField]
        protected GameObject itemAniPrefab = null;
        [SerializeField]
        protected Vector2 itemSize = Vector2.zero;
        [SerializeField]
        protected float itemScale = 1f;
        [SerializeField]
        protected float itemSpancing = 0f;
        [SerializeField]
        protected float itemPadding = 10f;
        [SerializeField]
        protected float minAniTime = 0.07f;
        [SerializeField]
        protected float maxAniTime = 0.2f;
        [Space(10f)]
        [Header("Text")]
        [SerializeField]
        protected Text titleText = null;
        [Space(10f)]
        [SerializeField]
        protected GameObject fireworkObj = null;
        [SerializeField]
        protected GameObject scrollEffectObj = null;

        protected List<Animator> itemAnimators = null;
        protected float aniTime = 0f;
        protected float scrollDelay = 0.35f;
        protected float scrollTimeDelay = 0f;
        protected float scrollTime = 0f;
        protected float scrollTimeMax = 2f;
        protected IEnumerator wait = null;
        protected int curAniIndex = 0;
        protected float aniTimeDelay = 0f;
        protected bool isScrolling = false;
        private SBListPool<GameObject> itemPool = null;

        public VoidDelegate onClose = null;


        Coroutine scrollCo = null;



        int slotIndex = -1;
        Asset slotItem = null;
        List<Asset> rewardList = new List<Asset>();
        bool isBoxIdle = false;
        int MaxOpenCount
        {
            get
            {
                if (slotIndex < 0)
                    return 1;
                else
                {
                    switch(slotIndex)
                    {
                        case 0:
                            return Convert.ToInt32(GameConfigData.Get("2023_HOLIDAY_BOX1_OPEN_MAX").VALUE);
                        case 1:
                            return Convert.ToInt32(GameConfigData.Get("2023_HOLIDAY_BOX2_OPEN_MAX").VALUE);
                        case 2:
                            return Convert.ToInt32(GameConfigData.Get("2023_HOLIDAY_BOX3_OPEN_MAX").VALUE);
                    }
                    return 1;
                }
            }
        }

        #region OpenPopup
        public static DiceEventGiftOpenPopup OpenPopup(int _slotIndex, Asset _slotItem, List<Asset> _rewardList)
        {
            return OpenPopup(new GiftBoxOpenPopupData(_slotIndex, _slotItem, _rewardList));
        }
        public static DiceEventGiftOpenPopup OpenPopup(GiftBoxOpenPopupData data)
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<DiceEventGiftOpenPopup>(data);
        }
        #endregion

        public override void InitUI()
        {
            if(Data == null)
            {
                Debug.LogError("popupData is null");
                return;
            }

            PopupTopUIRefreshEvent.Hide();
            ProductionProcess();
        }

        void ProductionProcess()
        {
            DataClear();
            SetData();
            RefreshCount();
            RefreshButton();
            SetVisibleButtonNode(false);
        }

        void DataClear()
        {
            slotIndex = -1;
            slotItem = null;
            if (rewardList == null)
                rewardList = new List<Asset>();
            rewardList.Clear();
        }

        void SetData()
        {
            slotIndex = Data.slotIndex;
            slotItem = Data.item;
            rewardList = Data.rewardList.ToList();

            SBFunc.RemoveAllChildrens(scrollView.content);
            SetVisibleEffectode(false);

            if (spine != null)
            {
                var skinName = BOX_SKIN_NAME_PREFIX + (slotIndex + 1).ToString("D2");
                spine.initialSkinName = skinName;
                spine.Initialize(true);
                spine.Skeleton.SetSkin(skinName);
                spine.startingAnimation = "open";

                SetVisibleEffectode(true);

                DOTween.Sequence().AppendInterval(0.6f).AppendCallback(() => {
                    ShowRewardList(null);
                }).Play();
            }
        }

        public void RefreshCount()
        {
            if (slotItem == null)
                return;

            if (itemIcon != null)
            {
                itemIcon[0].sprite = slotItem.BaseData?.ICON_SPRITE;
                itemIcon[1].sprite = slotItem.BaseData?.ICON_SPRITE;
                itemIcon[2].sprite = slotItem.BaseData?.ICON_SPRITE;
            }
            if (itemCount != null)
            {                
                var curAmount = slotItem.Amount;
                var maxCount = 1;
                itemCount[0].text = string.Format("x {0}", maxCount > curAmount ? curAmount : maxCount);
                itemCount[0].color = curAmount == 0 ? Color.red : Color.white;

                maxCount = 10;
                itemCount[1].text = string.Format("x {0}", maxCount > curAmount ? curAmount : maxCount);
                itemCount[1].color = curAmount == 0 ? Color.red : Color.white;

                maxCount = 100;
                itemCount[2].text = string.Format("x {0}", maxCount > curAmount ? curAmount : maxCount);
                itemCount[2].color = curAmount == 0 ? Color.red : Color.white;
            }

            if (remainCountText != null)
                remainCountText.text = StringData.GetStringFormatByStrKey("box_count", slotItem.Amount);//남은 상자 : {0}개
        }

        public void RefreshButton()
        {
            var curAmount = slotItem.Amount;
            var isZero = curAmount <= 0;

            if (keepOpenButton != null)
            {
                keepOpenButton[0].gameObject.SetActive(true);
                keepOpenButton[1].gameObject.SetActive(false);
                keepOpenButton[2].gameObject.SetActive(false);

                keepOpenButton[0].interactable = !isZero;
                keepOpenButton[0].SetButtonSpriteState(!isZero);

                if (curAmount >= 10)
                {
                    keepOpenButton[1].gameObject.SetActive(true);
                    keepOpenButton[1].interactable = true;
                    keepOpenButton[1].SetButtonSpriteState(true);

                    if(curAmount >= 100)
                    {
                        keepOpenButton[2].gameObject.SetActive(true);
                        keepOpenButton[2].interactable = true;
                        keepOpenButton[2].SetButtonSpriteState(true);
                    }
                }                
            }
            buyButton.gameObject.SetActive(false);
            //if (keepOpenButton != null)
            //    keepOpenButton.gameObject.SetActive(!isZero);

            //if (buyButton != null)
            //    buyButton.gameObject.SetActive(isZero);

            if(isZero)
                buyButton.SetButtonSpriteState(slotIndex == 0 ? true : false);
        }

        void SetVisibleScrollViewNode(bool _isVisible)
        {
            if (scrollViewNode != null)
                scrollViewNode.SetActive(_isVisible);
        }

        void SetVisibleButtonNode(bool _isVisible)
        {
            if (buttonBundleNode != null)
                buttonBundleNode.SetActive(_isVisible);

            isBoxIdle = _isVisible;
        }
        void SetVisibleEffectode(bool _isVisible)
        {
            if (spineEffectNode != null)
                spineEffectNode.SetActive(_isVisible);
        }

        public void ShowRewardList(Spine.TrackEntry e)
        {
            //spine 제작 된다고 함. idle 상태 -> complete 이 후 끝나면 보상 연출 팝업 넘기는 시퀀스 추가해야함.
            if (rewardList == null)
                return;

            InitItemPool();
            DOTween.Sequence().AppendCallback(() =>
            {
                ShowRewardScroll();
            });
        }

        void InitItemPool()
        {
            if (itemPool != null)
                itemPool.Clear();

            itemPool = new SBListPool<GameObject>(Reuse, Unuse);
            SBFunc.RemoveAllChildrens(scrollView.content);

            if (fireworkObj != null)
                fireworkObj.SetActive(false);

            if (scrollEffectObj != null)
                scrollEffectObj.SetActive(false);
        }

        void ShowRewardScroll()
        {
            if (scrollViewNode != null)
            {
                var uniqueList = SystemRewardPopup.GetUniqueReward(rewardList);
                
                DOTween.Sequence().AppendCallback(()=> {
                    SetVisibleScrollViewNode(true);
                }).AppendInterval(0.1f).AppendCallback(() =>
                {
                    if (scrollEffectObj != null)
                        scrollEffectObj.SetActive(true);

                    ShowRewardScrollView(uniqueList);
                });
            }
        }

        public void OnClickOpenBox(int count = 1)
        {
            if(slotItem.Amount <= 0)
            {
                ToastManager.On(StringData.GetStringByStrKey("보유상자없음"));
                return;
            }

            if(scrollViewNode != null)
            {
                initScrollView();
                
                if (fireworkObj != null)
                    fireworkObj.SetActive(false);

                if (scrollEffectObj != null)
                    scrollEffectObj.SetActive(false);

                SetVisibleScrollViewNode(false);
            }

            //이벤트 요청
            DiceUIEvent.RequestOpenBox(slotIndex, count);
        }

        public void OnClickBuyButton()//상자 카운트 다 떨어지면, 상자 오픈으로 연결
        {
            if (!isBoxIdle)
                return;

            if(slotIndex > 0)
            {
                ToastManager.On(StringData.GetStringByStrKey("보유상자없음"));
                return;
            }

            if (scrollViewNode != null)
            {
                initScrollView();

                if (fireworkObj != null)
                    fireworkObj.SetActive(false);

                if (scrollEffectObj != null)
                    scrollEffectObj.SetActive(false);

                SetVisibleScrollViewNode(false);
            }

            ClosePopup();

            DiceEventPopup.MoveTabForce(new TabTypePopupData(1, 1));
        }
        public override void OnClickDimd()
        {
            if (!isBoxIdle)
                return;

            if (scrollViewNode != null)
            {
                initScrollView();

                if (fireworkObj != null)
                    fireworkObj.SetActive(false);

                if (scrollEffectObj != null)
                    scrollEffectObj.SetActive(false);

                SetVisibleScrollViewNode(false);
            }

            base.OnClickDimd();
        }

        /// <summary>
        /// 현재 팝업이 열려있고, 추가 상자 오픈을 시도 할 때 호출하도록.
        /// </summary>
        /// <param name="data"></param>
        public override void ForceUpdate(GiftBoxOpenPopupData data)
        {
            base.ForceUpdate(data);//data refresh

            ProductionProcess();
        }

        #region scrollView
        public void ShowRewardScrollView(List<Asset> _rewards)
        {
            var itemSize = this.itemSize * itemScale;
            float itemSizeX = itemSize.x + itemSpancing;
            int itemCount = _rewards.Count;
            float itemX = itemSizeX * itemCount;
            float contentSizeX = itemPadding * 2f + itemX;
            float itemStart = -itemX * 0.5f + itemPadding;
            aniTime = 0f;
            scrollDelay = 0.35f;
            scrollTimeDelay = 0f;
            scrollTime = 0f;
            scrollTimeMax = 2f;
            wait = null;
            curAniIndex = 0;
            aniTimeDelay = 0f;
            isScrolling = false;

            if (titleText != null)
            {
                titleText.text = StringData.GetStringByIndex(100001361);
            }
            if (fireworkObj != null)
            {
                fireworkObj.SetActive(false);
            }


            if (itemCount > 0)
            {
                scrollTime = 0f;
                aniTime = 1f / itemCount;
                aniTime = Mathf.Min(aniTime, maxAniTime);
                if (aniTime < minAniTime)
                {
                    aniTime = minAniTime;
                }
                wait = SBDefine.GetWaitForSeconds(aniTime);
                scrollTimeMax = Mathf.Min(3.0f, aniTime * itemCount - scrollDelay);
            }
            if (itemAnimators == null)
                itemAnimators = new List<Animator>();

            isScrolling = true;
            if (scrollView != null)
            {
                if (contentSizeX <= scrollView.viewport.rect.width)
                {
                    contentSizeX = scrollView.viewport.rect.width;
                    isScrolling = false;
                }

                scrollView.content.sizeDelta = new Vector2(contentSizeX, 0f);
                if (isScrolling)
                {
                    scrollView.horizontalNormalizedPosition = 0f;
                }

                scrollView.enabled = isScrolling;

                if (itemAniPrefab != null)
                {
                    itemAnimators.Clear();
                    SpawnPrefab(itemCount);
                    for (int i = 0; i < itemCount; ++i)
                    {
                        var itemData = _rewards[i];
                        var itemObj = itemPool.Get();
                        var itemFrame = itemObj.GetComponent<ItemFrame>();
                        if (itemFrame != null)
                        {
                            itemFrame.SetFrameItem(itemData.ItemNo, itemData.Amount, (int)itemData.GoodType);
                        }
                        var animator = itemObj.GetComponent<Animator>();
                        if (animator != null)
                        {
                            animator.enabled = false;//애니 끄기

                            //animator.SetBool("prev", true);
                            //itemAnimators.Add(animator);
                        }
                        itemObj.transform.localPosition = new Vector3(contentSizeX * 0.5f + itemStart + itemSizeX * (0.5f + i), 0f, 0f);
                        itemObj.SetActive(true);
                    }
                }

                if(scrollCo != null)
                    StopCoroutine(scrollCo);

                scrollCo = StartCoroutine(ScrollAnimation(false));
            }

            CancelInvoke("ShowCloseButton");
            SetVisibleButtonNode(false);
            Invoke("ShowCloseButton", 0.25f);
        }

        private void SpawnPrefab(int count)
        {
            if (itemAniPrefab == null)
                return;

            while (itemPool.Count < count)
            {
                itemPool.Put(Instantiate(itemAniPrefab, scrollView.content));
            }
        }
        private void Reuse(GameObject obj)
        {
            obj.transform.localScale = Vector3.one * itemScale;
            obj.SetActive(true);
        }
        private void Unuse(GameObject obj)
        {
            obj.SetActive(false);
        }
        public void ShowCloseButton()
        {
            CancelInvoke("ShowCloseButton");
            SetVisibleButtonNode(true);
        }
        protected virtual IEnumerator ItemAnimation()
        {
            aniTimeDelay += SBGameManager.Instance.DTime;
            if (aniTimeDelay < 0)
                aniTimeDelay = 0;

            if (aniTimeDelay >= aniTime)
            {
                if (itemAnimators.Count <= curAniIndex || itemAnimators[curAniIndex] == null)
                {
                    aniTimeDelay -= aniTime;
                    curAniIndex++;
                    yield return null;
                    yield break;
                }
                itemAnimators[curAniIndex].SetBool("prev", false);
                itemAnimators[curAniIndex].SetBool("end", false);
                aniTimeDelay -= aniTime;
                curAniIndex++;
            }
            yield return null;
            yield break;
        }
        private void SkipItemAnimation()
        {
            var count = itemAnimators.Count;
            var anchor = -scrollView.content.anchoredPosition.x;
            while (count > curAniIndex)
            {
                if (curAniIndex > 0 && anchor > itemAnimators[curAniIndex].transform.localPosition.x)
                {
                    itemAnimators[curAniIndex].SetBool("prev", false);
                    curAniIndex++;
                }
                else
                    break;
            }
            for (int i = 0, skipCount = curAniIndex - 7; i < skipCount; ++i)
            {
                itemAnimators[i].SetBool("end", true);
            }
        }
        IEnumerator ScrollAnimation(bool _isShowItemAnim = true)
        {
            if(_isShowItemAnim)
            {
                if (isScrolling)
                {
                    while (scrollTimeDelay < scrollDelay)
                    {
                        yield return ItemAnimation();
                        scrollTimeDelay += SBGameManager.Instance.DTime;
                        if (scrollTimeDelay >= scrollDelay)
                            break;
                        SkipItemAnimation();
                    }
                    scrollTime += scrollTimeDelay - scrollDelay;
                    while (scrollTime < scrollTimeMax)
                    {
                        yield return ItemAnimation();
                        scrollView.horizontalNormalizedPosition = SBFunc.BezierCurveSpeed(0f, 1f, scrollTime, scrollTimeMax, new Vector4(0.25f, 0f, 0.75f, 1f));
                        scrollTime += SBGameManager.Instance.DTime;
                        if (scrollTime > scrollTimeMax)
                            break;
                        SkipItemAnimation();
                    }

                    scrollView.DOHorizontalNormalizedPos(1.0f, 1.0f);
                }

                SkipItemAnimation();
                while (itemAnimators.Count > curAniIndex)
                {
                    yield return ItemAnimation();
                }
            }

            if (fireworkObj != null)
            {
                yield return SBDefine.GetWaitForSeconds(0.35f);
                fireworkObj.SetActive(true);
            }

            yield break;
        }
        void initScrollView()
        {
            scrollView.horizontalNormalizedPosition = 0f;
            scrollView.content.sizeDelta = new Vector2(0, 0);
            scrollView.content.localPosition = new Vector3(0, scrollView.content.localPosition.y, 0);
            itemAnimators.Clear();

            if (scrollCo != null)
                StopCoroutine(scrollCo);
        }
        #endregion
    }
}

