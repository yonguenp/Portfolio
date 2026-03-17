using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class UIGoldObject : UIObject
    {
        [SerializeField]
        private Text goldAmountLabel = null;

        [SerializeField]
        private Animator getTextAnim = null;
        [SerializeField]
        private Text goldAddLabel = null;
        public static int lastestGold = 0;
        public override void InitUI(eUIType targetType)
        {
            base.InitUI(targetType);
            lastestGold = User.Instance.GOLD;
            getTextAnim.gameObject.SetActive(false);
            RefreshGoldCount();
        }
        public override bool RefreshUI(eUIType targetType)
        {
            getTextAnim.gameObject.SetActive(false);
            if (base.RefreshUI(targetType))
            {
                RefreshGoldCount();
            }
            return curSceneType != targetType;
        }
        private void OnDisable()
        {
            StopAllCoroutines();
            if (goldAmountLabel != null)
                goldAmountLabel.text = SBFunc.CommaFromNumber(User.Instance.GOLD);
            getTextAnim.gameObject.SetActive(false);
        }
        private void OnEnable()
        {
            var gold = User.Instance.GOLD;
            var distance = gold - lastestGold;
            if (distance != 0)
            {
                StartCoroutine(Count(lastestGold, gold));
                StartCoroutine(TextShow(distance));
                lastestGold = gold;
            }
        }
        public void RefreshGoldCount()
        {
            StopAllCoroutines();
            
            if (goldAmountLabel != null)
                goldAmountLabel.text = SBFunc.CommaFromNumber(User.Instance.GOLD);
            if (gameObject.activeInHierarchy) { 
                var gold = User.Instance.GOLD;
                var distance = gold - lastestGold;
                if (distance != 0)
                {
                    StartCoroutine(Count(lastestGold,gold));
                    StartCoroutine(TextShow(distance));
                    lastestGold = gold;
                }
            }
        }

        public void OnClickGoldButton()
        {
            if (TutorialManager.tutorialManagement.IsOtherContentsBlock())
                return;
            PopupManager.OpenPopup<ShopPopup>(new MainShopPopupData(10));

        }

        IEnumerator TextShow(int dist)
        {
            if(dist == 0)
            {
                getTextAnim.gameObject.SetActive(false);
                yield break;
            }
            getTextAnim.gameObject.SetActive(true);
            getTextAnim.Play("DiaGet", 0);
            goldAddLabel.text = dist.ToString("+#;-#");
            while(getTextAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }
            getTextAnim.gameObject.SetActive(false);
        }
        
        //WJ - 기존 float, float param을 int로 바꿈(부동소수점으로 형식 바뀌는 연산을 돌면 - (ex) 1억) 고장나서 단순 틱값을 위한 float 연산 같아서 int로 변경)
        IEnumerator Count(int beforeMoney, int afterMoney)
        {
            if (goldAmountLabel == null)
                yield break;
            var bMoney = beforeMoney;
            var lMoney = afterMoney;

            float duration = 0.5f; // 카운팅에 걸리는 시간 설정. 
            float offset = (lMoney - bMoney) / duration;
            
            if (lMoney != bMoney)
            {
                if (bMoney > lMoney)
                {
                    while (bMoney > lMoney)
                    {
                        bMoney += (int)(offset * Time.deltaTime);
                        goldAmountLabel.text = SBFunc.CommaFromNumber(bMoney);
                        yield return null;
                    }
                }
                else
                {
                    while (bMoney < lMoney)
                    {
                        bMoney += (int)(offset * Time.deltaTime);
                        goldAmountLabel.text = SBFunc.CommaFromNumber(bMoney);
                        yield return null;

                    }
                }
            }
            goldAmountLabel.text = SBFunc.CommaFromNumber(lMoney);
            yield return null;
            
        }
    }
}
