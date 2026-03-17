using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class BattleDragonData : DragonData
    {
        public override eElementType Element
        {
            get
            {
                return BaseData.ELEMENT_TYPE;
            }
        }
        public override float Size { get { return 1f; } }


        public override void SetData(int pos, JToken token)
        {
            if (token == null)
                return;

            Alive = true;
            Position = pos;
            ID = token.Value<int>();

            var userDragon = User.Instance.DragonData.GetDragon(ID);
            if (userDragon != null)
            {
                Level = userDragon.Level;
                SkillLevel = userDragon.SLevel;
                PetID = userDragon.Pet;
                Stat = userDragon.Status.Clone();
                TranscendenceData = userDragon.TranscendenceData;
                MaxHP = Stat.GetTotalStatusInt(eStatusType.HP);
                HP = MaxHP;

                if (Skill1 != null)
                    SetSkill1Delay(Skill1.START_COOL_TIME - (Skill1.START_COOL_TIME * Stat.GetTotalStatusConvert(eStatusType.DEL_START_COOLTIME)));
            }

            if (Infos == null)
                Infos = new();
            if (PriorityTargets == null)
                PriorityTargets = new();
        }
    }

    public class WorldBossBattleDragonData : BattleDragonData
    {
        public int PartyIndex { get; private set; } = -1;
        public eDirectionBit PartyDirection { get; private set; } = eDirectionBit.None;
        public override eDirectionBit KnockBackDirection 
        { 
            get { 
                switch(PartyDirection)
                {
                    case eDirectionBit.Right:
                        return eDirectionBit.Left;
                    case eDirectionBit.Left:
                        return eDirectionBit.Right;
                    default:
                        return eDirectionBit.None;
                }
            } 
        }
        public override bool IsLeft
        {
            get
            {
                return (PartyDirection == eDirectionBit.Left);
            }
        }

        public void SetData(int party, int pos, JToken token)
        {
            PartyIndex = party;
            SetData(pos, token);
        }
        public override void SetData(int pos, JToken token)
        {
            base.SetData(pos, token);

            eDirectionBit dir = ((pos - 1) / 6) % 2 == 0 ? eDirectionBit.Right : eDirectionBit.Left;
            PartyDirection = dir;
        }
    }
}