using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Coffee.UIEffects;
using Spine.Unity;

namespace SandboxNetwork
{
    public class PickupGachaMenuItem : GachaMenuItem
    {
        [Header("[Pickup Layer]")]
        [SerializeField] SkeletonGraphic spine = null;

        [SerializeField] protected Text charText = null;
        [SerializeField] protected Text charDescText = null;

        [SerializeField] protected Image menuClassIcon = null;
        [SerializeField] protected Text menuClassText = null;

        [SerializeField] protected Image menuSkillIcon = null;
        [SerializeField] protected Text menuSkillNameText = null;
        [SerializeField] protected Text menuSkillCoolTimeText = null;
        [SerializeField] protected Text menuSkillDescText = null;
        public override void InitMenuItem(GachaMenuData menuData, GachaUIController parent)
        {
            if (menuData == null) return;

            CurrentMenuData = menuData;
            parentController = parent;
            
            // 필요 & 보유 아이템 관련 - 여러개에 대한 처리는 되어있지 않음
            IsAvailCondition = CheckGachaCondition();

            // 시간 관련
            IsAvailPeriod = CheckGachaPeriod();

            // 가챠 블락 레이어 처리
            IsAvailGacha = IsAvailCondition && IsAvailPeriod;

            menuTitleText.text = CurrentMenuData.Name;


            CharBaseData charData = CharBaseData.Get(CurrentMenuData.resource);
            if(charData != null)
            {
                spine.gameObject.SetActive(false);
                spine.skeletonDataAsset = charData.GetSkeletonDataAsset();
                var data = spine.skeletonDataAsset.GetSkeletonData(true).FindSkin(charData.SKIN);
                if (data != null)
                {
                    spine.initialSkinName = charData.SKIN;
                    spine.gameObject.SetActive(true);
                    spine.Initialize(true);
                    spine.Skeleton.SetSkin(charData.SKIN);
                    spine.AnimationState.SetAnimation(0, "idle_ani1", true);
                }

                charText.text = StringData.GetStringByStrKey(charData._NAME);
                charDescText.text = StringData.GetStringByStrKey(charData._DESC);
                menuClassIcon.sprite = charData.GetClassIcon();
                string job = "Unknown";
                switch ((int)charData.JOB)
                {
                    case 1:
                        job = "탱커";
                        break;
                    case 2:
                        job = "워리어";
                        break;
                    case 3:
                        job = "어쌔신";
                        break;
                    case 4:
                        job = "캐논";
                        break;
                    case 5:
                        job = "아처";
                        break;
                    case 6:
                        job = "서포터";
                        break;
                }
                menuClassText.text = StringData.GetStringByStrKey(job);

                menuSkillIcon.sprite = charData.SKILL1.GetIcon();
                menuSkillNameText.text = StringData.GetStringByStrKey(charData.SKILL1.NAME);

                string originStrData = StringData.GetStringByStrKey(charData.SKILL1.DESC);
                var summonSkill = SkillSummonData.Get(charData.SKILL1.SUMMON_KEY);

                while (summonSkill != null)
                {
                    if (summonSkill.EFFECT_GROUP_KEY > 0)
                    {
                        var effectSkills = SkillEffectData.GetGroup(summonSkill.EFFECT_GROUP_KEY);
                        if (effectSkills != null)
                        {
                            originStrData = SBFunc.GetConvertStatStr(originStrData, effectSkills, 50);
                        }
                    }

                    summonSkill = SkillSummonData.Get(summonSkill.NEXT_SUMMON);
                }
                menuSkillDescText.text = originStrData;
                menuSkillCoolTimeText.text = string.Format("{0}" + StringData.GetStringByIndex(100000081), charData.SKILL1.COOL_TIME);
            }

            if (eventIcon != null)
            {
                eventIcon.SetActive(menuData.IsEvent());
            }

            RefreshAllLayout();
        }

        public override void SetSelectedState(bool isSelected)
        {
            gameObject.SetActive(isSelected);

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

    }
}
