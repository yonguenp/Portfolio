using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBossLevelTable : TableBase<WorldBossLevelData, DBRaid_boss_level>
{
    Dictionary<int, Dictionary<int, WorldBossLevelData>> bossTable = new Dictionary<int, Dictionary<int, WorldBossLevelData>>();

    public override void DataClear()
    {
        base.DataClear();
        bossTable.Clear();
    }
    public override void Preload()
    {
        base.Preload();

        LoadAll();

        foreach (WorldBossLevelData data in datas.Values)
        {
            if (false == bossTable.ContainsKey(data.BOSS_KEY))
                bossTable.Add(data.BOSS_KEY, new());

            bossTable[data.BOSS_KEY][data.LEVEL] = data;
        }
    }

    public WorldBossLevelData GetLevelData(int bosskey, int level)
    {
        if(bossTable.ContainsKey(bosskey))
        {
            if(bossTable[bosskey].ContainsKey(level))
            {
                return bossTable[bosskey][level];
            }
        }

        return null;
    }
}


public class WorldBossLevelData : TableData<DBRaid_boss_level>
{
    static private WorldBossLevelTable table = null;

    static public WorldBossLevelData Get(int boss_key, int level)
    {
        if (table == null)
            table = TableManager.GetTable<WorldBossLevelTable>();
        return table.GetLevelData(boss_key, level);
    }

    public int BOSS_KEY => Data.MONSTER_KEY;
    public int LEVEL => Data.LEVEL;
    public int NEED_DMG => (int)(Data.NEED_DMG * ServerOptionData.GetJsonValueFloat("raid_balance", "step", 1.0f));
    public string REWARD_DESC => Data.REWARD_DESC;
}

public class WorldBossPartTable : TableBase<WorldBossPartData, DBRaid_boss_parts>
{
    Dictionary<int, Dictionary<int, List<WorldBossPartData>>> bossTable = new Dictionary<int, Dictionary<int, List<WorldBossPartData>>>();
    Dictionary<int, List<WorldBossPartData>> partsGroup = new Dictionary<int, List<WorldBossPartData>>();
    public override void DataClear()
    {
        base.DataClear();
        bossTable.Clear();
    }
    public override void Preload()
    {
        base.Preload();

        LoadAll();

        foreach (var data in datas.Values)
        {
            if (!bossTable.ContainsKey(data.BOSS_KEY))
                bossTable.Add(data.BOSS_KEY, new Dictionary<int, List<WorldBossPartData>>());

            if (!bossTable[data.BOSS_KEY].ContainsKey(data.ACTIVE_LEVEL))
                bossTable[data.BOSS_KEY][data.ACTIVE_LEVEL] = new List<WorldBossPartData>();

            bossTable[data.BOSS_KEY][data.ACTIVE_LEVEL].Add(data);

            if (data.GROUP > 0)
            {
                if (!partsGroup.ContainsKey(data.GROUP))
                    partsGroup.Add(data.GROUP, new List<WorldBossPartData>());

                partsGroup[data.GROUP].Add(data);
            }
        }
    }

    public List<WorldBossPartData> GetLevelData(int bosskey, int level)
    {
        if (bossTable.ContainsKey(bosskey))
        {
            if (bossTable[bosskey].ContainsKey(level))
            {
                return bossTable[bosskey][level];
            }
        }

        return new List<WorldBossPartData>();
    }
    public List<WorldBossPartData> GetGroupData(int group)
    {
        if (partsGroup.ContainsKey(group))
        {
            return partsGroup[group];
        }

        return new List<WorldBossPartData>();
    }
}

public class WorldBossPartData : TableData<DBRaid_boss_parts>
{
    static private WorldBossPartTable table = null;

    static public WorldBossPartData Get(int key)
    {
        if (table == null)
            table = TableManager.GetTable<WorldBossPartTable>();

        return table.Get(key);
    }

    static public List<WorldBossPartData> GetLevelData(int boss_key, int level)
    {
        if (table == null)
            table = TableManager.GetTable<WorldBossPartTable>();
        return table.GetLevelData(boss_key, level);
    }

    static public List<WorldBossPartData> GetGroup(int group)
    {
        if (table == null)
            table = TableManager.GetTable<WorldBossPartTable>();
        return table.GetGroupData(group);
    }

    public int KEY => Int(Data.UNIQUE_KEY);
    public int BOSS_KEY => Data.BOSS_KEY;
    public int PARTS_TYPE => Data.PARTS_TYPE;
    public int ACTIVE_LEVEL => Data.ACTIVE_LEVEL;
    public int GROUP => Data.GROUP;
    public int TARGET_PRIORITY => Data.TARGET_PRIORITY;
    public int ATTACK_PRIORITY => Data.ATTACK_PRIORITY;
    
