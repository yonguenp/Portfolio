using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ArenaBattleData : BattleData
    {
        #region IBattleData
        public override float Speed { get; set; } = 1.5f;
        public override bool IsAuto => true;
        public override float Time { get; set; } = 0f;
        public override eBattleType BattleType { get => eBattleType.ARENA; }
        public override int Wave { get; protected set; } = 1;
        public override int MaxWave { get; protected set; } = 1;
        #endregion
        #region ArenaData 
        public string EnemyNick { get; private set; } = "";
        public List<int> OffMaxHP { get; private set; } = new List<int>();
        public List<int> DefMaxHP { get; private set; } = new List<int>();
        public int RewardTrophy { get; private set; } = -1;
        public int RewardPoint { get; private set; } = -1;
        public int RewardAccountExp { get; private set; } = -1;
        public int RewardGuildExp { get; private set; } = -1;

        public float MaxTime { get; private set; } = 180f;
        public bool IsMock { get; private set; } = false;
        public bool IsRevenge { get; private set; } = false;

        private eArenaWinType winType = eArenaWinType.None;
        public eArenaWinType WinType { 
            get
            {
                if (winType != eArenaWinType.None)
                    return winType;

                if (CheckTimeOver())
                {
                    if (OffHPRate() > DefHPRate())
                        return !IsRevenge ? eArenaWinType.Offense : eArenaWinType.REV_Success;
                    else
                        return !IsRevenge ? eArenaWinType.Defense : eArenaWinType.REV_Fail;
                }
                else
                {
                    var offDeathCount = 0;
                    foreach (var offenseInfo in OffenseDic)
                    {
                        if (offenseInfo.Value == null || offenseInfo.Value.Death)
                            ++offDeathCount;
                    }
                    if (OffenseDic.Count == offDeathCount)
                        return !IsRevenge ? eArenaWinType.Defense : eArenaWinType.REV_Fail;
                    var defDeathCount = 0;
                    foreach (var defenseInfo in DefenseDic)
                    {
                        if (defenseInfo.Value == null || defenseInfo.Value.Death)
                            ++defDeathCount;
                    }
                    if (DefenseDic.Count == defDeathCount)
                        return !IsRevenge ? (IsMock ? eArenaWinType.Open : eArenaWinType.Offense) : eArenaWinType.REV_Success;
                }

                return winType;
            }
        }
        public List<ArenaResultDragonStat> Stats { get; private set; } = null;
        public List<LogStruct> LogDatas { get; private set; } = null;
        public List<int> OffHP
        {
            get
            {
                if (OffenseDic == null || OffenseDic.Count < 1)
                {
                    return OffMaxHP;
                }

                List<int> hp = new List<int>();
                foreach (var it in OffenseDic)
                {
                    if (it.Value == null) continue;
                    hp.Add(it.Value.HP);
                }
                return hp;
            }
        }

        public List<int> DefHP
        {
            get
            {
                if (DefenseDic == null || DefenseDic.Count < 1)
                {
                    return DefMaxHP;
                }

                List<int> hp = new List<int>();
                foreach (var it in DefenseDic)
                {
                    if (it.Value == null) continue;
                    hp.Add(it.Value.HP);
                }
                return hp;
            }
        }
        public bool IsTimeOut
        {
            get
            {
                return (MaxTime - Time) <= 0;
            }
        }
        #endregion

        const float DefaultX = 0.85f;
        const float SpancingX = 0.65f;
        const float DefaultY = -0.0792f;
        const float SpancingY = 0.3f;

        public override void Initialize()
        {
            Time = 0f;
            MaxTime = 0f;
            OffMaxHP = new List<int>();
            DefMaxHP = new List<int>();
            RewardTrophy = -1;
            RewardPoint = -1;
            winType = eArenaWinType.None;

            IsAuto = false;

            var cur = PlayerPrefs.GetInt("ArenaSpeedIndex", 0);
            int max = User.Instance.BATTLE_SPPED_BOOST ? 5 : 4;
            cur = cur % max;

            var speed = 1.0f;
            switch (cur)
            {
                case 1:
                    speed = 1.2f;
                    break;
                case 2:
                    speed = 1.5f;
                    break;
                case 3:
                    speed = 2f;
                    break;
                case 4:
                    speed = 2.5f;
                    break;
                case 0:
                default:
                    speed = 1.0f;
                    break;
            }
            PlayerPrefs.SetFloat("ArenaSpeed", speed);
            Speed = speed;

            base.Initialize();
        }
        public void AddOffenseDragon(IBattleCharacterData dragon)
        {
            if (OffenseDic == null)
                return;

            OffMaxHP.Add(dragon.MaxHP);
            Characters.Add(dragon);
            OffenseDic.Add(dragon.Position, dragon);
        }
        public void AddDefenseDragon(IBattleCharacterData dragon)
        {
            if (DefenseDic == null)
                return;

            DefMaxHP.Add(dragon.MaxHP);
            Characters.Add(dragon);
            DefenseDic.Add(dragon.Position, dragon);
        }

        public void InitializeCharacterPos()
        {
            InitializeCharacterPos(OffenseDic);
            InitializeCharacterPos(DefenseDic);
        }
        private void InitializeCharacterPos(Dictionary<int, IBattleCharacterData> dic)
        {
            if (dic != null)
            {
                var it = dic.GetEnumerator();
                while (it.MoveNext())
                {
                    if (it.Current.Value.Transform == null)
                        continue;

                    switch ((eArenaPos)it.Current.Key)
                    {
                        case eArenaPos.Offense1Top:
                            if (dic.ContainsKey((int)eArenaPos.Offense1Bot))
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 0f, DefaultY + SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 0f, DefaultY, 0f);
                            break;
                        case eArenaPos.Offense1Bot:
                            if (dic.ContainsKey((int)eArenaPos.Offense1Top))
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 0f, DefaultY - SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 0f, DefaultY, 0f);
                            break;
                        case eArenaPos.Offense2Top:
                            if (dic.ContainsKey((int)eArenaPos.Offense2Bot))
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 1f, DefaultY + SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 1f, DefaultY, 0f);
                            break;
                        case eArenaPos.Offense2Bot:
                            if (dic.ContainsKey((int)eArenaPos.Offense2Top))
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 1f, DefaultY - SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 1f, DefaultY, 0f);
                            break;
                        case eArenaPos.Offense3Top:
                            if (dic.ContainsKey((int)eArenaPos.Offense3Bot))
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 2f, DefaultY + SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 2f, DefaultY, 0f);
                            break;
                        case eArenaPos.Offense3Bot:
                            if (dic.ContainsKey((int)eArenaPos.Offense3Top))
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 2f, DefaultY - SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 2f, DefaultY, 0f);
                            break;
                        case eArenaPos.Defense1Top:
                            if (dic.ContainsKey((int)eArenaPos.Defense1Bot))
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 0f, DefaultY + SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 0f, DefaultY, 0f);
                            break;
                        case eArenaPos.Defense1Bot:
                            if (dic.ContainsKey((int)eArenaPos.Defense1Top))
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 0f, DefaultY - SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 0f, DefaultY, 0f);
                            break;
                        case eArenaPos.Defense2Top:
                            if (dic.ContainsKey((int)eArenaPos.Defense2Bot))
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 1f, DefaultY + SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 1f, DefaultY, 0f);
                            break;
                        case eArenaPos.Defense2Bot:
                            if (dic.ContainsKey((int)eArenaPos.Defense2Top))
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 1f, DefaultY - SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 1f, DefaultY, 0f);
                            break;
                        case eArenaPos.Defense3Top:
                            if (dic.ContainsKey((int)eArenaPos.Defense3Bot))
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 2f, DefaultY + SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 2f, DefaultY, 0f);
                            break;
                        case eArenaPos.Defense3Bot:
                            if (dic.ContainsKey((int)eArenaPos.Defense3Top))
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 2f, DefaultY - SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 2f, DefaultY, 0f);
                            break;
                    }
                }
            }
        }

        public JObject GetResultData()
        {
            JObject result = new JObject();
            foreach (var it in OffenseDic)
            {
                var dragon = it.Value;
                if (dragon == null) continue;
                if (result.ContainsKey(dragon.Position.ToString())) continue;

                JObject resultDragon = new JObject();
                resultDragon.Add("dtag", dragon.ID);
                resultDragon.Add("death", dragon.Death);
                result.Add(dragon.Position.ToString(), resultDragon);
            }
            return result;
        }

        public void Set(JObject jsonData, bool isMock = false)
        {
            if (!SBFunc.IsJTokenType(jsonData["err"], JTokenType.Integer) || (eApiResCode)jsonData["err"].Value<int>() != eApiResCode.OK)
                return;

            Initialize();

            Speed = PlayerPrefs.GetFloat("ArenaSpeed", 1.0f);
            Time = 0;

            IsMock = isMock;

            if (SBFunc.IsJTokenType(jsonData["maxTime"], JTokenType.Float))
                MaxTime = (float)jsonData["maxTime"];
            else
                MaxTime = GameConfigTable.GetArenaBattleTime();

            //로그 클라 전투로 바뀌면서 받는 쪽이아니라 보내는 쪽으로 변경.
            if (SBFunc.IsJTokenType(jsonData["log"], JTokenType.Object) && jsonData["log"].HasValues)
            {
                LogDatas = new List<LogStruct>();
                foreach (JProperty data in jsonData["log"])
                {
                    List<string> Log = new(data.Value.Values<string>());
                    LogDatas.Add(new LogStruct(int.Parse(data.Name), Log));
                }
                LogDatas.Sort((e1, e2) =>
                {
                    if (e1 == null || e2 == null) return -1;
                    return e1.time - e2.time;
                });
            }

            if (SBFunc.IsJTokenType(jsonData["nick"], JTokenType.String))
            {
                EnemyNick = jsonData["nick"].Value<string>();
            }

            //제한구역 점령전시에는 nick은 내닉네임이 온다.
            if(SBFunc.IsJTokenType(jsonData["ctrl_user_nick"], JTokenType.String))
            {
                EnemyNick = jsonData["ctrl_user_nick"].Value<string>();
            }
            //승패 클라 전투로 바뀌면서 클라 결정으로 변경
            //if (SBFunc.IsJTokenType(jsonData["win"], JTokenType.Integer))
            //{
            //    WinType = (eArenaWinType)jsonData["win"].Value<int>();
            //}
            winType = eArenaWinType.None;

            if (SBFunc.IsJArray(jsonData["off"]))
            {
                var Offences = (JArray)jsonData["off"];
                for (int i = 0, count = Offences.Count; i < count; ++i)
                {
                    if (!SBFunc.IsJObject(Offences[i]))
                        continue;

                    if (!SBFunc.IsJTokenType(Offences[i]["id"], JTokenType.Integer) || Offences[i]["id"].Value<int>() <= 0)
                        continue;

                    var data = new ArenaDragonData();
                    data.SetData((int)eArenaPos.TEAM1 + i, (JObject)Offences[i]);
                    AddOffenseDragon(data);
                }
            }
            if (SBFunc.IsJArray(jsonData["def"]))
            {
                var Defences = (JArray)jsonData["def"];
                for (int i = 0, count = Defences.Count; i < count; ++i)
                {
                    if (!SBFunc.IsJObject(Defences[i]))
                        continue;

                    if (!SBFunc.IsJTokenType(Defences[i]["id"], JTokenType.Integer) || Defences[i]["id"].Value<int>() <= 0)
                        continue;

                    var data = new ArenaDragonData();
                    data.SetData((int)eArenaPos.TEAM2 + i, (JObject)Defences[i]);
                    AddDefenseDragon(data);
                }
            }

            //if (SBFunc.IsJArray(jsonData["stats"]))
            //{
            //    var statsArray = jsonData["stats"].ToObject<JArray>();
            //    Stats = new List<ArenaResultDragonStat>();
            //    for (int i = 0, count = statsArray.Count; i < count; ++i)
            //    {
            //        if (!DragonsDic.ContainsKey(curPos))
            //            continue;

            //        Stats.Add(new ArenaResultDragonStat(curPos, DragonsDic[curPos].ID, DragonsDic[curPos].Level, statsArray[i].ToObject<JArray>()));
            //    }
            //}
            if (SBFunc.IsJTokenType(jsonData["obtainpoint"], JTokenType.Integer))
            {
                RewardTrophy = (int)jsonData["obtainpoint"];
            }

            if (SBFunc.IsJTokenType(jsonData["revenge"], JTokenType.Boolean))
                IsRevenge = jsonData["revenge"].Value<bool>();
            else
                IsRevenge = false;
        }

        public IEnumerator StartLoadingCO()
        {
            var loadDatas = new Dictionary<eResourcePath, List<string>>();//로딩 리소스 파일 리스트 확보
            foreach (var data in OffenseDic.Values)
            {
                var baseData = data.BaseData;
                if (baseData == null)
                    continue;
                
                SBFunc.AddedResourceKey(loadDatas, eResourcePath.DragonClonePath, baseData.IMAGE);
                SBFunc.SkillDataSetLoadingList(loadDatas, baseData.NORMAL_SKILL);
                SBFunc.SkillDataSetLoadingList(loadDatas, baseData.SKILL1);
                SBFunc.SkillDataSetLoadingList(loadDatas, baseData.SKILL2);

                SBFunc.PassiveDataSetLoadingList(loadDatas, data.TranscendenceData);
            }
            foreach (var data in DefenseDic.Values)
            {
                var baseData = data.BaseData;
                if (baseData == null)
                    continue;

                SBFunc.AddedResourceKey(loadDatas, eResourcePath.DragonClonePath, baseData.IMAGE);
                SBFunc.SkillDataSetLoadingList(loadDatas, baseData.NORMAL_SKILL);
                SBFunc.SkillDataSetLoadingList(loadDatas, baseData.SKILL1);
                SBFunc.SkillDataSetLoadingList(loadDatas, baseData.SKILL2);

                SBFunc.PassiveDataSetLoadingList(loadDatas, data.TranscendenceData);
            }

            yield return ResourceManager.LoadAsyncPaths(loadDatas);


        }


        public void SetResultData(JObject jsonData)
        {
            if (SBFunc.IsJTokenType(jsonData["points"], JTokenType.Integer))
            {
                RewardTrophy = (int)jsonData["points"];
            }
            if (SBFunc.IsJTokenType(jsonData["reward_point"], JTokenType.Integer))
            {
                RewardPoint = (int)jsonData["reward_point"];
            }
            if (SBFunc.IsJTokenType(jsonData["reward_account_exp"], JTokenType.Integer))
            {
                RewardAccountExp = (int)jsonData["reward_account_exp"];
            }
            if (SBFunc.IsJTokenType(jsonData["reward_guild_exp"], JTokenType.Integer))
            {
                RewardGuildExp = (int)jsonData["reward_guild_exp"];
            }
            //승패 클라 전투로 바뀌면서 클라 결정으로 변경
            if (SBFunc.IsJTokenType(jsonData["win"], JTokenType.Integer))
            {
                winType = (eArenaWinType)jsonData["win"].Value<int>();
            }
            ArenaManager.Instance.UserArenaData.SetData(jsonData);
        }

        public override bool CheckTimeOver()
        {
            if (Time >= MaxTime)
            {
                State = eBattleState.TimeOver;
                //WinType = eArenaWinType.Draw;
                return true;
            }
            return false;
        }

        public override void SetData(JObject jsonData)
        {
        }
        public void SetWinType(eArenaWinType type)
        {
        }
        public override void Update(float dt)
        {
            UpdateGlobalDelay(dt);
        }

        public float OffHPRate()
        {
            if (OffMaxHP.Count <= 0)
                return 1.0f;

            float ret = 0.0f;

            float perRate = 1f / OffMaxHP.Count;
            for (int i = 0; i < OffMaxHP.Count; i++)
            {
                ret += ((float)OffHP[i] / OffMaxHP[i]) * perRate;
            }

            return ret;
        }

        public float DefHPRate()
        {
            if (DefMaxHP.Count <= 0)
                return 1.0f;

            float ret = 0.0f;

            float perRate = 1f / DefMaxHP.Count;
            for (int i = 0; i < DefMaxHP.Count; i++)
            {
                ret += ((float)DefHP[i] / DefMaxHP[i]) * perRate;
            }

            return ret;
        }
    }
}