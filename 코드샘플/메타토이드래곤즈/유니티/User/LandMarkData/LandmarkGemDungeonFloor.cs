using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class LandmarkGemDungeonFloor
    {
        /// <summary> 1초에 획득하는 수량 1 / Day 약간 수치 반올림 </summary>
        public static readonly float RewardItemTick = 1f / SBDefine.Day;

        public int Floor { get; private set; } = 0;
        public eGemDungeonState State { get; private set; } = eGemDungeonState.NONE;
        public eGemDungeonState ExpectedState { get { 
                if(Dragons == null || Dragons.Count == 0)
                    return eGemDungeonState.IDLE;
                var dragonDic = LandmarkGemDungeon.Get().DragonDatas;
                foreach (var tag in Dragons)
                {
                    if (dragonDic.ContainsKey(tag))
                    {
                        if (dragonDic[tag].ExpectedFatigue > 0)
                            return eGemDungeonState.BATTLE;
                    }
                }
                
                return eGemDungeonState.END;
            } }
        public int TimeStamp { get; private set; } = 0;
        public int Slot { get; private set; } = 0;
        public int BuffItemNo { get; private set; } = -1;
        public int BuffTimeStamp { get; private set; } = -1;
        public int GainTimeStamp { get; private set; } = -1;
        public int TotalBattlePoint { 
            get
            {
                RefreshDragonBattlePoint();
                return Mathf.FloorToInt(CalcBuff(DragonBattlePoint)); 
            } 
        }
        public int DragonBattlePoint { get; private set; } = 0;

        public List<int> Dragons { get; private set; } = null;
        public List<long> Rewards { get; private set; } = null;
        private LandmarkGemDungeon gemdungeonData = null;
        private LandmarkGemDungeon GemdungeonData
        {
            get
            {
                if (gemdungeonData == null)
                    gemdungeonData = LandmarkGemDungeon.Get();
                return gemdungeonData;
            }
        }
        private List<ProductAutoData> ItemProductData { get; set;} =null;

        /// <summary> 클라이언트에서 마지막 갱신에서 얼마나 지나갔는가 </summary>
        public int ClientTimeDiff { get => TimeManager.GetTimeCompareFromNow(TimeStamp); }
        /// <summary> 버프가 활성화 되어있는 시간인가 </summary>
        public bool IsBuffState { get => BuffTimeDiff < GameConfigTable.GetConfigIntValue("GEMDUNGEON_BOOSTER_TIME"); }
        /// <summary> 버프 시작부터 얼마나 지났는가 </summary>
        public int BuffTimeDiff { get => TimeManager.GetTimeCompareFromNow(BuffTimeStamp); }
        /// <summary> 버프 남은 시간이 얼마나 되는가 </summary>
        public int BuffTimeLimit { get => TimeManager.GetTimeCompare(BuffTimeStamp + GameConfigTable.GetConfigIntValue("GEMDUNGEON_BOOSTER_TIME")); }

        /// <summary> 보상이 가득차서 아무것도 진행할 수 없는 상태인가?  </summary>
        public bool IsFullReward
        {
            get
            {
                if(State == eGemDungeonState.END)
                {
                    int rewardLimit = GameConfigTable.GetConfigIntValue("GEMDUNGEON_REWARD_LIMIT");
                    for(int i=0, count = Rewards.Count;i<count;++i)
                    {
                        if (i == 0)
                            continue;
                        rewardLimit -= Mathf.FloorToInt(Rewards[i] / SBDefine.MILLION);
                    }
                    if (rewardLimit <= 0)
                        return true;
                }
                return false;
            }
        }
        public bool IsReward
        {
            get
            {
                for (int i = 0, count = Rewards.Count; i < count; ++i)
                {
                    if (((Rewards[i] + GetClientReward(i)) / SBDefine.MILLION) > 0)
                        return true;
                }
                return false;
            }
        }

        public LandmarkGemDungeonFloor()
        {
            Floor = 0;
            State = eGemDungeonState.NONE;
        }

        public static int ParseFloor(JToken jsonData)
        {
            if (null == jsonData)
                return 0;

            if (false == SBFunc.IsJArray(jsonData))
                return 0;

            var jsonArray = (JArray)jsonData;
            if (jsonArray.Count > 0 && SBFunc.IsJTokenType(jsonArray[0], JTokenType.Integer))
            {
                return jsonArray[0].Value<int>();
            }
            return 0;
        }
        /// <summary>
        /// 데이터 입력
        /// </summary>
        /// <param name="jsonData">[Floor, State, TimeStamp, Dragons, Rewards]</param>
        public void SetData(JToken jsonData)
        {
            Floor = ParseFloor(jsonData);
            if (0 == Floor)
                return;

            var jsonArray = (JArray)jsonData;
            if (jsonArray.Count > 1 && SBFunc.IsJTokenType(jsonArray[1], JTokenType.Integer))
            {
                State = (eGemDungeonState)jsonArray[1].Value<int>();
            }
            if (jsonArray.Count > 2 && SBFunc.IsJTokenType(jsonArray[2], JTokenType.Integer))
            {
                TimeStamp = jsonArray[2].Value<int>();
            }
            if (jsonArray.Count > 3 && SBFunc.IsJTokenType(jsonArray[3], JTokenType.Integer))
            {
                Slot = jsonArray[3].Value<int>();
            }
            if (jsonArray.Count > 4)//드래곤 Array Or Dic [tag1,tag2,...]
            {
                Dragons = jsonArray[4].Values<int>().ToList();
            }
            if (jsonArray.Count > 5)//보상 Array [item1_count, item2_count, ..., item5_count] 100만 단위
            {
                Rewards = jsonArray[5].Values<long>().ToList();
                //   Rewards = new List<int>((IEnumerable<int>)jsonArray[5].Value<Array>());
            }
            if (jsonArray.Count > 6 && SBFunc.IsJTokenType(jsonArray[6], JTokenType.Integer))
            {
                BuffItemNo = jsonArray[6].Value<int>();
            }
            if (jsonArray.Count > 7 && SBFunc.IsJTokenType(jsonArray[7], JTokenType.Integer))
            {
                BuffTimeStamp = jsonArray[7].Value<int>();
            }
            if (jsonArray.Count > 8 && SBFunc.IsJTokenType(jsonArray[8], JTokenType.Integer))
            {
                GainTimeStamp = jsonArray[8].Value<int>();
            }
            RefreshDragonBattlePoint();
        }

        public void RefreshDragonBattlePoint()
        {
            if (null != GemdungeonData && null != Dragons)
            {
                DragonBattlePoint = 0;
                for (int i = 0, count = Dragons.Count; i < count; ++i)
                {
                    var gemDragon = GemdungeonData.GetDragonData(Dragons[i]);
                    if (gemDragon == null || gemDragon.ExpectedFatigue <= 0)
                        continue;

                    UserDragon dragon = User.Instance.DragonData.GetDragon(Dragons[i]);
                    if (null == dragon)
                        continue;

                    DragonBattlePoint += dragon.GetTotalINF();
                }
            }
        }
        private float CalcBuff(float itemCalc)
        {
            var item = ItemBaseData.Get(BuffItemNo);
            if (item == null)
                return itemCalc;

            if (IsBuffState)
                return itemCalc * (1f + item.VALUE * SBDefine.CONVERT_FLOAT);

            return itemCalc;
        }
        public long GetReward(int index)
        {
            if (null == Rewards || Rewards.Count <= index || 0 > index)
                return 0;

            return Rewards[index];
        }
        private ProductAutoData GetAutoItemData(int index)
        {
            if (null == ItemProductData)
            {
                var curBuildingInfo = User.Instance.GetUserBuildingInfoByTag((int)eLandmarkType.GEMDUNGEON);
                if (null != curBuildingInfo)
                    ItemProductData = ProductAutoData.GetListByGroupAndLevel(BuildingOpenData.GetWithTag(curBuildingInfo.Tag).BUILDING, GetFloorLevel());
            }

            if (null == ItemProductData || ItemProductData.Count <= index || 0 > index)
                return null;

            return ItemProductData[index];
        }
        /// <returns>Amount를 전투력 기준으로 사용하기로 함</returns>
        public int GetAutoAmount(int index)
        {
            var data = GetAutoItemData(index);
            if (null == data)
                return 0;

            return data.ProductItem.Amount;
        }
        /// <returns>MaxTime Gage의 MaxAmount로 사용하기로 함</returns>
        public int GetAutoMaxAmount(int index)
        {
            var data = GetAutoItemData(index);
            if (null == data)
                return 0;

            return data.MAX_TIME;
        }
        /// <returns>TERM을 UI에서 사용할 때 표시 시간 기준으로 사용하기로 함</returns>
        public int GetAutoTerm(int index)
        {
            var data = GetAutoItemData(index);
            if (null == data)
                return 0;

            return data.TERM;
        }
        public int GetClientReward(int index)
        {
            var Amount = GetAutoAmount(index);
            return Mathf.FloorToInt(RewardItemTick * ClientTimeDiff * Mathf.FloorToInt(0 >= Amount ? 0 : TotalBattlePoint / Amount) * SBDefine.MILLION);
        }

        public int GetSpecificBattlePoint(int dragonNo)
        {
            var gemDragon = GemdungeonData.GetDragonData(dragonNo);
            if (gemDragon == null || gemDragon.ExpectedFatigue <= 0)
                return 0;
            int battlePoint = User.Instance.DragonData.GetDragon(dragonNo).GetTotalINF();
            return Mathf.FloorToInt(CalcBuff(battlePoint));

        }

        /// <returns> 현재 FloorData의 데이터 레벨 </returns>
        private int GetFloorLevel()
        {
            return -Floor + SBDefine.GemDungeonDefaultFloor + 1;
        }
        
        public void OnHarvest(VoidDelegate function = null)
        {
            if (false == IsReward)
                return;

            WWWForm param = new WWWForm();
            param.AddField("floor", Floor);
            NetworkManager.SendWithCAPTCHA("gemdungeon/harvest", param, (JObject jsonData) =>
            {
                if (SBFunc.IsJTokenCheck(jsonData["rs"]) && (int)jsonData["rs"] == (int)eApiResCode.OK)
                {
                    if (SBFunc.IsJArray(jsonData["rewards"]))
                    {
                        var rewardPopup = SystemRewardPopup.OpenPopup(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonData["rewards"])));
                        rewardPopup.SetHotTime(GameConfigTable.IsGemDungeonHotTime);
                    }
                    function?.Invoke();
                }
            });
        }
    }
}