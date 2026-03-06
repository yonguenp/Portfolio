using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class UIDiaObject : UIObject
    {
        [SerializeField]
        private Text diaAmountLabel = null;

        [SerializeField]
        private Animator getTextAnim = null;
        [SerializeField]
        private Text diaAddLabel = null;
        public static int lastestDia = 0;
        public override void InitUI(eUIType targetType)
        {
            base.InitUI(targetType);
            StopAllCoroutines();
            lastestDia = User.Instance.GEMSTONE;
            getTextAnim.gameObject.SetActive(false);
            RefreshDiaCount();
        }
        public override bool RefreshUI(eUIType targetType)
        {
            if (base.RefreshUI(targetType))
            {
                RefreshDiaCount();
            }
            return curSceneType != targetType;
        }
        private void OnDisable()
        {
            StopAllCoroutines();
            lastestDia = User.Instance.GEMSTONE;
            getTextAnim.gameObject.SetActive(false);
        }
        private void OnEnable()
        {
            var dia = User.Instance.GEMSTONE;
            var distance = dia - lastestDia;
            if (distance != 0)
            {
                StartCoroutine(TextShow(distance));
            }
        }
        void RefreshDiaCount()
        {
            if (diaAmountLabel != null)
            {
                diaAmountLabel.text = SBFunc.CommaFromNumber(User.Instance.GEMSTONE);
            }
            if (gameObject.activeInHierarchy)
            {
                var dia = User.Instance.GEMSTONE;
                var distance = dia - lastestDia;
                if (distance != 0)
                {
                    StartCoroutine(TextShow(distance));
                    lastestDia = dia;
                }
            }
        }

        public void OnClickDiaButton()
        {
            if (TutorialManager.tutorialManagement.IsOtherContentsBlock())
                return;
            PopupManager.OpenPopup<ShopPopup>(new MainShopPopupData(10));
        }
        
        IEnumerator TextShow(int DiaDistance)
        {
            if (DiaDistance == 0)
            {
                getTextAnim.gameObject.SetActive(false);
                yield break;
            }
            getTextAnim.gameObject.SetActive(true);
            getTextAnim.Play("DiaGet", 0);
            diaAddLabel.text = DiaDistance.ToString("+#;-#");
            while (getTextAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }
            getTextAnim.gameObject.SetActive(false);
            yield return null;
        }

        public void OnClickDiaIcon()
        {
            PopupManager.OpenPopup<ToolTip>(new TooltipPopupData(new ToolTipData(StringData.GetStringByStrKey("item_base:name:10000005"), StringData.GetStringByStrKey("다이아안내팝업"), gameObject, true, true, eToolTipDataFlag.Default)));
        }
    }
}
