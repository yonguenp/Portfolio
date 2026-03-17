using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PricePopup : Popup<PricePopupData>
    {
        [Header("Top")]
        [SerializeField]
        private Text titleText = null;
        [SerializeField]
        private Button closeBtn = null;
        [Space(10f)]
        [Header("Middle")]
        [SerializeField]
        private GameObject subTitleLayer = null;
        [SerializeField]
        private Text subTitleText = null;
        [SerializeField]
        private GameObject contentBg = null;
        [SerializeField]
        private Text contentText = null;
        [Space(10f)]
        [Header("Bottom")]
        [SerializeField]
        private Button priceBtn = null;
        [SerializeField]
        private List<Image> priceImages = null;
        [SerializeField]
        private Text priceText = null;
        [SerializeField]
        private Button cancleBtn = null;

        bool isAvailable = false;

        #region OpenPopup
        public static PricePopup OpenPopup(int titleStr, int subTitleStr, int contentStr, int price, ePriceDataFlag flag, VoidDelegate btnDelegate = null)
        {
            return OpenPopup(new PricePopupData(titleStr, subTitleStr, contentStr, price, flag, btnDelegate));
        }
        public static PricePopup OpenPopup(string titleStr, string subTitleStr, string contentStr, int price, ePriceDataFlag flag, VoidDelegate btnDelegate = null)
        {
            return OpenPopup(new PricePopupData(titleStr, subTitleStr, contentStr, price, flag, btnDelegate));
        }
        public static PricePopup OpenPopup(PricePopupData data) 
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<PricePopup>(data);
        }
        #endregion
        //사용법
        //PricePopup.OpenPopup(titleStr, subTitleStr, contentStr, price, flag, () =>
        //{
        //});
        public override void InitUI()
        {
            if (Data == null)
                return;

            // 가능여부 체크 - 추후 2개가 동시에 요구되는 케이스가 있으면 수정필요
            if (Data.Flag.HasFlag(ePriceDataFlag.GemStone))
            {
                isAvailable = User.Instance.GEMSTONE >= Data.Price;
            }
            else if (Data.Flag.HasFlag(ePriceDataFlag.Gold))
            {
                isAvailable = User.Instance.GOLD >= Data.Price;
            }

            //flag 세팅
            if (closeBtn != null)
                closeBtn.gameObject.SetActive(Data.Flag.HasFlag(ePriceDataFlag.CloseBtn));
            if (subTitleLayer != null)
                subTitleLayer.SetActive(Data.Flag.HasFlag(ePriceDataFlag.SubTitleLayer));
            if (contentBg != null)
                contentBg.SetActive(Data.Flag.HasFlag(ePriceDataFlag.ContentBG));
            if (cancleBtn != null)
                cancleBtn.gameObject.SetActive(Data.Flag.HasFlag(ePriceDataFlag.CancelBtn));
            //

            if (titleText != null)
                titleText.text = Data.TitleStr;

            if (priceImages != null)
            {
                for(int i = 0, count = priceImages.Count; i < count; ++i)
                {
                    if (priceImages[i] == null)
                        continue;

                    priceImages[i].gameObject.SetActive(false);
                }

                if(Data.Flag.HasFlag(ePriceDataFlag.Gold) && priceImages.Count > 0 && priceImages[0] != null)
                {
                    priceImages[0].gameObject.SetActive(true);
                }
                else if (Data.Flag.HasFlag(ePriceDataFlag.GemStone) && priceImages.Count > 1 && priceImages[1] != null)
                {
                    priceImages[1].gameObject.SetActive(true);
                }
            }

            if (subTitleText != null)
                subTitleText.text = Data.SubTitleStr;

            if (contentText != null)
                contentText.text = Data.ContentStr;

            if (priceBtn != null)
            {
                priceBtn.SetButtonSpriteState(isAvailable);
            }
            if (priceText != null)
            {
                priceText.text = SBFunc.CommaFromNumber(Data.Price);
                priceText.color = isAvailable ? Color.white : Color.red;
            }
        }

        public void OnClickOk()
        {
            if (isAvailable)
            {
                Data?.BtnDelegate?.Invoke();
            }
            else
            {
                ToastManager.On(100002522);
            }
        }

        public override void ForceUpdate(PricePopupData data)
        {
            base.DataRefresh(data);

            InitUI();
        }

        public void OnClickClose()
        {
            ClosePopup();
        }
    }
}