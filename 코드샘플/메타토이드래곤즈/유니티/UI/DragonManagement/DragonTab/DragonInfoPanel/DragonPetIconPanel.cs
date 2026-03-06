using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonPetIconPanel : MonoBehaviour
    {
        [SerializeField]
        DragonTabLayer tabLayer = null;

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

        int tempDragonTag = 0;
        int tempPetTag = 0;
        public void Init()
        {
            RefreshCurrentDragonData();
        }

        void RefreshCurrentDragonData()
        {
            if (PopupManager.GetPopup<DragonManagePopup>().CurDragonTag != 0)//드래곤 태그값
            {
                var dragonTag = PopupManager.GetPopup<DragonManagePopup>().CurDragonTag;
                var dragonData = User.Instance.DragonData;
                if (dragonData == null)
                {
                    Debug.Log("user's dragon Data is null");
                    return;
                }

                tempPetTag = 0;
                UserDragon userDragon = null;
                var hasDragon = User.Instance.DragonData.IsUserDragon(dragonTag);
                if (hasDragon)
                {
                    userDragon = User.Instance.DragonData.GetDragon(dragonTag);
                    tempPetTag = userDragon.Pet;
                }

                tempDragonTag = dragonTag;
                

                RefreshPetButton();
                RefreshIconNode(userDragon);
            }
        }
        public void OnClickPetTab()
        {
            if (PopupManager.GetPopup<DragonManagePopup>().CurDragonTag != 0)//드래곤 태그값
            {
                var dragonManagePopup = PopupManager.GetPopup<DragonManagePopup>();
                if (dragonManagePopup != null)
                    dragonManagePopup.moveTab(new DragonTabTypePopupData(1, 1));
            }
        }
        void RefreshIconNode(UserDragon dragonData)
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
                var petData = User.Instance.PetData.GetPet(petTag);
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
                var hasDragon = User.Instance.DragonData.IsUserDragon(tempDragonTag);
                lockNodeImage.sprite = hasDragon ? plus_icon : lock_icon;
                lockNodeImage.GetComponent<RectTransform>().sizeDelta = hasDragon ? new Vector2(126, 126) : new Vector2(126, 157);
            }
        }

        public void OnClickChangeLayer(string _petTag = "")
        {
            if (!IsPetChangeLayerCondition(true))
                return;


            var dragonManagePopup = PopupManager.GetPopup<DragonManagePopup>();
            if (dragonManagePopup != null)
            {
                if (_petTag != "")
                    PopupManager.GetPopup<DragonManagePopup>().CurPetTag = int.Parse(_petTag);
                else if (tempPetTag > 0)
                {
                    PopupManager.GetPopup<DragonManagePopup>().CurPetTag = tempPetTag;
                }
                else
                {
                    PopupManager.GetPopup<DragonManagePopup>().CurPetTag = 0;
                }

                if (dragonManagePopup.CurDragonTag != 0)//드래곤 태그값
                {
                    dragonManagePopup.moveTab(new DragonTabTypePopupData(1, 1));
                }
            }
        }

        bool IsPetChangeLayerCondition(bool _isShowToast = false)
        {
            var hasDragon = User.Instance.DragonData.IsUserDragon(tempDragonTag);
            if(hasDragon)
            {
                var petAllData = User.Instance.PetData.GetAllUserPets();
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
            else
            {
                if(_isShowToast)
                    ToastManager.On(StringData.GetStringByStrKey("미획득드래곤알림"));
                return false;
            }
        }
    }
}

