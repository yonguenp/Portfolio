using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SandboxNetwork
{
    public class SystemRewardPopup : Popup<RewardPopupData>
    {
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
        [Header("Image")]
        [SerializeField]
        protected Image titleBG = null;
        [Space(10f)]
        [Header("Text")]
        [SerializeField]
        protected Text titleText = null;
        [SerializeField]
        protected Text clickText = null;
        [SerializeField]
        protected Text emptyRewardText = null;

        [Space(10f)]
        [Header("Btn")]
        [SerializeField]
        protected Button resultBtn = null;
        [SerializeField]
        protected GameObject fireworkObj = null;

        [SerializeField]
        protected GameObject hotTime = null;

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

        private VoidDelegate dimmedClickAction = null;
		public VoidDelegate onClose = null;
        #region OpenPopup
        public static SystemRewardPopup OpenPopup(List<Asset> rewards, VoidDelegate closeCallback = null, bool isSetUnique =false)
        {
            if (rewards == null || rewards.Count < 1)
                return null;

            return OpenPopup(new RewardPopupData(isSetUnique ? GetUniqueReward(rewards) : rewards), closeCallback);
        }

        public static List<Asset> GetUniqueReward(List<Asset> rewards)  // 중복 제거
        {
            Dictionary<eGoodType, Dictionary<int, int>> itemDic = new Dictionary<eGoodType, Dictionary<int, int>>();
            List<Asset> uniqueRewards = new List<Asset>();
            foreach(Asset reward in rewards) { 
                Dictionary<int, int> itemNoAndCnt = new Dictionary<int, int>();
                eGoodType itemType = reward.GoodType;
                int itemNo = reward.ItemNo;
                int itemCnt = reward.Amount;
                if (itemDic.ContainsKey(itemType))
                {
                    itemNoAndCnt = itemDic[itemType];
                    if (itemNoAndCnt.ContainsKey(itemNo))
                    {
                        itemNoAndCnt[itemNo] += itemCnt;
                    }
                    else
                    {
                        itemNoAndCnt.Add(itemNo, itemCnt);
                    }
                    itemDic[itemType] = itemNoAndCnt;
                }
                else
                {
                    itemNoAndCnt.Add(itemNo, itemCnt);
                    itemDic.Add(itemType, itemNoAndCnt);
                }
            }
            foreach (var items in itemDic)
            {
                eGoodType itemType = items.Key;
                foreach (var item in items.Value)
                {
                    int itemNo = item.Key;
                    int itemCnt = item.Value;
                    if(itemCnt > 0)
                        uniqueRewards.Add(new Asset(itemType, itemNo, itemCnt));
                }
            }
            return uniqueRewards;
        }

        public static SystemRewardPopup OpenPopup(RewardPopupData data, VoidDelegate closeCallback = null)
        {
            if (data == null)
                return null;

            var popup = PopupManager.OpenPopup<SystemRewardPopup>(data);
            if (popup == null)
                return null;

            popup.onClose = closeCallback;
            return popup;
        }
        #endregion
        public override void InitUI()
        {
            //사용법
            //var data = new SystemRewardData();
            //for (int i = 0; i < 25; ++i) 아이템 종류 만큼 KeyValuePair<아이템 번호, 아이템 수량> 추가하기
            //{
            //    data.ItemInfos.Add(new System.Collections.Generic.KeyValuePair<int, int>(10000001, SBFunc.Random(100, 999)));
            //}
            //SystemRewardPopup.OpenPopup(data, closeCallback);

            if(itemPool == null)
            {
                itemPool = new SBListPool<GameObject>(Reuse, Unuse);
            }

            if(hotTime != null)
            {
                hotTime.SetActive(false);
            }

            var itemSize = this.itemSize * itemScale;
            float itemSizeX = itemSize.x + itemSpancing;
            int itemCount = Data.Rewards.Count;
            float itemX = itemSizeX * itemCount;
            float contentSizeX = itemPadding * 2f + itemX;
            float itemStart = -itemX * 0.5f;
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
            if (clickText != null)
            {
                clickText.text = StringData.GetStringByIndex(100000413);
            }
            if(fireworkObj != null)
            {
                fireworkObj.SetActive(false);
            }

            var itemEmpty = itemCount <= 0;
            if (emptyRewardText != null)
                emptyRewardText.gameObject.SetActive(itemEmpty);

            if (!itemEmpty)
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
                if(isScrolling)
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
                        var itemData = Data.Rewards[i];
                        var itemObj = itemPool.Get();
                        var itemFrame = itemObj.GetComponent<ItemFrame>();
                        if (itemFrame != null)
                        {
                            itemFrame.SetFrameItem(itemData.ItemNo, itemData.Amount, (int)itemData.GoodType);
                        }
                        var animator = itemObj.GetComponent<Animator>();
                        if (animator != null)
                        {
                            animator.SetBool("prev", true);
                            itemAnimators.Add(animator);
                        }
                        itemObj.transform.localPosition = new Vector3(contentSizeX * 0.5f + itemStart + itemSizeX * (0.5f + i), 0f, 0f); 
                    }
                }

                StartCoroutine(ScrollAnimation());
            }

            CancelInvoke("ShowCloseButton");
            resultBtn.gameObject.SetActive(false);
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
            resultBtn.gameObject.SetActive(true);
        }
        protected virtual IEnumerator ItemAnimation()
        {
            aniTimeDelay += SBGameManager.Instance.DTime;
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

        IEnumerator DragonRewardAnimation()
        {
            foreach (var reward in Data.Rewards)
            {
                if (reward.GoodType == eGoodType.CHARACTER)
                {
                    bool anim_done = false;
                    var dragonID = reward.ItemNo;
                    var isSuccess = true;
                    var hasDragon = User.Instance.DragonData.IsUserDragon(dragonID);
                    DragonCompoundInfoData info = new DragonCompoundInfoData(dragonID, isSuccess, !hasDragon);
                    List<DragonCompoundInfoData> dragonList = new List<DragonCompoundInfoData>() { info };

                    DragonCompoundResultPopupData newPopupData = new DragonCompoundResultPopupData(dragonList, null, " ");//StringData.GetStringByIndex(100002334) //보상획득
                    
                    var popup = PopupManager.OpenPopup<DragonCompoundResultPopup>(newPopupData);
                    
                    yield return new WaitUntil(() => !PopupManager.IsPopupOpening(popup));
                }
            }
        }
        protected virtual IEnumerator ScrollAnimation()
        {
            yield return DragonRewardAnimation();

            Invoke("ShowCloseButton", 0.5f);

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

            if (fireworkObj != null)
            {
                yield return SBDefine.GetWaitForSeconds(0.35f);
                fireworkObj.SetActive(true);
            }

            
            yield break;
        }
        public override void ClosePopup()
        {
            if (!resultBtn.gameObject.activeInHierarchy)
                return;

			scrollView.horizontalNormalizedPosition = 0f;
            scrollView.content.sizeDelta = new Vector2(0, 0);
            scrollView.content.localPosition = new Vector3(0, scrollView.content.localPosition.y, 0);
            Data.Rewards.Clear();
            for(int i = 0, count = itemAnimators.Count; i < count; ++i)
            {
                if (itemAnimators[i] == null)
                    continue;

                itemPool.Put(itemAnimators[i].gameObject);
            }
			itemAnimators.Clear();
            
            dimmedClickAction = null;

            if (onClose != null)
                onClose.Invoke();
            base.ClosePopup();
        }

        public override void OnClickDimd()
        {
            if(dimmedClickAction != null)
            {
                dimmedClickAction.Invoke();
            }
            else
            {
                base.OnClickDimd();
            }
        }

        public void SetText(string titleString="", string clickBtnString="")
        {
            if(titleText !=null && titleString != "") { 
                titleText.text = titleString;
            }
            if(clickText !=null && clickBtnString != "") { 
                clickText.text = clickBtnString;
            }
        }

        public void SetDimmedClickAction(VoidDelegate action)
        {
            if(action != null) 
                dimmedClickAction = action;
        }

        public void SetHotTime(bool isHot)
        {
            if(hotTime != null)
                hotTime.SetActive(isHot);
        }
    }
}