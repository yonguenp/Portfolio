using SBSocketSharedLib;
using UnityEngine;

public class CreatureObject : BaseObject
{
    HpBar _hpBar;

    public override StatInfo Stat
    {
        get { return base.Stat; }
        protected set
        {
            base.Stat.Hp = value.Hp;
            base.Stat.MaxHp = value.MaxHp;
            base.Stat.MoveSpeed = value.MoveSpeed;
            base.Stat.QuestSpeed = value.QuestSpeed;
            base.Stat.Attack = value.Attack;
            base.Stat.ReduceAttackTime = value.ReduceAttackTime;
            base.Stat.ReduceSkillTime = value.ReduceSkillTime;
            base.Stat.AttackDist = value.AttackDist;

            UpdateHpBar();
        }
    }

    protected void AddHpBar()
    {
        GameObject go = Managers.Resource.Instantiate("UI/HpBar", transform);
        go.transform.localPosition = new Vector3(0, 0.5f, 0);
        go.name = "HpBar";
        _hpBar = go.GetComponent<HpBar>();
        UpdateHpBar();
    }

    void UpdateHpBar()
    {
        if (_hpBar == null)
            return;

        float ratio = 0.0f;
        if (Stat.MaxHp > 0)
            ratio = ((float)Hp) / Stat.MaxHp;

        _hpBar.SetHpBar(ratio);
    }

    public override void Init()
    {
        base.Init();
        AddHpBar();
    }

    public virtual void OnDamaged()
    {

    }

    public virtual void OnDead()
    {
        SetState(CreatureStatus.Dead);

        GameObject effect = Managers.Resource.Instantiate("Particle/DieEffect");
        effect.transform.position = transform.position;
        effect.GetComponent<Animator>().Play("START");
        GameObject.Destroy(effect, 0.5f);
    }
}
