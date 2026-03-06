using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScriptTriggerData : TableData<DBScript_trigger>
{
    static private ScriptTriggerTable table = null;

    static public ScriptTriggerData Get(int key)
    {
        if (table == null)
            table = TableManager.GetTable<ScriptTriggerTable>();
        return table.Get(key);
    }
    static public ScriptTriggerData GetSeq(int seq)
    {
        if (table == null)
            table = TableManager.GetTable<ScriptTriggerTable>();

        return table.GetSeq(seq);
    }
    static public List<ScriptTriggerData> GetTriggerList(ScriptTriggerType type)
    {
        if (table == null)
            table = TableManager.GetTable<ScriptTriggerTable>();

        List<ScriptTriggerData> ret = new List<ScriptTriggerData>();
        var list = table.GetTriggerList(type);
        if (list.Count > 0)
        {
            string strTrigger = CacheUserData.GetString("SCRIPT_" + type.ToString());
            List<string> triggers = strTrigger.Split(',').ToList();

            foreach (var data in list)
            {
                if (triggers.Contains(data.UNIQUE_KEY))
                    continue;

                ret.Add(data);
            }
        }

        return ret;
    }

    public int KEY => Int(Data.UNIQUE_KEY);
    public int SEQ => Data.seq;
    public ScriptTriggerType TYPE => (ScriptTriggerType)Data.trigger_type;
    public int TYPE_PARAM => Data.trigger_param;
    public int GROUP => Data.script_group;

    public List<ScriptGroupData> Child { get { return ScriptGroupData.GetGroup(GROUP); } }    
}
public class ScriptGroupData : TableData<DBScript_group>
{
    static private ScriptGroupTable table = null;

    static public List<ScriptGroupData> GetGroup(int key)
    {
        if (table == null)
            table = TableManager.GetTable<ScriptGroupTable>();

        return table.GetGroup(key);
    }
    public enum UI_TYPE
    {
        UNKNOWN = -1,
        NORMAL = 1,
        SHOUT = 2,
        MONOLOGUE = 3,
        TUTORIAL = 4,
    }

    public enum OBJECT_POS
    { 
        LEFT = 0,
        RIGHT = 1,
        CENTER = 2,
        MAX = 3,
    }

    public enum OBJECT_STATE
    {
        UNKNOWN = -1,
        NONE = 0,
        SPEAK = 1,
        DIM = 2,
        NORMAL = 3,
    }

    
    public int KEY => Int(Data.UNIQUE_KEY);
    public int GROUP_ID => Data.group;
    public float DURATION => Data.duration * SBDefine.CONVERT_THOUSAND;
    public UI_TYPE UI => (UI_TYPE)Data.script_UI;

    private ScriptObjectData[] objects = null;
    public ScriptObjectData[] OBJECTS { get { if (objects == null) LoadObject(); return objects; } }
    public OBJECT_STATE[] STATES = new OBJECT_STATE[(int)OBJECT_POS.MAX];
    public string BG_resource => Data.BG_resource;
    public string BGM_resource => Data.BGM_resource;
    public string TEXT 
    { 
        get {
            var data = ScriptStringData.Get(KEY);
            if (data == null)
                return string.Empty;

            return ScriptStringData.Get(KEY).TEXT; 
        } 
    }

    public override void Init()
    {
        base.Init();

        STATES[(int)OBJECT_POS.LEFT] = (OBJECT_STATE)Data.object_effect_1;
        STATES[(int)OBJECT_POS.RIGHT] = (OBJECT_STATE)Data.object_effect_2;
        STATES[(int)OBJECT_POS.CENTER] = (OBJECT_STATE)Data.object_effect_3;
    }    

    public void LoadObject()
    {
        objects = new ScriptObjectData[(int)OBJECT_POS.MAX];
        objects[(int)OBJECT_POS.LEFT] = ScriptObjectData.Get(Data.object_key_1);
        objects[(int)OBJECT_POS.RIGHT] = ScriptObjectData.Get(Data.object_key_2);
        objects[(int)OBJECT_POS.CENTER] = ScriptObjectData.Get(Data.object_key_3);        
    }
}
public class ScriptObjectData : TableData<DBScript_object>
{
    static private ScriptObjectTable table = null;
    static public ScriptObjectData Get(int key)
    {
        if (table == null)
            table = TableManager.GetTable<ScriptObjectTable>();

        return table.Get(key);
    }

    public enum OBJECT_TYPE { 
        UNKNOWN = -1,
        NONE = 0,
        DRAGON = 1,
        MONSTER = 2,
        ITEM = 3,        
        GROUP = 4,
        ETC = 5,
    }

    private string name => Data.name;
    public string NAME { get { return StringData.GetStringByStrKey(name); } }
    public Color COLOR { get; private set; } = Color.white;
    public OBJECT_TYPE TYPE => (OBJECT_TYPE)Data.type;
    public Vector2 SCALE { get; private set; } = Vector2.zero;
    public Vector2 POSITION { get; private set; } = Vector2.zero;
    public int RESOURCE_ID => Data.resource;
    public string PARAM => Data.param;
    public override void Init()
    {
        base.Init();

        if(false == ColorUtility.TryParseHtmlString("#" + Data.color, out var clr))
            clr = Color.white;
        COLOR = clr;

        SCALE = new Vector2(Data.scale_x, Data.scale_y) * SBDefine.CONVERT_THOUSAND;
        POSITION = new Vector2(Data.pos_x, Data.pos_y);
    }
}

public class ScriptStringData : TableData<DBScript_string>
{
    static public ScriptStringData Get(int key)
    {
        return StringDataManager.Instance.GetScript(key.ToString());
    }
    public int KEY { get { return Int(UNIQUE_KEY); } }
    public string TEXT => SBFunc.Replace(Data.TEXT);
}
