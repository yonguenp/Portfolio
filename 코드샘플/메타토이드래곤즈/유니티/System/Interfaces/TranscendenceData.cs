using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class TranscendenceData
    {
        public int Step { get; private set; } = 0;
        public int PassiveSlot { get; private set; } = 0;
        public List<int> PassiveSkills { get; private set; } = new();
        private Dictionary<int, SkillPassiveData> PassiveSkillDic { get; set; } = new();
        public void SetData(JObject jsonData)
        {
            if (SBFunc.IsJTokenType(jsonData["transcendence_step"], JTokenType.Integer))
            {
                Step = jsonData["transcendence_step"].Value<int>();
            }
            if (SBFunc.IsJTokenType(jsonData["passive_skill_slot"], JTokenType.Integer))
            {
                PassiveSlot = jsonData["passive_skill_slot"].Value<int>();
            }
            if (jsonData["passive_skill"] != null && jsonData["passive_skill"].Type == JTokenType.Array)
            {
                SetPassiveData(jsonData["passive_skill"].ToObject<List<int>>());
            }
        }

        public void SetPassiveData(List<int> skills)
        {
            PassiveSkills = skills;
            SetPassiveDic();
        }

        public void SetArenaData(JObject jsonData)
        {
            if (SBFunc.IsJTokenType(jsonData["t_step"], JTokenType.Integer))
            {
                Step = jsonData["t_step"].Value<int>();
            }
            if (jsonData["p_skill"] != null && jsonData["p_skill"].Type == JTokenType.Array)
            {
                PassiveSkills = jsonData["p_skill"].ToObject<List<int>>();
                PassiveSlot = PassiveSkills.Count;
                SetPassiveDic();
            }
        }
        /// <summary>
        /// 깡통 초월 드래곤이 필요하다면 사용
        /// </summary>
        /// <param name="transcendendceStep">초월 단계</param>
        /// <param name="passiveSkillSlot">패시브 슬롯</param>
        /// <param name="passiveSkills">패시브 스킬번호</param>
        public void SetData(int transcendendceStep, int passiveSkillSlot = 0, List<int> passiveSkills = null)
        {
            Step = transcendendceStep;
            PassiveSlot = passiveSkillSlot;
            if (passiveSkills == null)
                PassiveSkills = new();
            else
                PassiveSkills = passiveSkills;

            SetPassiveDic();
        }
        private void SetPassiveDic()
        {
            PassiveSkillDic.Clear();
            for (int i = 0, count = PassiveSlot; i < count; ++i)
            {
                if (PassiveSkills.Count <= i)
                    break;
                var skill = SkillPassiveData.Get(PassiveSkills[i]);
                if (skill == null)
                    continue;

                PassiveSkillDic.Add(i + 1, skill);
            }
        }
        public SkillPassiveData GetPassiveData(int index)
        {
            if (PassiveSkillDic != null && PassiveSkillDic.TryGetValue(index, out var value))
                return value;

            return null;
        }
    }
}