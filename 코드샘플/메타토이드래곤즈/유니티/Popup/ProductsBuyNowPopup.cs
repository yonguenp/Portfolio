using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// WJ - 2023.11.29 (즉시 구매 팝업)
/// 생산 및 아이템 부족 시에 젬스톤(또는 기타 재화)으로 아이템을 구매하는 팝업
/// 즉시 구매 (최하단) 버튼을 누르면 이 팝업의 호출을 일으킨 팝업을 갱신하는 콜백 달아야함. - 외부 후처리 필요.
/// 이 팝업에서 구매를 원하고자 하는 항목이 단 한 개 남았을 때(종속 조건 : 즉시 구매 가격과 로우 하나의 가격이 같다면 구매아이템 정보 슬롯의 구매 버튼 꺼 달라고함.)
/// 세부 아이템 구매시 - dirtyCallback을 통하여 이 팝업을 호출한 UI의 갱신부 추가 (일부 구매하고 팝업 끌 가능성)
/// 추가 - 이 팝업에서 갱신 콜백으로 돌아갈 시 - 생산 요청 결과에 따라 추가적으로 생산 큐에 들어가야함.
/// 
/// </summary>
namespace SandboxNetwork
{
    public class ProductsBuyNowPopupData : PopupData
    {
        /// <summary>
        /// asset 의 Amount 는 요구 총 갯수로 세팅함 - 현재 인벤 수량은 여기서 계산 할 거라, 요구 총 갯수만 필요.
        /// </summary>
        public List<Asset> requireList = new();
        public VoidDelegate mainBuyButtonCallback = null;
        public IntDelegate cloneDirtyCallback = null;//세부 아이템만 구매하고 팝업을 끌 수도있다.(일단 param 열어둠)
        public bool isSameDirty = false;//콜백의 내용이 세부아이템 구매시와 같은지
        public ProductsBuyNowPopupData(List<Asset> _requireList, VoidDelegate _mainBuyButtonCallback = null, bool _isSameDirty = true, IntDelegate _cloneDirtyCallback = null)
        {
            requireList = _requireList;
            mainBuyButtonCallback = _mainBuyButtonCallback;
            isSameDirty = _isSameDirty;
            cloneDirtyCallback = _cloneDirtyCallback;
        }
    }
    public class ProductsBuyNowPopup : Popup<ProductsBuyNowPopupData>
    {
        [SerializeField] ScrollRect itemScroll = null;
        [SerializeField] GameObject itemSlotParent = null;
        [SerializeField] GameObject itemPrefab = null;//요구 재료 아이템 프리펩

        [SerializeField] GameObject itemInfoSlotParent = null;
        [SerializeField] GameObject itemInfoSlotClone = null;//생산 아이템 정보 슬롯 프리펩

        [SerializeField] Text totalCostText = null;

        List<Asset> requireList = new List<Asset>();
        int requireCost = 0;
        List<productsBuyInfoClone> uiSlotList = new List<productsBuyInfoClone>();

        #region OpenPopup
        public static ProductsBuyNowPopup OpenPopup(ProductsBuyNowPopupData data)
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<ProductsBuyNowPopup>(data);
        }
        public static ProductsBuyNowPopup OpenPopup(List<Asset> _requireList, VoidDelegate _mainBuyButtonCallback = null, bool _isSameDirty = true , IntDelegate _cloneDirtyCallback = null)
        {
            if (_requireList == null || _requireList.Count <= 0)
                return null;

            ProductsBuyNowPopupData data = new ProductsBuyNowPopupData(_requireList, _mainBuyButtonCallback, _isSameDirty, _cloneDirtyCallback);
            return PopupManager.OpenPopup<ProductsBuyNowPopup>(data);
        }
        #endregion
        public override void InitUI()
        {
            if (Data == null || Data.requireList == null || Data.requireList.Count <= 0)
                return;

            SetData();
            ClearDataSlot();
            RefreshUI();
        }

        void SetData()
        {
            requireList = Data.requireList.ToList();
        }

        void ClearDataSlot()
        {
            if (itemSlotParent != null)
                SBFunc.RemoveAllChildrens(itemSlotParent.transform);
            if (itemInfoSlotParent != null)
                SBFunc.RemoveAllChildrens(itemInfoSlotParent.transform);
            
            if (uiSlotList == null)
                uiSlotList = new List<productsBuyInfoClone>();
            
            uiSlotList.Clear();
        }
        /// <summary>
        /// 하위 슬롯에서 구매한 이후 상단의 데이터에서 구매한 아이템 ID를 가지고 데이터 삭제 (구매했으니, 들고 있을 필요없음)
        /// </summary>
        /// <param name="_itemID"></param>
        void RemoveTargetItem(int _itemNo)
        {
            if (requireList == null || requireList.Count <= 0)
                return;

            var findTarget = requireList.Find(element => element.ItemNo == _itemNo);
            if(findTarget != null)
                requireList.Remove(findTarget);
        }


        /// <summary>
        /// 슬롯 안쪽에서 구매할 경우 갱신 용도
        /// </summary>
        void RefreshUI()
        {
            ClearDataSlot();
            RefreshRequireItem();
            RefreshRequireItemInfoSlot();
            RefreshCloneButton();
            RefreshTotalCostButton();

            RefreshContentFitter(itemSlotParent.GetComponent<RectTransform>());
            RefreshContentFitter(itemInfoSlotParent.GetComponent<RectTransform>());
        }
        /// <summary>
        /// 버튼 금액 세팅
        /// </summary>
        void RefreshTotalCostButton()
        {
            requireCost = GetTotalCostByItem();

            totalCostText.color = requireCost > User.Instance.GEMSTONE ? Color.red : Color.white;

            if (totalCostText != null)
                totalCostText.text = SBFunc.CommaFromNumber(requireCost);
        }

