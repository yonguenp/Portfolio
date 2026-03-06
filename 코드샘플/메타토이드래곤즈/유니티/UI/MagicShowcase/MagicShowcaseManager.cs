using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 서버 쪽에서 들어오는(각 타입별 레벨, 각 재료 투입상태) 데이터는 MagicShowcaseInfoData 객체에 담음
/// 각 타입은 magic_showcase_info 테이블의 MENU_TYPE 칼럼의 타입을 가져와서 각 타입별로 dictionary 형태로 들고있음 (key : MENU_TYPE value : MagicShowcaseInfoData)
/// 달성해서 유지되는 버프는 UserData 내부 extra_stat에 포함되는 식으로 로그인할 때 요청
/// </summary>
/// 
namespace SandboxNetwork
{
    public class MagicShowcaseManager : IManagerBase
	{
		private static MagicShowcaseManager instance = null;
		public static MagicShowcaseManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MagicShowcaseManager();
				}
				return instance;
			}
		}

		public Dictionary<eShowcaseGroupType, MagicShowcaseInfoData> ShowcaseDic { get; private set; } = null;//유저의 진열장 상태

        public void Initialize()
        {
			ShowcaseDic = new();

		}

		/// <summary>
		/// 로그인시 진열장 정보 동기화
		/// </summary>
		/// <param name="jsonData"></param>
		/// "magicshowcase" : [] // 없을 때
		/// "magicshowcase" : {"1" : {"level" : 3 , "stock" : [[3,150000001,0],[3,150000002,3],[3,150000003,4]]} , "2" : {} , ...}

		//objectName : menu_type (메뉴타입) 1~5 - WJ - 2024.01.05 신규 메뉴 타입 추가 (1~6으로 변경)
		//level : 각 타입별 진행 레벨
		//stock : 투입상태 리스트

		public void UserDataSync(JObject jsonData)//1~5 타입 다줘야함.- WJ - 2024.01.05 신규 메뉴 타입 추가 (1~6으로 변경)
		{
			if(SBFunc.IsJTokenType(jsonData , JTokenType.Object))
				UpdateTypeData(jsonData);
		}

		/// <summary>
		/// push data를 통해 처리 해줄 부분 (건설 완료 - 레벨의 변경 / 재료 투입 갱신 부)
		/// </summary>
		/// <param name="_jsonData"></param>
		/// "magic_showcase_update" : {"1" : {"level" : 3 , "stock" : [[3,150000001,0],[3,150000002,3],[3,150000003,4]]}} - 변경되는 object 하나만 주면 되지 않을까

		public void  ProgrssUpdate(JToken _jsonData)
        {
			if (SBFunc.IsJTokenType(_jsonData, JTokenType.Object))
				UpdateTypeData((JObject)_jsonData);
		}

		void UpdateTypeData(JObject _jsonData)
        {
			foreach (var val in _jsonData.Properties().ToArray())
			{
				var key = int.Parse(val.Name);//menu_type - magicShowcaseData 의 Group 값
				var typeKey = (eShowcaseGroupType)key;

				var value = val.Value;
				if(SBFunc.IsJTokenType(value , JTokenType.Object))
                {
					var typeDataObject = (JObject)value;
					var level = 0;
                    if (typeDataObject.ContainsKey("level"))
						level = typeDataObject["level"].Value<int>();

					List<Asset> tempStockList = new List<Asset>();
					if(typeDataObject.ContainsKey("stock") && SBFunc.IsJTokenType(typeDataObject["stock"] , JTokenType.Array))
                    {
						var arr = (JArray)typeDataObject["stock"];

						foreach(JArray rewardArray in arr)
                        {
							List<int> tempRewardList = rewardArray.ToObject<List<int>>();
							if (tempRewardList == null || tempRewardList.Count < 3) { continue; }

							eGoodType type = (eGoodType)tempRewardList[0];
							int no = tempRewardList[1];
							int amount = tempRewardList[2];

							tempStockList.Add(new Asset(type, no, amount));
						}
                    }

					var tempShowcaseInfoData = new MagicShowcaseInfoData(typeKey, level, tempStockList);
					SetDataInDic(typeKey, tempShowcaseInfoData);
				}
			}
		}

		void SetDataInDic(eShowcaseGroupType _type , MagicShowcaseInfoData _data)
        {
			if (ShowcaseDic.ContainsKey(_type))
				ShowcaseDic[_type] = _data;
			else
				ShowcaseDic.Add(_type, _data);
        }

		public MagicShowcaseInfoData GetInfoDataByType(eShowcaseGroupType _type)
        {
			if (ShowcaseDic.ContainsKey(_type))
				return ShowcaseDic[_type];
			return null;
        }

		/// <summary>
		/// 데이터 테이블 상에서 뿌릴 수 있는 stat을 전부 가져오기
		/// </summary>
		/// <param name="_type"></param>
		/// <param name="_isIncludeCurrentLevel"></param>
		/// <returns></returns>
		public Dictionary<eStatusType, Dictionary<eStatusValueType, float>> GetStatTypeListByType(eShowcaseGroupType _type, bool _isIncludeCurrentLevel = true)
		{
			Dictionary<eStatusType, Dictionary<eStatusValueType, float>> tempDic = new Dictionary<eStatusType, Dictionary<eStatusValueType, float>>();

			var currentLevelData = GetAccumulateTableDataByType(_type, _isIncludeCurrentLevel);
			if (currentLevelData == null || currentLevelData.Count <= 0)
				return tempDic;

			foreach (var dicFactor in currentLevelData)
			{
				if (dicFactor == null)
					continue;

				var statType = dicFactor.STAT_TYPE;
				var statValueType = dicFactor.STAT_VALUE_TYPE;
				var statValue = dicFactor.STAT_VALUE;
				if (!tempDic.ContainsKey(statType))
				{
					var data = new Dictionary<eStatusValueType, float>();
					data.Add(statValueType, statValue);
					tempDic.Add(statType, data);
				}
				else
                {
					var dicData = tempDic[statType];
					if (dicData.ContainsKey(statValueType))
					{
						var originValue = dicData[statValueType];
						tempDic[statType][statValueType] = (originValue + statValue);
					}
					else
						tempDic[statType].Add(statValueType, statValue);
                }
			}
			return tempDic;
		}

		/// <summary>
		/// 현재의 레벨을 기준으로 누적 스탯테이블 데이터 호출
		/// </summary>
		/// <param name="_type"></param>
		/// <param name="_isIncludeCurrentLevel"></param>
		/// <returns></returns>
		public List<MagicShowcaseData> GetAccumulateTableDataByType(eShowcaseGroupType _type, bool _isIncludeCurrentLevel = true)
        {
			var tempList = new List<MagicShowcaseData>();
			var currentData = GetInfoDataByType(_type);
			if (currentData == null)
				return tempList;

			return MagicShowcaseData.GetAccumulateDataByLevel(_type,currentData.LEVEL ,_isIncludeCurrentLevel);
        }

		/// <summary>
		/// 현재의 레벨을 기준으로 다음 레벨의 데이터 호출
		/// </summary>
		/// <param name="_type"></param>
		/// <param name="_isIncludeCurrentLevel"></param>
		/// <returns></returns>
		public List<MagicShowcaseData> GetNextLevelTableDataByType(eShowcaseGroupType _type, bool _isIncludeCurrentLevel = false)
		{
			var tempList = new List<MagicShowcaseData>();
			var currentData = GetInfoDataByType(_type);
			if (currentData == null)
				return tempList;

			return MagicShowcaseData.GetNextTotalDataByLevel(_type, currentData.LEVEL, _isIncludeCurrentLevel);
		}

		public eShowcaseGroupType GetGroupType(int _index)
        {
			return (eShowcaseGroupType)_index;
        }

		public bool IsReddotCondition()//mainUI 쪽에서 각 탭 별로 업그레이드 하나라도 가능한 상태가 있다면
        {
			foreach(var keypair in ShowcaseDic)
            {
				var value = keypair.Value;
				if (value.IsUpgradeCondition)
					return true;
            }
			return false;
        }

		public bool IsReddotConditionByType(eShowcaseGroupType _type)//각 탭별 레드닷
        {
			if (ShowcaseDic.ContainsKey(_type))
				return ShowcaseDic[_type].IsUpgradeCondition;

			return false;
        }
		public void Update(float dt) { }//not use
    }
}

