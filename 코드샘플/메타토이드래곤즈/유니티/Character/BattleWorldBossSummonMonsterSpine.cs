using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleWorldBossSummonMonsterSpine : BattleMonsterSpine
{
    [SerializeField]
    private bool showAnim = true;
    [SerializeField]
    private bool formChange = false;

    private string IdleAnimation = "";
    private string AttackAnimation = "";

    public override void InitializeTypeFunc()
    {
        GetTypeToName = GetMonsterAnimTypeToName;
        GetTypeToLoop = SBFunc.IsTypeToLoop;
        GetNameToType = GetMonsterAnimNameToType;
        GetTypeToSkip = SBFunc.IsAnimSkip;
    }
    protected override void InitAnimation()
    {
        //SetSkin(Data.BaseData.SKIN);
        Animation = eSpineAnimation.NONE;

        IdleAnimation = "idle";
        AttackAnimation = "attack";

        if (formChange)
        {
            IdleAnimation = "idle_b";
            AttackAnimation = "attack_b";
            formChangeCheck();
        }

        SetAnimation(0, showAnim ? "show" : IdleAnimation, false);
    }
    public override Spine.TrackEntry SetAnimation(eSpineAnimation anim)
    {
        formChangeCheck();

        return base.SetAnimation(anim);
    }

    protected override void CompleteHandleAnimation(Spine.TrackEntry trackEntry)
    {
        if (Animation == eSpineAnimation.DEATH || Data.Death)
        {
            switch (trackEntry.Animation.Name)
            {
                case "monster_death":
                    isDeath = true;
                    break;
            }
            return;
        }

        switch (GetMonsterAnimNameToType(trackEntry.Animation.Name))
        {
            case eSpineAnimation.ATTACK:
                Animation = eSpineAnimation.NONE;
                SetAnimation(eSpineAnimation.IDLE);
                break;
            case eSpineAnimation.CASTING:
                Animation = eSpineAnimation.NONE;
                SetAnimation(eSpineAnimation.SKILL);
                break;
            case eSpineAnimation.SKILL:
                Animation = eSpineAnimation.NONE;
                SetAnimation(eSpineAnimation.IDLE);
                break;
            case eSpineAnimation.HIT:
                Animation = eSpineAnimation.NONE;
                SetAnimation(eSpineAnimation.IDLE);
                break;
            case eSpineAnimation.WALK:
                Animation = eSpineAnimation.NONE;
                SetAnimation(eSpineAnimation.IDLE);
                break;
        }        
    }

    public string GetMonsterAnimTypeToName(eSpineAnimation eAnim)
    {
        return eAnim switch
        {
            eSpineAnimation.A_CASTING => "",
            eSpineAnimation.ATTACK => AttackAnimation,
            eSpineAnimation.SKILL => AttackAnimation,
            eSpineAnimation.WALK => IdleAnimation,
            eSpineAnimation.IDLE => IdleAnimation,
            eSpineAnimation.WIN => "",
            eSpineAnimation.LOSE => "",
            eSpineAnimation.CASTING => AttackAnimation,
            eSpineAnimation.DEATH => "death",
            eSpineAnimation.HIT => "",
            _ => ""
        };
    }

    public eSpineAnimation GetMonsterAnimNameToType(string strAnim)
    {
        return strAnim switch
        {
            "monster_attack" => eSpineAnimation.ATTACK,
            "monster_skill1" => eSpineAnimation.SKILL,
            "monster_walk" => eSpineAnimation.WALK,
            "monster_idle" => eSpineAnimation.IDLE,
            "monster_win" => eSpineAnimation.WIN,
            "monster_lose" => eSpineAnimation.LOSE,
            "monster_casting" => eSpineAnimation.CASTING,
            "monster_death" => eSpineAnimation.DEATH,
            "monster_hit" => eSpineAnimation.HIT,
            "attack" => eSpineAnimation.ATTACK,
            "attack_t" => eSpineAnimation.ATTACK,
            "attack_b" => eSpineAnimation.ATTACK,
            "idle" => eSpineAnimation.IDLE,
            "idle_b" => eSpineAnimation.IDLE,
            "idle_t" => eSpineAnimation.IDLE,
            "death" => eSpineAnimation.DEATH,
            _ => eSpineAnimation.NONE
        };
    }


    public void formChangeCheck()
    {
        if (formChange)
        {
            bool change = true;

            if (change)
            {
                if (SkeletonAni.transform.localScale.x > 0)
                {
                    int startIndex = eWorldBoss.POS_BOTTOM_LEFT * WorldBossFormationData.MAX_DRAGON_COUNT;
                    for (int i = startIndex; i < startIndex + WorldBossFormationData.MAX_DRAGON_COUNT; i++)
                    {
                        int index = i + 1;
                        if (!WorldBossManager.Instance.Data.OffenseDic.ContainsKey(index))
                        {
                            continue;
                        }
                        var target = WorldBossManager.Instance.Data.OffenseDic[index];
                        if (target != null && !target.Death)
                        {
                            change = false;
                            break;
                        }
                    }
                }
            }

            if (change)
            {
                if (SkeletonAni.transform.localScale.x < 0)
                {
                    int startIndex = eWorldBoss.POS_BOTTOM_RIGHT * WorldBossFormationData.MAX_DRAGON_COUNT;
                    for (int i = startIndex; i < startIndex + WorldBossFormationData.MAX_DRAGON_COUNT; i++)
                    {
                        int index = i + 1;
                        if (!WorldBossManager.Instance.Data.OffenseDic.ContainsKey(index))
                        {
                            continue;
                        }
                        var target = WorldBossManager.Instance.Data.OffenseDic[index];
                        if (target != null && !target.Death)
                        {
                            change = false;
                            break;
                        }
                    }
                }
            }

            if (!change)
                return;

            IdleAnimation = IdleAnimation.Replace("_b", "_t");
            AttackAnimation = AttackAnimation.Replace("_b", "_t");

            formChange = false;
        }
    }

    public override void SetDamage(int value, IBattleCharacterData caster)
    {
        if (Data == null || Data.Death)
            return;

        base.SetDamage(value, caster);
    }

    public override void Death()
    {
        SetShadow(false);
        isDeath = true;
        ClearEffectSpine();
        gameObject.SetActive(false);
    }
}
