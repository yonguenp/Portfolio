using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SQLite4Unity3d;
using System.Linq.Expressions;
using System;

namespace SandboxNetwork
{
    /// <summary>
    /// JObject 형태 사용할지 말지 결정필요
    /// 현재는 사용중이지만 TableManager개편을 진행 할 경우에만 사용안할 수 있음.
    /// </summary>
    public static class DataBase
    {
        private static string DBName 
        { 
            get 
            {
                if (NetworkManager.IsLiveServer)
                    return "game.db";
                else
                {
#if SB_TEST
                    if (NetworkManager.IsQAServer)
                        return "game_qa.db";
#endif
                    return "game_dev.db";
                }
            } 
        }

        private static int DBVersion = 0;
#if UNITY_EDITOR
        private static readonly string PATH = Application.dataPath + "/";
#else
        private static readonly string PATH = Application.persistentDataPath + "/";
#endif
        private static SQLiteConnection Connection = null;

        #region Create
        public static void SetVersion(int version)
        {
            DBVersion = version;
        }
        private static void CreateTable(int version)
        {
            Destroy();
            var dbFullName = Path.Combine(PATH, DBName);
            if (File.Exists(dbFullName))
            {
                File.Delete(dbFullName);
            }

            using var connection = Get(SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

            connection.DropCreate<DBVersion>();
            connection.Insert(new DBVersion(version.ToString()));

            connection.DropCreate<DesignHash>();
            connection.DropCreate<DBDefine_resource>();

            connection.DropCreate<DBLanguage>();
            connection.DropCreate<DBGame_config>();

            #region String, Mail, Script
            connection.DropCreate<DBString_kor>();
            connection.DropCreate<DBString_eng>();
            connection.DropCreate<DBString_prt>();
            connection.DropCreate<DBString_jpn>();
            connection.DropCreate<DBString_chs>();
            connection.DropCreate<DBString_cht>();
            
            connection.DropCreate<DBMail_kor>();
            connection.DropCreate<DBMail_eng>();
            connection.DropCreate<DBMail_prt>();
            connection.DropCreate<DBMail_jpn>();
            connection.DropCreate<DBMail_chs>();
            connection.DropCreate<DBMail_cht>();

            connection.DropCreate<DBScript_kor>();
            connection.DropCreate<DBScript_eng>();
            connection.DropCreate<DBScript_prt>();
            connection.DropCreate<DBScript_jpn>();
            connection.DropCreate<DBScript_chs>();
            connection.DropCreate<DBScript_cht>();
            #endregion
            connection.DropCreate<DBAccount_exp>();

            connection.DropCreate<DBArea_expansion>();
            connection.DropCreate<DBArea_level>();

            connection.DropCreate<DBBuilding_product>();
            connection.DropCreate<DBBuilding_product_auto>();

            connection.DropCreate<DBBuilding_base>();
            connection.DropCreate<DBBuilding_level>();
            connection.DropCreate<DBBuilding_open>();

            connection.DropCreate<DBItem_base>();
            connection.DropCreate<DBItem_group>();
            connection.DropCreate<DBItem_group_list>();

            connection.DropCreate<DBInventory>();

            connection.DropCreate<DBSlot_cost>();

            connection.DropCreate<DBWorld_trip>();
            connection.DropCreate<DBWorld_trip_hell>();

            connection.DropCreate<DBSubway_platform>();
            connection.DropCreate<DBSubway_delivery>();

            // 드래곤
            connection.DropCreate<DBChar_base>();
            connection.DropCreate<DBChar_grade>();
            connection.DropCreate<DBChar_exp>();

            connection.DropCreate<DBStat_factor>();

            connection.DropCreate<DBElement_rate>();

            connection.DropCreate<DBMonster_base>();
            connection.DropCreate<DBMonster_spawn>();

            connection.DropCreate<DBWorld_base>();
            connection.DropCreate<DBStage_base>();

            connection.DropCreate<DBGacha_shop>();
            connection.DropCreate<DBGacha_list>();

            connection.DropCreate<DBPart_base>();

            connection.DropCreate<DBSub_option>();

            connection.DropCreate<DBPart_set>();
            connection.DropCreate<DBPart_reinforce>();

            connection.DropCreate<DBQuest_base>();
            connection.DropCreate<DBQuest_trigger_group>();

            connection.DropCreate<DBPart_merge_base>();
            connection.DropCreate<DBPart_merge_reinforcebonus>();
            connection.DropCreate<DBPart_merge_equipamountbonus>();
            connection.DropCreate<DBPart_decompose>();

            // 아레나
            connection.DropCreate<DBPvp_rank>();
            connection.DropCreate<DBPvp_rank_season_reward>();
            connection.DropCreate<DBPvp_season>();

            // 펫
            connection.DropCreate<DBPet_base>();
            connection.DropCreate<DBPet_grade>();
            connection.DropCreate<DBPet_skill_normal>();
            connection.DropCreate<DBPet_exp>();
            connection.DropCreate<DBPet_reinforce>();
            connection.DropCreate<DBPet_element>();
            connection.DropCreate<DBPet_merge_base>();

            // 드래곤합성
            connection.DropCreate<DBChar_merge_base>();
            connection.DropCreate<DBChar_merge_list>();

            connection.DropCreate<DBArea_level_mission>();

            connection.DropCreate<DBExchange_base>();
            connection.DropCreate<DBExchange_group>();

            connection.DropCreate<DBPet_stat>();

            connection.DropCreate<DBStat_type>();

            connection.DropCreate<DBSound_resource>();

            connection.DropCreate<DBPet_decompose>();

            connection.DropCreate<DBDaily_stage>();

            // 뽑기
            connection.DropCreate<DBGacha_group>();
            connection.DropCreate<DBGacha_menu>();
            connection.DropCreate<DBGacha_type>();
            connection.DropCreate<DBGacha_rate>();

            // 상점
            connection.DropCreate<DBShop_menu>();
            connection.DropCreate<DBShop_goods>();
            connection.DropCreate<DBSubscription_item>();
            connection.DropCreate<DBPersonal_goods>();
            connection.DropCreate<DBShop_banner>();
            connection.DropCreate<DBPost_reward>();
            connection.DropCreate<DBShop_sku>();

            connection.DropCreate<DBDaily_reward>();

            connection.DropCreate<DBAdv_limit>();

            // 업적 콜렉션
            connection.DropCreate<DBAchievements_info>();
            connection.DropCreate<DBCollection_info>();
            connection.DropCreate<DBCollection_group>();

            connection.DropCreate<DBShop_random>();

            connection.DropCreate<DBScript_trigger>();
            connection.DropCreate<DBScript_group>();
            connection.DropCreate<DBScript_object>();

            // 스킬
            connection.DropCreate<DBSkill_char>();
            connection.DropCreate<DBSkill_summon>();
            connection.DropCreate<DBSkill_effect>();
            connection.DropCreate<DBSkill_level>();
            connection.DropCreate<DBSkill_level_group>();
            connection.DropCreate<DBSkill_resource>();

            connection.DropCreate<DBEvent_banner>();

            connection.DropCreate<DBPass_item>();

            connection.DropCreate<DBMagic_showcase_info>();

            connection.DropCreate<DBRecipe_core>();
            connection.DropCreate<DBRecipe_material>();

            connection.DropCreate<DBReserv_mail>();

            connection.DropCreate<DBEvent_schedule>();
            connection.DropCreate<DBEvent_reward>();
            connection.DropCreate<DBEvent_rank_reward>();
            connection.DropCreate<DBEvent_attendance_resource>();

            connection.DropCreate<DBDice_board>();

            //초월
            connection.DropCreate<DBChar_transcendence>();

            //패시브스킬
            connection.DropCreate<DBSkill_passive>();
            connection.DropCreate<DBSkill_passive_group>();
            connection.DropCreate<DBSkill_passive_rate>();

            // 월드보스
            connection.DropCreate<DBRaid_boss_level>();
            connection.DropCreate<DBRaid_boss_rank_reward>();
            connection.DropCreate<DBRaid_boss_parts>();

            // 튜토리얼
            connection.DropCreate<DBTutorial_script>();
            connection.DropCreate<DBTutorial_trigger>();

            // 광산
            connection.DropCreate<DBMine_booster>();
            connection.DropCreate<DBMine_drill>();

            // 길드
            connection.DropCreate<DBGuild_donation>();
            connection.DropCreate<DBGuild_exp>();
            connection.DropCreate<DBGuild_rank_reward>();
            connection.DropCreate<DBGuild_resource>();

            connection.DropCreate<DBRate_table_url>();

            connection.Destory();
        }
        #endregion

        public static void SetDefaultDB()
        {
            var dbFullName = Path.Combine(PATH, DBName);
            if (File.Exists(dbFullName))
                return;

            using var connection = Get(SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

            connection.DropCreate<DBLanguage>();
            connection.DropCreate<DBGame_config>();
            
            connection.DropCreate<DBString_kor>();
            connection.DropCreate<DBString_eng>();
            connection.DropCreate<DBString_prt>();
            connection.DropCreate<DBString_jpn>();
            connection.DropCreate<DBString_chs>();
            connection.DropCreate<DBString_cht>();

            connection.Destory();
        }

        #region DropTable

        public static void DropTable()
        {
            //var dbFullName = Path.Combine(PATH, DBName);
            //if (File.Exists(dbFullName))
            //{
            //    File.Delete(dbFullName);
            //}

            using var connection = Get();

            connection.DropTable<DBVersion>();

            connection.Destory();
        }

        #endregion



        #region ConnectionExtension
        public static void DropCreate<T>(this SQLiteConnection connection)
        {
            connection.DropTable<T>();
            connection.CreateTable<T>();
        }
        public static bool CheckDBVersion(this SQLiteConnection connection, int checkVersion)
        {
            if (null == connection)
                return false;

            var command = connection.CreateCommand(SelectTableQuery("Version"));
            if (null == command)
                return false;

            try
            {
                var list = command.ExecuteQuery<DBVersion>();
                if (null == list)
                    return false;

                for (int i = 0, count = list.Count; i < count; ++i)
                {
                    if (null == list[i])
                        continue;

                    return checkVersion.ToString() == list[i].UNIQUE_KEY;
                }
            }
            catch (SQLiteException e)
            {
                Debug.Log(e.Message);
            }

            return false;
        }
        public static void Destory(this SQLiteConnection connection)
        {
            if (connection == null)
                return;

            connection.Dispose();
        }
        /// <summary> Format형태의 Select문 사용 </summary>
        /// <param name="formetString">ex) "SELECT {0}, {1} FROM TABLENAME WHERE COLUMN1='{2}';"</param>
        /// <param name="items">COLUMN1, COLUMN2, "1"</param>
        /// <returns></returns>
        public static int Insert(this SQLiteConnection connection, string tableName, List<string> columns, params string[] values)
        {
            if (columns == null || values == null)
                return -1;

            return connection.Insert(SBFunc.StrBuilder("INSERT OR REPLACE INTO ",
                    tableName, " (`", string.Join("`, `", columns), "`) ",
                    "VALUES ", string.Join(",", values), ";"));
        }
        public static int DeleteTable(this SQLiteConnection connection, string tableName)
        {
            return connection.CreateCommand(string.Format("DELETE FROM {0}", tableName)).ExecuteNonQuery();
        }
        /// <summary> 테이블 통짜 입력에만 사용할 것 </summary>
        public static int Insert(this SQLiteConnection connection, string tableName, List<string> columns, List<string> values)
        {
            if (columns.Count <= 0 || (values.Count % columns.Count) != 0)
                return -1;

            connection.BeginTransaction();
            var builder = SBFunc.GetBuilder();
            builder.Append("INSERT OR REPLACE INTO ");
            builder.Append(tableName);
            builder.Append(" (`");
            builder.Append(string.Join("`, `", columns));
            builder.Append("`) ");
            builder.Append("VALUES (");
            for (int i = 0, count = columns.Count; i < count; ++i)
            {
                if (i != 0)
                    builder.Append(",");

                builder.Append(string.Format("@PARAM{0}", i));
            }
            builder.Append(");");

            int colCount = columns.Count;
            int rs = 0;
            for (int i = 0, count = values.Count; i < count; i += colCount)
            {
                SQLiteCommand command = connection.CreateCommand(builder.ToString());

                for (int j = 0; j < colCount; ++j)
                {
                    command.Bind(string.Format("@PARAM{0}", j), values[i + j]);
                }

                rs += command.ExecuteNonQuery();
            }
            connection.Commit();
            return rs;
        }
        public static string SelectTableQuery(string tableName, string extension = "")
        {
            return SBFunc.StrBuilder("SELECT * FROM ", tableName, extension == string.Empty ? string.Empty : " ", extension);
        }
        #endregion
        #region ReaderExtension
        public static IEnumerable<T> SelectTable<T>(this SQLiteConnection connection) where T : DBData, new()
        {
            return connection.Table<T>();
        }
        #endregion
        public static SQLiteConnection Get(SQLiteOpenFlags flag = SQLiteOpenFlags.ReadWrite)
        {
            if (File.Exists(Path.Combine(PATH, DBName)) || flag.HasFlag(SQLiteOpenFlags.Create))
            {
                return new SQLiteConnection(Path.Combine(PATH, DBName), flag);
            }
            return null;
        }
        public static void VersionCheck()
        {
            if (DBVersion < 0)
                return;

            using (var connection = Get())
            {
                var check = connection.CheckDBVersion(DBVersion);
                if (check)
                    return;
            }

            CreateTable(DBVersion);
        }
        public static List<SQLiteConnection.ColumnInfo> GetInfo(string tableName)
        {
            using var connection = Get();
            var rs = connection.GetTableInfo(tableName);
            connection.Destory();
            return rs;
        }
        public static DesignHash GetHash(string key)
        {
            using var connection = Get();
            if (key != string.Empty)
            {
                var rs = connection.Get<DesignHash>(key);
                connection.Destory();
                return rs;
            }
            return null;
        }
        public static void InsertHash(this SQLiteConnection connection, string key, string value)
        {
            var hash = new DesignHash();
            hash.UNIQUE_KEY = key;
            hash.HASH = value;
            connection.InsertOrReplace(hash);
        }
        public static List<DesignHash> GetHash()
        {
            using var connection = Get();
            var commend = connection.CreateCommand(SelectTableQuery("DesignHash"));
            var rs = commend.ExecuteQuery<DesignHash>();
            connection.Destory();
            return rs;
        }
        public static IEnumerable<T> SelectAll<T>() where T : DBData, new()
        {
            if (Connection == null)
                Connection = Get();

            return Connection.SelectTable<T>();
        }
        public static T Select<T>(string key) where T : DBData, new()
        {
            if (Connection == null)
                Connection = Get();

            var ret = Connection.Get<T>(key);
            if (ret != null)
            {
                if (string.IsNullOrEmpty(ret.UNIQUE_KEY))
                    return null;

                return ret;
            }

            return null;
        }
        public static T Select<T>(Expression<Func<T, bool>> predicate) where T : DBData, new()
        {
            if (Connection == null)
                Connection = Get();

            return Connection.Get(predicate);
        }
        public static TableMapping GetMapping<T>(this SQLiteConnection connection)
        {
            return connection.GetMapping<T>();
        }
        /// <summary> 로그아웃 시 호출 </summary>
        public static void Destroy()
        {
            if (Connection != null)
                Connection.Destory();

            Connection = null;
        }
    }
}
