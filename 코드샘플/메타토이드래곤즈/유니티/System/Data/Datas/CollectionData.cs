using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
	public interface ICollectionAchievementBaseData
	{
		public int KEY { get; }
		public eStatusType REWARD_STAT_TYPE { get; }
		public eStatusValueType REWARD_STAT_VALUE_TYPE { get; }
		public float REWARD_STAT_VALUE { get; }
		public string GetTableName();
		public string _NAME { get; }
		public string TOAST { get; }
	}
	public abstract class CollectionAchievementBaseData<T> : TableData<T>, ICollectionAchievementBaseData where T : DBData, new()
    {
		public int KEY => Int(Data.UNIQUE_KEY);
		public abstract eStatusType REWARD_STAT_TYPE { get; }
		public abstract eStatusValueType REWARD_STAT_VALUE_TYPE { get; }
		public abstract float REWARD_STAT_VALUE { get; }
		public virtual string GetTableName()
        {
			return "";
        }
		public virtual string _NAME
		{
			get
			{
				return SBFunc.StrBuilder(GetTableName() + ":name:" + KEY);
			}
		}
		public virtual string TOAST
		{
			get
			{
				return StringData.GetStringByStrKey(GetTableName() + ":complete:" + KEY);
			}
		}
	}

	public class CollectionData : CollectionAchievementBaseData<DBCollection_info>
	{
		public override eStatusType REWARD_STAT_TYPE => SBFunc.ConvertStatusType(Data.REWARD_STAT_TYPE);
		public override eStatusValueType REWARD_STAT_VALUE_TYPE => (eStatusValueType)Data.VALUE_TYPE;
		public override float REWARD_STAT_VALUE => Data.REWARD_STAT_VALUE;
		public override string GetTableName()
		{
			return "collection_info";
		}

		static private CollectionTable table = null;
		static public CollectionData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<CollectionTable>();

			return table.Get(key);
		}
		static public CollectionData Get(int key)
		{
			if (table == null)
				table = TableManager.GetTable<CollectionTable>();

			return table.Get(key);
		}
		static public int GetCollectionTotalCount()
		{
			if (table == null)
				table = TableManager.GetTable<CollectionTable>();

			return table.GetCollectionTotalCount();
		}

		public List<int> CollectionIDList 
		{ 
			get
            {
				var groupTable = TableManager.GetTable<CollectionGroupTable>();
				if (groupTable != null)
				{
					return groupTable.GetIDListByGroup(KEY);
				}
				return new();
			} 
		}

		public string collection_name { get; private set; } = string.Empty;
		public override string _NAME
		{
			get
			{
				return collection_name;
			}
		}

		string GetDragonName(int no)
        {
			CharBaseData charData = CharBaseData.Get(no);
			if (charData != null)
			{
				return (StringData.GetStringByStrKey(charData._NAME));
			}
			else
			{
				return StringData.GetStringByStrKey("Unkown");
			}
		}
		public override string TOAST
		{
			get
			{
				string valType = "VALUE#";
				if(REWARD_STAT_VALUE_TYPE == eStatusValueType.PERCENT)
					valType = "PERCENT#";
				 
				return StringData.GetStringFormatByStrKey(string.Join("/", new string[]{ "@BUFF", Data.REWARD_STAT_TYPE, valType}), REWARD_STAT_VALUE);
			}
		}

		public override void SetData(DBCollection_info data)
		{
			base.SetData(data);

			string name = GetDragonName(CollectionIDList[0]);
			string value = StringData.GetStringByStrKey("collection:" + Data.REWARD_STAT_TYPE + ":" + Data.VALUE_TYPE);

			string count = string.Empty;
			switch (CollectionIDList.Count)
			{
				case 1:					
					count = StringData.GetStringByStrKey("결정");
					break;
				case 2:
					count = StringData.GetStringByStrKey("조각");
					break;
				case 3:
					count = StringData.GetStringByStrKey("파편");
					break;
				default:
					count = StringData.GetStringByStrKey("가루");
					break;
			}

			collection_name = StringData.GetStringFormatByStrKey("콜렉션이름", name, value, count);
		}

		public void SetNameTag(int tag)
        {
			collection_name += " - " + (char)('A' + tag); 
        }
	}

	public class CollectionGroupData : TableData<DBCollection_group>
	{
		static private CollectionGroupTable table = null;
		static public CollectionGroupData Get(string key)
		{
			if (table == null)
				table = TableManager.GetTable<CollectionGroupTable>();

			return table.Get(key);
		}

		static public List<int> GetGroupListByGroup(int group)
        {
			if (table == null)
				table = TableManager.GetTable<CollectionGroupTable>();

			return table.GetIDListByGroup(group);
		}

		public string KEY => Data.UNIQUE_KEY;
		public int GROUP_ID => Data.GROUP_ID;
		public int DRAGON_KEY => Data.DRAGON_KEY;
	}
}
