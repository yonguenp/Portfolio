using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public abstract class DragonData : BattleCharacterData
    {
        public override ICharacterBaseData BaseData
        {
            get
            {
                if (baseData == null)
                    baseData = CharBaseData.Get(ID);

                return baseData;
            }
            set
            {
                baseData = value;
            }
        }
        public override bool IsEnemy { get => false; }
        public override bool IsLeft { get => true; }
        public override bool IsBoss => false;
    }
}