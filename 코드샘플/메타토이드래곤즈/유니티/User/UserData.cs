using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SandboxNetwork
{
    public class UserData
    {
#if UNITY_EDITOR
        public bool USE_EDITOR_STATE = false;
#endif
        public string UserNick { get; private set; } = "";
        /// <summary> 채팅용 로그인 시 마지막 길드 </summary>
        public long LastGuildNo { get; private set; } = 0;

        public int AccountState { get; private set; } = 0;
        public int Gold { get; private set; } = -1;
        public int Gemstone { get; private set; } = -1;
        public int CashGemstone { get; private set; } = -1;
        public int Oracle { get; private set; } = 0;
        public int Energy { get; private set; } = -1;
        public int Energy_Exp { get; private set; } = -1;
        public int Exp { get; private set; } = -1;
        public int Level { get; private set; } = -1;
        public int Friendly_Point { get; private set; } = -1;
        /// <summary>
        /// 아레나 포인트 상점에 사용가능한 포인트로 추정됨
        /// </summary>
        public int Arena_Point { get; private set; } = -1;
        public int Mileage { get; private set; } = 0;
        public int Guild_Point { get; private set; } = 0;
        public string UserPortrait { get; private set; } = "";
        public int Magnet { get; private set; } = 0;
        public int Magnite { get; private set; } = 0;
        public int State { get; private set; } = 0;
        private int sequence = 0;
        public int Sequence 
        { 
            get {
                return Math.Max(CacheUserData.GetInt("Sequence", sequence), sequence);
            } 
        }
        public UserBuffData ExtraStatBuff { get; private set; } = new UserBuffData();

        public PortraitEtcInfoData UserPortraitFrameInfo { get; private set; } = new PortraitEtcInfoData() ;

        public void Set(JObject jsonData)
        {
            if (SBFunc.IsJTokenCheck(jsonData["nick"]))
            {
                UpdateID(jsonData["nick"].Value<string>());
            }
            if (SBFunc.IsJTokenCheck(jsonData["gold"]))
            {
                UpdateGold(jsonData["gold"].Value<int>());
            }            
            if (SBFunc.IsJTokenCheck(jsonData["gemstone"]))
            {
                UpdateGemstone(jsonData["gemstone"].Value<int>());
            }
            if (SBFunc.IsJTokenCheck(jsonData["gemstone_cash"]))
            {
                UpdateCashGemstone(jsonData["gemstone_cash"].Value<int>());
            }
            if (SBFunc.IsJTokenCheck(jsonData["energy"]))
            {
                UpdateEnergy(jsonData["energy"].Value<int>());
            }
            if (SBFunc.IsJTokenCheck(jsonData["energy_tick"]))
            {
                UpdateEnergyExp(jsonData["energy_tick"].Value<int>());
            }
            if (SBFunc.IsJTokenCheck(jsonData["exp"]))
            {
                UpdateExp(jsonData["exp"].Value<int>());
            }
            if (SBFunc.IsJTokenCheck(jsonData["level"]))
            {
                UpdateLevel(jsonData["level"].Value<int>());
            }
            if (SBFunc.IsJTokenCheck(jsonData["icon"]))
            {
                UpdatePortrait(jsonData["icon"].Value<string>());
            }
            if (SBFunc.IsJTokenCheck(jsonData["friendly_point"]))
            {
                UpdateFriendlyPoint(jsonData["friendly_point"].Value<int>());
            }
            if (SBFunc.IsJTokenCheck(jsonData["arena_point"]))
            {
                UpdateArenaPoint(jsonData["arena_point"].Value<int>());
            }
            if (SBFunc.IsJTokenCheck(jsonData["mileage"]))
            {
                UpdateMileage(jsonData["mileage"].Value<int>());
            }
            //WJ - User의 계정 상태 정보 (밴, 삭제, 등)
            if (SBFunc.IsJTokenCheck(jsonData["state"]))
            {
                // UpdateUserState(jsonData["user_state"].Value<int>());
                AccountState = jsonData["state"].Value<int>();
            }

            if (SBFunc.IsJTokenCheck(jsonData["sequence"]))
            {
                sequence = jsonData["sequence"].Value<int>();
            }

            if (jsonData.ContainsKey("extra_stat"))
            {
                ExtraStatBuff.SetUserBuffData(jsonData["extra_stat"]);
            }

            //초상화 보상 데이터 추가
            if (jsonData.ContainsKey("portrait"))
            {
                UserPortraitFrameInfo.UpdateInfo(jsonData["portrait"]);
            }

            if (jsonData.ContainsKey("guild_point"))
            {
                UpdateGuildPoint(jsonData["guild_point"].Value<int>());
            }

            if (jsonData.ContainsKey("guild_no"))
            {
                LastGuildNo = jsonData["guild_no"].Value<long>();
            }
        }
        public void UpdateID(string value)
        {
            UserNick = value;
        }
        public void UpdateGold(int value)
        {
            Gold = value;
        }
        public void UpdateGemstone(int value)
        {
            Gemstone = value;
        }
        public void UpdateCashGemstone(int value)
        {
            CashGemstone = value;
        }

        public void UpdateOracle(int value)
        {
            Oracle = value;
        }
        public void UpdateEnergy(int value)
        {
            Energy = value;
        }
        public void UpdateEnergyExp(int value)
        {
            Energy_Exp = value;
        }
        public void UpdateExp(int value)
        {
            Exp = value;
        }
        public void UpdateLastGuildID(long value)
        {
            LastGuildNo = value;
        }
        public void UpdateLevel(int value)
        {
            if(value >= 10 && Level != -1 && Level != value)//레벨 패스용 갱신 조건(초기 로그인일 때는 안타게)
            {
                BattlePassManager.Instance.SetUserLevelChangedReward(value);
                UserStatusEvent.RefreshLevel();
            }
            Level = value;
            if (value >= GameConfigTable.GetConfigIntValue("GUILD_REMOVAL_CONDITION", 30))
            {
                if (Town.Instance != null && Town.Instance.guildBuilding != null)
                    Town.Instance.guildBuilding.RefreshState();
            }
        }
        public void UpdatePortrait(string value)
        {
            UserPortrait = value;
        }
        public void UpdateFriendlyPoint(int value)
        {
            Friendly_Point = value;
        }
        public void UpdateArenaPoint(int value)
        {
            Arena_Point = value;
        }
        public void UpdateMileage(int value)
        {
            Mileage = value;
        }
        public void UpdateMagnet(int value)
        {
            Magnet = value;
        }
        public void UpdateMagnite(int value)
        {
            Magnite = value;
        }
        public void UpdateGuildPoint(int value)
        {
            Guild_Point = value;
        }
        public void UpdateUserState(int value)
        {
#if UNITY_EDITOR
            if (USE_EDITOR_STATE)
                return;
#endif
            State = value;
        }

#if UNITY_EDITOR
        public void SetUserStateForDebug(int value)
        {
            if(USE_EDITOR_STATE)
                State = value;
        }
#endif

        public void UpdatePortraitInfo(JToken _jsonData)
        {
            if (UserPortraitFrameInfo != null)
                UserPortraitFrameInfo.UpdateInfo(_jsonData);
        }

        public void Clear()
        {
            UserNick = "";
            Gold = -1;
            Gemstone = -1;
            CashGemstone = -1;
            Energy = -1;
            Energy_Exp = -1;
            Oracle = 0;
            Exp = -1;
            Level = -1;
            UserPortrait = "";
            ExtraStatBuff = new UserBuffData();
            UserPortraitFrameInfo.Clear();
        }
    }
}