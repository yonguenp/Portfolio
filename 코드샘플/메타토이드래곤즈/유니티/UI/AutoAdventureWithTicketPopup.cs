using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork {
    public class AutoAdventureWithTicketPopup : Popup<StagePopupData>
    {

        [SerializeField] private Button leftButton = null;
        [SerializeField] private Button rightButton = null;
        [SerializeField] private Button maxButton = null;
        [SerializeField] private Text autoCountLabel = null;
        [SerializeField] private Text compasLabel = null;
        [SerializeField] private Text ticketLabel = null;

        StageBaseData stageData = null;

        int userStamina = 0;
        int maxCount = 0;
        int currentClickNumber = 1;
        public delegate void CallBack(StageBaseData _stageData, int repeat);
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

            RefreshAssets();
        }


        void RefreshAssets()
        {
            compasLabel.text = (currentClickNumber * stageData.COST_VALUE).ToString();
            ticketLabel.text = (currentClickNumber * GameConfigTable.GetConfigIntValue("adventure_sweep_per_ticket_" + stageData.DIFFICULT + "_" + stageData.WORLD + "_" + stageData.STAGE, 1)).ToString();
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

            RefreshAssets();
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
            var staMax = (int)Mathf.Floor(userStamina / currentStageRequireAmount);
            var ticMax = User.Instance.Inventory.GetItem(20000013).Amount / GameConfigTable.GetConfigIntValue("adventure_sweep_per_ticket_" + stageData.DIFFICULT + "_" + stageData.WORLD + "_" + stageData.STAGE, 1);
            
            maxCount = Mathf.Min(staMax, ticMax, GameConfigTable.GetConfigIntValue("sweep_per_max", 100));
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
            PopupManager.ClosePopup<AutoAdventureWithTicketPopup>();
        }
        public void OnClickOK()
        {
            if (okCallBack != null) { 
                okCallBack(stageData, currentClickNumber);
            }
            PopupManager.ClosePopup<AutoAdventureWithTicketPopup>();
        }

    }
}