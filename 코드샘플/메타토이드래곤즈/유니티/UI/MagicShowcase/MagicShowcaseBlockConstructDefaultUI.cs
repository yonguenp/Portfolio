using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 블록 제작 팝업 기본 상태
    /// </summary>
    public class MagicShowcaseBlockConstructDefaultUI : MonoBehaviour
    {
        [SerializeField] RectTransform targetLayoutTransform = null;
        [SerializeField] List<MagicShowcaseBlockConstructItemSlot> itemList = new List<MagicShowcaseBlockConstructItemSlot>();
        [SerializeField] Text blockAmountText = null;
        [SerializeField] Image iconImage = null;
        [Header("[Slider]")]
        [SerializeField] Slider inputBlockSlider = null;
        [Header("[Buttons]")]
        [SerializeField] Button insertBlockButton = null;

        RecipeBaseData baseData = null;
        List<RecipeMaterialData> materialData = null;
        int currentBlock = 0;
        int maxBlock = 0;
        bool isAvailInsertBlock = false;

        List<MagicShowcaseBlockConstructItemSlot> materialList = new List<MagicShowcaseBlockConstructItemSlot>();//실제 활성 슬롯

        public void SetVisible(bool _isVisible)
        {
            gameObject.SetActive(_isVisible);
        }

        public void InitUI(RecipeBaseData _recipeData)
        {
            if (_recipeData != null)
                baseData = _recipeData;

            materialData = RecipeMaterialData.GetDataByGroup(int.Parse(baseData.GetKey()));

            Clear();
            SetIconImage();
            SetMaterialList();
            CalcMaxBlock();

            if (maxBlock >= 1)//최대값이 1개 (1개 이상 생산 가능)면 기본 선택 1로 세팅
                currentBlock = 1;

            UpdatePopupState();
        }
        void SetIconImage()
        {
            if (iconImage != null)
                iconImage.sprite = baseData.REWARD_ITEM_LIST[0].ICON;
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
            currentBlock = 0;
            maxBlock = int.MaxValue;
            if (materialList == null || materialList.Count <= 0)
                maxBlock = 0;
            if(materialList != null && materialList.Count > 0)
            {
                foreach(var slot in materialList)
                {
                    var currentMaxCount = slot.GetMaxCount();
                    if (maxBlock > currentMaxCount)
                        maxBlock = currentMaxCount;
                }
            }
        }
        public void OnInputSliderValueChanged()
        {
            if (inputBlockSlider == null) return;

            currentBlock = (int)inputBlockSlider.value;

            UpdatePopupState();
        }
        public void OnClickPlusButton()
        {
            currentBlock++;

            currentBlock = currentBlock > maxBlock ? maxBlock : currentBlock;

            UpdatePopupState();
        }

        public void OnClickMinusButton()
        {
            currentBlock--;

            currentBlock = currentBlock < 0 ? 0 : currentBlock;

            UpdatePopupState();
        }

        public void OnClickMaxButton()
        {
            currentBlock = maxBlock;

            UpdatePopupState();
        }
        void UpdatePopupState()
        {
            blockAmountText.text = StringData.GetStringFormatByStrKey("production_quantity", currentBlock);

            // 인풋 슬라이더
            inputBlockSlider.maxValue = maxBlock;
            inputBlockSlider.value = currentBlock;

            //재료 수치 
            SetSlotCount(currentBlock);

            // 버튼 상태 갱신
            isAvailInsertBlock = currentBlock > 0;
            insertBlockButton.SetButtonSpriteState(isAvailInsertBlock);
        }

        void SetSlotCount(int _requireCount)
        {
            if(materialList != null && materialList.Count > 0)
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
            currentBlock = 0;
            maxBlock = 0;

            inputBlockSlider.value = 0;

            isAvailInsertBlock = false;

            if (materialList == null)
                materialList = new List<MagicShowcaseBlockConstructItemSlot>();

            materialList.Clear();
        }

        public void OnClickProductButton()
        {
            if(!isAvailInsertBlock)
            {
                ToastManager.On(StringData.GetStringByStrKey("최소수량입력"));
                return;
            }
            /*
             * 예상 결과에 대한 인벤 체크 처리
             * 예시 3케이스에 대한 하나라도 삑나면 실패로 간주.
             * 실패 1개      // 성공 999999개
             * 실패 999999개 // 성공 1개
             * 실패 500000개 // 성공 500000개 
             */
            var requestCount = currentBlock;
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
                MagicShowcaseBlockConstructEvent.StartingProducting(int.Parse(baseData.GetKey()), currentBlock);
        }

        bool IsFullInventoryCheck(int _requestCount)
        {
            var successReward = baseData.REWARD_ITEM_LIST[0];
            var failReward = baseData.FAIL_ITEM_LIST[0];

            bool averageCase = IsAvailableAverageCase(_requestCount, successReward, failReward);
            bool extremeCase = IsExtremeCompareCase(_requestCount, successReward, failReward);

            return averageCase || extremeCase;
        }

        bool IsAvailableAverageCase(int _requestCount , Asset _successReward , Asset _failReward)
        {
            bool isOdd = (_requestCount % 2) >= 0;

            int checkSumFirst;
            int checkSumSecond;

            checkSumFirst = _requestCount / 2;
            if (isOdd)
                checkSumFirst += 1;

            checkSumSecond = _requestCount - checkSumFirst;

            var successRewardData = new Asset(_successReward.ItemNo, _successReward.Amount * checkSumFirst);
            var failRewardData = new Asset(_failReward.ItemNo, _failReward.Amount * checkSumSecond);

            var checkList = new List<Asset>();
            if (checkSumFirst > 0)
                checkList.Add(successRewardData);
            if (checkSumSecond > 0)
                checkList.Add(failRewardData);

            return CheckInventoryGetItem(checkList);
        }

        bool IsExtremeCompareCase(int _requestCount, Asset _successReward, Asset _failReward)
        {
            var checkSumFirst = 1;
            var checkSumSecond = _requestCount - checkSumFirst;

            var successRewardData = new Asset(_successReward.ItemNo, _successReward.Amount * checkSumFirst);
            var failRewardData = new Asset(_failReward.ItemNo, _failReward.Amount * checkSumSecond);

            var checkList = new List<Asset>();
            if (checkSumFirst > 0)
                checkList.Add(successRewardData);
            if (checkSumSecond > 0)
                checkList.Add(failRewardData);

            bool check = CheckInventoryGetItem(checkList);

            successRewardData = new Asset(_successReward.ItemNo, _successReward.Amount * checkSumSecond);
            failRewardData = new Asset(_failReward.ItemNo, _failReward.Amount * checkSumFirst);

            checkList.Clear();
            if (checkSumFirst > 0)
                checkList.Add(successRewardData);
            if (checkSumSecond > 0)
                checkList.Add(failRewardData);

            bool checkReverse = CheckInventoryGetItem(checkList);

            return check || checkReverse;
        }

        bool CheckInventoryGetItem(List<Asset> _expectData)//true 면 인벤 공간 부족
        {
            return User.Instance.CheckInventoryGetItem(_expectData);
        }
    }
}

