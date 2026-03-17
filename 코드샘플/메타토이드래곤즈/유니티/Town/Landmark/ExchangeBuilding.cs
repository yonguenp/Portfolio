using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ExchangeBuilding : LandmarkBuilding, EventListener<LandmarkUpdateEvent>
    {
        public ExchangeManager Exchange { get { return User.Instance.Exchange; } }
        private LandmarkExchange exchangeCenter = null;
        [SerializeField] private GameObject exchangeStatueEffect = null;
        public LandmarkExchange ExchangeCenter
        {
            get
            {
                if (exchangeCenter == null)
                    exchangeCenter = User.Instance.GetLandmarkData<LandmarkExchange>();

                return exchangeCenter;
            }
        }
        bool isIdle = false;

        protected override void OnDisable()
        {
            base.OnDisable();
            EventManager.RemoveListener(this);
        }

        private void OnEnable()
        {
            EventManager.AddListener(this);
            AddBuildProductUI();

            CancelInvoke("RefreshExchangeState");
            Invoke("RefreshExchangeState", 0.1f);

            isIdle = false;
            SetAnimation(0, "off", true);
            exchangeStatueEffect.SetActive(false);
        }

        public void OnEvent(LandmarkUpdateEvent eventData)
        {
            if (eventData.eLandmark == eLandmarkType.EXCHANGE)
            {
                RefreshExchangeState();
            }
            
        }
        public override bool ActiveAction()
        {
            if(base.ActiveAction())
            {
                if (!isIdle)
                {
                    SetAnimation(0, "idle", true);
                    exchangeStatueEffect.SetActive(true);
                    isIdle = true;
                }                
            }
            else
            {
                if (isIdle)
                {
                    SetAnimation(0, "off", true);
                    exchangeStatueEffect.SetActive(false);
                    isIdle = false;
                }                
            }

            return false;
        }
        protected override void SetProductState(ProductState state)
        {

        }
        public override void CheckProductAlarm()
        {

        }

        public void OnEvent(UIObjectEvent eventType)
        {
            switch (eventType.e)
            {
                case UIObjectEvent.eEvent.ITEM_USE:
                case UIObjectEvent.eEvent.ITEM_GET:
                    CancelInvoke("RefreshExchangeState");
                    Invoke("RefreshExchangeState", 0.1f);
                    ActiveAction();
                    break;                   
            }
        }

        public void RefreshExchangeState()
        {
            var building = User.Instance.GetUserBuildingInfoByTag((int)eLandmarkType.EXCHANGE);
            if(building != null)
            {
                if(building.State == eBuildingState.NORMAL)
                    Exchange.Prepare(RefreshState);
            }            
        }

        void RefreshState()
        {
            List<UserDragon> exchangableDragons = new List<UserDragon>();
            BuildingProductUI.Clear();
            foreach (var ex in Exchange.Exchange)
            {
                if (ex.State != SandboxNetwork.Exchange.EXCHANGE_STATE.BEGGING)
                    continue;

                Dictionary<int, Item> needitems = new Dictionary<int, Item>();

                for (int i = 0; i < ex.NeedItemInfo.Count; i++)
                {
                    if (ex.NeedItemInfo[i] != null)
                    {
                        int groupNo = ex.NeedItemInfo[i].GROUP;
                        var exchangeData = Exchange.GetExchangeData(groupNo);
                        if (exchangeData != null)
                        {
                            int required = ex.NeedItemInfo[i].REQUIRED_NUMBER;

                            if (needitems.ContainsKey(exchangeData.NEED_ITEM.KEY))
                                needitems[exchangeData.NEED_ITEM.KEY].AddCount(required);
                            else
                                needitems[exchangeData.NEED_ITEM.KEY] = new Item(exchangeData.NEED_ITEM.KEY, required);
                        }
                    }
                }

                bool enableExchange = true;
                foreach (var need in needitems)
                {
                    enableExchange &= User.Instance.GetItemCount(need.Key) >= need.Value.Amount;
                }

                if(enableExchange)
                    exchangableDragons.Add(User.Instance.DragonData.GetDragon(ex.dragon_no));
            }

            BuildingProductUI.SetData(exchangableDragons);
        }

        public override void OnHarvest(eHarvestType harvestType)
        {
            //추수없음
        }

        public override bool TryHarvest(eHarvestType harvestType)
        {
            //추수없음
            return false;
        }
    }
}
