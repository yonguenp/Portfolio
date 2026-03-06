using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{

    /// <summary>
    /// 우정포인트 (친구 팝업 & 친구 상점에서만 뜨는 용도)
    /// </summary>
    public class UIFriendPointObject : UIObject
    {
        [SerializeField]
        private Text AmountLabel = null;

        public override void InitUI(eUIType targetType)
        {
            //base.InitUI(targetType);
            RefreshCount();
        }
        public override bool RefreshUI(eUIType targetType)
        {
            if (base.RefreshUI(targetType))
            {
                RefreshCount();
            }
            return curSceneType != targetType;
        }

        public void RefreshCount()
        {
            if (AmountLabel != null)
            {
                var friendPointAmount = User.Instance.UserData.Friendly_Point;
                if (friendPointAmount <= 0)
                {
                    AmountLabel.text = "0";
                    return;
                }

                AmountLabel.text = SBFunc.CommaFromNumber(friendPointAmount);
            }
        }
        public override void ShowEvent()
        {
            base.ShowEvent();
        }
        public void OnClickFriendPointButton()//우정 상점 이동
        {
            PopupManager.OpenPopup<FriendPointShopPopup>();
        }

        public override void RefreshUI()
        {
            base.RefreshUI();

            RefreshCount();
        }
    }
}