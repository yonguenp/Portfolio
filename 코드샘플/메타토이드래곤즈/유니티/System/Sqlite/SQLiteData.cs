using SQLite4Unity3d;
using UnityEngine;

namespace SandboxNetwork
{
    public class DBData
    {
        [Column("KEY")]
        [PrimaryKey]
        public string UNIQUE_KEY { get; set; } = string.Empty;
    }
    [Table("DesignHash")]
    public class DesignHash : DBData
    {
        public string HASH { get; set; }
    }
    [Table("Version")]
    public class DBVersion : DBData
    {
        public DBVersion()
        {
            UNIQUE_KEY = "1";
        }
        public DBVersion(string KEY)
        {
            UNIQUE_KEY = KEY;
        }
    }


    [Table("account_exp")]
    public class DBAccount_exp : DBData
    {
        public int LEVEL { get; set; } = -1;
        public int EXP { get; set; } = -1;
        public int TOTAL_EXP { get; set; } = -1;
        public int MAX_STAMINA { get; set; } = -1;
        public int REWARD_STAMINA { get; set; } = -1;
        public int MAX_CHAR_LEVEL { get; set; } = -1;
        public int NORMAL_REWARD { get; set; } = -1;
        public int SPECIAL_REWARD { get; set; } = -1;
        public int LUNA_NORMAL { get; set; } = -1;
        public int LUNA_SPECIAL { get; set; } = -1;
    }

    [Table("achievements_info")]
    public class DBAchievements_info : DBData
    {
        public int GROUP { get; set; } = -1;
        public int TYPE { get; set; } = -1;
        public int GRADE { get; set; } = -1;
        public int ELEMENT { get; set; } = -1;
        public int VALUE { get; set; } = -1;
        public int NUM { get; set; } = -1;
        public string REWARD_STAT_TYPE { get; set; } = "";
        public int VALUE_TYPE { get; set; } = -1;
        public float REWARD_STAT_VALUE { get; set; } = 0.0f;
    }

    [Table("adv_limit")]
    public class DBAdv_limit : DBData
    {
        public int LIMIT { get; set; } = -1;
        public int TERM { get; set; } = -1;
    }

    [Table("area_expansion")]
    public class DBArea_expansion : DBData
    {
        public int AREA_GROUP { get; set; } = -1;
        public int FLOOR { get; set; } = -1;
        public int WIDTH { get; set; } = -1;
        public int OPEN_LEVEL { get; set; } = -1;
        public string GROUND { get; set; }
        public string COST_TYPE { get; set; }
        public int COST_NUM { get; set; } = -1;
    }

    [Table("area_level")]
    public class DBArea_level : DBData
    {
        public int LEVEL { get; set; } = -1;
        public int NEED_ITEM_1 { get; set; } = -1;
        public int NEED_ITEM_1_NUM { get; set; } = -1;
        public int NEED_ITEM_2 { get; set; } = -1;
        public int NEED_ITEM_2_NUM { get; set; } = -1;
        public int NEED_ITEM_3 { get; set; } = -1;
        public int NEED_ITEM_3_NUM { get; set; } = -1;
        public int NEED_GOLD { get; set; } = -1;
        public int NEED_MISSION { get; set; } = -1;
        public int EXPANSION_AREA { get; set; } = -1;
        public int UPGRADE_TIME { get; set; } = -1;
        public int WIDTH { get; set; } = -1;
    }

    [Table("area_level_mission")]
    public class DBArea_level_mission : DBData
    {
        public int GROUP { get; set; } = -1;
        public int SORT { get; set; } = -1;
        public int _DESC { get; set; } = -1;
        public string TYPE { get; set; }
        public string SUB_TYPE { get; set; }
        public string TYPE_KEY { get; set; }
        public int TYPE_KEY_VALUE { get; set; } = -1;
    }

    [Table("building_base")]
    public class DBBuilding_base : DBData
    {
        public int TYPE { get; set; } = -1;
        public string _NAME { get; set; }
        public string _DESC { get; set; }
        public int SIZE { get; set; } = -1;
        public int START_SLOT { get; set; } = -1;
        public int MAX_SLOT { get; set; } = -1;
        public string BUILD_AREA { get; set; }
        public int BUILDING_ID { get; set; } = -1;
    }

    [Table("building_level")]
    public class DBBuilding_level : DBData
    {
        public string BUILDING_GROUP { get; set; }
        public int LEVEL { get; set; } = -1;
        public string IMAGE { get; set; }
        public int UPGRADE_TIME { get; set; } = -1;
        public int NEED_AREA_LEVEL { get; set; } = -1;
        public int NEED_ITEM_1 { get; set; } = -1;
        public int NEED_ITEM_1_NUM { get; set; } = -1;
        public int NEED_ITEM_2 { get; set; } = -1;
        public int NEED_ITEM_2_NUM { get; set; } = -1;
        public int NEED_ITEM_3 { get; set; } = -1;
        public int NEED_ITEM_3_NUM { get; set; } = -1;
        public int NEED_ITEM_4 { get; set; } = -1;
        public int NEED_ITEM_4_NUM { get; set; } = -1;
        public string COST_TYPE { get; set; }
        public int COST_NUM { get; set; } = -1;
    }

    [Table("building_open")]
    public class DBBuilding_open : DBData
    {
        public int OPEN_LEVEL { get; set; } = -1;
        public string BUILDING { get; set; }
        public int COUNT { get; set; } = -1;
        public int INSTALL_TAG { get; set; } = -1;
    }

    [Table("building_product")]
    public class DBBuilding_product : DBData
    {
        public string BUILDING_GROUP { get; set; }
        public int BUILDING_LEVEL { get; set; } = -1;
        public string ICON { get; set; }
        public int PRODUCT_ITEM { get; set; } = -1;
        public int PRODUCT_NUM { get; set; } = -1;
        public int PRODUCT_TIME { get; set; } = -1;
        public int NEED_ITEM_1 { get; set; } = -1;
        public int NEED_ITEM_1_NUM { get; set; } = -1;
        public int NEED_ITEM_2 { get; set; } = -1;
        public int NEED_ITEM_2_NUM { get; set; } = -1;
        public int NEED_ITEM_3 { get; set; } = -1;
        public int NEED_ITEM_3_NUM { get; set; } = -1;
        public int NEED_GOLD { get; set; } = -1;
    }

    [Table("building_product_auto")]
    public class DBBuilding_product_auto : DBData
    {
        public string BUILDING_GROUP { get; set; }
        public int LEVEL { get; set; } = -1;
        public string TYPE { get; set; }
        public int VALUE { get; set; } = -1;
        public int TERM { get; set; } = -1;
        public int NUM { get; set; } = -1;
        public int MAX_TIME { get; set; } = -1;
    }

    [Table("char_base")]
    public class DBChar_base : DBData
    {
        public int FACTOR { get; set; } = -1;
        public int GRADE { get; set; } = -1;
        public int ELEMENT { get; set; } = -1;
        public string BACKGROUND { get; set; }
        public string IMAGE { get; set; }
        public string SPINE_NAME { get; set; }
        public string THUMBNAIL { get; set; }
        public string SKIN { get; set; }
        public int JOB { get; set; } = -1;
        public int POSITION { get; set; } = -1;
        public string _NAME { get; set; }
        public string _DESC { get; set; }
        public int ATK { get; set; } = -1;
        public int DEF { get; set; } = -1;
        public int HP { get; set; } = -1;
        public float CRI_PROC { get; set; } = 0.0f;
        public int CRI_DMG { get; set; } = -1;
        public int LIGHT_DMG { get; set; } = -1;
        public int DARK_DMG { get; set; } = -1;
        public int WATER_DMG { get; set; } = -1;
        public int FIRE_DMG { get; set; } = -1;
        public int WIND_DMG { get; set; } = -1;
        public int EARTH_DMG { get; set; } = -1;
        public int ADD_PVP_DMG { get; set; } = -1;
        public float RATIO_PVP_DMG { get; set; } = 0.0f;
        public int ADD_PVP_CRI_DMG { get; set; } = -1;
        public int RATIO_PVP_CRI_DMG { get; set; } = -1;
        public float ADD_ATKSPEED { get; set; } = 0.0f;
        public int MOVE_SPEED { get; set; } = -1;
        public int NORMAL_SKILL { get; set; } = -1;
        public int SKILL1 { get; set; } = -1;
        public int SKILL2 { get; set; } = -1;
        public string SCRIPT_OBJECT_KEY { get; set; }
        public int IN_USE { get; set; } = -1;
        public float OFFSET { get; set; } = 0.0f;
        public int USE_CHAMPION { get; set; } = 0;
    }

