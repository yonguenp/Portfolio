using DG.Tweening;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{

    public class ItemMakePopup : Popup<ItemMakePopupData>
    {
        [Header("item setting layer")]
        [SerializeField] GameObject itemSettingLayer;
        [SerializeField] Text titleText;
        [SerializeField] RectTransform targetLayoutTransform = null;
        [SerializeField] List<ItemMakeSlot> itemList = new List<ItemMakeSlot>();
        [SerializeField] Text blockAmountText = null;
        [SerializeField] Image iconImage = null;
        [SerializeField] Text PercentageText = null;
        [SerializeField] Text itemQuestionText = null;
        [Header("[Slider]")]
        [SerializeField] Slider inputBlockSlider = null;
        [Header("[Buttons]")]
        [SerializeField] Button insertBlockButton = null;
        [Space(10)]
        [Header("tween layer")]
        [SerializeField] GameObject tweenLayer;
        [SerializeField] Slider tweenSlider = null;
        [SerializeField] GameObject tweenObj = null;
        [Space(10)]
        [Header("result layer")]
        [SerializeField] GameObject resultLayer;
        [Header("success")]
        [SerializeField] GameObject successNode = null;
        [SerializeField] ItemFrame successItem = null;
        [SerializeField] SkeletonGraphic successSpine = null;

        [Space(10)]
        [Header("fail")]
        [SerializeField] GameObject failNode = null;
        [SerializeField] ItemFrame failItem = null;
        [SerializeField] SkeletonGraphic failSpine = null;

        RecipeBaseData baseData = null;

        List<RecipeMaterialData> materialData = null;
        int currentAmount = 0;
        int maxAmount = 0;
        bool isAvailInsertBlock = false;
        int recipeKey = 0;
        VoidDelegate RefreshCallBack = null;
        List<ItemMakeSlot> materialList = new List<ItemMakeSlot>();//실제 활성 슬롯


        Sequence coolTimeSeq = null;
        const float constructCoolTime = 3f;
        public override void InitUI()
        {
            itemSettingLayer.SetActive(true);
            tweenLayer.SetActive(false);
            resultLayer.SetActive(false);
            recipeKey = Data.reciepeID;
            if(Data.titleStr != string.Empty)
                titleText.text = Data.titleStr;
            else
                titleText.text = StringData.GetStringByStrKey("제작하기");
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
            if (PercentageText != null)
            {
                var calc_percent = (baseData.RATE /(float) SBDefine.MILLION) * SBDefine.BASE_FLOAT;
                PercentageText.text = string.Format("{0:0.##}%",calc_percent);
            }

            var rewardItem = baseData.REWARD_ITEM_LIST[0];
            var rewardItemName = rewardItem.BaseData.NAME;
            var colorStrReward = SBFunc.StrBuilder("<color=#03B027>", rewardItemName, "</color>");

            if (baseData.FAIL_ITEM_LIST.Count > 0)
            {                
                var failItemName = baseData.FAIL_ITEM_LIST[0].BaseData.NAME;
                var colorStrFail = SBFunc.StrBuilder("<color=#D41F2C>", failItemName, "</color>");
                itemQuestionText.text = StringData.GetStringFormatByStrKey("블록제작가이드", colorStrReward, colorStrFail);
            }
            else
            {
                itemQuestionText.text = StringData.GetStringFormatByStrKey("블록제작가이드2", colorStrReward);
            }
            Clear();
            SetIconImage();
            SetMaterialList();
            CalcMaxBlock();

            if (maxAmount >= 1)//최대값이 1개 (1개 이상 생산 가능)면 기본 선택 1로 세팅
                currentAmount = 1;

            UpdatePopupState();
        }
        private void OnDisable()
        {
            if (coolTimeSeq != null)
                coolTimeSeq.Kill();

            coolTimeSeq = null;
        }
        void CompleteEffect()
        {
            WWWForm data = new WWWForm();

            string url = "item/product";
            switch (recipeKey)
            {
                case 5:
                case 6:
                    url = "foundry/cast";
                    data.AddField("recipe_id", recipeKey);
                    data.AddField("item_amount", currentAmount);
                    break;
                default:
                    url = "item/product";
                    data.AddField("recipe_no", recipeKey);
                    data.AddField("recipe_count", currentAmount);
                    break;
            }

            NetworkManager.Send(url, data, (jsonData) =>   // 성공 보상은 jsonData["rewards"]["success"] 로, 실패 보상은 jsonData["rewards"]["fail"]로 옴
            {
                tweenLayer.SetActive(false);
                itemSettingLayer.SetActive(false);
                resultLayer.SetActive(true);
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
                            JArray successArr = null;
                            JArray faileArr = null;
                            switch (recipeKey)
                            {
                                case 5:
                                case 6:
                                    successArr = JArray.FromObject(jsonData["result"]["success"]);
                                    faileArr = JArray.FromObject(jsonData["result"]["fail"]);
                                    break;
                                default:
                                    successArr = JArray.FromObject(jsonData["rewards"]["success"]);
                                    faileArr = JArray.FromObject(jsonData["rewards"]["fail"]);
                                    break;
                            }


                            SetItem(true, successArr);
                            SetItem(false, faileArr);

                            RefreshCallBack?.Invoke();

                            if ((successArr == null || successArr.Count == 0) && (faileArr == null || faileArr.Count == 0))
                            {
                                ToastManager.On(StringData.GetStringByStrKey("제작결과없음"));
                                OnClickComplete();
                            }
                        }
                    }
                }
            }, (jsonData) => {
                InitUI();
            });
        }

        #region 재료 세팅
        void SetIconImage()
        {
            if (iconImage != null)
                iconImage.sprite = baseData.REWARD_ITEM_LIST[0].ICON;
        }
        public void SetRefreshCallBack(VoidDelegate callBack)
        {
            if (callBack != null)
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
            {  // 서버 타고 팝업 리프레쉬
                itemSettingLayer.SetActive(false);
                tweenLayer.SetActive(true);
                resultLayer.SetActive(false);
                PlaySequence();
            }
        }
        bool IsAvailableAverageCase(int _requestCount, Asset _successReward, Asset _failReward)
        {
            bool isOdd = (_requestCount % 2) >= 0;

            int checkSumFirst;
            int checkSumSecond;

            checkSumFirst = _requestCount / 2;
            if (isOdd)
                checkSumFirst += 1;

            checkSumSecond = _requestCount - checkSumFirst;

            var successRewardData = new Asset(_successReward.ItemNo, _successReward.Amount * checkSumFirst);
            Asset failRewardData = null;
            if(_failReward != null)
                failRewardData = new Asset(_failReward.ItemNo, _failReward.Amount * checkSumSecond);

            var checkList = new List<Asset>();
            if (checkSumFirst > 0)
                checkList.Add(successRewardData);
            if (failRewardData != null && checkSumSecond > 0)
                checkList.Add(failRewardData);

            return CheckInventoryGetItem(checkList);
        }
        bool IsExtremeCompareCase(int _requestCount, Asset _successReward, Asset _failReward)
        {
            var checkSumFirst = 1;
            var checkSumSecond = _requestCount - checkSumFirst;

            var successRewardData = new Asset(_successReward.ItemNo, _successReward.Amount * checkSumFirst);
            Asset failRewardData = null;
            if (_failReward != null)
                failRewardData = new Asset(_failReward.ItemNo, _failReward.Amount * checkSumSecond);

            var checkList = new List<Asset>();
            if (checkSumFirst > 0)
                checkList.Add(successRewardData);
            if (failRewardData != null && checkSumSecond > 0)
                checkList.Add(failRewardData);

            bool check = CheckInventoryGetItem(checkList);

            successRewardData = new Asset(_successReward.ItemNo, _successReward.Amount * checkSumSecond);
            if (_failReward != null)
                failRewardData = new Asset(_failReward.ItemNo, _failReward.Amount * checkSumFirst);

            checkList.Clear();
            if (checkSumFirst > 0)
                checkList.Add(successRewardData);
            if (failRewardData != null && checkSumSecond > 0)
                checkList.Add(failRewardData);

            bool checkReverse = CheckInventoryGetItem(checkList);

            return check || checkReverse;
        }
        bool IsFullInventoryCheck(int _requestCount)
        {
            var successReward = baseData.REWARD_ITEM_LIST[0];
            Asset failReward = null;
            if(baseData.FAIL_ITEM_LIST.Count > 0)
                failReward = baseData.FAIL_ITEM_LIST[0];

            bool averageCase = IsAvailableAverageCase(_requestCount, successReward, failReward);
            bool extremeCase = IsExtremeCompareCase(_requestCount, successReward, failReward);

            return averageCase || extremeCase;
        }
        bool CheckInventoryGetItem(List<Asset> _expectData)//true 면 인벤 공간 부족
        {
            return User.Instance.CheckInventoryGetItem(_expectData);
        }
        #endregion

        #region 연출 세팅
        void SetSlider(float value, float maxValue = 0)
        {
            if (tweenSlider != null)
            {
                if (maxValue > 0)
                    tweenSlider.maxValue = maxValue;

                tweenSlider.value = value;
            }
        }
        void PlaySequence()
        {
            if (coolTimeSeq != null)
                coolTimeSeq.Kill();

            if (tweenObj != null)
                tweenObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            SetSlider(0, constructCoolTime);

            tweenObj.GetComponent<RectTransform>().DOAnchorPosY(constructCoolTime, constructCoolTime).SetEase(Ease.Linear).OnUpdate(() => {
                SetSlider(tweenObj.GetComponent<RectTransform>().anchoredPosition.y);
            });

            coolTimeSeq = DOTween.Sequence();
            coolTimeSeq.AppendInterval(constructCoolTime).AppendCallback(() => {
                CompleteEffect();
            }).Play();
        }

        public void OnClickCancel()
        {
            coolTimeSeq?.Kill();
            itemSettingLayer.SetActive(true);
            tweenLayer.SetActive(false);
            resultLayer.SetActive(false);
        }
        #endregion


        #region 결과 세팅
        void SetItem(bool _isSuccess, JToken _data)
        {
            var hasValue = SBFunc.IsJTokenType(_data, JTokenType.Array);
            SetVisibleNode(_isSuccess, hasValue);

            if (!hasValue)
                return;

            var valueData = (JArray)_data[0];
            if (valueData.Count != 3)
                return;

            var itemNo = valueData[1].Value<int>();
            var itemCount = valueData[2].Value<int>();

            if (_isSuccess)
            {
                successItem.SetFrameItemInfo(itemNo, itemCount);
                successSpine.AnimationState.SetAnimation(0, "success", false);
            }
            else
            {
                failItem.SetFrameItemInfo(itemNo, itemCount);
                failSpine.AnimationState.SetAnimation(0, "failed", false);
            }
        }
        void SetVisibleNode(bool _isSuccess, bool _isVisible)
        {
            if (_isSuccess)
                successNode.SetActive(_isVisible);
            else
                failNode.SetActive(_isVisible);
        }
        public void OnClickComplete()
        {
            InitUI();
        }
        #endregion
    }

}
