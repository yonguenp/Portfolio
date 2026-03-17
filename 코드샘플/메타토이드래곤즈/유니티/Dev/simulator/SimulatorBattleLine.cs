using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class SimulatorBattleLine : BattleLine
    {
        protected override int MaxDeckCount => 5;
        protected override int HiddenCount => 0;
        protected override int XSize => 3;
        protected override int YSize => 2;

        //기존 저장된 데이터를 읽을지 말지 (저장장소도 고려할것)
        public override bool LoadBattleLine(int index =0)
        {
            Clear();
            return true;
        }
    }
}
