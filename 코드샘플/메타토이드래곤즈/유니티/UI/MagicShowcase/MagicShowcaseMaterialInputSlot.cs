using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 단계를 업그레이드 하기 위한 재료 UI 그리기
    /// </summary>
    public class MagicShowcaseMaterialInputSlot : MonoBehaviour
    {
        [SerializeField] ItemFrame item = null;
        [SerializeField] Slider slider = null;
        [SerializeField] Text progressLabel = null;
        [SerializeField] Button inputButton = null;
        [SerializeField] GameObject dimmedObject = null;
        [SerializeField] Sprite dimmedDefaultSprite = null;

        private bool isCheckStock = false;//투입량을 고려해야하는 항목인가

        private int currentMaxCount = -1;
        private int currentInputCount = -1;
        private int currentItemNo = -1;

        public int ItemNo { get { return currentItemNo; } }
        public bool IsCheckStock { get { return isCheckStock; } }
        public void InitUI(int _itemNo , int _currentCount , int _maxCount)
        {
            isCheckStock = true;
            currentItemNo = _itemNo;
            currentInputCount = _currentCount;
            currentMaxCount = _maxCount;

            dimmedObject.SetActive(false);
            SetItemFrame(_itemNo);
            SetSlider(_currentCount, _maxCount);
            RefreshButton();
        }

        void SetItemFrame(int _itemNo)
        {
            if(item != null)
                item.SetFrameItemInfo(_itemNo,0);
        }

        void SetSlider(int _currentCount, int _maxCount)
        {
            if (_currentCount > _maxCount)
                _currentCount = _maxCount;

            if (slider != null)
                slider.value = _maxCount <= 0.0f ? 0.0f : ((float)_currentCount / _maxCount);

            if (progressLabel != null)
                progressLabel.text = SBFunc.StrBuilder(_currentCount, "/", _maxCount);
        }

        void RefreshButton()
        {
            if (inputButton != null)
            {
                inputButton.interactable = true;
                inputButton.SetButtonSpriteState(!IsFullStocked());
            }
        }

        public bool IsFullStocked()//투입량이 많으면, 클릭안되게
        {
            return currentInputCount >= currentMaxCount;
        }
        public void SetDimmed(int _itemNo)
        {
            currentItemNo = _itemNo;
            currentInputCount = 0;
            currentMaxCount = 0;

            dimmedObject.SetActive(true);
            SetItemFrame(_itemNo);
            SetSlider(0, 0);

            inputButton.SetButtonSpriteState(false);

            isCheckStock = false;
        }

        public void DoMoveSliderDown(float time)
        {
            slider.DOValue(0, time).SetEase(Ease.InOutQuart);
        }
    }
}
