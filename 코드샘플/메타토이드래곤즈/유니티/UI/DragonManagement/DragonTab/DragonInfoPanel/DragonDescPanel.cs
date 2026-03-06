using Coffee.UIEffects;
using DG.Tweening;
using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonDescPanel : MonoBehaviour
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
        UIGradient dragonBGgradient = null;

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
        GameObject favoriteObject = null;
        [SerializeField]
        GameObject normalObject = null;

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

        [SerializeField]
        Material NormalMat = null;
        [SerializeField]
        Material GrayscaleMat = null;
        public void OnClickBG()
        {
            OnAnimation(eSpineAnimation.NONE);
        }
        public void OnAnimation(eSpineAnimation anim)
        {
            if (dragonSpine == null)
                return;

            if (dragonSpine.Animation != eSpineAnimation.IDLE)
                return;

            if (PopupManager.GetPopup<DragonManagePopup>().CurDragonTag <= 0)//드래곤 태그값
                return;

            var hasDragon = User.Instance.DragonData.IsUserDragon(PopupManager.GetPopup<DragonManagePopup>().CurDragonTag);
            if (!hasDragon)
                return;

            if (anim == eSpineAnimation.NONE)
            {
                switch (SBFunc.Random(0, 7))
                {
                    case 0:
                    case 1:
                    case 2:
                    {
                        anim = eSpineAnimation.ATTACK;
                    }
                    break;
                    case 3:
                    case 4:
                    {
                        anim = eSpineAnimation.SKILL;
                    }
                    break;
                    case 5:
                    {
                        anim = eSpineAnimation.LOSE;
                    }
                    break;
                    case 6:
                    {
                        anim = eSpineAnimation.WIN;
                    }
                    break;
                }
            }

            if (anim == eSpineAnimation.ATTACK)
            {
                anim = eSpineAnimation.A_CASTING;
            }

            if (anim == eSpineAnimation.SKILL)
            {
                anim = eSpineAnimation.CASTING;
            }

            dragonSpine.SetAnimation(anim);
        }

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

                UserDragon userDragon = User.Instance.DragonData.GetDragon(dragonTag);

                RefreshDragonCharacterSlot(dragonTag, userDragon != null ? userDragon.TranscendenceStep : 0);
                RefreshDragonCharacterBG(dragonTag, userDragon != null ? userDragon.TranscendenceStep : 0);
                RefreshDragonDesc(dragonTag);
                RefreshDragonFollowButton(dragonTag);
                RefreshDragonStoryButton(dragonTag);
                RefreshDragonFavoritButton(userDragon);
                RefreshPetCharacterSlot(userDragon);
                RefreshTranscendenceStar(userDragon);
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
                    dragonSpine.SetDragonData(baseData);

                    var mat = dragonSpine.SkeletonAni.material;
                    if (mat != null)
                        mat.SetFloat("_FillPhase", 0f);

                    DOTween.Sequence().AppendInterval(0.1f).AppendCallback(() => {
                        dragonSpine.SetCompleteAniCallback(CompleteAnimation);
                    }).Play();

                    var hasDragon = User.Instance.DragonData.IsUserDragon(_dragonTag);
                    if(hasDragon)
                    {
                        dragonSpine.SkeletonAni.material = NormalMat;
                        dragonSpine.SkeletonAni.timeScale = 1.0f;
                    }
                    else
                    {
                        dragonSpine.SkeletonAni.material = GrayscaleMat;
                        dragonSpine.RandomAnimationFrame();
                        dragonSpine.SkeletonAni.timeScale = 0.0f;                        
                    }
                }
            }

            dragonSpine.SetTranscendEffect(transcendenceStep);
        }

        void CompleteAnimation(UIDragonSpine spineData)
        {
            if (spineData.Animation == eSpineAnimation.A_CASTING)
            {
                dragonSpine.SetAnimation(eSpineAnimation.ATTACK);
                return;
            }

            if (spineData.Animation == eSpineAnimation.CASTING)
            {
                dragonSpine.SetAnimation(eSpineAnimation.SKILL);
                return;
            }

            if (spineData.Animation != eSpineAnimation.IDLE)
            {
                dragonSpine.SetAnimation(eSpineAnimation.IDLE);
            }
        }
        void RefreshDragonCharacterBG(int _dragonTag, int transcendenceStep = 0)
        {
            var designData = GetDragonDesignData(_dragonTag);
            if (designData == null)
                return;

            if (dragonBGTarget != null)
            {
                SBFunc.RemoveAllChildrens(dragonBGTarget.transform);


                var hasDragon = User.Instance.DragonData.IsUserDragon(_dragonTag);
                if (hasDragon)
                {
                    var resourceString = MakeStringByGradeAndElement(designData.GRADE);
                    var icon = ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, resourceString);
                    if (icon == null)
                        return;

                    dragonBGTarget.sprite = icon;
                }
                else
                {
                    var resourceString = MakeStringByGradeAndElement(1);
                    var icon = ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, resourceString);
                    if (icon == null)
                        return;

                    dragonBGTarget.sprite = icon;
                }
            }

            if(dragonBGgradient != null)
            {
                dragonBGgradient.offset = 1.0f - (transcendenceStep / 3.0f);
            }
        }

        void RefreshPetCharacterSlot(UserDragon dragonInfo)
        {
            if (petImageParentNode != null)
                SBFunc.RemoveAllChildrens(petImageParentNode.transform);

            if (dragonInfo == null)
                return;

            var petTag = dragonInfo.Pet;
            if (petTag <= 0)
                return;

            var petInfo = User.Instance.PetData.GetPet(petTag);
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

                        petSpine.transform.localPosition = Vector3.down * 50.0f;
                                                
                        petSpine.transform.DOLocalMove(Vector3.zero, 1.5f).SetEase(Ease.OutBack).OnComplete(()=> {
                            petSpine.transform.DOLocalMoveY(10.0f, 1.5f).SetLoops(-1, LoopType.Yoyo);
                        });                        
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

      

        void RefreshDragonDesc(int _dragonTag)
        {
            RefreshName(_dragonTag);

            var designData = GetDragonDesignData(_dragonTag);
            if(designData != null)
            {
                var grade = designData.GRADE;

                if (frame != null)
                {
                    var hasDragon = User.Instance.DragonData.IsUserDragon(PopupManager.GetPopup<DragonManagePopup>().CurDragonTag);
                    if (hasDragon)
                        frame.SetColor(grade);
                    else
                        frame.SetColor(Color.gray);
                }

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

        void RefreshName(int _dragonTag)
        {
            var hasDragon = User.Instance.DragonData.IsUserDragon(_dragonTag);
            var designData = GetDragonDesignData(_dragonTag);
            if (designData == null)
                return;

            string nameLabel = "";
            string levelLabel = "";
            if (hasDragon)//소유
            {
                var userDragon = User.Instance.DragonData.GetDragon(_dragonTag);

                var levelString = StringData.GetStringByIndex(100000056);
                string levelFormat = string.Format(levelString, userDragon.Level);
                levelFormat = string.Format("<color=#FCCA1E>" + levelFormat + "</color>" + "/{0}", GameConfigTable.GetDragonLevelMax());

                nameLabel = userDragon.Name();
                levelLabel = levelFormat;
            }
            else
            {
                levelLabel = StringData.GetStringByStrKey("미획득드래곤");
                nameLabel = StringData.GetStringByStrKey(designData._NAME);
            }

            if (dragonNameLabel != null && dragonLevelLabel != null)
            {
                dragonNameLabel.text = nameLabel;
                dragonNameLabel.color = hasDragon ? Color.white : new Color(0.674509f, 0.674509f, 0.674509f, 1.0f);
                dragonLevelLabel.text = levelLabel;
            }

        }

        void RefreshDragonFollowButton(int tag)
        {
            if (FollowTownDragonBtn == null) return;

            var hasDragon = User.Instance.DragonData.IsUserDragon(tag);//소유 드래곤이 아니면 꺼버림.
            if(!hasDragon)
            {
                FollowTownDragonBtn.gameObject.SetActive(false);
                return;
            }

            if(Town.Instance == null)
            {
                FollowTownDragonBtn.gameObject.SetActive(false);
                return;
            }

            FollowTownDragonBtn.gameObject.SetActive(true);

            var isDragonTravel = IsDragonTravel(tag);
            var isGemDungeon = IsGemDungeon(tag);
            var isGuild = false;
            if (!GuildManager.Instance.IsNoneGuild)
            {                
                if (int.TryParse(User.Instance.UserData.UserPortrait, out int no))
                {
                    isGuild = no == tag;
                }
            }

            FollowTownDragonBtn.SetButtonSpriteState( !isDragonTravel && !isGemDungeon && !isGuild);

            FollowTownDragonBtn.onClick.RemoveAllListeners();
            FollowTownDragonBtn.onClick.AddListener(() =>
            {
                if (Town.Instance == null)
                    return;

                if (isDragonTravel)
                {
                    ToastManager.On(100002549); // 여행중인 드래곤입니다.
                    return;
                }
                if (isGemDungeon)
                {
                    ToastManager.On(StringData.GetStringByStrKey("젬던전참여중"));
                    return;
                }
                if(isGuild)
                {
                    ToastManager.On(StringData.GetStringByStrKey("길드드래곤참여중"));
                    return;
                }

                Town.Instance.SetSubCamState(false);
                UICanvas.Instance.EndBackgroundBlurEffect();

                PopupManager.AllClosePopup();
                Town.Instance.CamFollowDragon(tag);
            });
        }

        void RefreshDragonStoryButton(int _dragonTag)
        {
            var designData = GetDragonDesignData(_dragonTag);
            if (designData == null)
                return;

            if (storyButton != null)
                storyButton.gameObject.SetActive(designData._DESC != "0");
        }

        void RefreshDragonFavoritButton(UserDragon userDragon)
        {
            if (favoriteObject == null || normalObject == null)
                return;

            favoriteObject.SetActive(false);
            normalObject.SetActive(false);

            if (userDragon != null)
            {
                if(User.Instance.DragonData.IsUserDragon(userDragon.Tag))
                {
                    bool favorite = User.Instance.DragonData.IsFavorite(userDragon.Tag);
                    if(favorite)
                    {
                        favoriteObject.SetActive(true);
                    }
                    else
                    {
                        normalObject.SetActive(true);
                    }
                }
            }
        }

        public void OnFavoriteButton()
        {
            var dragonData = User.Instance.DragonData;
            if (dragonData == null)
            {
                Debug.Log("user's dragon Data is null");
                return;
            }

            var dragonTag = PopupManager.GetPopup<DragonManagePopup>().CurDragonTag;
            dragonData.SetFavorite(dragonTag, !dragonData.IsFavorite(dragonTag));

            RefreshDragonFavoritButton(dragonData.GetDragon(dragonTag));
        }

        bool IsDragonTravel(int _tag)
        {
            var travelData = User.Instance.GetLandmarkData<LandmarkTravel>();
            if (travelData != null)
            {
                if (travelData.TravelState == eTravelState.Travel)
                {
                    var curDragon = User.Instance.DragonData.GetDragon(_tag);
                    if (travelData.TravelDragon.Contains(curDragon))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        bool IsGemDungeon(int _tag)
        {
            return User.Instance.DragonData.GetDragon(_tag).State.HasFlag(eDragonState.GemDungeon);
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
            PopupManager.GetPopup<DragonManagePopup>().CurDragonTag = nextDragonTag;//눌렀을 때 tag값 세팅 및 refresh

            var tapLayerComponent = currentTapLayerNode.GetComponent<SubLayer>();
            if (tapLayerComponent != null)
            {
                tapLayerComponent.ForceUpdate();
            }
        }

        int GetNextDragonIndex(bool isRight)
        {
            //글로벌데이터 선체크
            if (PopupManager.GetPopup<DragonManagePopup>().DragonInfoList.Count == 0)
            {
                return -1;
            }

            var currentDragonTag = GetCurrentDragonTag();
            if (currentDragonTag < 0)
            {
                return -1;
            }
            var currentIndex = PopupManager.GetPopup<DragonManagePopup>().DragonInfoList.FindIndex(element => element == currentDragonTag);
            var modifyIndex = 0;
            //오른쪽 버튼 누르면
            if (isRight)
            {
                modifyIndex = currentIndex + 1;
                if (modifyIndex == PopupManager.GetPopup<DragonManagePopup>().DragonInfoList.Count)
                {
                    modifyIndex = 0;
                }
            }
            else
            {
                modifyIndex = currentIndex - 1;
                if (modifyIndex < 0)
                {
                    modifyIndex = PopupManager.GetPopup<DragonManagePopup>().DragonInfoList.Count - 1;
                }
            }

            return PopupManager.GetPopup<DragonManagePopup>().DragonInfoList[modifyIndex];
        }

        int GetCurrentDragonTag()
        {
            if (PopupManager.GetPopup<DragonManagePopup>().CurDragonTag != 0)//드래곤 태그값
            {
                return PopupManager.GetPopup<DragonManagePopup>().CurDragonTag;
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
    }
}