    public List<BattleSpine> GetTargets(List<List<BattleSpine>> outTargets)
    {
        List<BattleSpine> targets = new();
        if (outTargets == null)
            return targets;

        var outCount = outTargets.Count;
        if ((TARGET_PRIORITY & eWorldBoss.PRIORITY_BOTTON_LEFT) > 0)
        {
            if (outCount > eWorldBoss.POS_BOTTOM_LEFT)
                targets.AddRange(outTargets[eWorldBoss.POS_BOTTOM_LEFT].FindAll(DeathCheck));
        }
        if ((TARGET_PRIORITY & eWorldBoss.PRIORITY_BOTTON_RIGHT) > 0)
        {
            if (outCount > eWorldBoss.POS_BOTTOM_RIGHT)
                targets.AddRange(outTargets[eWorldBoss.POS_BOTTOM_RIGHT].FindAll(DeathCheck));
        }
        if ((TARGET_PRIORITY & eWorldBoss.PRIORITY_TOP_LEFT) > 0)
        {
            if (outCount > eWorldBoss.POS_TOP_LEFT)
                targets.AddRange(outTargets[eWorldBoss.POS_TOP_LEFT].FindAll(DeathCheck));
        }
        if ((TARGET_PRIORITY & eWorldBoss.PRIORITY_TOP_RIGHT) > 0)
        {
            if (outCount > eWorldBoss.POS_TOP_RIGHT)
                targets.AddRange(outTargets[eWorldBoss.POS_TOP_RIGHT].FindAll(DeathCheck));
        }
        if ((TARGET_PRIORITY & eWorldBoss.PRIORITY_LEFT) > 0)
        {
            if (outCount > eWorldBoss.POS_BOTTOM_LEFT)
            {
                targets.AddRange(outTargets[eWorldBoss.POS_BOTTOM_LEFT].FindAll(DeathCheck));
                if (targets.Count == 0 && outCount > eWorldBoss.POS_TOP_LEFT)
                {
                    targets.AddRange(outTargets[eWorldBoss.POS_TOP_LEFT].FindAll(DeathCheck));
                }
            }
        }
        if ((TARGET_PRIORITY & eWorldBoss.PRIORITY_RIGHT) > 0)
        {
            if (outCount > eWorldBoss.POS_BOTTOM_RIGHT)
            {
                targets.AddRange(outTargets[eWorldBoss.POS_BOTTOM_RIGHT].FindAll(DeathCheck));
                if (targets.Count == 0 && outCount > eWorldBoss.POS_TOP_RIGHT)
                {
                    targets.AddRange(outTargets[eWorldBoss.POS_TOP_RIGHT].FindAll(DeathCheck));
                }
            }
        }

        return targets;
    }
    private bool DeathCheck(BattleSpine spine)
    {
        return spine != null && spine.Data.Death == false;
    }
    public List<int> GetAttackPartyIndexs()
    {
        List<int> targets = new();
        if ((ATTACK_PRIORITY & eWorldBoss.PRIORITY_BOTTON_LEFT) > 0)
        {
            targets.Add(eWorldBoss.POS_BOTTOM_LEFT);
        }
        if ((ATTACK_PRIORITY & eWorldBoss.PRIORITY_BOTTON_RIGHT) > 0)
        {
            targets.Add(eWorldBoss.POS_BOTTOM_RIGHT);
        }
        if ((ATTACK_PRIORITY & eWorldBoss.PRIORITY_TOP_LEFT) > 0)
        {
            targets.Add(eWorldBoss.POS_TOP_LEFT);
        }
        if ((ATTACK_PRIORITY & eWorldBoss.PRIORITY_TOP_RIGHT) > 0)
        {
            targets.Add(eWorldBoss.POS_TOP_RIGHT);
        }
        if ((ATTACK_PRIORITY & eWorldBoss.PRIORITY_LEFT) > 0)
        {
            targets.Add(eWorldBoss.POS_BOTTOM_LEFT);
            targets.Add(eWorldBoss.POS_TOP_LEFT);
        }
        if ((ATTACK_PRIORITY & eWorldBoss.PRIORITY_RIGHT) > 0)
        {
            targets.Add(eWorldBoss.POS_BOTTOM_RIGHT);
            targets.Add(eWorldBoss.POS_TOP_RIGHT);
        }
        return targets;
    }
}