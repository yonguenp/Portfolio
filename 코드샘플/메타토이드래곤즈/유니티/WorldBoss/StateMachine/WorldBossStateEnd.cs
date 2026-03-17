using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;

namespace SandboxNetwork
{
    public class WorldBossStateEnd : WorldBossState
    {
        private float slowDelay = 0.2f;
        private bool isSlowEffect = false;
        private bool isAnimEnd = false;
        private float endDelay = 1f;
        private bool IsEnd { get => endDelay > 0f; }
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                slowDelay = 0.2f;
                endDelay = 1f;
                Time.timeScale = 0.2f;
                isSlowEffect = false;
                isAnimEnd = false;

                UIBattleStateEndEvent.Send();
                Clear();

                var anim = eSpineAnimation.WIN;
                switch (Data.State)
                {
                    case eBattleState.Lose:
                    case eBattleState.TimeOver:
                        anim = eSpineAnimation.LOSE;
                        break;
                }

                for (int x = 0, count = Stage.OffenseSpines.Count; x < count; ++x)
                {
                    var list = Stage.OffenseSpines[x];
                    if (list == null)
                        continue;

                    for (int y = 0, yCount = list.Count; y < yCount; ++y)//드래곤 정위치 이동
                    {
                        var curDragon = list[y];
                        if (curDragon == null || curDragon.Data.Death)
                            continue;

                        curDragon.ClearBuffStat();
                        curDragon.Data.ClearTypeInfo(eSkillEffectType.AIRBORNE);
                        curDragon.Data.ClearTypeInfo(eSkillEffectType.STUN);
                        curDragon.Data.ClearTypeInfo(eSkillEffectType.AGGRO);
                        curDragon.Data.SetActionCoroutine(null);
                        curDragon.SetDefaultSpeed(1f);
                        curDragon.SetAnimation(anim);

                        if (Data.State != eBattleState.Lose)
                        {
                            ProCamera2D.Instance.AddCameraTarget(curDragon.transform);
                        }
                    }
                }

                if (Data.State != eBattleState.Win)
                {
                    for (int i = 0, count = Stage.DefenseSpines.Count; i < count; ++i)
                    {
                        var it = Stage.DefenseSpines[i];
                        if (it == null)
                            continue;
                        for (int j = 0, jCount = it.Count; j < jCount; ++j)
                        {
                            var character = it[j];
                            if (character == null)
                                continue;

                            character.ClearBuffStat();
                            character.Data.ClearTypeInfo(eSkillEffectType.AIRBORNE);
                            character.Data.ClearTypeInfo(eSkillEffectType.STUN);
                            character.Data.ClearTypeInfo(eSkillEffectType.AGGRO);
                            ProCamera2D.Instance.AddCameraTarget(character.transform);
                        }
                    }
                }

                return true;
            }
            return false;
        }
        public override bool OnResume()
        {
            if (base.OnResume()) //복구
            {
                return true;
            }
            return false;
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt)) //종료 UI, 연출 등
            {
                slowDelay -= dt;
                if (slowDelay < 0f)
                {
                    if (!isSlowEffect)
                    {
                        Time.timeScale = 1f;
                        isSlowEffect = true;
                    }

                    if (!isAnimEnd)
                    {
                        isAnimEnd = true;
                    }

                    endDelay += slowDelay;
                    slowDelay = 0f;
                }
                return IsEnd;
            }
            return !IsPlaying;
        }
    }
}