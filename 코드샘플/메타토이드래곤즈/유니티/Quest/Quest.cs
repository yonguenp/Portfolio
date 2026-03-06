using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace SandboxNetwork
{
	public struct QuestDescStruct
	{
		public string stringID;//triggerData의 _Noti 칼럼 값
		public string subType;//triggerData 의 기본 subType 값
		public string subTypeStringIndex;//stringIndex로 찾아야 하는 subType(ex. 건설 건물 desc index)
		public string typeKey;//triggerData 의 기본 type_key 값
		public string typeKeyStringIndex;//stringIndex로 찾아야 하는 typeKey(ex. 생산 아이템 name Index)

		public void InitData()
		{
			stringID = "";
			typeKey = "";
			typeKeyStringIndex = "";
			subType = "";
			subTypeStringIndex = "";
		}

		public void SetData(string _stringID, string _typeKey, string _typeKeyStringIndex, string _subtype, string _subTypeStringIndex)
		{
			stringID = _stringID;
			typeKey = _typeKey;
			typeKeyStringIndex = _typeKeyStringIndex;
			subType = _subtype;
			subTypeStringIndex = _subTypeStringIndex;
		}
	}
	public class QuestConditionData
	{
		private QuestTriggerData triggerData;
		public QuestTriggerData TriggerData { get { return triggerData; } }
		private int completeValue;
		public int CompleteValue { get { return completeValue; } }
		private int currentValue;
		public int CurrentValue { get { return currentValue; } }
		private QuestDescStruct descData;

		public QuestConditionData(QuestTriggerData _triggerData, int _completeValue)
        {
			triggerData = _triggerData;
			completeValue = _completeValue;
			currentValue = 0;
			descData = new QuestDescStruct();
			descData.InitData();
			SetQuestDesc();//desc 미리 세팅
		}

		public void UpdateCondition(int _currentValue)
        {
			if (_currentValue < completeValue)
				currentValue = _currentValue;
			else
				currentValue = completeValue;
		}
		/*
		 * {0} : SUB_TYPE
		 * {1} : TYPE_KEY
		 * {2} : TYPE_KEY_VALUE //총 체크값 == completeValue
		 * {3} : 현재 진행 횟수
		 */
		private void SetQuestDesc()//quest_base 의 noti 인덱스, typeKey와 subkey구분 체크
		{
			currentValue = Mathf.Min(currentValue, completeValue);//최소값 세팅을 하는 이유..?

			string completeType = triggerData.TYPE;
			string typeKey = triggerData.TYPE_KEY;
			string subKey = triggerData.SUB_TYPE;
			string subKeyString = "";
			string typeKeyString = "";
			string _notieIndex = triggerData._NOTIE;

			if (subKey == "")//sub_key 는 NONE이거나 무조건 string 데이터가 있어야함
				return;

			switch ((eQuestCompleteCondType)QuestCondition.strCondition.IndexOf(completeType))
			{
				case eQuestCompleteCondType.BUILD://{1}레벨 {0} 건설하기 {3}/{2}
					if (subKey.CompareTo("subway_slot") == 0 || subKey.CompareTo("SUBWAY_SLOT") == 0)
					{
						break;
					}
					var buildingData = BuildingBaseData.Get(subKey.ToLower());
					if(buildingData != null)
						subKeyString = buildingData._NAME;
                    else
						Debug.Log("subkey Error buildingData is Null" + subKey);

					if(int.TryParse(typeKey , out int result))
                    {
						if (result > 1)
							_notieIndex += "_u";
                    }

					break;
				case eQuestCompleteCondType.GAIN://제작한 {1} 획득하기 {3}/{2}
				case eQuestCompleteCondType.PRODUCE:
					if (subKey.CompareTo(QuestManager.QuestSubTypeParam) == 0)
					{
						break;
					}
					var itemData = ItemBaseData.Get(int.Parse(typeKey));
					if(itemData != null)
						typeKeyString = itemData.NAME_KEY;
					break;
				case eQuestCompleteCondType.STAGE_COMPLETE:
				case eQuestCompleteCondType.STAGE_CLEAR:
					if (subKey.CompareTo(QuestManager.QuestSubTypeParam) == 0)
					{
						break;
					}
					StageBaseData stageData = StageBaseData.Get(typeKey);
					if(stageData != null)
						typeKey = SBFunc.StrBuilder(stageData.WORLD, "-", stageData.STAGE);
					break;
				case eQuestCompleteCondType.CHECK_DRAGON://{0}등급 드래곤 {2}마리 보유하기 {3}/{2}
				case eQuestCompleteCondType.CHECK_PET://{0}등급 펫 {2}마리 보유하기 {3}/{2}
				case eQuestCompleteCondType.CHECK_EQUIP://{0}등급 장비 {2}개 보유하기 {3}/{2}
				case eQuestCompleteCondType.TUTORIAL:
				case eQuestCompleteCondType.BUILD_START:
				case eQuestCompleteCondType.GAIN_DOZER:
				case eQuestCompleteCondType.LEVEL_ACCOUNT:
				case eQuestCompleteCondType.LEVEL_DRAGON:
				case eQuestCompleteCondType.LEVEL_PET:
				case eQuestCompleteCondType.LEVEL_DRAGON_SKILL:
				case eQuestCompleteCondType.LEVEL_TOWN:
				case eQuestCompleteCondType.ENHANCE_PET:
				case eQuestCompleteCondType.ENHANCE_EQUIP:
				case eQuestCompleteCondType.MERGE_DRAGON:
				case eQuestCompleteCondType.MERGE_PET:
				case eQuestCompleteCondType.MERGE_EQUIP:
				case eQuestCompleteCondType.TRAVEL:
				case eQuestCompleteCondType.DELIVERY:
				case eQuestCompleteCondType.REQUEST:
				case eQuestCompleteCondType.ARENA:
				case eQuestCompleteCondType.DAY_DUNGEON:
				case eQuestCompleteCondType.GACHA_DRAGON:
				case eQuestCompleteCondType.GACHA_PET:
				case eQuestCompleteCondType.CONSUME_GOLD:
				case eQuestCompleteCondType.CONSUME_ENERGY:
				case eQuestCompleteCondType.EQUIPMENT_EQUIP:
				case eQuestCompleteCondType.EQUIPMENT_PET:
				case eQuestCompleteCondType.ADD_FLOOR:
				case eQuestCompleteCondType.START:
				case eQuestCompleteCondType.EQUIP_DECOM:
				case eQuestCompleteCondType.CLEAR_QUEST:
				case eQuestCompleteCondType.CLEAR_TYPE:
				case eQuestCompleteCondType.DAILY_ALL_CLEAR_AD:
				case eQuestCompleteCondType.ARENA_WIN_AD:
				case eQuestCompleteCondType.GAIN_BATTERY:
				case eQuestCompleteCondType.ATTENDANCE:
				case eQuestCompleteCondType.GACHA_LUCKY:
				case eQuestCompleteCondType.GAIN_GEMDUNGEON:
				case eQuestCompleteCondType.BOSS_RAID:
				case eQuestCompleteCondType.CHECK_TARGET_DRAGON:
				case eQuestCompleteCondType.LEVEL_TARGET_DRAGON:
				case eQuestCompleteCondType.LEVEL_TARGET_DRAGON_SKILL:
				case eQuestCompleteCondType.CHECK_TRANSCENDENCE_TARGET_DRAGON:
					break;
				default:
					Debug.Log(string.Format("해당 퀘스트 타입 찾을 수 없음 : {0} - {1}", completeType, triggerData.KEY));
					break;
			}

			descData.SetData(_notieIndex, typeKey, typeKeyString, subKey, subKeyString);
		}

		string ReplaceStringWithParameter(bool isFullDesc = true)
		{
			string stringID = descData.stringID;
			string typeKey = descData.typeKey;
			string typeKeyStringIndex = descData.typeKeyStringIndex;
			string subTypeKey = descData.subType;
			string subtypeStringIndex = descData.subTypeStringIndex;

			var replaceSubType = !string.IsNullOrEmpty(subtypeStringIndex) ? StringData.GetStringByStrKey(subtypeStringIndex) : subTypeKey;
			var replaceKeyType = !string.IsNullOrEmpty(typeKeyStringIndex) ? StringData.GetStringByStrKey(typeKeyStringIndex) : typeKey;

			var originStrData = "";
			if (int.TryParse(stringID, out int parseStringID))
				originStrData = StringData.GetStringByIndex(parseStringID);
			else
				originStrData = StringData.GetStringByStrKey(stringID);

			if (isFullDesc)
				return originStrData.Replace("{0}", replaceSubType).Replace("{1}", replaceKeyType).Replace("{2}", completeValue.ToString()).Replace("{3}", currentValue.ToString());
            else
				return originStrData.Replace("{0}", replaceSubType).Replace("{1}", replaceKeyType).Replace("{3}/{2}", "").Replace("{2}", completeValue.ToString());
		}

		public string GetDesc(bool isFullDesc = true)
        {
			return ReplaceStringWithParameter(isFullDesc);
		}

		public bool IsQuestClear()
        {
			return currentValue >= completeValue;
        }
	}

	public class Quest : ITableData
	{
		private int questID;
		public int ID { get { return questID; } }
		
		public eQuestType Type { get; private set; }

		public eQuestGroup Group { get; private set; }

		public eQuestState State { get; private set; }

		public int TimeStamp { get; private set; }//퀘스트 발생(수락)시각

		public QuestData QuestTableData { get; private set; }
		public Dictionary<int, QuestConditionData> Conditions { get; private set; }

		private bool isSingleCondition = false;
		public bool IsSigleCondition { get { return isSingleCondition; } }
		public Quest(QuestData _quest)
		{
			if (_quest == null)
				return;

			Initialize();
			SetQuestTableData(_quest);
			SetConditionDataByTriggerData();
		}

		public virtual void Init() { }
		public string GetKey() { return questID.ToString(); }
		public void UpdateCondition(int cID, int cValue)
		{
			if(Conditions.ContainsKey(cID))
				Conditions[cID].UpdateCondition(cValue);
			else
				Debug.LogWarning("퀘스트 데이터 입력 오류 key : " + cID + " value : " + cValue);
		}
		void Initialize()
        {
			Conditions = new Dictionary<int, QuestConditionData>();
		}
		void SetQuestTableData(QuestData _quest)
        {
			QuestTableData = _quest;
			questID = QuestTableData.KEY;
			Type = QuestTableData.TYPE;
			Group = QuestTableData.GROUP;

			if(Type == eQuestType.NONE)
				Debug.LogError("QuestType 이 NONE 일 수가 없음 : key : " + questID);
		}

		void SetConditionDataByTriggerData()
        {
			List<QuestTriggerData> triggers = QuestTriggerData.Get(QuestTableData.CONDITION_GROUP.ToString());

			if (triggers == null || triggers.Count <= 0)
			{
				Debug.Log("trigger Data is Null : key " + QuestTableData.CONDITION_GROUP.ToString());
				return;
			}

			foreach (var trigger in triggers)//condition set
			{
				var triggerKey = int.Parse(trigger.KEY);
				var completeValue = trigger.TYPE_KEY_VALUE;
				
				QuestConditionData cond = new QuestConditionData(trigger, completeValue);
				Conditions.Add(triggerKey, cond);
			}

			if (Conditions.Count == 0)
			{
				Debug.LogError("잘못된 퀘스트 데이터 - 트리거 데이터 확인할 것 퀘스트 인덱스 : " + questID);
				return;
			}

			isSingleCondition = Conditions.Count <= 1;
		}
		public void SetTimeStamp(int _timeStamp)
        {
			TimeStamp = _timeStamp;
        }

		public void SetState(eQuestState _state)
		{
			State = _state;
		}

		public bool IsQuestClear()
		{
			bool ret = true;
			List<int> keys = Conditions.Keys.ToList();

			for (int i = 0; i < keys.Count; i++)
			{
				ret = Conditions[keys[i]].IsQuestClear();
				if (!ret)
				{
					return false;
				}
			}
			return ret;
		}
		public int GetQuestConditionClearCount()
		{
			int clearCount = 0;
			List<int> keys = Conditions.Keys.ToList();
			for (int i = 0; i < keys.Count; i++)
			{
				if (Conditions[keys[i]].IsQuestClear())
					clearCount++;
			}

			return clearCount;
		}
		public bool IsNewQuest()
        {
			return State == eQuestState.NEW_QUEST;
        }
		public bool IsAlreadyGetRewards()//이미 보상을 받음(보상을 받고나서 accomplish로 이동)
		{
			switch (State)
			{
				case eQuestState.PROCESS_DONE:
				case eQuestState.TERMINATE:
					return true;
			}
			return false;
		}
		public List<Asset> GetReward()
		{
			var qData = QuestData.Get(GetKey());
			return qData.REWARDS.ToList();
		}
		public int GetTypeWeight()
		{
			int weight = 0;

			switch (Type)
			{
				case eQuestType.MAIN:
					weight = -1000000;
					break;

				case eQuestType.SUB:
				case eQuestType.CHAIN:
					weight = -100000;
					break;
				case eQuestType.DAILY:
				case eQuestType.EVENT:
					weight = 0;
					break;

				default:
					break;
			}

			return weight;
		}
		public string GetQuestDesc(bool isFullDesc = true)//현재 conditionData 의 desc 집합
        {
			string desc = "";

			if (Conditions == null || Conditions.Count <= 0)
				return desc;

			var dicKeyList = Conditions.Keys.ToList();
			for (int i = 0; i < Conditions.Count; i++)
			{
				if (i > 0)
					desc += "\n";

				var conditionData = Conditions[dicKeyList[i]];
				if (conditionData == null)
					continue;
				desc += conditionData.GetDesc(isFullDesc);
			}
			return desc;
        }

		public QuestConditionData GetSingleConditionData()
        {
			if (!isSingleCondition)
				return null;

			return Conditions[Conditions.Keys.ToList()[0]];
        }

		public QuestTriggerData GetSingleTriggerData()
        {
			if (!isSingleCondition)
				return null;

			return QuestTriggerData.GetTrigger(Conditions.Keys.ToList()[0]);
		}

		public string GetQuestSubjectNotie()
        {
			if (QuestTableData == null)
				return "";
			switch(QuestTableData._SUBJECT)
            {
				case "quest_base_CHECK_TARGET_DRAGON":					
					return StringData.GetStringFormatByStrKey(QuestTableData._SUBJECT, GetTargetDragonName());
				case "quest_base_LEVEL_TARGET_DRAGON":
					return StringData.GetStringFormatByStrKey(QuestTableData._SUBJECT, GetTargetDragonName(), GetTriggerTypeKeyValue());
				case "quest_base_LEVEL_TARGET_DRAGON_SKILL":
					return StringData.GetStringFormatByStrKey(QuestTableData._SUBJECT, GetTargetDragonName(), GetTriggerTypeKeyValue());
				case "quest_base_CHECK_TRANSCENDENCE_TARGET_DRAGON":
					return StringData.GetStringFormatByStrKey(QuestTableData._SUBJECT, GetTargetDragonName(), GetTriggerTypeKeyValue());
				case "quest_base_ENHANCE_EQUIP":
				{
					if(Conditions.Count > 0)
                    {
						string replaceSubType = "";
						string replaceKeyType = "";
						foreach (var trigger in Conditions.Values)
						{
							switch (trigger.TriggerData.SUB_TYPE)
							{
								case "N":
									replaceSubType = StringData.GetStringByStrKey("Common");
									break;
								case "R":
									replaceSubType = StringData.GetStringByStrKey("Uncommon");
									break;
								case "SR":
									replaceSubType = StringData.GetStringByStrKey("Rare");
									break;
								case "UR":
									replaceSubType = StringData.GetStringByStrKey("Unique");
									break;
								case "L":
									replaceSubType = StringData.GetStringByStrKey("Legendary");
									break;
							}
							
							if (!string.IsNullOrEmpty(replaceSubType))
							{
								replaceKeyType = trigger.TriggerData.TYPE_KEY;
								var originStrData = StringData.GetStringByStrKey("quest_group_ENHANCE_EQUIP");
								return originStrData.Replace("{0}", replaceSubType).Replace("{1}", replaceKeyType).Replace("{3}/{2}", "");
							}
						}
					}
					break;
				}
			}

			return GetQuestDesc(false);
			//return StringData.GetStringByStrKey(QuestTableData._SUBJECT); 
        }

		string GetTargetDragonName()
        {
			var trigger = GetSingleTriggerData();
			if(trigger != null)
            {
				var charData = CharBaseData.Get(trigger.TYPE_KEY);
				if(charData != null)
                {
					return StringData.GetStringByStrKey(charData._NAME);
				}
            }

			return "";
		}

		int GetTriggerTypeKeyValue()
		{
			var trigger = GetSingleTriggerData();
			if (trigger != null)
			{
				return trigger.TYPE_KEY_VALUE;
			}

			return 0;
		}
	}
}
