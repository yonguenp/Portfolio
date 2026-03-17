using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonSkillDescPanel : DragonManageSubPanel
    {
        [SerializeField]
        Text skillName = null;
        [SerializeField]
        Text skillDesc = null;
        [SerializeField]
        Text skillCoolTime = null;

        [SerializeField]
        GameObject lockObject = null;

        [SerializeField]
        DragonDescPanel descPanel = null;

        SkillCharData myskilldata = null;
        public override void ShowPanel(VoidDelegate _successCallback = null)
        {
            base.ShowPanel(_successCallback);            
        }

        public override void HidePanel()
        {
            base.HidePanel();
        }

        public override void Init()
        {
            base.Init();
            SetLabel();
        }

        void SetLabel()
        {
            if (dragonBase == null)
                return;

            if (descPanel != null)
                descPanel.OnAnimation(eSpineAnimation.SKILL);

            int skillLevel = 1;
            var hasDragon = User.Instance.DragonData.IsUserDragon(dragonTag);
            if(hasDragon)
            {
                var currentDragonData = User.Instance.DragonData.GetDragon(dragonTag);
                if (currentDragonData != null)
                    skillLevel = currentDragonData.SLevel;
            }
            else
                skillLevel = GameConfigTable.GetSkillLevelMax();

            lockObject.gameObject.SetActive(false);

            myskilldata = dragonBase.SKILL1;
            if(myskilldata != null)
            {
                skillDesc.text = StringData.GetStringByStrKey(myskilldata.DESC);
                if (skillName != null && skillDesc != null && skillCoolTime != null)
                {
                    skillName.text = StringData.GetStringByStrKey(myskilldata.NAME);

                    string originStrData = StringData.GetStringByStrKey(myskilldata.DESC);
                    var summonSkill = SkillSummonData.Get(myskilldata.SUMMON_KEY);
                    
                    while (summonSkill != null)
                    {
                        if (summonSkill.EFFECT_GROUP_KEY > 0)
                        {
                            var effectSkills = SkillEffectData.GetGroup(summonSkill.EFFECT_GROUP_KEY);
                            if (effectSkills != null)
                            {
                                originStrData = GetConvertStatStr(originStrData, effectSkills, skillLevel);
                            }
                        }

                        summonSkill = SkillSummonData.Get(summonSkill.NEXT_SUMMON);
                    }
                    skillDesc.text = originStrData;
                    skillCoolTime.text = string.Format("{0}" + StringData.GetStringByIndex(100000081), myskilldata.COOL_TIME);
                }
            }
            else//스킬 없는 드래곤은 null
            {
                lockObject.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// "@BUFF/ATK/PERCENT#" //@로 시작해서 # 으로 끝나는, 기본 split 구분자 '/' 타입 + 스탯타입 + VALUE 타입의 조합값.
        /// </summary>
        /// <param name="_originDesc"></param>
        /// <param name="_list"></param>
        /// <returns></returns>

        string GetConvertStatStr(string _originDesc , List<SkillEffectData> _list, int _currentSlevel)
        {
            return SBFunc.GetConvertStatStr(_originDesc, _list, _currentSlevel);
        }
    }
}
