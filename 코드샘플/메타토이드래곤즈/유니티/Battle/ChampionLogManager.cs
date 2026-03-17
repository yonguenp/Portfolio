using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using SandboxNetwork;
using System.IO.Compression;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System.Collections;

[Serializable]
public class DamageLog
{
    public int ID;
    public int TARGET;
    public float TIME;
    public int VALUE;



    public DamageLog(IBattleCharacterData caster, IBattleCharacterData target, float time, int val)
    {
        ID = caster.ID;
        TARGET = target.ID;
        TIME = time;
        VALUE = val;
        
    }

    //public string DumpStr()
    //{
    //    //return "T:" + TIME.ToString() + ",  R:" + REASON + ",  V:" + VALUE.ToString();
    //}
}

[Serializable]
public class RandomLog
{    
    public enum RandomReason
    {
        NONE,
        EffectTriggerTarget1,
        EffectTriggerTarget2,
        EffectTriggerLand,
        Damage,
        DotSkillDamage,
        SummonTrigger,
        RapidTrigger,
        PassiveRate,
    }
    public RandomReason randomReason;

    public float TIME;
    public int VALUE;
    public RandomLog(float time, int val, RandomReason reason)
    {
        TIME = time;
        VALUE = val;
        randomReason = reason;
    }

    //public string DumpStr()
    //{
    //    //return "T:" + TIME.ToString() + ",  R:" + REASON + ",  V:" + VALUE.ToString();
    //}
}

[Serializable]
public class Loger
{
    public int RandomSeed = 0;

    public ChampionUserInfo UserA;
    public ChampionUserInfo UserB;

    public Queue<DamageLog> DamageLog = new Queue<DamageLog>();
    public Queue<RandomLog> RandomLog = new Queue<RandomLog>();
    public Queue<FrameLog> FrameLog = new Queue<FrameLog>();
    public StatisticsLog Statistics = null;

    public List<int> PDR = new List<int>();
    public List<int> AEDR = new List<int>();
    public List<int> CDR = new List<int>();
    public void Clear()
    {
        DamageLog.Clear();
        FrameLog.Clear();
        RandomLog.Clear();
    }

    public FrameLog PopFrameLog()
    {
        if (FrameLog.Count > 0)
            return FrameLog.Dequeue();

        return null;
    }
}

[Serializable]
public class FrameLog
{
    public float Time;
    public FrameLog(float t)
    {
        Time = t;
    }

    public void AddLog(ActionLog log)
    {
        switch(log.side)
        {
            case eBattleSide.OffenseSide_1:
                SideAActions.Add(log.ID, log);
                break;
            case eBattleSide.DefenseSide_1:
                SideBActions.Add(log.ID, log);
                break;
        }
    }

    public ActionLog GetActionLogA(int id)
    {
        if(SideAActions.ContainsKey(id))
            return SideAActions[id];
        return null;
    }

    public ActionLog GetActionLogB(int id)
    {
        if (SideBActions.ContainsKey(id))
            return SideBActions[id];
        return null;
    }

    public Dictionary<int, ActionLog> SideAActions = new Dictionary<int, ActionLog>();
    public Dictionary<int, ActionLog> SideBActions = new Dictionary<int, ActionLog>();

    //public string Dump()
    //{
    //return "Time:" +Time+ Actions.Dump();
    //}
}

[Serializable]
public class ActionLog
{
    public enum ActionType
    {
        None,
        Attack,
        Skill,
        Move,
        Skip,
        SetPos,
    }

    public eBattleSide side;
    public int ID;
    public ActionType actionType;
    
    public float posx;
    public float posy;
    public float posz;

    public Vector3 GetPosition() { return new Vector3(posx, posy, posz); }

    public ActionLog(IBattleCharacterData data, ActionType a)
    {
        side = data.IsEnemy ? eBattleSide.DefenseSide_1 : eBattleSide.OffenseSide_1;
        ID = data.ID;
        actionType = a;


        posx = data.Transform.position.x;
        posy = data.Transform.position.y;
        posz = data.Transform.position.z;
    }

    public virtual string Dump()
    {
        return "Side: " + side + "ID: " + ID + "ActionType:" + actionType;
    }
}

[Serializable]
public class ActtackActionLog : ActionLog
{
    public int Caster;
    public int SkillKey;
    public int SummonKey;
    public bool IsActioning;
    public eBattleSkillType SkillType;

    public SBSkill GetSkill(IBattleCharacterData data)
    {
        return new SBSkill(data, SkillCharData.Get(SkillKey), SkillSummonData.Get(SummonKey), SkillType);
    }
    public ActtackActionLog(IBattleCharacterData data, SBSkill s)
        : base(data, ActionType.Attack)
    {
        Caster = s.Caster.ID;
        SkillKey = s.Skill.KEY;
        SummonKey = s.GetSummon().KEY;
        SkillType = s.SkillType;
        IsActioning = data.IsActioning;
    }

    public override string Dump()
    {
        return base.Dump();
    }
}

