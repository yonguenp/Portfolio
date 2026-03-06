using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SandboxNetwork { 
    public class StageNode : MonoBehaviour
    {

        public int worldNo { get; private set; } = 0;
        public int stageNo { get; private set; } = 0;
        [SerializeField] private Transform roadParent = null;
        
        [Header("Object")]
        [SerializeField] private Image sprImage = null;
        [SerializeField] private Sprite[] colorImages = null;
        [SerializeField] private Sprite[] grayImages = null;
        [SerializeField] private Text labelNode = null;
        [SerializeField] private Image labelBack = null;
        [SerializeField] private Color[] labelBackColors = null;
        [Header("Locked")]
        [SerializeField] private Image sprLock = null;
        [SerializeField] private Image sprShadow = null;
        [Header("Stars")]
        [SerializeField] private GameObject starNode = null;
        [SerializeField] private List<GameObject> starsNode = new List<GameObject>();



        public Transform RoadParent { get { return roadParent; } }
        public Transform NodeParent { get { if(sprImage ==null) return null;  return sprImage.transform; } }
        
        public delegate void Callback(int worldIndex, int stageIndex);
        Callback pointDownCallback = null;
        Callback clickCallback = null;


        private Sprite colorImage = null;
        private Sprite grayImage = null;

        private ScrollRect stageScroll = null;
        Tween tween = null;
        public bool First { get; private set; } = false;
        public bool Lock { get; private set; } = false;
        public int Diff { get; private set; } = 1;
        public int Star { get; private set; } = 0;

        bool isBossStage = false;

        bool isScrolling = false;
        public void SetStage(ScrollRect scrollRect, bool first, bool locked, int world, int stage, int diff = 1, int star = 0)
        {
            stageScroll = scrollRect;
            First = first;
            Lock = locked;
            worldNo = world;
            stageNo = stage;
            Diff = diff;
            Star = star;
            colorImage = colorImages[world - 1];
            grayImage = grayImages[world - 1];
            Refresh();
        }


        public void SetClickCallBack(Callback clickCallBack, Callback pointDownCallBack)
        {
            if(clickCallBack != null)
            {
                clickCallback = clickCallBack ; 
            }
            if (pointDownCallBack != null)
            {
                 pointDownCallback = pointDownCallBack;
            }
        }

        private void Refresh()
        {
            sprImage.sprite = Lock ? grayImage : colorImage;
            sprLock.gameObject.SetActive(Lock);
            sprShadow.gameObject.SetActive(Lock);
            starNode.SetActive(!Lock);
            if (!Lock)
            {
                for (int i = 0; i < starsNode.Count; ++i)
                {
                    if (starsNode[i] == null) return;
                    starsNode[i].SetActive(i < Star);
                }
            }

            labelBack.color = labelBackColors[worldNo-1];
            labelNode.text = string.Format("{0}-{1}", worldNo.ToString(), stageNo.ToString());
        }

        public void SetHighLight()
        {
            tween = sprImage.transform.DOScale(Vector3.one * 1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
        public void OffHighLight()
        {
            if (tween != null)
            {
                tween.Kill();
                sprImage.transform.localScale = Vector3.one;
            }
        }
        private void OnDisable()
        {
            OffHighLight();
        }

        public void OnClickStage()
        {
            if (Lock || isScrolling) 
                return;

            StageInfoPopupData newPopupData = new StageInfoPopupData(worldNo, stageNo, Diff, Star);

            PopupManager.OpenPopup<AdventureReadyPopup>(newPopupData);
            clickCallback?.Invoke(worldNo, stageNo - 1);
        }

        public void OnPointDown()
        {
            if(Lock) return;
            if (pointDownCallback !=null) {
                pointDownCallback(worldNo,stageNo-1);
            }
        }
        public void OnDrag(BaseEventData e)
        {
            stageScroll.OnDrag((PointerEventData)e);
        }
        public void OnBeginDrag(BaseEventData e)
        {
            isScrolling = true;
            stageScroll.OnBeginDrag((PointerEventData)e);
        }
        public void OnEndDrag(BaseEventData e)
        {
            isScrolling = false;
            stageScroll.OnEndDrag((PointerEventData)e);
        }
    }
}