using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

namespace SandboxNetwork
{
    [System.Serializable]
    public class BuffIconInfo
    {
        [SerializeField]
        eStatusType statType = eStatusType.NONE; 
        [SerializeField]
        eSkillEffectType buffType = eSkillEffectType.NONE;
        [SerializeField]
        Sprite buffSprite = null;

        public eStatusType StatType { get => statType; }
        public eSkillEffectType BuffType { get => buffType; }
        public Sprite BuffSprite { get => buffSprite; }
    }
    public class buffSlotFrame : MonoBehaviour
    {
        [SerializeField]
        Image iconTarget;
        [SerializeField]
        Image frame;
        [SerializeField]
        Sprite defaultSprite;
        [SerializeField]
        Image buffDelay = null;
        [SerializeField]
        Text nestText = null;
        [SerializeField]
        BuffIconInfo[] buffSpriteArr;
        [SerializeField]
        Sprite[] bgArr;
        private static Dictionary<eSkillEffectType, Dictionary<eStatusType, Sprite>> buffSpriteDic = null;
        private SkillEffectData targetEffect = null;
        private SkillPassiveData targetPassive = null;
        private EffectInfo targetInfo = null;

        // Start is called before the first frame update
        public void SetIcon(SkillEffectData skill, EffectInfo info)
        {
            if (buffSpriteArr == null || skill == null)
                return;

            if (buffSpriteDic == null)
                CreateDic();

            targetEffect = skill;
            targetInfo = info;
            Sprite icon = null;
            switch (targetEffect.TYPE)
            {
                case eSkillEffectType.ENV_BUFF:
                case eSkillEffectType.BUFF_MAIN_ELEMENT:
                {
                    var type = eSkillEffectType.BUFF;
                    if (buffSpriteDic.ContainsKey(type))
                    {
                        if (buffSpriteDic[type].ContainsKey(targetInfo.STAT_TYPE))
                        {
                            icon = buffSpriteDic[type][targetInfo.STAT_TYPE];
                        }
                    }
                }
                break;
                case eSkillEffectType.DEBUFF:
                case eSkillEffectType.BUFF:
                {
                    var type = targetEffect.TYPE;
                    if (buffSpriteDic.ContainsKey(type))
                    {
                        if(buffSpriteDic[type].ContainsKey(targetInfo.STAT_TYPE))
                        {
                            icon = buffSpriteDic[type][targetInfo.STAT_TYPE];
                        }
                    }
                } break;
                case eSkillEffectType.IMN_CC:
                case eSkillEffectType.IMMUNE_DMG:
                case eSkillEffectType.IMMUNE_HARM:
                case eSkillEffectType.SILENCE:
                case eSkillEffectType.STUN:
                case eSkillEffectType.FROZEN:
                case eSkillEffectType.AGGRO_R:
                {
                    if (buffSpriteDic.ContainsKey(targetEffect.TYPE))
                    {
                        if (buffSpriteDic[targetEffect.TYPE].ContainsKey(eStatusType.NONE))
                        {
                            icon = buffSpriteDic[targetEffect.TYPE][eStatusType.NONE];
                        }
                    }
                } break;
                default:
                    break;
            }

            if (icon == null)
                icon = defaultSprite;

            iconTarget.sprite = icon;
            frame.sprite = bgArr[GetIndexBGFrame(targetEffect.TYPE)];

            StopCoroutine("Refresh");
            StartCoroutine("Refresh");
        }
        public void SetIcon(SkillPassiveData passive, EffectInfo info)
        {
            if (buffSpriteArr == null || passive == null)
                return;

            if (buffSpriteDic == null)
                CreateDic();

            targetPassive = passive;
            targetInfo = info;
            Sprite icon = null;
            switch (targetPassive.PASSIVE_EFFECT)
            {
                case eSkillPassiveEffect.BUFF:
                case eSkillPassiveEffect.BUFF_MAIN_ELEMENT:
                {
                    if (buffSpriteDic.ContainsKey(eSkillEffectType.BUFF))
                    {
                        if (buffSpriteDic[eSkillEffectType.BUFF].ContainsKey(targetInfo.STAT_TYPE))
                        {
                            icon = buffSpriteDic[eSkillEffectType.BUFF][targetInfo.STAT_TYPE];
                        }
                    }
                } break;
                case eSkillPassiveEffect.DEBUFF:
                {
                    if (buffSpriteDic.ContainsKey(eSkillEffectType.DEBUFF))
                    {
                        if (buffSpriteDic[eSkillEffectType.DEBUFF].ContainsKey(targetInfo.STAT_TYPE))
                        {
                            icon = buffSpriteDic[eSkillEffectType.DEBUFF][targetInfo.STAT_TYPE];
                        }
                    }
                } break;
                case eSkillPassiveEffect.SILENCE:
                {
                    if (buffSpriteDic.ContainsKey(eSkillEffectType.SILENCE))
                    {
                        if (buffSpriteDic[eSkillEffectType.SILENCE].ContainsKey(eStatusType.NONE))
                        {
                            icon = buffSpriteDic[eSkillEffectType.SILENCE][eStatusType.NONE];
                        }
                    }
                } break;
                default:
                    break;
            }

            if (icon == null)
                icon = defaultSprite;

            iconTarget.sprite = icon;
            frame.sprite = bgArr[GetIndexBGFrame(targetPassive.PASSIVE_EFFECT)];

            StopCoroutine("Refresh");
            StartCoroutine("Refresh");
        }
        int GetIndexBGFrame(eSkillEffectType index)
        {
            switch (index)
            {
                case eSkillEffectType.ENV_BUFF:
                case eSkillEffectType.BUFF:
                case eSkillEffectType.SHIELD:
                case eSkillEffectType.BUFF_MAIN_ELEMENT:
                    return 0;
                case eSkillEffectType.DEBUFF:
                case eSkillEffectType.AGGRO_R:
                    return 1;
                case eSkillEffectType.STUN:
                case eSkillEffectType.FROZEN:
                case eSkillEffectType.AIRBORNE:
                case eSkillEffectType.SILENCE:
                case eSkillEffectType.TICK_DMG:
                case eSkillEffectType.POISON:
                case eSkillEffectType.KNOCK_BACK:
                case eSkillEffectType.AGGRO:
                    return 2;
                case eSkillEffectType.IMN_CC:
                case eSkillEffectType.IMMUNE_DMG:
                case eSkillEffectType.IMMUNE_HARM:
                    return 3;
                default:
                    return 0;
            }
        }
        int GetIndexBGFrame(eSkillPassiveEffect index)
        {
            switch (index)
            {
                case eSkillPassiveEffect.BUFF:
                case eSkillPassiveEffect.BUFF_MAIN_ELEMENT:
                    return 0;
                case eSkillPassiveEffect.DEBUFF:
                    return 1;
                case eSkillPassiveEffect.SILENCE:
                    return 2;
                default:
                    return 0;
            }
        }
        private void CreateDic()
        {
            if (buffSpriteArr == null)
                return;

            buffSpriteDic = new();

            for(int i = 0, count = buffSpriteArr.Length; i < count; ++i)
            {
                if (buffSpriteArr[i] == null)
                    continue;

                var bType = buffSpriteArr[i].BuffType;
                if (!buffSpriteDic.ContainsKey(bType))
                    buffSpriteDic.Add(bType, new());

                buffSpriteDic[bType].Add(buffSpriteArr[i].StatType, buffSpriteArr[i].BuffSprite);
            }
        }

        IEnumerator Refresh()
        {
            if (buffDelay == null || targetInfo == null)
                yield break;

            nestText.gameObject.SetActive(false);

            while (targetInfo.Time > 0f && targetInfo.MaxTime > 0f)
            {
                buffDelay.fillAmount = 1f - targetInfo.Time / targetInfo.MaxTime;
                if (buffDelay.fillAmount <= 0f)
                    buffDelay.fillAmount = 0f;

                var isNest = targetInfo.NestCount > 1;
                nestText.gameObject.SetActive(isNest);
                if (isNest)
                    nestText.text = targetInfo.NestCount.ToString();

                yield return null;
            }

            nestText.gameObject.SetActive(false);
            buffDelay.fillAmount = 1f;
            yield break;
        }
    }
}
