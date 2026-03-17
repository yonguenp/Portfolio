using UnityEngine;

namespace SandboxNetwork
{
    public class ChampionBattleColosseumEnd : ChampionBattleColosseumState
    {
        private float endDelay = 1.5f;
        public override bool OnEnter()
        {
            if (base.OnEnter()) //일시 정지
            {
                endDelay = 1.5f;
                return true;
            }
            return false;
        }
        public override bool OnPause()
        {
            if (base.OnPause()) //일시 정지
            {
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
                endDelay -= dt;

                var isExit = endDelay > 0f;
                if (!isExit)
                    Time.timeScale = 1f;
                return isExit;
            }
            return false;
        }
    }

    public class ChampionPracticeColosseumEnd : ChampionBattleColosseumEnd
    { 
    }
}