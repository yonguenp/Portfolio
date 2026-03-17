using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionDragonDetailPetIconPanel : MonoBehaviour
    {
        [SerializeField]
        ChampionBattleDragonSelectTabLayer tabLayer = null;

        [SerializeField]
        PetPortraitFrame petPortrait = null;

        [SerializeField]
        GameObject lockNode  = null;

        [SerializeField]
        Button petIconButton = null;

        [SerializeField]
        Image lockNodeImage = null;

        [SerializeField]
        Sprite lock_icon = null;

        ChampionDragon Dragon = null;
        public void Init()
        {
            RefreshCurrentDragonData();
        }

        void RefreshCurrentDragonData()
        {
            if (PopupManager.GetPopup<ChampionDragonDetailPopup>().Dragon != null)//드래곤 태그값
            {
                Dragon = PopupManager.GetPopup<ChampionDragonDetailPopup>().Dragon;
                if (Dragon == null)
                {
                    Debug.Log("user's dragon Data is null");
                    return;
                }
                
                RefreshPetButton();
                RefreshIconNode(Dragon);
            }
        }
        public void OnClickPetTab()
        {
            if (Dragon != null)//드래곤 태그값
            {
                var dragonManagePopup = PopupManager.GetPopup<ChampionDragonDetailPopup>();
            }
        }
        void RefreshIconNode(ChampionDragon dragonData)
        {
            if (dragonData == null)
            {
                petPortrait.gameObject.SetActive(false);
                lockNode.gameObject.SetActive(true);
                return;
            }

            var petTag = dragonData.Pet;
            var isBelongingDragon = petTag > 0;

            petPortrait.gameObject.SetActive(isBelongingDragon);
            lockNode.gameObject.SetActive(!isBelongingDragon);

            if (isBelongingDragon)
            {
                var petData = dragonData.ChampionPet;
                if (petData == null)
                    return;

                petPortrait.SetPetPortraitFrame(petData, false, false);
                petPortrait.SetCallback(OnClickChangeLayer);
            }
        }

        void RefreshPetButton()
        {
            if (petIconButton != null)
                petIconButton.SetButtonSpriteState(IsPetChangeLayerCondition());

        }

        public void OnClickChangeLayer(string _petTag = "")
        {
            if (!IsPetChangeLayerCondition(true))
                return;

            PopupManager.GetPopup<ChampionDragonDetailPopup>().PetClear();
            var dragonManagePopup = PopupManager.GetPopup<ChampionDragonDetailPopup>();
        }

        bool IsPetChangeLayerCondition(bool _isShowToast = false)
        {
                var petAllData = ChampionManager.GetSelectablePets();
                int petCount;
                if (petAllData == null || petAllData.Count <= 0)
                    petCount = 0;
                else
                    petCount = petAllData.Count;

                if (petCount <= 0)
                {
                    if(_isShowToast)
                        ToastManager.On(100002234);
                    return false;
                }
                else
                    return true;
        }
    }
}