    [Table("char_exp")]
    public class DBChar_exp : DBData
    {
        public int GRADE { get; set; } = -1;
        public int LEVEL { get; set; } = -1;
        public int EXP { get; set; } = -1;
        public int TOTAL_EXP { get; set; } = -1;
        public int OPEN_EQUIP_SLOT { get; set; } = -1;
    }

    [Table("char_grade")]
    public class DBChar_grade : DBData
    {
        public int _NAME { get; set; } = -1;
        public int STAT_POINT { get; set; } = -1;
    }

    [Table("char_merge_base")]
    public class DBChar_merge_base : DBData
    {
        public int MATERIAL1_GRADE { get; set; } = -1;
        public int MATERIAL2_GRADE { get; set; } = -1;
        public int MATERIAL3_GRADE { get; set; } = -1;
        public int MATERIAL4_GRADE { get; set; } = -1;
        public int MERGE_SUCCESS_RATE { get; set; } = -1;
        public int REWARD_GRADE { get; set; } = -1;
        public string COST_TYPE { get; set; }
        public int COST_VALUE { get; set; } = -1;
        public int SUCCESS_REWARD_GROUP { get; set; } = -1;
        public int FAIL_REWARD_GROUP { get; set; } = -1;
        public float SHOW_RATE_VALUE { get; set; } = 0.0f;
    }

    [Table("char_merge_list")]
    public class DBChar_merge_list : DBData
    {
        public int GROUP { get; set; } = -1;
        public int CHAR_KEY { get; set; } = -1;
        public int NUM { get; set; } = -1;
        public int RATE { get; set; } = -1;
        public string SHOW_RATE { get; set; }
    }

    [Table("char_transcendence")]
    public class DBChar_transcendence : DBData
    {
        public int TARGET_GRADE { get; set; } = -1;
        public int STEP { get; set; } = -1;
        public int MATERIAL_GRADE_RATE_1 { get; set; } = -1;
        public int MATERIAL_GRADE_RATE_2 { get; set; } = -1;
        public int MATERIAL_GRADE_RATE_3 { get; set; } = -1;
        public int MATERIAL_GRADE_RATE_4 { get; set; } = -1;
        public int MATERIAL_GRADE_RATE_5 { get; set; } = -1;
        public int MATERIAL_MAX { get; set; } = -1;
        public int SKILL_SLOT_MAX { get; set; } = -1;
        public int ADD_STAT { get; set; } = -1;
    }

    [Table("collection_group")]
    public class DBCollection_group : DBData
    {
        public int GROUP_ID { get; set; } = -1;
        public int DRAGON_KEY { get; set; } = -1;
    }

    [Table("collection_info")]
    public class DBCollection_info : DBData
    {
        public string REWARD_STAT_TYPE { get; set; }
        public int VALUE_TYPE { get; set; } = -1;
        public float REWARD_STAT_VALUE { get; set; } = 0.0f;
    }

    [Table("daily_reward")]
    public class DBDaily_reward : DBData
    {
        public int TYPE { get; set; } = -1;
        public int DAY { get; set; } = -1;
        public int NORMAL_REWARD_ID { get; set; } = -1;
        public int HOLDER_REWARD_ID { get; set; } = -1;
        public int RARITY { get; set; } = -1;
    }

    [Table("daily_stage")]
    public class DBDaily_stage : DBData
    {
        public int DAY_GROUP { get; set; } = -1;
        public int WORLD_NUM { get; set; } = -1;
        public string STAGE_DESC { get; set; }
        public string REWARD_DESC { get; set; }
        public string REWARD_ICON { get; set; }
        public string HOLDER_REWARD_DESC { get; set; }
        public string HOLDER_REWARD_ICON { get; set; }
        public string STAGE_IMAGE { get; set; }
    }

    [Table("define_resource")]
    public class DBDefine_resource : DBData
    {
        public string DEFINE { get; set; }
    }

    [Table("dice_board")]
    public class DBDice_board : DBData
    {
        public int EVENT_ID { get; set; } = -1;
        public int BOARD_ID { get; set; } = -1;
        public int REWARD_ID { get; set; } = -1;
        public int BOARD_TYPE { get; set; } = -1;
        public int RARITY { get; set; } = -1;
        public int RATE { get; set; } = -1;
    }

    [Table("element_rate")]
    public class DBElement_rate : DBData
    {
        public string A_ELEMENT { get; set; }
        public int T_FIRE { get; set; } = -1;
        public int T_WATER { get; set; } = -1;
        public int T_EARTH { get; set; } = -1;
        public int T_WIND { get; set; } = -1;
        public int T_LIGHT { get; set; } = -1;
        public int T_DARK { get; set; } = -1;
    }

    [Table("event_attendance_resource")]
    public class DBEvent_attendance_resource : DBData
    {
        public string ICON_RESOURCE_PATH { get; set; }
        public string POPUP_RESOURCE_PATH { get; set; }
        public string BG_RESOURCE_PATH { get; set; }
        public string DESC_HEX { get; set; }
        public string REWARD_DESC_HEX { get; set; }
        public int USE_CDN { get; set; } = -1;
    }

    [Table("event_banner")]
    public class DBEvent_banner : DBData
    {
        public string START_TIME { get; set; }
        public string END_TIME { get; set; }
        public string RESOURCE_PATH { get; set; }
        public int ACTION { get; set; } = -1;
        public string ACTION_PARAM { get; set; }
        public int USE { get; set; } = -1;
        public int SORT { get; set; } = -1;
    }

    [Table("event_rank_reward")]
    public class DBEvent_rank_reward : DBData
    {
        public int GROUP { get; set; } = -1;
        public int HIGHEST_RANK { get; set; } = -1;
        public uint LOWEST_RANK { get; set; } = 0;
        public int REWARD_GROUP { get; set; } = -1;
    }

    [Table("event_reward")]
    public class DBEvent_reward : DBData
    {
        public int GROUP { get; set; } = -1;
        public int TYPE { get; set; } = -1;
        public int SEQ { get; set; } = -1;
        public int REWARD_ID { get; set; } = -1;
        public int RARITY { get; set; } = -1;
    }

    [Table("event_schedule")]
    public class DBEvent_schedule : DBData
    {
        public string START_TIME { get; set; }
        public string END_TIME { get; set; }
        public string UI_END_TIME { get; set; }
        public string RESOURCE_PATH { get; set; }
        public int TYPE { get; set; } = -1;
        public int USE { get; set; } = -1;
        public int SORT { get; set; } = -1;
    }

    [Table("exchange_base")]
    public class DBExchange_base : DBData
    {
        public int TYPE { get; set; } = -1;
        public int NEED_PRODUCT_COUNT { get; set; } = -1;
        public int NEED_ITEM { get; set; } = -1;
        public int MIN_NUM { get; set; } = -1;
        public int MAX_NUM { get; set; } = -1;
    }

    [Table("exchange_group")]
    public class DBExchange_group : DBData
    {
        public int GROUP { get; set; } = -1;
        public int REQUIRED_NUMBER { get; set; } = -1;
        public int REWORD_GOLD { get; set; } = -1;
        public int REWORD_EXP { get; set; } = -1;
        public int RATE { get; set; } = -1;
    }

