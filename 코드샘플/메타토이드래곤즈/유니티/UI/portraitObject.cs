using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace SandboxNetwork { 
    public class portraitObject : MonoBehaviour
    {

        [SerializeField] private Image profileImage = null;
        [SerializeField] private Image profileImageBack = null;

        [SerializeField] private Sprite[] rankBackSprites = null;

        [SerializeField] private Image userFrame = null;
        [SerializeField] GameObject addNode = null;//꾸미기 (보상용) 프레임 추가 노드
        [SerializeField] Image topAddImg = null;
        [SerializeField] Image botAddImg = null;
        [SerializeField] GameObject crowns = null;
        //[SerializeField] private Image profileFrame = null;
        // Start is called before the first frame update
        public void SetProfile()
        {
            string dragonID = User.Instance.UserData.UserPortrait;

            if (string.IsNullOrWhiteSpace(dragonID) || CharBaseData.GetAllList().Exists(element => element.KEY.ToString() == dragonID) == false)
            {
                SetDefaultProfile();
            }
            else
            {
                var dragonInfo = CharBaseData.Get(dragonID);
                if (dragonInfo != null)
                {
                    profileImage.sprite = dragonInfo.GetThumbnail();

                    //cbt 에선 전부 디폴트 백판
                    profileImageBack.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, "default_infobg");

                    //profileImageBack.sprite = GetCustomGradeBG(dragonInfo.GRADE);
                }
            }

            SetPortraitReward();
        }
        public void OnClickProfileImg()
        {
            //return;//not use in cbt
            var popup = PopupManager.OpenPopup<PortraitPopup>();
            popup.SetUICallback(SetProfile);
        }

        void SetDefaultProfile()
        {
            profileImage.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.CharIconPath,"mtdz_cap");
            profileImageBack.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, "default_infobg");
        }

        Sprite GetCustomGradeBG(int grade)
        {
            if (grade > 0 && grade - 1 < rankBackSprites.Length)
            {
                return rankBackSprites[grade - 1];
            }
            else
                return ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, "default_infobg");
        }

        void SetPortraitReward()
        {
            var userPortraitData = User.Instance.UserData.UserPortraitFrameInfo;
            if (userPortraitData == null)
                return;

            userPortraitData.SetPortraitReward(userFrame, addNode, topAddImg, botAddImg, crowns);
        }
    }
}