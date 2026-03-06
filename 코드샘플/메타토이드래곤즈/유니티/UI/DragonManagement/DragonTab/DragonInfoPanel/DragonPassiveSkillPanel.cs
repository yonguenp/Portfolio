using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class DragonPassiveSkillPanel : DragonManageSubPanel
    {
        [Header("skill 1")]
        [SerializeField] GameObject skill1Node;
        [SerializeField] GameObject skill1SkillLayer;
        [SerializeField] GameObject skill1LockLayer;
        [SerializeField] Text skill1Title;
        [SerializeField] Text skill1Text;
        [SerializeField] Text skill1LockText;

        [Header("skill 2")]
        [SerializeField] GameObject skill2Node;
        [SerializeField] GameObject skill2SkillLayer;
        [SerializeField] GameObject skill2LockLayer;
        [SerializeField] Text skill2Title;
        [SerializeField] Text skill2Text;
        [SerializeField] Text skill2LockText;

        [Space(10)]
        [SerializeField] Button skillGetBtn;

        UserDragon dragonData = null;
        int currentSlot = 0;
        int minSkillGetLv = 0;
        int skillSlotMax = 0;
        public override void Init()
        {
            base.Init();
            skillSlotMax = 0;
            dragonData = User.Instance.DragonData.GetDragon(dragonTag);
            var transcendenceDatas = CharTranscendenceData.GetByGrade((eDragonGrade)dragonData.Grade());
            currentSlot = dragonData.PassiveSkillSlot; // 뚫린 스킬 슬롯
            var curSkills = dragonData.PassiveSkills; // 현재 스킬들
            SetLockState();
            skill1Title.text = StringData.GetStringByStrKey("스킬1");
            skill2Title.text = StringData.GetStringByStrKey("스킬2");
            var maxSkillSlot = CharTranscendenceData.GetMaxSkillSlot((eDragonGrade)dragonData.Grade());
            skill2Node.SetActive(maxSkillSlot>1);
            foreach (var transcendenceData in transcendenceDatas)
            {
                skillSlotMax = Mathf.Max(skillSlotMax, transcendenceData.SKILL_SLOT_MAX);
                if (transcendenceData.SKILL_SLOT_MAX == 1)
                {
                    if (currentSlot < 1)
                    {
                        skill1LockText.text = StringData.GetStringFormatByStrKey("스킬오픈조건", transcendenceData.STEP);
                    }
                    else
                    {
                        skill1SkillLayer.SetActive(true);
                        skill1LockLayer.SetActive(false);
                        if (curSkills.Count < 1)
                        {
                            skill1Text.text = StringData.GetStringByStrKey("스킬미획득");
                            skill1Text.alignment = TextAnchor.MiddleCenter;
                        }
                        else
                        {
                            skill1Text.text = SkillPassiveData.Get(curSkills[0]).STRING;
                            skill1Text.alignment = TextAnchor.MiddleLeft;
                        }
                        
                        skill1Title.text = curSkills.Count < 1 ? StringData.GetStringByStrKey("스킬1") : SkillPassiveRateData.GetSkillGroupName(curSkills[0]);
                    }
                    minSkillGetLv = transcendenceData.STEP;
                }
                else if (transcendenceData.SKILL_SLOT_MAX == 2)
                {
                    if (currentSlot < 2)
                    {
                        skill2LockText.text = StringData.GetStringFormatByStrKey("스킬오픈조건", transcendenceData.STEP);
                    }
                    else
                    {
                        skill2SkillLayer.SetActive(true);
                        skill2LockLayer.SetActive(false);
                        if(curSkills.Count < 2)
                        {
                            skill2Text.text = StringData.GetStringByStrKey("스킬미획득");
                            skill2Text.alignment = TextAnchor.MiddleCenter;
                        }
                        else
                        {
                            skill2Text.text = SkillPassiveData.Get(curSkills[1]).STRING;
                            skill2Text.alignment = TextAnchor.MiddleLeft;
                        } 
                        skill2Title.text = curSkills.Count < 2 ? StringData.GetStringByStrKey("스킬2") : SkillPassiveRateData.GetSkillGroupName(curSkills[1]);
                    }
                }
            }
            skillGetBtn.SetButtonSpriteState(currentSlot > 0);
        }

        void SetLockState()
        {
            skill1SkillLayer.SetActive(false);
            skill2SkillLayer.SetActive(false);
            skill1LockLayer.SetActive(true);
            skill2LockLayer.SetActive(true);
            skill1LockText.text = StringData.GetStringByStrKey("스킬획득불가");
            skill2LockText.text = StringData.GetStringByStrKey("스킬획득불가");
        }

        public void OnClickGetPassiveSkill()
        {
            if (currentSlot > 0)
            {
                var popup = PopupManager.OpenPopup<PassiveSkillPopup>(new PassiveSkillPopupData(dragonTag, skillSlotMax));
                popup.SetSkillGetCallBack(
                    () =>
                    {
                        Init();
                        successCallback();
                    });
            }
            else
                ToastManager.On(StringData.GetStringFormatByStrKey("스킬슬롯잠김", minSkillGetLv));
        }
    }
}