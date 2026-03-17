using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionBattlePetIconPanel : MonoBehaviour
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
        Sprite plus_icon = null;

        [SerializeField]
        Sprite lock_icon = null;

        ChampionDragon Dragon = null;
        public void Init()
        {
            RefreshCurrentDragonData();
        }

        void RefreshCurrentDragonData()
        {
            if (PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon != null)//드래곤 태그값
            {
                Dragon = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;
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
                var dragonManagePopup = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>();
                if (dragonManagePopup != null)
                    dragonManagePopup.moveTab(new ChampionBattleDragonTabTypePopupData(1, 1));
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

            if(lockNodeImage != null)
            {
                //var hasDragon = User.Instance.DragonData.IsUserDragon(tempDragonTag);
                //lockNodeImage.sprite = hasDragon ? plus_icon : lock_icon;
                //lockNodeImage.GetComponent<RectTransform>().sizeDelta = hasDragon ? new Vector2(21, 21) : new Vector2(50, 50);

                lockNodeImage.sprite = plus_icon;
                lockNodeImage.GetComponent<RectTransform>().sizeDelta = new Vector2(21, 21);
            }
        }

        public void OnClickChangeLayer(string _petTag = "")
        {
            if (!IsPetChangeLayerCondition(true))
                return;

            int tag = 0;
            if (!string.IsNullOrEmpty(_petTag))
                tag = int.Parse(_petTag);
            PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().SetPetTag(tag);
            var dragonManagePopup = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>();
            if (dragonManagePopup.DragonTag != 0)//드래곤 태그값
            {
                if (dragonManagePopup != null)
                    dragonManagePopup.moveTab(new ChampionBattleDragonTabTypePopupData(1, 1));
            }
        }

        bool IsPetChangeLayerCondition(bool _isShowToast = false)
        {
            //var hasDragon = User.Instance.DragonData.IsUserDragon(tempDragonTag);
            //if(hasDragon)
            //{
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
            //}
            //else
            //{
            //    if(_isShowToast)
            //        ToastManager.On(StringData.GetStringByStrKey("미획득드래곤알림"));
            //    return false;
            //}
        }
    }
}

