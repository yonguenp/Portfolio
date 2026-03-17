using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SandboxNetwork
{   
    public class UIEventBanner : MonoBehaviour, EventListener<SettingEvent>, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField]
        GameObject sample_BannerItem = null;

        [SerializeField]
        Transform EventBannerParentTr;

        Vector2 touchPos = Vector2.zero;
        int curIndex = 0;
        bool isAnimate = false;
        List<EventBannerData> bannerList = new List<EventBannerData>();

        private void Start()
        {
            EventManager.AddListener<SettingEvent>(this);
        }
        private void OnDestroy()
        {
            EventManager.RemoveListener<SettingEvent>(this);
        }

        public void Init()
        {
            // 데이터 세팅 등 무언가의 작업
            SetEventBanner(true);
        }
        private void Clear()
        {
            bannerList.Clear();
            foreach (Transform item in EventBannerParentTr)
            {
                Destroy(item.gameObject);
            }
        }

        void SetEventBanner(bool init = false)
        {
            Clear();

            var banners = EventBannerData.GetBanners();
            if(banners.Count <= 0)
            {
                gameObject.SetActive(false);
                return;
            }

            if(init)
                gameObject.SetActive(true);

            sample_BannerItem.SetActive(true);
            if(banners.Count > 1)//fake rolling
            {
                GameObject clone = Instantiate(sample_BannerItem, EventBannerParentTr); // 이벤트 배너 오브젝트 생성                 
                SetSprite(clone.GetComponent<Image>(), banners[banners.Count - 1]);
            }

            foreach (var banner in banners)
            {
                bannerList.Add(banner);
                GameObject clone = Instantiate(sample_BannerItem, EventBannerParentTr); // 이벤트 배너 오브젝트 생성                 
                SetSprite(clone.GetComponent<Image>(), banner);
                clone.GetComponent<Image>().sprite = banner.SPRITE;
            }

            if (banners.Count > 1)//fake rolling
            {
                GameObject clone = Instantiate(sample_BannerItem, EventBannerParentTr); // 이벤트 배너 오브젝트 생성                 
                SetSprite(clone.GetComponent<Image>(), banners[0]);
            }
            sample_BannerItem.SetActive(false);

            CancelInvoke("OnNextMenu");
            if (banners.Count <= 1)
                Focus(curIndex);
            else
                Invoke("OnNextMenu", 3.0f);
        }

        void SetSprite(Image image, EventBannerData data)
        {
            if (data.SPRITE != null)
                image.sprite = data.SPRITE;
            else
                CDNManager.TrySetBannerCatchDefault(data.RESOURCE, "event", image);
        }

        public void SetActive(bool active)
        {
            switch (active)
            {
                case true:
                    // 데이터 세팅 등 무언가의 작업
                    SetEventBanner(true);
                    break;

                case false:
                    gameObject.SetActive(false);
                    break;
            }
        }

        private void OnPrevMenu()
        {
            if (isAnimate)
                return;

            CancelInvoke("OnNextMenu");

            Focus(curIndex - 1);

            Invoke("OnNextMenu", 3.0f);
        }

        private void OnNextMenu()
        {
            if (isAnimate)
                return;

            CancelInvoke("OnNextMenu");

            Focus(curIndex + 1);

            Invoke("OnNextMenu", 3.0f);
        }
        public void OnPointerDown(PointerEventData data)
        {
            CancelInvoke("OnNextMenu");
            touchPos = data.position;

            foreach (MaskableGraphic graphic in EventBannerParentTr.GetComponentsInChildren<MaskableGraphic>())
            {
                Color ogirin = Color.white;
                ogirin = ogirin * 0.7843137f;
                ogirin.a = 1.0f;
                graphic.color = ogirin;
            }
        }
        public void OnPointerUp(PointerEventData data)
        {
            foreach (MaskableGraphic graphic in EventBannerParentTr.GetComponentsInChildren<MaskableGraphic>())
            {
                Color ogirin = Color.white;
                graphic.color = ogirin;
            }

            if (isAnimate)
                return;

            Vector2 diff = data.position - touchPos;

            if (Mathf.Abs(diff.x) > (transform as RectTransform).sizeDelta.x * 0.5f)
            {
                if (diff.x < 0)//to right
                {
                    OnNextMenu();
                }
                else//to left
                {
                    OnPrevMenu();
                }
            }
            else if (!data.dragging && data.pointerCurrentRaycast.gameObject != null)
            {
                UIAction();
            }

            CancelInvoke("OnNextMenu");
            if (bannerList.Count <= 1)
                Focus(curIndex);
            else
                Invoke("OnNextMenu", 3.0f);
        }

        public void Focus(int idx)
        {
            if (bannerList.Count < 2)
            {
                EventBannerParentTr.localPosition = new Vector3(0f, EventBannerParentTr.localPosition.y, EventBannerParentTr.localPosition.z);
                return;
            }

            int max = bannerList.Count;
            var widthOffset = sample_BannerItem.GetComponent<RectTransform>().sizeDelta.x * -1.0f;
            var pos = widthOffset * max;

            isAnimate = true;
            EventBannerParentTr.DOLocalMoveX(pos * ((float)idx / max), 1.0f).OnComplete(() => {
                if (idx > max)
                    idx = 1;
                if (idx <= 0)
                    idx = max;

                curIndex = idx;

                EventBannerParentTr.localPosition = new Vector3(pos * ((float)curIndex / max), EventBannerParentTr.localPosition.y, EventBannerParentTr.localPosition.z);
                isAnimate = false;                
            });

            curIndex = idx;
        }

        public void UIAction()
        {
            int max = bannerList.Count - 1;

            int dataIndex = curIndex - 1;
            if (dataIndex < 0)
                dataIndex = max;
            if (bannerList.Count <= dataIndex && bannerList.Count != 0)
                dataIndex = dataIndex % bannerList.Count;

            if (bannerList.Count <= dataIndex || dataIndex < 0)
                return;

            var curData = bannerList[dataIndex];
            if (curData == null)
                return;

            SBFunc.InvokeCustomAction(curData.ACTION, curData.ACTION_PARAM);
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
        public void OnEvent(SettingEvent eventType)
        {
            SetEventBanner();
        }
    }

}
