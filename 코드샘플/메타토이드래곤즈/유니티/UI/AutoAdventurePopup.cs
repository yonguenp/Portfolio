using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork {
    public class AutoAdventurePopup : Popup<StagePopupData>
    {

        [SerializeField] private Button leftButton = null;
        [SerializeField] private Button rightButton = null;
        [SerializeField] private Button maxButton = null;
        [SerializeField] private Text autoCountLabel = null;

        StageBaseData stageData = null;

        int userStamina = 0;
        int maxCount = 0;
        int currentClickNumber = 1;
        public delegate void CallBack(StageBaseData _stageData);
        CallBack okCallBack = null;


        public override void InitUI()
        {
            userStamina = 0;
            maxCount = 0;
            currentClickNumber = 1;
            stageData = null;
            okCallBack = null;

            RefreshUserStamina();
            RefreshStageData(Data);
            SetCalculateMaxCount();
            autoCountLabel.text = currentClickNumber.ToString();

            RefreshButton();
        }

        public void SetCallBack(CallBack cb)
        {
            if (cb != null && okCallBack == null)
            {
                okCallBack = cb;
            }
        }

        public override void Init(StagePopupData data)
        {
            base.Init(data);
        }

        void SetClickNumber(int modifyNum)
        {
            if (modifyNum <= 0)
            {
                currentClickNumber = 1;
            }
            else if (modifyNum > maxCount)
            {
                currentClickNumber = maxCount;
            }
            else
            {
                currentClickNumber = modifyNum;
            }
        }
        void SetCalculateMaxCount()
        {
            if (stageData == null)
            {
                maxCount = 0;
                return;
            }
            int currentStageRequireAmount = stageData.COST_VALUE;
            if (currentStageRequireAmount > userStamina)
            {
                maxCount = 0;
                return;
            }
            maxCount = (int)Mathf.Floor(userStamina / currentStageRequireAmount);
            
        }
        void RefreshStageData(StagePopupData newPopupData)
        {
            stageData = StageBaseData.GetByAdventureWorldStage(newPopupData.World, newPopupData.Stage);
        }
        void RefreshUserStamina()
        {
            userStamina = User.Instance.UserData.Energy;
        }
        void RefreshButton()
        {
            leftButton.SetInteractable(currentClickNumber > 1);
            rightButton.SetInteractable(currentClickNumber < maxCount);
            maxButton.SetInteractable(currentClickNumber < maxCount);
        }

        public void OnClickLeftBtn()
        {
            SetClickNumber(currentClickNumber - 1);
            RefreshButton();
            autoCountLabel.text = currentClickNumber.ToString();
        }
        public void OnClickRightBtn()
        {
            SetClickNumber(currentClickNumber + 1);
            RefreshButton();
            autoCountLabel.text = currentClickNumber.ToString();
        }
        public void OnClickMaxBtn()
        {
            SetClickNumber(maxCount);
            RefreshButton();
            autoCountLabel.text = currentClickNumber.ToString();
        }
        public void OnClickCancel()
        {
            PopupManager.ClosePopup<AutoAdventurePopup>();
        }
        public void OnClickOK()
        {
            StageManager.InitAccumData(currentClickNumber, currentClickNumber);
            if (okCallBack != null) { 
                okCallBack(stageData);
            }
            PopupManager.ClosePopup<AutoAdventurePopup>();
        }

    }
}