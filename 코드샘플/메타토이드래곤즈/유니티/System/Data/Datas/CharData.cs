using Newtonsoft.Json.Linq;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class CharBaseData : TableData<DBChar_base>, ICharacterBaseData, ISpineCharacterData
    {
        static private CharBaseTable table = null;

        private GameObject prefab = null;
        private GameObject uiPrefab = null;
        private SkeletonDataAsset dataAsset = null;
        private Sprite thumbnail = null;
        private Sprite classIcon = null;
        private Sprite spriteBackGround = null;
        private string BackGroundName = "";

        private static Dictionary<string, SkeletonDataAsset> dataAssetDic = new Dictionary<string, SkeletonDataAsset>();
        private static Dictionary<string, GameObject> dataPrefabDic = new Dictionary<string, GameObject>();
        private static Dictionary<SkeletonDataAsset, GameObject> dataDragonDic = new Dictionary<SkeletonDataAsset, GameObject>();
        static public CharBaseData Get(int key) 
        {
            return Get(key.ToString());
        }
        static public CharBaseData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<CharBaseTable>();

            return table.Get(key);
        }

        static public CharBaseData GetDataBySkin(string SkinName)
        {
            if (table == null)
                table = TableManager.GetTable<CharBaseTable>();

            return table.GetDataBySkin(SkinName);
        }

        static public List<CharBaseData> GetAllList()
        {
            if (table == null)
                table = TableManager.GetTable<CharBaseTable>();

            return table.GetAllList();
        }

        static public Dictionary<string, CharBaseData> GetAllDic()
        {
            if (table == null)
                table = TableManager.GetTable<CharBaseTable>();

            return table.GetAllDic();
        }

        static public List<CharBaseData> GetAllForChampion()
        {
            if (table == null)
                table = TableManager.GetTable<CharBaseTable>();

            List<CharBaseData> ret = new List<CharBaseData>();
            foreach (var data in table.GetGradeAll((int)eDragonGrade.Legend))
            {
                if(data.USE_CHAMPION)
                {
                    ret.Add(data);
                }
            }

            return ret;
        }

        /// <summary>
        /// 데이터 테이블 상의 총 노출 드래곤 갯수 구하기
        /// </summary>
        /// <param name="_includeInUse"></param> // inUse 플래그는 0이면 미포함, 1은 포함임.(_include 가 true면 그냥 토탈 숫자)
        /// <returns></returns>
        static public int GetTotalDragonCount(bool _includeInUse = false)
        {
            if (table == null)
                table = TableManager.GetTable<CharBaseTable>();

            return table.GetTotalDragonCount(_includeInUse);
        }

        public int KEY => Int(Data.UNIQUE_KEY);
        public int FACTOR { get; private set; } = -1;
        public int GRADE { get; private set; } = -1;
        public int ELEMENT { get; private set; } = -1;
        public eElementType ELEMENT_TYPE { get; private set; } = eElementType.None;
        public string BACKGROUND { get; private set; } = "";
        public string IMAGE { get; private set; } = "";
        public string SPINE_NAME { get; private set; } = "";
        public string THUMBNAIL { get; private set; } = "";
        public string SKIN { get; private set; } = "";
        public eJobType JOB { get; private set; } = eJobType.NONE;
        public int POSITION { get; private set; } = -1;
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
        public bool IsMonster { get { return false; } }
        public float SIZE { get { return 1f; } }
        public int use_flag = 0;
        public bool IS_USE { get { return use_flag > 0; } }//드래곤 보유 목록 표시용 (0 = 미표시 1 표시)
        public bool IS_GACHA { get { return use_flag == 1; } }
        public bool IS_SCENARIO { get { return use_flag == 2; } }
        public bool IS_CASH { get { return use_flag == 3; } }
        public bool IS_LEVEL_PASS_REWARD { get { return use_flag == 4; } }//레벨 패스 보상 드래곤
        public bool IS_GUILD_SHOP_REWARD { get { return use_flag == 5; } }//길드 상점 전용 드래곤
        public bool IS_LIMITED { get { return use_flag == 6; } }//이벤트 한정 드래곤
        private int CC_IMN { get; set; } = 0;
        public eCharacterImmunity Immunity { get; private set; } = eCharacterImmunity.NONE;

        public float OFFSET { get; private set; } = 0.0f;
        public bool USE_CHAMPION => Data.USE_CHAMPION > 0;

        private int normal_skill = -1;
        private int skill1 = -1;
        private int skill2 = -1;

        private SkillCharData[] skill_data = new SkillCharData[3];
        public SkillCharData NORMAL_SKILL
        {
            get
            {
                if (skill_data[0] == null)
                    skill_data[0] = SkillCharData.GetBySkillID(normal_skill);

                return skill_data[0];
            }
        }
        public SkillCharData SKILL1 {
            get {
                if (skill_data[1] == null)
                    skill_data[1] = SkillCharData.GetBySkillID(skill1);

                return skill_data[1];
            } 
        }
        public SkillCharData SKILL2
        {
            get
            {
                if (skill_data[2] == null)
                    skill_data[2] = SkillCharData.GetBySkillID(skill2);

                return skill_data[2];
            }
        }

        public string SCRIPT_OBJECT_KEY { get; private set; } = "";

        public override void SetData(DBChar_base datas)
        {
            base.SetData(datas);

            FACTOR = Data.FACTOR;
            GRADE = Data.GRADE;
            ELEMENT = Data.ELEMENT;
            ELEMENT_TYPE = (eElementType)ELEMENT;
            BACKGROUND = Data.BACKGROUND;
            IMAGE = Data.IMAGE;
            SPINE_NAME = Data.SPINE_NAME;
            THUMBNAIL = Data.THUMBNAIL;
            SKIN = Data.SKIN;
            JOB = SBFunc.GetJobType(Data.JOB);
            POSITION = Data.POSITION;
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
            ADD_PVP_DMG = Data.ADD_PVP_DMG;
            RATIO_PVP_DMG = Data.RATIO_PVP_DMG;
            ADD_PVP_CRI_DMG = Data.ADD_PVP_CRI_DMG;
            RATIO_PVP_CRI_DMG = Data.RATIO_PVP_CRI_DMG;
            ADD_ATKSPEED = Data.ADD_ATKSPEED;
            MOVE_SPEED = Data.MOVE_SPEED * SBDefine.CONVERT_FLOAT;
            normal_skill = Data.NORMAL_SKILL;
            skill1 = Data.SKILL1;
            skill2 = Data.SKILL2;
            SCRIPT_OBJECT_KEY = Data.SCRIPT_OBJECT_KEY;
            use_flag = Data.IN_USE;
            OFFSET = Data.OFFSET;

            //드래곤은 기본 면역이 없음
            CC_IMN = 0;
            Immunity = (eCharacterImmunity)CC_IMN;

            //preload
            GetSkeletonDataAsset();
        }

        public static SkeletonDataAsset GetSkeletonDataAsset(string name)
        {
            if (!dataAssetDic.ContainsKey(name) || dataAssetDic[name] == null)
            {
                dataAssetDic[name] = ResourceManager.GetResource<SkeletonDataAsset>(eResourcePath.DragonSkeletonDataPath, SBFunc.StrBuilder(name, "_SkeletonData"));
            }

            return dataAssetDic[name];
        }
        public SkeletonDataAsset GetSkeletonDataAsset()
        {
            if (dataAsset == null)
                dataAsset = GetSkeletonDataAsset(SPINE_NAME);

            return dataAsset;
        }
        public GameObject GetDefaultSpine()
        {
            SkeletonDataAsset asset = GetSkeletonDataAsset();
            if (false == dataDragonDic.TryGetValue(asset, out var cache))
            {
                GameObject cacheObject = GetPrefab();
                cache = Object.Instantiate(cacheObject, Game.Instance.transform);
                if (false == cache.TryGetComponent<CharacterSpine>(out var spine))
                    spine = cache.AddComponent<CharacterSpine>();

                spine.SetData(this);
                cache.SetActive(false);
                dataDragonDic.Add(asset, cache);
            }

            return cache;
        }
        public TownDragonSpine LoadTownDragonSpine(UserDragon data, Transform parent)
        {
            GameObject dragonObj = Object.Instantiate(GetDefaultSpine(), parent);
            dragonObj.SetActive(true);
            TownDragonSpine dragon = dragonObj.GetComponent<TownDragonSpine>();
            SBFunc.SetLayer(dragonObj, "town_dragon");
            if (dragon == null)
            {
                dragon = dragonObj.AddComponent<TownDragonSpine>();
            }
            dragon.SetData(data);
            dragon.SetActive(false);

            return dragon;
        }
        public GameObject GetPrefab()
        {
            if(!dataPrefabDic.ContainsKey(IMAGE))
            {
                dataPrefabDic[IMAGE] = ResourceManager.GetResource<GameObject>(eResourcePath.DragonClonePath, IMAGE);
            }

            if (prefab == null)
                prefab = dataPrefabDic[IMAGE];

            return prefab;
        }
        public GameObject GetUIPrefab()
        {
            if (uiPrefab == null)
            {
                uiPrefab = ResourceManager.GetResource<GameObject>(eResourcePath.UIDragonClonePath, IMAGE);
            }

            return uiPrefab;
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
        public Sprite GetClassIcon()
        {
            if (classIcon == null)
            {
                classIcon = ResourceManager.GetResource<Sprite>(eResourcePath.ClassIconPath, string.Format("icon_class_{0}", SBFunc.ConvertJobTypeToString(JOB)));
                if (classIcon == null)
                    classIcon = CharBaseTable.DefaultClassIcon;
            }
            return classIcon;
        }
        public Sprite GetBackGround()
        {
            if (spriteBackGround == null)
            {
                spriteBackGround = ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, GetBackGroundName());
                if (spriteBackGround == null)
                    spriteBackGround = CharBaseTable.DefaultBackGround;
            }

            return spriteBackGround;
        }
        public string GetBackGroundName()
        {
            if (BackGroundName == "NONE")
            {
                if (BACKGROUND == "NONE")
                    BackGroundName = string.Format("{0}_{1}_infobg", SBFunc.GetElementConvertString(ELEMENT), CharGradeData.GetGradeConvertString(GRADE));
                else
                    BackGroundName = BACKGROUND;
            }

            return BackGroundName;
        }
    }

    public class CharGradeData : TableData<DBChar_grade>
    {
        static private CharGradeTable table = null;
        static public CharGradeData Get(int key)
        {
            return Get(key.ToString());
        }
        static public CharGradeData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<CharGradeTable>();

            return table.Get(key);
        }
        public string KEY => Data.UNIQUE_KEY;
        public int _NAME => Data._NAME;
        public int STAT_POINT => Data.STAT_POINT;

        public static string GetGradeConvertString(int grade)
        {
            switch (grade)
            {
                case 2:
                    return "r";
                case 3:
                    return "sr";
                case 4:
                    return "ur";
                case 1:
                default:
                    return "n";
            }
        }
    }

    public class CharExpData : TableData<DBChar_exp>
    {
        static private CharExpTable table = null;
        static public CharExpData Get(int grade, int level)
        {
            if (table == null)
                table = TableManager.GetTable<CharExpTable>();

            return table.Get(grade, level);
        }

        static public int GetSlotCountByDragonLevel(int grade, int dragonLevel)
        {
            var levelData = Get(grade, dragonLevel);
            if (levelData != null)
                return levelData.OPEN_EQUIP_SLOT;

            return -1;            
        }

        static public int GetUnLockLevelByGradeAndSlotIndex(int grade, int clickSlotIndex)
        {
            if (table == null)
                table = TableManager.GetTable<CharExpTable>();

            return table.GetUnLockLevelByGradeAndSlotIndex(grade, clickSlotIndex);
        }

        //다음 레벨을 올리기 위한 필요 경험치
        static public int GetCurrentRequireLevelExp(int grade, int level)
        {
            var levelData = Get(grade, level);
            if(levelData != null)
                return levelData.EXP;
            return -1;
        }

        //현재 레벨 총 누적 필요 경험치량
        static public int GetCurrentAccumulateGradeAndLevelExp(int grade, int level)
        {
            var levelData = Get(grade, level);
            if (levelData != null)
                return levelData.TOTAL_EXP;

            return -1;
        }
        //드래곤 최대 누적 경험치량
        static public int GetDragonMaxTotalExp(int grade)
        {
            var maxLevel = GameConfigTable.GetDragonLevelMax();
            return GetCurrentAccumulateGradeAndLevelExp(grade, maxLevel);
        }
        static public CharLevelExpData GetGradeAndLevelAddExp(int currentGrade, int currentLevel, int currentExp, int obtainExp)
        {
            if (table == null)
                table = TableManager.GetTable<CharExpTable>();

            return table.GetGradeAndLevelAddExp(currentGrade, currentLevel, currentExp, obtainExp);
        }

        static public CharLevelExpData GetLevelAndExpByTotalExp(int grade, int inComeTotalExp)
        {
            if (table == null)
                table = TableManager.GetTable<CharExpTable>();

            return table.GetLevelAndExpByGradeAndTotalExp(grade, inComeTotalExp);
        }

        public string KEY => Data.UNIQUE_KEY;
        public int GRADE => Data.GRADE;
        public int LEVEL => Data.LEVEL;
        public int EXP => Data.EXP;
        public int TOTAL_EXP => Data.TOTAL_EXP;
        public int OPEN_EQUIP_SLOT => Data.OPEN_EQUIP_SLOT;
    }

    public class CharMergeBaseData : TableData<DBChar_merge_base>
    {
        static private CharMergeBaseTable table = null;
        static public List<CharMergeBaseData> GetAll()
        {
            if (table == null)
                table = TableManager.GetTable<CharMergeBaseTable>();

            return table.GetAllList();
        }
        static public CharMergeBaseData GetMergeDataByGrade(int grade)
        {
            if (table == null)
                table = TableManager.GetTable<CharMergeBaseTable>();

            foreach (CharMergeBaseData element in table.GetAllList())
            {
                if (element.MATERIAL1_GRADE == grade)
                    return element;
            }

            return null;
        }

        public string KEY => Data.UNIQUE_KEY;
        public int MATERIAL1_GRADE => Data.MATERIAL1_GRADE;
        public int MATERIAL2_GRADE => Data.MATERIAL2_GRADE;
        public int MATERIAL3_GRADE => Data.MATERIAL3_GRADE;
        public int MATERIAL4_GRADE => Data.MATERIAL4_GRADE;
        public int MERGE_SUCCESS_RATE => Data.MERGE_SUCCESS_RATE;
        public int REWARD_GRADE => Data.REWARD_GRADE;
        public string COST_TYPE => Data.COST_TYPE;
        public int COST_VALUE => Data.COST_VALUE;
        public int SUCCESS_REWARD_GROUP => Data.SUCCESS_REWARD_GROUP;
        public int FAIL_REWARD_GROUP => Data.FAIL_REWARD_GROUP;
        public int GRADE { get { return MATERIAL1_GRADE; } }

        public int NEED_COUNT 
        { 
            get 
            { 
                int count = 0;
                if (MATERIAL1_GRADE > 0)
                    count++;
                if (MATERIAL2_GRADE > 0)
                    count++;
                if (MATERIAL3_GRADE > 0)
                    count++;
                if (MATERIAL4_GRADE > 0)
                    count++;
                return count;
            } 
        }
    }

    public class CharMergeListData : TableData<DBChar_merge_list>
    {
        static private CharMergeListTable table = null;
        static public List<CharMergeListData> Get(int group)
        {
            if (table == null)
                table = TableManager.GetTable<CharMergeListTable>();

            return table.GetByGroup(group);
        }
        public string KEY => Data.UNIQUE_KEY;
        public int GROUP => Data.GROUP;
        public int CHAR_KEY => Data.CHAR_KEY;
        public int NUM => Data.NUM;
        public int RATE => Data.RATE;
    }
}