[Serializable]
public class SkillActionLog : ActtackActionLog
{
    public SkillActionLog(IBattleCharacterData data, SBSkill s)
        : base(data, s)
    {
        actionType = ActionType.Skill;
    }

    public override string Dump()
    {
        return base.Dump();
    }
}


[Serializable]
public class MoveActionLog : ActionLog
{
    public float goalx { get; private set; }
    public float goaly { get; private set; }
    public float goalz { get; private set; }

    public Vector3 goal { get { return new Vector3(goalx, goaly, goalz); } }

    public bool IsActioning;
    public MoveActionLog(IBattleCharacterData data, Vector3 goal)
       : base(data, ActionType.Move)
    {
        goalx = goal.x;
        goaly = goal.y;
        goalz = goal.z;
        IsActioning = data.IsActioning;
    }

    public override string Dump()
    {
        return base.Dump();
    }
}

[Serializable]
public class SkipActionLog : ActionLog
{

    public bool isKnockback;
    public SkipActionLog(IBattleCharacterData data)
        : base(data, ActionType.Skip)
    {
        isKnockback = data.IsEffectInfo(eSkillEffectType.KNOCK_BACK);
    }

    public override string Dump()
    {
        return base.Dump();
    }
}

[Serializable]
public class SetPosActionLog : MoveActionLog
{
    public SetPosActionLog(IBattleCharacterData data, Vector3 goal)
    : base(data, goal)
    {
        actionType = ActionType.SetPos;
    }

    public override string Dump()
    {
        return base.Dump();
    }
}

[Serializable]
public class StatisticsLog
{
    public eChampionWinType WinType;
    public float Time;

    public Dictionary<int, StatisticsInfo> SideA;
    public Dictionary<int, StatisticsInfo> SideB;
    public StatisticsLog(eChampionWinType winType, float time, Dictionary<int, StatisticsInfo> a, Dictionary<int, StatisticsInfo> b)    
    {
        WinType = winType;
        Time = time;
        SideA = a;
        SideB = b;
    }
}

public class ChampionWinUserInfo : ChampionUserInfo, ITableData
{
    public int SEASON;
    public int SERVER = -1;
    public int[] Dragons = new int[20];
    public virtual void Init() { }
    public string GetKey() { return UID.ToString(); }
    public PortraitEtcInfoData EtcInfo { get; set; } = null;
    public ChampionWinUserInfo(JObject data, string servername)
        : base(data)
    {
        SEASON = data["season_id"].Value<int>();
        switch(servername)
        {
            case "ANGEL":
                SERVER = 1;
                break;
            case "WONDER":
                SERVER = 2;
                break;
            case "LUNA":
                SERVER = 3;
                break;
            case "UNIFIED":
                SERVER = 0;
                break;
        }

        JArray dragonarray = (JArray)data["dragons"];
        for (int i = 0; i < dragonarray.Count; i++)
        {
            if(Dragons.Length > i)
                Dragons[i] = dragonarray[i].Value<int>();
        }

        if (data.ContainsKey("portrait"))
        {
            EtcInfo = new PortraitEtcInfoData(data["portrait"]);
        }
    }

    public override ThumbnailUserData GetThumnailData()
    {
        return new ThumbnailUserData(UID, Nick, PortraitIcon, Level, EtcInfo);
    }
}

[Serializable]
public class ChampionUserInfo
{
    public long UID;
    public string Nick;
    public string PortraitIcon;
    public int Level;    
    public int GuildNo;
    public string GuildName;
    public int GuildMarkNo;
    public int GuildEmblemNo;
    public int Server;

    public string deck;
    public bool HasGuild { get { return GuildNo > 0; } }
    public JObject Teams { get { return JObject.Parse(deck); } }
    public JArray OffenceTeam { get { return Teams.ContainsKey("off") && Teams["off"].Type == JTokenType.Array ? (JArray)Teams["off"] : null; } }
    public JArray DefenceTeam { get { return Teams.ContainsKey("def") && Teams["def"].Type == JTokenType.Array ? (JArray)Teams["def"] : null; } }
    public JArray HiddenTeam { get { return Teams.ContainsKey("hid") && Teams["hid"].Type == JTokenType.Array ? (JArray)Teams["hid"] : null; } }

    public ChampionUserInfo(JObject data)
    {
        UID = data["user_no"].Value<long>();

        if(data.ContainsKey("name"))
            Nick = data["name"].Value<string>();
        if (data.ContainsKey("nick"))
            Nick = data["nick"].Value<string>();

        PortraitIcon = data["icon"].Value<string>();
        Level = data["level"].Value<int>();
        GuildNo = data["guild_no"].Value<int>();
        GuildName = data["guild_name"].Value<string>();
        GuildMarkNo = data["mark_no"].Value<int>();
        GuildEmblemNo = data["emblem_no"].Value<int>();

        if(data.ContainsKey("deck"))
            deck = data["deck"].ToString();
        if (data.ContainsKey("server_id"))
            Server = data["server_id"].Value<int>();
    }

