using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class EmptySlotFrame : MonoBehaviour
    {
        [SerializeField] Button addSlotBtn = null;
        [SerializeField] Image addSlotItemImg = null;
        [SerializeField] Text addSlotCostText = null;
        [SerializeField] GameObject maxSlotObj = null;

        BuildingBaseData currentBuildingData = null;
        ProducesBuilding currentProduceBuildingData = null;

        System.Action addSlotCallback = null;

        public void InitSlot(ProducesBuilding produceBuildingData ,BuildingBaseData buildingData, System.Action callback = null)
        {
            if (produceBuildingData == null || buildingData == null) return;

            currentProduceBuildingData = produceBuildingData;
            currentBuildingData = buildingData;

            addSlotCallback = callback;

            SetSlotData();
        }

        public void RefreshSlot(ProducesBuilding produceBuildingData, BuildingBaseData buildingData)
        {
            if (produceBuildingData == null || buildingData == null) return;

            currentProduceBuildingData = produceBuildingData;
            currentBuildingData = buildingData;

            SetSlotData();
        }

        public void OnClickAddSlot(ePriceDataFlag costType, int cost)
        {
            LandmarkDozer dozer = User.Instance.GetLandmarkData<LandmarkDozer>();

            // 코인 도저 건설여부 체크
            if (dozer != null && dozer.BuildInfo != null)
            {
                BuildInfo dozerData = dozer.BuildInfo;
                if (dozerData.Level > 0 || dozerData.State == eBuildingState.NORMAL)
                {
                    string popupTitle = StringData.GetStringByStrKey("production_slot_add");
                    //string popupSubTitle = StringData.GetStringByIndex(100000246);
                    string contentGuide = StringData.GetStringByStrKey("production_slot_add_text_01");
                    int price = cost < 0 ? 0 : cost;
                    ePriceDataFlag priceFlag = ePriceDataFlag.CloseBtn | ePriceDataFlag.ContentBG | costType;

                    PricePopup.OpenPopup(popupTitle, string.Empty, contentGuide, price, priceFlag, SendProductAddSlotAPI);

                    return;
                }
            }

            // 코인도저가 건설되지 않았을 경우 처리
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000430));
        }

        void SetSlotData()
        {
            SlotCostData slotCostInfo = SlotCostData.GetByType(eSlotCostInfoType.Product, (currentProduceBuildingData.Slot - currentBuildingData.START_SLOT) + 1);
                //SlotCostData.Get(((currentProduceBuildingData.Slot - currentBuildingData.START_SLOT) + 1).ToString());

            if (currentProduceBuildingData.Slot < currentBuildingData.MAX_SLOT && slotCostInfo != null)
            {
                gameObject.SetActive(true);
                maxSlotObj.SetActive(false);

                addSlotCostText.text = slotCostInfo.COST_NUM.ToString();
                ePriceDataFlag priceDataFlag = ePriceDataFlag.None;
                switch (slotCostInfo.COST_TYPE.ToUpper())
                {
                    case "GOLD":
                        addSlotItemImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gold");
                        priceDataFlag = ePriceDataFlag.Gold;
                        break;
                    case "GEMSTONE":
                        addSlotItemImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gemstone");
                        priceDataFlag = ePriceDataFlag.GemStone;
                        break;
                }
                addSlotBtn.onClick.RemoveAllListeners();
                addSlotBtn.onClick.AddListener(() => OnClickAddSlot(priceDataFlag, slotCostInfo.COST_NUM));
            }
            else
            {
                SetAddSlotMax();
            }
        }

        void SendProductAddSlotAPI()
        {
            WWWForm paramData = new WWWForm();
            paramData.AddField("tag", currentProduceBuildingData.Tag);

            addSlotBtn.interactable = false;

            NetworkManager.Send("building/expand", paramData, (JObject jsonData) =>
            {
                addSlotBtn.interactable = true;

                if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                {
                    switch (jsonData["rs"].Value<int>())
                    {
                        case (int)eApiResCode.OK:
                            addSlotCallback?.Invoke();
                            PopupManager.ClosePopup<PricePopup>();
                            break;
                    }
                }
            });
        }

        void SetAddSlotMax()
        {
            //maxSlotObj.SetActive(true);
            gameObject.SetActive(false);
            addSlotBtn.onClick.RemoveAllListeners();
        }
    }
}