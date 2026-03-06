using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class UISweepTicketObject : UIObject, EventListener<ItemFrameEvent>
    {
        [SerializeField]
        private Text coinAmountLabel = null;

        [SerializeField]
        private Animator getTextAnim = null;
        [SerializeField]
        private Text coinAddLabel = null;
        public int lastestCoin = 0;

        bool IsSweepEnable { get { return GameConfigTable.GetConfigIntValue("sweep_active", 1) > 0; } }
        public override void InitUI(eUIType targetType)
        {
            if (!IsSweepEnable)
            {
                this.gameObject.SetActive(false);
                return;
            }

            base.InitUI(targetType);
            StopAllCoroutines();
            lastestCoin = User.Instance.Inventory.GetItem(20000013).Amount;
            getTextAnim.gameObject.SetActive(false);
            RefreshCoinCount();
        }
        public override bool RefreshUI(eUIType targetType)
        {
            if (!IsSweepEnable)
            {
                this.gameObject.SetActive(false);
                return false;
            }

            if (base.RefreshUI(targetType))
            {
                RefreshCoinCount();
            }
            return curSceneType != targetType;
        }
        private void OnDisable()
        {
            EventManager.RemoveListener(this);

            StopAllCoroutines();
            lastestCoin = User.Instance.Inventory.GetItem(20000013).Amount;
            getTextAnim.gameObject.SetActive(false);
        }
        private void OnEnable()
        {
            EventManager.AddListener(this);

            var coin = User.Instance.Inventory.GetItem(20000013).Amount;
            var distance = coin - lastestCoin;
            if (distance != 0)
            {
                StartCoroutine(TextShow(distance));
            }
            RefreshCoinCount();
        }
        void RefreshCoinCount()
        {
            if (coinAmountLabel != null)
            {
                coinAmountLabel.text = SBFunc.CommaFromNumber(User.Instance.Inventory.GetItem(20000013).Amount);
            }
            if (gameObject.activeInHierarchy)
            {
                var coin = User.Instance.Inventory.GetItem(20000013).Amount;
                var distance = coin - lastestCoin;
                if (distance != 0)
                {
                    StartCoroutine(TextShow(distance));
                    lastestCoin = coin;
                }
            }
        }

        public void OnClickCoinButton()
        {
            var shop = PopupManager.GetPopup<ShopPopup>();
            if (PopupManager.IsPopupOpening(shop))
            {
                return;
            }

            PopupManager.OpenPopup<ShopPopup>(new MainShopPopupData(14));
        }
        
        IEnumerator TextShow(int CoinDistance)
        {
            if (CoinDistance == 0)
            {
                getTextAnim.gameObject.SetActive(false);
                yield break;
            }
            getTextAnim.gameObject.SetActive(true);
            getTextAnim.Play("DiaGet", 0);
            coinAddLabel.text = CoinDistance.ToString("+#;-#");
            while (getTextAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }
            getTextAnim.gameObject.SetActive(false);
            yield return null;
        }

        public void OnClickCoinIcon()
        {
            var shop = PopupManager.GetPopup<ShopPopup>();
            if (PopupManager.IsPopupOpening(shop))
            {
                return;
            }

            PopupManager.OpenPopup<ShopPopup>(new MainShopPopupData(14));
        }

        public void OnEvent(ItemFrameEvent eventType)
        {
            switch (eventType.Event)
            {
                case ItemFrameEvent.ItemFrameEventEnum.ITEM_UPDATE:
                    RefreshCoinCount();
                    break;
            }
        }
    }
}
