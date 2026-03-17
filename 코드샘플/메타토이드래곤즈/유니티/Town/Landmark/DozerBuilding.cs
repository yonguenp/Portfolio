using DG.Tweening;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class DozerBuilding : LandmarkBuilding, EventListener<LandmarkUpdateEvent>
    {
        private LandmarkDozer dozer = null;
        List<ProductReward> RewardList = null;
        public LandmarkDozer Dozer
        {
            get
            {
                if (dozer == null)
                    dozer = User.Instance.GetLandmarkData<LandmarkDozer>();

                return dozer;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventManager.RemoveListener(this);
        }

        private void OnEnable()
        {
            EventManager.AddListener(this);

            AddBuildProductUI();
            CheckProductAlarm();
        }

        public void OnEvent(LandmarkUpdateEvent eventData)
        {
            if (eventData.eLandmark != eLandmarkType.Dozer)
                return;

            CheckProductAlarm();
        }

        protected override void BuildingAction()
        {
            if (Dozer != null)
            {
                if(curState != ProductState.COMPLETED_ALL)
                    Dozer.Recall();

                if (TimeManager.GetTimeCompare(Dozer.ExpireTime) > 0)
                {
                    SetProductState(ProductState.RUNNING);
                }
                else
                {
                    SetProductState(ProductState.COMPLETED_ALL);
                }

                return;
            }

            SetProductState(ProductState.UNKNOWN);
        }

        public override void CheckProductAlarm()
        {
            if (BuildingProductUI != null)
            {
                BuildingProductUI.SetState(curState);
            }

            if (Dozer == null)
                return;
            
            if (RewardList == Dozer.RewardList)
                return;

            if (BuildingProductUI != null)
            {
                RewardList = Dozer.RewardList;

                Dictionary<eGoodType, Dictionary<int, int>> list = new Dictionary<eGoodType, Dictionary<int, int>>();
                for (int i = 0; i < RewardList.Count; i++)
                {
                    if (!list.ContainsKey(RewardList[i].GoodType))
                        list[RewardList[i].GoodType] = new Dictionary<int, int>();

                    if(!list[RewardList[i].GoodType].ContainsKey(RewardList[i].ItemNo))
                    {
                        list[RewardList[i].GoodType].Add(RewardList[i].ItemNo, RewardList[i].Amount);
                    }
                    else
                    {
                        list[RewardList[i].GoodType][RewardList[i].ItemNo] += RewardList[i].Amount;
                    }
                }

                List<ProductReward> param = new List<ProductReward>();
                foreach (var dic in list)
                {
                    foreach (var val in dic.Value)
                    {
                        param.Add(new ProductReward(dic.Key, val.Key, val.Value));
                    }
                }

                BuildingProductUI.SetProducts(param);
            }
        }

        public override void OnHarvest(eHarvestType harvestType)
        {
            Dozer.GetReward();
        }
    }
}