    [Table("gacha_group")]
    public class DBGacha_group : DBData
    {
        public string resource { get; set; }
        public int list_weight { get; set; } = -1;
        public int time_limit { get; set; } = -1;
        public string start_time { get; set; }
        public string end_time { get; set; }
    }

    [Table("gacha_list")]
    public class DBGacha_list : DBData
    {
        public int GROUP { get; set; } = -1;
        public int TYPE { get; set; } = -1;
        public int VALUE { get; set; } = -1;
        public int NUM { get; set; } = -1;
        public int RATE { get; set; } = -1;
        public string FBX { get; set; }
        public int GRADE { get; set; } = -1;
        public float SHOW_RATE { get; set; } = 0.0f;
    }

    [Table("gacha_menu")]
    public class DBGacha_menu : DBData
    {
        public int group { get; set; } = -1;
        public int menu_type { get; set; } = -1;
        public string bg_type { get; set; }
        public int resource_type { get; set; } = -1;
        public string resource_path { get; set; }
        public int list_weight { get; set; } = -1;
        public int time_limit { get; set; } = -1;
        public string start_time { get; set; }
        public string end_time { get; set; }
    }

    [Table("gacha_rate")]
    public class DBGacha_rate : DBData
    {
        public int group_id { get; set; } = -1;
        public int grade { get; set; } = -1;
        public int weight { get; set; } = -1;
        public string reward_type { get; set; }
        public int result_id { get; set; } = -1;
        public int result_type { get; set; } = -1;
        public int sort { get; set; } = -1;
    }

    [Table("gacha_shop")]
    public class DBGacha_shop : DBData
    {
        public int _NAME { get; set; } = -1;
        public int SORT { get; set; } = -1;
        public int GROUP1 { get; set; } = -1;
        public int GROUP1_RATE { get; set; } = -1;
        public int GROUP2 { get; set; } = -1;
        public int GROUP2_RATE { get; set; } = -1;
        public string COST_TYPE { get; set; }
        public int COST_NUM { get; set; } = -1;
        public int TICKET { get; set; } = -1;
    }

    [Table("gacha_type")]
    public class DBGacha_type : DBData
    {
        public int menu_id { get; set; } = -1;
        public int price_type { get; set; } = -1;
        public int price_uid { get; set; } = -1;
        public int price_value { get; set; } = -1;
        public int repeats { get; set; } = -1;
        public int proc_group { get; set; } = -1;
        public int mileage_point { get; set; } = -1;
    }

    [Table("game_config")]
    public class DBGame_config : DBData
    {
        public string VALUE { get; set; }
        public string TYPE { get; set; }
    }

    [Table("inventory")]
    public class DBInventory : DBData
    {
        public int STEP { get; set; } = -1;
        public int SLOT { get; set; } = -1;
        public int NEED_ITEM_1 { get; set; } = -1;
        public int NEED_ITEM_1_NUM { get; set; } = -1;
        public int NEED_ITEM_2 { get; set; } = -1;
        public int NEED_ITEM_2_NUM { get; set; } = -1;
        public int NEED_ITEM_3 { get; set; } = -1;
        public int NEED_ITEM_3_NUM { get; set; } = -1;
        public string COST_TYPE { get; set; }
        public int COST_NUM { get; set; } = -1;
    }

    [Table("item_base")]
    public class DBItem_base : DBData
    {
        public int KIND { get; set; } = -1;
        public string ICON { get; set; }
        public int GRADE { get; set; } = -1;
        public string SLOT_USE { get; set; }
        public string _NAME { get; set; }
        public string _DESC { get; set; }
        public int SORT { get; set; } = -1;
        public int MERGE { get; set; } = -1;
        public int SELL { get; set; } = -1;
        public int VALUE { get; set; } = -1;
        public int BUY { get; set; } = -1;
        public int USE { get; set; } = -1;
        public int PROPERTY { get; set; } = -1;
    }

    [Table("item_group")]
    public class DBItem_group : DBData
    {
        public int GROUP { get; set; } = -1;
        public string TYPE { get; set; }
        public int VALUE { get; set; } = -1;
        public int NUM { get; set; } = -1;
        public int ITEM_RATE { get; set; } = -1;
    }

    [Table("item_group_list")]
    public class DBItem_group_list : DBData
    {
        public int DICE_TYPE { get; set; } = -1;
        public int MAX_RATE { get; set; } = -1;
    }

    [Table("language")]
    public class DBLanguage : DBData
    {
        public string LANGUAGE_SHEET { get; set; }
        public string MAIL_STRING { get; set; }
        public string SCRIPT_STRING { get; set; }
        public string RESOURCE { get; set; }
        public string NAME { get; set; }
        public int USE { get; set; } = -1;
        public string SURPPORT_URL { get; set; }        
        public string GAME_GUIDE_URL { get; set; }        
        public string SERVICE_TERMS { get; set; }
        public string INFO_TERMS { get; set; }
    }

    [Table("magic_showcase_info")]
    public class DBMagic_showcase_info : DBData
    {
        public int GROUP { get; set; } = -1;
        public int LEVEL { get; set; } = -1;
        public int CORE1_AMOUNT { get; set; } = -1;
        public int CORE2_AMOUNT { get; set; } = -1;
        public int CORE3_AMOUNT { get; set; } = -1;
        public int STAT_TYPE_KEY { get; set; } = -1;
        public int STAT_VALUE_TYPE { get; set; } = -1;
        public float STAT_VALUE { get; set; } = 0.0f;
    }

    [Table("mail_string")]
    public class DBMail_string : DBData
    {
        public int TYPE { get; set; } = -1;
        public string TEXT { get; set; }
    }

    [Table("mail_eng")]
    public class DBMail_eng : DBMail_string
    {
    }

    [Table("mail_jpn")]
    public class DBMail_jpn : DBMail_string
    {
    }

    [Table("mail_kor")]
    public class DBMail_kor : DBMail_string
    {
    }

    [Table("mail_prt")]
    public class DBMail_prt : DBMail_string
    {
    }

    [Table("mail_chs")]
    public class DBMail_chs : DBMail_string
    {
    }

    [Table("mail_cht")]
    public class DBMail_cht : DBMail_string
    {
    }

    [Table("mine_booster")]
    public class DBMine_booster : DBData
    {
        public int GROUP { get; set; } = -1;
        public float VALUE { get; set; } = -1;
        public int VALUE_TYPE { get; set; } = -1;
        public int TYPE { get; set; } = -1;
        public int BOOST_TIME { get; set; } = -1;
        public string EXPIRE_AT { get; set; }
    }

    [Table("mine_drill")]
    public class DBMine_drill : DBData
    {
        public int LEVEL { get; set; } = -1;
        public int MINE_DURABILITY { get; set; } = -1;
        public string REPAIR_COST_TYPE { get; set; }
        public int REPAIR_COST_VALUE { get; set; } = -1;
        public int REPAIR_COST_NUM { get; set; } = -1;
    }

    [Table("monster_base")]
    public class DBMonster_base : DBData
    {
        public int FACTOR { get; set; } = -1;
        public int GRADE { get; set; } = -1;
        public int ELEMENT { get; set; } = -1;
        public int SIZE { get; set; } = -1;
        public string IMAGE { get; set; }
        public string THUMBNAIL { get; set; }
        public string SKIN { get; set; }
        public string _NAME { get; set; }
        public string _DESC { get; set; }
        public int ATK { get; set; } = -1;
        public int DEF { get; set; } = -1;
        public int HP { get; set; } = -1;
        public float CRI_PROC { get; set; } = 0.0f;
        public int CRI_DMG { get; set; } = -1;
        public int LIGHT_DMG { get; set; } = -1;
        public int DARK_DMG { get; set; } = -1;
        public int WATER_DMG { get; set; } = -1;
        public int FIRE_DMG { get; set; } = -1;
        public int WIND_DMG { get; set; } = -1;
        public int EARTH_DMG { get; set; } = -1;
        public float ADD_ATKSPEED { get; set; } = 0.0f;
        public int MOVE_SPEED { get; set; } = -1;
        public long NORMAL_SKILL { get; set; } = -1;
        public long SKILL1 { get; set; } = -1;
        public long SKILL2 { get; set; } = -1;
        public string SCRIPT_OBJECT_KEY { get; set; }
        public int CC_IMN { get; set; } = -1;
    }

