using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class MiningManager : IManagerBase
    {
		public static string MINE_BUILDING_GROUP_KEY = "mine";
		public static int MINE_BUILDING_INSTALL_TAG = (int)eLandmarkType.MINE;
		public static int MINE_SHOP_GOODS_KEY = 2001;

		static MiningManager instance = null;
		public static MiningManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MiningManager();
				}
				return instance;
			}
		}
		/// <summary>
		/// 광산 base building Data
		/// </summary>
		public BuildInfo MineBuildingInfo
        {
            get
            {
				return User.Instance.GetUserBuildingInfoByTag(MINE_BUILDING_INSTALL_TAG);
            }
        }
		//단순 광산 맥스렙 가져오기
		public int GetMineMaxLevel()
        {
			return MineDrillData.GetMineDrillMaxLevel();
        }

		/// <summary>
		/// 광산 오픈 타운 레벨
		/// </summary>
		/// <returns></returns>
		public int GetOpenTownLevel()
        {
			var openData = BuildingOpenData.GetWithTag(MINE_BUILDING_INSTALL_TAG);
			if (openData == null)
				return 0;

			return openData.OPEN_LEVEL;
        }

		/// <summary>
		/// UI 오픈 조건 현재는 타운레벨만
		/// </summary>
		/// <returns></returns>
		public bool IsAvailableMiningUIOpen(bool _showToast = false)
        {
			var openLevel = Instance.GetOpenTownLevel();
			var userTownLevel = User.Instance.TownInfo.AreaLevel;

			var result = openLevel <= userTownLevel;
			if (!result && _showToast)
				ToastManager.On(StringData.GetStringFormatByIndex(100002306, openLevel));

			return result;
		}

		public MineDrillData GetUserDrillData()
        {
			return MineDrillData.GetMineDrillDataByLevel(MineBuildingInfo.Level);
        }

		public ProductData GetProductData()
        {
			return ProductData.GetProductDataByGroupAndLevel(MINE_BUILDING_GROUP_KEY, Instance.MineBuildingInfo.Level);
        }

		//다음 레벨로 업글 가능한지
		public bool IsUpgradeCondition(bool _showToast , bool _includeMaterialConidition = false)
		{
			if(MineBuildingInfo.Level >= 1)//레벨 1부터 상태 제어 조건
            {
				var isAvailState = UserMiningData.IsAvailUpgrade();
				if (!isAvailState)//채광 대기 상태 -> 채광 시작일 때만 업글 가능
				{
					if (_showToast)
						ToastManager.On(StringData.GetStringByStrKey("광산토스트12"));
					return false;
				}
			}

			var isMaxLevel = BuildingLevelData.GetBuildingMaxLevelByGroup(MINE_BUILDING_GROUP_KEY);
			if (isMaxLevel == MineBuildingInfo.Level)//맥렙
			{
				if (_showToast)
					ToastManager.On(StringData.GetStringByStrKey("광산토스트13"));
				return false;
			}
			
			var buildLevelData = BuildingLevelData.GetDataByGroupAndLevel(MINE_BUILDING_GROUP_KEY, MineBuildingInfo.Level);
			if (buildLevelData == null)//현재 레벨 데이터 없음
				return false;

			var townCondition = buildLevelData.NEED_AREA_LEVEL <= User.Instance.GetAreaLevel();
			if (!townCondition)//타운 조건
            {
				if (_showToast)
					ToastManager.On(StringData.GetStringFormatByStrKey("광산토스트14", buildLevelData.NEED_AREA_LEVEL));
				return false;
            }

            var materialCondition = IsBuildingUpgradeMaterialCondition(buildLevelData);
            if (_includeMaterialConidition && !materialCondition)
            {
                if (_showToast)
                    ToastManager.On(StringData.GetStringByStrKey("town_upgrade_text_07"));//필요한 재료가 부족합니다.
                return false;
            }

            return true;
		}
		/// <summary>
		/// 현재 레벨에서 업그레이드 가능한 상태인지 (재료 체크)
		/// </summary>
		/// <returns></returns>
		bool IsBuildingUpgradeMaterialCondition(BuildingLevelData _levelData)
        {
			var isSufficientGold = true;
			var isSufficientMaterial = true;

			foreach (var needItem in _levelData.NEED_ITEM)
			{
				if (needItem == null)
					continue;

				var itemSufficient = User.Instance.GetItemCount(needItem.ItemNo) >= needItem.Amount;
				isSufficientMaterial = isSufficientMaterial && itemSufficient;
			}
			if (_levelData.COST_NUM > 0)
			{
				int userGold = User.Instance.GOLD;
				isSufficientGold = userGold >= _levelData.COST_NUM;
			}

			return isSufficientGold && isSufficientMaterial;
		}

		/// <summary>
		/// 유저의 현재 채광 데이터
		/// </summary>
		public MineUserInfoData UserMiningData { get; set; } = new MineUserInfoData();

		private Dictionary<int, MineBoosterItem> mineBoosterInventory = new Dictionary<int, MineBoosterItem>();

		public delegate void UpdateFinishCallback();

		//protected int recent = 0;//mine/state 쏠때 시간 갱신 체크 용도
		//protected bool requested = false;//mine/state 체크 플래그

		public void Initialize()
        {
			Clear();
		}
		//시간 갱신관련 
		//public bool Recall(bool force = false)
		//{
		//	if (force)
		//	{
		//		requested = true;
		//		NetworkManager.Send("mine/state", null, null);
		//	}
		//	else if ((TimeManager.GetTime() - recent > 60 || UserMiningData.MiningEndTimeStamp - TimeManager.GetTime() > 0 && ((UserMiningData.MiningEndTimeStamp - TimeManager.GetTime()) % 60) == 0) && !requested && recent != TimeManager.GetTime())
		//	{
		//		requested = true;
		//		NetworkManager.Send("mine/state", null, null);
		//	}

		//	return requested;
		//}

		/// <summary>
		/// push - landmarkUpdate를 통해서 데이터 갱신 부분 연결 - rs 가 0 (success 일 때 UI 갱신 부분 호출)
		/// </summary>
		/// 
		public void UpdateUserMiningData(JObject _jsonData)
        {
			if(UserMiningData != null)// 채굴 데이터 세팅
				UserMiningData.SetInfoData(_jsonData);

			if(mineBoosterInventory != null)//부스터 아이템 세팅
            {
				if (_jsonData.ContainsKey("data"))
                {
					JObject itemData = (JObject)_jsonData["data"];
					if (itemData == null)
						return;

					if(itemData.ContainsKey("items"))
                    {
						if (SBFunc.IsJTokenCheck(itemData["items"]))
						{
							var jObjItems = (JArray)itemData["items"];
							foreach (var element in jObjItems)
							{
								var element_item = (JArray)element;
								if (element_item.Count == 3)
								{
									UpdateItem(element_item[0].Value<int>(), element_item[1].Value<int>(), element_item[2].Value<int>());
								}
								else
									Debug.LogError("item size not 3");
							}
						}
						//서버쪽 수정으로 인해서 클라쪽에 필요없음 - 무조건 push를 주는 것으로 변경 (갯수가 0일 때도 push로 줌)
						//else
						//	mineBoosterInventory.Clear();
					}
				}
			}
			//requested = false;
			//recent = TimeManager.GetTime();
		}

		public void RefreshMiningState(UpdateFinishCallback callback = null)
        {
			NetworkManager.Send("mine/state", null, (jsonData) =>
			{
				if (jsonData.ContainsKey("rs") && jsonData["rs"].Value<int>() == 0)
				{
					if (callback != null)
						callback.Invoke();
				}
			});
		}

		#region 부스터 아이템 관련 참조 및 갱신
		/// <summary>
		/// 부스터 아이템 관련 참조 및 갱신
		/// </summary>
		/// <returns></returns>
		public List<MineBoosterItem> GetAllBoosterItem()
		{
			return mineBoosterInventory.Values.ToList();
		}
		public void UpdateItem(MineBoosterItem item)
		{
			UpdateItem(item.ItemNo, item.Amount, item.ExpireTime);
		}

		public void UpdateItem(int no, int amount, int expireTime)
		{
			if (!mineBoosterInventory.ContainsKey(no))
				mineBoosterInventory.Add(no, new MineBoosterItem(no, amount, expireTime));
			else
				mineBoosterInventory[no].SetData(no, amount, expireTime);
		}
		public MineBoosterItem GetItem(int no)
		{
			if (mineBoosterInventory.ContainsKey(no))
				return mineBoosterInventory[no];
			else
				return null;
		}
        #endregion

        void Clear()
		{
			UserMiningData = new MineUserInfoData();

			if (mineBoosterInventory == null)
				mineBoosterInventory = new Dictionary<int, MineBoosterItem>();

			mineBoosterInventory.Clear();
		}

		/// <summary>
		/// 광산의 레드닷 컨디션을 체크하는 용도 타운씬 입장 할 때마다 일단 세팅
		/// 1. 건설 가능 상태
		/// 2. 건설 완료 상태
		/// 3. 채굴 획득 가능 상태
		/// </summary>
		/// <returns></returns>
		public bool IsReddotCondition()
		{
			var isAvailUpgrade = IsUpgradeCondition(false, true);
			var level = MineBuildingInfo.Level;
			var buildingState = MineBuildingInfo.State;
			var isCompleteUpgrade = buildingState == eBuildingState.CONSTRUCT_FINISHED ||
				(buildingState == eBuildingState.CONSTRUCTING && TimeManager.GetTimeCompare(MineBuildingInfo.ActiveTime) <= 0);

			var isWaitClaim = level >= 1 ? UserMiningData.IsAvailGetMagnet() : true;//1렙 밑 (건설 전 상태에서는 건설 가능한지만 체크)

			return level >= 1 ? isAvailUpgrade || isCompleteUpgrade || isWaitClaim : isAvailUpgrade || isCompleteUpgrade;
		}

		public void Update(float dt) { }
    }
}