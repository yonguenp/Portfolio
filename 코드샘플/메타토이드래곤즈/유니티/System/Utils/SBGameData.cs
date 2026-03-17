using Newtonsoft.Json.Linq;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;

namespace SandboxNetwork
{

    public class SBGameData
    {
        private static bool isLocalLoaded = false;

        public void ClearLocalFlag()
        {
            isLocalLoaded = false;
        }

        /// <summary> Data가 없는 경우에 로컬에 남아있는 기본 데이터라도 입력하도록 세팅 </summary>
        public static void GetCompactLocalDatas(bool force = false)
        {
            if (!force && isLocalLoaded)
                return;

            using var connection = DataBase.Get();

            /// 기본 로드 테이블 세팅
            var include = new List<TableMapping>();
            include.Add(connection.GetMapping<DBLanguage>());
            include.Add(connection.GetMapping<DBGame_config>());

            var prevLanguage = (SystemLanguage)PlayerPrefs.GetInt("Setting_Language", (int)Application.systemLanguage);
            switch (prevLanguage)
            {
                case SystemLanguage.Korean:
                    include.Add(connection.GetMapping<DBString_kor>());
                    break;
                case SystemLanguage.Japanese:
                    include.Add(connection.GetMapping<DBString_jpn>());
                    break;
                case SystemLanguage.ChineseSimplified:
                    include.Add(connection.GetMapping<DBString_chs>());
                    break;
                case SystemLanguage.ChineseTraditional:
                    include.Add(connection.GetMapping<DBString_cht>());
                    break;
                case SystemLanguage.Portuguese:
                    include.Add(connection.GetMapping<DBString_prt>());
                    break;
                default:
                    include.Add(connection.GetMapping<DBString_eng>());
                    break;
            }
            //

            foreach (var info in include)
            {
                var tableName = info.TableName;
                IEnumerable<DBData> DBtable = null;
                switch (info.MappedType)
                {
                    case Type t when t == typeof(DBLanguage):
                        DBtable = connection.SelectTable<DBLanguage>();
                        break;
                    case Type t when t == typeof(DBGame_config):
                        DBtable = connection.SelectTable<DBGame_config>();
                        break;
                    case Type t when t == typeof(DBString_kor):
                        DBtable = connection.SelectTable<DBString_kor>();
                        break;
                    case Type t when t == typeof(DBString_jpn):
                        DBtable = connection.SelectTable<DBString_jpn>();
                        break;
                    case Type t when t == typeof(DBString_chs):
                        DBtable = connection.SelectTable<DBString_chs>();
                        break;
                    case Type t when t == typeof(DBString_cht):
                        DBtable = connection.SelectTable<DBString_cht>();
                        break;
                    case Type t when t == typeof(DBString_prt):
                        DBtable = connection.SelectTable<DBString_prt>();
                        break;
                    case Type t when t == typeof(DBString_eng):
                        DBtable = connection.SelectTable<DBString_eng>();
                        break;
                }
                if(DBtable == null)
                    continue;

                var table = DBtable.ToList();
                if (table == null || table.Count < 1)
                {
                    TextAsset dAsset = Resources.Load<TextAsset>(SBFunc.StrBuilder("Data/", tableName));
                    if (dAsset == null)
                        continue;

                    using (StringReader sr = new(dAsset.text))
                    {
                        string[] splitType = sr.ReadLine().Split(",");
                        string[] splitColumn = sr.ReadLine().Split(",");
                        string[] splitValue = null;
                        string dataLine = null;
                        var typeLen = Mathf.Min(splitType.Length, splitColumn.Length);
                        List<int> dataIndex = new();

                        List<string> columns = new();
                        for (int i = 0; i < typeLen; ++i)
                        {
                            if (IsValuableData(splitType[i]))
                            {
                                dataIndex.Add(i);
                                columns.Add(splitColumn[i]);
                            }
                        }

                        List<string> values = new();
                        while (true)
                        {
                            dataLine = sr.ReadLine();

                            if (dataLine == null)
                            {
                                //EOF
                                break;
                            }

                            splitValue = dataLine.Split(",");
                            int len = Mathf.Min(splitValue.Length, typeLen);
                            //입력용 컬럼이 맞는지 필터함

                            for (int i = 0, count = dataIndex.Count; i < count; ++i)
                            {
                                if (len <= dataIndex[i] || dataIndex[i] < 0)
                                    continue;

                                values.Add(splitValue[dataIndex[i]]);
                            }
                        }
                        if (connection.Insert(tableName, columns, values) < 0)
                        {
                            Debug.LogError("DB Insert fail : " + tableName);
                        }
                    }
                }
            }

            TableManager.GetTable<GameConfigTable>().Preload();
            TableManager.GetTable<LanguageTable>().Preload();
            SettingEvent.RefreshString();

            connection.Destory();
            isLocalLoaded = true;
        }