    [Table("monster_spawn")]
    public class DBMonster_spawn : DBData
    {
        public int SPAWN_GROUP { get; set; } = -1;
        public int WAVE { get; set; } = -1;
        public int GROUP { get; set; } = -1;
        public int POSITION { get; set; } = -1;
        public int MONSTER { get; set; } = -1;
        public int IS_BOSS { get; set; } = -1;
        public float SCALE { get; set; } = 0.0f;
        public int LEVEL { get; set; } = -1;
        public int RATE { get; set; } = -1;
        public int INF { get; set; } = -1;
        public int IS_MOVE { get; set; } = -1;
    }

    [Table("part_base")]
    public class DBPart_base : DBData
    {
        public int GRADE { get; set; } = -1;
        public int ITEM { get; set; } = -1;
        public string STAT_TYPE { get; set; }
        public string VALUE_TYPE { get; set; }
        public float VALUE { get; set; } = 0.0f;
        public float VALUE_GROW { get; set; } = 0.0f;
        public int SUB_1_STEP { get; set; } = -1;
        public int SUB_1 { get; set; } = -1;
        public int SUB_2_STEP { get; set; } = -1;
        public int SUB_2 { get; set; } = -1;
        public int SUB_3_STEP { get; set; } = -1;
        public int SUB_3 { get; set; } = -1;
        public int SUB_4_STEP { get; set; } = -1;
        public int SUB_4 { get; set; } = -1;
        public int SET_GROUP { get; set; } = -1;
        public string UNEQUIP_COST_TYPE { get; set; }
        public int UNEQUIP_COST_NUM { get; set; } = -1;
        public int INF { get; set; } = -1;
    }

    [Table("part_decompose")]
    public class DBPart_decompose : DBData
    {
        public int GRADE { get; set; } = -1;
        public int REINFORCE_NUM { get; set; } = -1;
        public int ITEM { get; set; } = -1;
        public int ITEM_NUM { get; set; } = -1;
    }

    [Table("part_merge_base")]
    public class DBPart_merge_base : DBData
    {
        public int GRADE { get; set; } = -1;
        public int RATE { get; set; } = -1;
        public int BASE_NUM { get; set; } = -1;
        public int ITEM { get; set; } = -1;
        public int ITEM_NUM { get; set; } = -1;
        public string COST_TYPE { get; set; }
        public int COST_NUM { get; set; } = -1;
        public int RESULT_SUCCESS { get; set; } = -1;
        public int RESULT_FAIL { get; set; } = -1;
    }

    [Table("part_merge_equipamountbonus")]
    public class DBPart_merge_equipamountbonus : DBData
    {
        public int GRADE { get; set; } = -1;
        public int EXTRA_NUM { get; set; } = -1;
        public int ADD_RATE { get; set; } = -1;
    }

    [Table("part_merge_reinforcebonus")]
    public class DBPart_merge_reinforcebonus : DBData
    {
        public int GRADE { get; set; } = -1;
        public int REINFORCE_NUM { get; set; } = -1;
        public int ADD_RATE { get; set; } = -1;
    }

    [Table("part_reinforce")]
    public class DBPart_reinforce : DBData
    {
        public int GRADE { get; set; } = -1;
        public int STEP { get; set; } = -1;
        public int RATE { get; set; } = -1;
        public int ITEM { get; set; } = -1;
        public int ITEM_NUM { get; set; } = -1;
        public string COST_TYPE { get; set; }
        public int COST_NUM { get; set; } = -1;
        public int DESTROY { get; set; } = -1;
        public int DESTROY_REWARD { get; set; } = -1;

        public int ITEM2 { get; set; } = -1;
        public int ITEM_NUM2 { get; set; } = -1;
        public int RATE2 { get; set; } = -1;
        public int DESTROY2 { get; set; } = -1;
    }

    [Table("part_set")]
    public class DBPart_set : DBData
    {
        public int GROUP { get; set; } = -1;
        public int NUM { get; set; } = -1;
        public string STAT_TYPE { get; set; }
        public string VALUE_TYPE { get; set; }
        public float VALUE { get; set; } = 0.0f;
    }

    [Table("pass_item")]
    public class DBPass_item : DBData
    {
        public int GROUP { get; set; } = -1;
        public int LEVEL { get; set; } = -1;
        public int NORMAL_REWARD { get; set; } = -1;
        public int SPECIAL_REWARD { get; set; } = -1;
        public int HOLDER_SP_REWARD { get; set; } = -1;
        public int NEXT_POINT { get; set; } = -1;
    }

    [Table("fusion_option")]
    public class DBFusion_option : DBData
    {
        public string GROUP { get; set; } = "";
        public string STAT { get; set; } = "";
        public string VALUE_TYPE { get; set; } = "";
        public float VALUE_MIN { get; set; } = -1;
        public float VALUE_MAX { get; set; } = -1;
        public float LEGEND_BONUS { get; set; } = -1;
        public float VALUE_REINFORCE { get; set; } = -1;
        public int RATE { get; set; } = -1;
        public string _DESC { get; set; } = "";
    }

    [Table("personal_goods")]
    public class DBPersonal_goods : DBData
    {
        public int CONDITION_TYPE { get; set; } = -1;
        public int VALUE { get; set; } = -1;
        public int TIME { get; set; } = -1;
        public string STR_KEY { get; set; }
    }

    [Table("pet_base")]
    public class DBPet_base : DBData
    {
        public int GRADE { get; set; } = -1;
        public int ELEMENT { get; set; } = -1;
        public string ELEMENT_BUFF_TYPE { get; set; }
        public int SUB_1 { get; set; } = -1;
        public int SUB_2 { get; set; } = -1;
        public int SUB_3 { get; set; } = -1;
        public int SUB_4 { get; set; } = -1;
        public string BACKGROUND { get; set; }
        public string IMAGE { get; set; }
        public string THUMBNAIL { get; set; }
        public string SKIN { get; set; }
        public string _NAME { get; set; }
        public string _DESC { get; set; }
    }

    [Table("pet_decompose")]
    public class DBPet_decompose : DBData
    {
        public int GRADE { get; set; } = -1;
        public int REINFORCE { get; set; } = -1;
        public int ITEM { get; set; } = -1;
        public int ITEM_NUM { get; set; } = -1;
    }

    [Table("pet_element")]
    public class DBPet_element : DBData
    {
        public int EQUAL_PET_BONUS_ATK { get; set; } = -1;
        public int EQUAL_PET_BONUS_DEF { get; set; } = -1;
        public int EQUAL_PET_BONUS_HP { get; set; } = -1;
    }

    [Table("pet_exp")]
    public class DBPet_exp : DBData
    {
        public int GRADE { get; set; } = -1;
        public int LEVEL { get; set; } = -1;
        public int EXP { get; set; } = -1;
        public int TOTAL_EXP { get; set; } = -1;
        public int OFFER_EXP { get; set; } = -1;
    }

    [Table("pet_grade")]
    public class DBPet_grade : DBData
    {
        public int _NAME { get; set; } = -1;
        public int START_STAT_NUM { get; set; } = -1;
        public int SKILL_MAX { get; set; } = -1;
        public int UNIQUE_MIN_GROUP_NUM { get; set; } = -1;
        public int UNIQUE_MAX_GROUP_NUM { get; set; } = -1;
    }

