using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class CharTranscendenceData : TableData<DBChar_transcendence>
    {
        static private CharTranscendenceTable table = null;
        static public CharTranscendenceData Get(int key)
        {
            if (table == null)
                table = TableManager.GetTable<CharTranscendenceTable>();

            return table.Get(key);
        }
        static public CharTranscendenceData Get(eDragonGrade targetGrade, int step)
        {
            if (table == null)
                table = TableManager.GetTable<CharTranscendenceTable>();

            return table.Get(targetGrade, step);
        }
        static public int GetStepMax(eDragonGrade targetGrade)
        {
            if (table == null)
                table = TableManager.GetTable<CharTranscendenceTable>();

            return table.GetStepMax(targetGrade);
        }

        static public List<CharTranscendenceData> GetByGrade(eDragonGrade targetGrade)
        {
            if (table == null)
                table = TableManager.GetTable<CharTranscendenceTable>();
            return table.GetByGrade(targetGrade);
        }

        static public int GetNewSkillSlotMinimumStep(eDragonGrade targetGrade)
        {
            if (table == null)
                table = TableManager.GetTable<CharTranscendenceTable>();
            return table.GetNewSkillSlotMinimumStep(targetGrade);
        }

        static public int GetMaxSkillSlot(eDragonGrade targetGrade)
        {
            if (table == null)
                table = TableManager.GetTable<CharTranscendenceTable>();
            int maxStep = table.GetStepMax(targetGrade);
            return table.Get(targetGrade, maxStep).SKILL_SLOT_MAX;
        }

        public string KEY => Data.UNIQUE_KEY;
        public eDragonGrade TARGET_GRADE => SBFunc.GetDragonGrade(Data.TARGET_GRADE);
        public int STEP => Data.STEP;
        public int MATERIAL_GRADE_RATE_1 => Data.MATERIAL_GRADE_RATE_1;
        public int MATERIAL_GRADE_RATE_2 => Data.MATERIAL_GRADE_RATE_2;
        public int MATERIAL_GRADE_RATE_3 => Data.MATERIAL_GRADE_RATE_3;
        public int MATERIAL_GRADE_RATE_4 => Data.MATERIAL_GRADE_RATE_4;
        public int MATERIAL_GRADE_RATE_5 => Data.MATERIAL_GRADE_RATE_5;
        public int MATERIAL_MAX => Data.MATERIAL_MAX;
        public int SKILL_SLOT_MAX => Data.SKILL_SLOT_MAX;
        public int ADD_STAT => Data.ADD_STAT;
    }
}
