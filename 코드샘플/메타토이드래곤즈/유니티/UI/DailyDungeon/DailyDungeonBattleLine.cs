using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork { 

    public class DailyDungeonBattleLine : BattleLine
    {
        protected override int MaxDeckCount => 5;
        protected override int HiddenCount => 0;
        protected override int XSize => 3;
        protected override int YSize => 2;
        public override bool LoadBattleLine(int index = 0)
        {
            int currentWorldIndex = StageManager.Instance.DailyDungeonProgressData.TodayWorldIndex[0];
            int formationIndex = currentWorldIndex - (int)eDailyDungeonWorldIndex.Mon;
            DailyDungeonFormationData dailyData = User.Instance.PrefData.DailyDungeonFormationData;
            if(dailyData == null)
                return false;

            return SetLine(dailyData.TeamFormation[formationIndex]);
        }
    }
}