    [Table("pet_merge_base")]
    public class DBPet_merge_base : DBData
    {
        public int MATERIAL_GRADE { get; set; } = -1;
        public int MERGE_SUCCESS_RATE { get; set; } = -1;
        public int REWARD_GRADE { get; set; } = -1;
        public int SUCCESS_REWARD_GROUP { get; set; } = -1;
        public int FAIL_REWARD_GROUP { get; set; } = -1;
        public string COST_TYPE { get; set; }
        public int COST_NUM { get; set; } = -1;
    }

    [Table("pet_reinforce")]
    public class DBPet_reinforce : DBData
    {
        public int GRADE { get; set; } = -1;
        public int STEP { get; set; } = -1;
        public int ELEMENT_BUFF { get; set; } = 0;
        public int RATE { get; set; } = -1;
        public int ITEM { get; set; } = -1;
        public int ITEM_NUM { get; set; } = -1;
        public int RAISE_ITEM_NUM { get; set; } = -1;
        public int MAX_ITEM_NUM { get; set; } = -1;
        public string COST_TYPE { get; set; }
        public int COST_NUM { get; set; } = -1;
        public int LEVEL_BONUS { get; set; } = -1;
    }

    [Table("pet_skill_normal")]
    public class DBPet_skill_normal : DBData
    {
        public int GRADE { get; set; } = -1;
        public int _NAME { get; set; } = -1;
        public int _DESC { get; set; } = -1;
        public string ICON { get; set; }
        public string STAT { get; set; }
        public float VALUE { get; set; } = 0.0f;
        public float VALUE_FACTOR { get; set; } = 0.0f;
        public int NEXT_GENE_SKILL { get; set; } = -1;
        public int GENE_WEIGHT { get; set; } = -1;
        public int RATE { get; set; } = -1;
    }

    [Table("pet_stat")]
    public class DBPet_stat : DBData
    {
        public string STAT_TYPE { get; set; }
        public int GENO_GROUP { get; set; } = -1;
        public int GENO_TYPE { get; set; } = -1;
        public string VALUE_TYPE { get; set; }
        public float START_STAT_1 { get; set; } = 0.0f;
        public float START_STAT_2 { get; set; } = 0.0f;
        public float STAT_LEVEL_GROW { get; set; } = 0.0f;
        public float STAT_REINFORCE_GROW { get; set; } = 0.0f;
        public int RATE { get; set; } = -1;
        public int _DESC { get; set; } = -1;
    }

    [Table("post_reward")]
    public class DBPost_reward : DBData
    {
        public int GROUP_ID { get; set; } = -1;
        public string TYPE { get; set; }
        public int VALUE { get; set; } = -1;
        public int NUM { get; set; } = -1;
        public string RESOURCE { get; set; }
        public int ORDER { get; set; } = -1;
    }

    [Table("pvp_rank")]
    public class DBPvp_rank : DBData
    {
        public int GROUP { get; set; } = -1;
        public string _NAME { get; set; }
        public string ICON { get; set; }
        public int NEED_POINT { get; set; } = -1;
        public int FIRST_REWARD_GROUP { get; set; } = -1;
        public int FIRST_REWARD_GROUP_HD { get; set; } = -1;
        public int SEASON_REWARD_GROUP { get; set; } = -1;
        public int SEASON_REWARD_GROUP_HD { get; set; } = -1;
        public int RESET_RANK { get; set; } = -1;
        public int POINT_REWARD { get; set; } = -1;
        public int REWARD_ACCOUNT_EXP { get; set; } = -1;
    }

    [Table("pvp_rank_season_reward")]
    public class DBPvp_rank_season_reward : DBData
    {
        public string TYPE { get; set; }
        public int MIN { get; set; } = -1;
        public int MAX { get; set; } = -1;
        public int REWARD_GOLD { get; set; } = -1;
        public int REWARD_CASH { get; set; } = -1;
        public int REWARD_ITEM { get; set; } = -1;
        public int REWARD_ITEM_NUM { get; set; } = -1;
    }

    [Table("pvp_season")]
    public class DBPvp_season : DBData
    {
        public string START_TIME { get; set; }
        public string END_TIME { get; set; }
        public string _NAME { get; set; }
        public int ATK_ELEM { get; set; } = -1;
        public int DEF_ELEM { get; set; } = -1;
        public int HP_ELEM { get; set; } = -1;
    }

    [Table("quest_base")]
    public class DBQuest_base : DBData
    {
        public string TYPE { get; set; }
        public int GROUP { get; set; } = -1;
        public string ICON { get; set; }
        public string _SUBJECT { get; set; }
        public int IS_START_POPUP { get; set; } = -1;
        public int START_GROUP { get; set; } = -1;
        public int CONDITION_GROUP { get; set; } = -1;
        public int REWARD_ACCOUNT_EXP { get; set; } = -1;
        public int REWARD_GOLD { get; set; } = -1;
        public int REWARD_ENERGY { get; set; } = -1;
        public int REWARD_TICKET_PVP { get; set; } = -1;
        public int REWARD_GROUP { get; set; } = -1;
        public int EVENT_KEY { get; set; } = -1;
    }

    [Table("quest_trigger_group")]
    public class DBQuest_trigger_group : DBData
    {
        public int TRIGGER_TYPE { get; set; } = -1;
        public int GROUP { get; set; } = -1;
        public string TYPE { get; set; }
        public string SUB_TYPE { get; set; }
        public string TYPE_KEY { get; set; }
        public int TYPE_KEY_VALUE { get; set; } = -1;
        public string _NOTIE { get; set; }
    }

    [Table("raid_boss_level")]
    public class DBRaid_boss_level : DBData
    {
        public int MONSTER_KEY { get; set; } = -1;
        public int LEVEL { get; set; } = -1;
        public int NEED_DMG { get; set; } = -1;
        public int REWARD { get; set; } = -1;
        public int REWARD_ACCOUNT_EXP { get; set; } = -1;
        public string REWARD_DESC { get; set; } = string.Empty;
    }

    [Table("raid_boss_parts")]
    public class DBRaid_boss_parts : DBData
    {
        public int BOSS_KEY { get; set; } = -1;
        public int PARTS_TYPE { get; set; } = -1;
        public int ACTIVE_LEVEL { get; set; } = -1;
        public int GROUP { get; set; } = -1;
        public int TARGET_PRIORITY { get; set; } = -1;
        public int ATTACK_PRIORITY { get; set; } = -1;
    }

    [Table("raid_boss_rank_reward")]
    public class DBRaid_boss_rank_reward : DBData
    {
        public int GROUP { get; set; } = -1;
        public int HIGHEST_RANK { get; set; } = -1;
        public uint LOWEST_RANK { get; set; } = 0;
        public int REWARD_GROUP { get; set; } = -1;
    }

    [Table("recipe_core")]
    public class DBRecipe_core : DBData
    {
        public int REWARD { get; set; } = -1;
        public int FAIL { get; set; } = -1;
        public int RATE { get; set; } = -1;
    }

    [Table("recipe_material")]
    public class DBRecipe_material : DBData
    {
        public int RECIPE_ID { get; set; } = -1;
        public int TYPE { get; set; } = -1;
        public int PARAM { get; set; } = -1;
        public int VALUE { get; set; } = -1;
    }

    [Table("reserv_mail")]
    public class DBReserv_mail : DBData
    {
        public int TITLE { get; set; } = -1;
        public int REWARD { get; set; } = -1;
        public string SEND_START { get; set; }
        public string SEND_END { get; set; }
        public string EXPIRE_AT { get; set; }
        public int IS_ACTIVATED { get; set; } = -1;
        public int IS_DELETED { get; set; } = -1;
        public string SENDER { get; set; }
    }

    [Table("script_chs")]
    public class DBScript_chs : DBScript_string
    {
    }

    [Table("script_cht")]
    public class DBScript_cht : DBScript_string
    {
    }

    [Table("script_eng")]
    public class DBScript_eng : DBScript_string
    {
    }

