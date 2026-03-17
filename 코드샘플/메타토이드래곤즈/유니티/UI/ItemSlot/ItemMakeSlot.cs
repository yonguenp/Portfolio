using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    /// <summary>
    /// 블록 제작에서 하위 재작 재료 관리 슬롯
    /// </summary>
    public class ItemMakeSlot : MonoBehaviour
    {
        [SerializeField] ItemFrame item = null;

        int maxCount = 0;//현재 생산 가능한 맥스카운트
        Asset itemData = null;

        [SerializeField] Color textColor;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_material"></param>(itemType, itemNo, amount) -> amount 는 해당 레시피의 최소 1개 제작 요구수량
        public void SetSlot(Asset _material)
        {
            if (_material == null)
                return;

            itemData = _material;
            SetCustomCount(0);
            CalcMaxCount();
        }

        public void SetCustomCount(int _requireCount)
        {
            if (_requireCount <= 0)
                _requireCount = 1;

            var itemNo = itemData.ItemNo;
            var itemType = itemData.GoodType;
            var amount = itemData.Amount * _requireCount;//레시피 제작 1개 요구 수량 * 요청 수량

            if (itemType == eGoodType.GOLD || itemType == eGoodType.GEMSTONE || itemType == eGoodType.MAGNET)
            {
                item.setFrameCashInfo((int)itemType, amount, true, false, true);
                int userCash = 0;
                switch(itemType)
                {
                    case eGoodType.GOLD:
                        userCash = User.Instance.GOLD;
                        break;
                    case eGoodType.GEMSTONE:
                        userCash = User.Instance.GEMSTONE;
                        break;
                    case eGoodType.MAGNET:
                        userCash = User.Instance.UserData.Magnet;
                        break;
                }
                item.SetTextColor(userCash >= amount ? textColor : Color.red);
            }
            else
            {
                item.setFrameRecipeInfo(itemNo, amount, false);
                var itemMine = User.Instance.GetItem(itemNo);
                var userItem = 0;
                if (item != null)
                {
                    userItem = itemMine.Amount;
                }
                item.SetTextColor(userItem >= amount ? textColor : Color.red);
            }
        }

        void CalcMaxCount()
        {
            var itemNo = itemData.ItemNo;
            var itemType = itemData.GoodType;
            var amount = itemData.Amount;//레시피 제작 1개 요구 수량 * 요청 수량

            int userCount;
            if (itemType == eGoodType.GOLD)
                userCount = User.Instance.GOLD;
            else if (itemType == eGoodType.MAGNET)
                userCount = User.Instance.UserData.Magnet;
            else if (itemType == eGoodType.MAGNITE)
                userCount = User.Instance.UserData.Magnite;
            else if (itemType == eGoodType.GEMSTONE)
                userCount = User.Instance.GEMSTONE;
            else
                userCount = User.Instance.GetItemCount(itemNo);

            if (amount <= 0)
                amount = 1;

            maxCount = userCount / amount;
        }

        public int GetMaxCount()//현재 재화로 생산 가능 최대갯수
        {
            return maxCount;
        }
    }
}
