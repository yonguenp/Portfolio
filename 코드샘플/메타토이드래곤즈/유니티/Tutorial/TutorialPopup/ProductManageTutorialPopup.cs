using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ProductManageTutorialPopup : Popup<PopupData>
    {

        [SerializeField]
        ProductManageSlot slot1;
        [SerializeField]
        ProductManageSlot slot2;
        [SerializeField]
        ProductManageRecipeCard tutorialItem;
        [SerializeField]
        Button GetAllBtn;
        [SerializeField]
        Button GetFillBtn;

        public override void InitUI()
        {
            var buildingList = User.Instance.GetAllProducesList(true);
            int tag1 = TutorialManager.tutorialManagement.GetCurTutoPrivateKey(0);
            int tag2 = TutorialManager.tutorialManagement.GetCurTutoPrivateKey(1);
            var brickBuilding = buildingList.Find(dat => dat.OpenData.INSTALL_TAG == tag1);
            var brickToolBuilding = buildingList.Find(dat => dat.OpenData.INSTALL_TAG == tag2);
            var buildInfo = User.Instance.GetUserBuildingInfoByTag(tag2);
            slot1.InitSlot(brickBuilding, null);
            slot2.InitSlot(brickToolBuilding, null);
            

            var recipeDat = ProductData.GetProductListByGroup(brickToolBuilding.OpenData.BUILDING)[0];
            tutorialItem.InitRecipeCard(recipeDat, buildInfo, slot2, slot2.AfterEnqueueProduct);
        }
    }

}