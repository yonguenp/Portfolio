using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



namespace SandboxNetwork
{
    public class AdvRemoveBuyGuidePopup : Popup<PopupData>
    {
        const int ADVRemoveGoodsID = 20003;
        public delegate void Callback();

        Callback showCallback = null;
        Callback closeCallback = null;
        public override void InitUI()
        {
           
        }

        public void SetCallBack(Callback cb, Callback failCallBack)
        { 
            showCallback = cb;
            closeCallback = failCallBack;
        }

        public void OnClickGuideAdvRemove()
        {
            ShopGoodsData data = ShopGoodsData.Get(ADVRemoveGoodsID);  // 광고 제거 상품인지 클라이언트에서 구분할 수단이 없음. 키값이 바뀌면 이것도 바꾸어야 함
            PopupManager.OpenPopup<ShopBannerPopup>(new ShopBuyPopupData(data));
            ClosePopup();
        }

        public override void ClosePopup()
        {
            base.ClosePopup();
            closeCallback?.Invoke();
        }

        public void ShowAdv()
        {
            SetExitCallback(()=> showCallback?.Invoke());
            ClosePopup();
        }
    }
}

