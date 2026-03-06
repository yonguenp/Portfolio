using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
	/// <summary>
	/// 아이템 형식을 따르지만, expire_time 을 들고 있는 형태 실제 있는지의 형태는 별도의 itemSet을 따름.
	/// </summary>
	#region mineBoostItemData

	public class MineBoosterItem : Item, ITableData
	{
		public int ExpireTime { get; private set; } = -1;//timestamp

		public MineBoosterItem() : base() { }
		public MineBoosterItem(int no, int count, int expireTime) : base(no, count)
		{
			ExpireTime = expireTime;
		}

		public MineBoosterData BoostTableData
		{
			get
			{
				return MineBoosterData.Get(ItemNo);
			}
		}
		public void SetData(int no, int count, int expireTime)
		{
			base.SetData(no, count);
			ExpireTime = expireTime;
		}
		public void Init() { }
		public string GetKey() { return ItemNo.ToString(); }

		public bool IsPercentType() { return BoostTableData.IS_PERCENT; }
	}

	#endregion

	#region Mining Data

	// 채광 데이터 클래스
	public class MineUserInfoData
	{
		//서버쪽에서 세팅 해주는 키 값 
		const string DATAKEY_MINING_COUNT = "mc";               // 채굴 횟수 (클라쪽에서는 미사용)
		const string DATAKEY_DRILL_HP = "hp";                   // 현재 드릴의 내구도
		const string DATAKEY_RECIPE_ID = "rid";                 // 생산품 레시피 아이디
		const string DATAKEY_START_AT = "s";                    // 채굴 시작 시간
		const string DATAKEY_EXPIRE_AT = "e";                   // 채굴 완료 시간
		const string DATAKEY_MINING_UPDATE_AT = "mu";           // 중간 정산 시간
		const string DATAKEY_INTERMEDIATE_RESULT = "ir";        // 중간 결과 (클라쪽에서는 현재까지의 누적량 표시 용도)
		const string DATAKEY_EVENT_BOOST_NO = "eb";             // 이벤트 부스터 번호
		const string DATAKEY_EVENT_BOOST_EXPIRE_AT = "ebe";     // 이벤트 부스터 만료 시간
		const string DATAKEY_ADD_BOOST_NO = "ab";               // 채굴량 추가(+) 부스터 아이템 번호 - 
		const string DATAKEY_ADD_BOOST_EXPIRE_AT = "abe";       // 채굴량 추가(+) 부스터 아이템 만료 시간
		const string DATAKEY_MULTIPLE_BOOST_NO = "mb";          // 채굴량 증폭(%) 부스터 아이템 번호
		const string DATAKEY_MULTIPLE_BOOST_EXPIRE_AT = "mbe";  // 채굴량 증폭(%) 부스터 아이템 만료 시간


		// 현재 채굴 상태 정의 - 서버에서 직접적으로 주는것이 아니라, 건물 시간 및 건물 상태, 채굴 종료 시간 비교에서 state 를 정의
		public eMiningState MiningState
		{
			get
			{
				var endTime = TimeManager.GetTimeCompare(MiningEndTimeStamp);//현재시간과의 비교
				if (endTime <= 0)//채굴 끝
				{
					var userBuildingInfo = MiningManager.Instance.MineBuildingInfo;
					if (userBuildingInfo == null)
						return eMiningState.NONE;

					var hasConstructingTime = TimeManager.GetTimeCompare(userBuildingInfo.ActiveTime) > 0;
					var hasClaimTime = TimeManager.GetTimeCompare(MiningStartTimeStamp) <= 0;// 받을 것이 있음.
					var isConstructing = userBuildingInfo.State == eBuildingState.CONSTRUCTING;
					if (hasConstructingTime && isConstructing)// 건설 중이거나, 건설 중 상태 - 현재 업글 중
						return eMiningState.UPGRADE;

					if(!hasConstructingTime && userBuildingInfo.State == eBuildingState.NORMAL && hasClaimTime && MiningEndTimeStamp > 0)
						return eMiningState.WAIT_CLAIM;

					switch (userBuildingInfo.State)
					{
						case eBuildingState.LOCKED:
						case eBuildingState.NOT_BUILT:
						case eBuildingState.CONSTRUCT_FINISHED:
							return eMiningState.NONE;
						case eBuildingState.NORMAL:
							return eMiningState.START;
						default:
							return eMiningState.NONE;
					}
				}
				else
					return eMiningState.MINING;
			}
		}

		public int MiningDurability { get; private set; } = 0;			// 현재 드릴의 내구도
		public int MiningStartTimeStamp { get; private set; } = 0;		// 채굴 시작 시간
		public int MiningEndTimeStamp { get; private set; } = 0;		// 채굴 종료 시간
		public float MiningTotalAmountValue { get; private set; } = 0;  // 채굴 누적 마그넷 갯수
		public string VALUE_DESC { get {
				return Mathf.CeilToInt(MiningTotalAmountValue).ToString();
			} }

		public MineBoosterItem plusBoostItem { get; private set; } = null; // 현재 적용 중인 채굴량 추가(+) 부스터 아이템
		public MineBoosterItem percentBoostItem { get; private set; } = null; // 현재 적용 중인 채굴량 증폭(%) 부스터 아이템
		public MineBoosterItem eventBoostItem { get; private set; } = null; // 현재 적용 중인 이벤트 채굴량 부스터 아이템 - 기획이 없어서 아직은 미사용

		public bool IsMiningState() => MiningState == eMiningState.MINING;          // 현재 채굴 진행 중 상태인지 체크
		public bool IsMiningUpgradeState() => MiningState == eMiningState.UPGRADE;  // 현재 업그레이드 중 상태인지 체크
		public bool IsAvailGetMagnet() => MiningState == eMiningState.WAIT_CLAIM;    // 마그넷 획득 가능한 상태인지 체크
		public bool IsAvailUpgrade() => MiningState == eMiningState.START; // 업그레이드 가능 상태인지 체크

		/// <summary>
		/// 현재 부스터 아이템 상태 가져오기 - 퍼센트, 플러스, 이벤트
		/// </summary>
		/// <returns></returns>
		public List<MineBoosterItem> GetBoosterItemList()
		{
			List<MineBoosterItem> retList = new List<MineBoosterItem>();

			retList.Add(percentBoostItem);
			retList.Add(plusBoostItem);
			retList.Add(eventBoostItem);

			return retList;
		}

		/// <summary>
		/// "push":[{"api":"building_update","tag":601,"state":5,"level":5},{"api":"landmark_update"
		/// ,"data":[{"tag":601,"data":{"mc":0,"hp":672,"rid":0,"s":0,"e":0,"mu":0,"ir":0,"eb":0,"ebe":0,"ab":0,"abe":0,"mb":0,"mbe":0,"ebv":0,"abv":0,"mbv":0}}]}]}
		/// </summary>
		/// <param name="newData"></param>
		public void SetInfoData(JObject _jsonData)
        {
			if (_jsonData == null)
				return;

			if(_jsonData.ContainsKey("data"))
            {
				JObject jsonObject = (JObject)_jsonData["data"];

				if (jsonObject.ContainsKey(DATAKEY_DRILL_HP))//드릴 내구도
					MiningDurability = jsonObject[DATAKEY_DRILL_HP].Value<int>();

				if(jsonObject.ContainsKey(DATAKEY_START_AT))//채굴 시작 시간
					MiningStartTimeStamp = jsonObject[DATAKEY_START_AT].Value<int>();

				if (jsonObject.ContainsKey(DATAKEY_EXPIRE_AT))//채굴 종료 시간
					MiningEndTimeStamp = jsonObject[DATAKEY_EXPIRE_AT].Value<int>();

				if (jsonObject.ContainsKey(DATAKEY_INTERMEDIATE_RESULT))//누적 마그넷 총 갯수
					MiningTotalAmountValue = jsonObject[DATAKEY_INTERMEDIATE_RESULT].Value<float>();

				if (jsonObject.ContainsKey(DATAKEY_ADD_BOOST_NO) && jsonObject.ContainsKey(DATAKEY_ADD_BOOST_EXPIRE_AT))//현재 적용 중인 채굴량 추가(+) 부스터 아이템
				{
					var plusItemKey = jsonObject[DATAKEY_ADD_BOOST_NO].Value<int>();
					var expireTimeStamp = jsonObject[DATAKEY_ADD_BOOST_EXPIRE_AT].Value<int>();

					if (plusItemKey > 0 && expireTimeStamp > 0)
						plusBoostItem = new MineBoosterItem(plusItemKey, 0, expireTimeStamp);
					else
						plusBoostItem = null;
				}

				if (jsonObject.ContainsKey(DATAKEY_MULTIPLE_BOOST_NO) && jsonObject.ContainsKey(DATAKEY_MULTIPLE_BOOST_EXPIRE_AT))//현재 적용 중인 채굴량 증폭(%) 부스터 아이템
				{
					var percentItemKey = jsonObject[DATAKEY_MULTIPLE_BOOST_NO].Value<int>();
					var expireTimeStamp = jsonObject[DATAKEY_MULTIPLE_BOOST_EXPIRE_AT].Value<int>();

					if (percentItemKey > 0 && expireTimeStamp > 0)
						percentBoostItem = new MineBoosterItem(percentItemKey, 0, expireTimeStamp);
					else
						percentBoostItem = null;
				}

				if (jsonObject.ContainsKey(DATAKEY_EVENT_BOOST_NO) && jsonObject.ContainsKey(DATAKEY_EVENT_BOOST_EXPIRE_AT))//현재 적용 중인 채굴량 이벤트 부스터 아이템
				{
					var eventBoosterItemKey = jsonObject[DATAKEY_EVENT_BOOST_NO].Value<int>();
					var expireTimeStamp = jsonObject[DATAKEY_EVENT_BOOST_EXPIRE_AT].Value<int>();

					if (eventBoosterItemKey > 0 && expireTimeStamp > 0)
						eventBoostItem = new MineBoosterItem(eventBoosterItemKey, 0, expireTimeStamp);
					else
						eventBoostItem = null;
				}
			}
		}
	}

	#endregion
}