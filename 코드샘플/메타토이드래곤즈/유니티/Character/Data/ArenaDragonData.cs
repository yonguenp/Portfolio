using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class ArenaDragonData : BattleDragonData
    {
        public override bool IsEnemy
        {
            get
            {
                switch ((eArenaPos)Position)
                {
                    case eArenaPos.Defense1Top:
                    case eArenaPos.Defense2Top:
                    case eArenaPos.Defense3Top:
                    case eArenaPos.Defense1Bot:
                    case eArenaPos.Defense2Bot:
                    case eArenaPos.Defense3Bot:
                        return true;
                    case eArenaPos.Offense1Top:
                    case eArenaPos.Offense2Top:
                    case eArenaPos.Offense3Top:
                    case eArenaPos.Offense1Bot:
                    case eArenaPos.Offense2Bot:
                    case eArenaPos.Offense3Bot:
                    default:
                        return false;
                }
            }
        }
        public override void SetData(int pos, JToken token)
        {
            if (token == null)
                return;

            var data = token.ToObject<JObject>();
            if (data == null)
                return;

            Alive = true;
            Position = pos;

            if (data.ContainsKey("id") && data["id"].Type == JTokenType.Integer)
            {
                ID = data["id"].Value<int>();
            }

            if (data.ContainsKey("lvl") && data["lvl"].Type == JTokenType.Integer)
            {
                Level = data["lvl"].Value<int>();
            }

            if (data.ContainsKey("slvl") && data["slvl"].Type == JTokenType.Integer)
            {
                SkillLevel = data["slvl"].Value<int>();
            }

            if (data.ContainsKey("petId") && data["petId"].Type == JTokenType.Integer)
            {
                PetID = data["petId"].Value<int>();
            }
            TranscendenceData.SetArenaData(data);

            if (SBFunc.IsJArray(data["stat"]))
            {
                if (Stat == null)
                {
                    Stat = new();
                    Stat.Initialze();
                }
                Stat.SetStatus(true);

                eStatusType type_;
                var statArray = (JArray)data["stat"];
                for (int i = 0, count = statArray.Count; i < count; ++i)
                {
                    type_ = (eStatusType)i;
                    float value = 0f;
                    if (SBFunc.IsJTokenType(statArray[i], JTokenType.Integer))
                        value = statArray[i].Value<int>();
                    else if (SBFunc.IsJTokenType(statArray[i], JTokenType.Float))
                        value = Mathf.RoundToInt(statArray[i].Value<float>());

                    if (Stat.GetType(type_) == eStatusValueType.PERCENT)
                        value *= SBDefine.CONVERT_FLOAT;

                    if (float.IsInfinity(value))
                        value = 0f;

                    Stat.IncreaseStatus(eStatusCategory.BASE, type_, value);
                }
                Stat.CalcStatusAll();

                if (Skill1 != null)
                    SetSkill1Delay(Skill1.START_COOL_TIME - (Skill1.START_COOL_TIME * Stat.GetTotalStatusConvert(eStatusType.DEL_START_COOLTIME)));

                if (Infos == null)
                    Infos = new();

                HP = Stat.GetTotalStatusInt(eStatusType.HP);
                MaxHP = HP;
            }

            if (Infos == null)
                Infos = new();
            if (PriorityTargets == null)
                PriorityTargets = new();
        }
    }
}