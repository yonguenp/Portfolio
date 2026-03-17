using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ChampionBattleBattleData : BattleData
    {
        #region IBattleDataChampionBattle
        public override float Speed { get; set; } = 1.0f;
        public override bool IsAuto => true;
        public override float Time { get; set; } = 0f;
        public override eBattleType BattleType { get => eBattleType.ChampionBattle; }
        public override int Wave { get; protected set; } = 1;
        public override int MaxWave { get; protected set; } = 1;
        #endregion
        #region ChampionBattleData 

        public virtual ChampionUserInfo UserA { get { return ChampionManager.Instance.CurLoger.UserA; } }
        public virtual ChampionUserInfo UserB { get { return ChampionManager.Instance.CurLoger.UserB; } }
        public virtual string ASideNick { get { return UserA.Nick; } }
        public virtual string BSideNick { get { return UserB.Nick; } }
        public List<int> OffMaxHP { get; private set; } = new List<int>();
        public List<int> DefMaxHP { get; private set; } = new List<int>();
        public float MaxTime { get; private set; } = 180f;

        public virtual float BattleTime { get { return ChampionManager.Instance.CurLoger.Statistics.Time; } }
        public virtual Dictionary<int, StatisticsInfo> GetStatisticsInfo(bool left) { return left ? ChampionManager.Instance.CurLoger.Statistics.SideA : ChampionManager.Instance.CurLoger.Statistics.SideB; }
        public virtual eChampionWinType WinType
        {
            get
            {
                return ChampionManager.Instance.CurLoger.Statistics.WinType;
            }
        }

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

            IsAuto = false;

            Speed = 1.0f;

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

                    switch ((eChampionBattlePos)it.Current.Key)
                    {
                        case eChampionBattlePos.Offense1Top:
                            if (dic.ContainsKey((int)eChampionBattlePos.Offense1Bot))
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 0f, DefaultY + SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 0f, DefaultY, 0f);
                            break;
                        case eChampionBattlePos.Offense1Bot:
                            if (dic.ContainsKey((int)eChampionBattlePos.Offense1Top))
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 0f, DefaultY - SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 0f, DefaultY, 0f);
                            break;
                        case eChampionBattlePos.Offense2Top:
                            if (dic.ContainsKey((int)eChampionBattlePos.Offense2Bot))
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 1f, DefaultY + SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 1f, DefaultY, 0f);
                            break;
                        case eChampionBattlePos.Offense2Bot:
                            if (dic.ContainsKey((int)eChampionBattlePos.Offense2Top))
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 1f, DefaultY - SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 1f, DefaultY, 0f);
                            break;
                        case eChampionBattlePos.Offense3Top:
                            if (dic.ContainsKey((int)eChampionBattlePos.Offense3Bot))
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 2f, DefaultY + SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 2f, DefaultY, 0f);
                            break;
                        case eChampionBattlePos.Offense3Bot:
                            if (dic.ContainsKey((int)eChampionBattlePos.Offense3Top))
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 2f, DefaultY - SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(-DefaultX - SpancingX * 2f, DefaultY, 0f);
                            break;
                        case eChampionBattlePos.Defense1Top:
                            if (dic.ContainsKey((int)eChampionBattlePos.Defense1Bot))
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 0f, DefaultY + SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 0f, DefaultY, 0f);
                            break;
                        case eChampionBattlePos.Defense1Bot:
                            if (dic.ContainsKey((int)eChampionBattlePos.Defense1Top))
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 0f, DefaultY - SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 0f, DefaultY, 0f);
                            break;
                        case eChampionBattlePos.Defense2Top:
                            if (dic.ContainsKey((int)eChampionBattlePos.Defense2Bot))
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 1f, DefaultY + SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 1f, DefaultY, 0f);
                            break;
                        case eChampionBattlePos.Defense2Bot:
                            if (dic.ContainsKey((int)eChampionBattlePos.Defense2Top))
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 1f, DefaultY - SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 1f, DefaultY, 0f);
                            break;
                        case eChampionBattlePos.Defense3Top:
                            if (dic.ContainsKey((int)eChampionBattlePos.Defense3Bot))
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 2f, DefaultY + SpancingY, 0f);
                            else
                                it.Current.Value.Transform.localPosition = new Vector3(DefaultX + SpancingX * 2f, DefaultY, 0f);
                            break;
                        case eChampionBattlePos.Defense3Bot:
                            if (dic.ContainsKey((int)eChampionBattlePos.Defense3Top))
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

        public void Set(JArray a_side, JArray b_side)
        {
            Initialize();

            Speed = 1.0f;
            Time = 0;

            MaxTime = GameConfigTable.GetArenaBattleTime();

            if (SBFunc.IsJArray(a_side))
            {
                var Offences = a_side;
                for (int i = 0, count = Offences.Count; i < count; ++i)
                {
                    if (!SBFunc.IsJObject(Offences[i]))
                        continue;

                    if (!SBFunc.IsJTokenType(Offences[i]["id"], JTokenType.Integer) || Offences[i]["id"].Value<int>() <= 0)
                        continue;

                    var data = new ChampionBattleDragonData();
                    data.SetData((int)eChampionBattlePos.TEAM1 + i, (JObject)Offences[i]);
                    AddOffenseDragon(data);
                }
            }

            if (SBFunc.IsJArray(b_side))
            {
                var Defences = b_side;

                for (int i = 0, count = Defences.Count; i < count; ++i)
                {
                    if (!SBFunc.IsJObject(Defences[i]))
                        continue;

                    if (!SBFunc.IsJTokenType(Defences[i]["id"], JTokenType.Integer) || Defences[i]["id"].Value<int>() <= 0)
                        continue;

                    var data = new ChampionBattleDragonData();
                    data.SetData((int)eChampionBattlePos.TEAM2 + i, (JObject)Defences[i]);
                    AddDefenseDragon(data);
                }
            }
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
        public void SetWinType(eChampionWinType type)
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

    public class PracticeBattleData : ChampionBattleBattleData
    {
        public override ChampionUserInfo UserA { get { return null; } }
        public override ChampionUserInfo UserB { get { return null; } }
        public override string ASideNick { get { return StringData.GetStringByStrKey("left_side"); } }
        public override string BSideNick { get { return StringData.GetStringByStrKey("right_side"); } }

        public override Dictionary<int, StatisticsInfo> GetStatisticsInfo(bool left) { return left ? StatisticsMananger.Instance.myDamageDic : StatisticsMananger.Instance.enemyDamageDic; }
        public override float BattleTime { get { return Time; } }
        public override eChampionWinType WinType
        {

            get
            {
                if (CheckTimeOver())
                {
                    if (OffHPRate() > DefHPRate())
                        return eChampionWinType.SIDE_A_WIN;
                    else
                        return eChampionWinType.SIDE_B_WIN;
                }
                else
                {
                    var offDeathCount = 0;
                    foreach (var offenseInfo in OffenseDic)
                    {
                        if (offenseInfo.Value == null || offenseInfo.Value.Death)
                            ++offDeathCount;
                    }

                    var defDeathCount = 0;
                    foreach (var defenseInfo in DefenseDic)
                    {
                        if (defenseInfo.Value == null || defenseInfo.Value.Death)
                            ++defDeathCount;
                    }

                    bool a_alldie = OffenseDic.Count == offDeathCount;
                    bool b_alldie = DefenseDic.Count == defDeathCount;

                    if (a_alldie && b_alldie)
                        return eChampionWinType.Open;

                    if (a_alldie)
                        return eChampionWinType.SIDE_B_WIN;

                    if (b_alldie)
                        return eChampionWinType.SIDE_A_WIN;
                }

                //비김
                return eChampionWinType.Open;
            }
        }
    }
}