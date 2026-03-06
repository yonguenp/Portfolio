using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace SandboxNetwork
{

    [Serializable]
    public class StatisticsInfo
    {
        public int ID = 0; // 드래곤 ID
        public long RecieveDmgInShield = 0; // 내 쉴드가 막은 피해량
        public long RecieveDmgReal = 0; // 실제 내 채력이 깎인 피해량
        public int SkillCount = 0; // 스킬 사용 횟수
        public int NormalAtkCount = 0; // 평타 사용 횟수
        public long TotalDmg = 0;
        public long SkillTotalDmg = 0; // 스킬딜 총합
        public int SkillTotalDmgToShield = 0; // 쉴드에 가한 스킬피해량
        public long NormalAtkTotalDmg = 0; // 평타딜 총합
        public int NormalTotalDmgToShield = 0;  //쉴드에 가한 평타 피해량
        public int AliveTime = -1; // 생존시간 //   -1 : 죽지 않음 
        public int BestSkillDmg = 0; // 가장 강한 스킬 딜
        public int BestNormalAtkDmg = 0; // 가장 강한 평타 딜
        public int BossDmg = 0; // 보스에게 준 대미지

        public StatisticsInfo(int id)
        {
            ID = id;
        }
        public void AddAtkDmg(int dmg, bool isSkill, int shieldAtkDmg = 0)
        {
            if(isSkill)
            {
                SkillTotalDmg += dmg;
                SkillTotalDmgToShield += shieldAtkDmg;
                if (dmg > BestSkillDmg)
                    BestSkillDmg = dmg;
            }
            else
            {
                NormalAtkTotalDmg += dmg;
                NormalTotalDmgToShield += shieldAtkDmg;
                if (dmg > BestNormalAtkDmg)
                    BestNormalAtkDmg = dmg;
            }
            TotalDmg += dmg;
        }
        public void AddBossDmg(int dmg)
        {
            BossDmg += dmg;
        }

        public string DumpStr()
        {
            return "ID :" + ID.ToString() + "   ,쉴막 :" + RecieveDmgInShield.ToString() + "   ,받은피해량 :" + RecieveDmgReal.ToString() + "   ,평타횟수 :" + NormalAtkCount.ToString() + "   ,스킬사용횟수 :" + SkillCount.ToString() + "   , 뎀:" + TotalDmg.ToString() + ",    스킬뎀:" + SkillTotalDmg.ToString() +
                ",    쉴드에 가한 스킬뎀:" + SkillTotalDmgToShield.ToString() + ",    평타뎀:" + NormalAtkTotalDmg.ToString() + ",    쉴드에 가한 평타뎀:" + NormalTotalDmgToShield.ToString() + ",    생존시간:" + AliveTime.ToString() + ",    가장강한스킬뎀:" + BestSkillDmg.ToString() +
                 ",    가장강한평타뎀:" + BestNormalAtkDmg.ToString() + ",    보스뎀:" + BossDmg.ToString();
        }
    }
    public class StatisticsMananger
    {
        private static StatisticsMananger instance;
        public static StatisticsMananger Instance
        {
            get{
                if(instance == null)
                {
                    instance = new StatisticsMananger();
                }
                return instance;
            }
        }
        public Dictionary<int, StatisticsInfo> myDamageDic { get; private set; }  = new Dictionary<int, StatisticsInfo>();
        public Dictionary<int, StatisticsInfo> enemyDamageDic { get; private set; } = new Dictionary<int, StatisticsInfo>();

        private eUIStatisticsContentType contentType;
        public void InitStatistic(eUIStatisticsContentType type)
        {
            contentType = type;
            myDamageDic.Clear();
            enemyDamageDic.Clear(); 
        }
        public void AddDamage(int casterID,int targetID, int damangeAmount, bool isCasterEnemy, bool isSkill,int shieldAtkDmg)
        {
            UIStatisticEvent.Send(damangeAmount, casterID, targetID, isSkill, isCasterEnemy);
            if (isCasterEnemy)
            {
                if (enemyDamageDic.ContainsKey(casterID)==false)
                {
                    enemyDamageDic.Add(casterID, new StatisticsInfo(casterID));
                }
                enemyDamageDic[casterID].AddAtkDmg(damangeAmount, isSkill, shieldAtkDmg);
            }
            else
            {
                if (myDamageDic.ContainsKey(casterID)==false)
                {
                    myDamageDic.Add(casterID, new StatisticsInfo(casterID));
                }                
                myDamageDic[casterID].AddAtkDmg(damangeAmount, isSkill, shieldAtkDmg);
            }
        }
        public void AddBossDamage(int casterID, int damangeAmount, bool isCasterEnemy)
        {
            if (isCasterEnemy)
            {
                if (enemyDamageDic.ContainsKey(casterID) == false)
                {
                    enemyDamageDic.Add(casterID, new StatisticsInfo(casterID));
                }
                enemyDamageDic[casterID].AddBossDmg(damangeAmount);
            }
            else
            {
                if (myDamageDic.ContainsKey(casterID) == false)
                {
                    myDamageDic.Add(casterID, new StatisticsInfo(casterID));
                }
                myDamageDic[casterID].AddBossDmg(damangeAmount);
            }
        }
        public void AddAtkCount(int casterID,bool isCasterEnemy, bool isSkill)
        {
            UIStatisticEvent.Send(casterID, isCasterEnemy, isSkill);
            if (isCasterEnemy)
            {
                if (enemyDamageDic.ContainsKey(casterID) == false)
                {
                    enemyDamageDic.Add(casterID, new StatisticsInfo(casterID));
                }
                if (isSkill)
                    enemyDamageDic[casterID].SkillCount += 1;
                else
                    enemyDamageDic[casterID].NormalAtkCount += 1;
            }
            else
            {
                if (myDamageDic.ContainsKey(casterID) == false)
                {
                    myDamageDic.Add(casterID, new StatisticsInfo(casterID));
                }
                if (isSkill)
                    myDamageDic[casterID].SkillCount += 1;
                else
                    myDamageDic[casterID].NormalAtkCount += 1;
            }
        }
        public void AddRecieveDmg(int targetID, int recievedDmg, bool isTargetEnemy, bool isShield)
        {
            UIStatisticEvent.Send(targetID, isTargetEnemy, recievedDmg, isShield);
            if (isTargetEnemy)
            {
                if (enemyDamageDic.ContainsKey(targetID) == false)
                {
                    enemyDamageDic.Add(targetID, new StatisticsInfo(targetID));
                }
                if (isShield)
                    enemyDamageDic[targetID].RecieveDmgInShield += recievedDmg;
                else
                    enemyDamageDic[targetID].RecieveDmgReal += recievedDmg;
            }
            else
            {
                if (myDamageDic.ContainsKey(targetID) == false)
                {
                    myDamageDic.Add(targetID, new StatisticsInfo(targetID));
                }
                if (isShield)
                    myDamageDic[targetID].RecieveDmgInShield += recievedDmg;
                else
                    myDamageDic[targetID].RecieveDmgReal += recievedDmg;
            }
        }
        public void SetDeath(int targetID, bool isTargetEnemy)
        {
            int aliveTime = 0;
            switch (contentType)
            {
                case eUIStatisticsContentType.Arena:
                    aliveTime = (int)ArenaManager.Instance.ColosseumData.Time;
                    break;
                case eUIStatisticsContentType.Adventure:
                    aliveTime = (int)AdventureManager.Instance.Data.Time;
                    break;
                case eUIStatisticsContentType.DailyDungeon:
                    aliveTime = (int)DailyManager.Instance.Data.Time;
                    break;
                case eUIStatisticsContentType.WorldBoss:
                    aliveTime = (int)WorldBossManager.Instance.Data.Time;
                    break;
                case eUIStatisticsContentType.ChampionBattle:
                    aliveTime = (int)ChampionManager.Instance.ChampionData.Time;
                    break;
            }
            UIStatisticEvent.Send(targetID, isTargetEnemy, aliveTime);
            if (isTargetEnemy)
            {
                if (enemyDamageDic.ContainsKey(targetID) == false)
                {
                    enemyDamageDic.Add(targetID, new StatisticsInfo(targetID));
                }
                enemyDamageDic[targetID].AliveTime = aliveTime;
            }
            else
            {
                if (myDamageDic.ContainsKey(targetID) == false)
                {
                    myDamageDic.Add(targetID, new StatisticsInfo(targetID));
                }
                myDamageDic[targetID].AliveTime = aliveTime;
            }
        }

        public void SetAliveTime()
        {
            List<IBattleCharacterData> characters = null;
            int aliveTime = 0;
            switch (contentType)
            {
                case eUIStatisticsContentType.Arena:
                    aliveTime = (int)ArenaManager.Instance.ColosseumData.Time;
                    characters = ArenaManager.Instance.ColosseumData.Characters;
                    break;
                case eUIStatisticsContentType.Adventure:
                    aliveTime = (int)AdventureManager.Instance.Data.Time;
                    characters = AdventureManager.Instance.Data.Characters;
                    break;
                case eUIStatisticsContentType.DailyDungeon:
                    aliveTime = (int)DailyManager.Instance.Data.Time;
                    characters = DailyManager.Instance.Data.Characters;
                    break;
                case eUIStatisticsContentType.WorldBoss:
                    aliveTime = (int)WorldBossManager.Instance.Data.Time;
                    characters = WorldBossManager.Instance.Data.Characters;
                    break;
                case eUIStatisticsContentType.ChampionBattle:
                    aliveTime = (int)ChampionManager.Instance.ChampionData.Time;
                    characters = ChampionManager.Instance.ChampionData.Characters;
                    break;
            }

            if (characters == null || characters.Count <= 0)
                return;

            foreach(var character in characters)
            {
                if (character == null)
                    continue;

                if (!character.Death && myDamageDic.ContainsKey(character.ID))
                    myDamageDic[character.ID].AliveTime = aliveTime;
            }
        }

        public string GetDamageInfoString(bool isMine)
        {
            JObject json = new JObject();

            if (isMine)
            {
                foreach (int dragonNo in myDamageDic.Keys)
                {
                    var info = myDamageDic[dragonNo];
                    json.Add(dragonNo.ToString(), new JArray(){ info.TotalDmg, info.SkillCount, info.NormalAtkCount, info.BestSkillDmg, info.BestNormalAtkDmg, info.RecieveDmgReal, info.AliveTime });
                }
            }
            else
            {
                foreach (int dragonNo in enemyDamageDic.Keys)
                {
                    var info = enemyDamageDic[dragonNo];
                    json.Add(dragonNo.ToString(), new JArray() { info.TotalDmg, info.SkillCount, info.NormalAtkCount, info.BestSkillDmg, info.BestNormalAtkDmg, info.RecieveDmgReal, info.AliveTime });
                }
            }
            
            return json.ToString(Formatting.None);
        }
        public string GetWorldBossDamageInfo(bool isMine)
        {
            JObject json = new JObject();

            if (isMine)
            {
                foreach (int dragonNo in myDamageDic.Keys)
                {
                    var info = myDamageDic[dragonNo];
                    json.Add(dragonNo.ToString(), new JArray() { info.BossDmg, info.SkillCount, info.NormalAtkCount, info.BestSkillDmg, info.BestNormalAtkDmg, info.RecieveDmgReal, info.AliveTime });
                }
            }
            else
            {
                foreach (int dragonNo in enemyDamageDic.Keys)
                {
                    var info = enemyDamageDic[dragonNo];
                    json.Add(dragonNo.ToString(), new JArray() { info.BossDmg, info.SkillCount, info.NormalAtkCount, info.BestSkillDmg, info.BestNormalAtkDmg, info.RecieveDmgReal, info.AliveTime });
                }
            }

            return json.ToString(Formatting.None);
        }
        public byte[] GetDamageInfoBinary(int dragonNo, bool isMine = true) 
        {
            if (isMine)
            {
                if(myDamageDic.ContainsKey(dragonNo)==false)
                    return new byte[0];
            }
            else
            {
                if (enemyDamageDic.ContainsKey(dragonNo) == false)
                    return new byte[0];
            }

            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(stream, isMine? myDamageDic[dragonNo] : enemyDamageDic[dragonNo]);
                    return stream.ToArray();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            return new byte[0];
        }
    }
}

