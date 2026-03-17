using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SandboxNetwork
{
    public class AchievementConditionInfo : CollectionAchievementConditionInfo
    {
        AchievementBaseData tableData = null;

        public AchievementConditionInfo(AchievementBaseData _tableData) : base()
        {
            tableData = _tableData;
            completeValue = _tableData.NUM;
        }
    }

    public class AchievementBase : CollectionAchievement
    {
        public AchievementBaseData achievementBaseData { get; private set; }
        public AchievementBase(AchievementBaseData _achieve)
        {
            if (_achieve == null)
                return;

            SetData(_achieve);
        }

        public override void SetData(ICollectionAchievementBaseData _tableData)
        {
            base.SetData(_tableData);
            achievementBaseData = (AchievementBaseData)_tableData;
            condition = new AchievementConditionInfo(achievementBaseData);            
        }
    }
}