    [Table("script_group")]
    public class DBScript_group : DBData
    {
        public int duration { get; set; } = -1;
        public int group { get; set; } = -1;
        public int script_UI { get; set; } = -1;
        public int object_key_1 { get; set; } = -1;
        public int object_effect_1 { get; set; } = -1;
        public int object_key_2 { get; set; } = -1;
        public int object_effect_2 { get; set; } = -1;
        public int object_key_3 { get; set; } = -1;
        public int object_effect_3 { get; set; } = -1;
        public string BG_resource { get; set; }
        public string BGM_resource { get; set; }
    }

    [Table("script_jpn")]
    public class DBScript_jpn : DBScript_string
    {
    }

    [Table("script_kor")]
    public class DBScript_kor : DBScript_string
    {
    }

    [Table("script_object")]
    public class DBScript_object : DBData
    {
        public string name { get; set; }
        public string color { get; set; }
        public int type { get; set; } = -1;
        public int scale_x { get; set; } = -1;
        public int scale_y { get; set; } = -1;
        public int pos_x { get; set; } = -1;
        public int pos_y { get; set; } = -1;
        public int resource { get; set; } = -1;
        public string param { get; set; }
    }

    [Table("script_prt")]
    public class DBScript_prt : DBScript_string
    {
    }

    [Table("script_string")]
    public class DBScript_string : DBData
    {
        public string TEXT { get; set; }
    }

    [Table("script_trigger")]
    public class DBScript_trigger : DBData
    {
        public int seq { get; set; } = -1;
        public int trigger_type { get; set; } = -1;
        public int trigger_param { get; set; } = -1;
        public int script_group { get; set; } = -1;
    }

    [Table("shop_banner")]
    public class DBShop_banner : DBData
    {
        public int TYPE { get; set; } = -1;
        public string RESOURCE { get; set; }
        public int BG_TYPE { get; set; }
        public string ICON_RESOURCE { get; set; }
    }

    [Table("shop_goods")]
    public class DBShop_goods : DBData
    {
        public int MENU { get; set; } = -1;
        public int USE { get; set; } = -1;
        public int TYPE { get; set; } = -1;
        public int SORT { get; set; } = -1;
        public int PRICE_TYPE { get; set; } = -1;
        public int PRICE_PARAM { get; set; } = -1;
        public int PRICE_AMOUNT { get; set; } = -1;
        public string RESOURCE { get; set; }
        public int TIME_LIMIT { get; set; } = -1;
        public int BUY_TYPE { get; set; } = -1;
        public int BUY_LIMIT { get; set; } = -1;
        public int REWARD_TYPE { get; set; } = -1;
        public int REWARD_ID { get; set; } = -1;
        public string START_TIME { get; set; }
        public string END_TIME { get; set; }
    }

    [Table("shop_menu")]
    public class DBShop_menu : DBData
    {
        public int TYPE { get; set; } = -1;
        public int SORT { get; set; } = -1;
        public int PAGE_TYPE { get; set; } = -1;
        public int ASSET_TYPE { get; set; } = -1;
        public int TIME_LIMIT { get; set; } = -1;
        public int USE { get; set; } = -1;
        public int BG_TYPE { get; set; } = -1;
        public string ICON { get; set; }
        public int TAP_COLOR { get; set; } = -1;
        public string START_TIME { get; set; }
        public string END_TIME { get; set; }
    }

    [Table("shop_random")]
    public class DBShop_random : DBData
    {
        public int MENU_GROUP { get; set; } = -1;
        public int GOOD_GROUP { get; set; } = -1;
        public int GOOD_ID { get; set; } = -1;
        public int RATE { get; set; } = -1;
    }

    [Table("shop_sku")]
    public class DBShop_sku : DBData
    {
        public string desc { get; set; }
        public string SKU_APPLE { get; set; }
        public string SKU_GOOGLE { get; set; }
        public string SKU_ONESTORE { get; set; }
        public string KRW_APPLE { get; set; }
        public string KRW_GOOGLE { get; set; }
        public string KRW_ONE { get; set; }
        public string USD_APPLE { get; set; }
        public string USD_GOOGLE { get; set; }
        public string USD_ONE { get; set; }
        public string JPY_APPLE { get; set; }
        public string JPY_GOOGLE { get; set; }
        public string JPY_ONE { get; set; }

        public string OFFER_START_TIME { get; set; }
        public string OFFER_END_TIME { get; set; }
    }

    [Table("skill_char")]
    public class DBSkill_char : DBData
    {
        public float WEAK_TIME { get; set; } = 0.0f;
        public float CASTING_TIME { get; set; } = 0.0f;
        public float AFTER_DELAY { get; set; } = 0.0f;
        public float START_COOL_TIME { get; set; } = 0.0f;
        public float COOL_TIME { get; set; } = 0.0f;
        public float GLOBAL_COOL_TIME { get; set; } = 0.0f;
        public string TARGET_TYPE { get; set; }
        public string TARGET_SORT { get; set; }
        public float RANGE_X { get; set; } = 0.0f;
        public float RANGE_Y { get; set; } = 0.0f;
        public string ICON { get; set; }
        public string NAME { get; set; }
        public string DESC { get; set; }
        public int SUMMON_KEY { get; set; } = -1;
        public int ANI { get; set; } = -1;
        public int CASTING_EFFECT_RSC_KEY { get; set; } = -1;
        public float CASTING_EFFECT_RSC_DELAY { get; set; } = 0.0f;
        public string CASTING_SOUND { get; set; }
        public float CASTING_SOUND_DELAY { get; set; } = 0.0f;
        public string SKILL_CONDITION { get; set; }
        public string CONDITION_VALUE_TYPE { get; set; }
        public float CONDITION_VALUE { get; set; } = 0.0f;
        public int INF { get; set; } = -1;
    }

    [Table("skill_effect")]
    public class DBSkill_effect : DBData
    {
        public int GROUP_KEY { get; set; } = -1;
        public float DELAY { get; set; } = 0.0f;
        public float RATE { get; set; } = 0.0f;
        public string TYPE { get; set; }
        public string STAT_TYPE { get; set; }
        public string EX_TARGET_TYPE { get; set; }
        public string EX_TARGET_SORT { get; set; }
        public int EX_TARGET_COUNT { get; set; } = -1;
        public string EX_RANGE_TYPE { get; set; }
        public float EX_X { get; set; } = 0.0f;
        public float EX_Y { get; set; } = 0.0f;
        public float EX_GROUND_X { get; set; } = 0.0f;
        public float EX_GROUND_Y { get; set; } = 0.0f;
        public int EX_GROUND_MAX { get; set; } = -1;
        public int EX_GROUND_MIN { get; set; } = -1;
        public string VALUE_TYPE { get; set; }
        public float VALUE { get; set; } = 0.0f;
        public float HP { get; set; } = 0.0f;
        public float DEF { get; set; } = 0.0f;
        public float ATK { get; set; } = 0.0f;
        public float LIGHT { get; set; } = 0.0f;
        public float DARK { get; set; } = 0.0f;
        public float WATER { get; set; } = 0.0f;
        public float FIRE { get; set; } = 0.0f;
        public float WIND { get; set; } = 0.0f;
        public float EARTH { get; set; } = 0.0f;
        public float CRI { get; set; } = 0.0f;
        public float PVP { get; set; } = 0.0f;
        public float BOSS { get; set; } = 0.0f;
        public int GROW_GROUP_KEY { get; set; } = -1;
        public int TARGET_EFFECT_RSC_KEY { get; set; } = -1;
        public int NEST_GROUP { get; set; } = -1;
        public int NEST_COUNT { get; set; } = -1;
        public int HIT_COUNT { get; set; } = -1;
        public float MAX_TIME { get; set; } = 0.0f;
        public string TRIGGER_TYPE { get; set; }
        public float TRIGGER_VALUE { get; set; } = 0.0f;
        public int NEXT_EFFECT { get; set; } = -1;
    }

    [Table("skill_level")]
    public class DBSkill_level : DBData
    {
        public int JOB { get; set; } = -1;
        public int SKILL_LEVEL { get; set; } = -1;
        public int ITEM { get; set; } = -1;
        public int ITEM_NUM { get; set; } = -1;
    }

