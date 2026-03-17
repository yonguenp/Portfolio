using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class UIGachaIconObject : UIObject, EventListener<UIObjectEvent>
    {

        const eReddotEvent reddotType = eReddotEvent.GACHA;
        public override void InitUI(eUIType targetType)
        {
            base.InitUI(targetType);

            CheckButtonStates();
        }

        public override bool RefreshUI(eUIType targetType) //타입 갱신부는 아래에서 상속으로 구현
        {
            if (base.RefreshUI(targetType))
            {
                CheckButtonStates();
                return true;
            }
            return false;
        }

        public static void CheckButtonStates()
        {
            ReddotManager.Set(reddotType, IsGachaAvailableCondition());
        }

        static bool IsGachaAvailableCondition()
        {
            var gachaGroupDataList = GachaGroupData.GetAll();
            foreach (var d in gachaGroupDataList)
            {
                if (d.IsValid())
                {
                    foreach (var m in d.GetGachaMenus())
                    {
                        foreach (var type in m.typeDatas)
                        {
                            switch (type.price_type)
                            {
                                case eGoodType.ITEM:
                                    if (User.Instance.GetItemCount(type.price_uid) >= type.price_value)
                                        return true;
                                    break;
                                case eGoodType.ADVERTISEMENT:
                                    if (ShopManager.Instance.GetAdvertiseState(type.price_uid).IS_VALIDE)
                                        return true;
                                    break;
                                default://다이아 골드등 재화 소모형은 빨콩 제외?
                                    break;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public void OnEvent(UIObjectEvent eventType)
        {
            switch (eventType.e)
            {
                case UIObjectEvent.eEvent.ITEM_GET:
                    CheckButtonStates();
                    break;
            }
        }
    }
}
