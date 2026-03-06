using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{

    public struct PetDataEvent
    {
        public enum PetEvent
        {
            LOCK_STATE,
            FOCUS_FRAME,
        }
        private static PetDataEvent obj;
        public PetEvent type;
        public int target_tag;
        public bool isShow;

        public static void Send(PetEvent type, int tag)
        {
            obj.type = type;
            obj.target_tag = tag;

            EventManager.TriggerEvent(obj);
        }

        public static void FocusFrame(int tag, bool _isShow)
        {
            obj.type = PetEvent.FOCUS_FRAME;
            obj.target_tag = tag;
            obj.isShow = _isShow;

            EventManager.TriggerEvent(obj);
        }

    }
    public class PetPortraitFrame : MonoBehaviour, EventListener<PetDataEvent>
    {
        [SerializeField]
        bool isChampionPet = false;

        [SerializeField]
        GameObject[] gradeNode = null;

        [SerializeField]
        Image sprIcon = null;

        [SerializeField]
        Text levelNode = null;

        [SerializeField]
        Text reinforceLabel = null;

        [SerializeField]
        Image elementIcon = null;

        [SerializeField]
        GameObject selectCheckNode = null;//선택되어진 상태면 딤드 + 체크 처리

        [SerializeField]
        GameObject clickNode = null;

        // 장착 드래곤 썸네일 관련
        [SerializeField]
        GameObject dragonThumbnNailNode = null;
        [SerializeField]
        Image dragonBg = null;
        [SerializeField]
        Image dragonIcon = null;

        [SerializeField]
        SlotFrameController frame = null;

        [SerializeField]
        Sprite[] customGradeBGList = null;
        [SerializeField]
        Sprite defaultCustomImage = null;
        [SerializeField]
        Image bgSprite = null;
        [SerializeField]
        GameObject emptyNode = null;
        [SerializeField]
        GameObject lockIcon = null;
        
        public delegate void func(string CustomEventData);

        private func clickCallback = null;
        public int PetTag { get; private set; } = 0;
        UserPet petData = null;

        public bool IsBelonged { get; private set; } = false;
        public bool IsSelected { get; private set; } = false;
        public bool IsClicked { get; private set; } = false;

        const int PET_LEGENDARY_GRADE = 5;

        void OnEnable()
        {
            EventManager.AddListener<PetDataEvent>(this);
        }

        void OnDisable()
        {
            EventManager.RemoveListener<PetDataEvent>(this);
        }

        public void SetPetPortraitFrame(UserPet petData, bool isSelectCheck = false, bool isVisibleBelongedDragon = true,
         bool isVisibleReinforceLevel = true)
        {
            //if (petData == this.petData)
            //{
            //    InitDefaultPortraitFrame(petData, isSelectCheck, isVisibleBelongedDragon, isVisibleReinforceLevel);
            //    return;
            //}
            if (emptyNode != null)
            {
                emptyNode.SetActive(false);
            }

            var dragonTag = GetDragonTag(petData.Tag);

            var isDragonTag = (dragonTag > 0);
            var isVisibleBelongedCheck = isDragonTag && isVisibleBelongedDragon;
            dragonThumbnNailNode.gameObject.SetActive(isVisibleBelongedCheck);
            if (isVisibleBelongedCheck)
            {
                DrawDragonIcon(dragonTag);//드래곤 초상화 세팅
            }
            IsBelonged = isDragonTag;

            this.petData = petData;

            var level = this.petData.Level;
            var petTag = this.petData.Tag;
            var petID = this.petData.ID;
            var petReinforce = this.petData.Reinforce;
            var petInfo = PetBaseData.Get(petID);
            var elementType = petInfo != null ? petInfo.ELEMENT : 0;
            var grade = petInfo != null ? petInfo.GRADE : 0;

            if (selectCheckNode != null)
            {
                selectCheckNode.gameObject.SetActive(isSelectCheck);
                IsSelected = isSelectCheck;
            }

            if (levelNode != null)
            {
                levelNode.gameObject.SetActive(!(level <= 0));

                var levelStr = level.ToString();
                levelNode.text = levelStr;
            }

            if (reinforceLabel != null)
            {
                reinforceLabel.gameObject.SetActive(isVisibleReinforceLevel && petReinforce > 0);
                reinforceLabel.text = SBFunc.StrBuilder("+",petReinforce);
            }

            if (elementIcon != null)
            {
                elementIcon.gameObject.SetActive(!(elementType <= 0));
                var elementFrame = GetElementIconSpriteByIndex(elementType);
                elementIcon.sprite = elementFrame;
            }

            Sprite icon = null;
            if (petInfo != null)
            {
                icon = ResourceManager.GetResource<Sprite>(eResourcePath.PetIconPath, petInfo.THUMBNAIL);

                if (icon == null)
                {
                    icon = ResourceManager.GetResource<Sprite>(eResourcePath.PetIconPath, "enemy_0");
                }
            }

            if (bgSprite != null)
            {
                bgSprite.sprite = GetBGIconSpriteByIndex(grade, elementType);
            }

            sprIcon.sprite = icon;
            PetTag = petTag;
            if(frame != null)
            {
                frame.SetColor(grade);
            }

            if(lockIcon != null)
                lockIcon.SetActive(IsLock());
        }
        public void SetCustomPotraitFrame(int petID, int level)
        {

            var petInfo = PetBaseData.Get(petID);
            var elementType = petInfo != null ? petInfo.ELEMENT : 0;
            var grade = petInfo != null ? petInfo.GRADE : 0;
            if (emptyNode != null)
            {
                emptyNode.SetActive(false);
            }
            if (levelNode != null)
            {
                levelNode.gameObject.SetActive(!(level <= 0));
                var levelStr = level.ToString();
                levelNode.text = levelStr;
            }

            if (elementIcon != null)
            {
                elementIcon.gameObject.SetActive(!(elementType <= 0));
                var elementFrame = GetElementIconSpriteByIndex(elementType);
                elementIcon.sprite = elementFrame;
            }

            Sprite icon = null;
            if (petInfo != null)
            {
                icon = ResourceManager.GetResource<Sprite>(eResourcePath.PetIconPath, petInfo.THUMBNAIL);

                if (icon == null)
                {
                    icon = ResourceManager.GetResource<Sprite>(eResourcePath.PetIconPath, "enemy_0");
                }
            }

            if (bgSprite == null)
            {
                var bgNode = SBFunc.GetChildrensByName(gameObject.transform, new string[] { "Background" });
                if (bgNode != null)
                {
                    this.bgSprite = bgNode.GetComponent<Image>();
                }
            }

            if (bgSprite != null)
            {
                bgSprite.sprite = GetBGIconSpriteByIndex(grade, elementType);
            }
            reinforceLabel.gameObject.SetActive(false);
            sprIcon.sprite = icon;
            if (frame != null)
            {
                frame.SetColor(grade);
            }
            SetGradeFrameNode(grade);

            if (lockIcon != null)
                lockIcon.SetActive(false);
        }

        void SetGradeFrameNode(int gradeIndex)//grade는 1부터 시작이니 -1 처리
        {
            if (gradeNode == null || gradeNode.Length < 1 || gradeNode.Length < gradeIndex)
            {
                return;
            }

            var target = gradeIndex - 1;
            var gradeCount = gradeNode.Length;
            for (var i = 0; i < gradeCount; ++i)
            {
                var gradeNode = this.gradeNode[i];
                if (gradeNode == null)
                {
                    continue;
                }

                gradeNode.gameObject.SetActive(target == i);
            }
        }

        public void SetCallback(func ok_cb)
        {
            //this.eFuncType = FrameFunctioal.CallBack;

            if(ok_cb != null)
            {
                clickCallback = ok_cb;
            }
        }

        public void OnClickFrame()
        {
            if (clickCallback != null)
            {
                clickCallback(this.PetTag.ToString());
            }
        }

        public void SetVisibleSelectedNode(bool isVisible)
        {
            if (selectCheckNode != null)
            {
                if (selectCheckNode.activeInHierarchy == !isVisible)
                {
                    selectCheckNode.gameObject.SetActive(isVisible);
                }
                IsSelected = isVisible;
            }
        }

        //현재 펫이 드래곤에 링크되있으면 드래곤 태그 리턴
        int GetDragonTag(int petTag)
        {
            if (isChampionPet)
            {
                return PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().DragonTag;
            }

            return User.Instance.PetData.GetPetLink(petTag);
        }

        bool IsLock()
        {
            if (isChampionPet)
                return false;

            return User.Instance.Lock.IsLockPet(PetTag);
        }

        void DrawDragonIcon(int dragonTag)
        {
            var dragonInfo = CharBaseData.Get(dragonTag.ToString());
            if (dragonInfo != null)
            {
                dragonIcon.sprite = dragonInfo.GetThumbnail();
                dragonBg.sprite = GetBGIconSpriteByIndex(dragonInfo.GRADE);
            }
        }

        Sprite GetElementIconSpriteByIndex(int e_type)
        {
            var elementStr = GetElementConvertString(e_type);
            return ResourceManager.GetResource<Sprite>(eResourcePath.ElementIconPath, SBFunc.StrBuilder("icon_property_", elementStr));
        }


        private Sprite GetBGIconSpriteByIndex(int grade, int element = 0)
        {
            if (customGradeBGList != null) { 
                foreach(var obj in gradeNode)
                {
                    obj.SetActive(false);
                }
                return GetCustomGradeBG(grade);
            }
            else
            {
                var fullStr = MakeStringByGradeAndElement(grade, element);
                SetGradeFrameNode(grade);
                return ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, fullStr);
            }
        }

        Sprite GetCustomGradeBG(int grade)
        {
            if (grade > 0 && grade - 1 < customGradeBGList.Length)
            {
                return customGradeBGList[grade - 1];
            }
            else
                return defaultCustomImage;
        }

        string GetGradeConvertString(int grade)
        {
            return grade switch {
                2 => "r",
                3 => "sr",
                4 => "ur",
                5 => "l",
                _ => "n"
            };
        }

        string GetElementConvertString(int e_type)
        {
            var elementStr = "";
            switch (e_type)
            {
                case 1:
                    elementStr = "fire";
                    break;
                case 2:
                    elementStr = "water";
                    break;
                case 3:
                    elementStr = "soil";
                    break;
                case 4:
                    elementStr = "wind";
                    break;
                case 5:
                    elementStr = "light";
                    break;
                case 6:
                    elementStr = "dark";
                    break;
                default:
                    break;
            }
            return elementStr;
        }

        string MakeStringByGradeAndElement(int grade,int element)
        {
            var elementString = GetElementConvertString(element);
            var gradeString = GetGradeConvertString(grade);

            return SBFunc.StrBuilder(elementString, "_" + gradeString, "_infobg");
        }

        public void SetVisibleClickNode(bool isVisible)
        {
            if (clickNode != null)
            {
                clickNode.gameObject.SetActive(isVisible);
            }
            IsClicked = isVisible;
        }

        void InitDefaultPortraitFrame(UserPet petData, bool isSelectCheck = false, bool isVisibleBelongedDragon = true, bool isVisibleReinforceLevel = false)
        {
            if (selectCheckNode != null)
            {
                selectCheckNode.gameObject.SetActive(isSelectCheck);
                IsSelected = isSelectCheck;
            }

            var dragonTag = GetDragonTag(petData.Tag);

            var isDragonTag = dragonTag > 0;
            var isVisibleBelongedCheck = isDragonTag && isVisibleBelongedDragon;
            this.dragonThumbnNailNode.gameObject.SetActive(isVisibleBelongedCheck);
            if (isVisibleBelongedCheck)
            {
                DrawDragonIcon(dragonTag);//드래곤 초상화 세팅
            }
            IsBelonged = isDragonTag;

            var level = this.petData.Level;
            if (levelNode != null)
            {
                levelNode.gameObject.SetActive(!(level <= 0));

                var levelStr = level.ToString();
                levelNode.text = levelStr;
            }

            var petReinforce = this.petData.Reinforce;
            if (reinforceLabel != null)
            {
                reinforceLabel.gameObject.SetActive(isVisibleReinforceLevel && petReinforce > 0);
                reinforceLabel.text = SBFunc.StrBuilder("+", petReinforce);
            }

            if (lockIcon != null)
                lockIcon.SetActive(IsLock());

            //if (bgSprite != null)
            //{
            //    bgSprite.sprite = GetBGIconSpriteByIndex(petData.Grade(), petData.Element());
            //}
        }

        public void SetUpperPortrait(int targetGrade)
        {
            if (levelNode != null)
            {
                levelNode.gameObject.SetActive(true);
                levelNode.text = "1";
            }

            if (reinforceLabel != null)
            {
                reinforceLabel.gameObject.SetActive(true);
                reinforceLabel.text = SBFunc.StrBuilder("+", 0);
            }

            if (elementIcon != null)
            {
                elementIcon.gameObject.SetActive(false);
            }

            SetGradeFrameNode(targetGrade);

            //pet 결과 값 리소스 받아야함

        }


        public void SetEmptyNode()
        {
            if (emptyNode != null)
            {
                emptyNode.SetActive(true);
            }
        }

        public void OnEvent(PetDataEvent eventData)
        {
            switch (eventData.type)
            {
                case PetDataEvent.PetEvent.LOCK_STATE:
                    if (PetTag != eventData.target_tag)
                        return;

                    if (lockIcon != null)
                        lockIcon.SetActive(IsLock());
                    break;
                case PetDataEvent.PetEvent.FOCUS_FRAME:
                    if (clickNode != null)
                    {
                        var isShow = eventData.isShow;
                        if (isShow)
                            clickNode.SetActive(PetTag == eventData.target_tag);
                        else
                            clickNode.SetActive(false);
                    }
                    break;
            }
        }
    }
}
