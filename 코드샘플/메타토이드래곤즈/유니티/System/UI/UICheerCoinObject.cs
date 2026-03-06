using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class UICheerCoinObject : UIObject, EventListener<UserStatusEvent>
    {
        [SerializeField]
        private Text coinAmountLabel = null;

        [SerializeField]
        private Animator getTextAnim = null;
        [SerializeField]
        private Text coinAddLabel = null;
        public int lastestCoin = 0;

        public override void InitUI(eUIType targetType)
        {
            if (!User.Instance.ENABLE_P2E)
            {
                this.gameObject.SetActive(false);
                return;
            }
            
            base.InitUI(targetType);
            StopAllCoroutines();
            lastestCoin = User.Instance.ORACLE;
            getTextAnim.gameObject.SetActive(false);
            RefreshCoinCount();
        }
        public override bool RefreshUI(eUIType targetType)
        {
            if (!User.Instance.ENABLE_P2E)
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
            lastestCoin = User.Instance.ORACLE;
            getTextAnim.gameObject.SetActive(false);
        }
        private void OnEnable()
        {
            EventManager.AddListener(this);

            if (!User.Instance.ENABLE_P2E)
            {
                this.gameObject.SetActive(false);
                return;
            }

            var coin = User.Instance.ORACLE;
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
                coinAmountLabel.text = SBFunc.CommaFromNumber(User.Instance.ORACLE);
            }
            if (gameObject.activeInHierarchy)
            {
                var coin = User.Instance.ORACLE;
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
            if (!User.Instance.ENABLE_P2E)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("응원봉상점이용불가안내"));
            }

            DAppManager.Instance.OpenDAppChampionPage();
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
            if (!User.Instance.ENABLE_P2E)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("안내"), StringData.GetStringByStrKey("응원봉상점이용불가안내"));
            }

            DAppManager.Instance.OpenDAppChampionPage();
        }

        public void OnEvent(UserStatusEvent eventType)
        {
            switch (eventType.Event)
            {
                case UserStatusEvent.eUserStatusEventEnum.ORACLE:
                    RefreshCoinCount();
                    break;
            }
        }
    }
}
