using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class ChampionBattleDragonData : BattleDragonData
    {
        public override bool IsEnemy
        {
            get
            {
                switch ((eChampionBattlePos)Position)
                {
                    case eChampionBattlePos.Defense1Top:
                    case eChampionBattlePos.Defense2Top:
                    case eChampionBattlePos.Defense3Top:
                    case eChampionBattlePos.Defense1Bot:
                    case eChampionBattlePos.Defense2Bot:
                    case eChampionBattlePos.Defense3Bot:
                        return true;
                    case eChampionBattlePos.Offense1Top:
                    case eChampionBattlePos.Offense2Top:
                    case eChampionBattlePos.Offense3Top:
                    case eChampionBattlePos.Offense1Bot:
                    case eChampionBattlePos.Offense2Bot:
                    case eChampionBattlePos.Offense3Bot:
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