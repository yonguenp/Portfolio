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
    public class ProductPopup : BuildingZoomPopup
    {
        [SerializeField]
        private ProductLayer productLayer = null;

        [SerializeField]
        private BatteryLayer batteryLayer = null;

        [SerializeField]
        private GameObject buildInfoFrame = null;
        [SerializeField]
        private GameObject[] buidlingObjs = null;

        [SerializeField]
        RectTransform buildingInfoTr = null;

        [SerializeField]
        private GameObject arrowNode = null;

        private BuildingLayer CurLayer = null;
        List<Transform> buildingSpineTransforms = new List<Transform>();
        bool isBattery = false;

        #region OpenPopup
        private static int GetBuildingTagByKey(string buildingKey, int level = -1)
        {
            List<BuildingOpenData> buildingList = BuildingOpenData.GetByBuildingGroup(buildingKey);

            foreach (BuildingOpenData bOpen in buildingList)
            {
                if (level < 0)
                {
                    return bOpen.INSTALL_TAG;
                }
                else if (User.Instance.GetUserBuildingInfoByTag(bOpen.INSTALL_TAG).Level < level)
                {
                    return bOpen.INSTALL_TAG;
                }
            }

            return -1;
        }
        public static ProductPopup OpenPopup(QuestTriggerData questtriggerData)
        {
            if (questtriggerData == null)
                return null;

            int key = GetBuildingTagByKey(questtriggerData.SUB_TYPE, int.Parse(questtriggerData.TYPE_KEY));
            if (key < 0)
                return null;

            return OpenPopup(key);
        }
        public static ProductPopup OpenPopup(string buildingKey)
        {
            int key = GetBuildingTagByKey(buildingKey);
            if (key < 0)
                return null;

            return OpenPopup(key);
        }
        public static ProductPopup OpenPopup(int buildingKey)
        {
            return OpenPopup(new BuildingPopupData(buildingKey));
        }
        public static ProductPopup OpenPopup(BuildingPopupData data)
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<ProductPopup>(data);
        }
        #endregion
        protected override void SetBuildingTags()
        {
            BuildingTags = User.Instance.GetCurProductBuildingTagList();
        }

        public override void InitUI()
        {
            base.InitUI();
            if (buildingSkeletons == null)
            {
                buildingSkeletons = new List<SkeletonGraphic>();
                foreach (var building in buidlingObjs)
                {
                    var skeletonGraphic = building.GetComponentInChildren<SkeletonGraphic>();
                    buildingSkeletons.Add(skeletonGraphic);
                }
            }

            foreach (var obj in buildingSkeletons)
            {
                buildingSpineTransforms.Add(obj.transform);
            }

            foreach (MaskableGraphic graphic in buildInfoFrame.GetComponentsInChildren<MaskableGraphic>())
            {
                if (buildingSpineTransforms.Contains(graphic.transform))
                    continue;

                Color color = graphic.color;
                color.a = 0.0f;
                graphic.color = color;

                graphic.DOFade(1.0f, 0.3f).SetDelay(0.2f);
            }

            int buildingTag = Data.BuildingTag;
            batteryLayer.gameObject.SetActive(false);
            productLayer.gameObject.SetActive(false);
            
            List<int> batteryTags = new List<int>();
            foreach (var item in BuildingOpenData.GetByBuildingGroup("exp_battery"))
            {
                batteryTags.Add(item.INSTALL_TAG);
            }
            if (Data.BuildingKey =="exp_battery")
            {
                isBattery = true;
                batteryLayer.gameObject.SetActive(true);
                BuildingTags = BuildingTags.Intersect(batteryTags).ToList(); //교집합 ( 내가 가진 건물 TAG ) & ( Battery 공장 TAG )
                CurLayer = batteryLayer;
                
                buildingInfoTr.localPosition = Vector3.up * 34;
                batteryLayer.SetBuildingTab(Data, BuildingTags);
                batteryLayer.SetQueueTabClickCallBack(OnQueueTab);
                batteryLayer.InitUI(null);
            }
            else
            {
                isBattery = false;
                productLayer.gameObject.SetActive(true);
                BuildingTags.RemoveAll(batteryTags.Contains);
                buildingInfoTr.localPosition = new Vector3(-680,34);
                CurLayer = productLayer;
               
                productLayer.SetBuildingTab(Data);
                productLayer.SetQueueTabClickCallBack(OnQueueTab);
                productLayer.InitUI(null);
            }
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);
            SetIndex(BuildingTags.IndexOf(Data.BuildingTag));

            OnChangeTargetBulding(buildingTag);

            RefreshCurrentBuildingSpine(true);

            RefreshSpineBg();
        }

        private void OnQueueTab(int tag)
        {
            OnChangeTargetBulding(tag);
        }

        public override void Refresh()
        {
            base.Refresh();

            RefreshCurrentBuildingSpine();
            if (Data.BuildingTag == 0) return;
            if (isBattery)
            {
                batteryLayer.SetBuildingTab(Data, BuildingTags);
                batteryLayer.RefreshUI();
            }
            else
            {
                productLayer.SetBuildingTab(Data);
                productLayer.RefreshUI();
            }
            RefreshArrowNode();//건설 화살표 갱신
        }

        protected override void RefreshCurrentBuildingSpine(bool animStart = false)
        {
            ClearObj();

            if (CurLayer.GetComponent<Animation>() != null && animStart)
            {
                CurLayer.GetComponent<Animation>().Play();
            }

            var buildingName = Data.BuildingKey;
            int tapIndex = 0;
            foreach(var obj in buidlingObjs)
            {
                if (obj.name == buildingName)
                {
                    obj.SetActive(true); break;
                }
                ++tapIndex;
            }

            SetCurrentBuildingSpine(Data.BuildingTag, tapIndex);
        }
        
        void ClearObj()
        {
            foreach (var obj in buidlingObjs)
            {
                obj.SetActive(false);
            }
        }
        public override void OnClickMoveTab(int index)
        {
            base.OnClickMoveTab(index);
            CurLayer.RefreshUI();
        }

        void RefreshArrowNode()//건설 건전지가 1개 이하일때, 일반 생산 1개 이하 일때 화살표 끄기
        {
            if (arrowNode == null)
                return;

            if (BuildingTags == null || BuildingTags.Count <= 0)
            {
                arrowNode.SetActive(false);
                return;
            }
            arrowNode.SetActive(BuildingTags.Count > 1);
        }
        protected override void SetCurrentBuildingSpine(int tag, int curTapNubmer)
        {
            if (buildingSkeletons.Count > curTapNubmer)
            {
                Building building = GetTownBuilding(Data.BuildingTag);
                if (building == null)
                    return;

                building.RefreshBuildingAction();

                SkeletonAnimation buildingSpine = building.GetSpine();
                var animName = buildingSpine.AnimationName;
                var animTime = buildingSpine.AnimationState.GetCurrent(0).TrackTime;
                var timeScale = buildingSpine.timeScale;
                var loop = buildingSpine.loop;

                var buildingSkeletonData = GetSpine(curTapNubmer);
                if (buildingSkeletonData != null)
                {
                    buildingSkeletonData.Clear();
                    var Track = buildingSkeletonData.AnimationState.SetAnimation(0, animName, loop);
                    Track.TrackTime = animTime;
                    buildingSkeletonData.timeScale = timeScale;
                }
            }
        }
    }
}

