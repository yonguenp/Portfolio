using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork 
{
    public class LandmarkGemDungeonDragon
    {
        public LandmarkGemDungeonDragon()
        {
        }
        public UserDragon Dragon { get; private set; } = null;
        public int DragonNo => Dragon == null ? 0 : Dragon.Tag;
        public bool IsFloor => Dragon == null ? false : Dragon.State.HasFlag(eDragonState.GemDungeon);
        private int floor = 0;
        public int Floor => IsFloor ? floor : 0;
        public int TimeStamp => Dragon == null ? 0 : Dragon.FatigueTimeStamp;
        public int Fatigue => Dragon == null ? 0 : GameConfigTable.GetConfigIntValue("CHAR_STAMINA_MAX", SBDefine.DefaultFatigue) - Dragon.Fatigue;

        private readonly int fatigueUseStandard = 216;
        private readonly int fatigueRecoveryStandard = 432;

        public int ExpectedFatigue { get {
                if(Floor == 0)
                {
                    int FatigueRecoveryTime = GameConfigTable.GetConfigIntValue("STAMINA_RECOVERY_TIME", fatigueRecoveryStandard);
                    int elapsedTime = TimeManager.GetTimeCompareFromNow(TimeStamp);
                    return Fatigue + elapsedTime / FatigueRecoveryTime;
                }
                else
                {
                    if (Fatigue == 0)
                        return Fatigue;
                    int FatigueDownTime = GameConfigTable.GetConfigIntValue("STAMINA_USEUP_TIME", fatigueUseStandard);
                    int elapsedTime = TimeManager.GetTimeCompareFromNow(TimeStamp);
                    return Fatigue - elapsedTime / FatigueDownTime;
                }
            }
        }
        public void SetFloor(int floor)
        {
            this.floor = floor;
        }
        public void SetData(int floor, UserDragon dragon)
        {
            SetFloor(floor);
            if (null == dragon)
                return;

            Dragon = dragon;
        }
    }
}
