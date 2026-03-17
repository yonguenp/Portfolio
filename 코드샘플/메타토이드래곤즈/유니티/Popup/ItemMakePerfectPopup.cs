using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{

    /// <summary>
    /// 확률 100 퍼센트 짜리만 쓸수 있는 아이템 조합 결과 팝업 // 당장 필요한 건 이 기능 뿐이라 간소화 하여 만듬
    /// </summary>
    public class ItemMakePerfectPopup : Popup<ItemMakePopupData>
    {
        [SerializeField] Text titleText;
        [SerializeField] RectTransform targetLayoutTransform = null;
        [SerializeField] List<ItemMakeSlot> itemList = new List<ItemMakeSlot>();
        [SerializeField] Text blockAmountText = null;
        [SerializeField] Image iconImage = null;
        [Header("[Slider]")]
        [SerializeField] Slider inputBlockSlider = null;
        [Header("[Buttons]")]
        [SerializeField] Button insertBlockButton = null;
        
        [SerializeField] GameObject PercentNotice;
        [SerializeField] Text PercentNoticeText;

        [SerializeField] GameObject inputLayer = null;
        [SerializeField] GameObject resultLayer = null;

        RecipeBaseData baseData = null;

        List<RecipeMaterialData> materialData = null;
        int currentAmount = 0;
        int maxAmount = 0;
        bool isAvailInsertBlock = false;
        int recipeKey = 0;

        VoidDelegate RefreshCallBack = null;
        List<ItemMakeSlot> materialList = new List<ItemMakeSlot>();//실제 활성 슬롯
        public override void InitUI()
        {
            recipeKey = Data.reciepeID;
            titleText.text = Data.titleStr;
            if (recipeKey == 0)
            {
                ClosePopup();
                return;
            }
                
            baseData = RecipeBaseData.Get(recipeKey);
            if (baseData == null)
                return;
            materialData = RecipeMaterialData.GetDataByGroup(recipeKey);
            if (materialData == null)
                return;

            Clear();
            SetIconImage();
            SetMaterialList();
            CalcMaxBlock();

            if (maxAmount >= 1)//최대값이 1개 (1개 이상 생산 가능)면 기본 선택 1로 세팅
                currentAmount = 1;

            UpdatePopupState();

            if(baseData.RATE < 1000000)
            {
                PercentNotice.SetActive(true);
                var calc_percent = (baseData.RATE / (float)SBDefine.MILLION) * SBDefine.BASE_FLOAT;
                PercentNoticeText.text = string.Format("{0:0.##}%", calc_percent);
            }
            else
            {
                PercentNotice.SetActive(false);
            }

            if(Data.oneByone)
            {
                inputLayer.SetActive(false);
                resultLayer.SetActive(false);
            }
            else
            {
                inputLayer.SetActive(true);
                resultLayer.SetActive(true);
            }
        }

        void SetIconImage()
        {
            if (iconImage != null)
                iconImage.sprite = baseData.REWARD_ITEM_LIST[0].ICON;
        }

        public void SetRefreshCallBack(VoidDelegate callBack)
        {
            if(callBack != null)
                RefreshCallBack = callBack;
        }
        void SetMaterialList()
        {
            foreach (var slotUI in itemList)
            {
                if (slotUI == null)
                    continue;
                slotUI.gameObject.SetActive(false);
            }

            if (materialData.Count > itemList.Count)
            {
                Debug.LogError("레시피 데이터 오류");
                return;
            }

            for (int i = 0; i < materialData.Count; i++)
            {
                var baseData = materialData[i];
                if (baseData == null)
                    continue;

                var targetItem = baseData.REWARD;
                itemList[i].SetSlot(targetItem);
                itemList[i].gameObject.SetActive(true);
                materialList.Add(itemList[i]);
            }

            RefreshContentFitter(targetLayoutTransform);
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

        void CalcMaxBlock()//현재 재료를 가지고 얼마만큼 만들 수 있는지 - 각 재료 maxCount 의 최소값
        {
            currentAmount = 0;
            maxAmount = int.MaxValue;
            if (materialList == null || materialList.Count <= 0)
                maxAmount = 0;
            if (materialList != null && materialList.Count > 0)
            {
                foreach (var slot in materialList)
                {
                    var currentMaxCount = slot.GetMaxCount();
                    if (maxAmount > currentMaxCount)
                        maxAmount = currentMaxCount;
                }
            }
        }
        public void OnInputSliderValueChanged()
        {
            if (inputBlockSlider == null) return;

            currentAmount = (int)inputBlockSlider.value;

            UpdatePopupState();
        }
        public void OnClickPlusButton()
        {
            currentAmount++;

            currentAmount = currentAmount > maxAmount ? maxAmount : currentAmount;

            UpdatePopupState();
        }

        public void OnClickMinusButton()
        {
            currentAmount--;

            currentAmount = currentAmount < 0 ? 0 : currentAmount;

            UpdatePopupState();
        }

        public void OnClickMaxButton()
        {
            currentAmount = maxAmount;

            UpdatePopupState();
        }
        void UpdatePopupState()
        {
            //재료 수치 
            SetSlotCount(currentAmount);

            blockAmountText.text = StringData.GetStringFormatByStrKey("production_quantity", currentAmount);

            // 인풋 슬라이더
            inputBlockSlider.maxValue = maxAmount;
            inputBlockSlider.value = currentAmount;
            // 버튼 상태 갱신
            isAvailInsertBlock = currentAmount > 0;
            insertBlockButton.SetButtonSpriteState(isAvailInsertBlock);
        }

        void SetSlotCount(int _requireCount)
        {
            if (materialList != null && materialList.Count > 0)
            {
                foreach (var slot in materialList)
                {
                    if (slot == null)
                        continue;

                    slot.SetCustomCount(_requireCount);
                }
            }
        }

        void Clear()
        {
            currentAmount = 0;
            maxAmount = 0;

            inputBlockSlider.value = 0;

            isAvailInsertBlock = false;

            if (materialList == null)
                materialList = new List<ItemMakeSlot>();

            materialList.Clear();
        }

        public void OnClickProductButton()
        {
            if (!isAvailInsertBlock)
            {
                if(Data.oneByone)
                    ToastManager.On(StringData.GetStringByStrKey("필요아이템확인"));
                else
                    ToastManager.On(StringData.GetStringByStrKey("최소수량입력"));
                return;
            }
            var requestCount = currentAmount;
            if (IsFullInventoryCheck(requestCount))
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                    () => {
                        PopupManager.AllClosePopup();
                        PopupManager.OpenPopup<InventoryPopup>();
                    }
                );
                return;
            }
            else
            {
                string url = "item/product";
                // 서버 타고 팝업 리프레쉬
                WWWForm data = new WWWForm();                
                switch (recipeKey)
                {                    
                    case 7:
                    case 8:
                        url = "foundry/open";
                        data.AddField("recipe_id", recipeKey);
                        break;
                    default:
                        url = "item/product";
                        data.AddField("recipe_no", recipeKey);
                        data.AddField("recipe_count", currentAmount);
                        break;
                }

                NetworkManager.Send(url, data, (jsonData) =>   // 성공 보상은 jsonData["rewards"]["success"] 로, 실패 보상은 jsonData["rewards"]["fail"]로 옴
                {
                    if (jsonData.ContainsKey("err"))
                    {
                        var errorFlag = jsonData["err"].Value<int>();
                        if (errorFlag != 0)
                        {
                            InitUI();
                            return;
                        }
                    }
                    if (jsonData.ContainsKey("rs"))
                    {
                        JToken resultResponse = jsonData["rs"];
                        if (resultResponse != null && resultResponse.Type == JTokenType.Integer)
                        {
                            int rs = resultResponse.Value<int>();
                            if ((eApiResCode)rs == eApiResCode.OK)
                            {
                                switch (recipeKey)
                                {
                                    case 7:
                                    {
                                        var data = LandmarkGemDungeon.Get();
                                        if (data != null)
                                        {
                                            if (data.NanochipsetSmithOpened)
                                            {
                                                ToastManager.On(StringData.GetStringByStrKey("건설완료"));
                                                ClosePopup();
                                            }
                                        }
                                    }break;
                                    case 8:
                                    {
                                        var data = LandmarkGemDungeon.Get();
                                        if (data != null)
                                        {
                                            if (data.GoldGemSmithOpened)
                                            {
                                                ToastManager.On(StringData.GetStringByStrKey("건설완료"));
                                                ClosePopup();
                                            }
                                        }
                                    }break;
                                    default:
                                        SystemRewardPopup.OpenPopup(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonData["rewards"]["success"])));
                                        InitUI();
                                        break;
                                }
                                RefreshCallBack?.Invoke();
                            }
                                
                        }
                    }
                }, (jsonData) => {
                    InitUI();
                });
            }
        }

        bool IsFullInventoryCheck(int _requestCount)
        {
            var successReward = baseData.REWARD_ITEM_LIST[0];
            var successRewardData = new Asset(successReward.ItemNo, successReward.Amount * _requestCount);
            return User.Instance.CheckInventoryGetItem(new List<Asset>() { successRewardData });
        }

    }

}
