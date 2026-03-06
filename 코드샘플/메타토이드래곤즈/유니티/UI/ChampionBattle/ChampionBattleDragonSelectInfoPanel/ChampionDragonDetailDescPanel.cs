using DG.Tweening;
using Google.Impl;
using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionDragonDetailDescPanel : MonoBehaviour
    {
        [SerializeField]
        RectTransform layoutTarget = null;

        [SerializeField]
        GameObject currentTapLayerNode = null;

        [SerializeField]
        UIDragonSpine dragonSpine = null;

        [SerializeField]
        GameObject petImageParentNode = null;

        [SerializeField]
        Image dragonBGTarget = null;

        [SerializeField]
        Text dragonNameLabel = null;
        [SerializeField]
        Text dragonLevelLabel = null;

        [SerializeField]
        SlotFrameController frame = null;

        [SerializeField]
        Image dragonElementSprite = null;
        [SerializeField]
        Image dragonGrade = null;
        [SerializeField]
        Sprite[] dragonGradeList = null;
        [SerializeField]
        Image dragonClass = null;

        [SerializeField]
        Button FollowTownDragonBtn = null;

        [SerializeField]
        Button storyButton = null;

        [SerializeField]
        Vector3 characterScale = new Vector3(4, 4, 4);

        [SerializeField]
        Vector3 characterPos = new Vector3(0, -125, 0);

        [SerializeField]
        Vector3 petScale = new Vector3(0.5f, 0.5f, 0.5f);

        [SerializeField]
        Vector3 petPos = new Vector3(0, 0, 0);

        [SerializeField]
        GameObject StarParent = null;

        [SerializeField]
        GameObject[] EmptyStars = null;

        [SerializeField]
        GameObject[] Stars = null;

        ChampionDragonDetailPopup ParentPopup { get { return PopupManager.GetPopup<ChampionDragonDetailPopup>(); } }

        public void OnClickBG()
        {
            if (dragonSpine == null)
                return;

            if (dragonSpine.Animation != eSpineAnimation.IDLE)
                return;

            switch (SBFunc.Random(0, 7))
            {
                case 0:
                case 1:
                case 2:
                {
                    dragonSpine.SetAnimation(eSpineAnimation.ATTACK);
                }
                break;
                case 3:
                case 4:
                {
                    dragonSpine.SetAnimation(eSpineAnimation.SKILL);
                }
                break;
                case 5:
                {
                    dragonSpine.SetAnimation(eSpineAnimation.LOSE);
                }
                break;
                case 6:
                {
                    dragonSpine.SetAnimation(eSpineAnimation.WIN);
                }
                break;
            }
        }

        public void Init()
        {
            RefreshCurrentDragonData();
        }

        void RefreshCurrentDragonData()
        {
            if (ParentPopup.DragonTag > 0)//드래곤 태그값
            {
                var dragon = ParentPopup.Dragon;
                RefreshDragonCharacterSlot(dragon.Tag, dragon != null ? dragon.TranscendenceStep : 0);
                RefreshDragonCharacterBG(dragon.Tag);
                RefreshDragonDesc(dragon);
                RefreshDragonFollowButton();
                RefreshDragonStoryButton(dragon.Tag);
                RefreshPetCharacterSlot(dragon);
                RefreshTranscendenceStar(dragon);
            }
        }
        void RefreshDragonCharacterSlot(int _dragonTag, int transcendenceStep = 0)
        {
            if (dragonSpine == null)
                return;
            
            var dragonImageName = GetDragonImageName(_dragonTag);
            if (dragonImageName != "")
            {
                var baseData = CharBaseData.Get(_dragonTag);
                if (baseData != null)
                    dragonSpine.SkeletonAni.skeletonDataAsset = baseData.GetSkeletonDataAsset();

                var data = dragonSpine.SkeletonAni.skeletonDataAsset.GetSkeletonData(true).FindSkin(baseData.SKIN);
                if (data != null)
                {
                    dragonSpine.SkeletonAni.initialSkinName = baseData.SKIN;
                    dragonSpine.SkeletonAni.Initialize(true);
                    dragonSpine.SkeletonAni.Skeleton.SetSkin(baseData.SKIN);
                    dragonSpine.SetAnimation(eSpineAnimation.IDLE);

                    var mat = dragonSpine.SkeletonAni.material;
                    if (mat != null)
                        mat.SetFloat("_FillPhase", 0f);

                    DOTween.Sequence().AppendInterval(0.1f).AppendCallback(() => {
                        dragonSpine.SetCompleteAniCallback(CompleteAnimation);
                    }).Play();
                }
            }

            dragonSpine.SetTranscendEffect(transcendenceStep);
        }

        void CompleteAnimation(UIDragonSpine spineData)
        {
            if (spineData.Animation != eSpineAnimation.IDLE)
            {
                dragonSpine.SetAnimation(eSpineAnimation.IDLE);
            }
        }
        void RefreshDragonCharacterBG(int _dragonTag)
        {
            var designData = GetDragonDesignData(_dragonTag);
            if (designData == null)
                return;

            if (dragonBGTarget != null)
            {
                SBFunc.RemoveAllChildrens(dragonBGTarget.transform);

                var resourceString = MakeStringByGradeAndElement(designData.GRADE);
                var icon = ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, resourceString);
                if (icon == null)
                    return;

                dragonBGTarget.sprite = icon;
            }
        }

        void RefreshPetCharacterSlot(ChampionDragon _dragon)
        {
            if (petImageParentNode != null)
                SBFunc.RemoveAllChildrens(petImageParentNode.transform);

            if (_dragon == null)
                return;

            var petTag = _dragon.Pet;
            if (petTag <= 0)
                return;

            var petInfo = _dragon.ChampionPet;
            if (petInfo == null)
                return;

            if (petImageParentNode == null)
                return;

            var petImageName = GetPetImageName(petInfo.ID);
            if (petImageName != "")
            {
                var baseData = petInfo.GetPetDesignData();
                var petPrefab = SBFunc.GetPetSpineByName(petImageName, eSpineType.UI);
                if (petPrefab != null)
                {
                    var clone = Instantiate(petPrefab, petImageParentNode.transform);
                    clone.GetComponent<RectTransform>().anchoredPosition = petPos;
                    clone.GetComponent<RectTransform>().localScale = petScale;
                    var petSpine = clone.GetComponent<UIPetSpine>();
                    if (petSpine != null)
                    {
                        petSpine.Init();
                        if (baseData != null && baseData.SKIN != "NONE")
                            petSpine.SetSkin(baseData.SKIN);
                    }
                }
            }
        }

        void RefreshTranscendenceStar(UserDragon dragonInfo)
        {
            var isUserDragon = dragonInfo != null;

            if (StarParent == null)
                return;
            if(isUserDragon == false)
            {
                StarParent.gameObject.SetActive(false);
                return;
            }
            if (dragonInfo.IsTranscendenceAble())
            {
                int curStar = dragonInfo.TranscendenceStep;
                int curMaxStar = CharTranscendenceData.GetStepMax((eDragonGrade)dragonInfo.Grade());
                StarParent.gameObject.SetActive(true);
                for (int i = 0, count = Stars.Length; i < count; ++i)
                {
                    if (Stars[i] != null)
                        Stars[i].SetActive(i < curStar);
                }
                for (int i = 0, count = EmptyStars.Length; i < count; ++i)
                {
                    if (EmptyStars[i] != null)
                        EmptyStars[i].SetActive(i < curStar);
                    }
            }
            else
            {
                 StarParent.gameObject.SetActive(false);
            }
        }

      

        void RefreshDragonDesc(ChampionDragon _dragon)
        {
            RefreshName(_dragon);

            var designData = GetDragonDesignData(_dragon.Tag);
            if(designData != null)
            {
                var grade = designData.GRADE;

                if (frame != null)
                    frame.SetColor(grade);

                if (dragonElementSprite != null)
                    dragonElementSprite.sprite = GetElementIconSpriteByIndex(designData.ELEMENT);

                if (dragonClass != null)
                    dragonClass.sprite = designData.GetClassIcon();

                var gradeModifyIndex = grade - 1;
                if (dragonGradeList != null && dragonGrade != null && gradeModifyIndex >= 0 && dragonGradeList.Length > gradeModifyIndex)
                    dragonGrade.sprite = dragonGradeList[gradeModifyIndex];
            }

            if (layoutTarget != null)
                RefreshContentFitter(layoutTarget);
        }
        protected void RefreshContentFitter(RectTransform transform)
        {
            if (transform == null || !transform.gameObject.activeSelf)
            {
                return;
            }

            foreach (RectTransform child in transform)
            {
                RefreshContentFitter(child);
            }

            var layoutGroup = transform.GetComponent<LayoutGroup>();
            var contentSizeFitter = transform.GetComponent<ContentSizeFitter>();
            if (layoutGroup != null)
            {
                layoutGroup.SetLayoutHorizontal();
                layoutGroup.SetLayoutVertical();
            }

            if (contentSizeFitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
            }
        }

        void RefreshName(ChampionDragon _dragon)
        {
            var designData = GetDragonDesignData(_dragon.Tag);
            if (designData == null)
                return;

            string nameLabel = "";
            string levelLabel = "";

            var levelString = StringData.GetStringByIndex(100000056);
            string levelFormat = string.Format(levelString, _dragon.Level);
            levelFormat = string.Format("<color=#FCCA1E>" + levelFormat + "</color>" + "/{0}", GameConfigTable.GetDragonLevelMax());

            nameLabel = _dragon.Name();
            levelLabel = levelFormat;

            if (dragonNameLabel != null)
            {
                dragonNameLabel.text = nameLabel;
            }

            if(dragonLevelLabel != null)
            {
                dragonLevelLabel.text = levelLabel;
            }

        }

        void RefreshDragonFollowButton()
        {
            if (FollowTownDragonBtn == null) return;

            if(Town.Instance == null)
            {
                FollowTownDragonBtn.gameObject.SetActive(false);
                return;
            }

            FollowTownDragonBtn.gameObject.SetActive(false);
        }

        void RefreshDragonStoryButton(int _dragonTag)
        {
            var designData = GetDragonDesignData(_dragonTag);
            if (designData == null)
                return;

            if (storyButton != null)
                storyButton.gameObject.SetActive(designData._DESC != "0");
        }
                
        CharBaseData GetDragonDesignData(int _tag)
        {
            var dragonData = CharBaseData.Get(_tag.ToString());
            if (dragonData != null)
                return dragonData;
            else
                return null;
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

        string GetDragonImageName(int dragonTag)
        {
            var dragonData = CharBaseData.Get(dragonTag.ToString());
            if (dragonData != null)
            {
                return dragonData.IMAGE;
            }
            return "";
        }

        public void OnClickLeftCharcterButton()
        {
            var nextdragonTag = GetNextDragonIndex(false);
            if (nextdragonTag > 0)
            {
                ResetAndRefreshDragonTagData(nextdragonTag);
            }
        }
        public void OnClickRightCharcterButton()
        {
            var nextdragonTag = GetNextDragonIndex(true);
            if (nextdragonTag > 0)
            {
                ResetAndRefreshDragonTagData(nextdragonTag);
            }
        }

        void ResetAndRefreshDragonTagData(int nextDragonTag)
        {
            //ParentPopup.SetData(ChampionManager.Instance.MyInfo.ChampionDragons.GetDragon(nextDragonTag) as ChampionDragon);//눌렀을 때 tag값 세팅 및 refresh

            //var tapLayerComponent = currentTapLayerNode.GetComponent<ChampionDragonDetailLayer>();
            //if (tapLayerComponent != null)
            //{
            //    tapLayerComponent.ForceUpdate();
            //}
            ParentPopup.SetData(ParentPopup.Dragons, ParentPopup.Dragons.GetDragon(nextDragonTag) as ChampionDragon);
        }

        int GetNextDragonIndex(bool isRight)
        {
            //글로벌데이터 선체크
            if (ParentPopup.DragonInfoList.Count == 0)
            {
                return -1;
            }

            var currentDragonTag = GetCurrentDragonTag();
            if (currentDragonTag < 0)
            {
                return -1;
            }
            var currentIndex = ParentPopup.DragonInfoList.FindIndex(element => element == currentDragonTag);
            var modifyIndex = 0;
            //오른쪽 버튼 누르면
            if (isRight)
            {
                modifyIndex = currentIndex + 1;
                if (modifyIndex == ParentPopup.DragonInfoList.Count)
                {
                    modifyIndex = 0;
                }
            }
            else
            {
                modifyIndex = currentIndex - 1;
                if (modifyIndex < 0)
                {
                    modifyIndex = ParentPopup.DragonInfoList.Count - 1;
                }
            }

            return ParentPopup.DragonInfoList[modifyIndex];
        }

        int GetCurrentDragonTag()
        {
            if (ParentPopup.DragonTag > 0)//드래곤 태그값
            {
                return ParentPopup.DragonTag;
            }
            return -1;
        }

        string GetGradeConvertString(int grade)
        {
            var gradeNameStrIndex = CharGradeData.Get(grade.ToString())._NAME;
            var gradeString = StringData.GetStringByIndex(gradeNameStrIndex).ToLower();
            return gradeString;
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
        Sprite GetElementIconSpriteByIndex(int e_type)
        {
            var elementStr = GetElementConvertString(e_type);
            return ResourceManager.GetResource<Sprite>(eResourcePath.ElementIconPath, SBFunc.StrBuilder("type_", elementStr));
        }

        string MakeStringByGradeAndElement(int grade)
        {
            var gradeString = GetGradeConvertString(grade);
            return SBFunc.StrBuilder("bggrade_", gradeString);
        }

        public void HideDescPanel()
        {
            gameObject.SetActive(false);
        }
        public void ShowDescPanel()
        {
            gameObject.SetActive(true);
        }
    }
}
