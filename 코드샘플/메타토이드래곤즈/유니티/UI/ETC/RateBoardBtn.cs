using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class RateBoardBtn : MonoBehaviour
    {
        [SerializeField]
        eRateBoardType urlKind = eRateBoardType.None;
        int param = -1;

        VoidDelegate clickCB = null;
        public void SetRateType(int pm = -1)
        {
            param = pm;
        }

        public void SetClickCallBack(VoidDelegate clickCallBack)
        {
            if(clickCallBack != null)
            {
                clickCB = clickCallBack;
            }
        }
        public void OnClickBtn()
        {
            clickCB?.Invoke();


            if (param > 0)
            {
                var types = GachaTypeData.GetByPriceItem(param);
                List<int> menus = new List<int>();
                if (types.Count > 0)
                {
                    foreach (var t in types)
                    {
                        menus.Add(t.menu_id);
                    }

                    menus = menus.Distinct().ToList();

                    if (menus.Count == 1)
                    {
                        GachaTablePopup.OpenPopup(menus[0]);
                        return;
                    }
                }
            }



            string url = SBDefine.GetRateBoardTypeToURL(urlKind);
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("확률표 url 누락");
                return;
            }
            PopupManager.OpenPopup<WebViewPopup>(new WebViewPopupData(url));
        }
    }
}

