using Newtonsoft.Json.Linq;
using Coffee.UIEffects;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;

namespace SandboxNetwork
{
    public class LandMarkPopup : BuildingZoomPopup
    {
        [SerializeField]
        private BuildingLayer[] tabLayers = null;

        [SerializeField]
        private GameObject buildInfoFrame = null;
        [SerializeField]
        private GameObject buildCardParent = null;
        [SerializeField]
        private GameObject[] buidlingObjs = null;

        [SerializeField]
        private Transform travelWaitingDragonTr = null;

        [Header("Landmark Arrow")]
        [SerializeField]
        private GameObject defaultArrowParent = null;
        [SerializeField]
        private GameObject subwayArrowParent = null;
        [SerializeField]
        private GameObject exchangeArrowParent = null;

        private RenderTexture blurTexture = null;
        private UIEffect effect = null;
        private List<UIDragonSpine> dragonSpines = null;

        public bool buttonLock { get; private set; } = false;

        private int currentWaitingDragonCount = 0;
        public void ButtonLock()
        {
            CancelInvoke("ButtonUnlock");
            buttonLock = true;
            Invoke("ButtonUnlock", 1.0f);
        }
        public void ButtonUnlock()
        {
            CancelInvoke("ButtonUnlock");
            buttonLock = false;
        }

        #region OpenPopup
        public static LandMarkPopup OpenPopup(eLandmarkType landmarkType)
        {
            return OpenPopup(new BuildingPopupData((int)landmarkType));
        }
        public static LandMarkPopup OpenPopup(int landmarkType)
        {
            return OpenPopup(new BuildingPopupData(landmarkType));
        }
        public static LandMarkPopup OpenPopup(BuildingPopupData data)
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<LandMarkPopup>(data);
        }

        public BuildingLayer GetCurLayer() { return tabLayers[CurIndex()]; }
        #endregion
        public eLandmarkType CurLandMarkType 
        { 
            get {
                switch (CurIndex())
                {
                    case 0: return eLandmarkType.Dozer;
                    case 1: return eLandmarkType.Travel;
                    case 2: return eLandmarkType.SUBWAY;
                }

                return eLandmarkType.UNKNOWN;
            }
        }

        protected override int CurIndex()
        {
            if (Data == null)
                return -1;

            return (Data.BuildingTag / 100) - 1;
            //dozer 0
            //travel 1
            //subway 2
            //exchange 3
        }

        protected override void SetBuildingTags()
        {
            BuildingTags = Enum.GetValues(typeof(eLandmarkType)).Cast<int>().ToList();
        }
        
        public override void ForceUpdate(BuildingPopupData data)
        {
            if(data == null)
            {
                Refresh();
                return;
            }

            base.DataRefresh(data);
            Refresh();
        }

        public override void InitUI()
        {
            ButtonUnlock();

            base.InitUI();

            ClearTravelWaitingDragons();

            if (buildingSkeletons == null)
            {
                buildingSkeletons = new List<SkeletonGraphic>();   
                foreach (var building in buidlingObjs)
                {
                    var skeletonGraphic = building.GetComponentInChildren<SkeletonGraphic>();
                    if (skeletonGraphic != null)
                    {
                        buildingSkeletons.Add(skeletonGraphic);
                    }
                }
            }

            List<Transform> buildingSpineTransforms = new List<Transform>();
            foreach (var obj in buildingSkeletons)
            {
                buildingSpineTransforms.Add(obj.transform);
            }
            Town.Instance.ZoomBuilding(Data.BuildingTag);
            Refresh(true);

            foreach (MaskableGraphic graphic in buildInfoFrame.GetComponentsInChildren<MaskableGraphic>())
            {
                if (buildingSpineTransforms.Contains(graphic.transform))
                    continue;

                Color color = graphic.color;
                color.a = 0.0f;
                graphic.color = color;

                graphic.DOFade(1.0f, 0.3f).SetDelay(0.2f);
            }

            if (Data.BuildingTag == (int)eLandmarkType.SUBWAY)
                return;

            RefreshSpineBg();
        }
        public override void Refresh()
        {
            if (!gameObject.activeInHierarchy)
                return;

            base.Refresh();

            ClearObj();

            int cur = CurIndex();
            if (cur < 0 || tabLayers.Length <= cur)
            {
                Debug.LogWarning("invalid landmark building index!");
                return;
            }

            if (tabLayers.Length > cur) {
                ClearTravelWaitingDragons();
                tabLayers[cur].gameObject.SetActive(true);
                tabLayers[cur].InitData(Data);

                if (buidlingObjs != null && buidlingObjs.Length>cur && buidlingObjs[cur] != null)
                {
                    buidlingObjs[cur].SetActive(true);
                }

                SetCurrentBuildingSpine(cur);
            }

            // 지하철 탭 별도 처리
            
            SetArrowParentObject(Data.BuildingTag);
        }