    public virtual ThumbnailUserData GetThumnailData()
    {
        return new ThumbnailUserData(UID, Nick, PortraitIcon, Level);
    }
}

public partial class ChampionManager
{
    public Loger CurLoger { get; private set; } = new Loger();

    public System.Random Random { get; private set; } = null;

    public Queue<DamageLog> DamageLog { get { return CurLoger.DamageLog; } }
    public Queue<RandomLog> RandomLog { get { return CurLoger.RandomLog; } }
    public StatisticsLog Statistics { get { return CurLoger.Statistics; } }
        
    public void LogInit()
    {
        SBFunc.SetRandomSeed(CurLoger.RandomSeed);
        Random = new System.Random(CurLoger.RandomSeed);

        Application.targetFrameRate = 30;
        Time.fixedDeltaTime = 1f / 30;
    }

    public FrameLog PopFrameLog()
    {
        return CurLoger.PopFrameLog();
    }

    public void LogRelease()
    {
        SBFunc.ReleaseRandomSeed();

        Application.targetFrameRate = 60;
        Time.fixedDeltaTime = 1f / 60;
    }

    public int OnDamage(IBattleCharacterData caster, IBattleCharacterData target, int val, float time)
    {
        int ret = val;

        if (!ChampionManager.Instance.Playing)
            return ret;

        if (DamageLog.Count > 0)
        {
            var cur = DamageLog.Dequeue();
            if (cur.ID != caster.ID || cur.TARGET != target.ID)
            {
                Debug.LogError("diff info");
                //ForceExitWithErrPopup();
            }

            if (cur.TIME != time)
            {
                Debug.LogError("diff time");
                //ForceExitWithErrPopup();
            }

            if (cur.VALUE != val)
            {
                Debug.LogWarning("diff damage : record - " + cur + " calculate - " + ret);
                //부동소수점이나 오류로 인해 조금의 오차가 발생하기도 하더라
                //ForceExitWithErrPopup();
            }

            ret = cur.VALUE;
        }
        else
        {
            Debug.LogError("index missing");
            //ForceExitWithErrPopup();
        }

        return ret;
    }

    public int OnRandomLog(float time, int val, RandomLog.RandomReason reason)
    {
        int ret = val;

        if (!ChampionManager.Instance.Playing)
            return ret;

        if (RandomLog.Count > 0)
        {
            var cur = RandomLog.Dequeue();            
            if (cur.randomReason != reason)
            {
                Debug.LogError("diff reason");
                //ForceExitWithErrPopup();
            }

            if (cur.VALUE != val)
            {
                Debug.LogError("diff random val");
                //ForceExitWithErrPopup();
            }

            if (cur.TIME != time)
            {
                Debug.LogError("diff time : " + (cur.TIME - time));
                //ForceExitWithErrPopup();
            }

            ret = cur.VALUE;
        }
        else
        {
            if(!IsMatchDone())
                Debug.LogError("index missing");
            //ForceExitWithErrPopup();
        }

        return ret;
    }

    bool IsMatchDone()
    {
        if (championBattleData.Time >= championBattleData.MaxTime)
            return true;

        int offhp = 0;
        int defhp = 0;
        foreach (var off in championBattleData.OffHP)
        {
            offhp += off;
        }

        foreach (var def in championBattleData.DefHP)
        {
            defhp += def;
        }

        return offhp <= 0 || defhp <= 0;
    }

    public void DeleteLog(string path)
    {

#if UNITY_EDITOR
        string dirPath = Application.streamingAssetsPath + "/log/";
#else
        string dirPath = Application.persistentDataPath + "/log/";
#endif
        string fullpath = dirPath + path;
        if (File.Exists(fullpath))
        {
            File.Delete(fullpath);

            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true);
            }
        }
    }

    public IEnumerator LoadLog(string path, Action cb, Action failcb)
    {
        CurLoger = null;

#if UNITY_EDITOR
        string dirPath = Application.streamingAssetsPath + "/log/";
#else
        string dirPath = Application.persistentDataPath + "/log/";
#endif
        string fullpath = dirPath + path;

        if (!File.Exists(fullpath))
        {
            Debug.Log("리플레이 다운로드 !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            UnityWebRequest wr = UnityWebRequest.Get("https://d2efgqatv3752r.cloudfront.net/" + "log/" + path);
            yield return wr.SendWebRequest();

            if (wr.result == UnityWebRequest.Result.Success)
            {
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                File.WriteAllBytes(fullpath, wr.downloadHandler.data);
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                failcb.Invoke();
            }
        }

        if (File.Exists(fullpath))
        {
            using (FileStream fs = new FileStream(fullpath, FileMode.Open))
            using (GZipStream gzip = new GZipStream(fs, CompressionMode.Decompress))
            {
                BinaryFormatter bf = new BinaryFormatter();
                CurLoger = (Loger)bf.Deserialize(gzip);

                cb.Invoke();
            }
        }
        else
        {
            failcb.Invoke();
        }
    }

 
}