    [Table("skill_level_group")]
    public class DBSkill_level_group : DBData
    {
        public int GROW_GROUP_KEY { get; set; } = -1;
        public int GROW_LEVEL { get; set; } = -1;
        public float EFFECT_RATE { get; set; } = 0.0f;
        public float VALUE { get; set; } = 0.0f;
        public float HP { get; set; } = 0.0f;
        public float DEF { get; set; } = 0.0f;
        public float ATK { get; set; } = 0.0f;
        public float LIGHT { get; set; } = 0.0f;
        public float DARK { get; set; } = 0.0f;
        public float WATER { get; set; } = 0.0f;
        public float FIRE { get; set; } = 0.0f;
        public float WIND { get; set; } = 0.0f;
        public float EARTH { get; set; } = 0.0f;
        public float CRI { get; set; } = 0.0f;
        public float PVP { get; set; } = 0.0f;
        public float BOSS { get; set; } = 0.0f;
        public float MAX_TIME { get; set; } = 0.0f;
        public int INF { get; set; } = -1;
    }

    [Table("skill_passive")]
    public class DBSkill_passive : DBData
    {
        public string PASSIVE_EFFECT { get; set; }
        public string STAT { get; set; }
        public string EFFECT_VALUE { get; set; }
        public float VALUE { get; set; } = 0.0f;
        public string TARGET { get; set; }
        public int START_TYPE { get; set; } = -1;
        public float RATE { get; set; } = 0.0f;
        public int RATE_TYPE { get; set; } = -1;
        public float ADD_RATE_MAX { get; set; } = 0.0f;
        public float MAX_TIME { get; set; } = 0.0f;
        public int NEST_GROUP { get; set; } = -1;
        public int NEST_COUNT { get; set; } = -1;
        public int SELF_EFFECT_RESOURCE { get; set; } = -1;
        public int TARGET_EFFECT_RESOURCE { get; set; } = -1;
        public string DESC { get; set; }
        public int USE_CONTENTS { get; set; } = -1;
    }

    [Table("skill_passive_group")]
    public class DBSkill_passive_group : DBData
    {
        public int GROUP_ID_SLOT_1 { get; set; } = -1;
        public int GROUP_ID_SLOT_2 { get; set; } = -1;
        public int PRICE_ITEM { get; set; } = -1;
        public int PRICE_VALUE { get; set; } = -1;
        public int PRICE_GOLD { get; set; } = -1;
    }

    [Table("skill_passive_rate")]
    public class DBSkill_passive_rate : DBData
    {
        public int GROUP_ID { get; set; } = -1;
        public int RATE { get; set; } = -1;
        public string RESULT_TYPE { get; set; }
        public int RESULT_GROUP { get; set; } = -1;
        public string PASSIVE_NAME { get; set; }
    }

    [Table("skill_resource")]
    public class DBSkill_resource : DBData
    {
        public string FILE { get; set; }
        public string IMAGE { get; set; }
        public string ORDER_TYPE { get; set; }
        public int LOCATION { get; set; } = -1;
        public int FOLLOW { get; set; } = -1;
        public int ORDER { get; set; } = -1;
        public string VIBE_FILE { get; set; }
        public float VIBE_DELAY { get; set; } = 0.0f;
        public float VIBE_LIFE { get; set; } = 0.0f;
        public int AUTO_DIRECTION { get; set; } = -1;
    }

    [Table("skill_summon")]
    public class DBSkill_summon : DBData
    {
        public float DELAY { get; set; } = 0.0f;
        public string TYPE { get; set; }
        public string TARGET { get; set; }
        public string TARGET_TYPE { get; set; }
        public string TARGET_SORT { get; set; }
        public int TARGET_COUNT { get; set; } = -1;
        public string RANGE_TYPE { get; set; }
        public float RANGE_X { get; set; } = 0.0f;
        public float RANGE_Y { get; set; } = 0.0f;
        public float GROUND_X { get; set; } = 0.0f;
        public float GROUND_Y { get; set; } = 0.0f;
        public float GROUND_MIN { get; set; } = 0.0f;
        public int EFFECT_GROUP_KEY { get; set; } = -1;
        public int ARROW_SPD { get; set; } = -1;
        public int POS_X { get; set; } = -1;
        public int POS_Y { get; set; } = -1;
        public int ARROW_RSC_KEY { get; set; } = -1;
        public float VALUE1 { get; set; } = 0.0f;
        public float VALUE2 { get; set; } = 0.0f;
        public int SKILL_EFFECT_RSC_KEY { get; set; } = -1;
        public string SKILL_SOUND { get; set; }
        public float SKILL_SOUND_DELAY { get; set; } = 0.0f;
        public string TRIGGER_TYPE { get; set; }
        public float TRIGGER_VALUE { get; set; } = -1;
        public int NEXT_SUMMON { get; set; } = -1;
    }

    [Table("slot_cost")]
    public class DBSlot_cost : DBData
    {
        public int TYPE { get; set; } = -1;
        public int BUY_SLOT_COUNT { get; set; } = -1;
        public string COST_TYPE { get; set; }
        public int COST_NUM { get; set; } = -1;
    }

    [Table("sound_resource")]
    public class DBSound_resource : DBData
    {
        public string SOUND_KEY { get; set; }
        public int TYPE { get; set; } = -1;
        public string SOUND_FILE_NAME { get; set; }
    }

    [Table("stage_base")]
    public class DBStage_base : DBData
    {
        public int TYPE { get; set; } = -1;
        public int DIFFICULT { get; set; } = -1;
        public int WORLD { get; set; } = -1;
        public int STAGE { get; set; } = -1;
        public int _NAME { get; set; } = -1;
        public string IMAGE { get; set; }
        public int PROPERTY_PAGE { get; set; } = -1;
        public string COST_TYPE { get; set; }
        public int COST_VALUE { get; set; } = -1;
        public int CLEAR_COUNT { get; set; } = -1;
        public int TIME { get; set; } = -1;
        public int SPAWN { get; set; } = -1;
        public int REWARD_ACCOUNT_EXP { get; set; } = -1;
        public int REWARD_CHAR_EXP { get; set; } = -1;
        public int REWARD_GOLD { get; set; } = -1;
        public int REWARD_ITEM { get; set; } = -1;
        public int HOLDER_REWARD_ITEM { get; set; } = -1;
        public int REWARD_ITEM_COUNT { get; set; } = -1;
        public int FIRST_REWARD_STAR_1 { get; set; } = -1;
        public int FIRST_REWARD_STAR_2 { get; set; } = -1;
        public int FIRST_REWARD_STAR_3 { get; set; } = -1;
        public string CHARGE_COST_TYPE { get; set; }
        public int CHARGE_COST_VALUE { get; set; } = -1;
        public int CHARGE_COUNT { get; set; } = -1;
        public int CHARGE_MAX { get; set; } = -1;
        public int UNLOCK_MISSION { get; set; } = -1;
    }

    [Table("stat_factor")]
    public class DBStat_factor : DBData
    {
        public int USER { get; set; } = -1;
        public int ATK { get; set; } = -1;
        public int DEF { get; set; } = -1;
        public int HP { get; set; } = -1;
        public int LIGHT_DMG { get; set; } = -1;
        public int DARK_DMG { get; set; } = -1;
        public int WATER_DMG { get; set; } = -1;
        public int FIRE_DMG { get; set; } = -1;
        public int WIND_DMG { get; set; } = -1;
        public int EARTH_DMG { get; set; } = -1;
        public float CRI_PROC { get; set; } = 0.0f;
        public int CRI_DMG { get; set; } = -1;
        public float ADD_ATKSPEED { get; set; } = 0.0f;
    }