        void SetArrowParentObject(int buildingTag )
        {
            defaultArrowParent.SetActive(false);
            subwayArrowParent.SetActive(false);
            exchangeArrowParent.SetActive(false);
            buildCardParent.SetActive(true);
            
            switch ((eLandmarkType)buildingTag)
            {
                case eLandmarkType.SUBWAY:
                    buildCardParent.SetActive(false);
                    subwayArrowParent.SetActive(true);
                    break;
                case eLandmarkType.Dozer:
                    defaultArrowParent.SetActive(true);
                    break;
                case eLandmarkType.Travel:
                    defaultArrowParent.SetActive(true);
                    break;
                case eLandmarkType.EXCHANGE:
                    buildCardParent.SetActive(false);
                    exchangeArrowParent.SetActive(true);
                    break;
            }
            
            
        }

        public void Refresh(bool animStart)
        {
            Refresh();

            if (tabLayers[CurIndex()].GetComponent<Animation>() != null && animStart)
            {
                tabLayers[CurIndex()].GetComponent<Animation>().Play();
            }
        }

        public override void OnClickMoveTab(int index)
        {
            if (buttonLock)
                return;

            int count = tabLayers.Length;
            if (Mathf.Abs(index)>=count ||index ==0)
            {
                return;
            }
            int cIndex = (CurIndex() + index + count) % count;
			int nextTag = 0;
            
            switch (cIndex)
            {
                case 0:
					nextTag = (int)eLandmarkType.Dozer;
					break;

                case 1:
					nextTag = (int)eLandmarkType.Travel;
					break;

                case 2:
					nextTag = (int)eLandmarkType.SUBWAY;
					break;
                case 3:
                    nextTag = (int)eLandmarkType.EXCHANGE;
                    break;
            }
            var buildingData = User.Instance.GetUserBuildingInfoByTag(nextTag);
            if (buildingData.State == eBuildingState.NORMAL)
            {
                OnChangeTargetBulding(nextTag);
            }
            else
            {
                OnClickMoveTab((index > 0) ? index +1 : index - 1);
            }

        }

        void ClearObj()
        {
            foreach (var obj in buidlingObjs)
            {
                obj.SetActive(false);
            }
            foreach (var obj in tabLayers)
            {
                obj.gameObject.SetActive(false);
            }
        }


        #region 여행사 연출 관련 
        // 원래 사용하려했던 용도 - 드래곤 선택하면 건물 UI에 드래곤들이 생성되고 그 드래곤을 이용한 연출 목적으로 짜놓은 코드 - 혹시모르니깐 남겨둠
        public void SetTravelWaitingDragons(List<UserDragon> userDragons = null)
        {
            
            if(dragonSpines ==null)
                dragonSpines = new List<UIDragonSpine>();
            else
                ClearTravelWaitingDragons();
            if (userDragons == null || userDragons.Count==0)
            {
                return;
            }
            currentWaitingDragonCount = userDragons.Count;
            for (int i =0; i < currentWaitingDragonCount; ++i)
            {
                if(i >= dragonSpines.Count)
                {
                    var clone = Instantiate(userDragons[i].BaseData.GetUIPrefab(), travelWaitingDragonTr);
                    clone.transform.localScale = new Vector3(-2, 2, 1);
                    UIDragonSpine spine = clone.GetComponent<UIDragonSpine>();
                    dragonSpines.Add(spine);
                }
                if (dragonSpines[i].GetComponent<CanvasGroup>() == null)
                {
                    dragonSpines[i].gameObject.AddComponent<CanvasGroup>().alpha = 1f;
                }
                else
                {
                    dragonSpines[i].GetComponent<CanvasGroup>().alpha = 1;
                }
                
                dragonSpines[i].gameObject.SetActive(true);
                dragonSpines[i].SetData(userDragons[i]);
            }
            
            SetDragonsSpineState(eSpineAnimation.IDLE);
        }
        public void AddTravelWaitingDragons(UserDragon userDragon) //미사용
        {
            
            var clone = Instantiate(userDragon.BaseData.GetUIPrefab(), travelWaitingDragonTr);
            clone.transform.localScale = new Vector3(-2, 2, 1);
            UIDragonSpine spine = clone.GetComponent<UIDragonSpine>();
            if (spine != null)
            {
                dragonSpines.Add(spine);
                spine.SetData(userDragon);
            }
        }

