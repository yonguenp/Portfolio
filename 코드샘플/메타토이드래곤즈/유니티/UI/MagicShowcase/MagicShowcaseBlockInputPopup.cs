using DG.Tweening;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 블록 투입 팝업 UI
    /// </summary>
    /// 필요한 팝업 데이터 -> 렙업에 필요한 타겟 블록 Asset형태 , magic_showcase_info 의 해당 레벨의 tableData (row)
    public class MagicShowcaseBlockInputPopupData : PopupData
    {
        private MagicShowcaseData tableData = null;
        public MagicShowcaseData TableData { get { return tableData; } }

        private Asset targetItem = null;
        public Asset TargetItem { get { return targetItem; } }

        private Asset stockItem = null;
        public Asset StockItem { get { return stockItem; } }

        public Action Callback { get; private set; } = null;

        private int menuType = 0;
        public int MenuType { get { return menuType; } }

        public MagicShowcaseBlockInputPopupData(int _menuType, Asset _targetItem, Asset _stockItem, MagicShowcaseData _resultData, Action cb = null)
        {
            menuType = _menuType;//현재 요청 메뉴 타입
            targetItem = _targetItem;//필요 아이템(현재 보유량 포함)
            stockItem = _stockItem;//현재 이 아이템의 투입량
            tableData = _resultData;//테이블 데이터
            Callback = cb;//서버 요청하고 나서 완료 액션
        }
    }
    public class MagicShowcaseBlockInputPopup : Popup<MagicShowcaseBlockInputPopupData>
    {
        [SerializeField] Text blockAmountText = null;
        [SerializeField] Image iconImage = null;

        [Header("[Silders]")]
        [SerializeField] Slider currentUpgradeProgressBar = null;
        [SerializeField] Text currentUpgradeProgressText = null;
        [SerializeField] Color inputAmountTextColor = Color.white;

        [Space(10)]
        [SerializeField] Slider expectedUpgradeProgressBar = null;

        [Space(10)]
        [SerializeField] Slider inputBlockSlider = null;

        [Header("[Buttons]")]
        [SerializeField] Button insertBlockButton = null;

        int currentBlock = 0;
        int maxBlock = 0;

        bool isAvailInsertBlock = false;

        int currentMenuType = 0;
        Asset currentItem = null;
        Asset stockItem = null;
        MagicShowcaseData tableData = null;

        #region OpenPopup
        public static MagicShowcaseBlockInputPopup OpenPopup(int _menuType,int _itemID, int _userInvenCount, int _stockCount, MagicShowcaseData _resultData, Action cb = null)
        {
            var popupData = new MagicShowcaseBlockInputPopupData(_menuType, new Asset(_itemID,_userInvenCount), new Asset(_itemID,_stockCount), _resultData, cb);

            return OpenPopup(popupData);
        }
        public static MagicShowcaseBlockInputPopup OpenPopup(MagicShowcaseBlockInputPopupData data)
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<MagicShowcaseBlockInputPopup>(data);
        }
        #endregion

        public override void InitUI()
        {
            currentMenuType = Data.MenuType;//현재 메뉴 인덱스 (타입 상태)
            currentItem = Data.TargetItem;//현재 타겟 아이템
            stockItem = Data.StockItem;//투입 아이템 
            tableData = Data.TableData;

            if (IsDataNull())
                return;

            SetIconImage();

            Clear();

            CalcMaxBlock();

            UpdatePopupState();
        }

        void SetIconImage()
        {
            if (iconImage != null)
                iconImage.sprite = currentItem.ICON;
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

        public void OnClickInsertBlockButton()
        {
            if (currentBlock <= 0)
            {
                ToastManager.On(StringData.GetStringByStrKey("최소수량입력"));
                return;
            }

            WWWForm data = new WWWForm();
            data.AddField("menutype", currentMenuType);
            data.AddField("itemid", currentItem.ItemNo);
            data.AddField("itemamount", currentBlock);

            NetworkManager.Send("magicshowcase/blockinput", data, (jsonData) =>
            {
                JToken resultResponse = jsonData["rs"];
                if (resultResponse != null && resultResponse.Type == JTokenType.Integer)
                {
                    int rs = resultResponse.Value<int>();
                    switch ((eApiResCode)rs)
                    {
                        case eApiResCode.OK:
                            if (Data != null && Data.Callback != null)//현재레이어 갱신 요청
                                Data.Callback();
                            ClosePopup();
                            break;
                    }
                }
            });
        }

        void CalcMaxBlock()
        {
            currentBlock = 0;

            int remainUpgradeCount = GetTableMaxCount() - stockItem.Amount;
            int userMagnetCount = currentItem.Amount;

            maxBlock = userMagnetCount < remainUpgradeCount ? userMagnetCount : remainUpgradeCount;
        }

        void UpdatePopupState()
        {
            blockAmountText.text = StringData.GetStringFormatByStrKey("production_quantity", currentBlock);

            // 현재 투입량 슬라이더
            string colorHexdecimal = ColorUtility.ToHtmlStringRGB(inputAmountTextColor);
            string inputAmountString = $"<color=#{colorHexdecimal}>(+{currentBlock})</color>";

            int currentAmount = stockItem.Amount;
            int totalCount = GetTableMaxCount();
            currentUpgradeProgressBar.value = (float)currentAmount / totalCount;
            currentUpgradeProgressText.text = $"{currentAmount} {inputAmountString}/{totalCount}";

            // 투입 예정량 슬라이더
            float expectedAmount = (float)(currentAmount + currentBlock) / totalCount;
            expectedUpgradeProgressBar.DOValue(expectedAmount, 0.2f);

            // 인풋 슬라이더
            inputBlockSlider.maxValue = maxBlock;
            inputBlockSlider.value = currentBlock;

            // 버튼 상태 갱신
            isAvailInsertBlock = currentBlock > 0;
            insertBlockButton.SetButtonSpriteState(isAvailInsertBlock);
        }

        void Clear()
        {
            currentBlock = 0;
            maxBlock = 0;

            currentUpgradeProgressBar.value = 0;
            expectedUpgradeProgressBar.value = 0;
            inputBlockSlider.value = 0;

            isAvailInsertBlock = false;
        }

        bool IsDataNull()
        {
            if (currentItem == null || stockItem == null || tableData == null)
            {
                Debug.LogError("Block Input Popup Load Error!");
                return true;
            }

            return false;
        }

        int GetTableMaxCount()//데이터 테이블 상의 업그레이드 하기 위한 목표치
        {
            if (IsDataNull())
                return -1;

            var requireAssetList = tableData.MATERIAL_LIST;
            if (requireAssetList == null || requireAssetList.Count <= 0)
                return -1;
            var requestItem = requireAssetList.Find(element => element.ItemNo == currentItem.ItemNo);
            return requestItem.Amount;
        }
    }
}
