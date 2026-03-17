using SQLite4Unity3d;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SandboxNetwork
{
    public static class UserDB
    {
        private static readonly string DBName = "{0}_user.db";
        private static readonly int DBVersion = 2;
#if UNITY_EDITOR
        private static readonly string PATH = Application.dataPath + "/";
#else
        private static readonly string PATH = Application.persistentDataPath + "/";
#endif
        private static long CurrentUID = -1;
        private static string GetDBName()
        {
            return Path.Combine(PATH, string.Format(DBName, CurrentUID));
        }
        private static SQLiteConnection Get(SQLiteOpenFlags flag = SQLiteOpenFlags.ReadWrite)
        {
            var dbName = GetDBName();
            if (File.Exists(dbName) || flag.HasFlag(SQLiteOpenFlags.Create))
            {
                return new SQLiteConnection(dbName, flag);
            }
            return null;
        }
        public static void VersionCheck(long user_no)
        {
            if (user_no <= 0)
                return;

            CurrentUID = user_no;

            var connection = Get();
            using (connection)
            {
                var check = connection.CheckDBVersion(DBVersion);
                if (check)
                    return;
            }

            CreateTable();
        }
        public static void CreateTable()
        {
            var dbFullName = GetDBName();
            if (File.Exists(dbFullName))
            {
                File.Delete(dbFullName);
            }
            using var connection = Get(SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

            connection.DropCreate<DBVersion>();
            connection.Insert(new DBVersion(DBVersion.ToString()));

            connection.DropCreate<ChatDataInfo>();
            connection.DropCreate<BlockUserData>();

            connection.Destory();
        }
        /// <summary> 로그아웃 시 호출 </summary>
        public static void Destroy()
        {
            CurrentUID = -1;
        }
        public static void Insert<T>(T data)
        {
            if (CurrentUID <= 0)
                return;

            using var connection = Get();

            connection.BeginTransaction();
            connection.Insert(data);
            connection.Commit();

            connection.Destory();
        }
        public static void Insert<T>(List<T> datas)
        {
            if (CurrentUID <= 0 || datas == null)
                return;

            using var connection = Get();

            connection.BeginTransaction();
            
            for (int i = 0, count = datas.Count; i < count; ++i)
            {
                if (datas[i] == null)
                    continue;

                connection.Insert(datas[i]);
            }

            connection.Commit();

            connection.Destory();
        }
        public static List<ChatDataInfo> GetGuildChat(long guildUID)
        {
            if (CurrentUID <= 0)
                return null;

            using var connection = Get();
            var items = connection.Query<ChatDataInfo>(string.Format("SELECT * FROM ChatDataInfo WHERE `SendUserGuildUID`={0} AND `ChatType` & {1} ORDER BY `KEY` DESC LIMIT 50;", guildUID, (int)eChatCommentType.Guild));
            if (items != null)
                items.Reverse();

            return items;
        }
        public static List<ChatDataInfo> GetWorldChat()
        {
            if (CurrentUID <= 0)
                return null;

            using var connection = Get();

            var items = connection.Query<ChatDataInfo>(string.Format("SELECT * FROM ChatDataInfo WHERE `ChatType` & {0} ORDER BY `KEY` DESC LIMIT 50;", (int)eChatCommentType.World));
            if (items != null)
                items.Reverse();

            return items;
        }
        public static List<ChatDataInfo> GetWisperChat(long recvUID)
        {
            if (CurrentUID <= 0)
                return null;

            using var connection = Get();

            var items = connection.Query<ChatDataInfo>(string.Format("SELECT * FROM ChatDataInfo WHERE `ChatType` & {0} AND (`SendUID`={1} OR `RecvUID`={2}) ORDER BY `KEY` DESC LIMIT 30;", (int)eChatCommentType.Whisper, recvUID, recvUID));
            if (items != null)
                items.Reverse();

            return items;
        }
        public static Dictionary<long, BlockUserData> GetBlockUser()
        {
            using var connection = Get();

            var res = new Dictionary<long, BlockUserData>();

            var list = connection.Query<BlockUserData>(string.Format("SELECT * FROM {0};", "BlockList"));
            if(list != null)
            {
                for (int i = 0, count = list.Count; i < count; ++i)
                {
                    if (list[i] == null)
                        continue;

                    res.TryAdd(list[i].UID, list[i]);
                }
            }

            return res;
        }
        public static void DeleteBlockList()
        {
            using var connection = Get();

            connection.DeleteTable("BlockList");
        }
    }
}