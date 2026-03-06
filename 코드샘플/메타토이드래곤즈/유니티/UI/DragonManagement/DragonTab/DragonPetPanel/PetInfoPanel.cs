using Newtonsoft.Json.Linq;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PetInfoPanel : MonoBehaviour
    {

        [Header("Check panel")]
        public bool isDragonUI = false;//드래곤 패널쪽인지, 펫 패널쪽인지 체크

        [Space(10)]
        [Header("default setting")]
        [SerializeField]
        SubLayer petTapLayer = null;
        [SerializeField]
        PetListPanel petListPanel = null;
        [SerializeField]
        PetSkillInfoPanel skillInfoPanel = null;


        [Space(10)]

        [Header("Image group")]
        [SerializeField]
        private GameObject petImageParentNode = null;
        [SerializeField]
        private Image petBGTarget = null;
        [SerializeField]
        private Vector3 characterScale = new Vector3(4, 4, 4);
        [SerializeField]
        private Vector3 characterPos = new Vector3(0, -125, 0);
        [SerializeField]
        private Sprite emptyImage = null;

        [Space(10)]

        [Header("Desc group")]

        [SerializeField]
        private GameObject petDescNode = null;
        [SerializeField]
        private GameObject EquipDragonNode = null;
        [SerializeField]
        private Text petNameLabel = null;
        [SerializeField]
        private Text petLevelLabel = null;
        [SerializeField]
        private Text petReinforceLevelLabel = null;
        [SerializeField]
        private Image petElementSprite = null;
        [SerializeField]
        private GameObject[] petGradeNodes = null;
        [SerializeField]
        public Color[] gradeColor = null;

        [Space(10)]

        [Header("Skill group")]

        [SerializeField]
        private GameObject[] skillImageNode = null;
        [SerializeField]
        private Sprite[] SkillBGSprite = null;

        [Space(10)]
        [Header("button group - dragon Panel")]

        [SerializeField]
        Button petTabClickButton = null;
        [SerializeField]
        Button equipButton = null;
        [SerializeField]
        Button unequipButton = null;

        [Space(10)]

        [Header("button group - pet Panel")]
        [SerializeField]
        Button compoundButton = null;
        [SerializeField]
        Button levelUpButton = null;
        [SerializeField]
        Sprite normalButtonSprite = null;
        [SerializeField]
        Sprite disableButtonSprite = null;


        private int dragonTag = -1;
        private int petTag = -1;

        public void Init()
        {
            if (isDragonUI)//드래곤 패널 쪽에서만 세팅
            {
                initCurrentDragonData();
            }
            else
            {
                dragonTag = -1;
            }

            RefreshCurrentPetData();
        }

        void initCurrentDragonData()
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

                var userDragonInfo = dragonData.GetDragon(dragonTag);
                if (userDragonInfo == null)
                {
                    Debug.Log("user Dragon is null");
                    return;
                }

                this.dragonTag = dragonTag;
            }
        }

        void RefreshCurrentPetData()
        {
            if (skillInfoPanel != null)
            {
                skillInfoPanel.gameObject.SetActive(false);
            }

            if (PopupManager.GetPopup<DragonManagePopup>().CurPetTag != 0)//펫 태그값
            {
                var petTag = PopupManager.GetPopup<DragonManagePopup>().CurPetTag;
                var petData = User.Instance.PetData;
                if (petData == null)
                {
                    Debug.Log("user's pet Data is null");
                    return;
                }

                var userPetInfo = petData.GetPet(petTag);
                if (userPetInfo == null)
                {
                    Debug.Log("user pet is null");
                    return;
                }

                this.petTag = petTag;

                RefreshDefaultUI();
                RefreshPetCharacterSlot(userPetInfo);
                RefreshPetCharacterBG(userPetInfo);
                SetBelongedDragonPortrait(userPetInfo.LinkDragonTag);
                RefreshPetDesc(userPetInfo);
                RefreshPetSkillIcon(userPetInfo);

                if (isDragonUI)//드래곤 패널 귀속 상태일때 버튼 규칙 (장착/ 해제/ 펫 탭 이동 버튼)
                {
                    RefreshButtonByBelonged(userPetInfo);
                }
                else//펫 내부 버튼 (강화, 합성, 레벨업)
                {
                    RefreshPetGrowButtonUI(userPetInfo);
                }
            }
            else//빈 값일 때 처리 (초기화)
            {
                initSkillIconButton();
                SetBelongedDragonPortrait(-1);
                if (petDescNode != null)
                {
                    petDescNode.SetActive(false);
                }

                if (petImageParentNode != null)
                {
                    SBFunc.RemoveAllChildrens(petImageParentNode.transform);
                }

                if (petBGTarget != null)
                {
                    petBGTarget.sprite = emptyImage;
                }

                SetButtonInteractable(false);
            }
        }

        void SetButtonInteractable(bool isInteractable)
        {
            if (petTabClickButton != null)
            {
                petTabClickButton.SetInteractable(isInteractable);
            }
            if (equipButton != null)
            {
                equipButton.gameObject.SetActive(true);
                equipButton.SetInteractable(isInteractable);
            }

            if (unequipButton != null)
            {
                unequipButton.gameObject.SetActive(false);
                unequipButton.SetInteractable(isInteractable);
            }
        }

        void RefreshButtonByBelonged(UserPet userPetInfo)
        {
            SetButtonInteractable(true);

            var belongedTag = userPetInfo.LinkDragonTag;

            var isBelonged = belongedTag > 0;

            equipButton.gameObject.SetActive(!isBelonged);
            unequipButton.gameObject.SetActive(isBelonged);

            equipButton.SetInteractable(!isBelonged);
            unequipButton.SetInteractable(isBelonged);
        }

        void RefreshPetGrowButtonUI(UserPet petInfo)//강화, 합성 레벨업 버튼 규칙 적용
        {
            //레벨업 버튼
            // let currentPetLevel = petInfo.Level;
            // let petMaxLevel= GameConfigTable.GetPetLevelMax();
            // if(levelupButton != null){
            //     levelupButton.interactable = (currentPetLevel < petMaxLevel);
            // }

            //강화 버튼


            //합성 버튼
            var compoundCheck = GetCompoundButtonCondition(petInfo);
            switch (compoundCheck)
            {
                case 0:
                    SetButtonSprite(compoundButton, normalButtonSprite);
                    break;
                case 1:
                case 2:
                    SetButtonSprite(compoundButton, disableButtonSprite);
                    break;
                default:
                    break;
            }
        }
        void SetButtonSprite(Button button, Sprite sprite)
        {
            if (button == null || sprite == null)
            {
                return;
            }

            button.image.sprite = sprite;
        }

        int GetCompoundButtonCondition(UserPet petInfo)//현재 버튼의 상태 가져오기
        {
            //case 0 : 합성 가능
            //case 1 : 장착함
            //case 2 : 만렙, 만강이 아님

            var isBelonged = petInfo.LinkDragonTag > 0;
            if (isBelonged)
            {
                return 1;
            }
            else
            {
                var level = petInfo.Level;
                var reinforce = petInfo.Reinforce;
                var grade = petInfo.Grade();
                if (level < GameConfigTable.GetPetLevelMax(grade) || reinforce < GameConfigTable.GetPetReinforceLevelMax(grade))
                {
                    return 2;
                }
                else
                {
                    return 0;
                }
            }
        }

        void RefreshDefaultUI()//초기 세팅때 껏던 오브젝트 켜고 , 반대도 세팅
        {
            if (petDescNode != null)
            {
                petDescNode.SetActive(true);
            }
        }

        void RefreshPetCharacterSlot(UserPet petInfo)
        {
            if (petImageParentNode == null)
            {
                return;
            }

            SBFunc.RemoveAllChildrens(petImageParentNode.transform);

            var petImageName = GetPetImageName(petInfo.ID);
            if (petImageName != "")
            {
                var baseData = petInfo.GetPetDesignData();
                var petPrefab = SBFunc.GetPetSpineByName(petImageName, eSpineType.UI);
                if (petPrefab != null)
                {
                    var clone = Instantiate(petPrefab, petImageParentNode.transform);
                    clone.GetComponent<RectTransform>().anchoredPosition = characterPos;
                    clone.GetComponent<RectTransform>().localScale = characterScale;
                    var petSpine = clone.GetComponent<UIPetSpine>();
                    if (petSpine != null)
                    {
                        petSpine.Init();
                        if (baseData != null && baseData.SKIN != "NONE")
                        {
                            petSpine.SetSkin(baseData.SKIN);
                        }
                    }
                }
            }
        }

        void RefreshPetCharacterBG(UserPet petInfo)
        {
            var designData = petInfo.GetPetDesignData();
            var petGrade = designData.GRADE;
            var petElement = designData.ELEMENT;

            var resourceString = MakeStringByGradeAndElement(petGrade, petElement);

            if (petBGTarget != null)
            {
                var icon = ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, resourceString);
                if (icon == null)
                {
                    return;
                }
                petBGTarget.sprite = icon;
            }
        }

        void SetBelongedDragonPortrait(int dragonTag)
        {
            var isBelonged = dragonTag > 0;
            if (!isBelonged)
            {
                EquipDragonNode.SetActive(false);
            }
            else
            {
                var dragonData = User.Instance.DragonData.GetDragon(dragonTag);
                if (dragonData == null)
                {
                    EquipDragonNode.SetActive(false);
                    return;
                }
                EquipDragonNode.SetActive(true);

                var dragonInfo = CharBaseData.Get(dragonTag.ToString());

                var grade = dragonData.Grade();
                var index = grade - 1;

                if (index >= gradeColor.Length)
                {
                    EquipDragonNode.SetActive(false);
                    return;
                }

                var color = gradeColor[grade - 1];

                var bg = SBFunc.GetChildrensByName(EquipDragonNode.transform, new string[] { "Bg" }).GetComponent<Image>();
                var emtpyBg = SBFunc.GetChildrensByName(EquipDragonNode.transform, new string[] { "EmptyBg" }).GetComponent<Image>();
                var dragonPortait = SBFunc.GetChildrensByName(EquipDragonNode.transform, new string[] { "Dragon_portrait" }).GetComponent<Image>();

                Sprite icon = null;
                if (dragonInfo != null)
                    icon = dragonInfo.GetThumbnail();

                bg.color = color;
                emtpyBg.color = color;
                dragonPortait.sprite = icon;
            }
        }

        void RefreshPetDesc(UserPet petInfo)
        {
            var data = StringData.GetStringByIndex(100000056);
            if (data != null)
            {
                petLevelLabel.text = string.Format(data, petInfo.Level);
                petReinforceLevelLabel.text = SBFunc.StrBuilder("+", petInfo.Reinforce);
            }
            if (petNameLabel != null)
            {
                petNameLabel.text = SBFunc.StrBuilder("[", petInfo.Name(), "]");
            }
            petElementSprite.sprite = GetElementIcon(petInfo.Element());
            RefreshGrade(petInfo.Grade());
        }

        void RefreshGrade(int grade)
        {
            var target = grade - 1;
            var count = petGradeNodes.Length;
            for (var i = 0; i < count; ++i)
            {
                var sprites = petGradeNodes[i];
                if (sprites == null)
                {
                    continue;
                }

                sprites.SetActive(i == target);
            }
        }

        void RefreshPetSkillIcon(UserPet petInfo)
        {
            initSkillIconButton();

            if (petInfo == null)
            {
                return;
            }

            var petSkillList = petInfo.SkillsID;//고유스킬은 빠진다고함
            var currentSkillIconLength = skillImageNode.Length;
            if (petSkillList == null || petSkillList.Length <= 0)
            {
                return;
            }

            if (petSkillList.Length > currentSkillIconLength)
            {
                return;
            }

            var petNormalSkillLevel = petInfo.Level;

            SetBelongedDragonPortrait(petInfo.LinkDragonTag);

            for (var i = 0; i < petSkillList.Length; i++)
            {
                var totalNode = skillImageNode[i];
                if (totalNode == null)
                {
                    continue;
                }
                var iconNode = SBFunc.GetChildrensByName(totalNode.transform, new string[] { "icon_bg", "skill_icon" });
                if (iconNode == null)
                {
                    continue;
                }
                var spriteComp = iconNode.GetComponent<Image>();
                if (spriteComp == null)
                {
                    continue;
                }

                var labelNode = SBFunc.GetChildrensByName(totalNode.transform, new string[] { "Label" });
                if (labelNode == null)
                {
                    continue;
                }
                var skillLabel = labelNode.GetComponent<Text>();
                if (skillLabel == null)
                {
                    continue;
                }

                var bgNode = SBFunc.GetChildrensByName(totalNode.transform, new string[] { "icon_bg" });
                if (bgNode == null)
                {
                    continue;
                }

                var petSkillID = petSkillList[i];
                if (petSkillID <= 0)
                {
                    continue;
                }

                var skillData = PetSkillNormalData.Get(petSkillID.ToString());
                if (skillData == null)
                {
                    continue;
                }

                totalNode.SetActive(true);

                var skillValue = PetSkillNormalData.GetSkillValue(petSkillID, petNormalSkillLevel);
                skillLabel.text = SBFunc.StrBuilder(Math.Round(skillValue, 2), "%");

                var skillStat = skillData.stat;
                var tempBGIndex = 0;
                switch (skillStat)//0 : 파랑(def), 1 : 빨강(atk), 2 : 보라(cri), 3 : 초록 (hp)
                {
                    case "DEF":
                        tempBGIndex = 0;
                        break;
                    case "ATK":
                        tempBGIndex = 1;
                        break;
                    case "CRI_RATE":
                        tempBGIndex = 2;
                        break;
                    case "HP":
                        tempBGIndex = 3;
                        break;
                }

                var spriteBGComp = bgNode.GetComponent<Image>();
                if (spriteBGComp != null && tempBGIndex < SkillBGSprite.Length)
                {
                    spriteBGComp.sprite = SkillBGSprite[tempBGIndex];
                }

                //임시로 스킬 아이콘 버프로 씁니다.
                spriteComp.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.SkillIconPath, skillData.icon);
            }
        }

        public void onClickSkillIcon()
        {
            if (skillInfoPanel != null)
            {
                skillInfoPanel.onChangeVisible(petTag);
            }
        }

        void initSkillIconButton()
        {
            if (skillImageNode == null || skillImageNode.Length <= 0)
            {
                return;
            }

            for (var i = 0; i < skillImageNode.Length; i++)
            {
                var imageNode = skillImageNode[i];
                if (imageNode == null)
                {
                    continue;
                }

                imageNode.SetActive(false);
            }
        }

        string GetPetImageName(int petID)
        {
            var petData = PetBaseData.Get(petID.ToString());
            if (petData != null)
            {
                return petData.IMAGE;
            }
            return "";
        }

        string GetGradeConvertString(int grade)
        {
            return grade switch
            {
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

        Sprite GetElementIcon(int e_type)
        {
            var elementStr = GetElementConvertString(e_type);
            return ResourceManager.GetResource<Sprite>(eResourcePath.ElementIconPath, SBFunc.StrBuilder("type_", elementStr));
        }

        string MakeStringByGradeAndElement(int grade, int element)
        {
            var elementString = GetElementConvertString(element);
            var gradeString = GetGradeConvertString(grade);

            return SBFunc.StrBuilder(elementString, "_", gradeString, "_bg");
        }

        public void RequestEquipPet()
        {
            var currentDragonData = User.Instance.DragonData.GetDragon(dragonTag);
            if (currentDragonData == null)
            {
                return;
            }

            var currentBelongingPet = currentDragonData.Pet;
            var isPrevOtherPet = currentBelongingPet > 0;

            if (isPrevOtherPet)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100001764), StringData.GetStringByIndex(100001357),
                    () => {
                        //ok
                        SendMsgEquipPet(true, currentBelongingPet);
                    },
                    () => {
                        //cancle
                    },
                    () => {
                        //x
                    }
                );
            }
            else
            {
                SendMsgEquipPet(false);
            }
        }

        void SendMsgEquipPet(bool isPrevOtherPet, int currentBelongingPet = -1)
        {
            var param = new WWWForm();
            param.AddField("did", dragonTag);
            param.AddField("ptag", petTag);

            Debug.Log("equip pet : " + petTag + "  Target Dragon : " + dragonTag);

            NetworkManager.Send("pet/equip", param, (jsonData) =>
            {
                //push로 dragon_update로 펫 세팅 해줌
                if (isPrevOtherPet)//이전에 장착한 드래곤이 있다면
                {
                    var petData = User.Instance.PetData.GetPet(currentBelongingPet);
                    if (petData != null)
                    {
                        petData.SetLinkDragonTag(-1);
                    }
                }

                if (jsonData.ContainsKey("rs") && (eApiResCode)jsonData["rs"].Value<int>() == eApiResCode.OK)
                {
                    PopupManager.GetPopup<DragonManagePopup>().CurPetTag = petTag;
                   
                    petListPanel.Init();
                    
                }
                else
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000621), StringData.GetStringByIndex(100000614));
                }
            });
        }

        public void RequestUnEquipPet()
        {
            var petData = User.Instance.PetData.GetPet(petTag);
            if (petData == null)
            {
                return;
            }

            var belongedTag = petData.LinkDragonTag;
            if (belongedTag <= 0)
            {
                return;
            }

            var param = new WWWForm();
            param.AddField("did", belongedTag);

            Debug.Log("unequip pet : " + petTag + "  Target Dragon : " + belongedTag);
            NetworkManager.Send("pet/unequip", param, (jsonData) =>
            {
                if (jsonData.ContainsKey("rs") && (eApiResCode)jsonData["rs"].Value<int>() == eApiResCode.OK)
                {

                    var curPetTag = jsonData["ptag"].Value<int>();
                    var petData = User.Instance.PetData.GetPet(curPetTag);
                    if (petData != null)
                    {
                        petData.SetLinkDragonTag(-1);
                    }

                    PopupManager.GetPopup<DragonManagePopup>().CurPetTag = petTag;
                    petListPanel.Init();
                }
                else
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000621), StringData.GetStringByIndex(100000614));
                }
            });
        }

        public void onClickDirectPetLayer()
        {
            PopupManager.GetPopup<DragonManagePopup>().CurPetTag = petTag;

            var popup = PopupManager.GetPopup<DragonManagePopup>();
            if (popup != null)
            {
                popup.moveTab(new DragonTabTypePopupData(1, 1));
            }
        }

        /**
         * //업데이트 기대해달라는 문구
         */
        public void onClickExpectGameAlphaUpdate()
        {
            ToastManager.On(100000326);
        }
    }
}
