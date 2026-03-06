using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScriptTriggerTable : TableBase<ScriptTriggerData, DBScript_trigger>
{
    Dictionary<ScriptTriggerType, List<ScriptTriggerData>> typeDic = new Dictionary<ScriptTriggerType, List<ScriptTriggerData>>();
    Dictionary<int, ScriptTriggerData> seqDic = new Dictionary<int, ScriptTriggerData>();
    public override void DataClear()
    {
        base.DataClear();
        typeDic.Clear();
        seqDic.Clear();
    }
    public override void Preload()
    {
        base.Preload();        
    }

    public void OrganizeTrigger()
    {
        typeDic.Clear();
        seqDic.Clear();

        LoadAll();

        Dictionary<ScriptTriggerType, List<string>> ignores = new Dictionary<ScriptTriggerType, List<string>>();
        foreach (var data in datas.Values)
        {
            if (data.SEQ > 0)
            {
                seqDic[data.SEQ] = data;
            }
            else
            {
                if (!ignores.ContainsKey(data.TYPE))
                {
                    ignores.Add(data.TYPE, CacheUserData.GetString("SCRIPT_" + data.TYPE.ToString(), "").Split(",").ToList());
                }

                if (ignores[data.TYPE].Contains(data.GetKey())) //참조할일 없으니 빼뻐리자
                    continue;

                if (!typeDic.ContainsKey(data.TYPE))
                    typeDic.Add(data.TYPE, new List<ScriptTriggerData>());

                typeDic[data.TYPE].Add(data);
            }
        }
    }
    public ScriptTriggerData GetSeq(int seq)
    {
        if (seqDic.ContainsKey(seq))
        {
            return seqDic[seq];
        }

        return null;
    }

    public List<ScriptTriggerData> GetTriggerList(ScriptTriggerType type)
    {
        if(typeDic.ContainsKey(type))
        {
            return typeDic[type];
        }

        return new List<ScriptTriggerData>();
    }

}
public class ScriptGroupTable : TableBase<ScriptGroupData, DBScript_group>
{
    Dictionary<int, List<ScriptGroupData>> groupDic = new Dictionary<int, List<ScriptGroupData>>();
    public override void Init()
    {
        base.Init();
        groupDic.Clear();
    }
    public override void DataClear()
    {
        base.DataClear();
        groupDic.Clear();
    }
    public override void Preload()
    {
        base.Preload();
        LoadAll();
    }
    protected override bool Add(ScriptGroupData data)
    {
        if (base.Add(data))
        {
            data.Init();
            if (false == groupDic.TryGetValue(data.GROUP_ID, out var value))
            {
                value = new();
                groupDic.Add(data.GROUP_ID, value);
            }
            value.Add(data);
            return true;
        }
        return false;
    }
    public List<ScriptGroupData> GetGroup(int group)
    {
        if (groupDic.ContainsKey(group))
            return groupDic[group];

        return null;
    }
}
public class ScriptObjectTable : TableBase<ScriptObjectData, DBScript_object>
{
    public override void Preload()
    {
        base.Preload();

        LoadAll();
        foreach (var data in datas.Values)
        {
            data.Init();
        }
    }
}
