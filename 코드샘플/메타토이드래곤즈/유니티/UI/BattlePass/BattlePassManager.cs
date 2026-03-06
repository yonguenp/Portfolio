using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class BattlePassManager
    {
        // Start is called before the first frame update
        private static BattlePassManager instance = null;
        public static BattlePassManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BattlePassManager();
                }
                return instance;
            }
        }



        public IEnumerator RefreshPassData(eBattlePassType passType)
        {
            long uno = User.Instance.UserAccountData.UserNumber;
            if (uno < 1)
                yield break;
            WWWForm param = new WWWForm();
            param.AddField("uno", uno.ToString());
            param.AddField("imholder", User.Instance.IS_HOLDER ? 1 : 0);
            NetworkManager.NetworkQueue data = new NetworkManager.NetworkQueue();
            if (passType == eBattlePassType.BATTLE)
            {
                data = new NetworkManager.NetworkQueue("pass/pass",param,(JObject jsonData)=> {
                    if (!SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                        return;
                    if ((eApiResCode)(int)jsonData["rs"] == eApiResCode.OK)
                    {
                        SetBattlePassData(jsonData);
                    }
                    },null);
            }
            else if (passType == eBattlePassType.HOLDER)
            {
                Debug.LogError("holder req");
                //holder 패스 제거
                //data = new NetworkManager.NetworkQueue("pass/holder", param, (JObject jsonData) => {
                //    if (!SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer))
                //        return;
                //    if ((eApiResCode)(int)jsonData["rs"] == eApiResCode.OK)
                //    {
                //        SetHolderData(jsonData);
                //    }
                //    if (jsonData.ContainsKey("pass_remain_time"))
                //    {
                //        if (jsonData["pass_remain_time"].Type == JTokenType.Null || jsonData["pass_remain_time"].ToObject<int>() < TimeManager.GetTime())
                //            return;
                //    }
                //}, null);
            }
            yield return NetworkManager.Instance.SendCorutine(data);
        }

        #region Battle Pass Info
        public ePassUserType UserType { get; private set; } = ePassUserType.DEFAULT;

        public PassInfoData BattlePassData
        {
            get
            {
                if (battlePassData == null)
                {
                    battlePassData = PassInfoData.GetCurPass(eBattlePassType.BATTLE);
                }
                return battlePassData;
            }
        }
        private PassInfoData battlePassData = null;
        public List<eBattlePassRewardState> PassRewardStates { get; private set; } = null;
        public List<eBattlePassRewardState> SpecialRewardStates { get; private set; } = null;

        public int PassPoint { get; private set; } = 0;
        public int PassLevel { get; private set; } = 1;

        public bool IsPassMaxLv
        {
            get
            {
                if (BattlePassData == null) return false;
                return PassLevel == BattlePassData.GetMaxLevel();
            }
        }



        public void SetBattlePassData(JObject jsonData)
        {
            if (SBFunc.IsJTokenType(jsonData["vip"], JTokenType.Integer))
            {
                SetUserType((ePassUserType)jsonData["vip"].ToObject<int>());
            }
            if (SBFunc.IsJTokenType(jsonData["season_id"], JTokenType.Integer))
            {
                battlePassData = PassInfoData.Get(jsonData["season_id"].ToObject<int>());
            }
            if (SBFunc.IsJTokenType(jsonData["exp"], JTokenType.Integer))
            {
                SetPassPoint((int)jsonData["exp"]);
            }
            if (SBFunc.IsJArray(jsonData["rewarded"]))
            {
                SetPassRewardState((JArray)jsonData["rewarded"]);
            }
            else
            {
                SetDefaultRewardState();
            }
        }

        void SetDefaultRewardState()
        {
            if (BattlePassData == null) return;
            int count = BattlePassData.PassItems.Count;
            if (PassRewardStates == null || PassRewardStates.Count < count)
            {
                PassRewardStates = Enumerable.Repeat(eBattlePassRewardState.REWARD_DISABLE, count).ToList();
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    PassRewardStates[i] = eBattlePassRewardState.REWARD_DISABLE;
                }
            }

            if (SpecialRewardStates == null || SpecialRewardStates.Count < count)
            {
                SpecialRewardStates = Enumerable.Repeat(UserType == ePassUserType.DEFAULT ? eBattlePassRewardState.LOCK : eBattlePassRewardState.REWARD_DISABLE, count).ToList();
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    SpecialRewardStates[i] = UserType == ePassUserType.DEFAULT ? eBattlePassRewardState.LOCK : eBattlePassRewardState.REWARD_DISABLE;
                }
            }

        }
        public void SetPassRewardState(JArray jsonAray)
        {
            if (BattlePassData == null) return;
            SetDefaultRewardState();

            for (int i = 0; i < jsonAray.Count; i++)
            {
                var info = (JArray)jsonAray[i];
                PassRewardStates[i] = info[0].ToObject<eBattlePassRewardState>();
                var state = info[1].ToObject<eBattlePassRewardState>();
                SpecialRewardStates[i] = state;

            }
        }
        public void SetPassPoint(int point)
        {
            PassPoint = point;
            int lastLv = PassLevel;
            PassLevel = BattlePassData.GetCurrentLevel(point);
            if (PassRewardStates == null|| PassRewardStates.Count < PassLevel -1)
                return;
            if( lastLv < PassLevel)
            {
                for (int i = Mathf.Max(lastLv - 1,0); i < PassLevel; ++i)
                {
                    if (PassRewardStates[i] == eBattlePassRewardState.REWARD_DISABLE)
                        PassRewardStates[i] = eBattlePassRewardState.REWARD_ABLE;
                    if (SpecialRewardStates[i] == eBattlePassRewardState.REWARD_DISABLE)
                        SpecialRewardStates[i] = UserType == ePassUserType.DEFAULT ? eBattlePassRewardState.LOCK : eBattlePassRewardState.REWARD_ABLE;
                }
            }

        }

        public void SetUserType(ePassUserType type)
        {
            
            if(UserType != type && SpecialRewardStates !=null)
            {
                if(type == ePassUserType.PASS_BUY && UserType == ePassUserType.DEFAULT) { 
                    for (int i = 0, count = SpecialRewardStates.Count; i < count; ++i)
                    {
                        if (SpecialRewardStates[i] == eBattlePassRewardState.LOCK)
                        {
                            SpecialRewardStates[i] = (i > PassLevel - 1) ? eBattlePassRewardState.REWARD_DISABLE : eBattlePassRewardState.REWARD_ABLE;
                        }
                    }
                }
            }
            UserType = type;
        }

        public void RefreshBattlePass()
        {
            battlePassData = PassInfoData.GetCurPass(eBattlePassType.BATTLE);
        }

        #endregion


        #region Holder Pass Info
        public List<eBattlePassRewardState> HolderPassRewardStates { get; private set; } = null;
        public PassInfoData HolderPassData { get { return holderPassData; } }
        private PassInfoData holderPassData = null;
        public int HolderPassPoint { get; private set; } = 0;
        public int HolderPassLevel { get; private set; } = 0;

        public int HolderPassRemainTime { get; private set; } = 0;
        public int HolderPassDailyMissionRefreshTime { get; private set; } = 0;
        public bool IsHolderPassMaxLv { get
            {
                if (HolderPassData == null) return false;
                return HolderPassLevel == HolderPassData.GetMaxLevel();
            } }


        public void SetHolderData(JObject jsonData)
        {
            // 서버에서 패스 key 값이나 그런거 보내주지 않음...
            holderPassData = PassInfoData.GetCurPass(eBattlePassType.HOLDER);
            //if (SBFunc.IsJTokenType(jsonData["season_id"], JTokenType.Integer))
            //{
            //    holderPassData = PassInfoData.Get(jsonData["season_id"].ToObject<int>());
            //}
            //else if (jsonData["season_id"].Type == JTokenType.Null)
            //{
            //    holderPassData = PassInfoData.GetCurPass(eBattlePassType.HOLDER);
            //}
            if (SBFunc.IsJTokenType(jsonData["exp"], JTokenType.Integer))
            {
                SetHolderPassPoint((int)jsonData["exp"]);
            }
            if (SBFunc.IsJArray(jsonData["rewarded"]))
            {
                SetHolderPassRewardState((JArray)jsonData["rewarded"]);
            }
            else
            {
                SetDefaultHolderRewardState();
            }
            if (SBFunc.IsJTokenType(jsonData["daily_remain_time"], JTokenType.Integer))
            {
                HolderPassDailyMissionRefreshTime = jsonData["daily_remain_time"].ToObject<int>();
            }
            if (SBFunc.IsJTokenType(jsonData["pass_remain_time"], JTokenType.Integer))
            {
                HolderPassRemainTime = (jsonData["pass_remain_time"].ToObject<int>());
            }
            else if(SBFunc.IsJTokenType(jsonData["pass_remain_time"],JTokenType.Null)|| jsonData["pass_remain_time"].Type == JTokenType.Null)
            {
                HolderPassRemainTime = 0;
            }
        }

        void SetDefaultHolderRewardState()
        {
            if (HolderPassData == null) return;
            int count = HolderPassData.PassItems.Count;
            if (HolderPassRewardStates == null || HolderPassRewardStates.Count < count)
            {
                HolderPassRewardStates = Enumerable.Repeat(eBattlePassRewardState.REWARD_DISABLE, count).ToList();
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    HolderPassRewardStates[i] = eBattlePassRewardState.REWARD_DISABLE;
                }
            }
        }

        public void SetHolderPassRewardState(JArray jsonAray)
        {
            if(HolderPassData == null) return;
            SetDefaultHolderRewardState();

            for (int i= 0; i <jsonAray.Count; i++)
            {
                HolderPassRewardStates[i] = jsonAray[i].ToObject<eBattlePassRewardState>();
            }
        }

        public void SetHolderPassPoint(int point)
        {
            HolderPassPoint = point;
            int lastLv = HolderPassLevel;
            if (HolderPassData == null)
                return;
            HolderPassLevel = HolderPassData.GetCurrentLevel(point);
            if (HolderPassRewardStates == null || HolderPassRewardStates.Count< HolderPassLevel-1)
                return;
            if (lastLv < HolderPassLevel)
            {
                for (int i = lastLv-1; i < HolderPassLevel-1; ++i)
                {
                    if (i < 0)
                        continue;
                    if (HolderPassRewardStates[i] == eBattlePassRewardState.REWARD_DISABLE)
                        HolderPassRewardStates[i] = eBattlePassRewardState.REWARD_ABLE;
                }
            }
        }

        public void RefreshHolderPassData()
        {
            holderPassData = PassInfoData.GetCurPass(eBattlePassType.HOLDER);
        }


        #endregion

        #region Level Pass Info
        //참조는 User 쪽에다가 연결
        public List<AccountData> TableData { get {
                return AccountData.GetTotalRewardList();
            } }

        public List<eBattlePassRewardState> LevelPassRewardStates { get; private set; } = null;
        public List<eBattlePassRewardState> LevelSpecialRewardStates { get; private set; } = null;

        public bool isUserLevelPassVIP = false;//결재 했는지

        bool isUserRewardLevel = false;//유저가 레벨업 해서 보상 받을 수 있는 상태인지 (UI 진입 전)

        public void SetLevelPassData(JObject jsonData)
        {
            if (SBFunc.IsJTokenType(jsonData["vip"], JTokenType.Integer))
            {
                var vipValue = jsonData["vip"].Value<int>();
                isUserLevelPassVIP = vipValue == 0 ? false : true;
            }
            if (SBFunc.IsJArray(jsonData["reward"]))
            {
                SetLevelPassRewardState((JArray)jsonData["reward"]);
            }
            else
            {
                SetLevelDefaultRewardState();
            }
        }
        public void SetLevelPassRewardState(JArray jsonAray)
        {
            SetLevelDefaultRewardState();

            for (int i = 0; i < jsonAray.Count; i++)
            {
                var info = (JArray)jsonAray[i];
                LevelPassRewardStates[i] = info[0].ToObject<eBattlePassRewardState>();
                var state = info[1].ToObject<eBattlePassRewardState>();
                LevelSpecialRewardStates[i] = state;

            }
        }
        void SetLevelDefaultRewardState()
        {
            if (TableData == null || TableData.Count <= 0) return;
            int count = TableData.Count;
            if (LevelPassRewardStates == null || LevelPassRewardStates.Count < count)
            {
                LevelPassRewardStates = Enumerable.Repeat(eBattlePassRewardState.REWARD_DISABLE, count).ToList();
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    LevelPassRewardStates[i] = eBattlePassRewardState.REWARD_DISABLE;
                }
            }

            if (LevelSpecialRewardStates == null || LevelSpecialRewardStates.Count < count)
            {
                LevelSpecialRewardStates = Enumerable.Repeat(isUserLevelPassVIP ? eBattlePassRewardState.REWARD_DISABLE : eBattlePassRewardState.LOCK, count).ToList();
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    LevelSpecialRewardStates[i] = isUserLevelPassVIP ? eBattlePassRewardState.REWARD_DISABLE : eBattlePassRewardState.LOCK;
                }
            }
        }
        /// <summary>
        /// 패스 구매 시 UI 쪽에 보상 가능한 state 만 변경 - 1회성 갱신
        /// </summary>
        public void RefreshSpecialRewardState()
        {
            if(TableData.Count == LevelSpecialRewardStates.Count)
            {
                var curUserLevel = User.Instance.UserData.Level;
                for(int i = 0; i < TableData.Count; i++)
                {
                    var infoData = TableData[i];
                    if (infoData == null)
                        continue;

                    var curState = LevelSpecialRewardStates[i];
                    if (curState == eBattlePassRewardState.REWARDED)//서버 기준 이미 보상 받음
                        continue;

                    var tableLevel = infoData.LEVEL;
                    var isInclude = tableLevel <= curUserLevel;//보상 획득 가능레벨

                    LevelSpecialRewardStates[i] = isInclude ? (isUserLevelPassVIP ? eBattlePassRewardState.REWARD_ABLE : eBattlePassRewardState.LOCK) : eBattlePassRewardState.REWARD_DISABLE;
                }
            }
        }

        public void ClearLevelPass()
        {
            if (LevelPassRewardStates == null)
                LevelPassRewardStates = new List<eBattlePassRewardState>();
            LevelPassRewardStates.Clear();

            if(LevelSpecialRewardStates == null)
                LevelSpecialRewardStates = new List<eBattlePassRewardState>();
            LevelSpecialRewardStates.Clear();
            isUserLevelPassVIP = false;
            isUserRewardLevel = false;
        }

        public void ClearLevelFlag()
        {
            isUserRewardLevel = false;
        }

        /// <summary>
        /// 보상을 받을 수 있는 상태가 있는지
        /// </summary>
        /// <returns></returns>
        public bool GetLevelPassReddotCondition()
        {
            if (LevelPassRewardStates == null || LevelPassRewardStates.Count <= 0)
                return false;
            if (LevelSpecialRewardStates == null || LevelSpecialRewardStates.Count <= 0)
                return false;

            foreach(var state in LevelPassRewardStates)
            {
                if(state == eBattlePassRewardState.REWARD_ABLE)
                {
                    return true;
                }
            }

            foreach (var state in LevelSpecialRewardStates)
            {
                if (state == eBattlePassRewardState.REWARD_ABLE)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// exp_update 를 통해서 level이 바뀌었고, 보상을 받을 수 있는 레벨에 도달하면 사용할 flag
        /// </summary>
        /// <param name="_level"></param>
        public void SetUserLevelChangedReward(int _level)
        {
            if (TableData == null || TableData.Count <= 0)
                return;

            var findData = TableData.Find(element => element.LEVEL == _level);
            if (findData != null)
                isUserRewardLevel = true;
        }

        /// <summary>
        /// portraitUIObject 에 쓰일 reddotTotalCondition
        /// </summary>
        /// <returns></returns>
        public bool IsReddot()
        {
            return GetLevelPassReddotCondition() || isUserRewardLevel;
        }
        #endregion
    }
}