        int GetTotalCostByItem()
        {
            int totalCount = 0;
            if (requireList == null || requireList.Count <= 0)
                return totalCount;

            foreach(var item in requireList)
            {
                if (item == null)
                    continue;

                var baseData = item.BaseData;
                if (baseData == null)
                    continue;

                var onceCost = baseData.BUY;
                var curCount = User.Instance.GetItemCount(baseData.KEY);
                var requireCount = item.Amount;
                var remainCount = requireCount - curCount;

                if (remainCount <= 0)//현재 충분한 양이있음 (오진 않겠지만)
                    continue;

                totalCount += (remainCount * onceCost);
            }

            return totalCount;
        }


        /// <summary>
        /// 아이템 슬롯 데이터 세팅
        /// </summary>
        void RefreshRequireItemInfoSlot()
        {
            if (requireList == null)
                return;

            var isMulti = requireList.Count > 1;
            foreach (var item in requireList)
            {
                if (item == null)
                    continue;

                var itemNo = item.ItemNo;
                var requireAmount = item.Amount;
                var currentCount = User.Instance.GetItemCount(itemNo);

                if (requireAmount <= currentCount)//충분하면 패스
                    continue;

                var clone = Instantiate(itemInfoSlotClone, itemInfoSlotParent.transform);
                var buyInfoComp = clone.GetComponent<productsBuyInfoClone>();
                if (buyInfoComp == null)
                {
                    Destroy(clone.gameObject);
                    continue;
                }

                buyInfoComp.SetData(item,(index) => {

                    RemoveTargetItem(index);//하위 아이템이 구매 발생 시, 해당 itemID를 가지고 info 삭제
                    RefreshUI();

                    if (Data.isSameDirty)
                    {
                        if(Data.mainBuyButtonCallback != null)
                            Data.mainBuyButtonCallback();
                    }
                    else
                    {
                        if (Data.cloneDirtyCallback != null)
                            Data.cloneDirtyCallback(index);
                    }
                });
                uiSlotList.Add(buyInfoComp);
            }
        }

        void RefreshCloneButton()
        {
            if (uiSlotList == null || uiSlotList.Count <= 0)
                return;

            var isSingleCount = uiSlotList.Count == 1;
            if (isSingleCount)
                uiSlotList[0].SetVisibleButton(false);
            else
            {
                foreach(var slotData in uiSlotList)
                {
                    if (slotData == null)
                        continue;
                    slotData.SetVisibleButton(true);
                }
            }
        }

        void RefreshRequireItem()
        {
            if (requireList == null)
                return;

            foreach (var item in requireList)
            {
                if (item == null)
                    continue;

                var itemNo = item.ItemNo;
                var requireAmount = item.Amount;
                var currentCount = User.Instance.GetItemCount(itemNo);

                if (requireAmount <= currentCount)//충분하면 패스
                    continue;

                var clone = Instantiate(itemPrefab, itemSlotParent.transform);
                var itemFrameComp = clone.GetComponent<ItemFrame>();
                if (itemFrameComp == null)
                {
                    Destroy(clone.gameObject);
                    continue;
                }

                itemFrameComp.setFrameRecipeInfo(itemNo, requireAmount);
            }

            if (itemScroll != null)
                itemScroll.horizontalNormalizedPosition = 0f;
        }

        private void RefreshContentFitter(RectTransform transform)
        {
            if (transform == null || !transform.gameObject.activeSelf)
            {
                return;
            }

            foreach (RectTransform child in transform)
            {
                RefreshContentFitter(child);
            }

            var layoutGroup = transform.GetComponent<LayoutGroup>();
            var contentSizeFitter = transform.GetComponent<ContentSizeFitter>();
            if (layoutGroup != null)
            {
                layoutGroup.SetLayoutHorizontal();
                layoutGroup.SetLayoutVertical();
            }

            if (contentSizeFitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
            }
        }

        public void OnClickTotalBuyButton()
        {
            if (User.Instance.GEMSTONE < requireCost)//다이아 부족
            {
                ToastManager.On(StringData.GetStringByStrKey("town_upgrade_text_06"));
                return;
            }

            SendRequestBuyitem();

            //SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("pass_buy"), StringData.GetStringByStrKey("town_upgrade_text_02"), StringData.GetStringByStrKey("확인"), StringData.GetStringByStrKey("취소"),
            //        () => {

            //            if (Data.mainBuyButtonCallback != null)
            //                Data.mainBuyButtonCallback();

            //            ClosePopup();
            //        }, () => { }, () => { });
        }

        void SendRequestBuyitem()
        {
            if (uiSlotList == null || uiSlotList.Count <= 0)
                return;

            string totalParam = "";

            foreach(var slot in uiSlotList)
            {
                if (slot == null)
                    continue;

                totalParam += slot.SERVER_PARAM;
            }
            
            if (string.IsNullOrEmpty(totalParam))
            {
                Debug.LogError("need item count is zero");
                return;
            }

            Debug.Log("item Param Field : " + totalParam);
            var param = new WWWForm();
            param.AddField("param", totalParam);
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
                                ToastManager.On(StringData.GetStringByStrKey("전체구매성공"));

                            if (Data.mainBuyButtonCallback != null)
                                Data.mainBuyButtonCallback();

                            ClosePopup();
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
