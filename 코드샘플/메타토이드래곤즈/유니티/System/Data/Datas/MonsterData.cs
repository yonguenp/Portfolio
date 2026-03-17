using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class MonsterBaseData : TableData<DBMonster_base>, ICharacterBaseData
    {
        static private MonsterBaseTable table = null;
        private Sprite thumbnail = null;
        private static Dictionary<string, GameObject> dataPrefabDic = new Dictionary<string, GameObject>();
        static public MonsterBaseData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<MonsterBaseTable>();

            return table.Get(key);
        }
        static public MonsterBaseData Get(int key)
        {
            return Get(key.ToString());
        }
        static public MonsterBaseData GetMonsterDataByImageName(string _skinName)
        {
            if (table == null)
                table = TableManager.GetTable<MonsterBaseTable>();

            return table.GetMonsterDataByImageName(_skinName);
        }

        public int KEY { get { return Int(UNIQUE_KEY); } }
        public int FACTOR { get; private set; } = -1;
        public int GRADE { get; private set; } = -1;
        public int ELEMENT { get; private set; } = -1;
        public eElementType ELEMENT_TYPE { get; private set; } = eElementType.None;
        public float SIZE { get; private set; } = -1;
        public string IMAGE { get; private set; } = "";
        public string THUMBNAIL { get; private set; } = "";
        public string SKIN { get; private set; } = "";
        public string _NAME { get; private set; } = "";
        public string _DESC { get; private set; } = "";
        public int ATK { get; private set; } = -1;
        public int DEF { get; private set; } = -1;
        public int HP { get; private set; } = -1;
        public float CRI_PROC { get; private set; } = 0.0f;
        public int CRI_DMG { get; private set; } = 0;
        public int LIGHT_DMG { get; private set; } = 0;
        public int DARK_DMG { get; private set; } = 0;
        public int WATER_DMG { get; private set; } = 0;
        public int FIRE_DMG { get; private set; } = 0;
        public int WIND_DMG { get; private set; } = 0;
        public int EARTH_DMG { get; private set; } = 0;
        public int ADD_PVP_DMG { get; private set; } = 0;
        public float RATIO_PVP_DMG { get; private set; } = 0.0f;
        public int ADD_PVP_CRI_DMG { get; private set; } = 0;
        public float RATIO_PVP_CRI_DMG { get; private set; } = 0.0f;
        public float ADD_ATKSPEED { get; private set; } = 0.0f;
        public float MOVE_SPEED { get; private set; } = 0.0f;
        public string SCRIPT_OBJECT_KEY { get; private set; } = "";
        public bool IsMonster { get { return true; } }
        private int CC_IMN { get; set; } = 0;
        public eCharacterImmunity Immunity { get; private set; } = eCharacterImmunity.NONE;

        private long normal_skill = -1;
        private long skill1 = -1;
        private long skill2 = -1;

        private SkillCharData[] skill_data = new SkillCharData[3];
        public SkillCharData NORMAL_SKILL
        {
            get
            {
                if (skill_data[0] == null)
                    skill_data[0] = SkillCharData.Get(normal_skill.ToString());

                return skill_data[0];
            }
        }
        public SkillCharData SKILL1
        {
            get
            {
                if (skill_data[1] == null)
                    skill_data[1] = SkillCharData.Get(skill1.ToString());

                return skill_data[1];
            }
        }
        public SkillCharData SKILL2
        {
            get
            {
                if (skill_data[2] == null)
                    skill_data[2] = SkillCharData.Get(skill2.ToString());

                return skill_data[2];
            }
        }

        public GameObject GetPrefab()
        {
            if (!dataPrefabDic.ContainsKey(IMAGE))
            {
                dataPrefabDic[IMAGE] = ResourceManager.GetResource<GameObject>(eResourcePath.MonsterClonePath, IMAGE);
            }
            return dataPrefabDic[IMAGE];

        }
        public override void SetData(DBMonster_base data)
        {
            base.SetData(data);

            FACTOR = Data.FACTOR;
            GRADE = Data.GRADE;
            ELEMENT = Data.ELEMENT;
            ELEMENT_TYPE = (eElementType)ELEMENT;
            SIZE = Data.SIZE;
            IMAGE = Data.IMAGE;
            SKIN = Data.SKIN;
            THUMBNAIL = Data.THUMBNAIL;
            _NAME = Data._NAME;
            _DESC = Data._DESC;
            ATK = Data.ATK;
            DEF = Data.DEF;
            HP = Data.HP;
            CRI_PROC = Data.CRI_PROC;
            CRI_DMG = Data.CRI_DMG;
            LIGHT_DMG = Data.LIGHT_DMG;
            DARK_DMG = Data.DARK_DMG;
            WATER_DMG = Data.WATER_DMG;
            FIRE_DMG = Data.FIRE_DMG;
            WIND_DMG = Data.WIND_DMG;
            EARTH_DMG = Data.EARTH_DMG;
            ADD_ATKSPEED = Data.ADD_ATKSPEED;
            normal_skill = Data.NORMAL_SKILL;
            skill1 = Data.SKILL1;
            skill2 = Data.SKILL2;
            MOVE_SPEED = Data.MOVE_SPEED * SBDefine.CONVERT_FLOAT;
            SCRIPT_OBJECT_KEY = Data.SCRIPT_OBJECT_KEY;
            CC_IMN = Data.CC_IMN;
            Immunity = (eCharacterImmunity)CC_IMN;
        }
        public Sprite GetThumbnail()
        {
            if (thumbnail == null)
            {
                thumbnail = ResourceManager.GetResource<Sprite>(eResourcePath.CharIconPath, THUMBNAIL);
                if (thumbnail == null)
                    thumbnail = CharBaseTable.DefaultThumbnail;
            }
            return thumbnail;
        }
    }

    public class MonsterSpawnData : TableData<DBMonster_spawn>
    {
        static private MonsterSpawnTable table = null;
        static public MonsterSpawnData GetKey(int key)
        {
            if (table == null)
                table = TableManager.GetTable<MonsterSpawnTable>();

            return table.Get(key);
        }
        static public List<MonsterSpawnData> GetBySpawnGroup(int spawnGroup)
        {
            if (table == null)
                table = TableManager.GetTable<MonsterSpawnTable>();

            return table.GetBySpawnGroup(spawnGroup);
        }

        public int KEY { get { return Int(UNIQUE_KEY); } }
        public int SPAWN_GROUP => Data.SPAWN_GROUP;
        public int WAVE => Data.WAVE;
        public int GROUP => Data.GROUP;
        public int POSITION => Data.POSITION;
        public int MONSTER => Data.MONSTER;
        public int IS_BOSS => Data.IS_BOSS;
        public float SCALE => Data.SCALE;
        public int LEVEL => Data.LEVEL;
        public int RATE => Data.RATE;
        public int INF => Data.INF;
        public int IS_MOVE => Data.IS_MOVE;
    }
}