using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class BatteryFactory : Building
    {
        protected ProductAutoData autoProductData = null;
        protected override void Start()
        {
            base.Start();

            if (Data != null)
            {
                var datas = ProductAutoData.GetListByGroupAndLevel(BName, Data.Level);
                if (datas != null)
                    autoProductData = datas[0];
            }
        }
        protected override void BuildingAction()
        {
            if (Data == null || spine == null)
                return;

            var queueList = User.Instance.GetProduces(Data.Tag);
            if (queueList == null || queueList.Items == null || queueList.Items.Count < 1)
            {
                if (0 != curProductCount)
                {
                    curProductCount = 0;
                    CheckProductAlarm();
                }

                SetProductState(ProductState.QUEUE_EMPTY);
                return;
            }

            int count = 0;
            int curExp = 0;

            ProductState animState = ProductState.COMPLETED_ALL;
            var itemCount = queueList.Items.Count;
            if (itemCount > 0)
            {
                var curItem = queueList.Items[0];
                if (curItem != null)
                {
                    if (curItem.ProductionExp > 0)
                    {
                        curExp = curItem.ProductionExp;
                    }

                    if (TimeManager.GetTimeCompare(curExp) > 0)
                    {
                        animState = ProductState.RUNNING;
                    }
                    else
                    {
                        count++;
                        while (autoProductData != null && queueList.Slot > count && curExp > 0)
                        {
                            count++;
                            curExp += Mathf.FloorToInt(autoProductData.MAX_TIME / queueList.Slot);
                            if (TimeManager.GetTimeCompare(curExp) > 0)
                            {
                                animState = ProductState.RUNNING;
                                break;
                            }
                        }
                    }

                    if (count != curProductCount)
                    {
                        curProductCount = count;
                        CheckProductAlarm();
                    }
                }
            }

            SetProductState(animState);
        }
    }
}