    [Table("stat_type")]
    public class DBStat_type : DBData
    {
        public string STAT_TYPE { get; set; }
        public string VALUE { get; set; }
        public int STAT_MIN { get; set; } = -1;
        public int STAT_MAX { get; set; } = -1;
        public int _DESC { get; set; } = -1;
        public int _DESC_PERCENT { get; set; } = -1;
        public int _DESC_VALUE { get; set; } = -1;
        public int SORT_GROUP { get; set; } = -1;
        public int GEM_STAT { get; set; } = -1;
    }

    [Table("string")]
    public class DBString : DBData
    {
        [Indexed]
        public string STR_KEY { get; set; }
        public string TEXT { get; set; }
    }

    [Table("string_chs")]
    public class DBString_chs : DBString
    {
    }

    [Table("string_cht")]
    public class DBString_cht : DBString
    {
    }

    [Table("string_eng")]
    public class DBString_eng : DBString
    {
    }

    [Table("string_jpn")]
    public class DBString_jpn : DBString
    {
    }

    [Table("string_kor")]
    public class DBString_kor : DBString
    {
    }

    [Table("string_prt")]
    public class DBString_prt : DBString
    {
    }

    [Table("subscription_item")]
    public class DBSubscription_item : DBData
    {
        public int GROUP_ID { get; set; } = -1;
        public int DAY { get; set; } = -1;
        public int REWARD_ID { get; set; } = -1;
    }

    [Table("subway_delivery")]
    public class DBSubway_delivery : DBData
    {
        public int NEED_ITEM { get; set; } = -1;
        public int NEED_NUM { get; set; } = -1;
        public int NEED_PRODUCT_COUNT { get; set; } = -1;
        public int DELIVERY_TIME { get; set; } = -1;
        public int REWARD_GROUP { get; set; } = -1;
    }

    [Table("subway_platform")]
    public class DBSubway_platform : DBData
    {
        public int PLATFORM { get; set; } = -1;
        public int OPEN_LEVEL { get; set; } = -1;
        public string COST_TYPE { get; set; }
        public int COST_NUM { get; set; } = -1;
    }

    [Table("sub_option")]
    public class DBSub_option : DBData
    {
        public int GROUP { get; set; } = -1;
        public string STAT_TYPE { get; set; }
        public string VALUE_TYPE { get; set; }
        public float VALUE_MIN { get; set; } = 0.0f;
        public float VALUE_MAX { get; set; } = 0.0f;
        public float VALUE_STEP { get; set; } = 0.0f;
        public int WEIGHT { get; set; } = -1;
        public int RATE { get; set; } = -1;
    }

    [Table("tutorial_script")]
    public class DBTutorial_script : DBData
    {
        public int GROUP { get; set; } = -1;
        public int SEQUENCE { get; set; } = -1;
        public int SCRIPT_STR { get; set; } = -1;
        public int RESTART_TUTORIAL { get; set; } = -1;
    }

    [Table("tutorial_trigger")]
    public class DBTutorial_trigger : DBData
    {
        public int TRIGGER_TYPE { get; set; } = -1;
        public int TRIGGER_PARAM { get; set; } = -1;
        public int TUTORIAL_TYPE { get; set; } = -1;
        public int REWARD { get; set; } = -1;
    }

    [Table("world_base")]
    public class DBWorld_base : DBData
    {
        public int NUM { get; set; } = -1;
        public int _NAME { get; set; } = -1;
        public string BACKGROUND { get; set; }
        public string IMAGE { get; set; }
        public int STAR_1 { get; set; } = -1;
        public int REWARD_STAR_1 { get; set; } = -1;
        public int STAR_2 { get; set; } = -1;
        public int REWARD_STAR_2 { get; set; } = -1;
        public int STAR_3 { get; set; } = -1;
        public int REWARD_STAR_3 { get; set; } = -1;
    }

    [Table("world_trip")]
    public class DBWorld_trip : DBData
    {
        public int WORLD { get; set; } = -1;
        public string _NAME { get; set; }
        public int CHAR_NUM { get; set; } = -1;
        public int TIME { get; set; } = -1;
        public int COST_STAMINA { get; set; } = -1;
        public int REWARD_ACCOUNT_EXP { get; set; } = -1;
        public int REWARD_CHAR_EXP { get; set; } = -1;
        public int REWARD_GOLD { get; set; } = -1;
        public int REWARD_BONUS { get; set; } = -1;
        public int REWARD_BONUS_NUM { get; set; } = -1;
    }

    [Table("world_trip_hell")]
    public class DBWorld_trip_hell : DBData
    {
        public int CONQUEST { get; set; } = -1;
        public int NEED_WORLD_CLEAR { get; set; } = -1;
        public int NEED_WORLD_DIFFICULT { get; set; } = -1;
        public string _NAME { get; set; } = "";
        public int NEED_BP { get; set; } = -1;
        public int TIME { get; set; } = -1;
        public int COST_STAMINA { get; set; } = -1;
        public int COST_FEE { get; set; } = -1;
        public int FEE_TAX { get; set; } = -1;
        public int PROTECT_TIME { get; set; } = -1;
        public int LOSE_TIME { get; set; } = -1;
        public int REWARD_ITEM { get; set; } = -1;
        public int REWARD_MAGNET_MIN { get; set; } = -1;
        public int REWARD_MAGNET_MAX { get; set; } = -1;
        public int REWARD_CHIPSET_MIN { get; set; } = -1;
        public int REWARD_CHIPSET_MAX { get; set; } = -1;
        public int REWARD_GOLDBLOCK_MIN { get; set; } = -1;
        public int REWARD_GOLDBLOCK_MAX { get; set; } = -1;
        public int REWARD_LEADBLOCK_MIN { get; set; } = -1;
        public int REWARD_LEADBLOCK_MAX { get; set; } = -1;
        public int REWARD_ELEMENTAL_MIN { get; set; } = -1;
        public int REWARD_ELEMENTAL_MAX { get; set; } = -1;
    }
    [Table("rate_table_url")]
    public class DBRate_table_url : DBData
    {
        public string KOR { get; set; } = string.Empty;
        public string ENG { get; set; } = string.Empty;
        public string JPN { get; set; } = string.Empty;
        public string PRT { get; set; } = string.Empty;
        public string CHS { get; set; } = string.Empty;
        public string CHT { get; set; } = string.Empty;
    }


    [Table("guild_exp")]
    public class DBGuild_exp : DBData
    {
        public int LEVEL { get; set; } = -1;
        public int EXP { get; set; } = -1;
        public int TOTAL_EXP { get; set; } = -1;
        public string REWARD_STAT_TYPE { get; set; } = string.Empty;
        public int VALUE_TYPE { get; set; } = -1;
        public float REWARD_STAT_VALUE { get; set; } = -1f;
    }

    [Table("guild_donation")]
    public class DBGuild_donation : DBData
    {
        public string NEED_TYPE { get; set; } = "";
        public int NEED_NUM { get; set; } = -1;
        public int REWARD_ACCOUNT_EXP { get; set; } = -1;
        public int REWARD_GUILD_EXP { get; set; } = -1;
        public int REWARD_GUILD_POINT { get; set; } = -1;
    }

    [Table("guild_rank_reward")]
    public class DBGuild_rank_reward : DBData
    {
        public int GROUP { get; set; } = -1;
        public int HIGHEST_RANK { get; set; } = -1;
        public uint LOWEST_RANK { get; set; } = 0;
        public int WEEKLY_REWARD_GROUP { get; set; } = -1;
        public int MONTHLY_REWARD_GROUP { get; set; } = -1;
        public int REWARD_GROUP_3 { get; set; } = -1;
    }
    [Table("guild_resource")]
    public class DBGuild_resource : DBData
    {
        public int TYPE { get; set; } = -1;
        public string FILE_NAME { get; set; } = "";
    }

    [Table("server_option")]
    public class DBServer_option : DBData
    {
        public string VALUE { get; set; } = "";
    }

    [Table("user_cache")]
    public class DBUser_cache : DBData
    {
        public string VALUE { get; set; } = "";
    }

}