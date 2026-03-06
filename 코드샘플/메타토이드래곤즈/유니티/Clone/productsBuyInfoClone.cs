using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class productsBuyInfoClone : MonoBehaviour
    {
        [SerializeField] ItemFrame item = null;
        [SerializeField] Text itemText = null;
        [SerializeField] Text buildingName = null;
        [SerializeField] Text costText = null;
        [SerializeField] Button costButton = null;

        IntDelegate buyCallback = null;
        Asset itemData = null;
        int requireCost = 0;

        public string SERVER_PARAM//서버에 보낼 요청 필드값
        {
            get
            {
                return GetServerFieldParam();
            }
        }

        public void SetData(Asset _item, IntDelegate _buyCallback = null)
        {
            if(item == null)
            {
                Debug.LogError("item is null");
                return;
            }

            itemData = _item;
            buyCallback = _buyCallback;

            SetItemFrame();
            SetCostButton();
            SetItemDesc();
            SetVisibleButton(true);
        }

        public void SetVisibleButton(bool _isVisible)
        {
            if (costButton != null)
                costButton.gameObject.SetActive(_isVisible);
        }

        void SetItemFrame()
        {
            if (itemData == null || item == null)
                return;

            item.SetFrameItem(itemData);
        }

        void SetCostButton()
        {
            if (itemData == null)
                return;

            if (costText == null)
                return;

            if (itemData.BaseData == null)
                return;

            var designData = itemData.BaseData;
            var itemID = designData.KEY;//아이템 key
            var requireTotalAmount = itemData.Amount;//요구 재료 갯수
            var curAmount = User.Instance.GetItemCount(itemID);
            var remainCount = requireTotalAmount - curAmount;

            var onceCost = designData.BUY;
            requireCost = onceCost * remainCount;
            costText.text = SBFunc.CommaFromNumber(requireCost);

            costText.color = requireCost > User.Instance.GEMSTONE ? Color.red : Color.white;
        }

        void SetItemDesc()
        {
            if(buildingName != null)
                buildingName.text = GetBuildingNameByItem();

            if (itemText != null)
                itemText.text = StringData.GetStringByStrKey(itemData.GetName()); 
        }
        
        public void OnClickMoveButton()// 해당 재료 관련 건물 및 씬으로 이동
        {
            var buildingName = GetBuildingNameByItem();
            if(string.IsNullOrEmpty(buildingName))
            {
                Debug.LogError("wrong building");
                return;
            }

            DirectMoveBuildingByItemKey();

            //var buildingBodyStr = string.Format("[{0}] 해당 건물로 이동 할까요?", buildingName);
            //SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("일일퀘스트바로가기"), buildingBodyStr, StringData.GetStringByStrKey("확인"), StringData.GetStringByStrKey("취소"),
            //        () => {
            //            DirectMoveBuildingByItemKey();
            //        }, ()=> { },() =>{});
        }
        string GetBuildingNameByItem()
        {
            if (itemData == null)
                return "";

            var itemDesignData = itemData.BaseData;
            if (itemDesignData == null)
                return "";

            string buildingIndex = ProductData.GetBuildingGroupByProductItem(itemDesignData.KEY);
            var buildingData = BuildingBaseData.Get(buildingIndex);
            if (buildingData == null)
                return "";

            return StringData.GetStringByStrKey(buildingData._NAME); 
        }

        void DirectMoveBuildingByItemKey()
        {
            if (itemData == null)
                return;

            var itemDesignData = itemData.BaseData;
            if (itemDesignData == null)
                return;

            string buildingIndex = ProductData.GetBuildingGroupByProductItem(itemDesignData.KEY);
            if (buildingIndex == "" && itemDesignData.KIND == eItemKind.EXP)
                buildingIndex = SBFunc.BATTERY_TYPE_ITEM_KEY;

            SBFunc.MoveScene(() => {
                SBFunc.RequestBuildingPopup(buildingIndex);//건물 건설 팝업 요청
            });
        }

        /// <summary>
        /// 구매 버튼
        /// </summary>
        public void OnClickBuyButton()
        {
            if (User.Instance.GEMSTONE < requireCost)//다이아 부족
            {
                ToastManager.On(StringData.GetStringByStrKey("town_upgrade_text_06"));
                return;
            }

            SendRequestBuyitem();

            //SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("pass_buy"), StringData.GetStringByStrKey("town_upgrade_text_02"), StringData.GetStringByStrKey("확인"), StringData.GetStringByStrKey("취소"),
            //        () => 
            //        {
            //            //to do - 서버처리


            //            if (buyCallback != null)
            //                buyCallback();
            //        }, () => { }, () => { });
        }

        string GetServerFieldParam()
        {
            if (itemData == null)
                return "";

            if (costText == null)
                return "";

            if (itemData.BaseData == null)
                return "";

            var designData = itemData.BaseData;
            var itemID = designData.KEY;//아이템 key
            var requireTotalAmount = itemData.Amount;//요구 재료 갯수
            var curAmount = User.Instance.GetItemCount(itemID);
            var remainCount = requireTotalAmount - curAmount;
            if (remainCount <= 0)
                return "";

            return SBFunc.StrBuilder(itemID, ":", remainCount, ";");
        }

        void SendRequestBuyitem()
        {
            var itemParam = GetServerFieldParam();
            if(string.IsNullOrEmpty(itemParam))
            {
                Debug.LogError("need item count is zero");
                return;
            }

            Debug.Log("item Param Field : " + itemParam);
            var param = new WWWForm();
            param.AddField("param", itemParam);
            NetworkManager.Send("shop/item", param, (jsonObj) =>
            {
                var data = jsonObj;
                var isSuccess = (data["err"].Value<int>() == 0);
                var rs = (eApiResCode)data["rs"].Value<int>();

                switch (rs)
                {
                    case eApiResCode.OK:
                    {
                        if (isSuccess)//결과 확인 팝업 출력
                        {
                            if (IsMailSended(jsonObj))
                                ToastManager.On(StringData.GetStringByStrKey("보상아이템우편발송"));
                            else
                                ToastManager.On(StringData.GetStringByStrKey("구매성공"));

                            if (buyCallback != null)
                                buyCallback(itemData.ItemNo);
                        }
                    }
                    break;
                }
            });
        }
        bool IsMailSended(JObject _jsonData)
        {
            if (_jsonData != null && _jsonData.ContainsKey("push"))
            {
                if (SBFunc.IsJArray(_jsonData["push"]))
                {
                    JArray pushArray = (JArray)_jsonData["push"];
                    if (pushArray == null)
                        return false;

                    var arrayCount = pushArray.Count;
                    for (var i = 0; i < arrayCount; ++i)
                    {
                        JObject jObject = (JObject)pushArray[i];

                        if (!SBFunc.IsJTokenCheck(jObject["api"]))
                            continue;

                        switch (jObject["api"].Value<string>())
                        {
                            case "mail_sended":
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }


    }
}