        public static bool IsValuableData(string data)
        {
            return (data.CompareTo("int") == 0 || data.CompareTo("string") == 0 || data.CompareTo("float") == 0 || data.CompareTo("datetime") == 0);
        }
        public static bool IsStringType(string data)
        {
            return (data.CompareTo("string") == 0 || data.CompareTo("datetime") == 0);
        }
        #region 미사용
        //public enum ExtractOption
        //{
        //    KEY,
        //    HASH,
        //    DATA,
        //}

        //public class GameData
        //{
        //    public string name;
        //    public string hash;
        //    public JArray data;
        //    public GameData(string stream)
        //    {
        //        string[] split = stream.Split("|");

        //        name = split[0];
        //        hash = split[1];
        //        data = JArray.Parse(split[2]);
        //    }

        //    public GameData(string _name, JProperty property)
        //    {
        //        name = _name;
        //        hash = property.Name;
        //        data = JArray.Parse(property.Value.Value<string>());
        //    }

        //    public GameData(string _name, string _hash, JArray _data)
        //    {
        //        name = _name;
        //        hash = _hash;
        //        data = _data;
        //    }

        //    public override string ToString()
        //    {
        //        string str = string.Format("{0}|{1}|{2}", name, hash, data.ToString(Newtonsoft.Json.Formatting.None));

        //        return str;
        //    }

        //    public string Extract(ExtractOption eo)
        //    {
        //        string extract = "";

        //        switch (eo)
        //        {
        //            case ExtractOption.KEY:
        //                extract = name;
        //                break;
        //            case ExtractOption.HASH:
        //                extract = hash;
        //                break;
        //            case ExtractOption.DATA:
        //                extract = data.ToString();
        //                break;
        //        }

        //        return extract;
        //    }
        //}
        //private static readonly string PASSWORD = "1f4d0c2fc2f543efa895c04ef5ace050";
        //// 인증키 정의
        //private static readonly string KEY = PASSWORD.Substring(0, 128 / 8);
        //private static Thread? DataLoadThread = null;
        //private static int Count { get; set; } = 0;
        //private static int LoadCount { get; set; } = 0;
        //public static bool IsLoaded => Count > 0 && Count == LoadCount;
        //public static float ThreadProgress => Count > 0 ? (float)LoadCount / Count : 0f;
        //public static void ThreadParseGameData()
        //{
        //    if (DataLoadThread != null)
        //    {
        //        DataLoadThread.Abort();
        //        DataLoadThread = null;
        //    }
        //    //DataLoadThread = new Thread(new ThreadStart(FileDataLoad));
        //    //DataLoadThread.Start();
        //    //DataLoadThread.Join();
        //}
        //private static void FileDataLoad()
        //{
        //    //lock (SBGameManager.Instance)
        //    //{
        //    //    ParseGameData();
        //    //    DataLoadThread = null;
        //    //}
        //}
        #region Crypt
        // 암호화
        //public static string Encrypt(string plain)
        //{
        //    byte[] plainBytes = Encoding.UTF8.GetBytes(plain);

        //    RijndaelManaged myRijndael = new RijndaelManaged();
        //    myRijndael.Mode = CipherMode.CBC;
        //    myRijndael.Padding = PaddingMode.PKCS7;
        //    myRijndael.KeySize = 128;

        //    MemoryStream memoryStream = new MemoryStream();

        //    ICryptoTransform encryptor = myRijndael.CreateEncryptor(Encoding.UTF8.GetBytes(KEY), Encoding.UTF8.GetBytes(KEY));

        //    CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        //    cryptoStream.Write(plainBytes, 0, plainBytes.Length);
        //    cryptoStream.FlushFinalBlock();

        //    byte[] encryptBytes = memoryStream.ToArray();

        //    string encryptString = Convert.ToBase64String(encryptBytes);

        //    cryptoStream.Close();
        //    memoryStream.Close();

        //    return encryptString;
        //}
        //// 복호화
        //public static string Decrypt(string encrypt)
        //{
        //    byte[] encryptBytes = Convert.FromBase64String(encrypt);

        //    RijndaelManaged myRijndael = new RijndaelManaged();
        //    myRijndael.Mode = CipherMode.CBC;
        //    myRijndael.Padding = PaddingMode.PKCS7;
        //    myRijndael.KeySize = 128;

        //    MemoryStream memoryStream = new MemoryStream(encryptBytes);

        //    ICryptoTransform decryptor = myRijndael.CreateDecryptor(Encoding.UTF8.GetBytes(KEY), Encoding.UTF8.GetBytes(KEY));

        //    CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

        //    byte[] plainBytes = new byte[encryptBytes.Length];

        //    int plainCount = cryptoStream.Read(plainBytes, 0, plainBytes.Length);

        //    string plainString = Encoding.UTF8.GetString(plainBytes, 0, plainCount);

        //    cryptoStream.Close();
        //    memoryStream.Close();

        //    return plainString;
        //}
        #endregion
        #endregion
    }
}