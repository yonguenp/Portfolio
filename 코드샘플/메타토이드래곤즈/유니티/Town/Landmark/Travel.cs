using DG.Tweening;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class Travel : LandmarkBuilding, EventListener<TravelDepartureEvent>, EventListener<LandmarkUpdateEvent>
    {

        private Coroutine animationCoroutine = null;
        private LandmarkTravel travel = null;
        public LandmarkTravel TravelData
        {
            get
            {
                if (travel == null)
                    travel = User.Instance.GetLandmarkData<LandmarkTravel>();

                return travel;
            }
        }


        void OnEnable()
        {
            EventManager.AddListener<TravelDepartureEvent>(this);
            EventManager.AddListener<LandmarkUpdateEvent>(this);

            AddBuildProductUI();
            CheckProductAlarm();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            EventManager.RemoveListener<TravelDepartureEvent>(this);
            EventManager.RemoveListener<LandmarkUpdateEvent>(this);
        }

        protected override void BuildingAction()
        {
            if (TravelData != null)
            {
                ProductState animState = ProductState.UNKNOWN;
                switch (TravelData.TravelState)
                {
                    case eTravelState.Normal:
                    {
                        animState = ProductState.QUEUE_EMPTY;
                    }
                    break;
                    case eTravelState.Travel:
                    {
                        animState = ProductState.RUNNING;
                        var time = TimeManager.GetTimeCompare(TravelData.TravelTime);
                        if (time < 0)
                        {
                            TravelData.SetTravelState(eTravelState.Complete);
                            animState = ProductState.COMPLETED_ALL;
                        }
                    }
                    break;
                    case eTravelState.Complete:
                    {
                        animState = ProductState.COMPLETED_ALL;
                    }
                    break;
                    case eTravelState.None:
                    default:
                    {
                        animState = ProductState.UNKNOWN;
                    }
                    break;
                }

                SetProductState(animState);
                return;
            }

            SetProductState(ProductState.UNKNOWN);
        }

        public void OnEvent(TravelDepartureEvent eventType)
        {
            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);

            //animationCoroutine = StartCoroutine(DepartureAnimationCoroutine());
        }

        public void OnEvent(LandmarkUpdateEvent eventData)
        {
            if (eventData.eLandmark != eLandmarkType.Travel)
                return;

            CheckProductAlarm();
            PopupManager.ForceUpdate<LandMarkPopup>();
        }

        public override void CheckProductAlarm()
        {
            if (BuildingProductUI != null)
            {
                BuildingProductUI.SetState(curState);
            }

            if (TravelData == null)
                return;

            if (TravelData.TravelState != eTravelState.Complete)
                return;

            if (TravelData.TravelDragon.Count <= 0)
                return;

            if (BuildingProductUI != null)
            {
                BuildingProductUI.SetData(TravelData.TravelDragon);
            }
        }

        IEnumerator DepartureAnimationCoroutine()
        {
            var track = SetAnimation(0, "open", false);
            yield return new WaitForSpineAnimationEnd(track);
            //track = SetAnimation(0, "wait", false);
            //yield return new Spine.Unity.WaitForSpineAnimationComplete(track);
            SetAnimation(0, "Closed", false);
            yield return new WaitForSpineAnimationEnd(track);
            SetAnimation(0, "play", true);
        }
        public override void OnHarvest(eHarvestType harvestType)
        {
            TravelData.GetReward();
        }

        protected override void SetProductState(ProductState state)
        {
            if (curState == state)
                return;

            curState = state;
            CheckProductAlarm();

            float timeScale = 1.0f;
            string anim = "off";
            bool loop = false;
            Color color = Color.white;
            switch (curState)
            {
                case ProductState.QUEUE_EMPTY:
                    anim = "play";
                    loop = true;
                    timeScale = 1.0f;
                    color = Color.gray;
                    break;
                case ProductState.COMPLETED_ALL:
                    anim = "wait";
                    loop = true;
                    timeScale = 1.0f;
                    color = Color.gray;
                    break;
                case ProductState.RUNNING:
                    anim = "wait";
                    loop = true;
                    timeScale = 1.0f;
                    color = Color.white;
                    break;

                case ProductState.UNKNOWN:
                default:
                    anim = "off";
                    loop = false;
                    timeScale = 0.0f;
                    color = Color.gray;
                    break;
            }

            spine.timeScale = timeScale;
            //spine.Skeleton.SetColor(color);
            SetAnimation(0, anim, loop);
        }
    }
}