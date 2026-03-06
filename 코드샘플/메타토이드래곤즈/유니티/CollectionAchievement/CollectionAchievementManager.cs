using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
	/// <summary>
	/// 업적 & 콜렉션 통합 관리 - (타운 씬 연출 제어나 전체 상태 데이터 들고있는 방향으로)
	/// </summary>
	/// 

	public enum eCollectionAchievementType//데이터 타입 분기용
    {
		NONE,
		COLLECTION,
		ACHIEVEMENT,
    }

    public class CollectionAchievementManager : IManagerBase
    {
		private static CollectionAchievementManager instance = null;
		public static CollectionAchievementManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CollectionAchievementManager();
				}
				return instance;
			}
		}

		private string USER_COLLECTION_ACCOMPLISH_LOCAL_CACHE_KEY = "collection_accomplish";
		private string USER_ACHIEVEMENT_ACCOMPLISH_LOCAL_CACHE_KEY = "achievement_accomplish";

		public Dictionary<int, Collection> CollectionDataDic { get; private set; } = null;//key : collection_info key, value : condition data(데이터 테이블 기반 전체 데이터)
		public Dictionary<int, AchievementBase> AchievementDataDic { get; private set; } = null;//key : achievement_info key, value : condition data(데이터 테이블 기반 전체 데이터)

		public Dictionary<int, Collection> CompleteCollectionDic { get; private set; } = null;
		public Dictionary<int, AchievementBase> CompleteAchievementDic { get; private set; } = null;

		public List<int> CollectionReddotDotCacheList { get; private set; } = null;//콜렉션 레드닷용 리스트 (key 값만 들고있음)
		public List<int> AchievementReddotDotCacheList { get; private set; } = null;//업적 레드닷용 리스트 (key 값만 들고있음)

		public void Initialize()
        {
			CollectionDataDic = new();
			AchievementDataDic = new();
			CompleteCollectionDic = new();
			CompleteAchievementDic = new();
			CollectionReddotDotCacheList = new();
			AchievementReddotDotCacheList = new();
		}

		/// <summary>
		/// 테이블 데이터 기반 리스트 세팅
		/// </summary>
		void SetCollectionDic()
		{
			CollectionDataDic.Clear();
			var collectionDataTable = TableManager.GetTable<CollectionTable>();
			if (collectionDataTable == null)
				return;

			var totalData = collectionDataTable.GetAllList();
			foreach (var collection in totalData)
            {
				if (collection == null)
					continue;

				CollectionDataDic.Add(collection.KEY, new Collection(collection));
			}
        }

		void SetAchievementDic()
        {
			AchievementDataDic.Clear();
			var achievementTable = TableManager.GetTable<AchievementBaseTable>();
			if (achievementTable == null)
				return;

			var totalData = achievementTable.GetAllList();
			foreach (var achievement in totalData)
			{
				if (achievement == null)
					continue;

				AchievementDataDic.Add(achievement.KEY, new AchievementBase(achievement));
			}
		}

		public string GetLocalAccomplishDataByType(eCollectionAchievementType _type)
        {
			return _type == eCollectionAchievementType.COLLECTION ? CacheUserData.GetString(USER_COLLECTION_ACCOMPLISH_LOCAL_CACHE_KEY, "") :
				CacheUserData.GetString(USER_ACHIEVEMENT_ACCOMPLISH_LOCAL_CACHE_KEY, "");
		}

		public void ClearLocalCollectionAccomplishDataByType(eCollectionAchievementType _type)
        {
			switch(_type)
            {
				case eCollectionAchievementType.COLLECTION:
					CacheUserData.SetString(USER_COLLECTION_ACCOMPLISH_LOCAL_CACHE_KEY, "");
					break;
				case eCollectionAchievementType.ACHIEVEMENT:
					CacheUserData.SetString(USER_ACHIEVEMENT_ACCOMPLISH_LOCAL_CACHE_KEY, "");
					break;
            }
		}
		void SetLocalCollectionAccomplishDataByType(eCollectionAchievementType _type , string _value)
		{
			switch (_type)
			{
				case eCollectionAchievementType.COLLECTION:
					CacheUserData.SetString(USER_COLLECTION_ACCOMPLISH_LOCAL_CACHE_KEY, _value);
					break;
				case eCollectionAchievementType.ACHIEVEMENT:
					CacheUserData.SetString(USER_ACHIEVEMENT_ACCOMPLISH_LOCAL_CACHE_KEY, _value);
					break;
			}
		}

		void AddCacheListByType(eCollectionAchievementType _type, int _key)
        {
			switch(_type)
            {
				case eCollectionAchievementType.COLLECTION:
					CollectionReddotDotCacheList.Add(_key);
					ReddotManager.Set(eReddotEvent.COLLECTION, true);
					break;
				case eCollectionAchievementType.ACHIEVEMENT:
					AchievementReddotDotCacheList.Add(_key);
					ReddotManager.Set(eReddotEvent.ACHIVEMENT, true);
					break;
			}
        }

		void InitDataByType(eCollectionAchievementType _type)
        {
			switch(_type)
            {
				case eCollectionAchievementType.COLLECTION:
					CollectionReddotDotCacheList.Clear();
					SetCollectionDic();//기본 데이터 세팅
					break;
				case eCollectionAchievementType.ACHIEVEMENT:
					AchievementReddotDotCacheList.Clear();
					SetAchievementDic();
					break;
            }
        }
		/// <summary>
		/// 로그인시 콜렉션 정보 동기화
		/// </summary>
		/// <param name="jsonData"></param>
		public void UserDataSync(JObject jsonData, eCollectionAchievementType _type)
		{
			InitDataByType(_type);

			if (jsonData.ContainsKey("progress_list"))
			{
				if (SBFunc.IsJTokenType(jsonData["progress_list"], JTokenType.Object))
				{
					JObject current = (JObject)jsonData["progress_list"];//key가 cID 이고, value array 가 드래곤 리스트
					switch(_type)
                    {
						case eCollectionAchievementType.COLLECTION:
							UpdateCollectionData(current);
							break;
						case eCollectionAchievementType.ACHIEVEMENT:
							UpdateAchievementData(current);
							break;
                    }
				}
			}
			if (jsonData.ContainsKey("accomplish") && SBFunc.IsJArray(jsonData["accomplish"]))
			{
				JArray current = (JArray)jsonData["accomplish"];
				for (int i = 0; i < current.Count; i++)
				{
					SetDataInCompleteDic(_type, current[i].Value<int>());
				}
			}
		}

		void UpdateAchievementData(JObject _data)
        {
			foreach (var val in _data.Properties().ToArray())
			{
				var key = val.Name;
				var value = val.Value.Value<int>();

				AchievementBase data = GetAchievementDataByKey(int.Parse(key));
				if (data != null)
					data.UpdateCondition(value);
			}
		}
		void UpdateCollectionData(JObject _data)
        {
			foreach (var val in _data.Properties().ToArray())
			{
				var key = val.Name;
				var valueList = (JArray)val.Value;

				List<int> tempArr = new List<int>();
				for (int i = 0; i < valueList.Count; i++)
					tempArr.Add(valueList[i].Value<int>());

				Collection data = GetCollectionDataByKey(int.Parse(key));
				if (data != null)
					data.UpdateCondition(tempArr);//영향 받는 드래곤만 리스트로 들어옴.
			}
		}
		/// <summary>
		/// push 처리 (콜렉션 일 때 : 필드 명 : list (key, dragonID List), accomplish (key))
		/// </summary>
		/// <param name="jsonData"></param>
		/// <param name="_type"></param>
		public void ProgressUpdate(JToken _jsonData, eCollectionAchievementType _type)
		{
			JObject jsonData = (JObject)_jsonData;
			if (jsonData.ContainsKey("data") && SBFunc.IsJObject(jsonData["data"]))
			{
				var objData = (JObject)jsonData["data"];
				if (objData.ContainsKey("list") && SBFunc.IsJObject(objData["list"]))
				{
					JObject listData = (JObject)objData["list"];
					switch (_type)
					{
						case eCollectionAchievementType.COLLECTION:
							UpdateCollectionData(listData);
							break;
						case eCollectionAchievementType.ACHIEVEMENT:
							UpdateAchievementData(listData);
							break;
					}
				}
				if (objData.ContainsKey("accomplish") && SBFunc.IsJArray(objData["accomplish"]))//달성한게 있으면
				{
					string prev = GetLocalAccomplishDataByType(_type);
					List<string> prevs = prev.Split(",").ToList();
					if (prevs == null)
						prevs = new List<string>();

					JArray accomDataArr = (JArray)objData["accomplish"];
					for (int i = 0; i < accomDataArr.Count; i++)
					{
						int key = accomDataArr[i].Value<int>();

						SetDataInCompleteDic(_type, key);
						AddCacheListByType(_type, key);

						//var isAchievement = _type == eCollectionAchievementType.ACHIEVEMENT;
						//var title = isAchievement ? StringData.GetStringByStrKey("achievements_info_title") : StringData.GetStringByStrKey("collection_info_title");
						//if (Town.Instance != null && isAchievement)//콜렉션(eCollectionAchievementType.COLLECTION)은 일단 넣어놓고 외부 호출로 변경
						//	NotificationManager.Instance.ShowToastCollectionAchievementComplete(title, AchievementBaseData.Get(key).TOAST);
						//else
						//	prevs.Add(key.ToString());

						//WJ - 09.06 업적 획득도 바로 뜨는게 아니라, 타운씬으로(팝업 하나도 없을 때) 갱신되는 시점에 해달라고함.
						prevs.Add(key.ToString());
					}

					prevs.RemoveAll(str => string.IsNullOrEmpty(str));
					SetLocalCollectionAccomplishDataByType(_type, string.Join(",", prevs));

					var showCollectReddot = IsShowCollectionReddot();
					var showAchieveReddot = IsShowAchievementReddot();
					if ((showCollectReddot || showAchieveReddot) && Town.Instance != null)
					{
                        if(showCollectReddot)
							UIObjectEvent.Event(UIObjectEvent.eEvent.REFRESH_COLLECTION_REDDOT, UIObjectEvent.eUITarget.RB);
						if(showAchieveReddot)
							UIObjectEvent.Event(UIObjectEvent.eEvent.REFRESH_ACHIEVEMENT_REDDOT, UIObjectEvent.eUITarget.HAMBURGER);
					}
                }
			}
		}

		void AddCompleteData(eCollectionAchievementType _type, CollectionAchievement _data)
        {
			if (_data == null)
				return;

			var key = _data.KEY;
			if(_type == eCollectionAchievementType.COLLECTION)
            {
				if (CompleteCollectionDic == null)
					CompleteCollectionDic = new();

				if (!CompleteCollectionDic.ContainsKey(key))
					CompleteCollectionDic.Add(key, (Collection)_data);
            }
			else if(_type == eCollectionAchievementType.ACHIEVEMENT)
            {
				if (CompleteAchievementDic == null)
					CompleteAchievementDic = new();

				if (!CompleteAchievementDic.ContainsKey(key))
					CompleteAchievementDic.Add(key, (AchievementBase)_data);
            }
		}

		/// <summary>
		/// 완료 컬렉션 등록하기
		/// </summary>
		/// <param name="_key"></param>
		void SetDataInCompleteDic(eCollectionAchievementType _type, int _key)
		{
			CollectionAchievement data = null;
			if(_type == eCollectionAchievementType.COLLECTION)
				data = GetCollectionDataByKey(_key);
			else
				data = GetAchievementDataByKey(_key);

			if (data == null)
			{
#if UNITY_EDITOR
				Debug.LogError(_type +" 타입 데이터 누락 >> key  : " + _key);
#endif
				return;
			}

			AddCompleteData(_type, data);
		}
		public Collection GetCollectionDataByKey(int _cID)
        {
			if (CollectionDataDic == null || CollectionDataDic.Count <= 0)
				return null;

			if (CollectionDataDic.ContainsKey(_cID))
				return CollectionDataDic[_cID];
			else
				return null;
        }

		public AchievementBase GetAchievementDataByKey(int _aID)
        {
			if (AchievementDataDic == null || AchievementDataDic.Count <= 0)
				return null;

			if (AchievementDataDic.ContainsKey(_aID))
				return AchievementDataDic[_aID];
			else
				return null;
        }

		string GetTableDataNameByType(eCollectionAchievementType _type, int _key)
        {
			string nameKey = "";
			switch(_type)
            {
				case eCollectionAchievementType.COLLECTION:
					var cData = GetCollectionDataByKey(_key);
					if (cData != null)
						nameKey = cData.NameKey;
					break;
				case eCollectionAchievementType.ACHIEVEMENT:
					var aData = GetAchievementDataByKey(_key);
					if (aData != null)
						nameKey = aData.NameKey;
					break;
            }
			return nameKey;
        }
		/// <summary>
		/// 이미 완료(보상을 받은)콜렉션 인지
		/// </summary>
		/// <param name="_cID"></param>
		/// <returns></returns>
		public bool IsCompleteUserData(eCollectionAchievementType _type, int _key)
		{
			return _type == eCollectionAchievementType.COLLECTION ? IsCompleteCollection(_key) : IsCompleteAchievement(_key);
		}

		bool IsCompleteCollection(int _key)
        {
			return CompleteCollectionDic.ContainsKey(_key);
        }

		bool IsCompleteAchievement(int _key)
        {
			return CompleteAchievementDic.ContainsKey(_key);
        }
		public List<CollectionAchievement> GetTotalDataByType(eCollectionAchievementType _type)
        {
			return _type == eCollectionAchievementType.COLLECTION ? CollectionDataDic.Values.ToList<CollectionAchievement>() : AchievementDataDic.Values.ToList<CollectionAchievement>();
        }
		public List<CollectionAchievement> GetCompleteDataByType(eCollectionAchievementType _type)
		{
			return _type == eCollectionAchievementType.COLLECTION ? CompleteCollectionDic.Values.ToList<CollectionAchievement>() : CompleteAchievementDic.Values.ToList<CollectionAchievement>();
		}

		public void ClearReddotList(eCollectionAchievementType _type)
        {
			if (_type == eCollectionAchievementType.COLLECTION)
				ClearCollectionCacheList();
			else if (_type == eCollectionAchievementType.ACHIEVEMENT)
				ClearAchievementCacheList();
        }
		void ClearCollectionCacheList()
        {
			if (CollectionReddotDotCacheList != null)
				CollectionReddotDotCacheList.Clear();

			ReddotManager.Set(eReddotEvent.COLLECTION, false);
		}
		void ClearAchievementCacheList()
		{
			if (AchievementReddotDotCacheList != null)
				AchievementReddotDotCacheList.Clear();

			ReddotManager.Set(eReddotEvent.ACHIVEMENT, false);
		}

		public bool IsContainReddotList(eCollectionAchievementType _type, int _key)
        {
			return _type == eCollectionAchievementType.COLLECTION ? IsContainReddotCollection(_key) : IsContainReddotAchievement(_key);
        }

		bool IsContainReddotCollection(int _key)
        {
			if (CollectionReddotDotCacheList == null)
				return false;
			return CollectionReddotDotCacheList.Contains(_key);
		}
		bool IsContainReddotAchievement(int _key)
		{
			if (AchievementReddotDotCacheList == null)
				return false;
			return AchievementReddotDotCacheList.Contains(_key);
		}
		/// <summary>
		/// 데이터 테이블 상에서 뿌릴 수 있는 stat을 전부 가져오기
		/// </summary>
		/// <param name="_type"></param>
		/// <returns></returns>
		public List<KeyValuePair<eStatusType, eStatusValueType>> GetStatTypeListByType(eCollectionAchievementType _type)
        {
			var ret = new List<KeyValuePair<eStatusType, eStatusValueType>>();
			Dictionary<eStatusType, List<eStatusValueType>> tempDic = new Dictionary<eStatusType, List<eStatusValueType>>();
			if(_type == eCollectionAchievementType.COLLECTION)
            {
				if (CollectionDataDic == null || CollectionDataDic.Count <= 0)
					return ret;

				foreach(var dicFactor in CollectionDataDic)
                {
					var value = dicFactor.Value;
					if (value == null)
						continue;

					var statType = value.StatType;
					if (!tempDic.ContainsKey(statType))
						tempDic.Add(statType, new List<eStatusValueType>());

					if (!tempDic[statType].Contains(value.StatValueType))
						tempDic[statType].Add(value.StatValueType);
				}
            }
			else if(_type == eCollectionAchievementType.ACHIEVEMENT)
            {
				if (AchievementDataDic == null || AchievementDataDic.Count <= 0)
					return ret;

				foreach (var dicFactor in AchievementDataDic)
				{
					var value = dicFactor.Value;
					if (value == null)
						continue;

					var statType = value.StatType;
					if (!tempDic.ContainsKey(statType))
						tempDic.Add(statType, new List<eStatusValueType>());

					if (!tempDic[statType].Contains(value.StatValueType))
						tempDic[statType].Add(value.StatValueType);
				}
			}

			foreach(var key in tempDic)
            {
				foreach (var val in key.Value)
				{
					ret.Add(new KeyValuePair<eStatusType, eStatusValueType>(key.Key, val));
				}
            }

			return ret;
        }

		public bool IsShowCollectionReddot()
        {
			if (CollectionReddotDotCacheList == null)
				return false;

			return CollectionReddotDotCacheList.Count > 0;
        }
		public bool IsShowAchievementReddot()
		{
			if (AchievementReddotDotCacheList == null)
				return false;

			return AchievementReddotDotCacheList.Count > 0;
		}

		public void Update(float dt) { }
    }
}

