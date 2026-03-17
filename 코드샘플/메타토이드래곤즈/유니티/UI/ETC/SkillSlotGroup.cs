using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SkillSlotGroup : MonoBehaviour, EventListener<AdventureStateEvent>
    {
        [SerializeField]
        private List<SkillSlot> slots = null;
        [SerializeField]
        private ToolTip skillInfo = null;
        private GameObject curInfoTarget = null;

        [SerializeField]
        private bool isBot = true;
        [SerializeField]
        private bool useTooltip = true;
        private void Start()
        {
            EventManager.AddListener(this);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener(this);
        }

        public void SetContentUIType(eUIType type)
        {
            gameObject.SetActive(!type.HasFlag(eUIType.Battle_WorldBoss));
        }
        public void SetSlot(IBattleData battleData, List<IBattleCharacterData> data)
        {
            int dataCount = 0;
            if (data != null)
                dataCount = data.Count;

            for (int i = 0, count = slots.Count; i < count; ++i)
            {
                if (slots[i] == null)
                    continue;

                bool isActive = dataCount > 0 && i < dataCount;
                slots[i].gameObject.SetActive(isActive);

                if (!isActive)
                    continue;

                slots[i].SetData(battleData, data[i]);
                slots[i].InitUI();
            }
        }

        public SkillSlot GetSlot(int dTag)
        {
            if (slots == null)
                return null;

            return slots.Find((cur) =>
            {
                if (cur == null)
                    return false;
                return cur.Data.ID == dTag;
            });
        }
        //public SkillSlot GetSlot(object bTag)
        //{
        //    if (slots == null)
        //        return null;

        //    return slots.Find((cur) =>
        //    {
        //        if (cur == null)
        //            return false;
        //        return cur.Data.Position == bTag;
        //    });
        //}

        public void OnEvent(AdventureStateEvent eventType)
        {
            for (int i = 0, count = slots.Count; i < count; ++i)
            {
                if (slots[i] == null)
                    continue;

                slots[i].Pause = eventType.Pause;
            }
        }

        public void OnSkillInfo(SkillCharData skillData, int skillLv, GameObject target)
        {
            if (!useTooltip)
                return;

            if (skillData == null)
                return;

            var skillName = StringData.GetStringByStrKey(skillData.NAME);
            var skillDescName = StringData.GetStringByStrKey(skillData.DESC);
            if (target != null)
            {
                var summonSkill = SkillSummonData.Get(skillData.SUMMON_KEY);
                while (summonSkill != null)
                {
                    if (summonSkill.EFFECT_GROUP_KEY > 0)
                    {
                        var effectSkills = SkillEffectData.GetGroup(summonSkill.EFFECT_GROUP_KEY);
                        if (effectSkills != null)
                        {
                            skillDescName = SBFunc.GetConvertStatStr(skillDescName, effectSkills, skillLv);
                        }
                    }

                    summonSkill = SkillSummonData.Get(summonSkill.NEXT_SUMMON);
                }
                var popupData = new TooltipPopupData(new ToolTipData(skillName, skillDescName, target, false, !isBot));
                PopupManager.OpenPopup<ToolTip>(popupData);
            }
            curInfoTarget = target;
        }

        public void ClearSkillInfo(GameObject target)
        {
            if(curInfoTarget == target)
                skillInfo.SetActive(false);
        }

        public bool IsSkillToolTipOn(GameObject target)
        {
            return curInfoTarget == target && skillInfo.gameObject.activeInHierarchy;
        }
    }
}