        public void ClearTravelWaitingDragons()
        {
            if(dragonSpines != null) { 
                foreach(var spine in dragonSpines)
                {
                    spine.gameObject.SetActive(false);
                }
                
            }
        }

        void SetDragonsSpineState(eSpineAnimation animation)
        {
            if (dragonSpines == null || dragonSpines.Count ==0)
            {
                return;
            }
            foreach(UIDragonSpine spine in dragonSpines)
            {
                if (spine != null) { 
                    spine.SetAnimation(animation);
                }
            }
        }
        public void DragonGoTravel()
        {
            if (dragonSpines == null || dragonSpines.Count == 0)
            {
                return;
            }
            SetBuildingSpine("wait");
            Sequence sequence = DOTween.Sequence();
            foreach (UIDragonSpine spine in dragonSpines)
            {
                var objCanvasGroup = spine.GetComponent<CanvasGroup>();
                if (objCanvasGroup == null)
                {
                    objCanvasGroup = spine.gameObject.AddComponent<CanvasGroup>();
                }
                spine.SetAnimation(eSpineAnimation.WALK);
                sequence.Append(spine.GetComponent<RectTransform>().DOLocalMoveX(-100f, 0.5f).OnComplete(() => { objCanvasGroup.DOFade(0f, 0.5f); }));
                sequence.AppendInterval(0.5f);
            }
        }

        #endregion
        public override void OnClickDimd()
        {
            if (buttonLock)
                return;

            base.OnClickDimd();
        }
        #region 건물 스파인 관련
        protected override void RefreshCurrentBuildingSpine(bool animStart = false)
        {
            SetCurrentBuildingSpine(CurIndex());
        }

        public void SetBuildingSpine(string spineName)
        {
            var spine = GetSpine(CurIndex());
            if(spine != null)
                spine.AnimationState.SetAnimation(0, spineName, true);            
        }
        protected override void SetCurrentBuildingSpine(int index)
        {
            Building building = GetTownBuilding(Data.BuildingTag);
            if (building == null)
            {
                Debug.LogError("invalid landmark town building !");
                return;
            }

            if (building.Data.State == eBuildingState.NORMAL)
                building.RefreshBuildingAction();

            SkeletonAnimation buildingSpine = building.GetSpine();
            SkeletonGraphic spine = GetSpine(index);
            string animName = "off";
            float animTime = 0.0f;
            float timeScale = 1.0f;
            bool loop = false;

            if (spine == null)
            {
                Debug.LogWarning("invalid landmark spine !");
                return;
            }

            if (buildingSpine != null)
            {
                animName = buildingSpine.AnimationName;
                animTime = buildingSpine.AnimationState.GetCurrent(0).TrackTime;
                timeScale = buildingSpine.timeScale;
                loop = buildingSpine.loop;
            }
            else
            {
                Debug.LogWarning("invalid landmark ui spine !");
            }

            var data = spine.SkeletonDataAsset.GetSkeletonData(true);
            if (data != null)
            {
                if (data.FindAnimation(animName) != null)
                {
                    var Track = spine.AnimationState.SetAnimation(0, animName, loop);
                    Track.TrackTime = animTime;
                    spine.timeScale = timeScale;
                }
                else
                {
                    Debug.LogWarning("not found landmark skeleton data !");
                }
            }
            else
            {
                Debug.LogWarning("load fail landmark skeleton data !");
            }
        }
        #endregion

        public override void ClosePopup()
        {
            int cur = CurIndex();
            if (cur < 0 || tabLayers.Length <= cur)
            {
                RunClose();
                return;
            }

            if (tabLayers.Length > cur)
            {
                if (tabLayers[cur].IsCloseAble())
                {
                    RunClose();
                }
                else
                {
                    tabLayers[cur].CloseAction();
                }

                return;
            }

            RunClose();
        }

        void RunClose()
        {
            base.ClosePopup();
            UITrainStateEvent.PopupCloseEvent();
        }

        public void BuildingSpineRefresh()
        {
            SetCurrentBuildingSpine(CurIndex());
        }
    }
}

