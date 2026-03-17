using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ProductManageProduceOptionPopup : Popup<ProductManageProduceOptionData>
    {
        [SerializeField] ToggleGroup filterToggleGroup = null;
        [SerializeField] Toggle[] toggleList = null;

        public override void InitUI()
        {
            if (Data == null || Data.parentPopup == null) return;
            if (toggleList == null || toggleList.Length <= 0) return;

            eProduceOptionFilter optionFilter = Data.parentPopup.ProduceOption;
            if (toggleList[(int)optionFilter] != null)
            {
                toggleList[(int)optionFilter].isOn = true;
            }
        }

        public void OnClickFilterToggle(int filterValue)
        {
            if (Data == null || Data.parentPopup == null) return;

            switch (filterValue) 
            {
                case (int)eProduceOptionFilter.PRODUCT_TIME_SHORT:
                    break;
                case (int)eProduceOptionFilter.PRODUCT_TIME_LONG:
                    break;
            }

            Data.parentPopup.ProduceOption = (eProduceOptionFilter)filterValue;
        }

        public void OnClickPrudceAllConfirmButton()
        {
            Data?.parentPopup?.ProduceAllProduct();

            PopupManager.ClosePopup<ProductManageProduceOptionPopup>();
        }
    }
}