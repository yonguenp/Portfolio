using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
	public class AchievementBaseData : CollectionAchievementBaseData<DBAchievements_info>
	{
		public override string GetTableName()
		{
			return "achievements_info";
		}

		static private AchievementBaseTable table = null;
		static public AchievementBaseData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<AchievementBaseTable>();

			return table.Get(key);
		}
		static public AchievementBaseData Get(int key)
		{
			if (table == null)
				table = TableManager.GetTable<AchievementBaseTable>();

			return table.Get(key);
		}
		static public int GetAchievementTotalCount()
		{
			if (table == null)
				table = TableManager.GetTable<AchievementBaseTable>();

			return table.GetAchievementTotalCount();
		}

		public override eStatusType REWARD_STAT_TYPE => SBFunc.ConvertStatusType(Data.REWARD_STAT_TYPE);
		public override eStatusValueType REWARD_STAT_VALUE_TYPE => (eStatusValueType)Data.VALUE_TYPE;
		public override float REWARD_STAT_VALUE => Data.REWARD_STAT_VALUE;
		public int GROUP => Data.GROUP;//업적 레어도
		public int TYPE => Data.TYPE;//업적 타입
		public int GRADE => Data.GRADE;//업적 레어도
		public int ELEMENT => Data.ELEMENT;//업적 달성 조건 속성 설정
		public int VALUE => Data.VALUE;//업적에 필요한 값
		public int NUM => Data.NUM;//업적 달성 횟수
	}
}
